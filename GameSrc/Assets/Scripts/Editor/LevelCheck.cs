using System.Text;
using UnityEditor;
using UnityEngine;

namespace MotMam
{
    public static class LevelCheck
    {
        public static void Run()
        {
            var log = new StringBuilder();
            int failed = 0;
            var levels = LevelConfig.FirstFour();

            failed += Expect(log, "L1 tap chỉ ăn songpyeon", HitsGumiho(levels[0], 0f, 0f), "songpyeon");
            failed += Expect(log, "L2 tap chỉ ăn tteokbokki", HitsGumiho(levels[1], 0f, 0f), "tteokbokki");
            failed += Expect(log, "L2 giữ hết nuốt phở", HitsGumiho(levels[1], 0f, 0.5f), "tteokbokki", "pho");
            failed += Expect(log, "L3 thả tại 70° chỉ ăn gimbap", HitsGumiho(levels[2], 70f, 0f), "gimbap");
            if (levels[3].charges != 1) failed += Fail(log, "L4 một lượt");
            failed += Expect(log, "L4 thả nhanh tại 45° hụt", HitsGumiho(levels[3], 45f, 0f));
            failed += Expect(log, "L4 giữ vừa tại 45° ăn đúng cặp", HitsGumiho(levels[3], 45f, 0.15f), "songpyeon", "tteokbokki");
            failed += Expect(log, "L4 giữ hết khi phở kề thì nuốt phở", HitsGumiho(levels[3], 340f, 0.5f), "pho", "tteokbokki");

            if (GameRules.Judge(true, true) != BiteOutcome.Foul) failed += Fail(log, "forbid thắng want");
            if (GameRules.Judge(false, true) != BiteOutcome.Eat) failed += Fail(log, "want thì ăn");
            if (GameRules.Judge(false, false) != BiteOutcome.Miss) failed += Fail(log, "hụt thì trừ lượt");
            if (GameRules.MeSlides(0.55f, 0.55f)) failed += Fail(log, "đúng ngưỡng thì Mê đứng yên");
            if (!GameRules.MeSlides(0.56f, 0.55f)) failed += Fail(log, "qua ngưỡng thì Mê lướt");

            failed += Expect(log, "L5 khe hẹp chỉ gỏi cuốn", HitsHo(false, 270f, 0f, 0f,
                F("goi_cuon", 270f, 0.58f), F("gimbap", 248f, 0.58f), F("bibimbap", 292f, 0.58f)), "goi_cuon");
            var level6 = new[]
            {
                F("goi_cuon", 250f, 0.58f), F("banh_mi", 228f, 0.58f), F("gimbap", 206f, 0.58f)
            };
            failed += Expect(log, "L6 θ20 khe hẹp chỉ gỏi cuốn", HitsHo(false, 270f, 20f, 0f, level6), "goi_cuon");
            failed += Expect(log, "L6 θ20 giữ hết dính bánh mì", HitsHo(false, 270f, 20f, 0.5f, level6), "banh_mi", "goi_cuon");
            failed += Expect(log, "L6 θ64 khe hẹp chỉ gimbap", HitsHo(false, 270f, 64f, 0f, level6), "gimbap");
            failed += Expect(log, "L7 θ0 giữ nhẹ ăn cặp thèm", HitsHo(false, 270f, 0f, 0.12f,
                F("gimbap", 262f, 0.58f), F("goi_cuon", 278f, 0.58f), F("pho", 300f, 0.58f), F("ca_phe", 100f, 0.78f)),
                "gimbap", "goi_cuon");

            failed += Expect(log, "L8 thả ngắn chỉ bibimbap", HitsMe(270f, 0f, 0f,
                F("bibimbap", 270f, 0.70f), F("songpyeon", 90f, 0.70f), F("tteokbokki", 0f, 0.50f)), "bibimbap");
            failed += Expect(log, "L8 lắc 6° vẫn bibimbap", HitsMe(276f, 0f, 0f,
                F("bibimbap", 270f, 0.70f), F("songpyeon", 90f, 0.70f), F("tteokbokki", 0f, 0.50f)), "bibimbap");
            failed += Expect(log, "L8 lắc 18° hụt bibimbap", HitsMe(288f, 0f, 0.5f,
                F("bibimbap", 270f, 0.70f), F("songpyeon", 90f, 0.70f), F("tteokbokki", 0f, 0.50f)));
            failed += Expect(log, "L9 nhát dài chỉ phở", HitsMe(270f, 0f, 0.5f,
                F("pho", 270f, 0.68f), F("ca_phe", 125f, 0.68f), F("banh_mi", 105f, 0.68f), F("gimbap", 200f, 0.50f)), "pho");
            failed += Expect(log, "L9 nhát 2 chỉ cà phê", HitsMe(90f, 325f, 0f,
                F("pho", 270f, 0.68f), F("ca_phe", 125f, 0.68f), F("banh_mi", 105f, 0.68f), F("gimbap", 200f, 0.50f)), "ca_phe");

            var level13 = new[]
            {
                F("goi_cuon", 258f, 0.58f), F("gimbap", 240f, 0.52f), F("bibimbap", 222f, 0.40f)
            };
            failed += Expect(log, "L13 thả ngay hụt", HitsHo(false, 270f, 0f, 0f, level13));
            failed += Expect(log, "L13 giữ vừa chỉ gỏi cuốn", HitsHo(false, 270f, 0f, 0.25f, level13), "goi_cuon");
            failed += Expect(log, "L13 giữ hết dính gimbap", HitsHo(false, 270f, 0f, 0.5f, level13), "gimbap", "goi_cuon");

            var level14 = new[]
            {
                F("songpyeon", 175f, 0.72f), F("banh_mi", 180f, 0.35f), F("gimbap", 0f, 0.70f), F("pho", 95f, 0.45f)
            };
            failed += Expect(log, "L14 trái thả nhanh chỉ songpyeon", HitsGumihoFoods(level14, 180f, 0f, 0f, true), "songpyeon");
            failed += Expect(log, "L14 trái giữ vừa dính bánh mì", HitsGumihoFoods(level14, 180f, 0f, 0.15f, true), "banh_mi", "songpyeon");
            failed += Expect(log, "L14 phải thả nhanh chỉ gimbap", HitsHo(true, 0f, 0f, 0f, level14), "gimbap");
            failed += Expect(log, "L14 phải giữ hết vẫn chỉ gimbap", HitsHo(true, 0f, 0f, 0.5f, level14), "gimbap");

            failed += Expect(log, "L15 chạm dính bánh mì", HitsCo(0f, F("goi_cuon", 270f, 0.55f), F("banh_mi", 270f, 0.82f), F("pho", 270f, 0.34f)), "banh_mi");
            failed += Expect(log, "L15 giữ vừa chỉ gỏi cuốn", HitsCo(0.22f, F("goi_cuon", 270f, 0.55f), F("banh_mi", 270f, 0.82f), F("pho", 270f, 0.34f)), "goi_cuon");
            failed += Expect(log, "L15 giữ sâu dính cả phở", HitsCo(0.37f, F("goi_cuon", 270f, 0.55f), F("banh_mi", 270f, 0.82f), F("pho", 270f, 0.34f)), "goi_cuon", "pho");
            failed += Expect(log, "L15 giữ hết chỉ phở", HitsCo(0.5f, F("goi_cuon", 270f, 0.55f), F("banh_mi", 270f, 0.82f), F("pho", 270f, 0.34f)), "pho");

            var level16 = new[]
            {
                F("banh_mi", 270f, 0.84f), F("gimbap", 288f, 0.56f), F("bibimbap", 312f, 0.56f)
            };
            failed += Expect(log, "L16 khóa mép thả ngay dính bánh mì", HitsDokka(0f, 0.84f, level16), "banh_mi");
            failed += Expect(log, "L16 khóa vành trong thả ngay thì hụt", HitsDokka(0f, 0.56f, level16));
            failed += Expect(log, "L16 khóa vành trong giữ vừa chỉ gimbap", HitsDokka(0.18f, 0.56f, level16), "gimbap");
            failed += Expect(log, "L16 khóa vành trong giữ hết chỉ bibimbap", HitsDokka(0.5f, 0.56f, level16), "bibimbap");
            failed += Expect(log, "L16 khóa mép giữ hết thì hụt", HitsDokka(0.5f, 0.84f, level16));

            var level17 = new[]
            {
                F("goi_cuon", 290f, 0.48f), F("ca_phe", 296f, 0.80f),
                F("banh_mi", 270f, 0.80f), F("bibimbap", 312f, 0.80f)
            };
            failed += Expect(log, "L17 khóa trong thả ngay thì hụt", HitsDokka(0f, 0.48f, level17));
            failed += Expect(log, "L17 khóa trong giữ vừa chỉ gỏi cuốn", HitsDokka(0.22f, 0.48f, level17), "goi_cuon");
            failed += Expect(log, "L17 khóa trong giữ hết thì hụt", HitsDokka(0.5f, 0.48f, level17));
            failed += Expect(log, "L17 khóa mép thả ngay dính bánh mì", HitsDokka(0f, 0.80f, level17), "banh_mi");
            failed += Expect(log, "L17 khóa mép giữ vừa chỉ cà phê", HitsDokka(0.28f, 0.80f, level17), "ca_phe");
            failed += Expect(log, "L17 khóa mép giữ hết chỉ bibimbap", HitsDokka(0.5f, 0.80f, level17), "bibimbap");

            var level10 = new[]
            {
                F("songpyeon", 250f, 0.58f), F("pho", 160f, 0.58f), F("gimbap", 70f, 0.58f), F("banh_mi", 340f, 0.58f)
            };
            failed += Expect(log, "L10 θ20 thả nhanh chỉ songpyeon", HitsGumihoFoods(level10, 270f, 20f, 0f, false), "songpyeon");
            failed += Expect(log, "L10 θ20 giữ hết vẫn chỉ songpyeon", HitsGumihoFoods(level10, 270f, 20f, 0.5f, false), "songpyeon");
            failed += Expect(log, "L10 θ110 thả nhanh chỉ phở", HitsGumihoFoods(level10, 270f, 110f, 0f, false), "pho");
            failed += Expect(log, "L10 θ200 thả nhanh chỉ gimbap", HitsGumihoFoods(level10, 270f, 200f, 0f, false), "gimbap");
            failed += Expect(log, "L10 θ290 thả nhanh chỉ bánh mì", HitsGumihoFoods(level10, 270f, 290f, 0f, false), "banh_mi");

            var level11 = new[]
            {
                F("songpyeon", 175f, 0.70f), F("gimbap", 355f, 0.70f), F("goi_cuon", 15f, 0.55f), F("pho", 95f, 0.45f)
            };
            failed += Expect(log, "L11 trái thả nhanh chỉ songpyeon", HitsGumihoFoods(level11, 180f, 0f, 0f, true), "songpyeon");
            failed += Expect(log, "L11 phải khe hẹp chỉ gimbap", HitsHo(true, 0f, 0f, 0f, level11), "gimbap");

            var level12 = new[]
            {
                F("songpyeon", 150f, 0.70f), F("gimbap", 10f, 0.70f), F("banh_mi", 190f, 0.55f), F("goi_cuon", 330f, 0.55f)
            };
            failed += Expect(log, "L12 θ30 trái thả nhanh chỉ songpyeon", HitsGumihoFoods(level12, 180f, 30f, 0f, true), "songpyeon");
            failed += Expect(log, "L12 mở màn phải giữ nhẹ chỉ gimbap", HitsHo(true, 0f, 0f, 0.18f, level12), "gimbap");

            Debug.Log(log.ToString());
            EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        struct BiteFood
        {
            public string dishId;
            public float angleDeg;
            public float radius;
        }

        static BiteFood F(string dishId, float angleDeg, float radius)
        {
            return new BiteFood { dishId = dishId, angleDeg = angleDeg, radius = radius };
        }

        static string[] HitsGumiho(LevelConfig level, float trayDeg, float hold)
        {
            float radius = GameRules.GumihoRadius(hold, false);
            var names = new System.Collections.Generic.List<string>();
            var foods = level.foods;
            for (int i = 0; i < foods.Length; i++)
            {
                var food = foods[i];
                var point = GameRules.FoodOnScreen(food.angleDeg, food.radius, trayDeg);
                if (GameRules.InGumihoZone(point, radius, GameRules.SingleAnchor)) names.Add(food.dishId);
            }
            names.Sort();
            return names.ToArray();
        }

        static string[] HitsGumihoFoods(BiteFood[] foods, float anchor, float trayDeg, float hold, bool dual)
        {
            float radius = GameRules.GumihoRadius(hold, dual);
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < foods.Length; i++)
            {
                var point = GameRules.FoodOnScreen(foods[i].angleDeg, foods[i].radius, trayDeg);
                if (GameRules.InGumihoZone(point, radius, anchor)) names.Add(foods[i].dishId);
            }
            names.Sort();
            return names.ToArray();
        }

        static string[] HitsHo(bool dual, float anchor, float trayDeg, float hold, params BiteFood[] foods)
        {
            float width = GameRules.HoHalfWidth(hold);
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < foods.Length; i++)
            {
                var point = GameRules.FoodOnScreen(foods[i].angleDeg, foods[i].radius, trayDeg);
                if (GameRules.InHoZone(point, width, anchor, dual)) names.Add(foods[i].dishId);
            }
            names.Sort();
            return names.ToArray();
        }

        static string[] HitsCo(float hold, params BiteFood[] foods)
        {
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < foods.Length; i++)
            {
                var point = GameRules.FoodOnScreen(foods[i].angleDeg, foods[i].radius, 0f);
                if (GameRules.InCoZone(point, hold, GameRules.SingleAnchor)) names.Add(foods[i].dishId);
            }
            names.Sort();
            return names.ToArray();
        }

        static string[] HitsDokka(float hold, float along, params BiteFood[] foods)
        {
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < foods.Length; i++)
            {
                var point = GameRules.FoodOnScreen(foods[i].angleDeg, foods[i].radius, 0f);
                if (GameRules.InDokkaebiZone(point, hold, GameRules.SingleAnchor, along)) names.Add(foods[i].dishId);
            }
            names.Sort();
            return names.ToArray();
        }

        static string[] HitsMe(float anchor, float trayDeg, float hold, params BiteFood[] foods)
        {
            float length = GameRules.MeLength(hold);
            var names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < foods.Length; i++)
            {
                var point = GameRules.FoodOnScreen(foods[i].angleDeg, foods[i].radius, trayDeg);
                if (GameRules.InMeZone(point, length, anchor)) names.Add(foods[i].dishId);
            }
            names.Sort();
            return names.ToArray();
        }

        static int Expect(StringBuilder log, string name, string[] actual, params string[] expected)
        {
            System.Array.Sort(expected);
            bool same = actual.Length == expected.Length;
            for (int i = 0; same && i < actual.Length; i++) same = actual[i] == expected[i];
            if (same)
            {
                log.AppendLine("OK " + name);
                return 0;
            }
            log.AppendLine("FAIL " + name + " got [" + string.Join(", ", actual) + "]");
            return 1;
        }

        static int Fail(StringBuilder log, string name)
        {
            log.AppendLine("FAIL " + name);
            return 1;
        }
    }
}
