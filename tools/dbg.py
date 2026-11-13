import sys, random, time
sys.path.insert(0,'.')
import design_levels as dl, mm_sim as ms
lid=int(sys.argv[1]); ew=float(sys.argv[2]); eh=float(sys.argv[3]); hi=int(sys.argv[4])
d=[x for x in dl.LEVELS if x['id']==lid][0]
spec=dl.make_spec(d); spec.ew=ew; spec.eh=eh
rng=random.Random(5); n=0; cnt={}
t0=time.time()
while time.time()-t0<60:
    items=dl.gen_layout(d,spec,rng)
    if items is None or not dl.scatter_ok(items,d['n']): continue
    n+=1
    items=dl.assign_dishes(dl.order_items(items),rng)
    foods=dl.build_foods(spec,items)
    sol=ms.solve(spec,foods,hi)
    k=None if sol is None else (sol.bites,round(sol.t))
    cnt[k]=cnt.get(k,0)+1
print(n,sorted(cnt.items(),key=lambda x:str(x[0])))
