using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MotMam
{
    // Chạy bằng: Unity -batchmode -executeMethod MotMam.LevelCheck.Run
    // Mỗi dòng Cases là một nhát trong lời giải ngắn nhất của màn (sinh từ trình mô phỏng).
    // Kiểm tra tập món nằm trong vùng ăn đúng như lời giải đã tính, đọc thẳng từ Levels/LevelNN.asset.
    public static class LevelCheck
    {
        struct BiteCase
        {
            public int level;
            public GuestId guest;
            public float anchor, tray, hold, sway, along;
            public int dir;
            public int[] eaten, expect;
        }

        static BiteCase Bite(int level, GuestId guest, float anchor, float tray, float hold, float sway, float along,
            int dir, int[] eaten, int[] expect)
        {
            return new BiteCase
            {
                level = level, guest = guest, anchor = anchor, tray = tray, hold = hold, sway = sway, along = along,
                dir = dir, eaten = eaten, expect = expect
            };
        }

        static readonly BiteCase[] Cases =
        {
            Bite(1, GuestId.Gumiho, 270.0f, 0.000f, 0.32f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0, 1, 2 }),
            Bite(2, GuestId.Gumiho, 270.0f, 0.000f, 0.64f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0, 1, 2 }),
            Bite(3, GuestId.Gumiho, 270.0f, 70.250f, 0.66f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0, 7, 9 }),
            Bite(3, GuestId.Gumiho, 270.0f, 117.500f, 0.68f, 0.000f, 0.500f, 0, new int[] { 0 }, new int[] { 1 }),
            Bite(4, GuestId.Gumiho, 270.0f, 114.743f, 0.30f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 1 }),
            Bite(4, GuestId.Gumiho, 270.0f, 174.343f, 0.70f, 0.000f, 0.500f, 0, new int[] { 1 }, new int[] { 0, 6 }),
            Bite(5, GuestId.Ho, 270.0f, 0.000f, 0.24f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0, 1, 2 }),
            Bite(6, GuestId.Ho, 270.0f, 55.312f, 0.22f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0, 2 }),
            Bite(6, GuestId.Ho, 270.0f, 147.362f, 0.52f, 0.000f, 0.500f, 0, new int[] { 0, 2 }, new int[] { 1 }),
            Bite(7, GuestId.Ho, 270.0f, 149.240f, 0.14f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 2 }),
            Bite(7, GuestId.Ho, 270.0f, 228.097f, 0.20f, 0.000f, 0.500f, 0, new int[] { 2 }, new int[] { 0 }),
            Bite(7, GuestId.Ho, 270.0f, 314.597f, 0.52f, 0.000f, 0.500f, 0, new int[] { 2, 0 }, new int[] { 1, 9 }),
            Bite(8, GuestId.Me, 270.0f, 0.000f, 0.60f, -0.314f, 0.500f, 0, new int[] {  }, new int[] { 0, 1 }),
            Bite(8, GuestId.Me, 450.0f, 0.000f, 0.58f, -0.314f, 0.500f, 0, new int[] { 0, 1 }, new int[] { 2 }),
            Bite(9, GuestId.Me, 270.0f, 7.600f, 0.56f, 4.524f, 0.500f, 0, new int[] {  }, new int[] { 2 }),
            Bite(9, GuestId.Me, 450.0f, 90.400f, 0.60f, 4.524f, 0.500f, 0, new int[] { 2 }, new int[] { 1 }),
            Bite(9, GuestId.Me, 630.0f, 164.603f, 0.04f, -1.841f, 0.500f, 0, new int[] { 2, 1 }, new int[] { 0 }),
            Bite(10, GuestId.Me, 270.0f, 41.250f, 0.62f, 0.627f, 0.500f, 0, new int[] {  }, new int[] { 3 }),
            Bite(10, GuestId.Me, 450.0f, 64.900f, 0.58f, -1.841f, 0.500f, 0, new int[] { 3 }, new int[] { 1 }),
            Bite(10, GuestId.Me, 630.0f, 197.450f, 0.56f, 2.129f, 0.500f, 0, new int[] { 3, 1 }, new int[] { 0 }),
            Bite(10, GuestId.Me, 810.0f, 297.000f, 0.56f, -4.524f, 0.500f, 0, new int[] { 3, 1, 0 }, new int[] { 2 }),
            Bite(11, GuestId.Gumiho, 270.0f, 16.150f, 0.42f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 3, 6, 9 }),
            Bite(11, GuestId.Gumiho, 270.0f, 80.479f, 0.20f, 0.000f, 0.500f, 0, new int[] { 3 }, new int[] { 0 }),
            Bite(11, GuestId.Gumiho, 270.0f, 146.707f, 0.20f, 0.000f, 0.500f, 0, new int[] { 3, 0 }, new int[] { 2 }),
            Bite(11, GuestId.Gumiho, 270.0f, 236.957f, 0.52f, 0.000f, 0.500f, 0, new int[] { 3, 0, 2 }, new int[] { 1, 10, 11 }),
            Bite(12, GuestId.Ho, 270.0f, 49.000f, 0.56f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0, 7 }),
            Bite(12, GuestId.Ho, 270.0f, 100.509f, 0.04f, 0.000f, 0.500f, 0, new int[] { 0 }, new int[] { 3 }),
            Bite(12, GuestId.Ho, 270.0f, 186.017f, 0.04f, 0.000f, 0.500f, 0, new int[] { 0, 3 }, new int[] { 1 }),
            Bite(12, GuestId.Ho, 270.0f, 325.017f, 0.52f, 0.000f, 0.500f, 0, new int[] { 0, 3, 1 }, new int[] { 2, 10 }),
            Bite(13, GuestId.Co, 270.0f, 0.000f, 0.20f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 1, 2 }),
            Bite(13, GuestId.Co, 270.0f, 0.000f, 0.36f, 0.000f, 0.500f, 0, new int[] { 1, 2 }, new int[] { 0 }),
            Bite(14, GuestId.Co, 258.0f, 36.562f, 0.16f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0 }),
            Bite(14, GuestId.Co, 258.0f, 55.162f, 0.64f, 0.000f, 0.500f, 0, new int[] { 0 }, new int[] { 1, 11 }),
            Bite(14, GuestId.Co, 258.0f, 182.962f, 0.64f, 0.000f, 0.500f, 0, new int[] { 0, 1 }, new int[] { 2 }),
            Bite(14, GuestId.Co, 258.0f, 351.562f, 0.64f, 0.000f, 0.500f, 0, new int[] { 0, 1, 2 }, new int[] { 3 }),
            Bite(15, GuestId.Dokkaebi, 270.0f, 0.000f, 0.72f, 0.000f, 0.794f, -1, new int[] {  }, new int[] { 2 }),
            Bite(15, GuestId.Dokkaebi, 270.0f, 0.000f, 0.42f, 0.000f, 0.794f, 1, new int[] { 2 }, new int[] { 5 }),
            Bite(15, GuestId.Dokkaebi, 270.0f, 0.000f, 0.68f, 0.000f, 0.624f, 1, new int[] { 2, 5 }, new int[] { 4 }),
            Bite(15, GuestId.Dokkaebi, 270.0f, 0.000f, 0.04f, 0.000f, 0.437f, -1, new int[] { 2, 5, 4 }, new int[] { 3 }),
            Bite(16, GuestId.Dokkaebi, 270.0f, 26.500f, 0.44f, 0.000f, 0.624f, -1, new int[] {  }, new int[] { 1 }),
            Bite(16, GuestId.Dokkaebi, 270.0f, 56.000f, 0.60f, 0.000f, 0.437f, -1, new int[] { 1 }, new int[] { 2 }),
            Bite(16, GuestId.Dokkaebi, 270.0f, 59.857f, 0.20f, 0.000f, 0.500f, 1, new int[] { 1, 2 }, new int[] { 3 }),
            Bite(16, GuestId.Dokkaebi, 270.0f, 120.357f, 0.48f, 0.000f, 0.885f, -1, new int[] { 1, 2, 3 }, new int[] { 4 }),
            Bite(17, GuestId.Dokkaebi, 270.0f, 36.081f, 0.04f, 0.000f, 0.376f, -1, new int[] {  }, new int[] { 1 }),
            Bite(17, GuestId.Dokkaebi, 270.0f, 76.849f, 0.28f, 0.000f, 0.794f, 1, new int[] { 1 }, new int[] { 0 }),
            Bite(17, GuestId.Dokkaebi, 270.0f, 210.099f, 0.74f, 0.000f, 0.741f, -1, new int[] { 1, 0 }, new int[] { 3 }),
            Bite(17, GuestId.Dokkaebi, 270.0f, 240.649f, 0.56f, 0.000f, 0.563f, -1, new int[] { 1, 0, 3 }, new int[] { 2 }),
            Bite(18, GuestId.Ho, 0.0f, 0.000f, 0.24f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 2, 3 }),
            Bite(18, GuestId.Gumiho, 180.0f, 0.000f, 0.62f, 0.000f, 0.500f, 0, new int[] { 2, 3 }, new int[] { 0, 1, 8, 12 }),
            Bite(19, GuestId.Gumiho, 180.0f, 80.696f, 0.28f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 0 }),
            Bite(19, GuestId.Ho, 0.0f, 223.111f, 0.16f, 0.000f, 0.500f, 0, new int[] { 0 }, new int[] { 2 }),
            Bite(19, GuestId.Gumiho, 180.0f, 298.461f, 0.52f, 0.000f, 0.500f, 0, new int[] { 0, 2 }, new int[] { 1, 8, 9, 12 }),
            Bite(19, GuestId.Ho, 0.0f, 307.657f, 0.28f, 0.000f, 0.500f, 0, new int[] { 0, 2, 1 }, new int[] { 3, 11 }),
            Bite(20, GuestId.Ho, 0.0f, 5.600f, 0.52f, 0.000f, 0.500f, 0, new int[] {  }, new int[] { 2, 10, 12 }),
            Bite(20, GuestId.Gumiho, 180.0f, 61.847f, 0.24f, 0.000f, 0.500f, 0, new int[] { 2 }, new int[] { 1, 4 }),
            Bite(20, GuestId.Ho, 0.0f, 101.047f, 0.58f, 0.000f, 0.500f, 0, new int[] { 2, 1, 4 }, new int[] { 3, 6 }),
            Bite(20, GuestId.Gumiho, 180.0f, 316.247f, 0.56f, 0.000f, 0.500f, 0, new int[] { 2, 1, 4, 3 }, new int[] { 0, 9 }),
        };

        public static void Run()
        {
            var log = new StringBuilder();
            int failed = 0;

            if (GameRules.Judge(true, true) != BiteOutcome.Foul) failed += Fail(log, "forbid thắng want");
            if (GameRules.Judge(false, true) != BiteOutcome.Eat) failed += Fail(log, "want thì ăn");
            if (GameRules.Judge(false, false) != BiteOutcome.Miss) failed += Fail(log, "hụt thì trừ lượt");
            if (GameRules.MeSlides(GameRules.MeCrossAt, GameRules.MeCrossAt)) failed += Fail(log, "đúng ngưỡng thì Mê đứng yên");
            if (!GameRules.MeSlides(GameRules.MeCrossAt + 0.01f, GameRules.MeCrossAt)) failed += Fail(log, "qua ngưỡng thì Mê lướt");

            var levels = new Dictionary<int, LevelConfig>();
            for (int id = 1; id <= 20; id++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Levels/Level" + id.ToString("00") + ".asset");
                if (asset == null) { failed += Fail(log, "thiếu Level" + id.ToString("00") + ".asset"); continue; }
                levels[id] = asset;
                failed += CheckLayout(log, asset);
                failed += CheckRules(log, asset);
            }

            for (int i = 0; i < Cases.Length; i++)
            {
                var c = Cases[i];
                if (!levels.TryGetValue(c.level, out var level)) continue;
                failed += Expect(log, "L" + c.level + " nhát " + (i + 1) + " giữ " + c.hold.ToString("0.00"), Hits(level, c), c.expect);
            }

            Debug.Log(log.ToString());
            EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        // Bàn tiệc: ít nhất 9 món, đúng vành cho phép, không hai món chồng lên nhau.
        static int CheckLayout(StringBuilder log, LevelConfig level)
        {
            int failed = 0;
            var foods = level.foods;
            string name = "L" + level.id;
            if (foods == null || foods.Length < 9) failed += Fail(log, name + " bàn tiệc cần ít nhất 9 món");
            if (foods == null) return failed;
            for (int i = 0; i < foods.Length; i++)
            {
                var a = GameRules.Point(foods[i].angleDeg, foods[i].radius);
                if (foods[i].radius < 0.28f || foods[i].radius > 0.86f) failed += Fail(log, name + " món " + i + " ngoài vành cho phép");
                for (int j = i + 1; j < foods.Length; j++)
                {
                    if (Vector2.Distance(a, GameRules.Point(foods[j].angleDeg, foods[j].radius)) < 0.10f)
                        failed += Fail(log, name + " món " + i + " và " + j + " chồng nhau");
                }
            }
            return failed;
        }


        // Luật xếp món: dưới 3 món thèm (mỗi khách, mỗi giai đoạn) thì không món nào bắt đầu ở 250..360°;
        // món rải khắp mâm (khoảng trống góc lớn nhất không quá 62°); mỗi khách có ít nhất một món thèm.
        static int CheckRules(StringBuilder log, LevelConfig level)
        {
            int failed = 0;
            string name = "L" + level.id;
            var foods = level.foods;
            if (foods == null) return failed;
            for (int phase = 0; phase < 2; phase++)
            {
                foreach (GuestId g in System.Enum.GetValues(typeof(GuestId)))
                {
                    int count = 0;
                    for (int i = 0; i < foods.Length; i++)
                    {
                        if (System.Array.IndexOf(phase == 0 ? foods[i].want : foods[i].wantAfterSwap ?? new GuestId[0], g) >= 0) count++;
                    }
                    if (count == 0 || count >= 3) continue;
                    for (int i = 0; i < foods.Length; i++)
                    {
                        var list = phase == 0 ? foods[i].want : foods[i].wantAfterSwap;
                        if (list == null || System.Array.IndexOf(list, g) < 0) continue;
                        float a = ((foods[i].angleDeg % 360f) + 360f) % 360f;
                        if (a >= 250f) failed += Fail(log, name + " dưới 3 món thèm nhưng món " + i + " bắt đầu ở " + a.ToString("0") + "°");
                    }
                }
            }
            var angles = new List<float>();
            for (int i = 0; i < foods.Length; i++) angles.Add(((foods[i].angleDeg % 360f) + 360f) % 360f);
            angles.Sort();
            for (int i = 0; i < angles.Count; i++)
            {
                float next = angles[(i + 1) % angles.Count];
                float gap = ((next - angles[i]) % 360f + 360f) % 360f;
                if (angles.Count > 1 && gap > 62f) failed += Fail(log, name + " khoảng trống " + gap.ToString("0") + "° giữa hai món liền kề");
            }
            return failed;
        }

        static int[] Hits(LevelConfig level, BiteCase c)
        {
            bool dual = level.guestMode == GuestMode.GumihoHo;
            var found = new List<int>();
            for (int i = 0; i < level.foods.Length; i++)
            {
                if (System.Array.IndexOf(c.eaten, i) >= 0) continue;
                var food = level.foods[i];
                var point = GameRules.FoodOnScreen(food.angleDeg, food.radius, c.tray);
                bool inside;
                switch (c.guest)
                {
                    case GuestId.Ho: inside = GameRules.InHoZone(point, GameRules.HoHalfWidth(c.hold), c.anchor, dual); break;
                    case GuestId.Me:
                        inside = GameRules.InMeZone(point, GameRules.MeLength(c.hold), c.anchor + c.sway); break;
                    case GuestId.Co: inside = GameRules.InCoZone(point, c.hold, c.anchor); break;
                    case GuestId.Dokkaebi:
                        inside = GameRules.InDokkaebiZone(point, c.hold, c.anchor, c.along, c.dir); break;
                    default: inside = GameRules.InGumihoZone(point, GameRules.GumihoRadius(c.hold, dual), c.anchor); break;
                }
                if (inside) found.Add(i);
            }
            return found.ToArray();
        }

        static int Expect(StringBuilder log, string name, int[] actual, int[] expected)
        {
            var a = (int[])actual.Clone();
            var e = (int[])expected.Clone();
            System.Array.Sort(a);
            System.Array.Sort(e);
            bool same = a.Length == e.Length;
            for (int i = 0; same && i < a.Length; i++) same = a[i] == e[i];
            if (same)
            {
                log.AppendLine("OK " + name);
                return 0;
            }
            log.AppendLine("FAIL " + name + " got [" + string.Join(", ", a) + "] want [" + string.Join(", ", e) + "]");
            return 1;
        }

        static int Fail(StringBuilder log, string name)
        {
            log.AppendLine("FAIL " + name);
            return 1;
        }
    }
}
