using System.Collections.Generic;
using UnityEngine;

namespace MotMam
{
    // Kiểu bước hướng dẫn.
    // Say: hộp thoại, chạm bất kỳ đâu để sang bước sau.
    // Tap / Hold: mở một lỗ sáng trên lớp phủ, chờ người chơi cho khách ăn trúng món rồi mới sang bước sau.
    public enum TutorialAction
    {
        Say,
        Tap,
        Hold
    }

    // Chỗ được chiếu sáng trên lớp phủ tối.
    public enum TutorialFocus
    {
        None,
        Guest,      // khách đang ngồi (khách chính của màn)
        GuestRight, // khách bên phải ở màn hai khách (Hổ)
        Zone,       // vùng ăn trước mặt khách chính
        ZoneRight,  // vùng ăn của khách bên phải
        Tray,       // cả mâm
        Icons,      // hàng icon món thèm / món cấm dưới đáy
        Stats,      // số Mạng và Lượt
        HoldBar,    // thanh giữ
        PressSpot,      // ô nhấn ở chéo dưới-phải khách chính, tránh xa mâm
        PressSpotRight  // ô nhấn của khách bên phải ở màn hai khách
    }

    public class TutorialStep
    {
        public TutorialAction action;
        public TutorialFocus focus;
        public bool hasSpeaker;
        public GuestId speaker;
        public string text;
        public string retryMiss;
        public string retryFoul;
    }

    public class TutorialScript
    {
        public string key;
        public string title;
        public readonly List<TutorialStep> steps = new();

        public TutorialScript(string scriptKey, string scriptTitle)
        {
            key = scriptKey;
            title = scriptTitle;
        }

        public TutorialScript Say(string text, TutorialFocus focus = TutorialFocus.None)
        {
            steps.Add(new TutorialStep { action = TutorialAction.Say, focus = focus, text = text });
            return this;
        }

        public TutorialScript Say(GuestId speaker, string text, TutorialFocus focus = TutorialFocus.None)
        {
            steps.Add(new TutorialStep
            {
                action = TutorialAction.Say, focus = focus, text = text,
                hasSpeaker = true, speaker = speaker
            });
            return this;
        }

        public TutorialScript Act(TutorialAction action, TutorialFocus focus, string text, string retryMiss, string retryFoul)
        {
            steps.Add(new TutorialStep
            {
                action = action, focus = focus, text = text,
                retryMiss = retryMiss, retryFoul = retryFoul
            });
            return this;
        }
    }

    // Toàn bộ lời thoại hướng dẫn. Sửa chữ ở đây, không cần đụng scene.
    // Khóa là id màn (LevelConfig.id). Màn có khóa thì tự mở hướng dẫn lần đầu vào màn.
    public static class TutorialContent
    {
        const string SeenPrefix = "motmam.tut.";

        static Dictionary<int, TutorialScript> scripts;

        public static TutorialScript ForLevel(int levelId)
        {
            Build();
            return scripts.TryGetValue(levelId, out var script) ? script : null;
        }

        // Nút "?" ở màn không có hướng dẫn riêng: lấy hướng dẫn đầu tiên của cùng kiểu khách.
        public static TutorialScript ForReplay(LevelConfig[] levels, int index)
        {
            if (levels == null || index < 0 || index >= levels.Length || levels[index] == null) return null;
            var own = ForLevel(levels[index].id);
            if (own != null) return own;
            var mode = levels[index].guestMode;
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null || levels[i].guestMode != mode) continue;
                var script = ForLevel(levels[i].id);
                if (script != null) return script;
            }
            return null;
        }

        public static bool Seen(TutorialScript script)
        {
            return script != null && PlayerPrefs.GetInt(SeenPrefix + script.key, 0) == 1;
        }

        public static void MarkSeen(TutorialScript script)
        {
            if (script == null) return;
            PlayerPrefs.SetInt(SeenPrefix + script.key, 1);
            PlayerPrefs.Save();
        }

        // Gọi khi muốn xem lại toàn bộ hướng dẫn từ đầu (ví dụ nút trong Cài đặt).
        public static void ResetAll()
        {
            Build();
            foreach (var script in scripts.Values) PlayerPrefs.DeleteKey(SeenPrefix + script.key);
            PlayerPrefs.Save();
        }

        static void Build()
        {
            if (scripts != null) return;
            scripts = new Dictionary<int, TutorialScript>();

            // Màn 1: chào mừng, giới thiệu Gumiho, chạm và giữ.
            scripts[1] = new TutorialScript("l1", "Gumiho")
                .Say("Chào mừng tới mâm cỗ Việt – Hàn!")
                .Say("Các vị khách đang tới, hãy tiếp đãi họ thật chu đáo nhé.", TutorialFocus.Tray)
                .Say(GuestId.Gumiho, "Vị khách đầu tiên đã tới, đó là Gumiho!", TutorialFocus.Guest)
                .Say(GuestId.Gumiho, "Cô ấy rất thích các món ăn Hàn Quốc.", TutorialFocus.Guest)
                .Say(GuestId.Gumiho, "Món cô ấy đang thèm hiện ở đây. Trên mâm, món thèm sáng lên, món khác bị tối đi.", TutorialFocus.Icons)
                .Act(TutorialAction.Tap, TutorialFocus.Zone,
                    "Chạm vào màn hình để cô ấy thưởng thức món ăn.",
                    "Hụt mất rồi! Chạm thật nhanh một cái để vòng nhỏ đớp món sáng gần miệng.",
                    "Ối, trúng món không được ăn! Chạm nhanh hơn để vòng nhỏ lại nhé.")
                .Say(GuestId.Gumiho, "Ngon quá! Nhưng vẫn còn mấy món ở xa, vòng nhỏ không với tới.", TutorialFocus.Zone)
                .Act(TutorialAction.Hold, TutorialFocus.Zone,
                    "Nhấn giữ để Gumiho nạp năng lượng và ăn món ở xa hơn. Thả tay khi vòng phủ món có viền xanh.",
                    "Chưa tới! Giữ lâu hơn chút để vòng to ra rồi mới thả.",
                    "Vòng lan sang món viền đỏ rồi! Thả tay sớm hơn một chút nhé.")
                .Say("Mỗi lần cắn tốn 1 Lượt. Cắn nhầm món không được ăn sẽ mất 1 Mạng.", TutorialFocus.Stats)
                .Say("Ăn hết các món sáng để qua màn. Chúc bạn tiếp khách thật vui!");

            // Màn 3: mâm bắt đầu xoay.
            scripts[3] = new TutorialScript("l3", "Mâm xoay")
                .Say("Từ màn này, mâm bắt đầu xoay!", TutorialFocus.Tray)
                .Say(GuestId.Gumiho, "Nhấn giữ thì mâm phanh lại dần. Canh lúc món sáng xoay tới gần Gumiho rồi hẵng chạm.", TutorialFocus.Zone)
                .Say("Thanh này cho biết bạn đã giữ bao lâu.", TutorialFocus.HoldBar);

            // Màn 5: Hổ.
            scripts[5] = new TutorialScript("l5", "Hổ")
                .Say(GuestId.Ho, "Một vị khách mới ghé mâm: Hổ!", TutorialFocus.Guest)
                .Say(GuestId.Ho, "Hổ có mặt trong tranh dân gian của cả Việt Nam lẫn Hàn Quốc. Hổ mê món cuộn, món gắp một miếng.", TutorialFocus.Guest)
                .Say(GuestId.Ho, "Hổ không cắn theo vòng mà cắn theo một khe thẳng, từ mép mâm vào tận giữa.", TutorialFocus.Zone)
                .Act(TutorialAction.Tap, TutorialFocus.Zone,
                    "Chạm để Hổ cắn theo khe hẹp. Nhấn giữ thì khe rộng dần.",
                    "Hụt rồi! Chờ món sáng nằm trên khe rồi chạm, hoặc giữ thêm cho khe rộng ra.",
                    "Khe rộng quá, dính món không được ăn! Thử khe hẹp hơn nhé.")
                .Say(GuestId.Ho, "Nhớ nhé: có lúc cắn hai nhát hẹp sẽ tốt hơn một nhát rộng.");

            // Màn 8: Mê.
            scripts[8] = new TutorialScript("l8", "Mê")
                .Say(GuestId.Me, "Gâu! Đây là Mê, chú chó trắng đội nón mê.", TutorialFocus.Guest)
                .Say(GuestId.Me, "Mê ăn theo một dải thẳng. Khi chưa giữ, dải lắc qua lại quanh Mê.", TutorialFocus.Zone)
                .Act(TutorialAction.Tap, TutorialFocus.Zone,
                    "Chạm: dải ngắn. Giữ lâu: dải dài ra. Nhấn giữ thì dải đứng yên ngay lúc bấm.",
                    "Hụt rồi! Thả tay đúng lúc dải đang đè lên món sáng.",
                    "Dải đè trúng món không được ăn! Thử ngắn hơn hoặc canh góc khác nhé.")
                .Say(GuestId.Me, "Giữ đủ lâu cho dải qua nửa mâm thì Mê lướt sang mép bên kia. Lượt sau Mê ăn từ phía mới!", TutorialFocus.Tray);

            // Màn 13: Cò.
            scripts[13] = new TutorialScript("l13", "Cò")
                .Say(GuestId.Co, "Cò trắng đã tới! Mỏ Cò dài, chọc được thật sâu.", TutorialFocus.Guest)
                .Say(GuestId.Co, "Cò ăn bằng một chấm nhỏ. Giữ càng lâu, chấm càng chạy sâu vào giữa mâm, nhưng không to ra.", TutorialFocus.Zone)
                .Act(TutorialAction.Hold, TutorialFocus.Zone,
                    "Nhấn giữ rồi thả khi chấm nằm đúng trên món sáng.",
                    "Hụt rồi! Chạm sớm thì chấm còn sát mép, giữ lâu thì chấm vào sâu.",
                    "Chấm đè lên món không được ăn! Canh lại độ sâu nhé.");

            // Màn 15: Dokkaebi.
            scripts[15] = new TutorialScript("l15", "Dokkaebi")
                .Say(GuestId.Dokkaebi, "Hù! Dokkaebi, yêu tinh trong truyền thuyết Hàn Quốc, đã tới.", TutorialFocus.Guest)
                .Say(GuestId.Dokkaebi, "Đốm lửa của Dokkaebi chạy qua lại từ tâm mâm ra mép.", TutorialFocus.Zone)
                .Act(TutorialAction.Tap, TutorialFocus.Zone,
                    "Chạm nhanh: đốm đứng yên tại chỗ. Giữ lâu: đốm quét ngang. Nhấn nửa trái thì quét sang trái, nửa phải thì sang phải.",
                    "Hụt rồi! Chờ đốm chạy tới ngang món sáng rồi chạm.",
                    "Đốm đè trúng món không được ăn! Thử lại nhé.")
                .Say(GuestId.Dokkaebi, "Dokkaebi hay đổi ý! Cứ sau một số nhát, món thèm lại đổi. Thấy icon nhấp nháy là sắp đổi đó.", TutorialFocus.Icons);

            // Màn 18: hai khách.
            scripts[18] = new TutorialScript("l18", "Gumiho và Hổ")
                .Say("Hôm nay mâm có hai vị khách cùng ngồi!", TutorialFocus.Tray)
                .Say(GuestId.Gumiho, "Nửa trái màn hình để đãi Gumiho.", TutorialFocus.Guest)
                .Say(GuestId.Ho, "Nửa phải màn hình để đãi Hổ.", TutorialFocus.GuestRight)
                .Say("Thẻ đỏ bên trái là món của Gumiho, thẻ xanh bên phải là món của Hổ. Mỗi khách chỉ ăn món \"Đang thèm\" của mình.", TutorialFocus.Icons)
                .Say("Kéo ngón qua đường giữa màn hình thì nhát đó bị hủy.");
        }
    }
}
