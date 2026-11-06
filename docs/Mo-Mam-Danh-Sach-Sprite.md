# Một Mâm — Danh sách sprite cần thiết

Checklist hình để vẽ / import vào `GameSrc/Assets/Sprites/`.  
Tên file khớp `dishId` trong `Mo-Mam-Vi-Tri-Mon-An.json` và `Art.cs`.

**Quy ước chung**

- Tự vẽ. Không lấy từ phim, webtoon, game, stock còn bản quyền.
- PNG, nền trong suốt, viền rõ, phong cách phẳng Đông Hồ + minhwa.
- Import Unity: Texture Type = Sprite (2D and UI), Pivot = Center.
- PPU = đúng chiều rộng ảnh (ảnh 256px → PPU 256) để scale `1` = đường kính 1 đơn vị world.
- Kéo từng file món vào bảng Dishes của `LevelRunner`: `Huong-Dan-Hierarchy.md` mục 9 và mục 13. Mỗi `dishId` một sprite.
- Màu nền giấy tham chiếu: `#F6F0E6`. Mực: `#2C2420`.

**Đã có trong project**

| File | Trạng thái |
|---|---|
| `Sprites/NPCs/Gumiho.png`, `Ho.png`, `Me.png`, `Co.png`, `Dokkaebi.png` | Có. Mỗi file là cái đầu, không có thân |
| `Sprites/Food/*.png` (8 món) | Có — mỗi `dishId` một file |

---

## 1. Ưu tiên vẽ (đủ nộp 12 màn)

### A. Ba khách — bắt buộc

| File gợi ý | Dùng cho | Ghi chú vẽ |
|---|---|---|
| `NPCs/Gumiho.png` | Guest + PPT | Đầu hồ ly chín đuôi, khăn/hanbok gọn. Màu trắng trên sprite, son `#8C2F39` là màu vùng ăn. |
| `NPCs/Ho.png` | Guest màn 5–7, 11–12 | Đầu hổ tranh dân gian. Màu lá `#2F6B4F` là màu vùng ăn. |
| `NPCs/Me.png` | Guest màn 8–9 | Đầu chó trắng đội nón mê. Màu chàm `#1F4E79` là màu vùng ăn. |
| `NPCs/Co.png` | Guest màn 15 | Đầu cò trắng, mỏ dài. Màu `#3E6B66` là màu vùng ăn. |
| `NPCs/Dokkaebi.png` | Guest màn 16–17 | Đầu Dokkaebi, sừng. Màu lửa `#E24A2A` là màu vùng ăn. |

Không vẽ sprite gật. Ảnh không có cổ. Nhát hỏng lật ngang đầu đang có.

### B. Tám món trên mâm — bắt buộc

Mỗi món: **một sprite thân** rõ mặt, bóng nhỏ. Cùng sprite dùng trên mâm và (có thể scale nhỏ) cho icon UI.

| dishId | File gợi ý | Tên hiện | Màu gợi ý vẽ |
|---|---|---|---|
| `banh_mi` | `Food/banh_mi.png` | Bánh mì | vàng bánh |
| `goi_cuon` | `Food/goi_cuon.png` | Gỏi cuốn | xanh lá nhạt |
| `pho` | `Food/pho.png` | Phở | nước dùng vàng |
| `ca_phe` | `Food/ca_phe.png` | Cà phê sữa đá | nâu đậm |
| `gimbap` | `Food/gimbap.png` | Gimbap | xanh rong |
| `songpyeon` | `Food/songpyeon.png` | Songpyeon | hồng |
| `tteokbokki` | `Food/tteokbokki.png` | Tteokbokki | đỏ ớt |
| `bibimbap` | `Food/bibimbap.png` | Bibimbap | cam–vàng |

Kích thước gợi ý canvas: **256×256** hoặc **512×512**, món chiếm ~70–80% khung, chừa lề trong suốt.

Prefab `Food` scale `0.4` → đường kính world ≈ bán kính logic `0.08` × 2 × `2.5` = `0.4`.

### C. Mâm — bắt buộc (có thể placeholder hình tròn trước)

| Object / file | Vai trò | Màu gợi ý |
|---|---|---|
| `Tray/rim.png` hoặc sprite tròn | Vành ngoài | `#8B6628` |
| `Tray/face.png` | Mặt mâm (mép trong = radius 1) | `#C6A15A` |
| `Tray/hub.png` | Núm giữa | `#EDDCAE` |
| `Tray/notch.png` | Vạch thấy xoay | `#2C2420` |

Ban đầu được phép dùng **sprite Circle** Unity tô màu; thay hình vẽ tay trước khi quay video / nộp.

### D. Vùng ăn — bắt buộc theo khách

| File / object | Khách | Ghi chú |
|---|---|---|
| Sprite tròn alpha ~70, `#8C2F39` | Gumiho | `GumihoZone` — script scale theo bán kính |
| Hai sọc / khe (hoặc 2 sprite thanh) | Hổ | Màn 5+ ; giữ để khe rộng |
| Dải thẳng (sprite chữ nhật bo nhẹ) | Mê | Màn 8–9 ; dài theo giữ, và lắc ở mọi màn có Mê |

### E. UI / phụ — tối thiểu

| Sprite | Bắt buộc? | Ghi chú |
|---|---|---|
| `Food/...` dùng lại cho `FoodIcon` Swatch | Có | Cùng sprite món trên mâm. Script gán lúc Play, kèm tên món. Món thèm sáng có viền, món chưa tới lượt tối |
| `ui/ring_want.png` | Không | Viền kem quanh món thèm; Hierarchy dùng con `Ring` sprite tròn |
| `ui/hold_fill` | Không | Image UI Filled, không cần art riêng |
| `ui/heart.png` ×3 | Không | Hiện là Text `Mạng 3` |
| `ui/title_logo.png` | Không | Text “Một Mâm” / “한상” đủ cho bản nộp |
| Nền / gradient giấy | Không | Camera background `#F6F0E6` |

---

## 2. Theo giai đoạn làm

### Giai đoạn 1 — màn 1–4 (đang làm)

Tám file món đã có. Còn thiếu khách và mâm vẽ tay nếu muốn bỏ placeholder:

| # | Sprite | Ghi chú |
|---|---|---|
| 1 | `Gumiho.png` | Đã có |
| 2–5 | `songpyeon`, `tteokbokki`, `gimbap`, `pho` | Đã có |
| 6–9 | `banh_mi`, `goi_cuon`, `bibimbap`, `ca_phe` | Đã có |
| 10 | Mâm (4 lớp hoặc placeholder) | |
| 11 | Vòng GumihoZone | Circle alpha được |

Màn 1–4 trong JSON đụng các `dishId`:  
`songpyeon`, `banh_mi`, `bibimbap`, `tteokbokki`, `pho`, `goi_cuon`, `gimbap`, `ca_phe` → **cả 8 món**.

### Giai đoạn 2 — màn 5–9

| # | Sprite |
|---|---|
| 12 | `Ho.png` |
| 13 | `Me.png` |
| 14 | Sprite khe Hổ |
| 15 | Sprite dải Mê |

### Giai đoạn 3 — màn 11–12 + nộp

| # | Sprite |
|---|---|
| 16 | (cùng khách + món đã có) |
| 17 | Sketch tay scan vào PPT (có thể khác file game) |

---

## 3. Thư mục đề xuất trong Unity

```
Assets/Sprites/
  NPCs/
    Gumiho.png
    Ho.png
    Me.png
    Co.png
    Dokkaebi.png
  Food/
    banh_mi.png
    goi_cuon.png
    pho.png
    ca_phe.png
    gimbap.png
    songpyeon.png
    tteokbokki.png
    bibimbap.png
  Tray/
    rim.png
    face.png
    hub.png
    notch.png
  Zones/
    gumiho_circle.png
    ho_gap.png          (sau)
    me_strip.png        (sau)
  UI/                   (tùy chọn)
    ring_want.png
    heart.png
```

---

## 4. Checklist đếm nhanh

| Nhóm | Số file tối thiểu | Đã có |
|---|---|---|
| Khách | 5 đầu (Gumiho, Hổ, Mê, Cò, Dokkaebi) | 5 |
| Món | 8 | 8 |
| Mâm | 4 (hoặc 1 circle tái dùng) | placeholder OK |
| Vùng ăn | 1 (Gumiho) → sau +2 | placeholder OK |
| **Cộng art khách và món** | **13** (5 đầu + 8 món) | **13** |
| PPT sketch | vài tấm scan | — |

Audio không nằm trong list này — xem `docs/ban-quyen` khi có tiếng.

---

## 5. Liên kết

- Hierarchy / PPU / sorting: `Huong-Dan-Hierarchy.md`
- `dishId` và vị trí màn: `Mo-Mam-Vi-Tri-Mon-An.md`
- Luật + bảng màu: `Mo-Mam-Tai-Lieu-Thiet-Ke.md` §6, §9
- Hồ sơ từng màn: `Mo-Mam-Thiet-Ke-Man.md`
- Tên món trong code: `GameSrc/Assets/Scripts/Art.cs`. Hình món gán ở bảng Dishes của `LevelRunner`, không tô màu theo `dishId`.
