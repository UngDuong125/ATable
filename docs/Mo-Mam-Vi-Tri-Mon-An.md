# Một Mâm — Vị trí món ăn từng màn

File thiết kế riêng để xếp mâm. Tọa độ ở đây là nguồn khi cắm `LevelConfig` / `Levels/Level0N.asset`.

Luật chơi nằm ở `Mo-Mam-Tai-Lieu-Thiet-Ke.md`. Số lượt và lời giải tổng quát nằm ở `Mo-Mam-Thiet-Ke-Man.md`. File này lo **chỗ đứng của món**, **ai thèm / chưa thèm**, **cửa sổ an toàn**, và **hằng số vùng ăn / thời gian** đã chốt cho code.

---

## 1. Hệ tọa độ

Mâm nhìn từ trên xuống. Tọa độ cực trên mặt mâm, lúc bắt đầu màn (`trayRotation = 0`).

| Ký hiệu | Nghĩa | Đơn vị |
|---|---|---|
| `angleDeg` | Góc món trên mâm | độ, ngược chiều kim đồng hồ, `0` = phải màn hình (đông) |
| `radius` | Khoảng cách tới tâm mâm | `0` = tâm, `1` = mép trong của vành mâm |
| `size` | Bán kính vẽ món | cùng hệ với `radius`, mặc định `0.08`. Không cộng vào vùng ăn |

### Neo khách (đứng yên trên màn hình)

| Chế độ | Khách | `guestAnchorDeg` | Tâm đầu |
|---|---|---|---|
| Một khách | Gumiho, Hổ, Mê, Cò, Dokkaebi | `270` | đáy màn hình |
| Hai khách | Gumiho | `180` | mép trái |
| Hai khách | Hổ | `0` | mép phải |

Hình mỗi khách chỉ là cái đầu. Tâm object là tâm ảnh, không phải cằm hay chân. `guestDrawRadius` đo tới tâm đó.

| Chỗ đứng | Bán kính | World |
|---|---|---|
| Một khách, đáy | `1.18` | `(0, -2.95)` |
| Gumiho, mép trái | `1.42` | `(-3.55, 0)` |
| Hổ, mép phải | `1.42` | `(3.55, 0)` |

PPU của mỗi file bằng chiều rộng ảnh, scale scene `1`, nên đầu rộng khoảng `1` đơn vị Unity. Trục trên của ảnh hướng vào tâm mâm. Khoảng trống dưới cằm, nếu có, hướng ra ngoài vành. Đầu nằm ngoài mép trong (`radius = 1`) nên không che món. Không gật, không đưa tay, không cụng: sprite không có cổ và không có tay. Nhát hỏng thì script lật ngang cái đầu.

Khi mâm xoay góc `θ`, góc món trên màn hình = `angleDeg + θ`.

### Ba vành đặt món (khuyến nghị)

| Vành | `radius` | Dùng khi |
|---|---|---|
| Trong | `0.38` | món gần tâm, khó với vòng Gumiho nhỏ, dễ với khe Hổ hẹp |
| Giữa | `0.58` | vành chính hầu hết màn |
| Ngoài | `0.78` | sát mép, vào vùng ăn sớm khi vùng lớn |

Không đặt món `radius < 0.28` (lọt tâm, khó đọc) hay `> 0.86` (dính vành trang trí).

### Tham số vùng ăn

Số này đã chốt. Đổi thì sửa cả đoạn dưới đây và khối `constants` trong JSON.

`u = clamp01(thời gian giữ / holdToMaxSeconds)`, với `holdToMaxSeconds = 0.50`. Mọi bán kính và chiều dài nội suy tuyến tính theo `u`.

**Gumiho — vòng lệch tâm**

- Tâm vòng cách tâm mâm `0.62` về phía `guestAnchor`.
- `r = lerp(0.20, rMax, u)`. Một khách: `rMax = 0.48`. Hai khách: `rMax = 0.47`.
- Món nằm trong vòng khi khoảng cách từ tâm vòng tới **tâm món** ≤ `r`.
- Với `rMax = 0.47`, vòng chạm đường tròn bán kính `0.15` quanh tâm mâm và không đi vào trong.

**Hổ — khe hai đường**

- Tia neo cố định trên màn hình, từ tâm mâm ra `guestAnchor`.
- `w = lerp(0.08, 0.28, u)`. `w` là nửa bề rộng đo **vuông góc** với tia neo, không đo theo cung.
- `along` = hình chiếu tâm món lên tia neo. `perp` = khoảng cách vuông góc từ tâm món tới tia đó.
- Trong khe khi `perp ≤ w` và `along > ho.minAlong`. JSON: `minAlong = 0`, so sánh lớn hơn, nên đường thẳng qua tâm không tính.
- Màn hai khách: `along ≥ ho.minAlongDual` với `minAlongDual = 0.15`. Khe không chui qua khoảng trống giữa hai khách.

**Mê — dải nửa mâm**

- Cùng `along` / `perp` theo neo Mê đang đứng.
- `len = lerp(0.35, 0.95, u)`, đo từ mép vào trong. Mép trong của dải nằm tại `along = 1 − len`.
- Nửa bề rộng cố định `0.10`. Giữ lâu làm dải dài thêm, không làm dải rộng thêm.
- Trong dải khi `along > 0`, `perp ≤ 0.10`, và `1 − along ≤ len`.
- `len = 0.95` dừng tại `along = 0.05`, gần tâm, không ăn sang nửa kia.
- `len > 0.55` lúc thả thì Mê lướt, `guestAnchor += 180`. Đúng `0.55` thì đứng yên. Màn có thể ghi `crossAt` khác `0.55`.
- Dải lắc quanh neo ở mọi màn có Mê, kể từ màn 8. Không có cờ để bật lắc từ một màn sau. Khi chưa giữ: `6° * sin(2π * 0.8 * t)`. Dương = lệch ngược chiều kim đồng hồ. Khách ở đáy thì đỉnh dương về phía phải. Khách không lắc, chỉ dải lắc.
- Nhấn giữ: góc dải đóng băng đúng giá trị lúc bấm, không cộng thời gian lắc nữa. Cùng lúc mâm giảm tốc, `spinSpeed * (1 − clamp01(giữ / brakeTime))`. Dải vẫn dài thêm theo `u`. Thả rồi hết nhịp khóa thì dải lắc tiếp từ pha đã dừng.

**Cò — chấm chạy vào tâm**

- Tâm chấm cách tâm mâm `lerp(0.82, 0.32, u)` trên tia neo. Bán kính chấm cố định `0.13`.
- Trong chấm khi khoảng cách từ tâm món tới tâm chấm ≤ `0.13`.

**Dokkaebi — đốm lửa trên tia tâm–mép**

- Đốm bán kính `0.14`.
- Chưa giữ, tâm đốm chạy trên tia neo: `along = lerp(0, 1, 0.5 + 0.5 * sin(2π * 0.25 * t))`. `0` là tâm mâm, `1` là mép. Một vòng ra–vào mất `4` giây.
- Nhấn giữ: `along` đóng băng đúng lúc bấm. Góc đốm = neo + `lerp(0°, 42°, u)`, lệch ngược chiều kim đồng hồ. Khách ở đáy thì quét về phía phải.
- Trong đốm khi khoảng cách từ tâm món tới tâm đốm ≤ `0.14`. Đốm không to ra.
- Thả rồi hết nhịp khóa thì đốm chạy tiếp từ pha đã dừng.

### Quy ước trạng thái món

| Cột | Giá trị |
|---|---|
| `want` | khách thèm món này lúc đầu màn |
| `forbid` | khách chưa tới lượt (bẫy) |
| `idle` | không liên quan icon (hiếm; ưu tiên gán `forbid` nếu nằm gần vùng) |
| `swap` (mọi màn có Dokkaebi) | trạng thái sau mốc đổi icon |

`dishId` lấy từ bộ 8 món:

`banh_mi`, `goi_cuon`, `pho`, `ca_phe`, `gimbap`, `songpyeon`, `tteokbokki`, `bibimbap`

### Các hằng số đã chốt

Khối `constants` trong `Mo-Mam-Vi-Tri-Mon-An.json` là bản máy đọc. Đoạn này là nghĩa của từng số.

**Thời gian**

| Hằng | Giá trị | Cách dùng |
|---|---|---|
| `holdToMaxSeconds` | `0.50` | `u` ở trên. Giữ đủ nửa giây thì vùng đạt mức tối đa. |
| `brakeTime` | `0.35` | Trong lúc giữ, tốc độ = `spinSpeed * (1 − clamp01(giữ / 0.35))`. Sau `0.35s` mâm đứng. Quãng trôi tới khi dừng = `spinSpeed * 0.35 / 2` độ. Màn không ghi `brakeTime` thì lấy số này. Màn 4 ghi `0.35`, trùng mặc định. |
| `feedbackLockSeconds` | `0.35` | Sau nhát đã tính: khóa tay, mâm đứng. Hết giờ thì tốc độ về ngay `spinSpeed`, không tăng dần. |
| `meSlideSeconds` | `0.40` | Nhát làm Mê lướt thì thời gian khóa lấy max với `0.40`. Neo chạy sang `+180` trong khoảng đó. Món của nhát này đã xét ở mép cũ. |
| `meSwayIdle` | `6` | Biên độ khi dải đang lắc, chưa giữ. |
| `meSwayMax` | `18` | Không còn nới lúc giữ. Giữ thì góc đóng băng. |
| `meSwayHz` | `0.8` | Số vòng lắc mỗi giây khi chưa giữ. |
| `dokkaAlongMin` | `0` | Tâm đốm lúc sâu nhất, trùng tâm mâm. |
| `dokkaAlongMax` | `1` | Tâm đốm lúc nông nhất, trên mép mâm. |
| `dokkaAlongHz` | `0.25` | Số vòng tâm–mép–tâm mỗi giây khi chưa giữ. |
| `swapBlinkSeconds` | `0.40` | Màn có Dokkaebi: icon nhấp trong lúc khóa, rồi mới đổi want/forbid. |
| `clearRingSeconds` | `0.70` | Qua màn: vòng sáng nở từ vành mâm rồi tắt. Thua màn thì không có vòng. |

Ngón thứ hai bỏ qua. Chuột trái giữ và thả giống một ngón. Thả ngón là tính nhát. Chỉ màn hai khách, ngón đang giữ mà sang nửa kia, mới hủy: không trừ lượt, không đếm đổi icon, mâm chạy lại ngay.

**Xét ăn**

Chỉ xét tâm món. `foodVisualRadius = 0.08` chỉ để vẽ.

Nhát tính cho khách của nửa màn hình đang thả. Màn một khách bỏ qua vị trí ngang.

| Nhát | Mạng | Lượt | Món | Đếm `swapAfterBite` |
|---|---|---|---|---|
| Có ít nhất một `forbid` của khách đó | −1 | −1 | giữ nguyên | có |
| Không forbid, có ít nhất một `want` | 0 | −1 | xóa các want nằm trong vùng | có |
| Không trúng món nào | 0 | −1 | giữ nguyên | có |

Đổi icon khi số nhát đã tính ≥ `swapAfterBite`, sau nhịp khóa của nhát đó. Món đã ăn thì khỏi mâm. Món còn lại thay `want` / `forbid` bằng `wantAfterSwap` / `forbidAfterSwap`. Hai mảng rỗng nghĩa là món đó không còn thèm và không còn cấm.

**Màn hình**

| Hằng | Giá trị |
|---|---|
| Khung tham chiếu | `1080 × 1920` |
| `trayWorldRadius` | `2.5` đơn vị Unity cho `radius = 1` |
| `orthoSize` | `6.1` |
| `guestDrawRadius` | `1.18` — tâm đầu khi một khách, ngoài vành. Hai khách dùng `1.42` |

Spin dương làm `θ` tăng. Góc món trên màn hình = `angleDeg + θ`.

**Kiểm nhanh với màn đã xếp**

- Màn 9, nhát 2: sau khi Mê đã sang mép `90°`, `ca_phe` (góc spawn `125`) nằm trong dải khi `θ` khoảng `317°` đến `333°`, tính lúc dải không lệch. `banh_mi` ở `105°` vẫn ngoài dải trong cửa sổ đó. Phải chờ gần một vòng.
- Màn 11–12: vòng Gumiho tối đa và khe Hổ cách nhau một khoảng trống bán kính `0.15` quanh tâm (15% đường kính mâm). Hai vùng không chồng.

---

## 2. Sơ đồ góc tham chiếu

Nhìn mâm, `trayRotation = 0`, một khách ở đáy (`270°`):

```
                    90° (trên)
                      |
                      |
        180° ---------+--------- 0° (phải)
         (trái)       |
                      |
                    270°
                  (khách)
```

Cụm “phía trước khách” = góc quanh `270°` (± khoảng vùng ăn).  
Cụm “đối diện khách” = quanh `90°`.  
Cụm “trái / phải vành” = quanh `180°` / `0°`.

---

## 3. Bảng nhanh 17 màn

| Màn | Khách | Xoay | Số món trên mâm | Thèm ban đầu | Cấm / bẫy | Lượt |
|---|---|---|---|---|---|---|
| 1 | Gumiho | 0 | 3 | 1 | 2 | 2 |
| 2 | Gumiho | 0 | 3 | 1 | 2 | 2 |
| 3 | Gumiho | chậm | 3 | 1 | 2 | 2 |
| 4 | Gumiho | vừa | 4 | 2 | 2 | 1 |
| 5 | Hổ | 0 | 3 | 1 | 2 | 2 |
| 6 | Hổ | chậm | 3 | 2 | 1 | 2 |
| 7 | Hổ | vừa | 4 | 2 | 2 | 1 |
| 8 | Mê | vừa | 3 | 1 | 2 | 1 |
| 9 | Mê | vừa | 4 | 2 | 2 | 2 |
| 10 | Gumiho | chậm | 4 | 3, mỗi nhát một món | 1, đi giữa các món thèm | 3 |
| 11 | Gumiho+Hổ | chậm | 4 | 2+2 | chồng vùng | 3 |
| 12 | Gumiho+Hổ | vừa | 4 | xung đột | bẫy chung | 3 |
| 13 | Hổ | 0 | 3 | 1, nới vừa tới món lệch trái | cấm kế bên; món xa hơn ngoài khe | 2 |
| 14 | Gumiho+Hổ | 0 | 4 | 1+1, thả ngay mỗi bên | cấm gần tâm phía trái | 2 |
| 15 | Cò | chậm | 3 | 1, đúng độ sâu | cấm mép và cấm gần tâm | 2 |
| 16 | Dokkaebi | 0 | 3 | 1 ở vành trong rồi đổi, thêm 1 sát mép | cấm sát mép trên tia; cấm cuối quét cùng vành trong | 3 |
| 17 | Dokkaebi | 0 | 4 | 2 rồi đổi, hai độ sâu | sau đổi, cấm món vừa thèm nếu còn | 4 |

---

## 4. Chi tiết từng màn

Góc ghi là góc **trên mâm lúc spawn**. Với màn đứng yên, góc màn hình = góc spawn. Với màn xoay, lời giải ghi thêm góc mâm lúc thả (`θ`).

---

### Màn 1 — Gumiho, đứng yên · thả ngay

**Ý đồ:** vòng nhỏ sẵn đã trùm đúng 1 món thèm. Món cấm ngoài `rMax`.

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| `songpyeon` | 270 | 0.58 | **want** |
| `banh_mi` | 90 | 0.58 | forbid |
| `bibimbap` | 20 | 0.78 | forbid |

```
        [banh_mi]
            90
             |
             ●
            / \
           /   \
 [bib]20       270 [songpyeon ★]  ← trong vòng r0
             khách
```

**Lời giải:** thả ngay (không cần giữ). 1 nhát ăn `songpyeon`. Còn 1 lượt dự phòng.

**Cửa sổ:** mọi góc giữ đều an toàn vì cấm ngoài `rMax`. Fail chỉ khi giữ cực đại rồi… vẫn không tới cấm — màn này gần như không fail bằng vùng; mạng chủ yếu để quen UI.

---

### Màn 2 — Gumiho, đứng yên · giữ vừa phải

**Ý đồ:** món thèm ngoài `r0`, trong một khoảng giữ. Món cấm sát phía ngoài `r` an toàn.

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| `tteokbokki` | 300 | 0.58 | **want** |
| `pho` | 200 | 0.38 | forbid |
| `goi_cuon` | 200 | 0.78 | forbid |

Tâm vòng Gumiho ở `radius≈0.62` phía 270°.  
`tteokbokki` ở `300°`, `radius 0.58`.  
`pho` ở `200°`, `radius 0.38`.  
`goi_cuon` cùng góc `200°`, `radius 0.78`.

**Lời giải:** giữ khoảng 35–55% thanh nạp (vòng vừa phủ `tteokbokki`, chưa tới `pho`), thả. Lượt dự phòng nếu nhả sớm hụt.

**Cửa sổ an toàn:** bán kính vòng `≈ 0.28–0.40`. Dưới → hụt. Trên → nuốt `pho`.

---

### Màn 3 — Gumiho, xoay chậm · chờ rồi thả nhanh

**Ý đồ:** thèm và cấm cùng vành `0.58`, lệch góc. Chờ món thèm vào vòng sẵn, thả trước khi cấm tới.

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| `gimbap` | 200 | 0.58 | **want** |
| `ca_phe` | 245 | 0.58 | forbid |
| `banh_mi` | 40 | 0.78 | forbid |

`spinSpeed ≈ 28°/s`. Vòng giữ ở `r0` (không giữ lâu).

**Lời giải:** không giữ (hoặc chạm rất ngắn). Chờ `θ ≈ 70°` để `gimbap` về ~270° (`200+70`), lúc đó `ca_phe` còn ở ~315° — ngoài vòng nhỏ. Thả. Còn 1 lượt dự phòng.

**Cửa sổ:** khoảng `θ ∈ [60°, 82°]`. Trễ hơn → `ca_phe` vào vòng. Sớm → hụt `gimbap`.

---

### Màn 4 — Gumiho, xoay · phanh ôm cụm 2 thèm

**Ý đồ:** hai món thèm sát nhau. Món cấm đi sát sau cụm. Phanh + nới vòng vừa ôm cặp thèm.

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| `songpyeon` | 200 | 0.58 | **want** |
| `tteokbokki` | 250 | 0.58 | **want** |
| `pho` | 320 | 0.58 | forbid |
| `goi_cuon` | 110 | 0.78 | forbid |

`spinSpeed ≈ 40°/s`. `brakeTime ≈ 0.35s` tới gần dừng.

**Lời giải:** khi cụm thèm sắp vào đáy, giữ để phanh và nới vòng vừa ôm `songpyeon` (`200°`) và `tteokbokki` (`250°`), thả trước khi `pho` (`320°`) vào. Một nhát hết 2 thèm.

`charges = 1`. Hai món thèm lệch `50°`, vòng lúc mới chạm chỉ rộng khoảng `±19°` trên vành `0.58`, nên thả nhanh không ôm cả hai. Hụt hoặc chỉ ăn một món thì hết lượt.

**Cửa sổ:** hai món thèm lệch `50°` trên vành `0.58`. `pho` ở `320°`, cùng vành. `goi_cuon` ở `110°`, `radius 0.78`.

---

### Màn 5 — Hổ, đứng yên · khe hẹp sẵn

**Ý đồ:** 1 thèm đúng đường tâm. Hai cấm hai bên ngoài `w0`.

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| `goi_cuon` | 270 | 0.58 | **want** |
| `gimbap` | 248 | 0.58 | forbid |
| `bibimbap` | 292 | 0.58 | forbid |

**Lời giải:** thả ngay, không nới khe. 1 nhát. 1 lượt dự phòng.

**Cửa sổ:** mọi mức giữ `w > ~0.12` tại vành 0.58 sẽ nuốt một trong hai cấm — dạy “đừng giữ thừa”.

---

### Màn 6 — Hổ, xoay chậm · cả ba món bên trái

**Ý đồ:** cả ba món nằm bên trái đường tâm, nên món thèm thứ hai vào khe ngay sau món thứ nhất. Cấm nằm giữa hai thèm. Một nhát rộng = hỏng.

Bên trái là cung từ `180°` tới `270°`. Mâm xoay ngược chiều kim đồng hồ, món trên cung này đi xuống miệng Hổ, không phải đi gần hết một vòng.

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| `goi_cuon` | 250 | 0.58 | **want** |
| `banh_mi` | 228 | 0.58 | forbid |
| `gimbap` | 206 | 0.58 | **want** |

`spinSpeed ≈ 24°/s`. Ba món cách nhau `22°`.

**Lời giải:**

1. Chờ `θ ≈ 20°` → `goi_cuon` về ~270°. Khe hẹp, thả. `banh_mi` lúc đó lệch `22°` về trái, `perp ≈ 0.22`, ngoài khe hẹp (`w = 0.08`) và trong khe nếu nới quá khoảng hai phần ba thanh.
2. Chờ `θ ≈ 64°` → `gimbap` về ~270°. Khe hẹp, thả. `banh_mi` lúc đó lệch `22°` sang phía kia của đường tâm.

Khoảng chờ giữa hai nhát là `44°`, khoảng `1,8` giây cộng nhịp khóa. Nới `w` quá `0.22` trong một trong hai nhát sẽ ăn `banh_mi`.

---

### Màn 7 — Hổ, xoay · cụm 2 thèm, cấm ở rìa

**Ý đồ:** hai thèm sát nhau gần đường tâm; cấm ở rìa cụm. Phanh và nới vừa đủ.

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| `gimbap` | 262 | 0.58 | **want** |
| `goi_cuon` | 278 | 0.58 | **want** |
| `pho` | 300 | 0.58 | forbid |
| `ca_phe` | 100 | 0.78 | forbid |

`spinSpeed ≈ 36°/s`.

**Lời giải:** khi tâm cụm thèm về 270° (`θ ≈ 8°`), phanh, nới `w ≈ 0.14` (ôm lệch ±8° tại 0.58), thả. Một nhát hết 2 thèm. `pho` @ +30° còn ngoài.

`charges = 1`.

**Cửa sổ:** `w ∈ [0.11, 0.18]`. Hẹp quá → sót một thèm, hết lượt. Rộng quá → `pho`.

---

### Màn 8 — Mê, xoay · một nhát dài lướt

**Ý đồ:** thèm nằm trên dải gần. Nhát dài ăn thèm + vượt `crossAt`, đáp mép kia. Dải mới lúc đáp không có cấm.

| dishId | angleDeg | radius | với Mê |
|---|---|---|---|
| `bibimbap` | 270 | 0.70 | **want** |
| `songpyeon` | 90 | 0.70 | forbid |
| `tteokbokki` | 0 | 0.50 | forbid |

Mê bắt đầu ở `270`. `spinSpeed ≈ 32°/s`.

**Lời giải:** chỉnh xoay nhẹ nếu cần để `bibimbap` thẳng dải (`θ ≈ 0` nếu đã sẵn 270). Giữ dài qua `crossAt`, thả. Ăn `bibimbap`, lướt sang `90`. Lúc đáp, `songpyeon` đang ở phía mới nhưng **đã là forbid và không còn thèm** — màn chỉ có 1 thèm, qua màn ngay khi ăn xong, trước khi nhát sau xảy ra. `songpyeon` / `tteokbokki` chỉ là bẫy nếu người chơi nhát ngắn rồi cố nhát 2 (nhưng hết lượt).

**Lượt = 1:** buộc phải nhát dài đúng. Nhát ngắn ăn được thèm nhưng không sao — vẫn qua màn vì hết thèm. (Nếu muốn bắt buộc lướt để dạy skill: chuyển `songpyeon` thành **want** và để nó chỉ vào dải sau khi lướt — đó là màn 9. Màn 8 giữ 1 thèm.)

**Cửa sổ:** miễn `bibimbap` nằm trên nửa dưới và dải đủ dài tới `radius 0.70`. Cấm lệch góc không nằm trên dải nếu không xoay nhầm quá lâu trước khi thả.

---

### Màn 9 — Mê, xoay · lướt rồi cắn nhát 2

**Ý đồ:** nhát dài ăn A + lướt. B spawn ở `125`, nên chỉ thẳng dải phía trên sau gần một vòng. Cắn B khi nó nằm trên dải, trước khi nó đi ra.

| dishId | angleDeg | radius | với Mê |
|---|---|---|---|
| `pho` | 255 | 0.68 | **want** (A) |
| `ca_phe` | 85 | 0.68 | **want** (B) |
| `banh_mi` | 110 | 0.68 | forbid |
| `gimbap` | 200 | 0.50 | forbid |

Mê bắt đầu `270`. `spinSpeed ≈ 34°/s`.

**Lời giải:**

1. Chờ `θ ≈ 15°` → A về ~270°. Giữ dài vượt `crossAt`, thả. Ăn A, Mê → `90`.
2. Sau khi đáp, xoay thêm tới `θ` tổng ≈ `50°` từ đầu: B (`85+50=135`… cần chỉnh) — **hiệu chỉnh góc spawn B** để sau lướt, với `θ₂` nhỏ, B nằm trên dải phía `90` còn `banh_mi` còn lệch.

Góc spawn chỉnh lại cho dễ playtest (bảng chính thức màn 9):

| dishId | angleDeg | radius | với Mê |
|---|---|---|---|
| `pho` | 270 | 0.68 | **want** A |
| `ca_phe` | 125 | 0.68 | **want** B |
| `banh_mi` | 105 | 0.68 | forbid |
| `gimbap` | 200 | 0.50 | forbid |

Nhát 1 tại `θ≈0`: dải đang gần giữa, nhấn giữ để đóng băng góc đó và phanh mâm. Giữ dài hơn `crossAt` rồi thả, lướt sang 90.  
Nhát 2: chờ `θ` khoảng `317°` đến `333°` thì `ca_phe` (`125 + θ`) về `90°`. `banh_mi` lúc đó vẫn lệch khỏi dải. Thả nhanh khi dải đang đè Cà phê.

**Cửa sổ nhát 2:** `θ ∈ [317°, 333°]`. Cà phê nằm trên dải phía trên. Nhấn giữ lúc dải đang đè Cà phê thì góc đóng băng và mâm phanh. Cà phê spawn ở `125`, nên không vào dải trên ngay sau lúc lướt. Nhát 1 nếu ngắn: hết 1 lượt, Mê còn ở 270; Cà phê phải đi gần nửa vòng nữa mới tới đáy, và Bánh mì dễ đi kèm. Lời giải dự kiến vẫn là nhát dài rồi chờ ở mép trên.

---

### Màn 10 — Gumiho, xoay chậm · bốn món cách xa

**Ý đồ:** không đổi món. Bốn món đứng cách nhau `90°` trên vành `0.58`. Vòng tối đa của Gumiho với tới khoảng `±47°` trên vành này, nên một nhát chỉ ăn được một món. Món cấm đi qua miệng giữa hai món thèm.

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| `songpyeon` | 250 | 0.58 | **want** |
| `pho` | 160 | 0.58 | forbid |
| `gimbap` | 70 | 0.58 | **want** |
| `banh_mi` | 340 | 0.58 | **want** |

`spinSpeed ≈ 26°/s`. `charges = 3`. `swapAfterBite = 0`.

Thứ tự vào đáy (`θ = (270 − angleDeg) mod 360`):

1. `θ ≈ 20°`: `songpyeon` vào vòng nhỏ. Thả nhanh.
2. `θ ≈ 110°`: `pho` đi qua đáy. Không thả.
3. `θ ≈ 200°`: `gimbap` vào vòng nhỏ. Thả nhanh.
4. `θ ≈ 290°`: `banh_mi` vào vòng nhỏ. Thả nhanh.

Khi một món nằm đúng đáy, món kế cách `90°`. Khoảng cách từ tâm vòng tới món đó khoảng `0.85`, lớn hơn `rMax = 0.48`. Giữ hết vẫn không nuốt kèm món bên cạnh. Cắn `pho` thì mất một lượt và một mạng, không đủ lượt cho ba món thèm.

---

### Màn 11 — Gumiho trái + Hổ phải · dạy chia nửa

**Ý đồ:** mỗi món chỉ đi vào vùng của đúng người thèm. Không xung đột.

Neo: Gumiho `180°`, Hổ `0°`. Vùng không chồng; chừa ~15% đường kính ở tâm. `spinSpeed ≈ 26°/s`. `charges = 3`.

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| `songpyeon` | 170 | 0.72 | **want** | forbid |
| `tteokbokki` | 200 | 0.55 | **want** | forbid |
| `gimbap` | 350 | 0.72 | forbid | **want** |
| `goi_cuon` | 20 | 0.55 | forbid | **want** |

```
     trên
       |
 Gumiho ----+---- Hổ
 180   tâm    0
```

**Lời giải (một trong các thứ tự):**

1. Nửa trái: chờ `songpyeon` vào vòng Gumiho, thả.  
2. Nửa phải: chờ `gimbap` vào khe Hổ, thả.  
3. Lượt còn lại: ăn nốt `tteokbokki` (Gumiho) hoặc `goi_cuon` (Hổ) — **thiếu 1 lượt**.  

Chỉnh: gộp một nhát ôm 2 thèm cùng khách, hoặc giảm còn 3 món thèm.

**Bảng ship màn 11 (3 thèm, 3 lượt):**

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| `songpyeon` | 175 | 0.70 | **want** | forbid |
| `gimbap` | 355 | 0.70 | forbid | **want** |
| `goi_cuon` | 15 | 0.55 | forbid | **want** |
| `pho` | 95 | 0.45 | forbid | forbid |

1. Trái — `songpyeon`.  
2. Phải — khe hẹp `gimbap` (chưa nới tới `goi_cuon`).  
3. Phải — chờ `goi_cuon` vào đường tâm, khe hẹp.

`pho` giữa trên là bẫy tâm / nhiễu, ngoài hai vùng mép nếu không nới quá tay.

---

### Màn 12 — Gumiho + Hổ · xung đột thời điểm

**Ý đồ:** một món Gumiho thèm / Hổ chưa thèm — phải cắn bằng Gumiho trước khi xoay sang miệng Hổ. Một món cả hai thèm nằm phía Hổ. Một bẫy chung.

| dishId | angleDeg | radius | Gumiho | Hổ | Ghi chú |
|---|---|---|---|---|---|
| `songpyeon` | 150 | 0.70 | **want** | forbid | xung đột: chỉ Gumiho được ăn |
| `gimbap` | 10 | 0.70 | **want** | **want** | cả hai thèm, đang phía Hổ |
| `banh_mi` | 190 | 0.55 | forbid | forbid | bẫy hai vùng |
| `goi_cuon` | 330 | 0.55 | forbid | **want** | chỉ Hổ |

`spinSpeed ≈ 38°/s`. `charges = 3`.

**Lời giải:**

1. **Ưu tiên trái:** khi `songpyeon` còn trong vòng Gumiho (`θ` nhỏ, món ~150°→ chưa sang nửa phải), giữ trái và thả. Trễ tới khi món qua ~0° phía Hổ → Hổ cắn sẽ hỏng (forbid với Hổ), Gumiho không với tới.
2. Phải — ăn `gimbap` (cả hai want, khe hoặc vòng bên phải tùy món đang trong vùng Hổ).
3. Phải — ăn `goi_cuon` khe hẹp, tránh `banh_mi` nếu đang gần.

**Cửa sổ xung đột `songpyeon`:** với `rMax = 0.47`, tâm món còn trong vòng Gumiho tới góc màn hình khoảng `221°` (`θ ≈ 71°`). Sau mốc đó Gumiho không với tới. Hổ chỉ với tới khi hình chiếu của món lên tia neo của Hổ ≥ `0.15`.

---

## 5. Màn 13–17

Cách chơi nằm ở `Huong-Dan-Man-13-16.md`. Màn 13 và 14 không còn là màn Mê. Dải lắc đã có ở màn 8 và 9.

### Màn 13 — Hổ, đứng yên · cụm bên trái

`spinSpeed = 0`. `charges = 2`. `hearts = 3`.

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| `goi_cuon` | 258 | 0.58 | **want** |
| `gimbap` | 240 | 0.52 | forbid |
| `bibimbap` | 222 | 0.40 | forbid |

Gỏi cuốn lệch `12°` về trái, `perp ≈ 0.12`. Khe lúc mới giữ (`w = 0.08`) chưa tới. Gimbap lệch `30°`, bán kính `0.52`, `perp ≈ 0.26`, chỉ vào khe khi giữ gần hết. Bibimbap lệch `48°`, `perp ≈ 0.30`, ngoài `wMax = 0.28`.

**Lời giải:** giữ khoảng `0.12` đến `0.40` giây rồi thả. Thả ngay thì hụt, còn 1 lượt. Giữ gần hết thì dính Gimbap.

### Màn 14 — hai khách, đứng yên

`guestMode = GumihoHo`. `spinSpeed = 0`. `charges = 2`. `hearts = 3`.

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| `songpyeon` | 175 | 0.72 | **want** | forbid |
| `banh_mi` | 180 | 0.35 | forbid | forbid |
| `gimbap` | 0 | 0.70 | forbid | **want** |
| `pho` | 95 | 0.45 | forbid | forbid |

Songpyeon cách tâm vòng Gumiho khoảng `0.12`, nằm trong vòng nhỏ. Bánh mì trên cùng tia, cách tâm vòng `0.27`, vào vòng khi giữ hơn khoảng một phần tư thanh. Gimbap nằm đúng khe hẹp của Hổ. Phở ở phía trên, ngoài cả hai vùng kể cả khi giữ hết.

**Lời giải:**

1. Nửa trái, thả ngay. Ăn Songpyeon. Giữ thì dính Bánh mì.
2. Nửa phải, thả ngay. Ăn Gimbap.

### Màn 15 — Cò, mâm xoay chậm

`spinSpeed = 28`, `charges = 2`, `hearts = 3`. Ba món vẫn cùng một tia. Lúc mở màn tia đang trước mỏ. Giữ khoảng `0.16` đến `0.34` giây khi tia còn trước mặt thì ăn Gỏi cuốn. Mâm phanh nên tia chỉ trôi vài độ trong nhát. Nếu để tia đi qua, chờ vòng sau.

| dishId | angleDeg | radius | với Cò |
|---|---|---|---|
| `goi_cuon` | 270 | 0.55 | **want** |
| `banh_mi` | 270 | 0.82 | forbid |
| `pho` | 270 | 0.34 | forbid |

### Màn 16 — Dokkaebi, khóa độ sâu rồi đổi sau nhát 1

`spinSpeed = 0`. `charges = 3`. `hearts = 3`. `swapAfterBite = 1`.

| dishId | angleDeg | radius | trước | sau |
|---|---|---|---|---|
| `banh_mi` | 270 | 0.84 | forbid | **want** |
| `gimbap` | 288 | 0.56 | **want** | **want** |
| `bibimbap` | 312 | 0.56 | forbid | forbid |

Gimbap và Bibimbap cùng vành trong. Bánh mì nằm trên tia, sát mép. Gimbap vẫn thèm sau khi đổi, để nhát hụt không xóa mục tiêu. Bánh mì thành thèm. Bibimbap vẫn cấm.

**Lời giải:**

1. Khóa `along` khoảng `0.56`, giữ `0.08` đến `0.30` giây, thả khi đốm nằm trên Gimbap. Icon đổi.
2. Khóa `along` khoảng `0.84`, thả ngay. Đốm đứng trên Bánh mì, món vừa thành thèm.

Còn 1 lượt nếu nhát đầu hụt: sau đổi vẫn phải ăn Gimbap rồi Bánh mì.

### Màn 17 — Dokkaebi, hai độ sâu rồi đổi sau nhát 2

`spinSpeed = 0`. `charges = 4`. `hearts = 3`. `swapAfterBite = 2`.

| dishId | angleDeg | radius | trước | sau |
|---|---|---|---|---|
| `goi_cuon` | 290 | 0.48 | **want** | |
| `ca_phe` | 296 | 0.80 | **want** | forbid |
| `banh_mi` | 270 | 0.80 | forbid | **want** |
| `bibimbap` | 312 | 0.80 | forbid | forbid |

Gỏi cuốn nằm vành trong. Ba món kia sát mép. Sau đổi, Gỏi cuốn không còn thèm, Cà phê thành cấm, Bánh mì thành thèm, Bibimbap vẫn cấm. Lời giải dùng 3 nhát, còn 1 lượt dự phòng. Chi tiết nhát nằm ở `Huong-Dan-Man-13-16.md`.

Hai khách gợi ý, chưa có màn: **Rùa** (vòng từ tâm lan ra) và **Quạt** (cánh mở góc về một phía). Quạt vẫn để sau hạn nộp.

---

## 6. JSON sẵn cắm Unity

Cùng dữ liệu, dạng máy đọc được. `spinSpeed` đơn vị độ/giây. `radius` chuẩn hóa.

```json
{
  "coord": {
    "angleZero": "east_screen_right_ccw",
    "radius": "0_center_1_inner_rim",
    "singleGuestAnchorDeg": 270,
    "dualGumihoAnchorDeg": 180,
    "dualHoAnchorDeg": 0
  },
  "constants": {
    "holdToMaxSeconds": 0.5,
    "brakeTime": 0.35,
    "feedbackLockSeconds": 0.35,
    "meSlideSeconds": 0.4,
    "swapBlinkSeconds": 0.4,
    "foodVisualRadius": 0.08,
    "hitTest": "food_center",
    "emptyBiteConsumesCharge": true,
    "resolvedBiteCountsForSwap": true,
    "centerGapRadius": 0.15,
    "trayWorldRadius": 2.5,
    "orthoSize": 6.1,
    "referenceWidth": 1080,
    "referenceHeight": 1920,
    "guestDrawRadius": 1.18,
    "guestDrawRadiusDual": 1.42,
    "gumiho": { "centerDistance": 0.62, "r0": 0.2, "rMax": 0.48, "rMaxDual": 0.47 },
    "clearRingSeconds": 0.7,
    "ho": { "w0": 0.08, "wMax": 0.28, "minAlong": 0, "minAlongDual": 0.15 },
    "me": { "len0": 0.35, "lenMax": 0.95, "halfWidth": 0.1, "crossAt": 0.55, "swayIdle": 6, "swayMax": 18, "swayHz": 0.8 },
    "co": { "radius": 0.13, "along0": 0.82, "alongMax": 0.32 },
    "dokkaebi": { "radius": 0.14, "alongMin": 0, "alongMax": 1, "alongHz": 0.25, "sweepMax": 42 }
  },
  "levels": [
    {
      "id": 1,
      "guestMode": "Gumiho",
      "spinSpeed": 0,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "songpyeon", "angleDeg": 270, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "banh_mi", "angleDeg": 90, "radius": 0.58, "want": [], "forbid": ["Gumiho"] },
        { "dishId": "bibimbap", "angleDeg": 20, "radius": 0.78, "want": [], "forbid": ["Gumiho"] }
      ]
    },
    {
      "id": 2,
      "guestMode": "Gumiho",
      "spinSpeed": 0,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "tteokbokki", "angleDeg": 300, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "pho", "angleDeg": 200, "radius": 0.38, "want": [], "forbid": ["Gumiho"] },
        { "dishId": "goi_cuon", "angleDeg": 200, "radius": 0.78, "want": [], "forbid": ["Gumiho"] }
      ]
    },
    {
      "id": 3,
      "guestMode": "Gumiho",
      "spinSpeed": 28,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "gimbap", "angleDeg": 200, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "ca_phe", "angleDeg": 245, "radius": 0.58, "want": [], "forbid": ["Gumiho"] },
        { "dishId": "banh_mi", "angleDeg": 40, "radius": 0.78, "want": [], "forbid": ["Gumiho"] }
      ]
    },
    {
      "id": 4,
      "guestMode": "Gumiho",
      "spinSpeed": 40,
      "charges": 1,
      "hearts": 3,
      "brakeTime": 0.35,
      "foods": [
        { "dishId": "songpyeon", "angleDeg": 200, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "tteokbokki", "angleDeg": 250, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "pho", "angleDeg": 320, "radius": 0.58, "want": [], "forbid": ["Gumiho"] },
        { "dishId": "goi_cuon", "angleDeg": 110, "radius": 0.78, "want": [], "forbid": ["Gumiho"] }
      ]
    },
    {
      "id": 5,
      "guestMode": "Ho",
      "spinSpeed": 0,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "goi_cuon", "angleDeg": 270, "radius": 0.58, "want": ["Ho"], "forbid": [] },
        { "dishId": "gimbap", "angleDeg": 248, "radius": 0.58, "want": [], "forbid": ["Ho"] },
        { "dishId": "bibimbap", "angleDeg": 292, "radius": 0.58, "want": [], "forbid": ["Ho"] }
      ]
    },
    {
      "id": 6,
      "guestMode": "Ho",
      "spinSpeed": 24,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "goi_cuon", "angleDeg": 250, "radius": 0.58, "want": ["Ho"], "forbid": [] },
        { "dishId": "banh_mi", "angleDeg": 228, "radius": 0.58, "want": [], "forbid": ["Ho"] },
        { "dishId": "gimbap", "angleDeg": 206, "radius": 0.58, "want": ["Ho"], "forbid": [] }
      ]
    },
    {
      "id": 7,
      "guestMode": "Ho",
      "spinSpeed": 36,
      "charges": 1,
      "hearts": 3,
      "foods": [
        { "dishId": "gimbap", "angleDeg": 262, "radius": 0.58, "want": ["Ho"], "forbid": [] },
        { "dishId": "goi_cuon", "angleDeg": 278, "radius": 0.58, "want": ["Ho"], "forbid": [] },
        { "dishId": "pho", "angleDeg": 300, "radius": 0.58, "want": [], "forbid": ["Ho"] },
        { "dishId": "ca_phe", "angleDeg": 100, "radius": 0.78, "want": [], "forbid": ["Ho"] }
      ]
    },
    {
      "id": 8,
      "guestMode": "Me",
      "spinSpeed": 32,
      "charges": 1,
      "hearts": 3,
      "crossAt": 0.55,
      "foods": [
        { "dishId": "bibimbap", "angleDeg": 270, "radius": 0.70, "want": ["Me"], "forbid": [] },
        { "dishId": "songpyeon", "angleDeg": 90, "radius": 0.70, "want": [], "forbid": ["Me"] },
        { "dishId": "tteokbokki", "angleDeg": 0, "radius": 0.50, "want": [], "forbid": ["Me"] }
      ]
    },
    {
      "id": 9,
      "guestMode": "Me",
      "spinSpeed": 34,
      "charges": 2,
      "hearts": 3,
      "crossAt": 0.55,
      "foods": [
        { "dishId": "pho", "angleDeg": 270, "radius": 0.68, "want": ["Me"], "forbid": [] },
        { "dishId": "ca_phe", "angleDeg": 125, "radius": 0.68, "want": ["Me"], "forbid": [] },
        { "dishId": "banh_mi", "angleDeg": 105, "radius": 0.68, "want": [], "forbid": ["Me"] },
        { "dishId": "gimbap", "angleDeg": 200, "radius": 0.50, "want": [], "forbid": ["Me"] }
      ]
    },
    {
      "id": 10,
      "guestMode": "Gumiho",
      "spinSpeed": 26,
      "charges": 3,
      "hearts": 3,
      "foods": [
        { "dishId": "songpyeon", "angleDeg": 250, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "pho", "angleDeg": 160, "radius": 0.58, "want": [], "forbid": ["Gumiho"] },
        { "dishId": "gimbap", "angleDeg": 70, "radius": 0.58, "want": ["Gumiho"], "forbid": [] },
        { "dishId": "banh_mi", "angleDeg": 340, "radius": 0.58, "want": ["Gumiho"], "forbid": [] }
      ]
    },
    {
      "id": 11,
      "guestMode": "GumihoHo",
      "spinSpeed": 26,
      "charges": 3,
      "hearts": 3,
      "foods": [
        { "dishId": "songpyeon", "angleDeg": 175, "radius": 0.70, "want": ["Gumiho"], "forbid": ["Ho"] },
        { "dishId": "gimbap", "angleDeg": 355, "radius": 0.70, "want": ["Ho"], "forbid": ["Gumiho"] },
        { "dishId": "goi_cuon", "angleDeg": 15, "radius": 0.55, "want": ["Ho"], "forbid": ["Gumiho"] },
        { "dishId": "pho", "angleDeg": 95, "radius": 0.45, "want": [], "forbid": ["Gumiho", "Ho"] }
      ]
    },
    {
      "id": 12,
      "guestMode": "GumihoHo",
      "spinSpeed": 38,
      "charges": 3,
      "hearts": 3,
      "foods": [
        { "dishId": "songpyeon", "angleDeg": 150, "radius": 0.70, "want": ["Gumiho"], "forbid": ["Ho"] },
        { "dishId": "gimbap", "angleDeg": 10, "radius": 0.70, "want": ["Gumiho", "Ho"], "forbid": [] },
        { "dishId": "banh_mi", "angleDeg": 190, "radius": 0.55, "want": [], "forbid": ["Gumiho", "Ho"] },
        { "dishId": "goi_cuon", "angleDeg": 330, "radius": 0.55, "want": ["Ho"], "forbid": ["Gumiho"] }
      ]
    },
    {
      "id": 13,
      "guestMode": "Ho",
      "spinSpeed": 0,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "goi_cuon", "angleDeg": 258, "radius": 0.58, "want": ["Ho"], "forbid": [] },
        { "dishId": "gimbap", "angleDeg": 240, "radius": 0.52, "want": [], "forbid": ["Ho"] },
        { "dishId": "bibimbap", "angleDeg": 222, "radius": 0.40, "want": [], "forbid": ["Ho"] }
      ]
    },
    {
      "id": 14,
      "guestMode": "GumihoHo",
      "spinSpeed": 0,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "songpyeon", "angleDeg": 175, "radius": 0.72, "want": ["Gumiho"], "forbid": ["Ho"] },
        { "dishId": "banh_mi", "angleDeg": 180, "radius": 0.35, "want": [], "forbid": ["Gumiho", "Ho"] },
        { "dishId": "gimbap", "angleDeg": 0, "radius": 0.70, "want": ["Ho"], "forbid": ["Gumiho"] },
        { "dishId": "pho", "angleDeg": 95, "radius": 0.45, "want": [], "forbid": ["Gumiho", "Ho"] }
      ]
    },
    {
      "id": 15,
      "guestMode": "Co",
      "spinSpeed": 28,
      "charges": 2,
      "hearts": 3,
      "foods": [
        { "dishId": "goi_cuon", "angleDeg": 270, "radius": 0.55, "want": ["Co"], "forbid": [] },
        { "dishId": "banh_mi", "angleDeg": 270, "radius": 0.82, "want": [], "forbid": ["Co"] },
        { "dishId": "pho", "angleDeg": 270, "radius": 0.34, "want": [], "forbid": ["Co"] }
      ]
    },
    {
      "id": 16,
      "guestMode": "Dokkaebi",
      "spinSpeed": 0,
      "charges": 3,
      "hearts": 3,
      "swapAfterBite": 1,
      "foods": [
        { "dishId": "banh_mi", "angleDeg": 270, "radius": 0.84, "want": [], "forbid": ["Dokkaebi"], "wantAfterSwap": ["Dokkaebi"], "forbidAfterSwap": [] },
        { "dishId": "gimbap", "angleDeg": 288, "radius": 0.56, "want": ["Dokkaebi"], "forbid": [], "wantAfterSwap": ["Dokkaebi"], "forbidAfterSwap": [] },
        { "dishId": "bibimbap", "angleDeg": 312, "radius": 0.56, "want": [], "forbid": ["Dokkaebi"], "wantAfterSwap": [], "forbidAfterSwap": ["Dokkaebi"] }
      ]
    },
    {
      "id": 17,
      "guestMode": "Dokkaebi",
      "spinSpeed": 0,
      "charges": 4,
      "hearts": 3,
      "swapAfterBite": 2,
      "foods": [
        { "dishId": "goi_cuon", "angleDeg": 290, "radius": 0.48, "want": ["Dokkaebi"], "forbid": [], "wantAfterSwap": [], "forbidAfterSwap": [] },
        { "dishId": "ca_phe", "angleDeg": 296, "radius": 0.80, "want": ["Dokkaebi"], "forbid": [], "wantAfterSwap": [], "forbidAfterSwap": ["Dokkaebi"] },
        { "dishId": "banh_mi", "angleDeg": 270, "radius": 0.80, "want": [], "forbid": ["Dokkaebi"], "wantAfterSwap": ["Dokkaebi"], "forbidAfterSwap": [] },
        { "dishId": "bibimbap", "angleDeg": 312, "radius": 0.80, "want": [], "forbid": ["Dokkaebi"], "wantAfterSwap": [], "forbidAfterSwap": ["Dokkaebi"] }
      ]
    }
  ]
}
```

Bản JSON sạch (không comment) để import: `docs/Mo-Mam-Vi-Tri-Mon-An.json`.

---

## 7. Checklist khi chỉnh tay

1. Xếp cụm góc trước, gán want/forbid sau.  
2. Với mỗi nhát trong lời giải, khoanh vùng ăn tại `θ` đó — không được có forbid trong vùng.  
3. Với mỗi nhát “giữ quá tay” dễ xảy ra, phải có ít nhất một forbid nằm ngay ngoài cửa sổ.  
4. Màn xoay: đo cửa sổ bằng giây (`Δθ / spinSpeed`) — mục tiêu ≥ 0.35s với màn dạy, ≥ 0.25s với màn khó.  
5. Đổi số trong file này trước, rồi mới sửa `Level0N.asset`.  
6. Chơi thử đủ lời giải bằng tay/giấy trước khi tin collider.

---

## 8. Liên kết

- Luật chơi: `Mo-Mam-Tai-Lieu-Thiet-Ke.md` §5–7, §11.
- Hồ sơ từng màn: `Mo-Mam-Thiet-Ke-Man.md`.  
- Dữ liệu máy: `Mo-Mam-Vi-Tri-Mon-An.json`.
