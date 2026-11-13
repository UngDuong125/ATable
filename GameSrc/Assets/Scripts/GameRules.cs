using UnityEngine;

namespace MotMam
{
    public enum BiteOutcome
    {
        Miss,
        Eat,
        Foul
    }

    // Hằng số và vùng ăn đã chốt trong Mo-Mam-Vi-Tri-Mon-An.md.
    public static class GameRules
    {
        public const float HoldToMaxSeconds = 0.50f;
        public const float BrakeTime = 0.35f;
        public const float FeedbackLockSeconds = 0.35f;
        public const float MeSlideSeconds = 0.40f;
        public const float SwapBlinkSeconds = 0.40f;
        public const float ClearRingSeconds = 0.70f;
        public const float TrayWorldRadius = 2.5f;
        public const float OrthoSize = 6.1f;
        public const float GuestDrawRadius = 1.28f; // nhích khách xuống chừa chỗ cho biểu cảm vui/buồn
        public const float FoodVisualRadius = 0.08f;

        public const float GumihoCenterDistance = 0.62f;
        public const float GumihoR0 = 0.20f;
        public const float GumihoRMax = 0.48f;
        public const float GumihoRMaxDual = 0.47f;

        public const float HoW0 = 0.08f;
        public const float HoWMax = 0.28f;
        public const float HoMinAlong = 0f;
        public const float HoMinAlongDual = 0.15f;

        public const float MeLen0 = 0.35f;
        public const float MeLenMax = 0.95f;
        public const float MeHalfWidth = 0.13f;
        public const float MeCrossAt = 0.50f;
        public const float MeSwayIdle = 5f;
        public const float MeSwayMax = 18f;
        public const float MeSwayHz = 0.6f;

        public const float CoRadius = 0.13f;
        public const float CoAlong0 = 0.82f;
        public const float CoAlongMax = 0.32f;

        public const float DokkaRadius = 0.14f;
        public const float DokkaAlongMin = 0f;
        public const float DokkaAlongMax = 1f;
        public const float DokkaAlongHz = 0.40f;
        public const float DokkaSweepMax = 42f;
        public const float DokkaTapSeconds = 0.15f;
        // Đốm quét ngang chậm: từ lúc quá DokkaTapSeconds tới góc tối đa mất chừng này giây.
        public const float DokkaSweepSeconds = 0.80f;

        public const float SingleAnchor = 270f;
        public const float DualGumihoAnchor = 180f;
        public const float DualHoAnchor = 0f;
        public const float GuestDrawRadiusDual = 1.42f;

        public static float HoldU(float holdSeconds)
        {
            return Mathf.Clamp01(holdSeconds / HoldToMaxSeconds);
        }

        public static float GumihoRadius(float holdSeconds, bool dual)
        {
            return Mathf.Lerp(GumihoR0, dual ? GumihoRMaxDual : GumihoRMax, HoldU(holdSeconds));
        }

        public static float HoHalfWidth(float holdSeconds)
        {
            return Mathf.Lerp(HoW0, HoWMax, HoldU(holdSeconds));
        }

        public static float MeLength(float holdSeconds)
        {
            return Mathf.Lerp(MeLen0, MeLenMax, HoldU(holdSeconds));
        }

        public static bool MeSlides(float length, float crossAt)
        {
            return length > crossAt;
        }

        // Dương = lệch ngược chiều kim đồng hồ khỏi tia neo.
        // Biên độ cố định khi dải đang lắc. Nhấn giữ thì LevelRunner đóng băng góc này.
        public static float MeSway(float time)
        {
            return MeSwayIdle * Mathf.Sin(time * MeSwayHz * Mathf.PI * 2f);
        }

        public static float CoAlong(float holdSeconds)
        {
            return Mathf.Lerp(CoAlong0, CoAlongMax, HoldU(holdSeconds));
        }

        public static float DokkaSweep(float holdSeconds)
        {
            // Chạm nhanh thì đốm đứng yên. Chỉ khi giữ quá DokkaTapSeconds mới bắt đầu lướt.
            float u = Mathf.Clamp01((holdSeconds - DokkaTapSeconds) / DokkaSweepSeconds);
            return Mathf.Lerp(0f, DokkaSweepMax, u);
        }

        // Chưa giữ: tâm đốm chạy từ tâm mâm ra mép rồi quay vào. Một vòng mất 2,5 giây.
        // Nhấn giữ thì LevelRunner đóng băng along này. Đốm chỉ quét góc.
        public static float DokkaAlong(float time)
        {
            float wave = Mathf.Sin(time * DokkaAlongHz * Mathf.PI * 2f);
            return Mathf.Lerp(DokkaAlongMin, DokkaAlongMax, 0.5f + 0.5f * wave);
        }

        public static Vector2 Point(float angleDeg, float radius)
        {
            float a = angleDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }

        public static Vector2 ToWorld(Vector2 normalized)
        {
            return normalized * TrayWorldRadius;
        }

        public static void Axes(Vector2 food, float anchorDeg, out float along, out float perp)
        {
            float a = anchorDeg * Mathf.Deg2Rad;
            var axis = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            along = Vector2.Dot(food, axis);
            perp = Mathf.Abs(food.x * axis.y - food.y * axis.x);
        }

        public static bool InGumihoZone(Vector2 foodNormalized, float zoneRadius, float anchorDeg)
        {
            return Vector2.Distance(foodNormalized, Point(anchorDeg, GumihoCenterDistance)) <= zoneRadius;
        }

        public static bool InHoZone(Vector2 foodNormalized, float halfWidth, float anchorDeg, bool dual)
        {
            Axes(foodNormalized, anchorDeg, out float along, out float perp);
            bool alongOk = dual ? along >= HoMinAlongDual : along > HoMinAlong;
            return alongOk && perp <= halfWidth;
        }

        public static bool InMeZone(Vector2 foodNormalized, float length, float anchorDeg)
        {
            Axes(foodNormalized, anchorDeg, out float along, out float perp);
            return along > 0f && perp <= MeHalfWidth && (1f - along) <= length;
        }

        // dir: +1 lướt sang phải (nhấn nửa phải), -1 lướt sang trái (nhấn nửa trái).
        public static bool InDokkaebiZone(Vector2 foodNormalized, float holdSeconds, float anchorDeg, float along, float dir = 1f)
        {
            float angle = anchorDeg + dir * DokkaSweep(holdSeconds);
            return Vector2.Distance(foodNormalized, Point(angle, along)) <= DokkaRadius;
        }

        public static bool InCoZone(Vector2 foodNormalized, float holdSeconds, float anchorDeg)
        {
            return Vector2.Distance(foodNormalized, Point(anchorDeg, CoAlong(holdSeconds))) <= CoRadius;
        }

        public static Vector2 FoodOnScreen(float angleDeg, float radius, float trayDeg)
        {
            return Point(angleDeg + trayDeg, radius);
        }

        public static BiteOutcome Judge(bool anyForbid, bool anyWant)
        {
            if (anyForbid) return BiteOutcome.Foul;
            if (anyWant) return BiteOutcome.Eat;
            return BiteOutcome.Miss;
        }
    }
}
