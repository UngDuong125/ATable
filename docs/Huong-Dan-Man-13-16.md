# Màn hình chọn màn, cài đặt, và các màn 8, 12–18

> **Cập nhật:** vị trí món, số lượt và lời giải của 20 màn đã làm lại, mỗi màn có 9–11 món. Số liệu hiện hành nằm ở `Mo-Mam-Vi-Tri-Mon-An.md` mục 3–4, file JSON cùng tên và `GameSrc/Assets/Levels/`. Các bảng tọa độ bên dưới chỉ còn là hướng dẫn cách dựng, không dùng để chép số.

Màn 1–7, 9–11, 19 và 20 giữ số liệu ở `Huong-Dan-Hierarchy.md` và `Huong-Dan-Man-5-12.md`. File này thêm màn hình lúc mở game, và tám màn: 8 và 12 đến 18. Dải lắc không mở ở đây: Mê đã lắc từ màn 8. Đổi món chỉ có ở màn Dokkaebi.

Cà phê ở màn 10 đã chuyển từ góc `75` sang `125`. Lời giải nhát hai nằm ở `Huong-Dan-Man-5-12.md`.

**Điều kiện:** đã làm xong `Huong-Dan-Hierarchy.md` và `Huong-Dan-Man-5-12.md`.

---

## Tiến độ

- [ ] Mục 1 — Xem màn hình `Một Mâm` chạy (không cần dựng gì)
- [ ] Mục 4 — Tạo `Co`, `CoZone`, `Dokkaebi`, `DokkaebiZone` (hoặc bỏ qua để script tự copy)
- [ ] Mục 5 — Đối chiếu Level08 và Level12 … Level18
- [ ] Mục 6 — Chơi thử và checklist

Mục 2 và 3 chỉ là mô tả hành vi, không có việc phải làm trong Unity.

---

## 1. Màn hình Một Mâm

Script dựng màn hình này lúc Play, trên Canvas đang có. **Không tạo object mới trong Hierarchy.**

Lúc mở game, màn 1 được xếp sẵn phía sau rồi tạm dừng. Tấm giấy giữa màn có ba nút:

| Nút | Việc |
|---|---|
| Chơi | Vào màn đang tạm dừng. Nếu màn đó đã thua hoặc đã qua, chơi lại màn đó. |
| Chọn màn | Lưới đủ các màn. Bấm số là vào đúng màn, không cần chơi từ màn 1. |
| Cài đặt | Âm thanh và tốc độ mâm. |

Góc trên trái, nút `Màn` mở lại tấm giấy này trong lúc chơi. Nhát đang giữ bị hủy. Bấm `Chơi` để tiếp tục. Nút `Màn` nằm cả trên bảng kết quả, nên từ `Qua màn` vẫn mở được lưới chọn màn.

Cài đặt ghi vào máy, lần Play sau còn:

| Mục | Mức |
|---|---|
| Âm thanh | Bật hoặc Tắt. Tắt thì `AudioListener` im. Chưa có file tiếng thì nút vẫn giữ trạng thái cho lúc gắn tiếng. |
| Tốc độ mâm | Chậm `0.8`, Vừa `1`, Nhanh `1.2`, nhân với `spinSpeed` của màn. Màn đứng yên vẫn đứng yên. |

**Mẹo test nhanh trong Editor:** nhảy tới màn cần xem bằng nút `Chọn màn`, không cần chơi từ màn 1.

---

## 2. Mê lắc

Dải lắc là skill của Mê, có ở màn 8, 9 và 10. Không chờ tới một màn sau mới bật.

| Hằng | Giá trị | Nghĩa |
|---|---|---|
| `meSwayIdle` | `6` độ | Biên độ khi chưa giữ |
| `meSwayMax` | `18` độ | Không dùng để nới lúc giữ |
| `meSwayHz` | `0.6` | Một vòng trái–phải–trái mất khoảng `1.67` giây, chỉ khi chưa giữ |

Góc dải khi đang lắc = neo khách + `6 * sin(thời gian)` độ. Dương nghĩa là lệch ngược chiều kim đồng hồ: khách ở đáy thì đỉnh dương nằm về phía phải màn hình.

Nhấn giữ: dải đóng băng đúng góc lúc bấm. Mâm giảm tốc và dừng sau `brakeTime`. Dải vẫn dài thêm. Thả thì dải giữ góc đó đến hết nhịp khóa, rồi lắc tiếp. Mê đang chạy sang mép kia thì dải không lắc thêm.

Màn 9: thả ngay vẫn ăn Bibimbap. Nhấn giữ lúc dải đang đè Bibimbap thì góc đứng yên, mâm phanh, giữ qua một phần tư thanh thì Mê lướt.

---

## 3. Hai khách dễ nhìn hơn

Gumiho vẫn ở mép trái (`180°`), Hổ vẫn ở mép phải (`0°`). Vùng ăn không đổi.

Chỗ đứng của hai đầu đẩy ra `guestDrawRadiusDual = 1.42` (một khách vẫn `1.18`), nên đầu không đè vành mâm. Màn hình dọc hẹp thì camera lùi vừa đủ để cả hai nằm trọn trong khung. Màn hình ngang không bị kéo xa thêm.

Dưới mâm có hai hàng icon, không chồng một hàng. Hàng trái có chữ `Gumiho` màu son, hàng phải có chữ `Hổ` màu lá. Dòng giữa ghi `Trái · Gumiho` và `Phải · Hổ`.

Nếu scene chưa gán `IconsHo`, script tự nhân hàng icon lúc Play.

---

## 4. Hai khách mới — Cò và Dokkaebi

Cử chỉ vẫn là giữ và thả. Chỉ hình vùng ăn đổi.

Hai khách này dựng giống `Ho` và `Me` ở `Huong-Dan-Man-5-12.md`. **Ô trống thì script tự copy khách và tô màu**, nên bước này có thể bỏ qua nếu chưa có sprite. Muốn kiểm soát hình thì làm theo các bước dưới.

### 4.1 Cò — mỏ chọc từ mép vào

Cò trắng. Màu gợi ý `#3E6B66`.

**Vùng ăn:** một chấm tròn bán kính `0.13`. Chưa giữ, tâm chấm ở `along = 0.82` trên tia đáy. Giữ hết, tâm chạy vào `along = 0.32`. Chấm không to ra. Thả đúng độ sâu thì cắn.

**Việc trong Unity:**

1. Duplicate `Gumiho` → đặt tên `Co`. Sprite: `Sprites/NPCs/Co.png`, Color trắng. Sorting Order `8`.
2. Duplicate `GumihoZone` → đặt tên `CoZone`. `CoZone` là sprite tròn, giống `GumihoZone`. Ô `Fill` trỏ chính nó.
3. Chọn `Game` → `LevelRunner`:

   | Ô | Kéo vào |
   |---|---|
   | Co Guest | `Co` |
   | Co Zone | `CoZone` |

### 4.2 Dokkaebi — đốm lửa trên tia tâm–mép

Dokkaebi trong truyền thuyết Hàn: yêu tinh có sừng, hay cầm chùy, và lửa ma `dokkaebi-bul` chạy dọc chỗ tối. Vẽ gốc, không lấy hình từ phim hay game. Màu lửa `#E24A2A`.

**Vùng ăn:** một đốm tròn bán kính `0.14`. Chưa giữ, tâm đốm chạy qua lại trên đường thẳng nối tâm mâm với mép, đúng tia trước mặt Dokkaebi. `along` đi từ `0` tới `1` rồi quay vào. Một vòng ra–vào mất `4` giây (`0.25` vòng mỗi giây).

**Nhấn giữ:** chiều dọc khóa đúng `along` lúc bấm. Giữ dưới `0.15` giây thì đốm đứng yên, coi như chạm. Quá `0.15` giây đốm mới quét ngược chiều kim đồng hồ, tối đa `42°` lúc giữ đủ `0.50` giây, và không to ra. Khách ở đáy thì quét về phía phải. Thả khi đốm đè đúng món. Hết nhịp khóa thì đốm chạy tiếp từ pha đã dừng.

**Đổi món** là đặc trưng của Dokkaebi. Màn 15, 16 và 17 đều có. Đốm không đổi. Sau mốc nhát, icon nhấp rồi món còn lại lấy `wantAfterSwap` / `forbidAfterSwap`. Màn 15 và màn 16 dùng `swapAfterBite = 1`. Màn 17 dùng `swapAfterBite = 2`.

**Việc trong Unity:**

1. Duplicate `Gumiho` → đặt tên `Dokkaebi`. Sprite: `Sprites/NPCs/Dokkaebi.png`, Color trắng. Sorting Order `8`.
2. Duplicate `GumihoZone` → đặt tên `DokkaebiZone`. Sprite tròn, giống `GumihoZone`. Ô `Fill` trỏ chính nó.
3. Chọn `Game` → `LevelRunner`:

   | Ô | Kéo vào |
   |---|---|
   | Dokkaebi Guest | `Dokkaebi` |
   | Dokkaebi Zone | `DokkaebiZone` |

**Kiểm tra:** `LevelRunner` không còn ô `Co ...` hay `Dokkaebi ...` nào `None`. Thiếu thì vào màn 13–17 **Console** báo `LevelRunner chưa gán ...`.

### 4.3 Gợi ý chưa làm

Hai khách này đủ khác Gumiho, Hổ và Mê. Chưa cắm vào game, để dành nếu muốn thêm màn:

| Khách | Vùng ăn | Câu đố |
|---|---|---|
| Rùa | Vòng đặt ở tâm, giữ để bán kính lớn ra. Ngược với Gumiho: Gumiho lan từ mép vào, Rùa lan từ núm ra. | Món gần núm, món cấm nằm vòng ngoài. |
| Quạt | Một cánh quạt từ khách, giữ để góc mở. Đã nhắc trong tài liệu thiết kế như tình huống vòng chung kết. | Mở vừa khít cụm món, quá tay thì ăn món kế. |

---

## 5. Năm màn thêm

Asset `Level08` và `Level12` … `Level18` đã nằm trong `Assets/Levels` và trong ô `Levels` của scene `GameScene`, đúng thứ tự số. Màn giới thiệu (8, 13, 15) dùng Hearts `3`, các màn còn lại `2`. Brake Time `0`.

### Bảng tóm tắt thông số

| Màn | Guest Mode | Spin Speed | Charges | Swap After Bite |
|---|---|---|---|---|
| 8 | Me | 0 | 3 | — |
| 12 | Ho | 0 | 2 | — |
| 13 | Co | 0 | 3 | — |
| 14 | Co | 28 | 2 | — |
| 15 | Dokkaebi | 0 | 3 | 1 |
| 16 | Dokkaebi | 0 | 3 | 1 |
| 17 | Dokkaebi | 0 | 4 | 2 |
| 18 | GumihoHo | 0 | 2 | — |

Mỗi màn dưới đây có bảng món, rồi **Cách qua** (lời giải để thử).

### Level08 — Mê, đứng yên, bắt buộc lướt

Màn giới thiệu Mê. Mâm không xoay. Hai món thèm nằm đối diện nhau: Bibimbap ở phía dưới, Songpyeon ở phía trên. Dải của Mê chỉ với tới món gần. Phở nằm sát tâm phía dưới là bẫy cho nhát giữ quá lâu.

| dishId | angleDeg | radius | với Mê |
|---|---|---|---|
| bibimbap | 270 | 0.70 | want |
| songpyeon | 90 | 0.70 | want |
| pho | 270 | 0.25 | forbid |

**Cách qua:**

1. Giữ hơn một phần tư thanh, khoảng `0.13` đến `0.33` giây, rồi thả. Ăn Bibimbap, Mê lướt lên mép trên.
2. Thả nhanh. Dải mới ở phía trên, ăn Songpyeon.

Thả quá sớm thì Mê ở lại phía dưới và Songpyeon ngoài tầm. Giữ hết thanh thì dải dài tới Phở và mất một mạng. Có `3` lượt, dư một lượt.

### Level12 — Hổ, cụm bên trái

Cả ba món nằm bên trái đường tâm. Gỏi cuốn gần đường tâm nhất. Gimbap xa hơn, bán kính nhỏ hơn. Bibimbap xa đến mức khe tối đa không với tới.

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| goi_cuon | 258 | 0.58 | want |
| gimbap | 240 | 0.52 | forbid |
| bibimbap | 222 | 0.40 | forbid |

**Cách qua:** giữ khoảng `0.12` đến `0.40` giây rồi thả. Ăn Gỏi cuốn. Thả ngay thì hụt, còn 1 lượt. Giữ gần hết thì dính Gimbap.

### Level13 — Cò, đứng yên, độ sâu

Màn giới thiệu Cò. Mâm không xoay. Hai món cùng một tia: Gỏi cuốn ở giữa, Bánh mì sát mép. Chấm lúc đầu đang đè Bánh mì.

| dishId | angleDeg | radius | với Cò |
|---|---|---|---|
| goi_cuon | 270 | 0.55 | want |
| banh_mi | 270 | 0.84 | forbid |

**Cách qua:** giữ khoảng `0.14` đến `0.40` giây rồi thả. Ăn Gỏi cuốn. Thả ngay thì dính Bánh mì, mất một mạng. Giữ hết thì chấm vào trong, hụt cả hai. Có `3` lượt.

### Level14 — Cò, độ sâu, mâm xoay chậm

Ba món lệch nhau `10°`, khác bán kính. Cò không ngồi đúng đáy: asset có `Anchor Offset = -12`, nên Cò lệch `12°` sang trái, mỏ chỉ về `258°`. Mâm xoay `28` độ/giây. Phở (`240°`) về trước mỏ đầu tiên, khoảng giây `0,64`. Gỏi cuốn (`230°`) về khoảng giây `1,0`. Bánh mì (`220°`) về khoảng giây `1,36`.

| dishId | angleDeg | radius | với Cò |
|---|---|---|---|
| goi_cuon | 230 | 0.55 | want |
| banh_mi | 220 | 0.82 | forbid |
| pho | 240 | 0.34 | forbid |

**Cách qua:** đừng thả ngay, chấm đang ở mép. Nhấn giữ khi Gỏi cuốn sắp về trước mỏ, khoảng giây `0,4` đến `1,3` kể từ lúc vào màn. Giữ khoảng `0.16` đến `0.35` giây rồi thả: chấm nằm trên Gỏi cuốn, hai món cấm ở ngoài. Mâm phanh trong lúc giữ nên Gỏi cuốn chỉ trôi vài độ. Giữ quá `0.36` giây thì chấm chọc tới Phở, giữ quá `0.39` giây thì hụt. Nếu để Gỏi cuốn đi qua, chờ vòng sau, khoảng `12,9` giây.

### Level15 — Dokkaebi, đứng yên, đốm và đổi món

Màn giới thiệu Dokkaebi. Mâm không xoay. Đốm chạy giữa tâm và mép. Chưa cần quét ngang: bấm là khóa chiều dọc, thả ngay là cắn. Sau nhát 1 icon đổi, Bánh mì ở mép thành món thèm.

| dishId | angleDeg | radius | trước | sau |
|---|---|---|---|---|
| gimbap | 270 | 0.55 | want | want |
| banh_mi | 270 | 0.84 | forbid | want |

**Cách qua:**

1. Chờ đốm về vành giữa, `along` khoảng `0.55`, bấm rồi thả ngay. Ăn Gimbap. Icon đổi.
2. Chờ đốm ra sát mép, `along` khoảng `0.84`, thả ngay. Ăn Bánh mì.

Bấm khi đốm ở mép lúc chưa đổi thì dính Bánh mì. Có `3` lượt, `swapAfterBite = 1`. Phần quét ngang nằm ở Level16.

### Level16 — Dokkaebi, khóa độ sâu rồi đổi sau nhát 1

Bánh mì nằm trên tia, sát mép. Gimbap và Bibimbap cùng vành trong `0.56`, lệch phải. Có Dokkaebi nên có đổi món.

| dishId | angleDeg | radius | trước | sau |
|---|---|---|---|---|
| banh_mi | 270 | 0.84 | forbid | want |
| gimbap | 288 | 0.56 | want | want |
| bibimbap | 312 | 0.56 | forbid | forbid |

**Cách qua:**

1. Chờ đốm về vành trong, `along` khoảng `0.56`, rồi nhấn giữ để khóa chiều dọc. Giữ khoảng `0.19` đến `0.38` giây, thả khi đốm quét tới Gimbap. Thả ngay ở độ sâu này thì hụt, và giữ dưới `0.15` giây đốm cũng chưa quét. Giữ quá `0.39` giây thì đốm tới Bibimbap. Icon đổi: Bánh mì thành thèm.
2. Chờ đốm ra sát mép, `along` khoảng `0.84`, thả ngay. Ăn Bánh mì. Giữ thêm thì đốm rời Bánh mì và không chạm Bibimbap, vì Bibimbap nằm vành trong.

Gimbap vẫn thèm sau khi đổi. Nhát đầu nếu hụt, còn đủ lượt để ăn Gimbap rồi Bánh mì.

### Level17 — Dokkaebi, hai độ sâu rồi đổi sau nhát 2

Cùng đốm màn 16. Gỏi cuốn nằm vành trong. Ba món kia nằm sát mép: Bánh mì trên tia, Cà phê và Bibimbap lệch phải.

| dishId | angleDeg | radius | trước đổi | sau đổi |
|---|---|---|---|---|
| goi_cuon | 290 | 0.48 | want | |
| ca_phe | 296 | 0.80 | want | forbid |
| banh_mi | 270 | 0.80 | forbid | want |
| bibimbap | 312 | 0.80 | forbid | forbid |

**Cách qua:**

1. Khóa `along` khoảng `0.48`. Giữ khoảng `0.18` đến `0.45` giây, thả khi đốm nằm trên Gỏi cuốn. Thả ngay thì hụt. Giữ hết thì đốm đã quét qua Gỏi cuốn.
2. Khóa `along` khoảng `0.80`. Giữ khoảng `0.29` đến `0.41` giây, thả khi đốm nằm trên Cà phê, chưa tới Bibimbap. Giữ dưới `0.23` giây thì đốm còn đè Bánh mì. Nhát này là nhát thứ hai, icon đổi.
3. Khóa lại `along` khoảng `0.80`, thả ngay. Ăn Bánh mì, món vừa thành thèm. Giữ vừa thì dính Cà phê đã thành cấm. Giữ hết thì dính Bibimbap.

### Level18 — hai khách, mâm đứng yên

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| songpyeon | 175 | 0.72 | want | forbid |
| banh_mi | 180 | 0.35 | forbid | forbid |
| gimbap | 0 | 0.70 | forbid | want |
| pho | 95 | 0.45 | forbid | forbid |

**Cách qua:**

1. Nửa trái, thả ngay. Ăn Songpyeon. Giữ thì vòng nuốt Bánh mì gần tâm.
2. Nửa phải, thả ngay. Ăn Gimbap. Phở ở phía trên, ngoài cả hai vùng.

---

## 6. Checklist cuối

**Ô gán và asset**

- [ ] `LevelRunner` đã gán `Co Guest`, `Co Zone`, `Dokkaebi Guest`, `Dokkaebi Zone` (hoặc chấp nhận script tự copy).
- [ ] `Levels` có 20 phần tử, `Level08` ở vị trí 7, `Level12` … `Level18` ở vị trí 11 … 17.
- [ ] Level08: Guest Mode `Me`, Spin Speed `0`, Charges `3`, Cross At `0.50`.
- [ ] Level13: Guest Mode `Co`, Spin Speed `0`, Charges `3`.
- [ ] Level15: Guest Mode `Dokkaebi`, Swap After Bite `1`, Charges `3`.
- [ ] Level16: Swap After Bite `1`. Level17: Swap After Bite `2`, Charges `4`.
- [ ] Level14: Guest Mode `Co`, Spin Speed `28`, Anchor Offset `-12`. Ba món góc `230`, `220`, `240`.
- [ ] Level16 và Level17: Guest Mode `Dokkaebi`, mọi món có Want/Forbid cả trước lẫn sau đổi như bảng.

**Khi Play — bấm `Chọn màn` rồi vào từng màn**

- [ ] Mở game thấy tấm `Một Mâm` trước, nút `Màn` ở góc trên trái khi đang chơi.
- [ ] Màn 8: mâm đứng yên. Giữ hơn một phần tư thanh rồi thả: ăn Bibimbap, Mê lướt lên mép trên. Thả nhanh ăn Songpyeon. Giữ hết ngay nhát đầu thì dính Phở.
- [ ] Màn 9: dải chàm lắc khi chưa giữ. Nhấn giữ thì dải đứng im và mâm chậm lại. Thả ngay vẫn ăn Bibimbap.
- [ ] Màn 12: khe xanh, cụm món bên trái. Giữ vừa thì ăn Gỏi cuốn. Giữ gần hết thì dính Gimbap.
- [ ] Màn 13: chấm đang đè Bánh mì. Thả ngay thì dính Bánh mì. Giữ vừa thì ăn Gỏi cuốn.
- [ ] Màn 14: Cò lệch trái `12°`, mâm xoay chậm, chấm tròn chạy vào tâm. Nhấn giữ lúc Gỏi cuốn gần về trước mỏ, giữ vừa thì ăn Gỏi cuốn.
- [ ] Màn 15: đốm đè Gimbap thì bấm và thả ngay. Icon đổi. Đốm ra mép thì thả ngay ăn Bánh mì.
- [ ] Màn 16: chưa giữ, đốm chạy tâm–mép. Khóa vành trong rồi giữ vừa thì ăn Gimbap. Khóa sát mép rồi thả ngay thì dính Bánh mì. Icon đổi sau nhát đầu. Nhát sau, khóa sát mép và thả ngay ăn Bánh mì vừa thành thèm.
- [ ] Màn 17: cùng đốm, hai độ sâu, icon đổi sau nhát thứ hai. Khóa vành trong ăn Gỏi cuốn, khóa sát mép ăn Cà phê, nhát ba khóa sát mép và thả ngay ăn Bánh mì.
- [ ] Màn 18: thấy cả Gumiho và Hổ, mâm đứng yên. Trái thả ngay ăn Songpyeon. Phải thả ngay ăn Gimbap.
- [ ] Màn 19 hoặc 20: thấy cả Gumiho và Hổ, hai hàng icon có chữ `Gumiho` và `Hổ`. Trên điện thoại dọc, hai người không bị cắt mép.
- [ ] Cài đặt Tốc độ `Chậm` rồi vào màn 3: mâm xoay chậm hơn lúc `Vừa`.

---

## Sửa lỗi

| Triệu chứng | Nguyên nhân hay gặp | Cách sửa |
|---|---|---|
| Vào màn 13 đến 17 thì Console báo lỗi, màn không chạy | Thiếu `Co ...` hoặc `Dokkaebi ...` trên `LevelRunner` | Gán 4 ô ở mục 4 |
| Đốm Dokkaebi không đổi icon | `Swap After Bite` của asset là `0`, hoặc `wantAfterSwap` / `forbidAfterSwap` trống | Đặt `1` (màn 16) hoặc `2` (màn 17), điền cột "sau" như bảng |
| Khách mới nhuộm màu lạ | Ô trống nên script copy rồi tô màu | Tạo `Co` / `Dokkaebi` thật với sprite trắng |
| Cài đặt tốc độ không đổi | Màn đang `Spin Speed 0` | Thử ở màn 3 trở đi |
| Tắt âm không có tiếng để nghe | Chưa có file tiếng | Bình thường: nút chỉ giữ trạng thái |
