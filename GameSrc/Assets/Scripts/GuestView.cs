using UnityEngine;

namespace MotMam
{
    public class GuestView : MonoBehaviour
    {
        Vector3 homeScale;

        void Awake()
        {
            homeScale = transform.localScale;
            if (homeScale.sqrMagnitude < 0.0001f) homeScale = Vector3.one;
        }

        public void Face(float anchorDeg)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, anchorDeg + 90f);
        }

        public void SetTurnedAway(bool away)
        {
            float x = Mathf.Abs(homeScale.x);
            transform.localScale = away
                ? new Vector3(-x, homeScale.y, homeScale.z)
                : new Vector3(x, homeScale.y, homeScale.z);
        }
    }
}
