# Một Mâm — Thiết kế màn

Hồ sơ từng màn: khách nào ngồi, mâm xoay thế nào, lời giải dự kiến, số lượt.

Luật thắng thua, icon và skill từng khách nằm ở `Mo-Mam-Tai-Lieu-Thiet-Ke.md`. Chỗ đứng của món, want/forbid và cửa sổ an toàn nằm ở `Mo-Mam-Vi-Tri-Mon-An.md` cùng file JSON cùng tên.

---

## 1. Cách xếp một màn

Xếp món thành cụm trước, gán thèm và chưa thèm sau. Mỗi màn phải chơi thử được bằng tay theo đúng số lượt. Nếu không còn cửa sổ an toàn, xếp lại cụm.

Số lượt trong bảng dưới đã tính sẵn một cách giải. Các màn giới thiệu khách và màn 2, 3, 12, 16 có thêm một lượt dự phòng. Các màn còn lại đúng số nhát của lời giải, để mỗi nhát đều đáng giá.

Màn 1–11 là xương sống bài nộp. Màn 12–20 là phần chơi thêm.

Hai luật gắn với khách, có từ màn đầu khách đó xuất hiện:

- Dải lắc là skill của Mê. Màn 8, 9 và 10 đều lắc. Không có màn nào để mở skill này.
- Đổi món là đặc trưng của Dokkaebi. Màn có Dokkaebi thì có đổi món. Màn không có Dokkaebi thì icon không đổi.

---

## 2. Màn hai khách

Dùng ở màn 18, 19 và 20. Gumiho ngồi trái, Hổ ngồi phải. Một mâm xoay chung.

- Nửa trái màn hình nạp vòng của Gumiho. Nửa phải nạp khe của Hổ.
- Một ngón tại một thời điểm. Giữ bên nào thì mâm phanh và vùng bên đó lớn dần.
- Hai vùng neo ở hai mép, chừa khoảng trống ở tâm khoảng 15% đường kính mâm. Hai vùng không chồng lên nhau.
- Món nằm trong vùng nào chỉ khách đó ăn được, và chỉ khi người chơi thả ngón cho khách đó.
- Qua màn khi mọi món đang được ít nhất một khách thèm đã bị đúng người ăn.
- Món Gumiho thèm và Hổ chưa thèm chỉ được cắn lúc nó nằm trong vùng của Gumiho.
- Món cả hai cùng thèm ăn bằng bên nào cũng được.
- Món cả hai chưa thèm là bẫy cho cả hai vùng.

Đường chia chỉ là ranh giới cảm ứng, không kẻ một vạch dày cắt ngang món ăn. Tay người chơi nằm trên hình khách ở mép, mắt nhìn mâm ở giữa.

Hai khách đứng xa vành hơn một khách, để người không đè món. Màn hình dọc thì camera lùi vừa đủ cho cả Gumiho và Hổ nằm trọn trong khung. Icon tách hai hàng: trái ghi Gumiho, phải ghi Hổ.

Màn 18 cho mâm đứng yên để làm quen hai vùng. Màn 19 dạy luật này khi từng món chỉ đi vào đúng vùng của người thèm nó. Màn 20 mới có món xung đột, phải cắn trước khi mâm đưa nó sang miệng người kia.

---

## 3. Hai mươi màn

| Màn | Khách | Mâm | Ý đồ | Nhát | Lượt |
|---|---|---|---|---|---|
| 1 | Gumiho | Đứng yên | Màn dạy Gumiho. Mâm đứng yên, vòng lớn dần khi giữ. Món cấm nằm sát món thèm để người chơi thấy phải thả đúng lúc. | 1 | 2 |
| 2 | Gumiho | Đứng yên | Mâm đứng yên, thêm một món cấm. Người chơi phải đọc vòng lớn dần trước khi thả. | 1 | 2 |
| 3 | Gumiho | Xoay 10°/s | Mâm xoay rất chậm. Có chưa tới ba món thèm nên không món nào bắt đầu ở vùng 250–360°: phải chờ mâm đưa tới. | 2 | 3 |
| 4 | Gumiho | Xoay 16°/s | Mâm xoay nhanh hơn, ba món cấm. Hai món thèm không ôm được trong một nhát, phải cắn hai lần. | 2 | 2 |
| 5 | Hổ | Đứng yên | Màn dạy Hổ. Mâm đứng yên, khe hẹp khi chạm và rộng dần khi giữ. | 1 | 2 |
| 6 | Hổ | Xoay 14°/s | Mâm xoay chậm. Món cấm đứng gần món thèm nên khe phải vừa đủ. | 2 | 2 |
| 7 | Hổ | Xoay 20°/s | Mâm xoay vừa, ba món cấm. Khe phải thu hẹp hoặc chờ mâm đưa món thèm vào đúng đường tâm. | 3 | 3 |
| 8 | Mê | Đứng yên | Màn dạy Mê. Mâm đứng yên, dải lắc nhẹ. Giữ quá nửa thanh thì Mê lướt sang mép đối diện. | 2 | 3 |
| 9 | Mê | Xoay 16°/s | Mâm xoay chậm, ba nhát. Mỗi nhát dài đổi mép đứng của Mê. | 3 | 3 |
| 10 | Mê | Xoay 22°/s | Bốn món thèm, hai món cấm. Phải tính trước nhát nào lướt, nhát nào đứng yên. | 4 | 4 |
| 11 | Gumiho | Xoay 38°/s | Gumiho quay lại, mâm xoay nhanh nhất từ đầu game. Bốn món thèm rải bốn phía, mỗi nhát một món. | 4 | 4 |
| 12 | Hổ | Xoay 40°/s | Hổ quay lại, mâm xoay nhanh. Bốn món thèm rải khắp mâm, khe phải đúng nhịp từng lần. | 4 | 5 |
| 13 | Cò | Đứng yên | Màn dạy Cò. Mâm đứng yên. Chấm là độ sâu, không phải bề rộng. | 2 | 3 |
| 14 | Cò | Xoay 24°/s | Cò ngồi lệch trái 12°, mâm xoay chậm. Bốn món thèm đi qua mỏ lần lượt. | 4 | 4 |
| 15 | Dokkaebi | Đứng yên | Màn dạy Dokkaebi. Mâm đứng yên, đốm lửa chạy tâm–mép. Icon đổi sau nhát đầu. | 4 | 5 |
| 16 | Dokkaebi | Xoay 20°/s | Mâm xoay chậm. Icon đổi sau nhát đầu: món cấm thành món thèm, món trung lập thành món cấm. | 4 | 5 |
| 17 | Dokkaebi | Xoay 26°/s | Mâm xoay nhanh hơn. Icon đổi sau nhát thứ hai, nên phải ăn đủ hai nhát trước khi đổi. | 4 | 4 |
| 18 | Gumiho trái, Hổ phải | Đứng yên | Màn dạy hai khách. Mâm đứng yên. Mỗi khách hai món thèm riêng, món của người này là món cấm của người kia. | 2 | 3 |
| 19 | Gumiho trái, Hổ phải | Xoay 22°/s | Mâm xoay chậm, bốn nhát luân phiên giữa hai khách. | 4 | 4 |
| 20 | Gumiho trái, Hổ phải | Xoay 32°/s | Mâm xoay nhanh nhất. Có món cả hai cùng thèm, và món chỉ một khách được ăn khi nó nằm đúng vùng. | 4 | 4 |

Tọa độ từng màn, lời giải và cửa sổ nhịp nằm ở `Mo-Mam-Vi-Tri-Mon-An.md` mục 3–4. Điểm độ khó nằm ở mục 3.1 của file đó.

Mỗi khách có một màn giới thiệu, mâm đứng yên và dư một lượt: Gumiho ở màn 1, Hổ ở màn 5, Mê ở màn 8, Cò ở màn 13, Dokkaebi ở màn 15, hai khách ở màn 18. Mạng là 3 ở màn giới thiệu, 2 ở các màn còn lại. Các màn 2, 3, 12 và 16 cũng chừa một lượt.

Luật xếp món chung cho cả 20 màn:

- Dưới 3 món thèm thì không món thèm nào bắt đầu ở góc 250°–360°. Món thèm đầu phải chờ mâm đưa tới.
- Món nằm rải khắp mâm, không để một góc trống quá 62°.
- Màn xoay đặt món thèm đầu cách vạch miệng khách ít nhất 30°.
- Đổi món chỉ có ở màn Dokkaebi. Màn 15 và 16 đổi sau nhát 1, màn 17 đổi sau nhát 2.
- Dải lắc có ở mọi màn Mê.
- Màn 14 dùng `anchorOffset = -12`: Cò ngồi ở 258°.
