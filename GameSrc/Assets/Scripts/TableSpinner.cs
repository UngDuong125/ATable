using UnityEngine;

namespace MotMam
{
    [DefaultExecutionOrder(-50)]
    public class TableSpinner : MonoBehaviour
    {
        public Transform Tray;
        public float SpinSpeed;
        public float BrakeTime = GameRules.BrakeTime;
        public bool Paused;

        public float Angle { get; private set; }
        public float HoldSeconds { get; private set; }
        public bool Holding { get; private set; }
        public bool Locked => lockLeft > 0f;

        float lockLeft;

        public void ResetSpin()
        {
            Angle = 0f;
            HoldSeconds = 0f;
            Holding = false;
            lockLeft = 0f;
            Paused = false;
            Apply();
        }

        public void BeginHold()
        {
            if (Locked || Paused) return;
            Holding = true;
            HoldSeconds = 0f;
        }

        public void EndHold()
        {
            Holding = false;
        }

        public void Lock(float seconds)
        {
            Holding = false;
            HoldSeconds = 0f;
            lockLeft = seconds;
        }

        void Update()
        {
            if (Paused) return;

            float dt = Time.deltaTime;
            if (Locked)
            {
                lockLeft -= dt;
                return;
            }

            float speed = SpinSpeed;
            if (Holding)
            {
                HoldSeconds += dt;
                float brake = BrakeTime > 0f ? BrakeTime : GameRules.BrakeTime;
                speed *= 1f - Mathf.Clamp01(HoldSeconds / brake);
            }

            Angle += speed * dt;
            Apply();
        }

        void Apply()
        {
            if (Tray != null) Tray.rotation = Quaternion.Euler(0f, 0f, Angle);
        }
    }
}
