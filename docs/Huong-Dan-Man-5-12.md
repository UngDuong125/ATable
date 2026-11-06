# Hướng dẫn xếp màn 5–12

Màn 1–4 giữ nguyên `Huong-Dan-Hierarchy.md`. File này thêm khe Hổ, dải Mê, màn 10 với các món cách xa, và hai khách ở màn 11–12. Dải lắc có ở mọi màn Mê trong file này. Màn hình chọn màn và màn 13–17 nằm ở `Huong-Dan-Man-13-16.md`. Đổi món chỉ có khi khách là Dokkaebi.

Script và tám asset `Level05` … `Level12` đã nằm trong project, đúng số ở các bảng dưới. **Bạn không tạo lại số liệu.** Việc còn lại: thêm object, gán ô, kéo asset vào `LevelRunner`, rồi chơi thử.

Luật vùng ăn lấy từ `Mo-Mam-Vi-Tri-Mon-An.md`. Bán kính logic `1` = `2.5` đơn vị Unity. Góc `0°` là bên phải, tăng ngược chiều kim đồng hồ. Lúc Play, script xếp khách và vùng ăn. Tọa độ bạn đặt trong scene chỉ để nhìn khi đang sửa.

**Điều kiện:** đã làm xong Bước 1–10 của `Huong-Dan-Hierarchy.md` (màn 1–4 chạy được).

---

## Tiến độ

- [ ] Bước 1 — Tạo `HoZone`, `MeZone`
- [ ] Bước 2 — Tạo `Ho`, `Me`
- [ ] Bước 3 — Tạo `IconsHo` trên Canvas
- [ ] Bước 4 — Gán ô trên `LevelRunner` và `LevelHud`
- [ ] Bước 5 — Đối chiếu Level05 … Level12
- [ ] Bước 6 — Chơi thử
- [ ] Bước 7 — Checklist cuối

---

## Bước 1. HoZone và MeZone

Cây sau màn 1–4, thêm bốn object con của `Game`. Không đặt chúng làm con `Tray`.

```
Game
├─ Tray
├─ GumihoZone
├─ HoZone      ← mới
├─ MeZone      ← mới
├─ Gumiho
├─ Ho          ← mới (Bước 2)
└─ Me          ← mới (Bước 2)
```

Hai vùng này là **hình chữ nhật**, không phải vòng tròn.

1. **Hierarchy** → chuột phải `Game` → 2D Object → Sprites → Square. Đặt tên `HoZone`.
2. Chọn `HoZone` → Add Component → `SkillZone`.
3. Chỉnh `HoZone`:

   | Ô | Giá trị |
   |---|---|
   | `SkillZone` → Fill | Kéo chính `HoZone` vào |
   | Sprite Renderer → Color | `#2F6B4F`, alpha khoảng `70` |
   | Sprite Renderer → Sorting Order | `2` |

4. Chọn sprite Square trong **Project**: PPU bằng chiều rộng ảnh, Pivot Center. Scale `1` là cạnh 1 đơn vị. Script ghi đè scale lúc Play.
5. Duplicate `HoZone` (Ctrl+D), đổi tên `MeZone`, Color `#1F4E79`, alpha khoảng `70`.

`GumihoZone` vẫn là hình tròn như màn 1–4. Không đổi sprite đó.

**Kiểm tra:** `HoZone` và `MeZone` đều có `SkillZone` với ô `Fill` trỏ chính nó.

---

## Bước 2. Ho và Me

1. Duplicate `Gumiho`, đổi tên `Ho`.
2. Sprite của `Ho`: chưa có `Sprites/NPCs/Ho.png` thì để Knob, Color `#2F6B4F`. Khi có PNG, kéo vào ô Sprite và trả Color về trắng.
3. Duplicate lần nữa, tên `Me`. Placeholder Knob màu `#1F4E79`, hoặc `Sprites/NPCs/Me.png` khi đã vẽ (Color trắng nếu dùng hình vẽ).
4. Cả hai:

   | Ô | Giá trị |
   |---|---|
   | Sorting Order | `8` |
   | Scale | `1` |
   | `GuestView` | Có sẵn vì duplicate từ `Gumiho` |
   | Sprite | Là đầu, PPU bằng chiều rộng ảnh |

5. `Ho` và `Me` có thể **tắt** (bỏ tick) sau khi gán sprite. Script bật đúng khách của từng màn.

Script xoay khách nhìn vào tâm mâm. Gumiho ở đáy vẫn đứng thẳng. Hổ bên phải quay mặt vào mâm. Mê lướt thì chạy theo mép.

**Kiểm tra:** `Ho` và `Me` là con của `Game`, không phải con `Tray`.

---

## Bước 3. Hàng icon thứ hai — IconsHo

1. Duplicate `Icons`, đổi tên `IconsHo`.
2. Giữ Anchor giữa dưới, Pos `0, 40`, Horizontal Layout Group giống `Icons`.
3. Để trống, không đặt icon con.

Màn một khách chỉ hiện `Icons`. Màn 11–12 script tách hai hàng: Gumiho bên trái, Hổ bên phải, icon thu nhỏ cho vừa.

---

## Bước 4. Gán ô

### 4.1 `Game` → `LevelRunner`

Các ô màn 1–4 giữ nguyên. Gán thêm:

| Ô | Kéo vào |
|---|---|
| Ho Zone | `HoZone` |
| Ho Guest | `Ho` |
| Me Zone | `MeZone` |
| Me Guest | `Me` |
| Levels | Đủ 17 asset. Phần tử 0 là Level01, phần tử 16 là Level17 |

Thiếu `Ho` hoặc `Me` thì màn 1–4 vẫn chạy. Vào màn cần khách đó, **Console** báo `LevelRunner chưa gán ...` và màn không đổi.

Mẹo kéo nhanh 17 asset: chọn `Game`, bấm khóa (lock) góc trên Inspector, rồi trong **Project** chọn `Level01` … `Level17` (Shift+click) và kéo cả cụm vào chữ `Levels`. Sắp xếp theo tên là đúng thứ tự.

### 4.2 `Canvas` → `LevelHud`

| Ô | Kéo vào |
|---|---|
| Guest Name | Text `GuestName` |
| Icon Row Ho | `IconsHo` |

`Guest Name` để trống thì chữ khách không đổi theo màn.

**Kiểm tra:** `LevelRunner` không còn ô khách nào `None` (trừ `Co`, `Dokkaebi` nếu chưa làm màn 15–17). `LevelHud` có `Icon Row Ho`.

---

## Bước 5. Đối chiếu Level asset

Vị trí: `Assets/Levels/Level05.asset` … `Level12.asset`. Mở từng asset đối chiếu bảng dưới.

**Mẹo đối chiếu:**

- Dropdown **Guest Mode** phải hiện `Ho`, `Me` hoặc `GumihoHo`, **không phải số**.
- Món thèm: `Want` có đúng tên khách.
- Món cấm: `Forbid` có đúng tên khách, `Want` size `0`.
- Brake Time để `0` thì game lấy `0.35`. Hearts mọi màn là `3`.

### Bảng tóm tắt thông số

| Màn | Guest Mode | Spin Speed | Charges | Cross At | Swap After Bite |
|---|---|---|---|---|---|
| 05 | Ho | 0 | 2 | — | — |
| 06 | Ho | 24 | 2 | — | — |
| 07 | Ho | 36 | 1 | — | — |
| 08 | Me | 32 | 1 | 0.55 | — |
| 09 | Me | 34 | 2 | 0.55 | — |
| 10 | Gumiho | 26 | 3 | — | 0 |
| 11 | GumihoHo | 26 | 3 | — | — |
| 12 | GumihoHo | 38 | 3 | — | — |

### Level05 — Hổ, đứng yên

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| goi_cuon | 270 | 0.58 | want |
| gimbap | 248 | 0.58 | forbid |
| bibimbap | 292 | 0.58 | forbid |

### Level06 — Hổ, xoay chậm, cả ba món bên trái

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| goi_cuon | 250 | 0.58 | want |
| banh_mi | 228 | 0.58 | forbid |
| gimbap | 206 | 0.58 | want |

### Level07 — Hổ, xoay

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| gimbap | 262 | 0.58 | want |
| goi_cuon | 278 | 0.58 | want |
| pho | 300 | 0.58 | forbid |
| ca_phe | 100 | 0.78 | forbid |

### Level08 — Mê, một nhát

| dishId | angleDeg | radius | với Mê |
|---|---|---|---|
| bibimbap | 270 | 0.70 | want |
| songpyeon | 90 | 0.70 | forbid |
| tteokbokki | 0 | 0.50 | forbid |

### Level09 — Mê, lướt rồi cắn nhát hai

| dishId | angleDeg | radius | với Mê |
|---|---|---|---|
| pho | 270 | 0.68 | want |
| ca_phe | 125 | 0.68 | want |
| banh_mi | 105 | 0.68 | forbid |
| gimbap | 200 | 0.50 | forbid |

### Level10 — Gumiho, bốn món cách xa

**Không điền** Want After Swap hay Forbid After Swap. Bốn món cùng vành `0.58`, cách nhau `90°`.

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| songpyeon | 250 | 0.58 | want |
| pho | 160 | 0.58 | forbid |
| gimbap | 70 | 0.58 | want |
| banh_mi | 340 | 0.58 | want |

### Level11 — Gumiho trái, Hổ phải

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| songpyeon | 175 | 0.70 | want | forbid |
| gimbap | 355 | 0.70 | forbid | want |
| goi_cuon | 15 | 0.55 | forbid | want |
| pho | 95 | 0.45 | forbid | forbid |

### Level12 — xung đột hai miệng

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| songpyeon | 150 | 0.70 | want | forbid |
| gimbap | 10 | 0.70 | want | want |
| banh_mi | 190 | 0.55 | forbid | forbid |
| goi_cuon | 330 | 0.55 | forbid | want |

Kéo đủ 17 asset vào ô `Levels`, đúng thứ tự. **Save scene** (Ctrl+S). Màn 13–17 đã có asset; số liệu ở `Huong-Dan-Man-13-16.md`.

---

## Cách điều khiển mới

Màn một khách: giữ chuột ở bất kỳ chỗ nào, giống màn 1–4. Vùng ăn đổi theo khách.

| Khách | Hình vùng ăn | Hành vi |
|---|---|---|
| Hổ | Khe dọc theo hướng khách | Giữ để khe rộng ra. Thả khi món nằm giữa hai mép khe. |
| Mê | Dải từ mép vào trong, bề rộng không đổi | Xem bên dưới |

**Mê chi tiết:**

- Chưa giữ: dải lắc ±`6°`, một vòng khoảng `1,25` giây. Khách ở đáy thì đỉnh lắc dương nằm về phía phải.
- Nhấn giữ: dải đóng băng đúng góc lúc bấm, mâm giảm tốc, dải dài thêm. Thả lúc dải đang đè món.
- Dải dài hơn `crossAt`: Mê chạy sang mép đối diện. Đúng ngưỡng thì đứng yên. Nhát sau tính từ mép mới.
- Thanh giữ hơn khoảng một phần ba là Mê lướt, với `crossAt = 0.55`.

**Màn 11–12 (hai khách):**

- Nửa trái màn hình là Gumiho, nửa phải là Hổ.
- Giữ bên nào thì chỉ vùng bên đó lớn lên, mâm vẫn phanh.
- Kéo chuột qua đường giữa trong lúc giữ thì nhát bị hủy: không trừ lượt, không cắn.
- Hai khách đứng xa vành hơn (`1.42`, world `±3.55` trên trục ngang, thay vì `1.18`) để đầu không đè món.
- Màn hình dọc thì camera lùi cho cả hai người nằm trọn trong khung.
- Dưới mâm: hàng trái ghi `Gumiho`, hàng phải ghi `Hổ`. Dòng giữa là `Trái · Gumiho` và `Phải · Hổ`.
- Vòng Gumiho ở màn hai khách ngắn hơn một chút so với màn một khách, để không chạm khe Hổ ở giữa mâm.

Màn 10 không đổi icon. Bốn món đứng xa nhau, mỗi nhát một món. Đổi món chỉ xảy ra ở màn có Dokkaebi.

---

## Bước 6. Cách chơi để thử

Số dưới đây là cửa sổ đã tính với bán kính vùng ăn trong code. Mâm xoay ngược chiều kim đồng hồ.

| Màn | Thử |
|---|---|
| 5 | Thả ngay. Gỏi cuốn đang nằm trong khe hẹp. Giữ lâu sẽ nuốt Gimbap hoặc Bibimbap hai bên. |
| 6 | Cả ba món đang ở bên trái Hổ. Chờ Gỏi cuốn xuống đúng khe, thả nhanh. Khoảng 2 giây sau Gimbap vào khe, thả nhanh lần nữa. Bánh mì nằm giữa hai món thèm: nới rộng một nhát sẽ dính. Không phải chờ hết một vòng. |
| 7 | Vừa vào màn, giữ hơi một chút rồi thả. Hai món thèm đang kẹp sát đường giữa. Giữ hết cỡ ngay lúc đó vẫn chưa tới Phở. Chỉ có 1 lượt: hẹp quá thì sót một món và thua. |
| 8 | Bibimbap đang ở dải dưới. Thả ngay là qua màn, còn 0 lượt vẫn tính là qua. Muốn thấy Mê chạy lên mép trên: nhấn giữ khi dải đang đè Bibimbap. Dải đứng yên, mâm chậm lại. Giữ hơn một phần ba thanh rồi thả. Chỉ có 1 lượt. |
| 9 | Phở đang ở đáy. Nhấn giữ khi dải đè Phở, giữ hơn một phần ba thanh rồi thả: ăn Phở, Mê sang mép trên, mâm đã phanh trong lúc giữ. Cà phê chưa nằm trên dải mới. Chờ gần một vòng, khoảng `θ` từ `317°` đến `333°`, Cà phê thẳng mép trên. Thả nhanh khi dải đè nó. Bánh mì ở `105°` vẫn ngoài dải trong cửa sổ đó. |
| 10 | 1) Chờ Songpyeon về đáy, thả nhanh. 2) Phở đi qua đáy, đừng thả. 3) Gimbap về đáy, thả nhanh. 4) Bánh mì về đáy, thả nhanh. Bốn món cách nhau 90°, giữ hết cũng không ăn được hai món trong một nhát. |
| 11 | Trái, thả nhanh: Songpyeon. Phải, thả nhanh: Gimbap. Phải, giữ hơi rộng hơn hoặc chờ Gỏi cuốn vào đúng khe, thả. Phở ở phía trên là bẫy nếu giữ khe hết cỡ lúc nó sang mép phải. |
| 12 | Đừng bấm trái lúc mở màn: Bánh mì đang nằm trong vòng nhỏ của Gumiho. 1) Ngay lập tức bấm nửa phải, giữ khoảng một phần ba thanh, Hổ ăn Gimbap. Giữ hết cỡ sẽ dính Gỏi cuốn. 2) Chờ một nhịp, Bánh mì ra khỏi vòng trái và Songpyeon vào giữa, thả nhanh bên trái. 3) Gỏi cuốn thẳng khe bên phải thì thả nhanh. Để Songpyeon chạy quá mép trái thì Gumiho không với tới; Hổ cắn món đó sẽ mất mạng. |

Màn 6 nhát hai tới sau khoảng 44° xoay. Cả ba món nằm bên trái đường tâm.

Qua màn 12, nút hiện `Từ đầu` và chơi lại từ màn 1.

---

## Bước 7. Checklist cuối

**Hierarchy**

- [ ] `HoZone` và `MeZone` là sprite vuông, Sorting Order `2`, ô `Fill` trỏ đúng object đó.
- [ ] `Ho` và `Me` có `GuestView`, không phải con `Tray`.

**Ô gán**

- [ ] `LevelRunner` đã gán Ho Zone, Ho Guest, Me Zone, Me Guest.
- [ ] `Levels` có 17 phần tử, Level01 ở vị trí 0, Level17 ở vị trí 16.
- [ ] `LevelHud` đã gán Guest Name và Icon Row Ho.

**Level asset**

- [ ] Level05: Guest Mode `Ho`, Gỏi cuốn Want = Ho.
- [ ] Level07: Charges `1`.
- [ ] Level08 và Level09: Cross At `0.55`.
- [ ] Level09: Cà phê góc `125`.
- [ ] Level10: Charges `3`, Swap After Bite `0`, không có món nào điền Want After Swap.
- [ ] Level11 và Level12: Guest Mode `GumihoHo`.

**Khi Play**

- [ ] Màn 5 thấy khe xanh, không thấy vòng son.
- [ ] Màn 8: dải chàm lắc khi chưa giữ; nhấn giữ thì dải đứng im và mâm chậm lại.
- [ ] Màn 11 thấy cả Gumiho bên trái và Hổ bên phải, hai hàng icon tách nhau.

---

## Sửa lỗi

| Triệu chứng | Nguyên nhân hay gặp | Cách sửa |
|---|---|---|
| Console: `LevelRunner chưa gán Ho Zone` / `Ho` / `Me Zone` / `Me` | Ô tương ứng trên `LevelRunner` còn `None` | Kéo `HoZone`, `Ho`, `MeZone`, `Me` vào |
| Vào màn 5 mà vẫn thấy vòng tròn son | `Levels` sai thứ tự, hoặc Guest Mode của asset là `Gumiho` | Đối chiếu Bước 5, kiểm tra thứ tự phần tử |
| Không có khe xanh | `HoZone` thiếu Sprite hoặc `Fill` trống | Thêm Square, gán `Fill` |
| Màn 11–12 chỉ thấy một hàng icon | `Icon Row Ho` trên `LevelHud` chưa gán | Gán `IconsHo` |
| Chữ khách không đổi theo màn | `Guest Name` trên `LevelHud` chưa gán | Gán Text `GuestName` |
| Dải Mê không lắc | Đang ở màn không phải Me, hoặc đang giữ chuột | Chọn màn 8 hoặc 9, chưa nhấn |
| Mê không chạy sang mép kia | Thanh giữ chưa vượt `crossAt` (`0.55`) | Giữ hơn một phần ba thanh |
| Hai khách bị cắt mép trên điện thoại dọc | Camera chưa lùi | Kiểm tra Camera là Orthographic, Size `6.1` |
