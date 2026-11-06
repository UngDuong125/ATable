# Một Mâm — Thiết kế màn

Hồ sơ từng màn: khách nào ngồi, mâm xoay thế nào, lời giải dự kiến, số lượt.

Luật thắng thua, icon và skill từng khách nằm ở `Mo-Mam-Tai-Lieu-Thiet-Ke.md`. Chỗ đứng của món, want/forbid và cửa sổ an toàn nằm ở `Mo-Mam-Vi-Tri-Mon-An.md` cùng file JSON cùng tên.

---

## 1. Cách xếp một màn

Xếp món thành cụm trước, gán thèm và chưa thèm sau. Mỗi màn phải chơi thử được bằng tay theo đúng số lượt. Nếu không còn cửa sổ an toàn, xếp lại cụm.

Số lượt trong bảng dưới đã tính sẵn một cách giải. Màn 1–3 có thêm một lượt dự phòng. Màn 4 trở đi đúng số nhát của lời giải, để mỗi nhát đều đáng giá. Màn 13 và màn 16 vẫn chừa một lượt, vì nhát hụt ở màn đứng yên còn cứu được.

Màn 1–12 là xương sống bài nộp. Màn 13–17 là phần chơi thêm.

Hai luật gắn với khách, có từ màn đầu khách đó xuất hiện:

- Dải lắc là skill của Mê. Màn 8 và màn 9 đều lắc. Không có màn nào để mở skill này.
- Đổi món là đặc trưng của Dokkaebi. Màn có Dokkaebi thì có đổi món. Màn không có Dokkaebi thì icon không đổi.

---

## 2. Màn hai khách

Dùng ở màn 11 và 12. Gumiho ngồi trái, Hổ ngồi phải. Một mâm xoay chung.

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

Màn 11 dạy luật này khi từng món chỉ đi vào đúng vùng của người thèm nó. Màn 12 mới có món xung đột, phải cắn trước khi mâm đưa nó sang miệng người kia.

---

## 3. Mười bảy màn

| Màn | Khách | Mâm | Lời giải dự kiến | Lượt |
|---|---|---|---|---|
| 1 | Gumiho | Đứng yên | Vòng nhỏ sẵn đã phủ đúng 1 món thèm. Món cấm nằm ngoài tầm phóng tối đa. Người chơi chỉ cần thả. | 2 |
| 2 | Gumiho | Đứng yên | Món thèm nằm xa hơn bán kính lúc mới giữ. Món cấm nằm sát phía ngoài. Có một khoảng giữ an toàn. | 2 |
| 3 | Gumiho | Xoay chậm | Món thèm và món cấm cùng một vành, khác góc. Chờ món thèm vào vòng rồi thả nhanh. | 2 |
| 4 | Gumiho | Xoay | Giữ để phanh đúng lúc cụm vào vùng, vòng lớn vừa đủ ôm 2 món thèm. Món cấm đi sát sau cụm. Một lượt: thả nhanh không ôm nổi cả hai. | 1 |
| 5 | Hổ | Đứng yên | Khe hẹp sẵn đã chứa 1 món thèm. Món cấm nằm hai bên, ngoài khe. Thả ngay. | 2 |
| 6 | Hổ | Xoay chậm | Cả ba món nằm bên trái đường tâm. Hai món thèm vào khe liên tiếp, món cấm nằm giữa hai món đó. Nới rộng một nhát sẽ hỏng. | 2 |
| 7 | Hổ | Xoay | Cụm 2 món thèm sát nhau, món cấm ở rìa. Phanh và nới vừa khe trong đúng một nhát. | 1 |
| 8 | Mê | Xoay, phanh khi giữ | Một nhát dài: ăn món thèm ở dải gần và lướt sang mép kia. Dải mới lúc đáp không có món cấm. | 1 |
| 9 | Mê | Xoay | Nhát dài ăn món A và lướt. Món B chỉ đi vào dải phía bên kia sau khi đã lướt. Cắn nhát 2 kịp trước món cấm. Nhát ngắn ở lại phía cũ thì món B không bao giờ vào dải. | 2 |
| 10 | Gumiho | Xoay chậm | Bốn món cách nhau khoảng 90° trên cùng một vành. Vòng tối đa không ôm hai món. Mỗi nhát một món thèm. Món cấm đi qua miệng ở giữa, phải để nó đi. | 3 |
| 11 | Gumiho trái, Hổ phải | Xoay chậm | Mỗi món chỉ đi vào vùng của đúng người thèm. Dạy chia trái phải. | 3 |
| 12 | Gumiho trái, Hổ phải | Xoay | Một món Gumiho thèm, Hổ chưa thèm. Phải cắn bằng Gumiho trước khi món xoay sang vùng Hổ. Một món cả hai thèm nằm phía Hổ. | 3 |
| 13 | Hổ | Đứng yên | Cụm ba món nằm bên trái đường tâm. Nới khe vừa tới món thèm. Giữ gần hết thì dính món cấm kế bên. Món xa hơn nằm ngoài khe tối đa. | 2 |
| 14 | Gumiho trái, Hổ phải | Đứng yên | Mỗi bên một món thèm đã nằm trong vùng nhỏ. Bên trái, giữ quá tay thì dính món cấm gần tâm. Mâm không xoay. | 2 |
| 15 | Cò | Xoay chậm | Chấm chạy từ mép vào. Ba món cùng một tia. Chờ tia trước mỏ, thả đúng độ sâu. | 2 |
| 16 | Dokkaebi | Đứng yên | Đốm chạy tâm–mép. Khóa vành trong rồi quét sang món lệch phải. Sau nhát 1, khóa sát mép và thả ngay. | 3 |
| 17 | Dokkaebi | Đứng yên | Cùng đốm, hai độ sâu. Nhát trong rồi nhát ngoài. Icon đổi sau nhát thứ hai. Nhát ba khóa sát mép và thả ngay. | 4 |

Màn 4 có 1 lượt. Hai món thèm lệch nhau 50° trên vành `0.58`, vòng lúc mới chạm không chứa cả hai. Người chơi phải giữ.

Màn 6 xếp Gimbap, Bánh mì và Gỏi cuốn cùng phía trái đường tâm. Gỏi cuốn vào khe trước, Gimbap vào sau khoảng 44° xoay, tầm 2 giây. Không còn nhát nào phải chờ gần một vòng.

Màn 10 không đổi món. Bốn icon đứng cách nhau 90°, nên một nhát không ăn được hai món.

Màn 16 và màn 17 đều đổi icon, vì cả hai đều có Dokkaebi. Chưa giữ, đốm chạy trên tia từ tâm ra mép. Nhấn giữ thì độ sâu khóa, đốm quét ngang. Màn 16 đổi sau nhát đã tính thứ nhất, còn một lượt dự phòng. Màn 17 đổi sau nhát thứ hai. Lượt là 4 vì lời giải dùng 3 nhát và còn một lượt dự phòng.
