using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MotMam
{
    public class LevelHud : MonoBehaviour
    {
        [SerializeField] Text levelText;
        [SerializeField] Text heartsText;
        [SerializeField] Text chargesText;
        [SerializeField] Text guestName;
        [SerializeField] Image holdFill;
        [SerializeField] RectTransform iconRow;
        [SerializeField] RectTransform iconRowHo;
        [SerializeField] FoodIcon iconPrefab;
        [SerializeField] GameObject resultRoot;
        [SerializeField] Text resultText;
        [SerializeField] Text buttonText;

        // Thẻ món ở màn hai khách: mỗi khách một thẻ màu riêng, chia hàng "Đang thèm" và "Không ăn".
        const float CardWidth = 486f;
        const float CardGap = 24f;
        const float CardBottom = 20f;
        const float CardHeader = 50f;
        const float CardPad = 12f;
        const float WantRowHeight = 128f;
        const float ForbidRowHeight = 100f;
        const float RowLabelHeight = 30f;
        static readonly Vector2 WantIcon = new(148f, 124f);
        static readonly Vector2 ForbidIcon = new(112f, 96f);

        class DualCard
        {
            public RectTransform root;
            public RectTransform wantRow;
            public RectTransform forbidRow;
            public Text forbidLabel;
        }

        Vector2 iconHomePos;
        Vector2 iconHomeSize;
        Vector2 nameHomeSize;
        int nameHomeFont;
        bool dualIcons;
        bool remembered;
        DualCard gumihoCard;
        DualCard hoCard;
        RectTransform holdBar;
        Vector2 holdHomePos;

        public Font UiFont
        {
            get
            {
                if (levelText != null && levelText.font != null) return levelText.font;
                if (guestName != null && guestName.font != null) return guestName.font;
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        CanvasScaler scaler;

        void Awake()
        {
            RememberRow();
            scaler = GetComponentInParent<CanvasScaler>();
            FitScaler();
        }

        void Update()
        {
            FitScaler();
        }

        // Thiết kế gốc là 1080x1920 (dọc). Màn rộng hơn 9:16 (PC, tablet) thì khớp theo
        // chiều cao để các mốc tính từ đáy (thanh giữ, icon) nằm cùng tỉ lệ với khách như trên mobile;
        // màn hẹp hơn 9:16 thì khớp theo chiều rộng để hàng icon không bị cắt.
        void FitScaler()
        {
            if (scaler == null || Screen.height <= 0) return;
            float aspect = (float)Screen.width / Screen.height;
            scaler.matchWidthOrHeight = aspect >= 1080f / 1920f ? 1f : 0f;
        }

        public void SetStats(int levelId, int hearts, int charges)
        {
            if (levelText != null) levelText.text = "Màn " + levelId;
            if (heartsText != null) heartsText.text = "Mạng " + hearts;
            if (chargesText != null) chargesText.text = "Lượt " + charges;
        }

        public void SetHold(float u, Color tint)
        {
            if (holdFill == null) return;
            holdFill.fillAmount = Mathf.Clamp01(u);
            holdFill.color = tint;
        }

        public void ShowFoods(IReadOnlyList<FoodItem> foods, GuestMode mode)
        {
            RememberRow();
            dualIcons = mode == GuestMode.GumihoHo;
            if (guestName != null)
            {
                guestName.rectTransform.sizeDelta = nameHomeSize;
                guestName.fontSize = nameHomeFont;
                // Màn hai khách: tên đã nằm trên đầu từng thẻ.
                guestName.text = dualIcons ? "" : Art.GuestLabel(mode);
                guestName.color = Art.Accent(Art.GuestOf(mode));
            }

            if (dualIcons)
            {
                if (gumihoCard == null) gumihoCard = BuildCard(GuestId.Gumiho, "Gumiho · nửa trái", -1f);
                if (hoCard == null) hoCard = BuildCard(GuestId.Ho, "Hổ · nửa phải", 1f);
                FillCard(gumihoCard, foods, GuestId.Gumiho);
                FillCard(hoCard, foods, GuestId.Ho);
            }
            else
            {
                if (iconRow != null)
                {
                    iconRow.anchoredPosition = iconHomePos;
                    iconRow.sizeDelta = iconHomeSize;
                }
                Fill(iconRow, foods, Art.GuestOf(mode));
            }

            PlaceHoldBar();
            SetIconsVisible(true);
        }

        Text hintText;
        Image hintBack;
        string hintMessage;

        public void SetHint(string message)
        {
            hintMessage = message;
            if (string.IsNullOrEmpty(message) && hintBack != null) hintBack.gameObject.SetActive(false);
        }

        // Gọi mỗi khung: hiện gợi ý khi có chữ và đang chơi.
        public void TickHint(bool visible)
        {
            bool on = visible && !string.IsNullOrEmpty(hintMessage);
            if (!on)
            {
                if (hintBack != null && hintBack.gameObject.activeSelf) hintBack.gameObject.SetActive(false);
                return;
            }
            if (hintBack == null) BuildHint();
            if (!hintBack.gameObject.activeSelf) hintBack.gameObject.SetActive(true);
            hintText.text = hintMessage;
            float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 3f);
            hintText.color = new Color(Art.Ink.r, Art.Ink.g, Art.Ink.b, pulse);
        }

        void BuildHint()
        {
            var back = new GameObject("Hint", typeof(RectTransform), typeof(Image));
            var rect = back.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(860f, 96f);
            rect.anchoredPosition = new Vector2(0f, -170f);
            hintBack = back.GetComponent<Image>();
            hintBack.color = new Color(Art.Paper.r, Art.Paper.g, Art.Paper.b, 0.88f);
            hintBack.raycastTarget = false;

            var label = new GameObject("HintText", typeof(RectTransform), typeof(Text));
            var lr = label.GetComponent<RectTransform>();
            lr.SetParent(rect, false);
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(20f, 8f);
            lr.offsetMax = new Vector2(-20f, -8f);
            hintText = label.GetComponent<Text>();
            hintText.font = UiFont;
            hintText.fontSize = 34;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.resizeTextForBestFit = true;
            hintText.resizeTextMinSize = 20;
            hintText.resizeTextMaxSize = 34;
            hintText.raycastTarget = false;
        }

        // Khung màn hình (pixel) của phần HUD mà hướng dẫn muốn chiếu sáng.
        public bool TryFocusRect(TutorialFocus focus, out Rect screen)
        {
            screen = default;
            switch (focus)
            {
                case TutorialFocus.Icons:
                    if (dualIcons && gumihoCard != null && hoCard != null)
                        return Union(gumihoCard.root, hoCard.root, out screen);
                    return Union(iconRow, null, out screen);
                case TutorialFocus.Stats:
                    return Union(heartsText != null ? heartsText.rectTransform : null,
                        chargesText != null ? chargesText.rectTransform : null, out screen);
                case TutorialFocus.HoldBar:
                    return Union(HoldBar(), null, out screen);
                default:
                    return false;
            }
        }

        static readonly Vector3[] corners = new Vector3[4];

        static bool Union(RectTransform a, RectTransform b, out Rect screen)
        {
            screen = default;
            bool any = false;
            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;
            foreach (var rect in new[] { a, b })
            {
                if (rect == null || !rect.gameObject.activeInHierarchy) continue;
                rect.GetWorldCorners(corners);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 p = corners[i];
                    if (!any) { min = max = p; any = true; }
                    min = Vector2.Min(min, p);
                    max = Vector2.Max(max, p);
                }
            }
            if (!any) return false;
            float pad = Screen.height * 0.015f;
            screen = Rect.MinMaxRect(min.x - pad, min.y - pad, max.x + pad, max.y + pad);
            return true;
        }

        public void SetIconsVisible(bool visible)
        {
            if (iconRow != null) iconRow.gameObject.SetActive(visible && !dualIcons);
            if (iconRowHo != null) iconRowHo.gameObject.SetActive(false);
            if (gumihoCard != null) gumihoCard.root.gameObject.SetActive(visible && dualIcons);
            if (hoCard != null) hoCard.root.gameObject.SetActive(visible && dualIcons);
        }

        public void ShowResult(string message, string button)
        {
            if (resultText != null) resultText.text = message;
            if (buttonText != null) buttonText.text = button;
            if (resultRoot != null) resultRoot.SetActive(true);
        }

        public void HideResult()
        {
            if (resultRoot != null) resultRoot.SetActive(false);
        }

        void Fill(RectTransform row, IReadOnlyList<FoodItem> foods, GuestId guest)
        {
            if (row == null || iconPrefab == null) return;
            Clear(row);

            for (int i = 0; i < foods.Count; i++)
            {
                var food = foods[i];
                bool forbid = food.ForbiddenTo(guest);
                bool want = food.WantedBy(guest);
                if (!forbid && !want) continue;

                var icon = Instantiate(iconPrefab, row);
                icon.gameObject.SetActive(true);
                icon.Show(food.dishId, want && !forbid, food.Portrait, Art.Accent(guest), false);
            }
        }

        // Thẻ một khách: món thèm to, có viền màu khách; món không được ăn nhỏ hơn, xám.
        // Món trùng tên chỉ hiện một lần cho gọn.
        void FillCard(DualCard card, IReadOnlyList<FoodItem> foods, GuestId guest)
        {
            if (card == null || iconPrefab == null) return;
            Clear(card.wantRow);
            Clear(card.forbidRow);

            var wants = new List<FoodItem>();
            var forbids = new List<FoodItem>();
            var seenWant = new HashSet<string>();
            var seenForbid = new HashSet<string>();
            for (int i = 0; i < foods.Count; i++)
            {
                var food = foods[i];
                bool forbid = food.ForbiddenTo(guest);
                bool want = food.WantedBy(guest) && !forbid;
                if (want && seenWant.Add(food.dishId)) wants.Add(food);
                else if (forbid && seenForbid.Add(food.dishId)) forbids.Add(food);
            }

            Color accent = Art.Accent(guest);
            AddIcons(card.wantRow, wants, true, accent, WantIcon);
            AddIcons(card.forbidRow, forbids, false, accent, ForbidIcon);
            card.forbidLabel.gameObject.SetActive(forbids.Count > 0);
        }

        void AddIcons(RectTransform row, List<FoodItem> items, bool wanted, Color accent, Vector2 baseSize)
        {
            if (items.Count == 0) return;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            float spacing = layout != null ? layout.spacing : 8f;
            float room = CardWidth - CardPad * 2f;
            float width = Mathf.Min(baseSize.x, (room - spacing * (items.Count - 1)) / items.Count);
            var size = new Vector2(width, baseSize.y * width / baseSize.x);

            for (int i = 0; i < items.Count; i++)
            {
                var food = items[i];
                var icon = Instantiate(iconPrefab, row);
                icon.gameObject.SetActive(true);
                var rect = (RectTransform)icon.transform;
                rect.sizeDelta = size;
                var element = icon.GetComponent<LayoutElement>();
                if (element == null) element = icon.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = element.minWidth = size.x;
                element.preferredHeight = element.minHeight = size.y;
                icon.Show(food.dishId, wanted, food.Portrait, accent, true);
                icon.FitTo(size);
            }
        }

        static void Clear(RectTransform row)
        {
            if (row == null) return;
            for (int i = row.childCount - 1; i >= 0; i--)
                Destroy(row.GetChild(i).gameObject);
        }

        DualCard BuildCard(GuestId guest, string title, float side)
        {
            Color accent = Art.Accent(guest);
            float height = CardHeader + RowLabelHeight * 2f + WantRowHeight + ForbidRowHeight + CardPad * 2f;
            var card = new DualCard();

            card.root = UiChild(transform, guest + "Card");
            card.root.anchorMin = card.root.anchorMax = new Vector2(0.5f, 0f);
            card.root.pivot = new Vector2(0.5f, 0f);
            card.root.sizeDelta = new Vector2(CardWidth, height);
            card.root.anchoredPosition = new Vector2(side * (CardWidth + CardGap) * 0.5f, CardBottom);
            if (iconRow != null) card.root.SetSiblingIndex(iconRow.GetSiblingIndex());
            var back = card.root.gameObject.AddComponent<Image>();
            back.sprite = WhiteSprite();
            back.color = Color.Lerp(Art.Paper, accent, 0.12f);
            back.raycastTarget = false;
            var outline = card.root.gameObject.AddComponent<Outline>();
            outline.effectColor = accent;
            outline.effectDistance = new Vector2(4f, -4f);

            var header = UiChild(card.root, "Header");
            TopBand(header, 0f, CardHeader);
            var headerImage = header.gameObject.AddComponent<Image>();
            headerImage.sprite = WhiteSprite();
            headerImage.color = accent;
            headerImage.raycastTarget = false;
            var headerText = UiText(header, title, 32, Art.Paper, TextAnchor.MiddleCenter);
            headerText.fontStyle = FontStyle.Bold;

            float y = CardHeader;
            var wantLabel = UiChild(card.root, "WantLabel");
            TopBand(wantLabel, y, RowLabelHeight);
            UiText(wantLabel, "Đang thèm", 24, accent, TextAnchor.MiddleLeft).fontStyle = FontStyle.Bold;
            y += RowLabelHeight;

            card.wantRow = UiChild(card.root, "Want");
            TopBand(card.wantRow, y, WantRowHeight);
            Row(card.wantRow, 10f);
            y += WantRowHeight;

            var forbidLabel = UiChild(card.root, "ForbidLabel");
            TopBand(forbidLabel, y, RowLabelHeight);
            card.forbidLabel = UiText(forbidLabel, "Không ăn", 24, new Color(Art.Ink.r, Art.Ink.g, Art.Ink.b, 0.7f), TextAnchor.MiddleLeft);
            y += RowLabelHeight;

            card.forbidRow = UiChild(card.root, "Forbid");
            TopBand(card.forbidRow, y, ForbidRowHeight);
            Row(card.forbidRow, 8f);

            card.root.gameObject.SetActive(false);
            return card;
        }

        // Dải ngang tính từ mép trên của thẻ, chừa lề hai bên.
        static void TopBand(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            float pad = top <= 0f ? 0f : CardPad;
            rect.offsetMin = new Vector2(pad, -top - height);
            rect.offsetMax = new Vector2(-pad, -top);
        }

        static void Row(RectTransform rect, float spacing)
        {
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        static RectTransform UiChild(Transform parent, string childName)
        {
            var go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        Text UiText(RectTransform parent, string value, int size, Color color, TextAnchor align)
        {
            var rect = UiChild(parent, "Text");
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 0f);
            rect.offsetMax = new Vector2(-8f, 0f);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = UiFont;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            return text;
        }

        static Sprite white;

        static Sprite WhiteSprite()
        {
            if (white != null) return white;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            white = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return white;
        }

        RectTransform HoldBar()
        {
            if (holdBar != null) return holdBar;
            if (holdFill == null) return null;
            holdBar = holdFill.transform.parent as RectTransform;
            if (holdBar == null || holdBar == transform) holdBar = holdFill.rectTransform;
            holdHomePos = holdBar.anchoredPosition;
            return holdBar;
        }

        // Màn hai khách: thẻ món cao hơn hàng icon thường, nên đẩy thanh giữ lên trên thẻ.
        void PlaceHoldBar()
        {
            var bar = HoldBar();
            if (bar == null) return;
            if (!dualIcons)
            {
                bar.anchoredPosition = holdHomePos;
                return;
            }
            float cardTop = CardBottom + CardHeader + RowLabelHeight * 2f + WantRowHeight + ForbidRowHeight + CardPad * 2f;
            float lift = cardTop + 36f - holdHomePos.y;
            bar.anchoredPosition = holdHomePos + new Vector2(0f, Mathf.Max(0f, lift));
        }

        void RememberRow()
        {
            if (remembered || iconRow == null) return;
            iconHomePos = iconRow.anchoredPosition;
            iconHomeSize = iconRow.sizeDelta;
            if (guestName != null)
            {
                nameHomeSize = guestName.rectTransform.sizeDelta;
                nameHomeFont = guestName.fontSize;
            }
            remembered = true;
        }
    }
}
