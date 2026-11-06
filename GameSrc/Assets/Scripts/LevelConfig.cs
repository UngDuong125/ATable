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
        public int hearts = 3;
        public float brakeTime;
        public float crossAt;
        public int swapAfterBite;
        public FoodPlacement[] foods;

        public float ResolvedBrakeTime => brakeTime > 0f ? brakeTime : GameRules.BrakeTime;
        public float ResolvedCrossAt => crossAt > 0f ? crossAt : GameRules.MeCrossAt;

        // Chỉ dùng cho bước kiểm tra trong Editor. Màn chơi lấy từ asset gán trên LevelRunner.
        public static LevelConfig[] FirstFour()
        {
            var levels = new[]
            {
                Make(1, 0f, 0f,
                    Food("songpyeon", 270f, 0.58f, true),
                    Food("banh_mi", 90f, 0.58f, false),
                    Food("bibimbap", 20f, 0.78f, false)),
                Make(2, 0f, 0f,
                    Food("tteokbokki", 270f, 0.58f, true),
                    Food("pho", 270f, 0.38f, false),
                    Food("goi_cuon", 200f, 0.78f, false)),
                Make(3, 28f, 0f,
                    Food("gimbap", 200f, 0.58f, true),
                    Food("ca_phe", 245f, 0.58f, false),
                    Food("banh_mi", 40f, 0.78f, false)),
                Make(4, 40f, 0.35f,
                    Food("songpyeon", 200f, 0.58f, true),
                    Food("tteokbokki", 250f, 0.58f, true),
                    Food("pho", 320f, 0.58f, false),
                    Food("goi_cuon", 110f, 0.78f, false))
            };
            levels[3].charges = 1;
            return levels;
        }

        static LevelConfig Make(int id, float spinSpeed, float brakeTime, params FoodPlacement[] foods)
        {
            var level = CreateInstance<LevelConfig>();
            level.name = "Level" + id.ToString("00");
            level.id = id;
            level.guestMode = GuestMode.Gumiho;
            level.spinSpeed = spinSpeed;
            level.charges = 2;
            level.hearts = 3;
            level.brakeTime = brakeTime;
            level.foods = foods;
            return level;
        }

        static FoodPlacement Food(string dishId, float angleDeg, float radius, bool want)
        {
            return new FoodPlacement
            {
                dishId = dishId,
                angleDeg = angleDeg,
                radius = radius,
                want = want ? new[] { GuestId.Gumiho } : Array.Empty<GuestId>(),
                forbid = want ? Array.Empty<GuestId>() : new[] { GuestId.Gumiho }
            };
        }
    }
}
