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
| Một khách | Gumiho, Hổ, Mê, Cò, Dokkaebi | `270` + `anchorOffset` | đáy màn hình |
| Hai khách | Gumiho | `180` | mép trái |
| Hai khách | Hổ | `0` | mép phải |

`anchorOffset` là trường riêng của từng màn, mặc định `0`, tính bằng độ. Âm = khách lệch sang trái khi ngồi phía dưới. Hiện chỉ màn 14 dùng: `-12`, nên Cò ngồi ở `258°`. Vùng ăn của khách đó đi theo neo mới. Màn hai khách bỏ qua trường này.

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
- Nửa bề rộng cố định `0.13`. Giữ lâu làm dải dài thêm, không làm dải rộng thêm.
- Trong dải khi `along > 0`, `perp ≤ 0.13`, và `1 − along ≤ len`.
- `len = 0.95` dừng tại `along = 0.05`, gần tâm, không ăn sang nửa kia.
- `len > 0.50` lúc thả thì Mê lướt, `guestAnchor += 180`. Đúng `0.50` thì đứng yên. Màn có thể ghi `crossAt` khác `0.50`. Giữ hơn một phần tư thanh (khoảng `0.125` giây) là đã lướt. Khi dải đủ dài để lướt, vùng dải sáng và đậm hơn, người chơi biết nhát thả lúc đó sẽ làm Mê đổi mép.
- Dải lắc quanh neo ở mọi màn có Mê, kể từ màn 8. Không có cờ để bật lắc từ một màn sau. Khi chưa giữ: `5° * sin(2π * 0.6 * t)`. Dương = lệch ngược chiều kim đồng hồ. Khách ở đáy thì đỉnh dương về phía phải. Khách không lắc, chỉ dải lắc.
- Nhấn giữ: góc dải đóng băng đúng giá trị lúc bấm, không cộng thời gian lắc nữa. Cùng lúc mâm giảm tốc, `spinSpeed * (1 − clamp01(giữ / brakeTime))`. Dải vẫn dài thêm theo `u`. Thả rồi hết nhịp khóa thì dải lắc tiếp từ pha đã dừng.

**Cò — chấm chạy vào tâm**

- Tâm chấm cách tâm mâm `lerp(0.82, 0.32, u)` trên tia neo. Bán kính chấm cố định `0.13`.
- Trong chấm khi khoảng cách từ tâm món tới tâm chấm ≤ `0.13`.

**Dokkaebi — đốm lửa trên tia tâm–mép**

- Đốm bán kính `0.14`.
- Chưa giữ, tâm đốm chạy trên tia neo: `along = lerp(0, 1, 0.5 + 0.5 * sin(2π * 0.40 * t))`. `0` là tâm mâm, `1` là mép. Một vòng ra–vào mất `2,5` giây.
- Nhấn giữ: `along` đóng băng đúng lúc bấm. Giữ dưới `dokkaTapSeconds = 0.15` thì đốm đứng yên tại neo. Quá mốc đó, góc đốm = neo + `lerp(0°, 42°, (giữ − 0.15) / 0.80)`, lệch ngược chiều kim đồng hồ. Khách ở đáy: nhấn nửa phải thì quét sang phải, nhấn nửa trái thì quét sang trái.
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
| `meSwayHz` | `0.6` | Số vòng lắc mỗi giây khi chưa giữ. |
| `dokkaAlongMin` | `0` | Tâm đốm lúc sâu nhất, trùng tâm mâm. |
| `dokkaAlongMax` | `1` | Tâm đốm lúc nông nhất, trên mép mâm. |
| `dokkaAlongHz` | `0.40` | Số vòng tâm–mép–tâm mỗi giây khi chưa giữ. Một vòng mất `2,5` giây. |
| `dokkaTapSeconds` | `0.15` | Giữ dưới mốc này thì đốm chưa quét. Quá mốc thì quét từ `0°` tới `42°` trong `dokkaSweepSeconds = 0.80` giây, tức giữ tổng cộng `0.95` giây mới quét hết. |
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

- Màn 18–20: vòng Gumiho tối đa và khe Hổ cách nhau một khoảng trống bán kính `0.15` quanh tâm (15% đường kính mâm). Hai vùng không chồng.

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

## 3. Bảng nhanh 20 màn

Số món là tổng món trên mâm (thèm + cấm + trang trí). Món trang trí không ai thèm hay cấm, nằm sẵn trong asset. Thời gian là thời gian ngắn nhất để qua màn theo lời giải. «Độ khó» là điểm tổng hợp ở mục 3.1.

| Màn | Khách | Xoay (°/s) | Số món | Thèm | Cấm | Nhát | Lượt | Mạng | Thời gian | Độ khó |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Gumiho | 0 | 10 | 3 | 1 | 1 | 2 | 3 | 0.7 s | 1.5 |
| 2 | Gumiho | 0 | 10 | 3 | 2 | 1 | 2 | 2 | 1.0 s | 5.0 |
| 3 | Gumiho | 10 | 10 | 2 | 2 | 2 | 3 | 2 | 13.4 s | 10.5 |
| 4 | Gumiho | 16 | 10 | 2 | 3 | 2 | 2 | 2 | 12.2 s | 16.5 |
| 5 | Hổ | 0 | 11 | 3 | 1 | 1 | 2 | 3 | 0.6 s | 6.5 |
| 6 | Hổ | 14 | 11 | 3 | 2 | 2 | 2 | 2 | 11.6 s | 19.5 |
| 7 | Hổ | 20 | 11 | 3 | 3 | 3 | 3 | 2 | 17.2 s | 25.5 |
| 8 | Mê | 0 | 11 | 3 | 1 | 2 | 3 | 3 | 2.8 s | 11.5 |
| 9 | Mê | 16 | 11 | 3 | 2 | 3 | 3 | 2 | 12.2 s | 28.0 |
| 10 | Mê | 22 | 11 | 4 | 2 | 4 | 4 | 2 | 16.7 s | 32.5 |
| 11 | Gumiho | 38 | 12 | 4 | 2 | 4 | 4 | 2 | 8.3 s | 35.5 |
| 12 | Hổ | 40 | 12 | 4 | 2 | 4 | 5 | 2 | 10.3 s | 38.0 |
| 13 | Cò | 0 | 12 | 3 | 2 | 2 | 3 | 3 | 1.3 s | 21.0 |
| 14 | Cò | 24 | 12 | 4 | 2 | 4 | 4 | 2 | 17.5 s | 41.0 |
| 15 | Dokkaebi | 0 | 12 | 6 | 3 | 4 | 5 | 3 | 4.6 s | 33.5 |
| 16 | Dokkaebi | 20 | 12 | 5 | 4 | 4 | 5 | 2 | 8.5 s | 42.0 |
| 17 | Dokkaebi | 26 | 12 | 4 | 4 | 4 | 4 | 2 | 11.8 s | 46.5 |
| 18 | Gumiho trái + Hổ phải | 0 | 13 | 4 | 6 | 2 | 3 | 3 | 1.6 s | 34.0 |
| 19 | Gumiho trái + Hổ phải | 22 | 13 | 4 | 6 | 4 | 4 | 2 | 16.0 s | 50.5 |
| 20 | Gumiho trái + Hổ phải | 32 | 13 | 5 | 5 | 4 | 4 | 2 | 12.5 s | 51.5 |

### 3.1 Độ khó tăng dần

Điểm độ khó = `2 × bậc khách` (Gumiho 0, Hổ 1, Mê 2, Cò 3, Dokkaebi 4, hai khách 5) + `xoay / 4` + `3 × (nhát − 1)` + `1,5 × số món cấm` + `2` nếu 2 mạng + `3` nếu không còn lượt dự phòng + `3 × bậc cửa sổ nhịp`.

Cửa sổ nhịp là phạm vi sai nhịp mà lời giải còn đúng: ±chờ / ±giữ (giây). Bậc 0: ±0,20 / ±0,06. Bậc 1: ±0,18 / ±0,05. Bậc 2: ±0,15 / ±0,045. Bậc 3: ±0,13 / ±0,04. Bậc 4: ±0,12 / ±0,035; riêng Dokkaebi xoay (màn 16, 17) ±0,08 / ±0,03 và màn 15 ±0,07 / ±0,03, vì đốm chạy tâm–mép rất nhanh.

Qua các màn thường (không kể màn giới thiệu) điểm luôn tăng. Màn giới thiệu khách mới (1, 5, 8, 13, 15, 18) theo luật có sẵn là mâm đứng yên và dư một lượt, nên điểm thấp hơn màn thường ngay trước đó. Điểm giữa các màn giới thiệu cũng tăng dần.

| Màn | Điểm | Loại |
|---|---|---|
| 1 | 1.5 | giới thiệu khách |
| 2 | 5.0 | thường |
| 3 | 10.5 | thường |
| 4 | 16.5 | thường |
| 5 | 6.5 | giới thiệu khách |
| 6 | 19.5 | thường |
| 7 | 25.5 | thường |
| 8 | 11.5 | giới thiệu khách |
| 9 | 28.0 | thường |
| 10 | 32.5 | thường |
| 11 | 35.5 | thường |
| 12 | 38.0 | thường |
| 13 | 21.0 | giới thiệu khách |
| 14 | 41.0 | thường |
| 15 | 33.5 | giới thiệu khách |
| 16 | 42.0 | thường |
| 17 | 46.5 | thường |
| 18 | 34.0 | giới thiệu khách |
| 19 | 50.5 | thường |
| 20 | 51.5 | thường |

---

## 4. Chi tiết từng màn

Góc ghi là góc **trên mâm lúc spawn**. Với màn đứng yên, góc màn hình = góc spawn. Với màn xoay, góc màn hình = góc spawn + góc mâm.

Mọi màn do `tools/design_levels.py` sinh và `tools/mm_sim.py` kiểm: lời giải tồn tại, không nhát nào dính món cấm, và mỗi nhát còn đúng khi người chơi lệch nhịp trong phạm vi của bậc cửa sổ.

Luật xếp món áp cho cả 20 màn: dưới 3 món thèm thì không món thèm nào bắt đầu ở góc 250°–360°; món rải khắp mâm (khoảng trống góc lớn nhất không quá 62°, mỗi góc phần tư có ít nhất 2 món, cả vành trong lẫn vành ngoài đều có món); màn xoay đặt món thèm đầu cách vạch miệng khách 30° trở lên.

---

### Màn 1 — Gumiho

**Ý đồ:** Màn dạy Gumiho. Mâm đứng yên, vòng lớn dần khi giữ. Món cấm nằm sát món thèm để người chơi thấy phải thả đúng lúc.

Xoay `0`°/s · 2 lượt · 3 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `gimbap` | 275.1 | 0.78 | **want** Gumiho |
| `pho` | 257.3 | 0.8 | **want** Gumiho |
| `gimbap` | 248.3 | 0.38 | **want** Gumiho |
| `bibimbap` | 223.2 | 0.54 | forbid Gumiho |
| `goi_cuon` | 77 | 0.83 | trang trí |
| `songpyeon` | 327.1 | 0.31 | trang trí |
| `banh_mi` | 166 | 0.7 | trang trí |
| `tteokbokki` | 6.4 | 0.84 | trang trí |
| `ca_phe` | 125.9 | 0.49 | trang trí |
| `goi_cuon` | 28.8 | 0.76 | trang trí |

**Lời giải ngắn nhất:**

1. không chờ, giữ 0.32 giây → ăn `gimbap`, `pho`, `gimbap`.

Cửa sổ nhịp hẹp nhất: chờ không đổi (mâm không đổi theo giờ), giữ 0.30 giây.

**Gợi ý hiện trong màn:** Chạm nhanh: vòng nhỏ ăn món sáng gần miệng. Giữ lâu: vòng to dần. Đừng chạm món bị cấm.

---

### Màn 2 — Gumiho

**Ý đồ:** Mâm đứng yên, thêm một món cấm. Người chơi phải đọc vòng lớn dần trước khi thả.

Xoay `0`°/s · 2 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 304.7 | 0.43 | **want** Gumiho |
| `goi_cuon` | 258.8 | 0.78 | **want** Gumiho |
| `tteokbokki` | 240.6 | 0.62 | **want** Gumiho |
| `songpyeon` | 219.7 | 0.65 | forbid Gumiho |
| `ca_phe` | 341.2 | 0.28 | forbid Gumiho |
| `bibimbap` | 145.7 | 0.82 | trang trí |
| `banh_mi` | 73.2 | 0.41 | trang trí |
| `gimbap` | 13.8 | 0.77 | trang trí |
| `pho` | 179.4 | 0.73 | trang trí |
| `bibimbap` | 116.7 | 0.52 | trang trí |

**Lời giải ngắn nhất:**

1. không chờ, giữ 0.64 giây → ăn `tteokbokki`, `goi_cuon`, `tteokbokki`.

Cửa sổ nhịp hẹp nhất: chờ không đổi (mâm không đổi theo giờ), giữ 0.72 giây.

---

### Màn 3 — Gumiho

**Ý đồ:** Mâm xoay rất chậm. Có chưa tới ba món thèm nên không món nào bắt đầu ở vùng 250–360°: phải chờ mâm đưa tới.

Xoay `10`°/s · 3 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `bibimbap` | 163.6 | 0.56 | **want** Gumiho |
| `ca_phe` | 112.5 | 0.41 | **want** Gumiho |
| `songpyeon` | 97.6 | 0.65 | forbid Gumiho |
| `pho` | 74.8 | 0.35 | forbid Gumiho |
| `gimbap` | 300.5 | 0.66 | trang trí |
| `goi_cuon` | 258.8 | 0.77 | trang trí |
| `tteokbokki` | 348.6 | 0.68 | trang trí |
| `banh_mi` | 203.9 | 0.58 | trang trí |
| `gimbap` | 24.9 | 0.79 | trang trí |
| `goi_cuon` | 241.1 | 0.55 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 6.85 giây, giữ 0.66 giây → ăn `bibimbap`.
2. chờ 4.55 giây, giữ 0.68 giây → ăn `ca_phe`.

Cửa sổ nhịp hẹp nhất: chờ 2.15 giây, giữ 0.64 giây.

---

### Màn 4 — Gumiho

**Ý đồ:** Mâm xoay nhanh hơn, ba món cấm. Hai món thèm không ôm được trong một nhát, phải cắn hai lần.

Xoay `16`°/s · 2 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `bibimbap` | 55.7 | 0.35 | **want** Gumiho |
| `goi_cuon` | 164.3 | 0.37 | **want** Gumiho |
| `tteokbokki` | 204.9 | 0.47 | forbid Gumiho |
| `pho` | 37.9 | 0.6 | forbid Gumiho |
| `tteokbokki` | 204.7 | 0.28 | forbid Gumiho |
| `gimbap` | 312.5 | 0.72 | trang trí |
| `banh_mi` | 113.5 | 0.38 | trang trí |
| `songpyeon` | 237.5 | 0.64 | trang trí |
| `ca_phe` | 340.3 | 0.7 | trang trí |
| `gimbap` | 266 | 0.49 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 7.00 giây, giữ 0.30 giây → ăn `goi_cuon`.
2. chờ 3.55 giây, giữ 0.70 giây → ăn `bibimbap`.

Cửa sổ nhịp hẹp nhất: chờ 1.40 giây, giữ 0.38 giây.

---

### Màn 5 — Hổ

**Ý đồ:** Màn dạy Hổ. Mâm đứng yên, khe hẹp khi chạm và rộng dần khi giữ.

Xoay `0`°/s · 2 lượt · 3 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 271.3 | 0.72 | **want** Hổ |
| `ca_phe` | 278.9 | 0.48 | **want** Hổ |
| `tteokbokki` | 263.2 | 0.59 | **want** Hổ |
| `songpyeon` | 287.8 | 0.85 | forbid Hổ |
| `pho` | 116 | 0.57 | trang trí |
| `gimbap` | 11.5 | 0.83 | trang trí |
| `banh_mi` | 216.8 | 0.33 | trang trí |
| `bibimbap` | 45 | 0.6 | trang trí |
| `goi_cuon` | 164.3 | 0.82 | trang trí |
| `pho` | 335.7 | 0.36 | trang trí |
| `gimbap` | 77.9 | 0.69 | trang trí |

**Lời giải ngắn nhất:**

1. không chờ, giữ 0.24 giây → ăn `tteokbokki`, `ca_phe`, `tteokbokki`.

Cửa sổ nhịp hẹp nhất: chờ không đổi (mâm không đổi theo giờ), giữ 0.42 giây.

**Gợi ý hiện trong màn:** Hổ cắn theo khe. Chạm: khe hẹp. Giữ: khe rộng dần, đừng chạm món bị cấm.

---

### Màn 6 — Hổ

**Ý đồ:** Mâm xoay chậm. Món cấm đứng gần món thèm nên khe phải vừa đủ.

Xoay `14`°/s · 2 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `gimbap` | 211.4 | 0.35 | **want** Hổ |
| `ca_phe` | 122.3 | 0.81 | **want** Hổ |
| `gimbap` | 213.1 | 0.63 | **want** Hổ |
| `bibimbap` | 244.9 | 0.47 | forbid Hổ |
| `banh_mi` | 192.8 | 0.76 | forbid Hổ |
| `songpyeon` | 333.1 | 0.34 | trang trí |
| `goi_cuon` | 75.5 | 0.4 | trang trí |
| `tteokbokki` | 15.3 | 0.43 | trang trí |
| `pho` | 290.1 | 0.44 | trang trí |
| `songpyeon` | 169.6 | 0.47 | trang trí |
| `goi_cuon` | 51.2 | 0.55 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 3.80 giây, giữ 0.22 giây → ăn `gimbap`, `gimbap`.
2. chờ 6.40 giây, giữ 0.52 giây → ăn `ca_phe`.

Cửa sổ nhịp hẹp nhất: chờ 1.30 giây, giữ 0.38 giây.

---

### Màn 7 — Hổ

**Ý đồ:** Mâm xoay vừa, ba món cấm. Khe phải thu hẹp hoặc chờ mâm đưa món thèm vào đúng đường tâm.

Xoay `20`°/s · 3 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 41.2 | 0.55 | **want** Hổ |
| `pho` | 315 | 0.58 | **want** Hổ |
| `tteokbokki` | 120.9 | 0.8 | **want** Hổ |
| `gimbap` | 16.7 | 0.55 | forbid Hổ |
| `banh_mi` | 138.5 | 0.86 | forbid Hổ |
| `gimbap` | 64.9 | 0.67 | forbid Hổ |
| `bibimbap` | 196.7 | 0.39 | trang trí |
| `goi_cuon` | 268.9 | 0.43 | trang trí |
| `songpyeon` | 233.7 | 0.76 | trang trí |
| `ca_phe` | 334.2 | 0.66 | trang trí |
| `bibimbap` | 171.9 | 0.81 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 7.35 giây, giữ 0.14 giây → ăn `tteokbokki`.
2. chờ 3.80 giây, giữ 0.20 giây → ăn `tteokbokki`.
3. chờ 4.15 giây, giữ 0.52 giây → ăn `pho`.

Cửa sổ nhịp hẹp nhất: chờ 0.90 giây, giữ 0.34 giây.

---

### Màn 8 — Mê

**Ý đồ:** Màn dạy Mê. Mâm đứng yên, dải lắc nhẹ. Giữ quá nửa thanh thì Mê lướt sang mép đối diện.

Xoay `0`°/s · 3 lượt · 3 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `bibimbap` | 265.4 | 0.71 | **want** Mê |
| `banh_mi` | 266.2 | 0.39 | **want** Mê |
| `bibimbap` | 84.2 | 0.44 | **want** Mê |
| `gimbap` | 286.9 | 0.72 | forbid Mê |
| `goi_cuon` | 156.5 | 0.53 | trang trí |
| `tteokbokki` | 15.8 | 0.51 | trang trí |
| `pho` | 198.1 | 0.79 | trang trí |
| `songpyeon` | 326 | 0.68 | trang trí |
| `ca_phe` | 116.8 | 0.8 | trang trí |
| `goi_cuon` | 40.1 | 0.53 | trang trí |
| `tteokbokki` | 235.4 | 0.61 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 0.85 giây, giữ 0.60 giây (Mê lướt sang mép đối diện) → ăn `bibimbap`, `banh_mi`.
2. không chờ, giữ 0.58 giây (Mê lướt sang mép đối diện) → ăn `bibimbap`.

Cửa sổ nhịp hẹp nhất: chờ 1.75 giây, giữ 0.80 giây.

**Gợi ý hiện trong màn:** Chạm: dải ngắn. Giữ lâu hơn một chút: dải dài ra và Mê lướt sang mép đối diện.

---

### Màn 9 — Mê

**Ý đồ:** Mâm xoay chậm, ba nhát. Mỗi nhát dài đổi mép đứng của Mê.

Xoay `16`°/s · 3 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `bibimbap` | 106.3 | 0.76 | **want** Mê |
| `banh_mi` | 356.8 | 0.4 | **want** Mê |
| `bibimbap` | 271.7 | 0.67 | **want** Mê |
| `gimbap` | 296.4 | 0.72 | forbid Mê |
| `ca_phe` | 36.7 | 0.42 | forbid Mê |
| `tteokbokki` | 156.2 | 0.82 | trang trí |
| `goi_cuon` | 196.1 | 0.58 | trang trí |
| `songpyeon` | 246.2 | 0.4 | trang trí |
| `pho` | 82.5 | 0.78 | trang trí |
| `tteokbokki` | 329.2 | 0.64 | trang trí |
| `goi_cuon` | 216.3 | 0.84 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 0.30 giây, giữ 0.56 giây (Mê lướt sang mép đối diện) → ăn `bibimbap`.
2. chờ 5.00 giây, giữ 0.60 giây (Mê lướt sang mép đối diện) → ăn `banh_mi`.
3. chờ 4.60 giây, chạm → ăn `bibimbap`.

Cửa sổ nhịp hẹp nhất: chờ 0.65 giây, giữ 0.10 giây.

---

### Màn 10 — Mê

**Ý đồ:** Bốn món thèm, hai món cấm. Phải tính trước nhát nào lướt, nhát nào đứng yên.

Xoay `22`°/s · 4 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `gimbap` | 70.4 | 0.69 | **want** Mê |
| `banh_mi` | 18.7 | 0.47 | **want** Mê |
| `gimbap` | 154.9 | 0.56 | **want** Mê |
| `banh_mi` | 227.7 | 0.37 | **want** Mê |
| `songpyeon` | 343.5 | 0.28 | forbid Mê |
| `ca_phe` | 52.4 | 0.86 | forbid Mê |
| `bibimbap` | 297.2 | 0.79 | trang trí |
| `goi_cuon` | 126.1 | 0.72 | trang trí |
| `tteokbokki` | 204.3 | 0.71 | trang trí |
| `pho` | 262.6 | 0.64 | trang trí |
| `bibimbap` | 88.7 | 0.55 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 1.70 giây, giữ 0.62 giây (Mê lướt sang mép đối diện) → ăn `banh_mi`.
2. chờ 0.90 giây, giữ 0.58 giây (Mê lướt sang mép đối diện) → ăn `banh_mi`.
3. chờ 5.85 giây, giữ 0.56 giây (Mê lướt sang mép đối diện) → ăn `gimbap`.
4. chờ 4.35 giây, giữ 0.56 giây (Mê lướt sang mép đối diện) → ăn `gimbap`.

Cửa sổ nhịp hẹp nhất: chờ 0.70 giây, giữ 0.78 giây.

---

### Màn 11 — Gumiho

**Ý đồ:** Gumiho quay lại, mâm xoay nhanh nhất từ đầu game. Bốn món thèm rải bốn phía, mỗi nhát một món.

Xoay `38`°/s · 4 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 192.2 | 0.74 | **want** Gumiho |
| `ca_phe` | 32.1 | 0.67 | **want** Gumiho |
| `tteokbokki` | 122.7 | 0.49 | **want** Gumiho |
| `ca_phe` | 221.1 | 0.36 | **want** Gumiho |
| `songpyeon` | 156.4 | 0.28 | forbid Gumiho |
| `pho` | 215.2 | 0.86 | forbid Gumiho |
| `gimbap` | 283.8 | 0.68 | trang trí |
| `goi_cuon` | 331.2 | 0.5 | trang trí |
| `bibimbap` | 87.2 | 0.71 | trang trí |
| `banh_mi` | 259.1 | 0.73 | trang trí |
| `gimbap` | 10.8 | 0.33 | trang trí |
| `goi_cuon` | 68.4 | 0.6 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 0.25 giây, giữ 0.42 giây → ăn `ca_phe`.
2. chờ 1.55 giây, giữ 0.20 giây → ăn `tteokbokki`.
3. chờ 1.60 giây, giữ 0.20 giây → ăn `tteokbokki`.
4. chờ 2.20 giây, giữ 0.52 giây → ăn `ca_phe`.

Cửa sổ nhịp hẹp nhất: chờ 0.55 giây, giữ 0.34 giây.

---

### Màn 12 — Hổ

**Ý đồ:** Hổ quay lại, mâm xoay nhanh. Bốn món thèm rải khắp mâm, khe phải đúng nhịp từng lần.

Xoay `40`°/s · 5 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `gimbap` | 211.5 | 0.59 | **want** Hổ |
| `goi_cuon` | 86.3 | 0.47 | **want** Hổ |
| `gimbap` | 304.8 | 0.83 | **want** Hổ |
| `goi_cuon` | 173.2 | 0.44 | **want** Hổ |
| `tteokbokki` | 184.7 | 0.69 | forbid Hổ |
| `banh_mi` | 102.3 | 0.52 | forbid Hổ |
| `songpyeon` | 3.5 | 0.4 | trang trí |
| `pho` | 256.6 | 0.39 | trang trí |
| `bibimbap` | 58.2 | 0.79 | trang trí |
| `ca_phe` | 138.9 | 0.63 | trang trí |
| `songpyeon` | 342.8 | 0.44 | trang trí |
| `pho` | 34.3 | 0.67 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 1.05 giây, giữ 0.56 giây → ăn `gimbap`.
2. chờ 1.25 giây, chạm → ăn `goi_cuon`.
3. chờ 2.10 giây, chạm → ăn `goi_cuon`.
4. chờ 3.30 giây, giữ 0.52 giây → ăn `gimbap`.

Cửa sổ nhịp hẹp nhất: chờ 0.40 giây, giữ 0.30 giây.

---

### Màn 13 — Cò

**Ý đồ:** Màn dạy Cò. Mâm đứng yên. Chấm là độ sâu, không phải bề rộng.

Xoay `0`°/s · 3 lượt · 3 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `bibimbap` | 273.5 | 0.39 | **want** Cò |
| `ca_phe` | 275.5 | 0.67 | **want** Cò |
| `bibimbap` | 264 | 0.56 | **want** Cò |
| `tteokbokki` | 250.9 | 0.37 | forbid Cò |
| `banh_mi` | 287.9 | 0.59 | forbid Cò |
| `gimbap` | 47.1 | 0.38 | trang trí |
| `pho` | 171 | 0.8 | trang trí |
| `songpyeon` | 121.3 | 0.8 | trang trí |
| `goi_cuon` | 336.4 | 0.73 | trang trí |
| `gimbap` | 200.6 | 0.42 | trang trí |
| `pho` | 71.3 | 0.47 | trang trí |
| `songpyeon` | 18.6 | 0.82 | trang trí |

**Lời giải ngắn nhất:**

1. không chờ, giữ 0.20 giây → ăn `ca_phe`, `bibimbap`.
2. không chờ, giữ 0.36 giây → ăn `bibimbap`.

Cửa sổ nhịp hẹp nhất: chờ không đổi (mâm không đổi theo giờ), giữ 0.12 giây.

**Gợi ý hiện trong màn:** Cò chọc từ mép vào. Giữ để chấm đi sâu hơn. Chạm sớm thì chấm còn ở sát mép.

---

### Màn 14 — Cò

**Ý đồ:** Cò ngồi lệch trái 12°, mâm xoay chậm. Bốn món thèm đi qua mỏ lần lượt.

Xoay `24`°/s · 4 lượt · 2 mạng · `anchorOffset = -12`

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 220.9 | 0.66 | **want** Cò |
| `ca_phe` | 203.4 | 0.42 | **want** Cò |
| `tteokbokki` | 68.7 | 0.43 | **want** Cò |
| `ca_phe` | 265.9 | 0.41 | **want** Cò |
| `bibimbap` | 231.8 | 0.32 | forbid Cò |
| `pho` | 45.7 | 0.4 | forbid Cò |
| `songpyeon` | 337.3 | 0.47 | trang trí |
| `banh_mi` | 151.4 | 0.73 | trang trí |
| `gimbap` | 112.9 | 0.42 | trang trí |
| `goi_cuon` | 290.9 | 0.39 | trang trí |
| `songpyeon` | 19.2 | 0.73 | trang trí |
| `banh_mi` | 180.1 | 0.31 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 1.40 giây, giữ 0.16 giây → ăn `tteokbokki`.
2. chờ 0.60 giây, giữ 0.64 giây → ăn `ca_phe`.
3. chờ 5.15 giây, giữ 0.64 giây → ăn `tteokbokki`.
4. chờ 6.85 giây, giữ 0.64 giây → ăn `ca_phe`.

Cửa sổ nhịp hẹp nhất: chờ 0.70 giây, giữ 0.26 giây.

---

### Màn 15 — Dokkaebi

**Ý đồ:** Màn dạy Dokkaebi. Mâm đứng yên, đốm lửa chạy tâm–mép. Icon đổi sau nhát đầu.

Xoay `0`°/s · 5 lượt · 3 mạng · đổi icon sau nhát 1

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `songpyeon` | 303.2 | 0.77 | **want** Dokkaebi → sau đổi: trang trí |
| `goi_cuon` | 230.2 | 0.31 | **want** Dokkaebi → sau đổi: trang trí |
| `songpyeon` | 240 | 0.81 | **want** Dokkaebi → sau đổi: trang trí |
| `tteokbokki` | 271.1 | 0.44 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `banh_mi` | 298.3 | 0.61 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `tteokbokki` | 284.3 | 0.83 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `gimbap` | 102.5 | 0.34 | trang trí |
| `pho` | 46.9 | 0.41 | trang trí |
| `bibimbap` | 179.7 | 0.47 | trang trí |
| `ca_phe` | 342.6 | 0.59 | trang trí |
| `gimbap` | 153.4 | 0.41 | trang trí |
| `pho` | 18.1 | 0.8 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 0.25 giây, giữ 0.72 giây (nhấn nửa trái màn hình) → ăn `songpyeon`.
2. không chờ, giữ 0.42 giây (nhấn nửa phải màn hình) → ăn `tteokbokki`.
3. chờ 0.90 giây, giữ 0.68 giây (nhấn nửa phải màn hình) → ăn `banh_mi`.
4. chờ 0.15 giây, chạm → ăn `tteokbokki`.

Cửa sổ nhịp hẹp nhất: chờ 0.25 giây, giữ 0.36 giây.

**Gợi ý hiện trong màn:** Đốm lửa chạy từ tâm ra mép. Chạm nhanh: đốm đứng yên. Giữ lâu: đốm quét ngang. Sau nhát đầu, icon đổi.

---

### Màn 16 — Dokkaebi

**Ý đồ:** Mâm xoay chậm. Icon đổi sau nhát đầu: món cấm thành món thèm, món trung lập thành món cấm.

Xoay `20`°/s · 5 lượt · 2 mạng · đổi icon sau nhát 1

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 178.7 | 0.67 | **want** Dokkaebi → sau đổi: trang trí |
| `banh_mi` | 226 | 0.63 | **want** Dokkaebi → sau đổi: trang trí |
| `bibimbap` | 189.7 | 0.43 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `goi_cuon` | 206.5 | 0.54 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `bibimbap` | 132.4 | 0.84 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `songpyeon` | 166.8 | 0.62 | trang trí → sau đổi: forbid Dokkaebi |
| `pho` | 332.1 | 0.47 | trang trí |
| `gimbap` | 52.7 | 0.57 | trang trí |
| `ca_phe` | 269.3 | 0.82 | trang trí |
| `pho` | 17.5 | 0.44 | trang trí |
| `gimbap` | 81.6 | 0.45 | trang trí |
| `ca_phe` | 312.1 | 0.36 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 1.15 giây, giữ 0.44 giây (nhấn nửa trái màn hình) → ăn `banh_mi`.
2. chờ 1.30 giây, giữ 0.60 giây (nhấn nửa trái màn hình) → ăn `bibimbap`.
3. chờ 0.05 giây, giữ 0.20 giây (nhấn nửa phải màn hình) → ăn `goi_cuon`.
4. chờ 2.85 giây, giữ 0.48 giây (nhấn nửa trái màn hình) → ăn `bibimbap`.

Cửa sổ nhịp hẹp nhất: chờ 0.20 giây, giữ 0.34 giây.

---

### Màn 17 — Dokkaebi

**Ý đồ:** Mâm xoay nhanh hơn. Icon đổi sau nhát thứ hai, nên phải ăn đủ hai nhát trước khi đổi.

Xoay `26`°/s · 4 lượt · 2 mạng · đổi icon sau nhát 2

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 200.2 | 0.77 | **want** Dokkaebi → sau đổi: trang trí |
| `pho` | 239.8 | 0.38 | **want** Dokkaebi → sau đổi: trang trí |
| `songpyeon` | 5.1 | 0.57 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `ca_phe` | 25.8 | 0.71 | forbid Dokkaebi → sau đổi: **want** Dokkaebi |
| `gimbap` | 217.6 | 0.68 | trang trí → sau đổi: forbid Dokkaebi |
| `goi_cuon` | 185.9 | 0.86 | trang trí → sau đổi: forbid Dokkaebi |
| `bibimbap` | 85.6 | 0.76 | trang trí |
| `banh_mi` | 283.7 | 0.35 | trang trí |
| `bibimbap` | 119.1 | 0.61 | trang trí |
| `banh_mi` | 313.4 | 0.6 | trang trí |
| `bibimbap` | 145.3 | 0.82 | trang trí |
| `banh_mi` | 50.1 | 0.56 | trang trí |

**Lời giải ngắn nhất:**

1. chờ 1.35 giây, chạm → ăn `pho`.
2. chờ 1.40 giây, giữ 0.28 giây (nhấn nửa phải màn hình) → ăn `tteokbokki`.
3. chờ 4.95 giây, giữ 0.74 giây (nhấn nửa trái màn hình) → ăn `ca_phe`.
4. chờ 1.00 giây, giữ 0.56 giây (nhấn nửa trái màn hình) → ăn `songpyeon`.

Cửa sổ nhịp hẹp nhất: chờ 0.25 giây, giữ 0.34 giây.

---

### Màn 18 — Gumiho trái + Hổ phải

**Ý đồ:** Màn dạy hai khách. Mâm đứng yên. Mỗi khách hai món thèm riêng, món của người này là món cấm của người kia.

Xoay `0`°/s · 3 lượt · 3 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `songpyeon` | 158.3 | 0.82 | **want** Gumiho, forbid Hổ |
| `pho` | 159 | 0.34 | **want** Gumiho, forbid Hổ |
| `bibimbap` | 2.4 | 0.83 | **want** Hổ, forbid Gumiho |
| `ca_phe` | 0.4 | 0.51 | **want** Hổ, forbid Gumiho |
| `gimbap` | 19 | 0.86 | forbid Gumiho, Hổ |
| `goi_cuon` | 342.6 | 0.86 | forbid Gumiho, Hổ |
| `tteokbokki` | 260.2 | 0.33 | trang trí |
| `banh_mi` | 99 | 0.66 | trang trí |
| `tteokbokki` | 217 | 0.4 | trang trí |
| `banh_mi` | 311.6 | 0.41 | trang trí |
| `tteokbokki` | 46.7 | 0.84 | trang trí |
| `banh_mi` | 137.2 | 0.82 | trang trí |
| `tteokbokki` | 187.8 | 0.55 | trang trí |

**Lời giải ngắn nhất:**

1. bên phải, Hổ: không chờ, giữ 0.24 giây → ăn `bibimbap`, `ca_phe`.
2. bên trái, Gumiho: không chờ, giữ 0.62 giây → ăn `songpyeon`, `pho`.

Cửa sổ nhịp hẹp nhất: chờ không đổi (mâm không đổi theo giờ), giữ 0.42 giây.

**Gợi ý hiện trong màn:** Chạm nửa trái cho Gumiho, nửa phải cho Hổ. Mỗi khách chỉ ăn món sáng của mình.

---

### Màn 19 — Gumiho trái + Hổ phải

**Ý đồ:** Mâm xoay chậm, bốn nhát luân phiên giữa hai khách.

Xoay `22`°/s · 4 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `bibimbap` | 102.5 | 0.35 | **want** Gumiho, forbid Hổ |
| `goi_cuon` | 226.6 | 0.51 | **want** Gumiho, forbid Hổ |
| `gimbap` | 143.7 | 0.39 | **want** Hổ, forbid Gumiho |
| `ca_phe` | 44.1 | 0.7 | **want** Hổ, forbid Gumiho |
| `tteokbokki` | 31.8 | 0.86 | forbid Gumiho, Hổ |
| `banh_mi` | 159.9 | 0.48 | forbid Gumiho, Hổ |
| `songpyeon` | 295.7 | 0.68 | trang trí |
| `pho` | 356.1 | 0.56 | trang trí |
| `songpyeon` | 273.6 | 0.82 | trang trí |
| `pho` | 200.4 | 0.38 | trang trí |
| `songpyeon` | 319.3 | 0.31 | trang trí |
| `pho` | 67.1 | 0.31 | trang trí |
| `songpyeon` | 256.6 | 0.8 | trang trí |

**Lời giải ngắn nhất:**

1. bên trái, Gumiho: chờ 3.50 giây, giữ 0.28 giây → ăn `bibimbap`.
2. bên phải, Hổ: chờ 6.35 giây, giữ 0.16 giây → ăn `gimbap`.
3. bên trái, Gumiho: chờ 3.25 giây, giữ 0.52 giây → ăn `goi_cuon`.
4. bên phải, Hổ: chờ 0.25 giây, giữ 0.28 giây → ăn `ca_phe`.

Cửa sổ nhịp hẹp nhất: chờ 0.60 giây, giữ 0.26 giây.

---

### Màn 20 — Gumiho trái + Hổ phải

**Ý đồ:** Mâm xoay nhanh nhất. Có món cả hai cùng thèm, và món chỉ một khách được ăn khi nó nằm đúng vùng.

Xoay `32`°/s · 4 lượt · 2 mạng

| dishId | angleDeg | radius | vai trò |
|---|---|---|---|
| `tteokbokki` | 213.3 | 0.83 | **want** Gumiho, forbid Hổ |
| `pho` | 117.2 | 0.38 | **want** Gumiho, forbid Hổ |
| `songpyeon` | 7.8 | 0.42 | **want** Hổ, forbid Gumiho |
| `goi_cuon` | 237 | 0.38 | **want** Hổ, forbid Gumiho |
| `bibimbap` | 134.9 | 0.54 | **want** Gumiho, Hổ |
| `ca_phe` | 156.7 | 0.63 | forbid Gumiho, Hổ |
| `gimbap` | 276.5 | 0.75 | trang trí |
| `banh_mi` | 63.2 | 0.74 | trang trí |
| `gimbap` | 315.8 | 0.52 | trang trí |
| `banh_mi` | 181.9 | 0.4 | trang trí |
| `gimbap` | 35 | 0.34 | trang trí |
| `banh_mi` | 83.4 | 0.57 | trang trí |
| `gimbap` | 347 | 0.46 | trang trí |

**Lời giải ngắn nhất:**

1. bên phải, Hổ: không chờ, giữ 0.52 giây → ăn `songpyeon`.
2. bên trái, Gumiho: chờ 1.60 giây, giữ 0.24 giây → ăn `pho`, `bibimbap`.
3. bên phải, Hổ: chờ 1.05 giây, giữ 0.58 giây → ăn `goi_cuon`.
4. bên trái, Gumiho: chờ 6.55 giây, giữ 0.56 giây → ăn `tteokbokki`.

Cửa sổ nhịp hẹp nhất: chờ 0.75 giây, giữ 0.34 giây.

---

## 5. Ghi chú

Cách chơi nằm ở `Huong-Dan-Man-5-12.md` (màn 5–7, 9–11, 19 và 20) và `Huong-Dan-Man-13-16.md` (màn 8 và 12–18). Dải lắc có ở mọi màn Mê, từ màn 8.

---

## 6. JSON sẵn cắm Unity

Dữ liệu máy đọc được nằm ở `docs/Mo-Mam-Vi-Tri-Mon-An.json`, cùng nội dung với bảng ở mục 4. `spinSpeed` đơn vị độ/giây. `radius` chuẩn hóa. Không chép lại cả khối JSON vào đây để khỏi lệch.

Bản JSON sạch (không comment) để import: `docs/Mo-Mam-Vi-Tri-Mon-An.json`.

---

## 7. Checklist khi chỉnh tay

1. Xếp cụm góc trước, gán want/forbid sau. Dưới 3 món thèm thì không món thèm nào bắt đầu ở 250°–360°. Món rải khắp mâm.  
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
