# -*- coding: utf-8 -*-
"""Xuất bản PowerPoint ý tưởng Một Mâm cho Korea Game Week 2026.

Sửa phần ĐIỀN TRƯỚC KHI NỘP rồi chạy:
    python tools/tao_pptx_y_tuong.py
"""

from lxml import etree
from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.oxml.ns import qn
from pptx.util import Emu, Inches, Pt

# --- Điền trước khi nộp ---
TEAM_NAME = "[Điền tên cá nhân hoặc đội]"
MEMBER_1 = "[Thành viên 1: họ tên — trường — vai trò]"
MEMBER_2 = "[Thành viên 2: họ tên — trường — vai trò. Xóa dòng này nếu thi cá nhân]"
EMAIL = "[Điền email]"
PHONE = "[Điền số điện thoại]"

OUT = r"d:\VieKorGame\Mot-Mam-Y-Tuong-KGW2026.pptx"

CREAM = RGBColor(0xF6, 0xF0, 0xE6)
INK = RGBColor(0x2C, 0x24, 0x20)
RED = RGBColor(0x8C, 0x2F, 0x39)
GOLD = RGBColor(0xC6, 0xA1, 0x5A)
GREEN = RGBColor(0x2F, 0x6B, 0x4F)
BLUE = RGBColor(0x1F, 0x4E, 0x79)
WHITE = RGBColor(0xFF, 0xFC, 0xF7)
CARD = RGBColor(0xFF, 0xF9, 0xF2)
MUTED = RGBColor(0x6B, 0x5E, 0x54)
ROW = RGBColor(0xFB, 0xF6, 0xEE)
SOFT_RED = RGBColor(0xF3, 0xE4, 0xE2)
SOFT_GREEN = RGBColor(0xE4, 0xEF, 0xE8)
SOFT_BLUE = RGBColor(0xE4, 0xEB, 0xF3)
DARK_GOLD = RGBColor(0x8A, 0x6A, 0x24)
TEAL = RGBColor(0x3E, 0x6B, 0x66)
FIRE = RGBColor(0xE2, 0x4A, 0x2A)
SOFT_TEAL = RGBColor(0xE3, 0xEE, 0xEC)
SOFT_FIRE = RGBColor(0xFB, 0xE6, 0xDF)
SOFT_GOLD = RGBColor(0xF5, 0xEB, 0xD3)

A_NS = "http://schemas.openxmlformats.org/drawingml/2006/main"
SLIDE_W = Inches(13.333)
SLIDE_H = Inches(7.5)


def rgb_hex(color):
    return f"{color[0]:02X}{color[1]:02X}{color[2]:02X}"


def set_run_font(run, name, size, bold, color):
    run.font.name = name
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    r_pr = run._r.get_or_add_rPr()
    for tag in ("a:latin", "a:ea", "a:cs"):
        el = r_pr.find(qn(tag))
        if el is None:
            el = etree.SubElement(r_pr, qn(tag))
        el.set("typeface", name)


def add_rect(slide, x, y, w, h, fill, line=None):
    shape = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, x, y, w, h)
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill
    if line is None:
        shape.line.fill.background()
    else:
        shape.line.color.rgb = line
        shape.line.width = Pt(1.25)
    # Drop the default shadow-less but also any text inset issues.
    sp_pr = shape._element.spPr
    effect = sp_pr.find(qn("a:effectLst"))
    if effect is not None:
        sp_pr.remove(effect)
    return shape


def add_round(slide, x, y, w, h, fill, line=None, radius=0.12):
    shape = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, x, y, w, h)
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill
    if line is None:
        shape.line.fill.background()
    else:
        shape.line.color.rgb = line
        shape.line.width = Pt(1.25)
    try:
        shape.adjustments[0] = radius
    except Exception:
        pass
    return shape


def add_oval(slide, x, y, w, h, fill, line=None, line_pt=1.5):
    shape = slide.shapes.add_shape(MSO_SHAPE.OVAL, x, y, w, h)
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill
    if line is None:
        shape.line.fill.background()
    else:
        shape.line.color.rgb = line
        shape.line.width = Pt(line_pt)
    return shape


def new_paragraph(tf):
    if len(tf.paragraphs) == 1 and len(tf.paragraphs[0].runs) == 0:
        return tf.paragraphs[0]
    return tf.add_paragraph()


def write_lines(slide, x, y, w, h, lines, anchor=MSO_ANCHOR.TOP):
    """lines: list of dicts with text, size, bold, color, align, space_before, font."""
    box = slide.shapes.add_textbox(x, y, w, h)
    tf = box.text_frame
    tf.word_wrap = True
    tf.auto_size = None
    tf.margin_left = Inches(0.02)
    tf.margin_right = Inches(0.02)
    tf.margin_top = Inches(0.0)
    tf.margin_bottom = Inches(0.0)
    tf.vertical_anchor = anchor
    for item in lines:
        p = new_paragraph(tf)
        p.alignment = item.get("align", PP_ALIGN.LEFT)
        p.space_before = Pt(item.get("before", 0))
        p.space_after = Pt(item.get("after", 0))
        run = p.add_run()
        run.text = item["text"]
        set_run_font(
            run,
            item.get("font", "Arial"),
            item.get("size", 16),
            item.get("bold", False),
            item.get("color", INK),
        )
    return box


def fill_shape_text(shape, lines, anchor=MSO_ANCHOR.TOP, margin=0.1):
    tf = shape.text_frame
    tf.word_wrap = True
    tf.auto_size = None
    tf.margin_left = Inches(margin)
    tf.margin_right = Inches(margin)
    tf.margin_top = Inches(0.08)
    tf.margin_bottom = Inches(0.06)
    tf.vertical_anchor = anchor
    for item in lines:
        p = new_paragraph(tf)
        p.alignment = item.get("align", PP_ALIGN.LEFT)
        p.space_before = Pt(item.get("before", 0))
        p.space_after = Pt(item.get("after", 0))
        run = p.add_run()
        run.text = item["text"]
        set_run_font(
            run,
            item.get("font", "Arial"),
            item.get("size", 15),
            item.get("bold", False),
            item.get("color", INK),
        )
    return shape


def set_cell_fill(cell, color):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    for child in list(tc_pr):
        if child.tag == f"{{{A_NS}}}solidFill":
            tc_pr.remove(child)
    solid = etree.SubElement(tc_pr, f"{{{A_NS}}}solidFill")
    srgb = etree.SubElement(solid, f"{{{A_NS}}}srgbClr")
    srgb.set("val", rgb_hex(color))


def set_cell_text(cell, text, size, bold, color, align=PP_ALIGN.LEFT):
    cell.margin_left = Inches(0.06)
    cell.margin_right = Inches(0.05)
    cell.margin_top = Inches(0.02)
    cell.margin_bottom = Inches(0.02)
    cell.vertical_anchor = MSO_ANCHOR.MIDDLE
    tf = cell.text_frame
    tf.word_wrap = True
    p = tf.paragraphs[0]
    p.alignment = align
    p.clear()
    run = p.add_run()
    run.text = text
    set_run_font(run, "Arial", size, bold, color)


def notes(slide, text):
    slide.notes_slide.notes_text_frame.text = text


def content_bg(slide):
    add_rect(slide, 0, 0, SLIDE_W, SLIDE_H, CREAM)
    add_rect(slide, 0, 0, SLIDE_W, Inches(0.12), RED)


def footer(slide, page, total):
    add_rect(slide, 0, Inches(7.18), SLIDE_W, Inches(0.32), INK)
    write_lines(
        slide,
        Inches(0.45),
        Inches(7.2),
        Inches(10.2),
        Inches(0.28),
        [{"text": "Một Mâm  ·  한상  ·  Korea Game Week 2026", "size": 11, "color": CREAM, "font": "Arial"}],
        anchor=MSO_ANCHOR.MIDDLE,
    )
    write_lines(
        slide,
        Inches(11.3),
        Inches(7.2),
        Inches(1.6),
        Inches(0.28),
        [{"text": f"{page}  /  {total}", "size": 11, "color": GOLD, "align": PP_ALIGN.RIGHT}],
        anchor=MSO_ANCHOR.MIDDLE,
    )


def title(slide, text, subtitle=None):
    write_lines(
        slide,
        Inches(0.48),
        Inches(0.28),
        Inches(12.3),
        Inches(0.55),
        [{"text": text, "size": 28, "bold": True, "color": INK}],
    )
    add_rect(slide, Inches(0.5), Inches(0.9), Inches(1.35), Inches(0.045), GOLD)
    if subtitle:
        write_lines(
            slide,
            Inches(0.48),
            Inches(1.0),
            Inches(12.3),
            Inches(0.38),
            [{"text": subtitle, "size": 14, "color": MUTED}],
        )


def new_slide(prs):
    return prs.slides.add_slide(prs.slide_layouts[6])


TOTAL = 14


def slide_cover(prs):
    s = new_slide(prs)
    add_rect(s, 0, 0, SLIDE_W, SLIDE_H, INK)
    add_rect(s, 0, 0, Inches(0.28), SLIDE_H, RED)
    add_rect(s, Inches(0.28), 0, Inches(0.08), SLIDE_H, GOLD)
    write_lines(
        s,
        Inches(0.85),
        Inches(1.15),
        Inches(11.5),
        Inches(0.4),
        [{"text": "TUẦN LỄ GAME HÀN QUỐC  2026", "size": 16, "bold": True, "color": GOLD}],
    )
    write_lines(
        s,
        Inches(0.8),
        Inches(1.7),
        Inches(11.5),
        Inches(1.3),
        [{"text": "Một Mâm", "size": 72, "bold": True, "color": CREAM}],
    )
    write_lines(
        s,
        Inches(0.85),
        Inches(3.05),
        Inches(8),
        Inches(0.7),
        [{"text": "한상", "size": 36, "color": GOLD, "font": "Malgun Gothic"}],
    )
    write_lines(
        s,
        Inches(0.85),
        Inches(3.9),
        Inches(10.5),
        Inches(1.15),
        [
            {
                "text": "Casual game một ngón về mâm tiệc Việt Nam và Hàn Quốc.",
                "size": 22,
                "color": CREAM,
            },
            {
                "text": "Giữ mâm. Đãi đúng người. Đừng để khách ăn nhầm món.",
                "size": 22,
                "bold": True,
                "color": WHITE,
                "before": 8,
            },
        ],
    )
    add_rect(s, Inches(0.85), Inches(5.5), Inches(2.2), Inches(0.015), GOLD)
    write_lines(
        s,
        Inches(0.85),
        Inches(5.7),
        Inches(11),
        Inches(0.9),
        [
            {"text": "Hạng mục lập trình game   ·   Unity   ·   Android   ·   Chơi thử trên máy tính", "size": 15, "color": CREAM},
            {"text": "Hyper-casual / casual   ·   Một màn khoảng 20–40 giây", "size": 15, "color": GOLD, "before": 4},
        ],
    )
    notes(
        s,
        "Mở bằng câu: Giữ mâm. Đãi đúng người. Đừng để khách ăn nhầm món. "
        "Game là một mâm chung, người chơi là người giữ mâm.",
    )


def slide_info(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Thông tin dự thi", "Điền phần trong ngoặc trước khi gửi. Nhóm tối đa 2 người.")
    rows = [
        ("Đội hoặc cá nhân", TEAM_NAME),
        ("Thành viên", MEMBER_1),
        ("Thành viên", MEMBER_2),
        ("Email", EMAIL),
        ("Điện thoại", PHONE),
        ("Hạn nộp", "Hết ngày 11/11/2026  ·  Công bố dự kiến 18/11/2026"),
        ("Nộp bài", "btc.koreagameweek@gmail.com"),
        ("Tiêu đề thư", "[Dự thi Lập trình game KGW2026]"),
    ]
    top = 1.5
    for i, (label, value) in enumerate(rows):
        y = Inches(top + i * 0.64)
        add_round(s, Inches(0.5), y, Inches(12.35), Inches(0.56), WHITE, line=SOFT_RED, radius=0.15)
        write_lines(
            s,
            Inches(0.7),
            y + Inches(0.08),
            Inches(2.5),
            Inches(0.4),
            [{"text": label, "size": 13, "bold": True, "color": RED}],
            anchor=MSO_ANCHOR.MIDDLE,
        )
        write_lines(
            s,
            Inches(3.3),
            y + Inches(0.06),
            Inches(9.3),
            Inches(0.44),
            [{"text": value, "size": 15, "color": INK}],
            anchor=MSO_ANCHOR.MIDDLE,
        )
    footer(s, 2, TOTAL)
    notes(s, "Nhớ đăng ký trên link chính thức của BTC trước khi nộp. File này chỉ là hồ sơ ý tưởng.")


def slide_inspiration(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Cảm hứng", "Giữ nhịp một chạm và vật xoay của game ném dao vào vòng gỗ. Đổi ném thành canh miếng.")

    # --- card trái: game gốc
    add_round(s, Inches(0.45), Inches(1.5), Inches(5.7), Inches(2.5), WHITE, radius=0.06)
    cx, cy, r = 1.75, 2.6, 0.65
    add_oval(s, Inches(cx - r), Inches(cy - r), Inches(2 * r), Inches(2 * r), SOFT_GOLD, line=GOLD, line_pt=2.5)
    for ang in (20, 80, 140, 200, 260, 320):
        d = r + 0.12
        import math
        kx = cx + d * math.sin(math.radians(ang))
        ky = cy - d * math.cos(math.radians(ang))
        k = add_rect(s, Inches(kx - 0.03), Inches(ky - 0.22), Inches(0.06), Inches(0.44), INK)
        k.rotation = ang
    k = add_rect(s, Inches(cx - 0.03), Inches(cy + r + 0.22), Inches(0.06), Inches(0.38), MUTED)
    write_lines(
        s, Inches(3.15), Inches(1.7), Inches(2.9), Inches(2.2),
        [
            {"text": "Game gốc", "size": 12, "bold": True, "color": MUTED},
            {"text": "Ném dao vào vòng xoay", "size": 17, "bold": True, "color": INK, "before": 2},
            {"text": "Chạm để ném. Dao cắm vào vòng. Ném trúng dao đã cắm là thua.", "size": 14, "color": INK, "before": 8},
        ],
    )

    # --- mũi tên
    arrow = s.shapes.add_shape(MSO_SHAPE.RIGHT_ARROW, Inches(6.3), Inches(2.4), Inches(0.6), Inches(0.5))
    arrow.fill.solid()
    arrow.fill.fore_color.rgb = RED
    arrow.line.fill.background()

    # --- card phải: Một Mâm
    add_round(s, Inches(7.05), Inches(1.5), Inches(5.8), Inches(2.5), WHITE, radius=0.06)
    mx, my = 8.35, 2.4
    add_oval(s, Inches(mx - r), Inches(my - r), Inches(2 * r), Inches(2 * r), CREAM, line=GOLD, line_pt=2.5)
    for dx, dy, col in ((-0.28, -0.25, RED), (0.25, -0.28, GREEN), (0.32, 0.15, BLUE), (-0.08, 0.28, GOLD), (-0.36, 0.08, RED)):
        add_oval(s, Inches(mx + dx - 0.12), Inches(my + dy - 0.12), Inches(0.24), Inches(0.24), col)
    add_oval(s, Inches(mx - 0.45), Inches(my + r - 0.15), Inches(0.9), Inches(0.9), SOFT_RED, line=RED, line_pt=2)
    add_oval(s, Inches(mx - 0.15), Inches(my + r + 0.15), Inches(0.3), Inches(0.3), RED)
    write_lines(
        s, Inches(9.65), Inches(1.7), Inches(3.1), Inches(2.2),
        [
            {"text": "Một Mâm", "size": 12, "bold": True, "color": RED},
            {"text": "Giữ mâm xoay, thả để khách cắn", "size": 17, "bold": True, "color": INK, "before": 2},
            {"text": "Cắn nhầm món chưa tới lượt là mất một mạng.", "size": 14, "color": INK, "before": 8},
        ],
    )

    # --- bảng so sánh
    rows = [
        ("", "Game ném dao", "Một Mâm"),
        ("Vật xoay", "Vòng gỗ", "Mâm tiệc có món Việt xen món Hàn"),
        ("Một ngón", "Chạm là ném dao", "Giữ để phanh mâm và nạp vùng ăn, thả để cắn"),
        ("Điều phải tránh", "Dao đã cắm trên vòng", "Món chưa tới lượt nằm sát món đang thèm"),
        ("Phần thêm", "Một loại dao", "Mỗi khách một hình vùng ăn, món đổi nghĩa, hai khách một mâm"),
    ]
    tbl = s.shapes.add_table(5, 3, Inches(0.45), Inches(4.2), Inches(12.4), Inches(2.75)).table
    for i, w in enumerate((2.2, 3.6, 6.6)):
        tbl.columns[i].width = Inches(w)
    for r_i, row in enumerate(rows):
        for c_i, text in enumerate(row):
            cell = tbl.cell(r_i, c_i)
            if r_i == 0:
                set_cell_fill(cell, RED)
                set_cell_text(cell, text, 13, True, CREAM)
            else:
                set_cell_fill(cell, WHITE if r_i % 2 else ROW)
                set_cell_text(cell, text, 14, c_i == 0, RED if c_i == 0 else INK)
        tbl.rows[r_i].height = Inches(0.4 if r_i == 0 else 0.58)
    footer(s, 3, TOTAL)
    notes(
        s,
        "Ý tưởng lớn lên từ game ném dao vào vòng xoay: một ngón, vật xoay, canh thời điểm. "
        "Mình giữ cái hay đó và đổi hai thứ. Một: không ném đồ vào vòng, mà để khách cắn món trên mâm xoay. "
        "Hai: giữ không chỉ là chờ, giữ còn phanh mâm và làm vùng ăn lớn dần, nên có lúc phải kìm ngón. "
        "Hình, nhân vật, luật thèm và chưa thèm đều là của đội.",
    )


def slide_idea(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Ý tưởng", "Người chơi giữ mâm. Khách tự cắn bằng vùng ăn của mình.")
    add_round(s, Inches(0.5), Inches(1.55), Inches(12.35), Inches(1.55), RED, radius=0.08)
    write_lines(
        s,
        Inches(0.8),
        Inches(1.7),
        Inches(11.8),
        Inches(1.3),
        [
            {"text": "Mỗi khách đang thèm vài món trên mâm xoay.", "size": 22, "color": CREAM},
            {
                "text": "Giữ ngón để mâm chậm lại và vùng ăn lớn ra. Thả ngón để cắn.",
                "size": 22,
                "bold": True,
                "color": WHITE,
                "before": 6,
            },
        ],
    )
    beats = [
        ("1", "Nhìn", "Icon trên bảng khách chỉ món đang thèm và món chưa tới lượt."),
        ("2", "Giữ", "Mâm phanh. Vòng, khe hổ hoặc dải thẳng lớn dần theo ngón."),
        ("3", "Thả", "Khách cắn một lần. Ăn nhầm món chưa tới lượt thì mất một mạng."),
    ]
    for i, (num, head, body) in enumerate(beats):
        x = Inches(0.5 + i * 4.15)
        card = add_round(s, x, Inches(3.4), Inches(3.95), Inches(2.35), WHITE, line=GOLD, radius=0.1)
        fill_shape_text(
            card,
            [
                {"text": num, "size": 20, "bold": True, "color": GOLD},
                {"text": head, "size": 26, "bold": True, "color": RED, "before": 2},
                {"text": body, "size": 15, "color": INK, "before": 8},
            ],
        )
    write_lines(
        s,
        Inches(0.55),
        Inches(5.95),
        Inches(12.2),
        Inches(0.95),
        [
            {
                "text": "Qua màn khi ăn hết món đang thèm, còn mạng, và không vượt số lượt cắn.",
                "size": 16,
                "bold": True,
                "color": INK,
            },
            {
                "text": "Chờ cũng là một nước đi. Món trôi qua vùng ăn sẽ không bị ăn nếu chưa thả ngón.",
                "size": 15,
                "color": MUTED,
                "before": 4,
            },
        ],
    )
    footer(s, 4, TOTAL)
    notes(
        s,
        "Nhấn mạnh một cử chỉ cho mọi khách. Giữ làm hai việc: phanh mâm và nạp vùng ăn. "
        "Khách không ghét món của nước kia. Họ chỉ chưa thèm món đó trong màn này.",
    )


def slide_setting(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Bối cảnh", "Một mâm. Hai bếp. Người chơi là người giữ mâm.")
    blocks = [
        (
            RED,
            "Nơi diễn ra",
            "Mâm tròn sơn son, giữa là mâm xoay. Món Việt và món Hàn xếp xen. Mỗi màn một hoặc hai khách ngồi ở mép.",
        ),
        (
            GREEN,
            "Nhịp chơi",
            "Hai mươi màn, mỗi màn khoảng 20–40 giây. Chơi liền một mạch hay chơi lẻ từng màn đều hợp casual trên điện thoại.",
        ),
        (
            BLUE,
            "Hình và chất",
            "Hình phẳng, nét rõ, màu son, mực, kem giấy. Lấy nhịp tranh Đông Hồ và minhwa. Hài khi ăn nhầm, không chê ẩm thực.",
        ),
        (
            GOLD,
            "Vì sao là văn hóa",
            "Mâm chung, hổ tranh dân gian hai nước và cách nhường đúng miếng nằm trong luật chơi. Văn hóa là cách đãi khách, không chỉ là bộ trang phục.",
        ),
    ]
    for i, (color, head, body) in enumerate(blocks):
        col = i % 2
        row = i // 2
        x = Inches(0.5 + col * 6.4)
        y = Inches(1.6 + row * 2.55)
        add_round(s, x, y, Inches(6.15), Inches(2.35), WHITE, radius=0.08)
        add_rect(s, x, y, Inches(0.12), Inches(2.35), color)
        write_lines(
            s,
            x + Inches(0.38),
            y + Inches(0.25),
            Inches(5.5),
            Inches(1.9),
            [
                {"text": head, "size": 22, "bold": True, "color": color},
                {"text": body, "size": 16, "color": INK, "before": 10},
            ],
        )
    footer(s, 5, TOTAL)


def slide_cast(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Nhân vật", "Năm khách, năm hình vùng ăn. Icon trên bảng khách mới là luật của từng màn.")
    cast = [
        (RED, "Gumiho", "Khách chính  ·  Hàn Quốc",
         "Hồ ly chín đuôi, vẽ gốc. Vòng tròn neo ở mép mâm, giữ để bán kính lan vào bàn.",
         "Món ký: songpyeon, bánh trôi."),
        (GREEN, "Hổ", "Cầu nối hai nước",
         "Hổ tranh dân gian, nét Đông Hồ trộn minhwa. Giữ để hai hàm tách ra. Khe quá rộng cuốn luôn món ở giữa.",
         "Món ký: gỏi cuốn, gimbap."),
        (BLUE, "Mê", "Khách lướt  ·  Việt Nam",
         "Chó trắng đội nón mê. Dải thẳng dài dần và luôn lắc. Nhát đủ dài thì Mê sang mép kia.",
         "Món ký: thịt nướng, galbi."),
        (TEAL, "Cò", "Mỏ chọc sâu  ·  Việt Nam",
         "Cò trắng. Vùng ăn là một chấm chạy từ mép vào tâm. Giữ để chấm đi sâu, chấm không to ra.",
         "Câu đố: độ sâu, không phải bề rộng."),
        (FIRE, "Dokkaebi", "Lửa ma  ·  Hàn Quốc",
         "Yêu tinh sừng và chùy. Đốm lửa chạy từ tâm ra mép. Sau vài nhát, món đang thèm đổi.",
         "Câu đố: icon đổi giữa màn."),
    ]
    for i, (color, name, role, body, food) in enumerate(cast):
        x = Inches(0.42 + i * 2.5)
        card = add_round(s, x, Inches(1.6), Inches(2.4), Inches(4.1), WHITE, radius=0.08)
        add_rect(s, x, Inches(1.6), Inches(2.4), Inches(0.14), color)
        fill_shape_text(
            card,
            [
                {"text": name, "size": 26, "bold": True, "color": color, "before": 8},
                {"text": role, "size": 12, "bold": True, "color": MUTED, "before": 2},
                {"text": body, "size": 15, "color": INK, "before": 10},
                {"text": food, "size": 14, "bold": True, "color": INK, "before": 10},
            ],
            margin=0.14,
        )
    strip = add_round(s, Inches(0.42), Inches(5.95), Inches(12.4), Inches(0.9), INK, radius=0.12)
    fill_shape_text(
        strip,
        [{"text": "Tám món dùng suốt game: bánh mì, gỏi cuốn, phở, cà phê sữa đá, gimbap, songpyeon, tteokbokki, bibimbap.",
          "size": 16, "color": CREAM}],
        anchor=MSO_ANCHOR.MIDDLE, margin=0.3,
    )
    footer(s, 6, TOTAL)
    notes(
        s,
        "Vòng của Gumiho phải lệch tâm. Vòng đúng tâm mâm thì xoay bàn không đổi ai bị ăn. "
        "Đường của Mê là dải nửa bàn, không phải đường kính ăn cả hai phía một lúc. "
        "Mê không vào màn hai khách vì lướt sẽ đổi mép đứng. "
        "Bài nộp vẫn kể được bằng ba khách đầu; Cò và Dokkaebi là phần thêm.",
    )


def slide_play(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Cách chơi", "Một cử chỉ. Mọi khách chỉ khác nhau ở hình vùng ăn.")
    rules = [
        ("Mâm tự xoay", "Món là con của mâm, nên trôi theo vòng. Vùng ăn đứng yên trên màn hình, neo ở mép chỗ khách ngồi."),
        ("Giữ để phanh và nạp", "Ngón giữ càng lâu, mâm càng chậm và vùng ăn càng lớn. Chạm thật nhanh thì vùng rất nhỏ."),
        ("Thả để cắn", "Nhát sạch: ăn món thèm, trừ một lượt. Nhát dính món chưa tới lượt: mất một mạng, món ở lại, không trừ lượt."),
        ("Mạng và lượt", "Màn giới thiệu có 3 mạng, các màn sau có 2. Hết mạng, hoặc hết lượt mà còn món thèm, thì chơi lại màn."),
    ]
    for i, (head, body) in enumerate(rules):
        y = Inches(1.5 + i * 1.3)
        add_round(s, Inches(0.5), y, Inches(12.35), Inches(1.18), WHITE, radius=0.1)
        add_oval(s, Inches(0.72), y + Inches(0.32), Inches(0.52), Inches(0.52), RED)
        write_lines(
            s,
            Inches(0.72),
            y + Inches(0.38),
            Inches(0.52),
            Inches(0.42),
            [{"text": str(i + 1), "size": 16, "bold": True, "color": CREAM, "align": PP_ALIGN.CENTER}],
        )
        write_lines(
            s,
            Inches(1.5),
            y + Inches(0.14),
            Inches(10.9),
            Inches(0.95),
            [
                {"text": head, "size": 18, "bold": True, "color": INK},
                {"text": body, "size": 15, "color": MUTED, "before": 2},
            ],
        )
    footer(s, 7, TOTAL)


def slide_skills(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Năm vùng ăn", "Aim bằng mâm xoay. Vùng ăn cố định trên màn hình. Giữ chỉ làm vùng đó lớn ra hoặc đi sâu.")
    cards = [
        (RED, "Gumiho  ·  vòng mép", "Vòng neo trước mặt Gumiho, lan vào mâm. Hợp khi món thèm nằm thành cụm tròn."),
        (GREEN, "Hổ  ·  khe hai hàm", "Hai hàm tách dần. Món cấm nằm giữa thì phải hai nhát hẹp."),
        (BLUE, "Mê  ·  dải và lướt", "Dải lắc khi chưa giữ. Giữ thì đóng băng. Nhát đủ dài thì Mê sang mép kia."),
        (TEAL, "Cò  ·  chấm sâu", "Một chấm chạy từ mép vào tâm. Giữ để đi sâu, chấm không to ra."),
        (FIRE, "Dokkaebi  ·  đốm lửa", "Đốm chạy tâm–mép. Giữ thì khóa độ sâu và quét ngang. Sau vài nhát, icon đổi."),
    ]
    for i, (color, head, body) in enumerate(cards):
        x = Inches(0.42 + i * 2.5)
        add_round(s, x, Inches(1.55), Inches(2.4), Inches(4.4), WHITE, radius=0.08)
        sx = x + Inches(0.35)
        sy = Inches(1.85)
        add_oval(s, sx, sy, Inches(1.7), Inches(1.7), CREAM, line=GOLD, line_pt=2)
        if i == 0:
            add_oval(s, sx + Inches(0.1), sy + Inches(0.45), Inches(0.8), Inches(0.8), SOFT_RED, line=RED, line_pt=2)
        elif i == 1:
            add_rect(s, sx + Inches(0.55), sy + Inches(0.25), Inches(0.06), Inches(1.2), GREEN)
            add_rect(s, sx + Inches(1.05), sy + Inches(0.25), Inches(0.06), Inches(1.2), GREEN)
        elif i == 2:
            add_round(s, sx + Inches(0.2), sy + Inches(1.0), Inches(1.3), Inches(0.2), SOFT_BLUE, line=BLUE, radius=0.4)
            write_lines(
                s, sx, sy + Inches(1.25), Inches(1.7), Inches(0.25),
                [{"text": "sang mép kia", "size": 10, "bold": True, "color": BLUE, "align": PP_ALIGN.CENTER}],
            )
        elif i == 3:
            add_rect(s, sx + Inches(0.83), sy + Inches(0.15), Inches(0.04), Inches(1.4), SOFT_TEAL)
            add_oval(s, sx + Inches(0.73), sy + Inches(0.95), Inches(0.24), Inches(0.24), TEAL)
        else:
            add_rect(s, sx + Inches(0.85), sy + Inches(0.83), Inches(0.7), Inches(0.04), SOFT_FIRE)
            add_oval(s, sx + Inches(1.2), sy + Inches(0.72), Inches(0.26), Inches(0.26), FIRE)
        write_lines(
            s, x + Inches(0.18), Inches(3.8), Inches(2.1), Inches(2.9),
            [
                {"text": head, "size": 16, "bold": True, "color": color},
                {"text": body, "size": 15, "color": INK, "before": 8},
            ],
        )
    strip = add_round(s, Inches(0.42), Inches(6.1), Inches(12.4), Inches(0.8), INK, radius=0.12)
    fill_shape_text(
        strip,
        [{"text": "Cùng một cử chỉ cho cả năm khách: giữ để phanh và nạp, thả để cắn.", "size": 16, "bold": True, "color": CREAM}],
        anchor=MSO_ANCHOR.MIDDLE, margin=0.3,
    )
    footer(s, 8, TOTAL)
    notes(
        s,
        "Mê: dải lắc khi chưa giữ. Nhấn giữ thì góc dải đóng băng và mâm giảm tốc. Thả trước ngưỡng giữa bàn thì đứng yên, dải không đổi phía. "
        "Thả sau ngưỡng thì lướt và đổi phía. Cò và Dokkaebi dùng cùng cử chỉ: chỉ đổi hình vùng ăn, một chấm và một đốm lửa.",
    )


def slide_duo(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Màn hai khách", "Gumiho bên trái, Hổ bên phải. Một ngón chọn một phía. Dùng ở màn 18, 19 và 20.")
    add_round(s, Inches(0.45), Inches(1.55), Inches(7.35), Inches(5.25), WHITE, radius=0.08)
    # screen split diagram
    add_round(s, Inches(0.75), Inches(1.85), Inches(6.75), Inches(3.55), CREAM, line=GOLD, radius=0.06)
    add_rect(s, Inches(4.1), Inches(1.85), Inches(0.015), Inches(3.55), GOLD)
    add_oval(s, Inches(2.55), Inches(2.35), Inches(2.55), Inches(2.55), WHITE, line=GOLD, line_pt=2)
    add_oval(s, Inches(2.15), Inches(3.05), Inches(0.95), Inches(0.95), SOFT_RED, line=RED, line_pt=1.75)
    add_rect(s, Inches(4.55), Inches(2.85), Inches(0.08), Inches(1.35), GREEN)
    add_rect(s, Inches(5.05), Inches(2.85), Inches(0.08), Inches(1.35), GREEN)
    write_lines(
        s,
        Inches(0.9),
        Inches(2.0),
        Inches(2.3),
        Inches(0.7),
        [
            {"text": "Nửa trái", "size": 14, "bold": True, "color": RED},
            {"text": "Giữ = Gumiho cắn", "size": 13, "color": MUTED, "before": 2},
        ],
    )
    write_lines(
        s,
        Inches(4.7),
        Inches(2.0),
        Inches(2.5),
        Inches(0.7),
        [
            {"text": "Nửa phải", "size": 14, "bold": True, "color": GREEN, "align": PP_ALIGN.RIGHT},
            {"text": "Giữ = Hổ cắn", "size": 13, "color": MUTED, "align": PP_ALIGN.RIGHT, "before": 2},
        ],
    )
    write_lines(
        s,
        Inches(0.85),
        Inches(5.5),
        Inches(6.6),
        Inches(1.05),
        [
            {
                "text": "Hai vùng neo ở hai mép, chừa khoảng trống ở tâm. Ngón trượt qua giữa thì hủy nhát.",
                "size": 14,
                "color": INK,
            }
        ],
    )
    points = [
        ("Cùng một mâm", "Món trôi từ vùng Gumiho sang vùng Hổ. Aim vẫn là việc chờ và phanh."),
        ("Món đổi nghĩa", "Món Gumiho thèm có thể là món Hổ chưa thèm. Phải cắn trước khi nó tới phía kia."),
        ("Ba màn, ba bước", "Màn 18 làm quen hai vùng. Màn 19 mỗi món chỉ vào đúng vùng người thèm. Màn 20 mới có món xung đột."),
        ("Tay không che mâm", "Ngón đặt trên hình khách ở mép. Mắt nhìn mâm ở giữa."),
    ]
    for i, (head, body) in enumerate(points):
        y = Inches(1.55 + i * 1.32)
        card = add_round(s, Inches(8.0), y, Inches(4.85), Inches(1.2), WHITE, radius=0.1)
        fill_shape_text(
            card,
            [
                {"text": head, "size": 16, "bold": True, "color": RED},
                {"text": body, "size": 13, "color": INK, "before": 3},
            ],
            margin=0.12,
        )
    footer(s, 9, TOTAL)
    notes(
        s,
        "Màn hai khách là màn người xem nhớ. Chỉ dùng ở màn 18, 19 và 20. Mê không vào các màn này. "
        "Một ngón, không bấm hai tay cùng lúc trong bản nộp.",
    )


def slide_levels(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Hai mươi màn", "Mỗi khách có một màn giới thiệu, mâm đứng yên. Màn 1–11 là phần thi, màn 12–20 là phần thêm.")
    lv = [
        ("1", "Gumiho", "Đứng", "Dạy vòng. Món cấm nằm sát món thèm.", "2", RED),
        ("2", "Gumiho", "Đứng", "Thêm món cấm. Đọc vòng trước khi thả.", "2", RED),
        ("3", "Gumiho", "10°/s", "Mâm xoay chậm. Chờ món vào vòng.", "3", RED),
        ("4", "Gumiho", "16°/s", "Hai món thèm, phải cắn hai lần.", "2", RED),
        ("5", "Hổ", "Đứng", "Dạy khe: hẹp khi chạm, rộng khi giữ.", "2", GREEN),
        ("6", "Hổ", "14°/s", "Món cấm gần món thèm, khe vừa đủ.", "2", GREEN),
        ("7", "Hổ", "20°/s", "Chờ món vào đúng đường tâm.", "3", GREEN),
        ("8", "Mê", "Đứng", "Dạy Mê. Giữ quá nửa thì lướt.", "3", BLUE),
        ("9", "Mê", "16°/s", "Mỗi nhát dài đổi mép đứng.", "3", BLUE),
        ("10", "Mê", "22°/s", "Tính trước nhát nào lướt, nhát nào đứng.", "4", BLUE),
        ("11", "Gumiho", "38°/s", "Mâm nhanh. Bốn món, mỗi nhát một món.", "4", RED),
        ("12", "Hổ", "40°/s", "Bốn món rải khắp mâm, khe đúng nhịp.", "5", GREEN),
        ("13", "Cò", "Đứng", "Dạy Cò. Chấm là độ sâu, không phải bề rộng.", "3", TEAL),
        ("14", "Cò", "24°/s", "Cò ngồi lệch. Bốn món đi qua mỏ.", "4", TEAL),
        ("15", "Dokkaebi", "Đứng", "Dạy Dokkaebi. Icon đổi sau nhát đầu.", "5", FIRE),
        ("16", "Dokkaebi", "20°/s", "Đổi sau nhát đầu: món cấm thành món thèm.", "5", FIRE),
        ("17", "Dokkaebi", "26°/s", "Đổi sau nhát thứ hai.", "4", FIRE),
        ("18", "Gumiho + Hổ", "Đứng", "Dạy hai khách. Món người này là món cấm của người kia.", "3", GOLD),
        ("19", "Gumiho + Hổ", "22°/s", "Bốn nhát luân phiên giữa hai khách.", "4", GOLD),
        ("20", "Gumiho + Hổ", "32°/s", "Có món chung, và món chỉ một người được ăn.", "4", GOLD),
    ]
    softs = {RED: SOFT_RED, GREEN: SOFT_GREEN, BLUE: SOFT_BLUE, TEAL: SOFT_TEAL, FIRE: SOFT_FIRE, GOLD: SOFT_GOLD}
    widths = [0.5, 1.35, 0.75, 3.1, 0.55]
    for half in range(2):
        rows = lv[half * 10:(half + 1) * 10]
        x0 = Inches(0.4 + half * 6.35)
        table = s.shapes.add_table(11, 5, x0, Inches(1.55), Inches(6.25), Inches(5.4)).table
        for i, w in enumerate(widths):
            table.columns[i].width = Inches(w)
        for j, text in enumerate(["Màn", "Khách", "Mâm", "Ý đồ", "Lượt"]):
            cell = table.cell(0, j)
            set_cell_fill(cell, INK)
            set_cell_text(cell, text, 11, True, CREAM, PP_ALIGN.LEFT if j == 3 else PP_ALIGN.CENTER)
        table.rows[0].height = Inches(0.34)
        for r, (num, guest, spin, aim, charges, col) in enumerate(rows, start=1):
            aligns = [PP_ALIGN.CENTER, PP_ALIGN.LEFT, PP_ALIGN.CENTER, PP_ALIGN.LEFT, PP_ALIGN.CENTER]
            for c, text in enumerate([num, guest, spin, aim, charges]):
                cell = table.cell(r, c)
                set_cell_fill(cell, softs[col])
                set_cell_text(cell, text, 11, c in (0, 1), (DARK_GOLD if col == GOLD else col) if c == 1 else INK, aligns[c])
            table.rows[r].height = Inches(0.5)
    footer(s, 10, TOTAL)
    notes(
        s,
        "Xếp cụm món trước, gán thèm và chưa thèm sau. Mỗi màn có một lời giải đã tính sẵn; số lượt bằng số nhát của lời giải, "
        "hoặc dư một lượt ở màn giới thiệu. Mạng là 3 ở màn giới thiệu, 2 ở các màn còn lại. "
        "Đổi món chỉ có ở màn Dokkaebi. Tiêu chí: người lạ qua màn 1 trong khoảng 20 giây, không cần đoạn văn hướng dẫn.",
    )


def slide_culture(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Văn hóa và bản quyền", "Hai nền ẩm thực đều đáng thèm. Món tối trên icon là món chưa tới lượt, không phải món bị chê.")
    left = [
        ("Mâm xoay là cách chơi", "Người chơi đãi khách bằng cách canh miếng, đúng với mâm xoay và bàn ăn chung."),
        ("Hổ là nhân vật cầu nối", "Hổ có trong tranh dân gian cả Việt và Hàn. Khe hai hàm là câu đố mà vòng tròn không làm được."),
        ("Khách tự vẽ", "Hồ ly, hổ tranh dân gian, chó đội nón mê, cò, dokkaebi. Cơ chế vật xoay quen thuộc, nhưng hình, nhân vật và luật thèm đều của đội."),
    ]
    for i, (head, body) in enumerate(left):
        y = Inches(1.55 + i * 1.7)
        card = add_round(s, Inches(0.45), y, Inches(7.55), Inches(1.55), WHITE, radius=0.08)
        fill_shape_text(
            card,
            [
                {"text": head, "size": 18, "bold": True, "color": RED},
                {"text": body, "size": 15, "color": INK, "before": 6},
            ],
            margin=0.16,
        )
    add_round(s, Inches(8.2), Inches(1.55), Inches(4.7), Inches(5.15), INK, radius=0.08)
    write_lines(
        s,
        Inches(8.45),
        Inches(1.8),
        Inches(4.25),
        Inches(4.7),
        [
            {"text": "Phần tự làm", "size": 18, "bold": True, "color": GOLD},
            {"text": "Hình khách, món, mâm: tự vẽ.", "size": 15, "color": CREAM, "before": 12},
            {"text": "Tiếng cắn, tiếng mâm, nhạc: tự làm hoặc dùng file có giấy phép rõ.", "size": 15, "color": CREAM, "before": 8},
            {"text": "Code Unity của đội.", "size": 15, "color": CREAM, "before": 8},
            {"text": "Giữ giấy phép âm thanh trong thư mục tài liệu, để đối chiếu nếu BTC hỏi.", "size": 15, "color": CREAM, "before": 8},
            {"text": "Sketch tay dán ở slide sau. BTC tính đây là một lợi thế.", "size": 15, "bold": True, "color": GOLD, "before": 14},
        ],
    )
    footer(s, 11, TOTAL)


def slide_scope(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Phạm vi bài nộp", "Unity 2D, một scene. Bản chơi được trên Android và trên máy tính bằng chuột.")
    have = [
        "5 khách: Gumiho, Hổ, Mê, Cò, Dokkaebi. Vùng ăn đúng như slide trước.",
        "8 món, 20 màn xếp tay, đúng bảng lời giải.",
        "Màn hình tiêu đề, qua màn, và chơi lại.",
        "APK Android, bản chạy bằng chuột trên máy tính.",
        "Video 45–75 giây: màn 1, một lần phanh, một lần Mê lướt, màn 20.",
        "PowerPoint này, sau khi điền tên và dán sketch.",
    ]
    later = [
        "Khách quạt, vùng mở góc như nón lá.",
        "Món chồng hai lớp.",
        "Món dính, ăn một cái kéo theo món kề bên.",
        "Thêm khách mới: giữ cử chỉ cũ, chỉ đổi hình vùng ăn.",
    ]
    add_round(s, Inches(0.45), Inches(1.5), Inches(7.7), Inches(5.3), WHITE, radius=0.08)
    write_lines(
        s,
        Inches(0.7),
        Inches(1.68),
        Inches(7.2),
        Inches(0.4),
        [{"text": "Có trong bài nộp", "size": 18, "bold": True, "color": GREEN}],
    )
    write_lines(
        s,
        Inches(0.7),
        Inches(2.2),
        Inches(7.15),
        Inches(4.3),
        [{"text": "•  " + line, "size": 15, "color": INK, "before": 8} for line in have],
    )
    add_round(s, Inches(8.35), Inches(1.5), Inches(4.55), Inches(5.3), WHITE, radius=0.08)
    write_lines(
        s,
        Inches(8.55),
        Inches(1.68),
        Inches(4.15),
        Inches(0.7),
        [{"text": "Để dành cho vòng chung kết", "size": 18, "bold": True, "color": RED}],
    )
    write_lines(
        s,
        Inches(8.55),
        Inches(2.55),
        Inches(4.15),
        Inches(3.8),
        [{"text": "•  " + line, "size": 15, "color": INK, "before": 10} for line in later],
    )
    footer(s, 12, TOTAL)
    notes(
        s,
        "Nếu trễ lịch, giảm animation trước khi bỏ màn. Giữ màn 20. "
        "Đó là màn hai nước ngồi cùng bàn.",
    )


def slide_sketch(prs):
    s = new_slide(prs)
    content_bg(s)
    title(s, "Chỗ dán phác thảo tay", "Vẽ chì hoặc mực, chụp rõ, dán đè lên từng khung. Máy không thay được nét tay của đội.")
    frames = [
        ("Gumiho", "Mặt, đuôi, món tròn khách đang thèm"),
        ("Hổ", "Hổ tranh dân gian, hai hàm / hai sọc"),
        ("Mê", "Chó trắng đội nón mê, đang cúi xuống mâm"),
        ("Mâm", "Nhìn từ trên, món Việt xen món Hàn"),
    ]
    for i, (name, hint) in enumerate(frames):
        x = Inches(0.42 + i * 3.22)
        add_round(s, x, Inches(1.55), Inches(3.05), Inches(4.15), WHITE, line=GOLD, radius=0.06)
        add_rect(s, x, Inches(1.55), Inches(3.05), Inches(0.12), RED if i % 2 == 0 else GREEN)
        write_lines(
            s,
            x + Inches(0.15),
            Inches(2.3),
            Inches(2.75),
            Inches(2.2),
            [
                {"text": name, "size": 22, "bold": True, "color": RED, "align": PP_ALIGN.CENTER},
                {"text": "Dán sketch vào khung này", "size": 14, "color": MUTED, "align": PP_ALIGN.CENTER, "before": 8},
            ],
        )
        write_lines(
            s,
            x + Inches(0.12),
            Inches(5.8),
            Inches(2.8),
            Inches(0.85),
            [{"text": hint, "size": 13, "color": INK, "align": PP_ALIGN.CENTER}],
        )
    footer(s, 13, TOTAL)


def slide_end(prs):
    s = new_slide(prs)
    add_rect(s, 0, 0, SLIDE_W, SLIDE_H, INK)
    add_rect(s, 0, 0, Inches(0.28), SLIDE_H, RED)
    add_rect(s, Inches(0.28), 0, Inches(0.08), SLIDE_H, GOLD)
    write_lines(
        s,
        Inches(0.85),
        Inches(0.7),
        Inches(11.5),
        Inches(0.8),
        [{"text": "Một Mâm", "size": 48, "bold": True, "color": CREAM}],
    )
    write_lines(
        s,
        Inches(0.85),
        Inches(1.6),
        Inches(11),
        Inches(0.8),
        [{"text": "Giữ mâm. Đãi đúng người. Đừng để khách ăn nhầm món.", "size": 22, "color": GOLD}],
    )
    steps = [
        "1    Đăng ký đúng link chính thức của BTC.",
        "2    Điền tên đội, thành viên, email ở slide thông tin.",
        "3    Vẽ tay và dán vào slide phác thảo.",
        "4    Hoàn thiện 20 màn trong Unity, xuất APK và bản chơi trên máy tính.",
        "5    Quay video 45–75 giây, thấy đủ Gumiho, Hổ, Mê và màn 20 hai khách.",
        "6    Gửi pptx kèm video hoặc APK trước hết ngày 11/11/2026.",
    ]
    write_lines(
        s,
        Inches(0.9),
        Inches(2.6),
        Inches(11.2),
        Inches(3.3),
        [{"text": line, "size": 20, "color": CREAM, "before": 8} for line in steps],
    )
    write_lines(
        s,
        Inches(0.9),
        Inches(6.3),
        Inches(11.2),
        Inches(0.6),
        [
            {
                "text": "btc.koreagameweek@gmail.com     ·     [Dự thi Lập trình game KGW2026]",
                "size": 16,
                "bold": True,
                "color": GOLD,
            }
        ],
    )
    notes(
        s,
        "Bài nói hai phút nằm trong tài liệu thiết kế, mục 13. "
        "Khi ban giám khảo đổi đề, giữ cử chỉ giữ và thả, chỉ đổi hình vùng ăn hoặc tốc độ mâm.",
    )


def build():
    prs = Presentation()
    prs.slide_width = SLIDE_W
    prs.slide_height = SLIDE_H
    prs.core_properties.title = "Một Mâm — Ý tưởng dự thi lập trình game KGW 2026"
    prs.core_properties.subject = "Casual game mâm tiệc Việt Nam – Hàn Quốc"
    prs.core_properties.category = "Game design"
    slide_cover(prs)
    slide_info(prs)
    slide_inspiration(prs)
    slide_idea(prs)
    slide_setting(prs)
    slide_cast(prs)
    slide_play(prs)
    slide_skills(prs)
    slide_duo(prs)
    slide_levels(prs)
    slide_culture(prs)
    slide_scope(prs)
    slide_sketch(prs)
    slide_end(prs)
    prs.save(OUT)
    print(f"Saved {OUT} ({len(prs.slides)} slides)")


if __name__ == "__main__":
    build()
