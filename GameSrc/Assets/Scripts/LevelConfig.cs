using System;
using UnityEngine;

namespace MotMam
{
    public enum GuestId
    {
        Gumiho,
        Ho,
        Me,
        Co,
        Dokkaebi
    }

    public enum GuestMode
    {
        Gumiho,
        Ho,
        Me,
        GumihoHo,
        Co,
        Dokkaebi
    }

    [Serializable]
    public class DishSprite
    {
        public string dishId;
        public Sprite sprite;
    }

    [Serializable]
    public class FoodPlacement
    {
        public string dishId;
        public float angleDeg;
        public float radius;
        public GuestId[] want;
        public GuestId[] forbid;
        public GuestId[] wantAfterSwap;
        public GuestId[] forbidAfterSwap;
    }

    [CreateAssetMenu(fileName = "Level", menuName = "Mot Mam/Level")]
    public class LevelConfig : ScriptableObject
    {
        public int id;
        public GuestMode guestMode;
        public float spinSpeed;
        public int charges = 2;
        public int hearts = 2;
        public float brakeTime;
        public float crossAt;
        public int swapAfterBite;
        // Lệch vị trí khách (độ). Âm = sang trái khi khách ngồi phía dưới.
        public float anchorOffset;
        [TextArea] public string hint;
        public FoodPlacement[] foods;

        public float ResolvedBrakeTime => brakeTime > 0f ? brakeTime : GameRules.BrakeTime;
        public float ResolvedCrossAt => crossAt > 0f ? crossAt : GameRules.MeCrossAt;
    }
}
