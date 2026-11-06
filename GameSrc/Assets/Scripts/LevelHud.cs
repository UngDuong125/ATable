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

        Vector2 iconHomePos;
        Vector2 iconHomeSize;
        Vector2 nameHomeSize;
        int nameHomeFont;
        float iconSpacing;
        bool dualIcons;
        bool remembered;
        Text gumihoCaption;
        Text hoCaption;

        public Font UiFont
        {
            get
            {
                if (levelText != null && levelText.font != null) return levelText.font;
                if (guestName != null && guestName.font != null) return guestName.font;
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        void Awake()
        {
            RememberRow();
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
            if (dualIcons) EnsureHoRow();
            if (guestName != null)
            {
                if (dualIcons)
                {
                    guestName.rectTransform.sizeDelta = new Vector2(920f, 72f);
                    guestName.fontSize = 34;
                    guestName.text = "Trái · Gumiho     Phải · Hổ";
                    guestName.color = Art.Ink;
                }
                else
                {
                    guestName.rectTransform.sizeDelta = nameHomeSize;
                    guestName.fontSize = nameHomeFont;
                    guestName.text = Art.GuestLabel(mode);
                    guestName.color = Art.Accent(Art.GuestOf(mode));
                }
            }

            LayoutRows(dualIcons && iconRowHo != null);
            ShowSideCaptions(dualIcons && iconRowHo != null);
            if (dualIcons)
            {
                Fill(iconRow, foods, GuestId.Gumiho, true);
                if (iconRowHo != null) Fill(iconRowHo, foods, GuestId.Ho, true);
                else Fill(iconRow, foods, GuestId.Ho, true, false);
            }
            else
            {
                Fill(iconRow, foods, Art.GuestOf(mode), false);
            }

            SetIconsVisible(true);
        }

        public void SetIconsVisible(bool visible)
        {
            if (iconRow != null) iconRow.gameObject.SetActive(visible);
            if (iconRowHo != null) iconRowHo.gameObject.SetActive(visible && dualIcons);
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

        void LayoutRows(bool dual)
        {
            if (iconRow == null) return;
            var layout = iconRow.GetComponent<HorizontalLayoutGroup>();
            if (dual)
            {
                iconRow.anchoredPosition = new Vector2(-250f, 36f);
                iconRow.sizeDelta = new Vector2(460f, 160f);
                if (iconRowHo != null)
                {
                    iconRowHo.anchorMin = iconRow.anchorMin;
                    iconRowHo.anchorMax = iconRow.anchorMax;
                    iconRowHo.pivot = iconRow.pivot;
                    iconRowHo.anchoredPosition = new Vector2(250f, 36f);
                    iconRowHo.sizeDelta = new Vector2(460f, 160f);
                }
            }
            else
            {
                iconRow.anchoredPosition = iconHomePos;
                iconRow.sizeDelta = iconHomeSize;
            }

            SetLayoutCompact(layout, dual);
            if (layout != null) layout.spacing = dual ? 8f : iconSpacing;
            if (iconRowHo != null)
            {
                var hoLayout = iconRowHo.GetComponent<HorizontalLayoutGroup>();
                SetLayoutCompact(hoLayout, dual);
                if (hoLayout != null) hoLayout.spacing = dual ? 8f : iconSpacing;
            }
        }

        static void SetLayoutCompact(HorizontalLayoutGroup layout, bool compact)
        {
            if (layout == null) return;
            layout.childControlWidth = compact;
            layout.childControlHeight = compact;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        void Fill(RectTransform row, IReadOnlyList<FoodItem> foods, GuestId guest, bool compact, bool clear = true)
        {
            if (row == null || iconPrefab == null) return;
            if (clear)
            {
                for (int i = row.childCount - 1; i >= 0; i--)
                    Destroy(row.GetChild(i).gameObject);
            }

            for (int i = 0; i < foods.Count; i++)
            {
                var food = foods[i];
                bool forbid = food.ForbiddenTo(guest);
                bool want = food.WantedBy(guest);
                if (!forbid && !want) continue;

                var icon = Instantiate(iconPrefab, row);
                icon.gameObject.SetActive(true);
                var rect = (RectTransform)icon.transform;
                if (compact) rect.sizeDelta = new Vector2(148f, 124f);
                var element = icon.GetComponent<LayoutElement>();
                if (element == null) element = icon.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = rect.sizeDelta.x;
                element.preferredHeight = rect.sizeDelta.y;
                element.minWidth = element.preferredWidth;
                element.minHeight = element.preferredHeight;
                icon.Show(food.dishId, want && !forbid, food.Portrait, Art.Accent(guest), compact);
            }
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
            var homeLayout = iconRow.GetComponent<HorizontalLayoutGroup>();
            if (homeLayout != null) iconSpacing = homeLayout.spacing;
            remembered = true;
        }

        void EnsureHoRow()
        {
            if (iconRowHo != null || iconRow == null) return;
            var copy = Instantiate(iconRow.gameObject, iconRow.parent);
            copy.name = "IconsHo";
            for (int i = copy.transform.childCount - 1; i >= 0; i--)
                Destroy(copy.transform.GetChild(i).gameObject);
            iconRowHo = copy.GetComponent<RectTransform>();
        }

        void ShowSideCaptions(bool dual)
        {
            if (dual)
            {
                gumihoCaption = Caption(gumihoCaption, "Gumiho", Art.Son, new Vector2(-250f, 198f));
                hoCaption = Caption(hoCaption, "Hổ", Art.Leaf, new Vector2(250f, 198f));
            }
            if (gumihoCaption != null) gumihoCaption.gameObject.SetActive(dual);
            if (hoCaption != null) hoCaption.gameObject.SetActive(dual);
        }

        Text Caption(Text current, string label, Color color, Vector2 pos)
        {
            if (current != null)
            {
                current.rectTransform.anchoredPosition = pos;
                return current;
            }
            var go = new GameObject(label + "Caption", typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(280f, 40f);
            rect.anchoredPosition = pos;
            var text = go.GetComponent<Text>();
            text.font = UiFont;
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = label;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
