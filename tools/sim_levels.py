import math

cx, cy = 0.0, -0.62
r0, rmax = 0.20, 0.48


def dist(ang, rad, theta=0.0):
    a = math.radians(ang + theta)
    x, y = rad * math.cos(a), rad * math.sin(a)
    return math.hypot(x - cx, y - cy)


print("L2", dist(270, 0.58), dist(270, 0.38), dist(200, 0.78))

print("--- L3 r=0.20 ---")
for th in range(0, 360, 1):
    dg = dist(200, 0.58, th)
    dc = dist(245, 0.58, th)
    db = dist(40, 0.78, th)
    hitg = dg <= 0.20
    hitc = dc <= 0.20
    hitb = db <= rmax
    mark = ""
    if hitg and not hitc and db > 0.20:
        mark = "SAFE_TAP"
    elif hitg and hitc:
        mark = "FAIL"
    elif hitg:
        mark = "gimbap+?"
    if mark:
        print(f"th={th:3d} g={dg:.3f} c={dc:.3f} b={db:.3f} {mark}")

print("--- L4 ---")
for th in range(0, 80):
    ds = dist(250, 0.58, th)
    dt = dist(268, 0.58, th)
    dp = dist(292, 0.58, th)
    dg = dist(110, 0.78, th)
    need = max(ds, dt)
    danger = min(dp, dg)
    if need <= rmax and need < danger:
        lo = max(need, r0)
        hi = min(danger, rmax)
        if lo < hi:
            print(
                f"th={th:3d} need={need:.3f} danger={danger:.3f} "
                f"window=[{lo:.3f},{hi:.3f}) song={ds:.3f} tteok={dt:.3f} pho={dp:.3f}"
            )
