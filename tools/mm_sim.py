"""Mô phỏng luật Một Mâm, khớp GameRules.cs / LevelRunner.cs.

Dùng bởi design_levels.py để sinh màn và kiểm lời giải.
Mỗi nhát = (chờ w giây, giữ h giây). Người chơi được giả định sai nhịp trong
khoảng (ew, eh): nhát chỉ tính là "vững" khi mọi nhịp lệch trong khoảng đó đều
ra cùng một tập món thèm và không dính món cấm.
"""
import math
import numpy as np

HOLD_MAX = 0.50
BRAKE = 0.35
LOCK = 0.35
SLIDE = 0.40
BLINK = 0.40

G_CD, G_R0, G_RMAX, G_RMAX_D = 0.62, 0.20, 0.48, 0.47
H_W0, H_WMAX, H_MIN_D = 0.08, 0.28, 0.15
M_L0, M_LMAX, M_HW, M_CROSS = 0.35, 0.95, 0.13, 0.50
M_SWAY, M_HZ = 5.0, 0.6
C_R, C_A0, C_AM = 0.13, 0.82, 0.32
D_R, D_HZ, D_SWEEP, D_TAP, D_SWEEP_S = 0.14, 0.40, 42.0, 0.15, 0.80

DW = 0.05   # lưới chờ (giây)
DH = 0.02   # lưới giữ (giây)
H_LO = 0.04
H_HI = 1.00

DUAL_ANCHOR = {'G': 180.0, 'H': 0.0}


class Food:
    __slots__ = ('dish', 'ang', 'rad', 'want', 'forbid', 'wantS', 'forbidS', 'role')

    def __init__(self, ang, rad, want=(), forbid=(), wantS=(), forbidS=(), dish='', role=''):
        self.dish = dish
        self.ang = float(ang)
        self.rad = float(rad)
        self.want = frozenset(want)
        self.forbid = frozenset(forbid)
        self.wantS = frozenset(wantS)
        self.forbidS = frozenset(forbidS)
        self.role = role

    def now(self, swapped):
        return (self.wantS, self.forbidS) if swapped else (self.want, self.forbid)


class Spec:
    """Cấu hình một màn (phần LevelConfig + ngưỡng thiết kế)."""

    def __init__(self, **kw):
        self.id = 0
        self.guest = 'G'          # G H M C D GH
        self.spin = 0.0
        self.hearts = 2
        self.slack = 0
        self.swap = 0
        self.anchor_off = 0.0
        self.brake = BRAKE
        self.cross = M_CROSS
        self.ew = 0.15
        self.eh = 0.05
        self.tmax = 30.0
        self.wmax = 8.0
        self.__dict__.update(kw)

    @property
    def dual(self):
        return self.guest == 'GH'

    def single_anchor(self):
        return 270.0 + self.anchor_off

    def anchor(self, who):
        if self.dual:
            return DUAL_ANCHOR[who]
        return self.single_anchor()


def hb(h, brake):
    """Góc mâm trôi thêm (đơn vị: giây quy đổi) khi giữ h giây."""
    h = np.asarray(h, dtype=float)
    return np.where(h <= brake, h - h * h / (2 * brake), brake / 2)


def u_of(h):
    return np.clip(np.asarray(h, dtype=float) / HOLD_MAX, 0, 1)


def sway(t):
    return M_SWAY * np.sin(t * M_HZ * 2 * np.pi)


def dok_along(t):
    return 0.5 + 0.5 * np.sin(t * D_HZ * 2 * np.pi)


def dok_sweep(h):
    return D_SWEEP * np.clip((np.asarray(h, dtype=float) - D_TAP) / D_SWEEP_S, 0, 1)


class State:
    __slots__ = ('theta', 'meClock', 'dokClock', 'meK', 'bites', 'swapped', 'eaten', 't', 'hist')

    def __init__(self):
        self.theta = 0.0
        self.meClock = 0.0
        self.dokClock = 0.0
        self.meK = 0
        self.bites = 0
        self.swapped = False
        self.eaten = frozenset()
        self.t = 0.0
        self.hist = []


def grids(spec):
    if spec.spin > 0:
        wmax = spec.wmax
    elif spec.guest == 'M':
        wmax = 1.7
    elif spec.guest == 'D':
        wmax = 2.5
    else:
        wmax = 0.0
    W = np.arange(0.0, wmax + 1e-9, DW)
    H = np.arange(H_LO, H_HI + 1e-9, DH)
    return W, H


def hit_grid(spec, foods, st, who, W, H, dirn=1):
    """bool (nw, nh, nf): món nằm trong vùng ăn của khách `who`."""
    nw, nh = len(W), len(H)
    ang = np.array([f.ang for f in foods])
    rad = np.array([f.rad for f in foods])
    theta = st.theta + spec.spin * (W[:, None] + hb(H, spec.brake)[None, :])
    a = np.radians(ang[None, None, :] + theta[:, :, None])
    x = rad[None, None, :] * np.cos(a)
    y = rad[None, None, :] * np.sin(a)
    u = u_of(H)
    if who == 'G':
        an = math.radians(spec.anchor('G'))
        cx, cy = G_CD * math.cos(an), G_CD * math.sin(an)
        rmax = G_RMAX_D if spec.dual else G_RMAX
        r = (G_R0 + (rmax - G_R0) * u)[None, :, None]
        return np.hypot(x - cx, y - cy) <= r
    if who == 'H':
        an = math.radians(spec.anchor('H'))
        ax, ay = math.cos(an), math.sin(an)
        along = x * ax + y * ay
        perp = np.abs(x * ay - y * ax)
        w = (H_W0 + (H_WMAX - H_W0) * u)[None, :, None]
        ok = along >= H_MIN_D if spec.dual else along > 0
        return ok & (perp <= w)
    if who == 'M':
        an = np.radians(270.0 + 180.0 * st.meK + sway(st.meClock + W))[:, None, None]
        ax, ay = np.cos(an), np.sin(an)
        along = x * ax + y * ay
        perp = np.abs(x * ay - y * ax)
        ln = (M_L0 + (M_LMAX - M_L0) * u)[None, :, None]
        return (along > 0) & (perp <= M_HW) & ((1 - along) <= ln)
    if who == 'C':
        an = math.radians(spec.single_anchor())
        d = (C_A0 + (C_AM - C_A0) * u)[None, :, None]
        return np.hypot(x - d * math.cos(an), y - d * math.sin(an)) <= C_R
    if who == 'D':
        al = dok_along(st.dokClock + W)[:, None, None]
        an = np.radians(spec.single_anchor() + dirn * dok_sweep(H))[None, :, None]
        return np.hypot(x - al * np.cos(an), y - al * np.sin(an)) <= D_R
    raise ValueError(who)


def robust_mask(S, ok, rw, rh):
    nw, nh = S.shape
    P = np.pad(S, ((rw, rw), (rh, rh)), mode='edge')
    r = ok.copy()
    for dw in range(-rw, rw + 1):
        for dh in range(-rh, rh + 1):
            r &= (P[rw + dw: rw + dw + nw, rh + dh: rh + dh + nh] == S)
    return r


def guests_of(spec):
    return ['G', 'H'] if spec.dual else [spec.guest]


def wants_left(spec, foods, st):
    for i, f in enumerate(foods):
        if i in st.eaten:
            continue
        w, _ = f.now(st.swapped)
        for g in guests_of(spec):
            if g in w:
                return True
    return False


def expand(spec, foods, st, W, H, cap_per_key=2):
    """Mọi nhát vững tiếp theo, gom theo tập món ăn. Trả về danh sách (state mới, thông tin nhát)."""
    out = []
    nf = len(foods)
    rw = int(math.ceil(spec.ew / DW - 1e-9)) if len(W) > 1 else 0
    rh = int(math.ceil(spec.eh / DH - 1e-9))
    for who in guests_of(spec):
        dirs = (-1, 1) if who == 'D' else (0,)
        for dirn in dirs:
            wl = np.zeros(nf, dtype=np.int64)
            fb = np.zeros(nf, dtype=bool)
            alive = np.zeros(nf, dtype=bool)
            for i, f in enumerate(foods):
                if i in st.eaten:
                    continue
                alive[i] = True
                w, fo = f.now(st.swapped)
                if who in w:
                    wl[i] = 1 << i
                if who in fo:
                    fb[i] = True
            if not wl.any():
                continue
            Hh = hit_grid(spec, foods, st, who, W, H, dirn if dirn else 1) & alive[None, None, :]
            wm = (Hh * wl[None, None, :]).sum(-1)
            foul = (Hh & fb[None, None, :]).any(-1)
            if who == 'M':
                slide = (M_L0 + (M_LMAX - M_L0) * u_of(H)) > spec.cross
                slide = np.broadcast_to(slide[None, :], wm.shape)
            else:
                slide = np.zeros(wm.shape, dtype=bool)
            S = wm + (foul.astype(np.int64) << 40) + (slide.astype(np.int64) << 41)
            ok = (wm > 0) & ~foul
            if not ok.any():
                continue
            R = robust_mask(S, ok, rw, rh)
            if not R.any():
                continue
            iw, ih = np.nonzero(R)
            keys = S[iw, ih]
            tcost = W[iw] + H[ih]
            for key in np.unique(keys):
                sel = np.nonzero(keys == key)[0]
                order = sel[np.argsort(tcost[sel])]
                picks = [order[0]]
                if cap_per_key > 1 and len(order) > 1:
                    # nhịp giữa của đoạn vững theo tiêu chí: xa mép nhất (nhiều láng giềng cùng key nhất)
                    best, bestv = None, -1
                    for k in order[:: max(1, len(order) // 40)]:
                        a, b = iw[k], ih[k]
                        v = _margin(S, a, b)
                        if v > bestv:
                            best, bestv = k, v
                    if best is not None and best != picks[0]:
                        picks.append(best)
                for k in picks:
                    a, b = int(iw[k]), int(ih[k])
                    out.append(_apply(spec, foods, st, who, dirn, W, H, S, a, b, int(key), R))
    return out


def _margin(S, a, b):
    key = S[a, b]
    m = 0
    nw, nh = S.shape
    for da, db in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        i, j, n = a, b, 0
        while 0 <= i + da < nw and 0 <= j + db < nh and S[i + da, j + db] == key and n < 30:
            i += da
            j += db
            n += 1
        m += n
    return m


def _span(S, a, b, axis):
    key = S[a, b]
    n = S.shape[axis]
    i = a if axis == 0 else b
    lo = i
    while lo - 1 >= 0 and (S[lo - 1, b] if axis == 0 else S[a, lo - 1]) == key:
        lo -= 1
    hi = i
    while hi + 1 < n and (S[hi + 1, b] if axis == 0 else S[a, hi + 1]) == key:
        hi += 1
    return lo, hi


def _apply(spec, foods, st, who, dirn, W, H, S, a, b, key, R):
    # tinh chỉnh về giữa đoạn, chỉ nhận nếu ô mới vẫn vững
    key_ = S[a, b]
    if len(W) > 1:
        lo, hi = _span(S, a, b, 0)
        a2 = (lo + hi) // 2
        if S[a2, b] == key_ and R[a2, b]:
            a = a2
    lo2, hi2 = _span(S, a, b, 1)
    b2 = (lo2 + hi2) // 2
    if S[a, b2] == key_ and R[a, b2]:
        b = b2
    lo, hi = _span(S, a, b, 0)
    lo2, hi2 = _span(S, a, b, 1)
    w, h = float(W[a]), float(H[b])
    win_w = (hi - lo + 1) * DW if len(W) > 1 else 99.0
    win_h = (hi2 - lo2 + 1) * DH
    Hh = hit_grid(spec, foods, st, who, W[a:a + 1], H[b:b + 1], dirn if dirn else 1)[0, 0]
    hit = [i for i in range(len(foods)) if Hh[i] and i not in st.eaten]
    eat = []
    for i in hit:
        wnt, _ = foods[i].now(st.swapped)
        if who in wnt:
            eat.append(i)
    n = State()
    n.theta = st.theta + spec.spin * (w + float(hb(h, spec.brake)))
    n.meClock = st.meClock + w
    n.dokClock = st.dokClock + w
    n.meK = st.meK
    slide = who == 'M' and (M_L0 + (M_LMAX - M_L0) * float(u_of(h))) > spec.cross
    if slide:
        n.meK += 1
    n.bites = st.bites + 1
    n.eaten = st.eaten | frozenset(eat)
    n.swapped = st.swapped
    lock = LOCK
    if slide:
        lock = max(lock, SLIDE)
    if spec.swap > 0 and not n.swapped and n.bites >= spec.swap:
        n.swapped = True
        lock = max(lock, BLINK)
    n.t = st.t + w + h + lock
    info = dict(who=who, dir=dirn, wait=w, hold=h, win_w=win_w, win_h=win_h, hit=hit, eat=eat,
                tray=st.theta + spec.spin * w + spec.spin * float(hb(h, spec.brake)),
                sway=float(sway(st.meClock + w)) if who == 'M' else 0.0,
                along=float(dok_along(st.dokClock + w)) if who == 'D' else 0.5,
                meK=st.meK, slide=slide, swapAfter=n.swapped and not st.swapped)
    n.hist = st.hist + [info]
    return n


def key_of(s):
    return (s.eaten, s.meK, s.swapped)


def solve(spec, foods, max_bites, beam=14, tmax=None):
    """BFS theo số nhát. Trả về lời giải ngắn nhất (state cuối) hoặc None."""
    tmax = tmax or spec.tmax
    W, H = grids(spec)
    layer = [State()]
    for _ in range(max_bites):
        nxt = {}
        for st in layer:
            for n in expand(spec, foods, st, W, H):
                if n.t > tmax:
                    continue
                k = key_of(n)
                cur = nxt.setdefault(k, [])
                cur.append(n)
        if not nxt:
            return None
        done = []
        pool = []
        for k, lst in nxt.items():
            lst.sort(key=lambda s: s.t)
            for s in lst[:2]:
                left = wants_left(spec, foods, s)
                if not left:
                    # Màn có Dokkaebi chỉ tính khi đã đổi món ở nhát trước đó.
                    if spec.swap == 0 or (s.swapped and not s.hist[-1]['swapAfter']):
                        done.append(s)
                else:
                    pool.append(s)
        if done:
            done.sort(key=lambda s: s.t)
            return done[0]
        pool.sort(key=lambda s: s.t)
        layer = pool[:beam]
        if not layer:
            return None
    return None
