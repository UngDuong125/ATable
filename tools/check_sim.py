import re, sys, numpy as np
sys.path.insert(0, '.')
import mm_sim as ms
GN = {0:'G',1:'H',2:'M',3:'C',4:'D'}
def hexset(s):
    s=s.strip()
    return [GN[int.from_bytes(bytes.fromhex(s[i:i+8]),'little')] for i in range(0,len(s),8)]
def load(path):
    t=open(path,encoding='utf8').read()
    g=lambda k: re.search(r'^  '+k+r': (.*)$',t,re.M).group(1)
    mode=int(g('guestMode')); spin=float(g('spinSpeed'))
    foods=[]
    for m in re.finditer(r'  - dishId: (\S+)\n    angleDeg: (\S+)\n    radius: (\S+)\n    want: (.*)\n    forbid: (.*)\n(?:    wantAfterSwap: (.*)\n    forbidAfterSwap: (.*)\n)?',t):
        foods.append(ms.Food(float(m[2]),float(m[3]),hexset(m[4]),hexset(m[5]),hexset(m[6] or ''),hexset(m[7] or ''),m[1]))
    guest={0:'G',1:'H',2:'M',3:'GH',4:'C',5:'D'}[mode]
    return ms.Spec(id=int(g('id')),guest=guest,spin=spin,swap=int(g('swapAfterBite')),anchor_off=float(g('anchorOffset'))),foods
if __name__=='__main__':
    # (level, guest, anchorK, tray, hold, sway, along, dir, eaten, expect)  -- vài ca từ LevelCheck cũ
    cases=[(1,'G',0,0.0,0.32,0,0.5,0,[],[0,1,2]),(5,'H',0,0,0.4,0,0.5,0,[],[0,1,2]),(8,'M',0,0,0.14,0,.5,0,[],[0]),(8,'M',1,0,0.08,0,.5,0,[0],[1,2]),
           (13,'C',0,0,0.2,0,.5,0,[],[0]),(15,'D',0,0,0.0,1.841,0.624,1,[],[2]),(20,'G',0,34*0+27.2*0+12.75,0.5,3.423,.741,0,[],[0,2,4])]
    for lv,gu,k,tray,hold,sw,al,dr,eaten,exp in cases:
        spec,foods=load(f'../GameSrc/Assets/Levels/Level{lv:02d}.asset')
        st=ms.State(); st.theta=tray; st.meK=k
        # đặt đồng hồ để khớp sway / along: bỏ qua, so trực tiếp bằng cách ép giá trị
        import numpy as np
        who=gu
        W=np.array([0.0]); H=np.array([max(hold,1e-6)])
        if who=='M':
            st.meClock=np.arcsin(sw/5)/(2*np.pi*0.6) if sw else 0
        hg=ms.hit_grid(spec,foods,st,who,W,H,dr or 1)[0,0]
        got=[i for i in range(len(foods)) if hg[i] and i not in eaten]
        print(lv,gu,got,exp,'OK' if got==exp else 'DIFF')
