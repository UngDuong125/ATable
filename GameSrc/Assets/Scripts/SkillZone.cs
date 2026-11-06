using UnityEngine;

namespace MotMam
{
    public class SkillZone : MonoBehaviour
    {
        [SerializeField] Transform fill;

        void Awake()
        {
            if (fill == null) fill = transform;
        }

        public void PlaceCircle(Vector2 worldCenter, float radiusNorm)
        {
            if (fill == null) fill = transform;
            var position = transform.position;
            transform.position = new Vector3(worldCenter.x, worldCenter.y, position.z);
            transform.rotation = Quaternion.identity;
            float diameter = radiusNorm * 2f * GameRules.TrayWorldRadius;
            fill.localScale = new Vector3(diameter, diameter, 1f);
            if (fill != transform)
            {
                fill.localPosition = Vector3.zero;
                fill.localRotation = Quaternion.identity;
            }
        }

        // Dải chữ nhật: trục dọc nằm dọc tia neo, tâm tại centerAlong.
        // perpOffset đẩy tâm sang phía ngược chiều kim đồng hồ, đơn vị bán kính mâm.
        public void PlaceBand(float anchorDeg, float centerAlong, float lengthNorm, float widthNorm, float perpOffset = 0f)
        {
            if (fill == null) fill = transform;
            float a = anchorDeg * Mathf.Deg2Rad;
            Vector2 center = GameRules.ToWorld(GameRules.Point(anchorDeg, centerAlong));
            center += new Vector2(-Mathf.Sin(a), Mathf.Cos(a)) * (perpOffset * GameRules.TrayWorldRadius);
            var position = transform.position;
            transform.position = new Vector3(center.x, center.y, position.z);
            transform.rotation = Quaternion.Euler(0f, 0f, anchorDeg - 90f);
            fill.localScale = new Vector3(
                widthNorm * GameRules.TrayWorldRadius,
                lengthNorm * GameRules.TrayWorldRadius,
                1f);
            if (fill != transform)
            {
                fill.localPosition = Vector3.zero;
                fill.localRotation = Quaternion.identity;
            }
        }
    }
}
