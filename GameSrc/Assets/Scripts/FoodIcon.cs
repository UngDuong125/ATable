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
    }
}
