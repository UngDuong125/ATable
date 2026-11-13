using UnityEngine;

namespace MotMam
{
    public class FoodItem : MonoBehaviour
    {
        [SerializeField] SpriteRenderer body;
        [SerializeField] GameObject wantRing;

        public string dishId;
        public GuestId[] want = System.Array.Empty<GuestId>();
        public GuestId[] forbid = System.Array.Empty<GuestId>();
        public Sprite Portrait { get; private set; }

        GuestId[] wantAfter = System.Array.Empty<GuestId>();
        GuestId[] forbidAfter = System.Array.Empty<GuestId>();
        Vector3 baseScale;
        public bool Eaten { get; private set; }
        public Vector3 BaseScale => baseScale;

        void Awake()
        {
            if (body == null) body = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
        }

        public void Apply(FoodPlacement placement, Sprite sprite, GuestMode mode)
        {
            dishId = placement.dishId;
            want = placement.want ?? System.Array.Empty<GuestId>();
            forbid = placement.forbid ?? System.Array.Empty<GuestId>();
            wantAfter = placement.wantAfterSwap ?? System.Array.Empty<GuestId>();
            forbidAfter = placement.forbidAfterSwap ?? System.Array.Empty<GuestId>();
            Portrait = sprite;
            Eaten = false;
            if (aimRing != null) aimRing.SetActive(false);
            gameObject.SetActive(true);

            if (body == null) body = GetComponent<SpriteRenderer>();
            if (body != null && sprite != null) body.sprite = sprite;
            RefreshLook(mode);
            baseScale = transform.localScale;
        }

        public void ApplySwap(GuestMode mode)
        {
            want = wantAfter;
            forbid = forbidAfter;
            RefreshLook(mode);
        }

        public void RefreshLook(GuestMode mode)
        {
            bool wanted = WantedBySeated(mode);
            if (body != null)
                body.color = wanted ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
            if (wantRing != null) wantRing.SetActive(wanted);
        }

        public bool WantedBy(GuestId guest)
        {
            return !Eaten && Has(want, guest);
        }

        public bool ForbiddenTo(GuestId guest)
        {
            return !Eaten && Has(forbid, guest);
        }

        public Vector2 ScreenNorm()
        {
            return new Vector2(transform.position.x, transform.position.y) / GameRules.TrayWorldRadius;
        }

        public void MarkEaten()
        {
            Eaten = true;
            if (wantRing != null) wantRing.SetActive(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetTint(Color color)
        {
            if (body != null) body.color = color;
        }

        GameObject aimRing;
        SpriteRenderer aimRenderer;

        // Vòng báo món sẽ bị cắn nếu thả ngay: xanh nếu khách thèm, đỏ nếu cấm.
        public void SetAim(int kind, Sprite ringSprite)
        {
            if (Eaten || kind == 0 || ringSprite == null)
            {
                if (aimRing != null && aimRing.activeSelf) aimRing.SetActive(false);
                return;
            }
            if (aimRing == null)
            {
                aimRing = new GameObject("AimRing");
                aimRing.transform.SetParent(transform, false);
                aimRenderer = aimRing.AddComponent<SpriteRenderer>();
                aimRenderer.sprite = ringSprite;
                aimRenderer.sortingOrder = (body != null ? body.sortingOrder : 5) + 1;
                float size = body != null && body.sprite != null ? body.sprite.bounds.size.x * 1.25f : 1.25f;
                aimRing.transform.localScale = new Vector3(size, size, 1f);
            }
            aimRing.SetActive(true);
            aimRenderer.color = kind > 0 ? new Color(0.25f, 0.78f, 0.42f, 0.95f) : new Color(0.9f, 0.22f, 0.18f, 0.95f);
        }

        public Color Tint => body != null ? body.color : Color.white;

        public void SetPulse(float amount)
        {
            if (Eaten) return;
            transform.localScale = baseScale * (1f + amount);
        }

        bool WantedBySeated(GuestMode mode)
        {
            if (Eaten) return false;
            if (mode == GuestMode.GumihoHo) return Has(want, GuestId.Gumiho) || Has(want, GuestId.Ho);
            return Has(want, Art.GuestOf(mode));
        }

        static bool Has(GuestId[] list, GuestId guest)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == guest) return true;
            }
            return false;
        }
    }
}
