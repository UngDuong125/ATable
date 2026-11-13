"""Ghi 20 màn từ levels_design.json vào asset Unity, LevelCheck.cs và tài liệu.

Chạy: python write_levels.py
"""
import json
import math
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import design_levels as dl  # noqa: E402
import mm_sim as ms  # noqa: E402

ROOT = dl.ROOT
LEVEL_DIR = os.path.join(ROOT, 'GameSrc', 'Assets', 'Levels')
CHECK_CS = os.path.join(ROOT, 'GameSrc', 'Assets', 'Scripts', 'Editor', 'LevelCheck.cs')
DOCS = os.path.join(ROOT, 'docs')

GUID = 'b4988bd2d3f9e35419515d028e9a36bf'
GUEST_ENUM_NAME = {'G': 'Gumiho', 'H': 'Ho', 'M': 'Me', 'C': 'Co', 'D': 'Dokkaebi'}

HINTS = {
    1: 'Chạm nhanh: vòng nhỏ ăn món sáng gần miệng. Giữ lâu: vòng to dần. Đừng chạm món bị cấm.',
    5: 'Hổ cắn theo khe. Chạm: khe hẹp. Giữ: khe rộng dần, đừng chạm món bị cấm.',
    8: 'Chạm: dải ngắn. Giữ lâu hơn một chút: dải dài ra và Mê lướt sang mép đối diện.',
    13: 'Cò chọc từ mép vào. Giữ để chấm đi sâu hơn. Chạm sớm thì chấm còn ở sát mép.',
    15: 'Đốm lửa chạy từ tâm ra mép. Chạm nhanh: đốm đứng yên. Giữ lâu: đốm quét ngang. Sau nhát đầu, icon đổi.',
    18: 'Chạm nửa trái cho Gumiho, nửa phải cho Hổ. Mỗi khách chỉ ăn món sáng của mình.',
}

INTENT = {
    1: 'Màn dạy Gumiho. Mâm đứng yên, vòng lớn dần khi giữ. Món cấm nằm sát món thèm để người chơi thấy phải thả đúng lúc.',
    2: 'Mâm đứng yên, thêm một món cấm. Người chơi phải đọc vòng lớn dần trước khi thả.',
    3: 'Mâm xoay rất chậm. Có chưa tới ba món thèm nên không món nào bắt đầu ở vùng 250–360°: phải chờ mâm đưa tới.',
    4: 'Mâm xoay nhanh hơn, ba món cấm. Hai món thèm không ôm được trong một nhát, phải cắn hai lần.',
    5: 'Màn dạy Hổ. Mâm đứng yên, khe hẹp khi chạm và rộng dần khi giữ.',
    6: 'Mâm xoay chậm. Món cấm đứng gần món thèm nên khe phải vừa đủ.',
    7: 'Mâm xoay vừa, ba món cấm. Khe phải thu hẹp hoặc chờ mâm đưa món thèm vào đúng đường tâm.',
    8: 'Màn dạy Mê. Mâm đứng yên, dải lắc nhẹ. Giữ quá nửa thanh thì Mê lướt sang mép đối diện.',
    9: 'Mâm xoay chậm, ba nhát. Mỗi nhát dài đổi mép đứng của Mê.',
    10: 'Bốn món thèm, hai món cấm. Phải tính trước nhát nào lướt, nhát nào đứng yên.',
    11: 'Gumiho quay lại, mâm xoay nhanh nhất từ đầu game. Bốn món thèm rải bốn phía, mỗi nhát một món.',
    12: 'Hổ quay lại, mâm xoay nhanh. Bốn món thèm rải khắp mâm, khe phải đúng nhịp từng lần.',
    13: 'Màn dạy Cò. Mâm đứng yên. Chấm là độ sâu, không phải bề rộng.',
    14: 'Cò ngồi lệch trái 12°, mâm xoay chậm. Bốn món thèm đi qua mỏ lần lượt.',
    15: 'Màn dạy Dokkaebi. Mâm đứng yên, đốm lửa chạy tâm–mép. Icon đổi sau nhát đầu.',
    16: 'Mâm xoay chậm. Icon đổi sau nhát đầu: món cấm thành món thèm, món trung lập thành món cấm.',
    17: 'Mâm xoay nhanh hơn. Icon đổi sau nhát thứ hai, nên phải ăn đủ hai nhát trước khi đổi.',
    18: 'Màn dạy hai khách. Mâm đứng yên. Mỗi khách hai món thèm riêng, món của người này là món cấm của người kia.',
    19: 'Mâm xoay chậm, bốn nhát luân phiên giữa hai khách.',
    20: 'Mâm xoay nhanh nhất. Có món cả hai cùng thèm, và món chỉ một khách được ăn khi nó nằm đúng vùng.',
}


def fmt(x):
    x = round(float(x), 2)
    return str(int(x)) if x == int(x) else ('%g' % x)


def hexs(lst):
    return ''.join(int(GUEST_ENUM_NAME and dl.GUEST_ENUM[g]).to_bytes(4, 'little').hex() for g in lst)


def level_spec(d, data):
    spec = dl.make_spec(d)
    foods = [ms.Food(f['ang'], f['rad'], dish=f['dish'], role=f['role']) for f in data['foods']]
    prim = spec.guest if spec.guest != 'GH' else None
    for f, fd in zip(foods, data['foods']):
        if fd['role'] == 'deco':
            continue
        w, fo, ws, fs = dl.ROLE[fd['role']]
        sub = lambda L: [prim if x == '@' else x for x in L]
        f.want, f.forbid, f.wantS, f.forbidS = frozenset(sub(w)), frozenset(sub(fo)), frozenset(sub(ws)), frozenset(sub(fs))
    return spec, foods


def asset_text(d, spec, foods, data):
    bites = data['bites']
    charges = bites + spec.slack
    hint = HINTS.get(d['id'], '')
    hint_txt = json.dumps(hint, ensure_ascii=True) if hint else ''
    lines = [
        '%YAML 1.1', '%TAG !u! tag:unity3d.com,2011:', '--- !u!114 &11400000', 'MonoBehaviour:',
        '  m_ObjectHideFlags: 0', '  m_CorrespondingSourceObject: {fileID: 0}', '  m_PrefabInstance: {fileID: 0}',
        '  m_PrefabAsset: {fileID: 0}', '  m_GameObject: {fileID: 0}', '  m_Enabled: 1', '  m_EditorHideFlags: 0',
        '  m_Script: {fileID: 11500000, guid: %s, type: 3}' % GUID, '  m_Name: Level%02d' % d['id'],
        '  m_EditorClassIdentifier: Assembly-CSharp::MotMam.LevelConfig',
        '  id: %d' % d['id'], '  guestMode: %d' % dl.MODE_ENUM[spec.guest], '  spinSpeed: %s' % fmt(spec.spin),
        '  charges: %d' % charges, '  hearts: %d' % spec.hearts, '  brakeTime: 0', '  crossAt: 0',
        '  swapAfterBite: %d' % spec.swap, '  anchorOffset: %s' % fmt(spec.anchor_off),
        '  hint: ' + hint_txt, '  foods:'
    ]
    for f in foods:
        lines += ['  - dishId: ' + f.dish, '    angleDeg: ' + fmt(f.ang), '    radius: ' + fmt(f.rad),
                  '    want: ' + hexs(sorted(f.want, key=lambda g: dl.GUEST_ENUM[g])),
                  '    forbid: ' + hexs(sorted(f.forbid, key=lambda g: dl.GUEST_ENUM[g])),
                  '    wantAfterSwap: ' + hexs(sorted(f.wantS, key=lambda g: dl.GUEST_ENUM[g])),
                  '    forbidAfterSwap: ' + hexs(sorted(f.forbidS, key=lambda g: dl.GUEST_ENUM[g]))]
    return '\n'.join(lines) + '\n'


def difficulty(d, spec, bites):
    gt = {'G': 0, 'H': 1, 'M': 2, 'C': 3, 'D': 4, 'GH': 5}[d['guest']]
    nforbid = sum(c for r, c in d['roles'].items() if r not in ('want', 'wG', 'wH', 'wB', 'dA'))
    if d['guest'] == 'GH':
        nforbid += d['roles'].get('wG', 0) + d['roles'].get('wH', 0)
    return 2 * gt + spec.spin / 4 + 3 * (bites - 1) + 1.5 * nforbid + 2 * (spec.hearts == 2) + 3 * (spec.slack == 0) + 3 * d['tier']


def role_text(f, spec):
    g = lambda s: ', '.join(dl.GUEST_NAME[x] for x in sorted(s, key=lambda x: dl.GUEST_ENUM[x]))
    if f.role == 'deco':
        return 'trang trí'
    if spec.guest == 'D':
        pre = ('**want** ' + g(f.want)) if f.want else ('forbid ' + g(f.forbid) if f.forbid else 'trang trí')
        post = ('**want** ' + g(f.wantS)) if f.wantS else ('forbid ' + g(f.forbidS) if f.forbidS else 'trang trí')
        return '%s → sau đổi: %s' % (pre, post)
    parts = []
    if f.want:
        parts.append('**want** ' + g(f.want))
    if f.forbid:
        parts.append('forbid ' + g(f.forbid))
    return ', '.join(parts) if parts else 'trang trí'


def solution_lines(hist, foods, spec):
    out = []
    for i, h in enumerate(hist, 1):
        wait = ('không chờ' if h['wait'] < 0.01 else 'chờ %.2f giây' % h['wait'])
        tap = h['hold'] <= 0.08
        hold = 'chạm' if tap else 'giữ %.2f giây' % h['hold']
        extra = ''
        if h['who'] == 'M' and h['slide']:
            extra = ' (Mê lướt sang mép đối diện)'
        if h['who'] == 'D' and h['hold'] > ms.D_TAP:
            extra = ' (nhấn nửa %s màn hình)' % ('trái' if h['dir'] < 0 else 'phải')
        who = ''
        if spec.guest == 'GH':
            who = 'bên %s: ' % ('trái, Gumiho' if h['who'] == 'G' else 'phải, Hổ')
        names = ', '.join('`%s`' % foods[j].dish for j in h['eat'])
        out.append('%d. %s%s, %s%s → ăn %s.' % (i, who, wait, hold, extra, names))
    return out


def check_cases(levels):
    rows = []
    for d, spec, foods, data in levels:
        eaten = []
        for h in data['hist']:
            who = h['who']
            if who == 'G':
                anchor = spec.anchor('G')
            elif who == 'H':
                anchor = spec.anchor('H')
            elif who == 'M':
                anchor = 270.0 + 180.0 * h['meK']
            else:
                anchor = spec.single_anchor()
            rows.append('            Bite(%d, GuestId.%s, %sf, %sf, %sf, %sf, %sf, %d, new int[] { %s }, new int[] { %s }),' % (
                d['id'], GUEST_ENUM_NAME[who], ('%.1f' % anchor), ('%.3f' % h['tray']), ('%.2f' % h['hold']),
                ('%.3f' % h['sway']), ('%.3f' % h['along']), h['dir'], ', '.join(map(str, eaten)),
                ', '.join(map(str, h['hit']))))
            eaten = eaten + h['eat']
    return '\n'.join(rows)


def write_check_cs(levels):
    src = open(CHECK_CS, encoding='utf8').read()
    a = src.index('static readonly BiteCase[] Cases =')
    a = src.index('{', a) + 1
    b = src.index('        };', a)
    src = src[:a] + '\n' + check_cases(levels) + '\n' + src[b:]
    # kiểm luật bố cục
    if 'CheckRules(' not in src:
        src = src.replace('                failed += CheckLayout(log, asset);\n',
                          '                failed += CheckLayout(log, asset);\n                failed += CheckRules(log, asset);\n')
        rules = '''
        // Luật xếp món: dưới 3 món thèm (mỗi khách, mỗi giai đoạn) thì không món nào bắt đầu ở 250..360°;
        // món rải khắp mâm (khoảng trống góc lớn nhất không quá 62°); mỗi khách có ít nhất một món thèm.
        static int CheckRules(StringBuilder log, LevelConfig level)
        {
            int failed = 0;
            string name = "L" + level.id;
            var foods = level.foods;
            if (foods == null) return failed;
            for (int phase = 0; phase < 2; phase++)
            {
                foreach (GuestId g in System.Enum.GetValues(typeof(GuestId)))
                {
                    int count = 0;
                    for (int i = 0; i < foods.Length; i++)
                    {
                        if (System.Array.IndexOf(phase == 0 ? foods[i].want : foods[i].wantAfterSwap ?? new GuestId[0], g) >= 0) count++;
                    }
                    if (count == 0 || count >= 3) continue;
                    for (int i = 0; i < foods.Length; i++)
                    {
                        var list = phase == 0 ? foods[i].want : foods[i].wantAfterSwap;
                        if (list == null || System.Array.IndexOf(list, g) < 0) continue;
                        float a = ((foods[i].angleDeg % 360f) + 360f) % 360f;
                        if (a >= 250f) failed += Fail(log, name + " dưới 3 món thèm nhưng món " + i + " bắt đầu ở " + a.ToString("0") + "°");
                    }
                }
            }
            var angles = new List<float>();
            for (int i = 0; i < foods.Length; i++) angles.Add(((foods[i].angleDeg % 360f) + 360f) % 360f);
            angles.Sort();
            for (int i = 0; i < angles.Count; i++)
            {
                float next = angles[(i + 1) % angles.Count];
                float gap = ((next - angles[i]) % 360f + 360f) % 360f;
                if (angles.Count > 1 && gap > 62f) failed += Fail(log, name + " khoảng trống " + gap.ToString("0") + "° giữa hai món liền kề");
            }
            return failed;
        }
'''
        src = src.replace('        static int[] Hits(LevelConfig level, BiteCase c)', rules + '\n        static int[] Hits(LevelConfig level, BiteCase c)')
    open(CHECK_CS, 'w', encoding='utf8', newline='\n').write(src)


def md_levels(levels):
    ws = ['## 3. Bảng nhanh 20 màn', '',
          'Số món là tổng món trên mâm (thèm + cấm + trang trí). Món trang trí không ai thèm hay cấm, nằm sẵn trong asset. '
          'Thời gian là thời gian ngắn nhất để qua màn theo lời giải. «Độ khó» là điểm tổng hợp ở mục 3.1.', '',
          '| Màn | Khách | Xoay (°/s) | Số món | Thèm | Cấm | Nhát | Lượt | Mạng | Thời gian | Độ khó |',
          '|---|---|---|---|---|---|---|---|---|---|---|']
    for d, spec, foods, data in levels:
        wants = sum(1 for f in foods if f.want or f.wantS) if spec.guest != 'GH' else sum(1 for f in foods if f.want)
        forb = sum(1 for f in foods if (f.forbid or f.forbidS) and not (f.want and spec.guest != 'GH'))
        guest = {'GH': 'Gumiho trái + Hổ phải'}.get(spec.guest, dl.GUEST_NAME.get(spec.guest))
        ws.append('| %d | %s | %s | %d | %d | %d | %d | %d | %d | %.1f s | %.1f |' % (
            d['id'], guest, fmt(spec.spin), len(foods), wants, forb, data['bites'], data['bites'] + spec.slack,
            spec.hearts, data['time'], difficulty(d, spec, data['bites'])))
    ws += ['', '### 3.1 Độ khó tăng dần', '',
           'Điểm độ khó = `2 × bậc khách` (Gumiho 0, Hổ 1, Mê 2, Cò 3, Dokkaebi 4, hai khách 5) + `xoay / 4` + `3 × (nhát − 1)` + '
           '`1,5 × số món cấm` + `2` nếu 2 mạng + `3` nếu không còn lượt dự phòng + `3 × bậc cửa sổ nhịp`.', '',
           'Cửa sổ nhịp là phạm vi sai nhịp mà lời giải còn đúng: ±chờ / ±giữ (giây). Bậc 0: ±0,20 / ±0,06. Bậc 1: ±0,18 / ±0,05. '
           'Bậc 2: ±0,15 / ±0,045. Bậc 3: ±0,13 / ±0,04. Bậc 4: ±0,12 / ±0,035; riêng Dokkaebi xoay (màn 16, 17) ±0,08 / ±0,03 '
           'và màn 15 ±0,07 / ±0,03, vì đốm chạy tâm–mép rất nhanh.', '',
           'Qua các màn thường (không kể màn giới thiệu) điểm luôn tăng. Màn giới thiệu khách mới (1, 5, 8, 13, 15, 18) theo luật có sẵn '
           'là mâm đứng yên và dư một lượt, nên điểm thấp hơn màn thường ngay trước đó. Điểm giữa các màn giới thiệu cũng tăng dần.', '']
    ws.append('| Màn | Điểm | Loại |')
    ws.append('|---|---|---|')
    for d, spec, foods, data in levels:
        ws.append('| %d | %.1f | %s |' % (d['id'], difficulty(d, spec, data['bites']), 'giới thiệu khách' if d.get('intro') else 'thường'))
    ws += ['', '---', '', '## 4. Chi tiết từng màn', '',
           'Góc ghi là góc **trên mâm lúc spawn**. Với màn đứng yên, góc màn hình = góc spawn. Với màn xoay, góc màn hình = góc spawn + góc mâm.', '',
           'Mọi màn do `tools/design_levels.py` sinh và `tools/mm_sim.py` kiểm: lời giải tồn tại, không nhát nào dính món cấm, '
           'và mỗi nhát còn đúng khi người chơi lệch nhịp trong phạm vi của bậc cửa sổ.', '',
           'Luật xếp món áp cho cả 20 màn: dưới 3 món thèm thì không món thèm nào bắt đầu ở góc 250°–360°; '
           'món rải khắp mâm (khoảng trống góc lớn nhất không quá 62°, mỗi góc phần tư có ít nhất 2 món, cả vành trong lẫn vành ngoài đều có món); '
           'màn xoay đặt món thèm đầu cách vạch miệng khách 30° trở lên.', '', '---', '']
    for d, spec, foods, data in levels:
        guest = {'GH': 'Gumiho trái + Hổ phải'}.get(spec.guest, dl.GUEST_NAME.get(spec.guest))
        ws += ['### Màn %d — %s' % (d['id'], guest), '', '**Ý đồ:** ' + INTENT[d['id']], '']
        extra = ''
        if spec.swap:
            extra = ' · đổi icon sau nhát %d' % spec.swap
        if spec.anchor_off:
            extra += ' · `anchorOffset = %s`' % fmt(spec.anchor_off)
        ws += ['Xoay `%s`°/s · %d lượt · %d mạng%s' % (fmt(spec.spin), data['bites'] + spec.slack, spec.hearts, extra), '',
               '| dishId | angleDeg | radius | vai trò |', '|---|---|---|---|']
        for f in foods:
            ws.append('| `%s` | %s | %s | %s |' % (f.dish, fmt(f.ang), fmt(f.rad), role_text(f, spec)))
        ws += ['', '**Lời lời giải ngắn nhất:**'.replace('Lời lời', 'Lời'), '']
        ws += solution_lines(data['hist'], foods, spec)
        mw = min(h['win_w'] for h in data['hist'])
        mh = min(h['win_h'] for h in data['hist'])
        ws += ['', 'Cửa sổ nhịp hẹp nhất: chờ %s, giữ %.2f giây.' % ('không đổi (mâm không đổi theo giờ)' if mw >= 90 else '%.2f giây' % mw, mh)]
        if HINTS.get(d['id']):
            ws += ['', '**Gợi ý hiện trong màn:** ' + HINTS[d['id']]]
        ws += ['', '---', '']
    return '\n'.join(ws)


def patch_vitri(levels):
    p = os.path.join(DOCS, 'Mo-Mam-Vi-Tri-Mon-An.md')
    t = open(p, encoding='utf8').read()
    a = t.index('## 3. Bảng nhanh 20 màn')
    b = t.index('## 5. Ghi chú')
    t = t[:a] + md_levels(levels) + '\n' + t[b:]
    # mục kiểm nhanh cũ
    t = re.sub(r'- Màn 9, nhát 2:.*?\n', '', t)
    t = t.replace('1. Xếp cụm góc trước, gán want/forbid sau.  ',
                  '1. Xếp cụm góc trước, gán want/forbid sau. Dưới 3 món thèm thì không món thèm nào bắt đầu ở 250°–360°. Món rải khắp mâm.  ')
    t = t.replace('Số liệu hiện hành', 'Số liệu hiện hành')
    open(p, 'w', encoding='utf8', newline='\n').write(t)


def patch_thietke(levels):
    p = os.path.join(DOCS, 'Mo-Mam-Thiet-Ke-Man.md')
    t = open(p, encoding='utf8').read()
    a = t.index('## 3. Hai mươi màn')
    ws = ['## 3. Hai mươi màn', '', '| Màn | Khách | Mâm | Ý đồ | Nhát | Lượt |', '|---|---|---|---|---|---|']
    for d, spec, foods, data in levels:
        guest = {'GH': 'Gumiho trái, Hổ phải'}.get(spec.guest, dl.GUEST_NAME.get(spec.guest))
        mam = 'Đứng yên' if spec.spin == 0 else 'Xoay %s°/s' % fmt(spec.spin)
        ws.append('| %d | %s | %s | %s | %d | %d |' % (d['id'], guest, mam, INTENT[d['id']], data['bites'], data['bites'] + spec.slack))
    ws += ['',
           'Tọa độ từng màn, lời giải và cửa sổ nhịp nằm ở `Mo-Mam-Vi-Tri-Mon-An.md` mục 3–4. Điểm độ khó nằm ở mục 3.1 của file đó.', '',
           'Mỗi khách có một màn giới thiệu, mâm đứng yên và dư một lượt: Gumiho ở màn 1, Hổ ở màn 5, Mê ở màn 8, Cò ở màn 13, '
           'Dokkaebi ở màn 15, hai khách ở màn 18. Mạng là 3 ở màn giới thiệu, 2 ở các màn còn lại. Các màn 2, 3, 12 và 16 cũng chừa một lượt.', '',
           'Luật xếp món chung cho cả 20 màn:', '',
           '- Dưới 3 món thèm thì không món thèm nào bắt đầu ở góc 250°–360°. Món thèm đầu phải chờ mâm đưa tới.',
           '- Món nằm rải khắp mâm, không để một góc trống quá 62°.',
           '- Màn xoay đặt món thèm đầu cách vạch miệng khách ít nhất 30°.',
           '- Đổi món chỉ có ở màn Dokkaebi. Màn 15 và 16 đổi sau nhát 1, màn 17 đổi sau nhát 2.',
           '- Dải lắc có ở mọi màn Mê.',
           '- Màn 14 dùng `anchorOffset = -12`: Cò ngồi ở 258°.', '']
    t = t[:a] + '\n'.join(ws)
    t = t.replace('Số lượt trong bảng dưới đã tính sẵn một cách giải. Màn 1–3 có thêm một lượt dự phòng. Màn 4 trở đi đúng số nhát của lời giải, để mỗi nhát đều đáng giá. Màn 12 và màn 16 vẫn chừa một lượt, vì nhát hụt ở màn đứng yên còn cứu được.',
                  'Số lượt trong bảng dưới đã tính sẵn một cách giải. Các màn giới thiệu khách và màn 2, 3, 12, 16 có thêm một lượt dự phòng. Các màn còn lại đúng số nhát của lời giải, để mỗi nhát đều đáng giá.')
    open(p, 'w', encoding='utf8', newline='\n').write(t)


def patch_json(levels):
    p = os.path.join(DOCS, 'Mo-Mam-Vi-Tri-Mon-An.json')
    j = json.load(open(p, encoding='utf8'))
    out = []
    for d, spec, foods, data in levels:
        e = {'id': d['id'], 'guestMode': dl.GUEST_MODE[spec.guest], 'spinSpeed': spec.spin if spec.spin != int(spec.spin) else int(spec.spin),
             'charges': data['bites'] + spec.slack, 'hearts': spec.hearts}
        if spec.swap:
            e['swapAfterBite'] = spec.swap
        if spec.anchor_off:
            e['anchorOffset'] = int(spec.anchor_off)
        if HINTS.get(d['id']):
            e['hint'] = HINTS[d['id']]
        names = lambda s: [dl.GUEST_MODE[x] for x in sorted(s, key=lambda x: dl.GUEST_ENUM[x])]
        fl = []
        for f in foods:
            row = {'dishId': f.dish, 'angleDeg': float(fmt(f.ang)), 'radius': float(fmt(f.rad)), 'want': names(f.want), 'forbid': names(f.forbid)}
            if spec.swap:
                row['wantAfterSwap'] = names(f.wantS)
                row['forbidAfterSwap'] = names(f.forbidS)
            fl.append(row)
        e['foods'] = fl
        e['solution'] = [{'who': dl.GUEST_MODE[h['who']], 'waitSeconds': round(h['wait'], 2), 'holdSeconds': round(h['hold'], 2),
                          'eats': [foods[i].dish for i in h['eat']]} for h in data['hist']]
        out.append(e)
    j['levels'] = out
    open(p, 'w', encoding='utf8', newline='\n').write(json.dumps(j, ensure_ascii=False, indent=2) + '\n')


def main():
    res = json.load(open(dl.OUT_JSON, encoding='utf8'))
    levels = []
    for d in dl.LEVELS:
        data = res[str(d['id'])]
        spec, foods = level_spec(d, data)
        levels.append((d, spec, foods, data))
    for d, spec, foods, data in levels:
        path = os.path.join(LEVEL_DIR, 'Level%02d.asset' % d['id'])
        open(path, 'w', encoding='utf8', newline='\n').write(asset_text(d, spec, foods, data))
    write_check_cs(levels)
    patch_vitri(levels)
    patch_thietke(levels)
    patch_json(levels)
    print('đã ghi', len(levels), 'màn')


if __name__ == '__main__':
    main()
