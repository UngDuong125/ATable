# Màn hình chọn màn, cài đặt, và màn 13–17

Màn 1–12 giữ số liệu ở `Huong-Dan-Hierarchy.md` và `Huong-Dan-Man-5-12.md`. File này thêm màn hình lúc mở game, và năm màn sau màn 12. Dải lắc không mở ở đây: Mê đã lắc từ màn 8. Đổi món chỉ có ở màn Dokkaebi.

Cà phê ở màn 9 đã chuyển từ góc `75` sang `125`. Lời giải nhát hai nằm ở `Huong-Dan-Man-5-12.md`.

---

## 1. Màn hình Một Mâm

Script dựng màn hình này lúc Play, trên Canvas đang có. Không tạo object mới trong Hierarchy.

Lúc mở game, màn 1 được xếp sẵn phía sau rồi tạm dừng. Tấm giấy giữa màn có ba nút:

| Nút | Việc |
|---|---|
| Chơi | Vào màn đang tạm dừng. Nếu màn đó đã thua hoặc đã qua, chơi lại màn đó. |
| Chọn màn | Lưới đủ các màn. Bấm số là vào đúng màn, không cần chơi từ màn 1. |
| Cài đặt | Âm thanh và tốc độ mâm. |

Góc trên trái, nút `Màn` mở lại tấm giấy này trong lúc chơi. Nhát đang giữ bị hủy. Bấm `Chơi` để tiếp tục.

Cài đặt ghi vào máy, lần Play sau còn:

| Mục | Mức |
|---|---|
| Âm thanh | Bật hoặc Tắt. Tắt thì `AudioListener` im. Chưa có file tiếng thì nút vẫn giữ trạng thái cho lúc gắn tiếng. |
| Tốc độ mâm | Chậm `0.8`, Vừa `1`, Nhanh `1.2`, nhân với `spinSpeed` của màn. Màn đứng yên vẫn đứng yên. |

Nút `Màn` nằm trên bảng kết quả, nên từ `Qua màn` vẫn mở được lưới chọn màn.

---

## 2. Mê lắc

Dải lắc là skill của Mê, có ở màn 8 và màn 9. Không chờ tới một màn sau mới bật.

| Hằng | Giá trị | Nghĩa |
|---|---|---|
| `meSwayIdle` | `6` độ | Biên độ khi chưa giữ |
| `meSwayMax` | `18` độ | Không dùng để nới lúc giữ |
| `meSwayHz` | `0.8` | Một vòng trái–phải–trái mất `1.25` giây, chỉ khi chưa giữ |

Góc dải khi đang lắc = neo khách + `6 * sin(thời gian)` độ. Dương nghĩa là lệch ngược chiều kim đồng hồ: khách ở đáy thì đỉnh dương nằm về phía phải màn hình.

Nhấn giữ: dải đóng băng đúng góc lúc bấm. Mâm giảm tốc và dừng sau `brakeTime`. Dải vẫn dài thêm. Thả thì dải giữ góc đó đến hết nhịp khóa, rồi lắc tiếp. Mê đang chạy sang mép kia thì dải không lắc thêm.

Màn 8: thả ngay vẫn ăn Bibimbap. Nhấn giữ lúc dải đang đè Bibimbap thì góc đứng yên, mâm phanh, giữ qua một phần ba thanh thì Mê lướt.

---

## 3. Hai khách dễ nhìn hơn

Gumiho vẫn ở mép trái (`180°`), Hổ vẫn ở mép phải (`0°`). Vùng ăn không đổi.

Chỗ đứng của hai đầu đẩy ra `guestDrawRadiusDual = 1.42` (một khách vẫn `1.18`), nên đầu không đè vành mâm. Màn hình dọc hẹp thì camera lùi vừa đủ để cả hai nằm trọn trong khung. Màn hình ngang không bị kéo xa thêm.

Dưới mâm có hai hàng icon, không chồng một hàng. Hàng trái có chữ `Gumiho` màu son, hàng phải có chữ `Hổ` màu lá. Dòng giữa ghi `Trái · Gumiho` và `Phải · Hổ`.

Nếu scene chưa gán `IconsHo`, script tự nhân hàng icon lúc Play.

---

## 4. Hai khách mới

Cử chỉ vẫn là giữ và thả. Chỉ hình vùng ăn đổi.

### Cò — mỏ chọc từ mép vào

Cò trắng. Màu gợi ý `#3E6B66`.

Vùng ăn là một chấm tròn bán kính `0.13`. Chưa giữ, tâm chấm ở `along = 0.82` trên tia đáy. Giữ hết, tâm chạy vào `along = 0.32`. Chấm không to ra. Thả đúng độ sâu thì cắn.

`Sprites/NPCs/Co.png` gán vào object `Co`, màu trắng. Ô trên `LevelRunner` là `Co Guest` và `Co Zone`. `CoZone` là sprite tròn, giống `GumihoZone`. Ô trống thì script mới copy khách và tô màu.

### Dokkaebi — đốm lửa trên tia tâm–mép

Dokkaebi trong truyền thuyết Hàn: yêu tinh có sừng, hay cầm chùy, và lửa ma `dokkaebi-bul` chạy dọc chỗ tối. Vẽ gốc, không lấy hình từ phim hay game. Màu lửa `#E24A2A`.

Vùng ăn là một đốm tròn bán kính `0.14`. Chưa giữ, tâm đốm chạy qua lại trên đường thẳng nối tâm mâm với mép, đúng tia trước mặt Dokkaebi. `along` đi từ `0` tới `1` rồi quay vào. Một vòng ra–vào mất `4` giây (`0.25` vòng mỗi giây).

Nhấn giữ: chiều dọc khóa đúng `along` lúc bấm. Đốm quét ngược chiều kim đồng hồ, tối đa `42°`, và không to ra. Khách ở đáy thì quét về phía phải. Thả khi đốm đè đúng món. Hết nhịp khóa thì đốm chạy tiếp từ pha đã dừng.

Đổi món là đặc trưng của Dokkaebi. Màn 16 và màn 17 đều có. Đốm không đổi. Sau mốc nhát, icon nhấp rồi món còn lại lấy `wantAfterSwap` / `forbidAfterSwap`. Màn 16 dùng `swapAfterBite = 1`. Màn 17 dùng `swapAfterBite = 2`.

`Sprites/NPCs/Dokkaebi.png` gán vào object `Dokkaebi`, màu trắng. Ô trên `LevelRunner` là `Dokkaebi Guest` và `Dokkaebi Zone`. `DokkaebiZone` là sprite tròn, giống `GumihoZone`. Ô trống thì script mới copy khách và tô màu lửa.

### Gợi ý chưa làm

Hai khách này đủ khác Gumiho, Hổ và Mê. Chưa cắm vào game, để dành nếu muốn thêm màn:

| Khách | Vùng ăn | Câu đố |
|---|---|---|
| Rùa | Vòng đặt ở tâm, giữ để bán kính lớn ra. Ngược với Gumiho: Gumiho lan từ mép vào, Rùa lan từ núm ra. | Món gần núm, món cấm nằm vòng ngoài. |
| Quạt | Một cánh quạt từ khách, giữ để góc mở. Đã nhắc trong tài liệu thiết kế như tình huống vòng chung kết. | Mở vừa khít cụm món, quá tay thì ăn món kế. |

---

## 5. Năm màn thêm

Asset `Level13` … `Level17` đã nằm trong `Assets/Levels` và trong ô Levels của scene `GameScene`, sau Level12, đúng số ở bảng dưới. Hearts `3`. Brake Time `0`. Màn 13 là Hổ. Màn 14 là hai khách. Màn 16 có Swap After Bite `1`.

### Level13 — Hổ, cụm bên trái

Guest Mode `Ho`. Spin Speed `0`. Charges `2`.

Cả ba món nằm bên trái đường tâm. Gỏi cuốn gần đường tâm nhất. Gimbap xa hơn, bán kính nhỏ hơn. Bibimbap xa đến mức khe tối đa không với tới.

| dishId | angleDeg | radius | với Hổ |
|---|---|---|---|
| goi_cuon | 258 | 0.58 | want |
| gimbap | 240 | 0.52 | forbid |
| bibimbap | 222 | 0.40 | forbid |

**Cách qua:** giữ khoảng `0.12` đến `0.40` giây rồi thả. Ăn Gỏi cuốn. Thả ngay thì hụt, còn 1 lượt. Giữ gần hết thì dính Gimbap.

### Level14 — hai khách, mâm đứng yên

Guest Mode `GumihoHo`. Spin Speed `0`. Charges `2`.

| dishId | angleDeg | radius | Gumiho | Hổ |
|---|---|---|---|---|
| songpyeon | 175 | 0.72 | want | forbid |
| banh_mi | 180 | 0.35 | forbid | forbid |
| gimbap | 0 | 0.70 | forbid | want |
| pho | 95 | 0.45 | forbid | forbid |

**Cách qua:**

1. Nửa trái, thả ngay. Ăn Songpyeon. Giữ thì vòng nuốt Bánh mì gần tâm.
2. Nửa phải, thả ngay. Ăn Gimbap. Phở ở phía trên, ngoài cả hai vùng.

### Level15 — Cò, độ sâu, mâm xoay chậm

Guest Mode `Co`. Spin Speed `28`. Charges `2`.

Ba món cùng một tia, khác bán kính. Lúc mở màn, tia đó đang trước mỏ. Mâm xoay `28` độ/giây, nên tia sẽ đi khỏi mỏ nếu chưa giữ.

| dishId | angleDeg | radius | với Cò |
|---|---|---|---|
| goi_cuon | 270 | 0.55 | want |
| banh_mi | 270 | 0.82 | forbid |
| pho | 270 | 0.34 | forbid |

**Cách qua:** đừng thả ngay, chấm đang đè Bánh mì ở mép. Giữ khoảng `0.16` đến `0.34` giây rồi thả, khi tia còn trước mặt: chấm nằm trên Gỏi cuốn, hai món cấm ở ngoài. Mâm phanh trong lúc giữ nên tia chỉ trôi vài độ. Giữ gần hết thì chấm chọc tới Phở. Nếu để tia đi qua, chờ nó quay lại trước mỏ.

### Level16 — Dokkaebi, khóa độ sâu rồi đổi sau nhát 1

Guest Mode `Dokkaebi`. Spin Speed `0`. Charges `3`. Swap After Bite `1`.

Bánh mì nằm trên tia, sát mép. Gimbap và Bibimbap cùng vành trong `0.56`, lệch phải. Có Dokkaebi nên có đổi món.

| dishId | angleDeg | radius | trước | sau |
|---|---|---|---|---|
| banh_mi | 270 | 0.84 | forbid | want |
| gimbap | 288 | 0.56 | want | want |
| bibimbap | 312 | 0.56 | forbid | forbid |

**Cách qua:**

1. Chờ đốm về vành trong, `along` khoảng `0.56`, rồi nhấn giữ để khóa chiều dọc. Giữ khoảng `0.08` đến `0.30` giây, thả khi đốm quét tới Gimbap. Thả ngay ở độ sâu này thì hụt. Giữ hết thì đốm tới Bibimbap. Icon đổi: Bánh mì thành thèm.
2. Chờ đốm ra sát mép, `along` khoảng `0.84`, thả ngay. Ăn Bánh mì. Giữ thêm thì đốm rời Bánh mì và không chạm Bibimbap, vì Bibimbap nằm vành trong.

Gimbap vẫn thèm sau khi đổi. Nhát đầu nếu hụt, còn đủ lượt để ăn Gimbap rồi Bánh mì.

### Level17 — Dokkaebi, hai độ sâu rồi đổi sau nhát 2

Guest Mode `Dokkaebi`. Spin Speed `0`. Charges `4`. Swap After Bite `2`.

Cùng đốm màn 16. Gỏi cuốn nằm vành trong. Ba món kia nằm sát mép: Bánh mì trên tia, Cà phê và Bibimbap lệch phải.

| dishId | angleDeg | radius | trước đổi | sau đổi |
|---|---|---|---|---|
| goi_cuon | 290 | 0.48 | want | |
| ca_phe | 296 | 0.80 | want | forbid |
| banh_mi | 270 | 0.80 | forbid | want |
| bibimbap | 312 | 0.80 | forbid | forbid |

**Cách qua:**

1. Khóa `along` khoảng `0.48`. Giữ khoảng `0.10` đến `0.40` giây, thả khi đốm nằm trên Gỏi cuốn. Thả ngay thì hụt. Giữ hết thì đốm đã quét qua Gỏi cuốn.
2. Khóa `along` khoảng `0.80`. Giữ khoảng `0.22` đến `0.36` giây, thả khi đốm nằm trên Cà phê, chưa tới Bibimbap. Nhát này là nhát thứ hai, icon đổi.
3. Khóa lại `along` khoảng `0.80`, thả ngay. Ăn Bánh mì, món vừa thành thèm. Giữ vừa thì dính Cà phê đã thành cấm. Giữ hết thì dính Bibimbap.

---

## 6. Checklist

- Play thấy tấm `Một Mâm` trước, nút `Màn` ở góc trên trái khi đang chơi.
- Chọn màn 8: dải chàm lắc khi chưa giữ. Nhấn giữ thì dải đứng im và mâm chậm lại. Thả ngay vẫn ăn Bibimbap.
- Chọn màn 13: khe xanh, cụm món bên trái. Giữ vừa thì ăn Gỏi cuốn. Giữ gần hết thì dính Gimbap.
- Màn 14: thấy cả Gumiho và Hổ, mâm đứng yên. Trái thả ngay ăn Songpyeon. Phải thả ngay ăn Gimbap.
- Chọn màn 11 hoặc 12: thấy cả Gumiho và Hổ, hai hàng icon có chữ `Gumiho` và `Hổ`. Trên điện thoại dọc, hai người không bị cắt mép.
- Màn 15: mâm xoay chậm, chấm tròn chạy vào tâm. Giữ vừa khi tia món còn trước mỏ thì ăn Gỏi cuốn.
- Màn 16: chưa giữ, đốm chạy tâm–mép. Khóa vành trong rồi giữ vừa thì ăn Gimbap. Khóa sát mép rồi thả ngay thì dính Bánh mì. Icon đổi sau nhát đầu. Nhát sau, khóa sát mép và thả ngay ăn Bánh mì vừa thành thèm.
- Màn 17: cùng đốm, hai độ sâu, icon đổi sau nhát thứ hai. Khóa vành trong ăn Gỏi cuốn, khóa sát mép ăn Cà phê, nhát ba khóa sát mép và thả ngay ăn Bánh mì.
- Cài đặt Tốc độ `Chậm` rồi vào màn 3: mâm xoay chậm hơn lúc `Vừa`.
