"""Sinh 20 màn Một Mâm theo luật hiện có và kiểm lời giải bằng mm_sim.

Chạy:  python design_levels.py gen      -> sinh bố cục, ghi levels_design.json
       python design_levels.py write    -> ghi asset, LevelCheck.cs, tài liệu từ levels_design.json
"""
import json
import math
import os
import random
import re
import sys
import time
from multiprocessing import Pool

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import mm_sim as ms  # noqa: E402

ROOT = os.path.dirname(HERE)
OUT_JSON = os.path.join(HERE, 'levels_design.json')

VN = ['banh_mi', 'goi_cuon', 'pho', 'ca_phe']
KR = ['gimbap', 'songpyeon', 'tteokbokki', 'bibimbap']

# ---- vai trò: (want, forbid, wantAfterSwap, forbidAfterSwap) ----
ROLE = {
    'want': (['@'], [], [], []),
    'forbid': ([], ['@'], [], []),
    'wG': (['G'], ['H'], [], []),
    'wH': (['H'], ['G'], [], []),
    'wB': (['G', 'H'], [], [], []),
    'fX': ([], ['G', 'H'], [], []),
    'dA': (['D'], [], [], []),          # thèm trước khi đổi, sau đổi trung lập
    'dB': ([], ['D'], ['D'], []),       # cấm trước, thèm sau đổi
    'dC': ([], [], [], ['D']),          # trung lập trước, cấm sau đổi
}
WANT_ROLES = {'want', 'wG', 'wH', 'wB', 'dA', 'dB'}
ROLE_ORDER = ['want', 'wG', 'wH', 'wB', 'dA', 'dB', 'forbid', 'fX', 'dC']

GUEST_MODE = {'G': 'Gumiho', 'H': 'Ho', 'M': 'Me', 'C': 'Co', 'D': 'Dokkaebi', 'GH': 'GumihoHo'}
GUEST_ENUM = {'G': 0, 'H': 1, 'M': 2, 'C': 3, 'D': 4}
MODE_ENUM = {'G': 0, 'H': 1, 'M': 2, 'GH': 3, 'C': 4, 'D': 5}
GUEST_NAME = {'G': 'Gumiho', 'H': 'Hổ', 'M': 'Mê', 'C': 'Cò', 'D': 'Dokkaebi'}
DISH_NAME = {'banh_mi': 'Bánh mì', 'goi_cuon': 'Gỏi cuốn', 'pho': 'Phở', 'ca_phe': 'Cà phê sữa đá',
             'gimbap': 'Gimbap', 'songpyeon': 'Songpyeon', 'tteokbokki': 'Tteokbokki', 'bibimbap': 'Bibimbap'}

# tier chống sai nhịp: (ew giây, eh giây)
TIERS = [(0.20, 0.060), (0.18, 0.050), (0.15, 0.045), (0.13, 0.040), (0.12, 0.035)]

# ---- bảng thiết kế 20 màn ----
# roles: số món mỗi vai. bites: (min, max) số nhát ngắn nhất cho phép.
LEVELS = [
    dict(id=1, guest='G', spin=0, n=10, roles={'want': 3, 'forbid': 1}, bites=(1, 1), tier=0, intro=True),
    dict(id=2, guest='G', spin=0, n=10, roles={'want': 3, 'forbid': 2}, bites=(1, 1), tier=0, slack=1),
    dict(id=3, guest='G', spin=10, n=10, roles={'want': 2, 'forbid': 2}, bites=(2, 2), tier=0, slack=1),
    dict(id=4, guest='G', spin=16, n=10, roles={'want': 2, 'forbid': 3}, bites=(2, 2), tier=0),
    dict(id=5, guest='H', spin=0, n=11, roles={'want': 3, 'forbid': 1}, bites=(1, 2), tier=1, intro=True),
    dict(id=6, guest='H', spin=14, n=11, roles={'want': 3, 'forbid': 2}, bites=(2, 2), tier=1),
    dict(id=7, guest='H', spin=20, n=11, roles={'want': 3, 'forbid': 3}, bites=(3, 3), tier=1),
    dict(id=8, guest='M', spin=0, n=11, roles={'want': 3, 'forbid': 1}, bites=(2, 2), tier=1, intro=True),
    dict(id=9, guest='M', spin=16, n=11, roles={'want': 3, 'forbid': 2}, bites=(3, 3), tier=2),
    dict(id=10, guest='M', spin=22, n=11, roles={'want': 4, 'forbid': 2}, bites=(4, 4), tier=2),
    dict(id=11, guest='G', spin=38, n=12, roles={'want': 4, 'forbid': 2}, bites=(4, 4), tier=3),
    dict(id=12, guest='H', spin=40, n=12, roles={'want': 4, 'forbid': 2}, bites=(4, 4), tier=4, slack=1),
    dict(id=13, guest='C', spin=0, n=12, roles={'want': 3, 'forbid': 2}, bites=(2, 2), tier=3, intro=True),
    dict(id=14, guest='C', spin=24, n=12, roles={'want': 4, 'forbid': 2}, bites=(4, 4), tier=4, anchor_off=-12),
    dict(id=15, guest='D', spin=0, n=12, roles={'dA': 3, 'dB': 3}, bites=(3, 4), tier=4, intro=True, swap=1, tol=(0.07, 0.03)),
    dict(id=16, guest='D', spin=20, n=12, roles={'dA': 2, 'dB': 3, 'dC': 1}, bites=(4, 4), tier=4, swap=1, slack=1, tol=(0.08, 0.03)),
    dict(id=17, guest='D', spin=26, n=12, roles={'dA': 2, 'dB': 2, 'dC': 2}, bites=(4, 4), tier=4, swap=2, tol=(0.08, 0.03)),
    dict(id=18, guest='GH', spin=0, n=13, roles={'wG': 2, 'wH': 2, 'fX': 2}, bites=(2, 2), tier=4, intro=True),
    dict(id=19, guest='GH', spin=22, n=13, roles={'wG': 2, 'wH': 2, 'fX': 2}, bites=(4, 4), tier=4),
    dict(id=20, guest='GH', spin=32, n=13, roles={'wG': 2, 'wH': 2, 'wB': 1, 'fX': 1}, bites=(4, 5), tier=4),
]


def make_spec(d):
    ew, eh = d.get('tol', TIERS[d['tier']])
    hearts = 3 if d.get('intro') else 2
    tmax = 11.0 if d['spin'] == 0 else 30.0
    wmax = 0.0 if d['spin'] == 0 else min(8.0, 360.0 / d['spin'])
    return ms.Spec(id=d['id'], guest=d['guest'], spin=float(d['spin']), hearts=hearts, slack=d.get('slack', 1 if d.get('intro') else 0),
                   swap=d.get('swap', 0), anchor_off=float(d.get('anchor_off', 0)), ew=ew, eh=eh, tmax=tmax, wmax=wmax)


# ---------------------------------------------------------------- bố cục
def wrap(a):
    return a % 360.0


def in_excl(a):
    a = wrap(a)
    return 250.0 <= a < 360.0


def want_guest(role, guest):
    return role in WANT_ROLES


def rule_groups(d, spec):
    """Nhóm món thèm theo (khách, giai đoạn): dưới 3 món -> không món nào bắt đầu ở 250..360."""
    groups = []
    roles = d['roles']
    if spec.guest == 'GH':
        g_n = roles.get('wG', 0) + roles.get('wB', 0)
        h_n = roles.get('wH', 0) + roles.get('wB', 0)
        groups.append(({'wG', 'wB'}, g_n))
        groups.append(({'wH', 'wB'}, h_n))
    elif spec.guest == 'D':
        groups.append(({'dA'}, roles.get('dA', 0)))
        groups.append(({'dB'}, roles.get('dB', 0)))
    else:
        groups.append(({'want'}, roles.get('want', 0)))
    return groups


def excluded_roles(d, spec):
    ex = set()
    for rs, n in rule_groups(d, spec):
        if n < 3:
            ex |= rs
    return ex


def static_sample(spec, role, rng, excl):
    """Vị trí món thèm trên mâm đứng yên: nằm trong tầm vùng ăn của khách."""
    for _ in range(300):
        g = spec.guest
        who = g
        if g == 'GH':
            who = 'H' if role in ('wH',) else ('G' if role == 'wG' else rng.choice(['G', 'H']))
        if who == 'G':
            an = math.radians(spec.anchor('G') if g == 'GH' else 270)
            c = (0.62 * math.cos(an), 0.62 * math.sin(an))
            a = wrap(math.degrees(an) + rng.uniform(-85, 85))
            r = rng.uniform(0.34, 0.84)
            p = (r * math.cos(math.radians(a)), r * math.sin(math.radians(a)))
            d = math.hypot(p[0] - c[0], p[1] - c[1])
            if not (0.10 <= d <= 0.44):
                continue
        elif who == 'H':
            base = spec.anchor('H') if g == 'GH' else 270
            a = wrap(base + rng.uniform(-26, 26))
            r = rng.uniform(0.34, 0.84)
            if abs(r * math.sin(math.radians(a - base))) > 0.25:
                continue
            if g == 'GH' and r * math.cos(math.radians(a - base)) < 0.2:
                continue
        elif who == 'M':
            base = rng.choice([270.0, 90.0])
            a = wrap(base + rng.uniform(-8, 8))
            r = rng.uniform(0.34, 0.84)
            if abs(r * math.sin(math.radians(a - base))) > 0.11:
                continue
        elif who == 'C':
            base = spec.single_anchor()
            a = wrap(base + rng.uniform(-7, 7))
            r = rng.uniform(0.34, 0.82)
            if abs(r * math.sin(math.radians(a - base))) > 0.10:
                continue
        else:  # D
            base = spec.single_anchor()
            a = wrap(base + rng.uniform(-40, 40))
            r = rng.uniform(0.30, 0.84)
        if role in excl and in_excl(a):
            continue
        return a, r
    return None


def free_sample(spec, role, rng, excl):
    for _ in range(100):
        a = rng.uniform(0, 360)
        r = rng.uniform(0.34, 0.84)
        if role in excl and in_excl(a):
            continue
        return a, r
    return None


def pdist(p, q):
    a = math.radians(p[0])
    b = math.radians(q[0])
    return math.hypot(p[1] * math.cos(a) - q[1] * math.cos(b), p[1] * math.sin(a) - q[1] * math.sin(b))


def far_enough(p, items, dmin=0.14):
    return all(pdist(p, q) >= dmin for q in items)


def gen_layout(d, spec, rng):
    """Trả về list món (đã sắp thứ tự: vai trò trước, trang trí sau) hoặc None."""
    excl = excluded_roles(d, spec)
    static = spec.spin == 0
    items = []   # (role, ang, rad)
    positions = []
    for role in ROLE_ORDER:
        cnt = d['roles'].get(role, 0)
        if role in WANT_ROLES:
            for _ in range(cnt):
                for _try in range(60):
                    s = static_sample(spec, role, rng, excl) if static else free_sample(spec, role, rng, excl)
                    if s is None:
                        return None
                    if not far_enough(s, positions, 0.16):
                        continue
                    # mâm xoay: món thèm đầu cách vạch miệng khách ít nhất 30 độ
                    if not static:
                        anchors = {'wG': [180], 'wH': [0], 'wB': [180, 0]}.get(role, [spec.single_anchor()])
                        if any(((anc - s[0]) % 360.0) < 30.0 for anc in anchors):
                            continue
                    break
                else:
                    return None
                items.append((role, s[0], s[1]))
                positions.append(s)
        elif cnt:
            wants = [p for p in items if p[0] in WANT_ROLES]
            for _ in range(cnt):
                for _try in range(120):
                    w = rng.choice(wants)
                    a = wrap(w[1] + rng.choice([-1, 1]) * rng.uniform(9, 42))
                    r = min(0.86, max(0.28, w[2] + rng.uniform(-0.26, 0.26)))
                    if static and spec.guest == 'D':
                        pass
                    s = (a, r)
                    if pdist(s, (w[1], w[2])) > 0.34 or not far_enough(s, positions, 0.13):
                        continue
                    break
                else:
                    return None
                items.append((role, s[0], s[1]))
                positions.append(s)
    # trang trí: lấp khoảng trống góc lớn nhất
    while len(items) < d['n']:
        angs = sorted(p[0] for p in positions)
        gaps = [((angs[(i + 1) % len(angs)] - angs[i]) % 360.0 or 360.0, angs[i]) for i in range(len(angs))]
        gaps.sort(reverse=True)
        placed = False
        for g, start in gaps[:3]:
            for _try in range(40):
                a = wrap(start + g * rng.uniform(0.3, 0.7))
                r = rng.uniform(0.30, 0.84)
                if far_enough((a, r), positions, 0.15):
                    items.append(('deco', a, r))
                    positions.append((a, r))
                    placed = True
                    break
            if placed:
                break
        if not placed:
            return None
    return items


def scatter_ok(items, n):
    angs = sorted(wrap(i[1]) for i in items)
    gmax = {10: 62, 11: 60, 12: 56, 13: 54}.get(n, 60)
    for i in range(len(angs)):
        g = (angs[(i + 1) % len(angs)] - angs[i]) % 360.0
        if g > gmax:
            return False
    for q in range(4):
        if sum(1 for a in angs if q * 90 <= a < (q + 1) * 90) < 2:
            return False
    rads = [i[2] for i in items]
    if sum(1 for r in rads if r < 0.46) < 2 or sum(1 for r in rads if r > 0.70) < 2:
        return False
    wants = [i for i in items if i[0] in WANT_ROLES]
    if len(wants) >= 3 and len({int(wrap(i[1]) // 90) for i in wants}) < 2:
        return False
    return True


def assign_dishes(items, rng):
    classes = []
    for r in ROLE_ORDER:
        if any(i[0] == r for i in items):
            classes.append(r)
    pool = VN + KR
    rng.shuffle(pool)
    # xen kẽ Việt-Hàn cho đẹp
    vn = [x for x in pool if x in VN]
    kr = [x for x in pool if x in KR]
    mixed = []
    while vn or kr:
        if kr:
            mixed.append(kr.pop())
        if vn:
            mixed.append(vn.pop())
    pool = mixed
    alloc = {}
    budget = 6
    for r in classes:
        cnt = sum(1 for i in items if i[0] == r)
        take = min(cnt, 2, budget - (len(classes) - len(alloc) - 1))
        take = max(1, take)
        alloc[r] = [pool.pop(0) for _ in range(take)]
        budget -= take
    deco = pool[:]
    counters = {r: 0 for r in alloc}
    deco_i = 0
    out = []
    for role, a, r in items:
        if role == 'deco':
            dish = deco[deco_i % len(deco)]
            deco_i += 1
        else:
            lst = alloc[role]
            dish = lst[counters[role] % len(lst)]
            counters[role] += 1
        out.append((role, dish, a, r))
    return out


def build_foods(spec, items):
    foods = []
    prim = spec.guest if spec.guest != 'GH' else None
    for role, dish, a, r in items:
        if role == 'deco':
            foods.append(ms.Food(a, r, dish=dish, role='deco'))
            continue
        w, f, ws, fs = ROLE[role]
        sub = lambda L: [prim if x == '@' else x for x in L]
        foods.append(ms.Food(a, r, sub(w), sub(f), sub(ws), sub(fs), dish, role))
    return foods


def order_items(items):
    key = {r: i for i, r in enumerate(ROLE_ORDER + ['deco'])}
    return sorted(items, key=lambda x: key[x[0]])


def trap_score(foods):
    """Số món cấm nằm sát một món thèm (khoảng cách chuẩn hóa <= 0.32)."""
    wants = [f for f in foods if f.role in WANT_ROLES]
    n = 0
    for f in foods:
        if f.role in ('forbid', 'fX', 'dC') or f.role == 'dB':
            if any(pdist((f.ang, f.rad), (w.ang, w.rad)) <= 0.32 for w in wants if w is not f):
                n += 1
    return n


def evaluate(d, spec, foods):
    lo, hi = d['bites']
    sol = ms.solve(spec, foods, hi)
    if sol is None or sol.bites < lo:
        return None
    return sol


def worker(args):
    d, seed, budget_s, want_ok = args
    spec = make_spec(d)
    rng = random.Random(seed)
    t0 = time.time()
    best = None
    tries = 0
    ok_count = 0
    while time.time() - t0 < budget_s and ok_count < want_ok:
        tries += 1
        items = gen_layout(d, spec, rng)
        if items is None or not scatter_ok(items, d['n']):
            continue
        items = order_items(items)
        items = assign_dishes(items, rng)
        foods = build_foods(spec, [(r, dish, a, rad) for r, dish, a, rad in items])
        # đảo (role, ang, rad) cho scatter đã kiểm
        sol = evaluate(d, spec, foods)
        if sol is None:
            continue
        ok_count += 1
        win = min(min(h['win_w'], 1.2) / spec.ew * 0.5 + min(h['win_h'], 0.3) / spec.eh * 0.5 for h in sol.hist)
        score = win + 0.4 * trap_score(foods) - 0.02 * sol.t
        if best is None or score > best[0]:
            best = (score, foods, sol)
    if best is None:
        return d['id'], None, tries
    score, foods, sol = best
    return d['id'], dict(score=score, foods=[dict(role=f.role, dish=f.dish, ang=round(f.ang, 1), rad=round(f.rad, 2)) for f in foods],
                         bites=sol.bites, time=round(sol.t, 2), hist=sol.hist), tries


def cmd_gen(only=None, budget=150, seeds=4):
    jobs = []
    for d in LEVELS:
        if only and d['id'] not in only:
            continue
        for s in range(seeds):
            jobs.append((d, d['id'] * 1000 + s + int(time.time()) % 997, budget, 3))
    res = {}
    with Pool(processes=4) as pool:
        for lid, out, tries in pool.imap_unordered(worker, jobs):
            print('level', lid, 'tries', tries, 'ok' if out else 'FAIL', flush=True)
            if out and (lid not in res or out['score'] > res[lid]['score']):
                res[lid] = out
    old = {}
    if os.path.exists(OUT_JSON):
        old = json.load(open(OUT_JSON, encoding='utf8'))
    for k, v in res.items():
        old[str(k)] = v
    json.dump(old, open(OUT_JSON, 'w', encoding='utf8'), ensure_ascii=False, indent=1, default=float)
    print('missing:', [d['id'] for d in LEVELS if str(d['id']) not in old])


if __name__ == '__main__':
    if len(sys.argv) > 1 and sys.argv[1] == 'gen':
        only = [int(x) for x in sys.argv[2].split(',')] if len(sys.argv) > 2 else None
        budget = float(sys.argv[3]) if len(sys.argv) > 3 else 150
        cmd_gen(only, budget)
