# Hướng dẫn xếp Hierarchy — màn 1–4

Script không tạo object trong scene. Bạn xếp Hierarchy và gán ô trong Inspector. Khi Play, script nhân bản hai prefab: món ăn (con của Tray) và icon món (con của hàng Icons). Qua màn, script tự tạo vòng sáng rồi xóa, không cần object vòng trong Hierarchy.

Scene chơi: `GameSrc/Assets/GameScene.unity`. Trong scene sẵn có Main Camera và Global Light 2D.

**Cách dùng file này:** làm lần lượt từ Bước 1 đến Bước 11, đánh dấu `[x]` ở bảng Tiến độ ngay bên dưới. Mỗi bước có phần **Kiểm tra** để biết đã đúng chưa. Gặp lỗi thì xuống mục **Sửa lỗi** cuối file, tra theo triệu chứng.

Các file liên quan:

| File | Nội dung |
|---|---|
| `Mo-Mam-Vi-Tri-Mon-An.md` | Luật vùng ăn và số liệu gốc |
| `Mo-Mam-Danh-Sach-Sprite.md` | Danh sách file hình cần vẽ |
| `Huong-Dan-Man-5-12.md` | Hổ, Mê, hai khách (màn 5–12) |
| `Huong-Dan-Man-13-16.md` | Màn hình chọn màn, cài đặt, màn 13–17 |

---

## Tiến độ

Chép bảng này vào ghi chú riêng nếu muốn tick, hoặc sửa thẳng trong file.

- [ ] Bước 1 — Camera và đèn
- [ ] Bước 2 — Cây Hierarchy (tạo object rỗng)
- [ ] Bước 3 — Tray
- [ ] Bước 4 — GumihoZone
- [ ] Bước 5 — Gumiho
- [ ] Bước 6 — Canvas, EventSystem, chữ, thanh giữ, hàng icon, bảng kết quả
- [ ] Bước 7 — Prefab Food
- [ ] Bước 8 — Prefab FoodIcon
- [ ] Bước 9 — Bốn Level asset
- [ ] Bước 10 — Gắn script: `LevelRunner` trên Game, `LevelHud` trên Canvas
- [ ] Bước 11 — Gắn sprite
- [ ] Chơi thử (mục 12)
- [ ] Checklist cuối (mục 13)

---

## Quy ước

| Ký hiệu | Nghĩa |
|---|---|
| **Hierarchy**, **Project**, **Inspector**, **Console** | Bốn cửa sổ của Unity |
| `Tên` in nghiêng mã | Tên object, tên file, hoặc tên ô trong Inspector, gõ đúng từng chữ |
| Position, Scale | World, trừ khi ghi Local |
| Bán kính logic `1` | = `2.5` đơn vị Unity |
| Góc `0°` | Bên phải. Tăng ngược chiều kim đồng hồ |
| PPU | Pixels Per Unit. Đặt bằng chiều rộng ảnh (ảnh 256px thì PPU = 256) để scale `1` = 1 đơn vị |

Bảng màu dùng xuyên suốt:

| Tên | Mã | Dùng cho |
|---|---|---|
| Giấy | `#F6F0E6` | Nền camera, chữ trên nút |
| Mực | `#2C2420` | Chữ, Notch, nền bảng kết quả |
| Son | `#8C2F39` | Vòng Gumiho, thanh Fill, nút Next, tên khách |
| Vàng đậm | `#8B6628` | Rim |
| Vàng mâm | `#C6A15A` | Face |
| Kem | `#EDDCAE` | Hub |

---

## Bước 1. Camera và đèn

**Hierarchy** → chọn `Main Camera`.

| Ô trong Inspector | Giá trị |
|---|---|
| Transform → Position | `0, 0, -10` |
| Camera → Projection | Orthographic |
| Camera → Size | `6.1` |
| Camera → Background | `#F6F0E6` |

**Hierarchy** → chọn `Global Light 2D`: giữ nguyên, Intensity `1`. Sprite trong scene cần đèn này để không bị đen.

**Kiểm tra:** Game view nền màu giấy kem, không còn màu xanh mặc định.

---

## Bước 2. Cây Hierarchy

Dựng đúng cây này trước, các bước sau gắn component vào từng object.

```
GameScene
├─ Main Camera
├─ Global Light 2D
├─ Game
│  ├─ Tray
│  │  ├─ Rim
│  │  ├─ Face
│  │  ├─ Hub
│  │  └─ Notch
│  ├─ GumihoZone
│  └─ Gumiho
├─ Canvas
│  ├─ Title
│  ├─ Subtitle
│  ├─ LevelText
│  ├─ HeartsText
│  ├─ ChargesText
│  ├─ GuestName
│  ├─ Hold
│  │  └─ Fill
│  ├─ Icons
│  └─ Result
│     ├─ ResultText
│     └─ Next
│        └─ ButtonText
└─ EventSystem
```

Cách tạo phần `Game`:

1. **Hierarchy** → chuột phải → Create Empty → đặt tên `Game`. Position `0, 0, 0`, Rotation `0`, Scale `1, 1, 1`.
2. Chuột phải `Game` → Create Empty → `Tray`. Position `0, 0, 0`.
3. Chuột phải `Game` → Create Empty → `GumihoZone`, và `Gumiho`. Hai object này cùng cấp với `Tray`.
4. Chuột phải `Tray` → 2D Object → Sprites → Circle (hoặc Knob, xem Bước 11) → đặt tên `Rim`, `Face`, `Hub`, `Notch`.

Tray, GumihoZone và Gumiho là con của Game, **không lồng vào nhau**. Vòng ăn và khách đứng yên. Chỉ Tray xoay. Không đặt `Gumiho` làm con của `Tray`.

Phần `Canvas` làm ở Bước 6.

**Kiểm tra:** cây trong Hierarchy khớp hình trên. `Rim`, `Face`, `Hub`, `Notch` thụt vào dưới `Tray`.

---

## Bước 3. Mâm — Tray

Object `Tray`: Position `0, 0, 0`. **Không gắn script.**

Mỗi phần dưới là một Sprite Renderer. Sprite là hình tròn, pivot ở giữa, PPU theo bảng Quy ước.

| Object | Local Position | Scale | Sorting Order | Color |
|---|---|---|---|---|
| `Rim` | `0, 0, 0` | `5.35, 5.35, 1` | 0 | `#8B6628` |
| `Face` | `0, 0, 0` | `5, 5, 1` | 1 | `#C6A15A` |
| `Hub` | `0, 0, 0` | `0.55, 0.55, 1` | 3 | `#EDDCAE` |
| `Notch` | `2.35, 0, 0` | `0.16, 0.34, 1` | 4 | `#2C2420` |

`Face` đường kính 5 là mép trong của mâm (`radius = 1`). `Notch` chỉ để thấy mâm đang xoay. Có thể đổi hình sau, giữ tâm Tray ở gốc tọa độ.

**Kiểm tra:** Scene view thấy mâm vàng tròn với viền đậm hơn và một vạch đen bên phải.

---

## Bước 4. Vòng của Gumiho — GumihoZone

Chọn `GumihoZone` → Add Component → `SkillZone`.

| Ô trong Inspector | Giá trị |
|---|---|
| Transform → Position | `0, -1.55, 0` |
| `SkillZone` → Fill | Kéo chính object `GumihoZone` vào, hoặc kéo con `Fill` nếu bạn tách sprite ra |
| Sprite Renderer → Color | `#8C2F39`, Alpha khoảng `70` |
| Sprite Renderer → Sorting Order | `2` (món order 6 vẽ đè lên vòng) |

Sprite vòng là hình tròn, PPU theo bảng Quy ước.

**Không chỉnh scale bằng tay.** Lúc Play, script đặt scale theo bán kính vùng ăn. Scale `1` phải là đường kính 1 đơn vị thì vòng mới đúng số.

**Kiểm tra:** ô `Fill` không còn `None`.

---

## Bước 5. Khách Gumiho

Chọn `Gumiho` → Add Component → `GuestView`.

| Ô trong Inspector | Giá trị |
|---|---|
| Transform → Position | `0, -2.95, 0` — tâm đầu, đáy màn hình. Bán kính `1.18` |
| Transform → Scale | `1, 1, 1` (PPU bằng chiều rộng ảnh thì đầu rộng khoảng 1 đơn vị) |
| Sprite Renderer → Sorting Order | `8` |

Hình là cái đầu, không có thân. Lúc Play, script đặt lại chỗ đứng:

| Số khách | Chỗ đứng |
|---|---|
| Một khách | `(0, -2.95)` |
| Hai khách | Gumiho ở `(-3.55, 0)`, Hổ ở `(3.55, 0)` |

Trục trên của ảnh hướng vào tâm mâm. Script chỉ lật scale X khi nhát hỏng. Không gật.

---

## Bước 6. Canvas

### 6.1 Tạo Canvas và EventSystem

**Hierarchy** → chuột phải → UI → Canvas. Unity tạo thêm `EventSystem`.

| Ô (Canvas) | Giá trị |
|---|---|
| Canvas → Render Mode | Screen Space - Overlay |
| Canvas Scaler → UI Scale Mode | Scale With Screen Size |
| Canvas Scaler → Reference Resolution | `1080` × `1920` |
| Canvas Scaler → Match | `0.5` |

**EventSystem.** Project dùng Input System, không dùng Input Manager cũ.

1. Chọn `EventSystem`. Xóa component `Standalone Input Module` nếu có.
2. Add Component → `Input System UI Input Module`.

### 6.2 Chữ

Chuột phải `Canvas` → UI → Text (Legacy), tạo sáu Text, đặt tên theo bảng. Pos Y tính từ anchor.

| Object | Anchor | Pos X, Y | Width × Height | Font Size | Chữ mẫu |
|---|---|---|---|---|---|
| `Title` | giữa trên | `0, -70` | `800 × 90` | 64 | Một Mâm |
| `Subtitle` | giữa trên | `0, -130` | `800 × 60` | 36 | 한상 |
| `LevelText` | giữa trên | `0, -190` | `400 × 60` | 42 | Màn 1 |
| `HeartsText` | trái trên | `180, -280` | `280 × 56` | 40 | Mạng 3 |
| `ChargesText` | phải trên | `-180, -280` | `280 × 56` | 40 | Lượt 2 |
| `GuestName` | giữa dưới | `0, 300` | `400 × 60` | 42 | Gumiho |

Áp dụng cho cả sáu:

| Ô | Giá trị |
|---|---|
| Alignment | Giữa. Riêng `HeartsText` căn trái, `ChargesText` căn phải |
| Horizontal Overflow | **Overflow** |
| Vertical Overflow | **Overflow** |
| Raycast Target | Tắt |
| Color | Mực `#2C2420`. Riêng `GuestName` màu son `#8C2F39` |
| Font | Font có tiếng Việt (LegacyRuntime hoặc font bạn nhúng) |

**Vì sao phải đặt Overflow:** Unity tạo Text với khung `160 × 30` và Overflow = Truncate. Cỡ chữ lớn hơn chiều cao khung thì cả dòng biến mất.

`Subtitle` (`한상`) cần font có chữ Hàn. Arial mặc định không có glyph này, nới khung vẫn thấy ô trống.

Script ghi đè `LevelText`, `HeartsText`, `ChargesText` khi chạy. `Title`, `Subtitle`, `GuestName` giữ nguyên.

### 6.3 Thanh giữ

1. Chuột phải `Canvas` → UI → Image, đặt tên `Hold`.

   | Ô | Giá trị |
   |---|---|
   | Anchor | giữa dưới |
   | Pos | `0, 250` |
   | Size | `640 × 22` |
   | Color | mực, alpha thấp |

2. Chuột phải `Hold` → UI → Image, đặt tên `Fill`.

   | Ô | Giá trị |
   |---|---|
   | Anchor | kéo dãn full cha: Min `0,0`, Max `1,1`, offset `0` |
   | Color | `#8C2F39` |
   | Image Type | `Filled` |
   | Fill Method | `Horizontal` |
   | Fill Origin | `Left` |
   | Fill Amount | `0` |

   Lưu ý: ô Fill Method chỉ hiện sau khi chọn Image Type = Filled, và cần Source Image khác `None` (dùng sprite UI mặc định) mới thấy Image Type.

### 6.4 Hàng icon

Chuột phải `Canvas` → Create Empty, đặt tên `Icons`. Dưới Canvas, Unity tự dùng RectTransform.

| Ô | Giá trị |
|---|---|
| Anchor | giữa dưới |
| Pivot | `0.5, 0` |
| Pos | `0, 40` |
| Size | `1000 × 180` |

Add Component → Horizontal Layout Group:

| Ô | Giá trị |
|---|---|
| Spacing | `18` |
| Child Alignment | Middle Center |
| Control Child Size | tắt cả Width và Height |
| Child Force Expand | tắt cả hai |

Để trống, **không đặt icon con**. Script đổ icon vào lúc Play.

### 6.5 Bảng kết quả

1. Chuột phải `Canvas` → UI → Image, tên `Result`. Anchor dãn full màn. Color `#2C2420`, alpha khoảng `140`.
2. Chuột phải `Result` → UI → Text (Legacy), tên `ResultText`.

   | Ô | Giá trị |
   |---|---|
   | Anchor | giữa |
   | Pos | `0, 80` |
   | Size | `800 × 100` |
   | Font Size | `72` |
   | Overflow | **Overflow** cả hai chiều |
   | Chữ mẫu | `Qua màn` |

   Khung `160 × 30` mặc định sẽ cắt mất dòng này và các chữ `Hết mạng`, `Hết lượt`.

3. Chuột phải `Result` → UI → Button, tên `Next`.

   | Ô | Giá trị |
   |---|---|
   | Anchor | giữa |
   | Pos | `0, -80` |
   | Size | `420 × 110` |
   | Image → Color | `#8C2F39` |

4. Con của `Next` (Unity tự tạo, đổi tên thành `ButtonText`):

   | Ô | Giá trị |
   |---|---|
   | Anchor | kéo dãn full nút: Min `0,0`, Max `1,1`, size `0 × 0` |
   | Font Size | `40` |
   | Alignment | giữa |
   | Color | giấy `#F6F0E6` |
   | Chữ mẫu | `Màn sau` |

   Khung này đã đủ cao, không bị cắt.

5. Chọn `Next` → Button → On Click ():
   1. Bấm `+`.
   2. Kéo object `Game` vào ô object.
   3. Dropdown chọn `LevelRunner → OnNextClicked`.

   Phải gắn `LevelRunner` lên `Game` (Bước 10) thì dropdown mới có `LevelRunner`.

6. **Tắt object `Result`** (bỏ tick cạnh tên ở Inspector). Script bật khi qua màn hoặc thua.

**Kiểm tra Bước 6:** Game view thấy `Một Mâm`, `한상`, `Màn 1`, `Mạng 3`, `Lượt 2` ở đúng chỗ, không bị cắt. Không thấy bảng kết quả.

---

## Bước 7. Prefab món — Food

Tạo trong `Assets/Prefabs`, không để bản này nằm trong scene.

```
Food
└─ Ring     (tùy chọn)
```

1. Tạo Empty hoặc Sprite, đặt tên `Food`. Sprite Renderer nằm trên chính `Food`.
2. Chỉnh `Food`:

   | Ô | Giá trị |
   |---|---|
   | Sprite Renderer → Sprite | Để trống, hoặc kéo một hình tròn tạm. Lúc Play, script đổi theo `dishId` từ bảng Dishes (Bước 10) |
   | Sprite Renderer → Color | trắng |
   | Sprite Renderer → Sorting Order | `6` (order `0` bị mặt mâm order `1` che) |
   | Transform → Scale | `0.4, 0.4, 1` |

   Chưa gán món nào ở Dishes thì món đó không hiện.

3. Con `Ring`: sprite tròn màu kem, scale lớn hơn thân một chút, Sorting Order `5`. **Tắt object này** (bỏ tick). Món thèm sẽ được script bật lại.
4. Chọn `Food` → Add Component → `FoodItem`:

   | Ô | Kéo vào |
   |---|---|
   | Body | Sprite Renderer của `Food` |
   | Want Ring | object `Ring`. Không có ring thì để trống |

5. Kéo `Food` từ Hierarchy vào `Assets/Prefabs` thành prefab, rồi **xóa object trong scene**.

`dishId`, Want, Forbid **không điền** trên prefab. Script điền từ Level asset lúc Play, và đặt local position trên Tray.

**Kiểm tra:** `Assets/Prefabs/Food.prefab` tồn tại, Hierarchy không còn `Food`.

---

## Bước 8. Prefab icon — FoodIcon

Tạo trong scene, kéo thành prefab, rồi xóa khỏi scene. **Không đặt prefab này vào `Icons`.**

```
FoodIcon          RectTransform 210 × 170
├─ Swatch         Image 72 × 72, anchor giữa trên
└─ Label          Text, anchor giữa dưới, cao khoảng 70
```

1. `Swatch`: Source Image là một sprite tròn tạm. Lúc Play, script thay bằng đúng hình món. Muốn viền món thèm thì Add Component → `Outline`:

   | Ô | Giá trị |
   |---|---|
   | Effect Color | `#8C2F39` |
   | Effect Distance | `4, -4` |

2. `Label`:

   | Ô | Giá trị |
   |---|---|
   | Size | `200 × 70` (cao 70 đủ cho cỡ chữ 28; rộng 200 để `Tteokbokki` không bị cắt) |
   | Font Size | `28` |
   | Alignment | giữa |
   | Overflow | **Overflow** cả hai chiều |
   | Raycast Target | Tắt |

3. Chọn gốc `FoodIcon` → Add Component → `FoodIcon`:

   | Ô | Kéo vào |
   |---|---|
   | Swatch | Image `Swatch` |
   | Label | Text `Label` |
   | Outline | Outline trên `Swatch`. Không dùng thì để trống |

4. Kéo thành prefab trong `Assets/Prefabs`, xóa khỏi scene.

---

## Bước 9. Bốn Level asset

**Project** → chuột phải → Create → Mot Mam → Level. Tạo bốn asset trong `Assets/Levels`: `Level01` … `Level04`.

> Trong project hiện đã có sẵn `Level01.asset` … `Level17.asset`. Nếu đã có, **mở ra đối chiếu với bảng dưới** thay vì tạo lại.

Ô chung của màn 1–3:

| Ô | Giá trị |
|---|---|
| Guest Mode | `Gumiho` |
| Charges | `2` |
| Hearts | `3` |

Màn 4 cùng Guest Mode và Hearts, nhưng Charges là `1` (một lượt buộc người chơi giữ để vòng ôm cả Songpyeon và Tteokbokki).

Brake Time để `0` thì game lấy `0.35`. Màn 4 ghi `0.35` cho khớp tài liệu.

Mỗi phần tử trong `Foods`:

- **Món thèm:** Want size `1`, phần tử là `Gumiho`. Forbid size `0`.
- **Món cấm:** Want size `0`. Forbid size `1`, phần tử là `Gumiho`.
- `dishId` viết đúng từng chữ, không dấu cách, khớp bảng Dishes ở Bước 10.

### Level01 — Id `1`, Spin Speed `0`, Brake Time `0`

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| songpyeon | 270 | 0.58 | want |
| banh_mi | 90 | 0.58 | forbid |
| bibimbap | 20 | 0.78 | forbid |

### Level02 — Id `2`, Spin Speed `0`, Brake Time `0`

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| tteokbokki | 300 | 0.58 | want |
| pho | 200 | 0.38 | forbid |
| goi_cuon | 200 | 0.78 | forbid |

### Level03 — Id `3`, Spin Speed `28`, Brake Time `0`

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| gimbap | 200 | 0.58 | want |
| ca_phe | 245 | 0.58 | forbid |
| banh_mi | 40 | 0.78 | forbid |

### Level04 — Id `4`, Spin Speed `40`, Brake Time `0.35`, Charges `1`

| dishId | angleDeg | radius | với Gumiho |
|---|---|---|---|
| songpyeon | 200 | 0.58 | want |
| tteokbokki | 250 | 0.58 | want |
| pho | 320 | 0.58 | forbid |
| goi_cuon | 110 | 0.78 | forbid |

**Kiểm tra:** mở từng asset, đếm số dòng Foods (3, 3, 3, 4), dòng nào cũng có `dishId`.

---

## Bước 10. Gắn script

### 10.1 Object `Game` — `LevelRunner`

Chọn `Game` → Add Component → `LevelRunner`.

Unity tự thêm `TableSpinner` và `BiteInput` (do `LevelRunner` yêu cầu). **Không chỉnh** Spin Speed, Brake Time, Paused. `LevelRunner` ghi các số đó từ Level asset. Nếu ô Tray của `TableSpinner` trống, `LevelRunner` tự lấy Tray đã gán bên dưới.

Gán các ô của `LevelRunner` (màn 1–4):

| Ô | Kéo vào |
|---|---|
| Levels | Level01 … Level04, đúng thứ tự (phần tử 0 là màn 1). Về sau mở rộng tới 17 |
| Tray | object `Tray` |
| Food Prefab | prefab `Food` |
| Zone | object `GumihoZone` |
| Guest | object `Gumiho` |
| Hud | object `Canvas` |
| Dishes | Size `8`, xem bảng dưới |

Các ô `Ho Zone`, `Ho Guest`, `Me Zone`, `Me Guest`, `Co ...`, `Dokkaebi ...` để trống ở bước này. Xem `Huong-Dan-Man-5-12.md` và `Huong-Dan-Man-13-16.md`.

**Bảng Dishes.** Mỗi dòng: `dishId` gõ đúng từng chữ, ô Sprite kéo PNG.

| dishId | File |
|---|---|
| banh_mi | `Sprites/Food/banh_mi.png` |
| goi_cuon | `Sprites/Food/goi_cuon.png` |
| pho | `Sprites/Food/pho.png` |
| ca_phe | `Sprites/Food/ca_phe.png` |
| gimbap | `Sprites/Food/gimbap.png` |
| songpyeon | `Sprites/Food/songpyeon.png` |
| tteokbokki | `Sprites/Food/tteokbokki.png` |
| bibimbap | `Sprites/Food/bibimbap.png` |

Dòng thiếu sprite thì món đó giữ hình trên prefab. Món thèm giữ màu trắng, món cấm tối đi. `dishId` lệch một chữ thì món đó không lấy được hình.

### 10.2 Object `Canvas` — `LevelHud`

Chọn `Canvas` → Add Component → `LevelHud`. Gán:

| Ô | Kéo vào |
|---|---|
| Level Text | `LevelText` |
| Hearts Text | `HeartsText` |
| Charges Text | `ChargesText` |
| Guest Name | `GuestName` |
| Hold Fill | `Fill` (Image Type đã là Filled) |
| Icon Row | `Icons` |
| Icon Prefab | prefab `FoodIcon` |
| Result Root | `Result` |
| Result Text | `ResultText` |
| Button Text | `ButtonText` |

`Icon Row Ho` để trống ở màn 1–4 (xem `Huong-Dan-Man-5-12.md`). `Guest Name` để trống thì chữ khách không đổi theo màn.

Thiếu ô nào, **Console** báo `LevelRunner chưa gán ...` và màn không chạy.

**Kiểm tra:** chọn `Game` và `Canvas`, không ô bắt buộc nào còn `None`.

---

## Bước 11. Gắn sprite

Danh sách file cần vẽ: `Mo-Mam-Danh-Sach-Sprite.md`. Mục này chỉ nói kéo file nào vào ô nào.

### 11.1 Import

1. Kéo PNG vào `Assets/Sprites/` (đúng thư mục trong danh sách sprite).
2. Chọn file trong **Project**. Trên **Inspector**:

   | Ô | Giá trị |
   |---|---|
   | Texture Type | Sprite (2D and UI) |
   | Sprite Mode | Single |
   | Pixels Per Unit | bằng chiều rộng ảnh (256px → `256`) |
   | Pivot | Center |
   | Filter Mode | Point nếu muốn nét vẽ cứng; Bilinear nếu muốn mềm |
   | Compression | None |

3. Bấm **Apply**.

**Sprite tròn tạm** (chưa có PNG): trong **Project** gõ `Knob`, dùng `Knob` của Unity. PPU của Knob không phải 256; scale ở các bước trên đã tính cho sprite đường kính 1 đơn vị. Nếu Knob bị to hoặc nhỏ, chỉnh scale object cho khớp bảng ở Bước 3–7, **đừng đổi PPU của Knob**.

### 11.2 Kéo vào ô Sprite

Chọn object → **Sprite Renderer** → ô **Sprite**: kéo file từ Project vào. Để trống thì object không hiện.

| Object | File | Color |
|---|---|---|
| `Rim` | `Sprites/Tray/rim.png`, hoặc Knob | `#8B6628` |
| `Face` | `Sprites/Tray/face.png`, hoặc Knob | `#C6A15A` |
| `Hub` | `Sprites/Tray/hub.png`, hoặc Knob | `#EDDCAE` |
| `Notch` | `Sprites/Tray/notch.png`, hoặc Knob | `#2C2420` |
| `GumihoZone` | `Sprites/Zones/gumiho_circle.png`, hoặc Knob | `#8C2F39`, alpha khoảng `70` |
| `Gumiho` | `Sprites/NPCs/Gumiho.png` | trắng `255, 255, 255, 255` |

`Gumiho` để màu trắng. Màu khác sẽ nhuộm cả hình hồ ly. Sorting Order và scale giữ như Bước 3–5.

`GumihoZone` trong scene nếu để trống Sprite thì vòng ăn không hiện. Kéo Knob vào, màu `#8C2F39`, alpha khoảng `70`. Không chỉnh scale: script đặt scale lúc Play.

### 11.3 UI không dùng Sprite Renderer

| Object | Ô | File |
|---|---|---|
| `FoodIcon` → `Swatch` | Image → Source Image | hình tròn tạm. Lúc Play script đổi thành sprite của món |
| `Hold`, `Fill`, `Result`, `Next` | Image → Source Image | sprite UI mặc định của Unity |

**Không kéo tám PNG món vào `Swatch`.** `Swatch` chỉ là chỗ trống. Hình thật nằm ở bảng Dishes (Bước 10.1). Prefab `Food` cũng chỉ có một bản, ô Sprite để trống.

### 11.4 Lưu

Ctrl+S để save scene, rồi Play.

---

## 12. Cách chơi để thử

Giữ chuột trái ở bất kỳ chỗ nào: mâm phanh, vòng lớn dần, thanh Fill đầy theo. Thả là một nhát.

| Màn | Thử |
|---|---|
| 1 | Thả ngay. Vòng nhỏ đã trùm Songpyeon. |
| 2 | Tteokbokki ở 300°, Phở và Gỏi cuốn ở 200°. |
| 3 | Chờ Gimbap về đáy mâm rồi thả nhanh, chưa giữ lâu. |
| 4 | Khi Songpyeon và Tteokbokki cùng ở đáy, giữ cho vòng ôm cả hai rồi thả. Chỉ có 1 lượt. Thả nhanh thì hụt hoặc sót một món. Giữ hết cỡ thì nuốt Phở. |

Qua màn hiện bảng `Qua màn`. Hết mạng hoặc hết lượt khi còn món thèm thì `Chơi lại`. Nút Next gọi `OnNextClicked`.

Lúc Play, script dựng tấm `Một Mâm` (màn hình chính) trên Canvas sẵn có. Không cần thêm nút trong Hierarchy. Nút `Màn` góc trên trái do script tạo. Chi tiết ở `Huong-Dan-Man-13-16.md`.

---

## 13. Checklist cuối

**Hierarchy**

- [ ] Camera Size `6.1`, nền giấy `#F6F0E6`.
- [ ] `Tray` ở gốc. `GumihoZone` ở `0, -1.55`. `Gumiho` ở `0, -2.95`. Hai object sau **không** là con của `Tray`.
- [ ] `Result` đang tắt.
- [ ] `EventSystem` dùng `Input System UI Input Module`.
- [ ] `Food` và `FoodIcon` **không** nằm trong scene.

**Sprite và thứ tự vẽ**

- [ ] Sprite tròn có PPU bằng chiều rộng ảnh (trừ Knob).
- [ ] Sorting Order: vòng `2`, món `6`, khách `8`.
- [ ] Sprite mâm, Gumiho, vòng ăn đã gắn theo Bước 11.

**Component và ô gán**

- [ ] `Fill` của thanh giữ là Image Type `Filled`.
- [ ] Nút `Next` trỏ `LevelRunner.OnNextClicked`.
- [ ] `Levels` có đủ asset, đúng thứ tự.
- [ ] Tám món nằm ở `Dishes` của `LevelRunner`, `dishId` đúng chữ.
- [ ] Console không báo `LevelRunner chưa gán ...`.

---

## Sửa lỗi

Tra theo cái bạn thấy trên màn hình hoặc trong **Console**.

| Triệu chứng | Nguyên nhân hay gặp | Cách sửa |
|---|---|---|
| Console: `LevelRunner chưa gán X` | Ô `X` trên `LevelRunner` còn `None` | Chọn `Game`, kéo object đúng vào ô. Màn 5+ cần thêm ô khách, xem `Huong-Dan-Man-5-12.md` |
| Sprite đen hoặc không thấy gì | Thiếu `Global Light 2D`, hoặc Sprite Renderer chưa có Sprite | Kiểm tra Light còn trong Hierarchy. Kéo sprite hoặc Knob vào |
| Một dòng chữ biến mất | Khung Text `160 × 30` + Overflow = Truncate | Tăng Width/Height, đặt Horizontal và Vertical Overflow = **Overflow** |
| `한상` hiện ô trống | Font không có glyph Hàn | Đổi sang font có chữ Hàn |
| Thanh giữ không đầy | `Fill` chưa là Image Type Filled, hoặc ô `Hold Fill` trên `LevelHud` chưa gán | Sửa Image Type, gán lại ô |
| Bấm Next không có gì | On Click chưa trỏ, hoặc trỏ nhầm object | Kéo `Game` vào, chọn `LevelRunner → OnNextClicked` |
| Bấm chuột không ăn | `EventSystem` còn `Standalone Input Module` | Xóa nó, thêm `Input System UI Input Module` |
| Dropdown On Click không có `LevelRunner` | Chưa gắn `LevelRunner` lên `Game` | Làm Bước 10.1 rồi chọn lại |
| Món không hiện | Prefab `Food` chưa có Sprite và dòng `Dishes` thiếu, hoặc Sorting Order thấp hơn mặt mâm | Gán dòng `Dishes`, đặt Sorting Order `6` |
| Món có hình sai hoặc giữ hình prefab | `dishId` ở Level asset lệch chữ so với `Dishes` | Gõ lại cho khớp từng chữ, không dấu cách |
| Vòng ăn không hiện | `GumihoZone` chưa có Sprite, hoặc `Fill` trống | Kéo Knob vào, kiểm tra ô `Fill` |
| Vòng sai kích thước | Đã chỉnh scale tay hoặc PPU sai | Để script đặt scale. Sprite phải có scale `1` = 1 đơn vị |
| Vòng đè lên món | Sorting Order sai | Vòng `2`, món `6` |
| Khách bị che hoặc đè món | Sorting Order sai, hoặc `Gumiho` là con của `Tray` | Khách order `8`. Đưa `Gumiho` ra làm con `Game` |
| Cả hồ ly nhuộm màu | Color trên Sprite Renderer của `Gumiho` không phải trắng | Đặt trắng |
| Tấm tối phủ cả màn ngay khi Play | Chưa tắt `Result` | Bỏ tick cạnh tên `Result` |
