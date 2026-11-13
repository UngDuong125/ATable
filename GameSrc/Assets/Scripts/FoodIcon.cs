using UnityEngine;
using UnityEngine.UI;

namespace MotMam
{
    public class FoodIcon : MonoBehaviour
    {
        [SerializeField] Image swatch;
        [SerializeField] Text label;
        [SerializeField] Outline outline;

        public void Show(string dishId, bool wanted, Sprite sprite, Color accent, bool compact)
        {
            if (swatch != null)
            {
                if (sprite != null) swatch.sprite = sprite;
                swatch.color = wanted ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
            }
            if (label != null)
            {
                label.text = Art.DishLabel(dishId);
                label.color = wanted ? Art.Ink : new Color(Art.Ink.r, Art.Ink.g, Art.Ink.b, 0.55f);
                if (compact)
                {
                    label.resizeTextForBestFit = true;
                    label.resizeTextMinSize = 12;
                    label.resizeTextMaxSize = 20;
                    label.fontSize = 20;
                    label.rectTransform.sizeDelta = new Vector2(140f, 40f);
                }
            }
            if (outline != null)
            {
                outline.enabled = wanted;
                outline.effectColor = accent;
            }
        }

        // Xếp lại hình và chữ cho vừa ô nhỏ (thẻ món màn hai khách): hình ở trên, chữ ở dưới, không chồng nhau.
        public void FitTo(Vector2 size)
        {
            float labelHeight = Mathf.Round(size.y * 0.28f);
            if (label != null)
            {
                var rect = label.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 2f);
                rect.sizeDelta = new Vector2(size.x, labelHeight);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 12;
                label.resizeTextMaxSize = Mathf.Max(12, Mathf.RoundToInt(labelHeight * 0.75f));
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
            }
            if (swatch != null)
            {
                float side = Mathf.Min(size.x * 0.75f, size.y - labelHeight - 8f);
                var rect = swatch.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -4f);
                rect.sizeDelta = new Vector2(side, side);
                swatch.preserveAspect = true;
            }
        }
    }
}
