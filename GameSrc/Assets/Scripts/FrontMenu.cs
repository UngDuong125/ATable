using UnityEngine;
using UnityEngine.UI;

namespace MotMam
{
    public class FrontMenu : MonoBehaviour
    {
        Font font;
        LevelConfig[] levels;
        LevelRunner runner;
        GameObject root;
        GameObject home;
        GameObject levelPage;
        GameObject settingsPage;
        Button corner;
        Slider musicSlider;
        Slider sfxSlider;
        Text musicValue;
        Text sfxValue;
        bool refreshing;
        Image[] speedImages;
        static Sprite quad;

        public void Build(Font uiFont, LevelConfig[] levelList, LevelRunner levelRunner)
        {
            font = uiFont;
            levels = levelList;
            runner = levelRunner;
            root = Panel("Menu", transform, Vector2.zero, Vector2.one, new Color(Art.Ink.r, Art.Ink.g, Art.Ink.b, 0.72f));
            var sheet = Child(root.transform, "Sheet");
            var sheetRect = sheet.GetComponent<RectTransform>();
            sheetRect.anchorMin = sheetRect.anchorMax = new Vector2(0.5f, 0.5f);
            sheetRect.sizeDelta = new Vector2(920f, 1500f);
            var sheetImage = sheet.AddComponent<Image>();
            sheetImage.sprite = White();
            sheetImage.color = Art.Paper;
            home = Page(sheet.transform, "Home");
            levelPage = Page(sheet.transform, "Levels");
            settingsPage = Page(sheet.transform, "Settings");
            BuildHome();
            BuildLevels();
            BuildSettings();
            corner = MakeButton(transform, "Màn", new Vector2(120f, -70f), new Vector2(180f, 72f), 32, () => runner.OpenFrontMenu());
            var cornerRect = corner.GetComponent<RectTransform>();
            cornerRect.anchorMin = cornerRect.anchorMax = new Vector2(0f, 1f);
            cornerRect.pivot = new Vector2(0f, 1f);
            cornerRect.anchoredPosition = new Vector2(36f, -36f);
            Hide();
        }

        public void ShowHome()
        {
            if (root == null) return;
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            home.SetActive(true);
            levelPage.SetActive(false);
            settingsPage.SetActive(false);
            if (corner != null) corner.gameObject.SetActive(false);
            RefreshSettings();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            if (corner != null) corner.gameObject.SetActive(true);
            GameSettings.Save();
        }

        void BuildHome()
        {
            Label(home.transform, "Một Mâm", 72, Art.Ink, new Vector2(0f, 460f), new Vector2(800f, 100f));
            Label(home.transform, "한상", 40, Art.Son, new Vector2(0f, 370f), new Vector2(800f, 60f));
            MakeButton(home.transform, "Chơi", new Vector2(0f, 120f), new Vector2(520f, 110f), 40, () => runner.ContinueFromMenu());
            MakeButton(home.transform, "Chọn màn", new Vector2(0f, -30f), new Vector2(520f, 110f), 40, ShowLevels);
            MakeButton(home.transform, "Cài đặt", new Vector2(0f, -180f), new Vector2(520f, 110f), 40, ShowSettings);
        }

        void BuildLevels()
        {
            Label(levelPage.transform, "Chọn màn", 56, Art.Ink, new Vector2(0f, 620f), new Vector2(800f, 80f));
            int count = levels == null ? 0 : levels.Length;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                int col = i % 4;
                int row = i / 4;
                var pos = new Vector2((col - 1.5f) * 200f, 420f - row * 200f);
                string guest = levels[i] == null ? "" : Art.GuestShort(levels[i].guestMode);
                string caption = (i + 1) + "\n" + guest;
                MakeButton(levelPage.transform, caption, pos, new Vector2(180f, 170f), 28, () => runner.BeginLevel(index));
            }
            MakeButton(levelPage.transform, "Đóng", new Vector2(0f, -620f), new Vector2(420f, 100f), 36, ShowHome);
        }

        void BuildSettings()
        {
            Label(settingsPage.transform, "Cài đặt", 56, Art.Ink, new Vector2(0f, 560f), new Vector2(800f, 80f));

            Label(settingsPage.transform, "Nhạc nền", 36, Art.Ink, new Vector2(0f, 400f), new Vector2(700f, 60f));
            musicSlider = MakeSlider(settingsPage.transform, new Vector2(-60f, 310f), new Vector2(580f, 64f), SetMusic);
            musicValue = Label(settingsPage.transform, "", 34, Art.Ink, new Vector2(320f, 310f), new Vector2(140f, 60f));

            Label(settingsPage.transform, "Hiệu ứng", 36, Art.Ink, new Vector2(0f, 190f), new Vector2(700f, 60f));
            sfxSlider = MakeSlider(settingsPage.transform, new Vector2(-60f, 100f), new Vector2(580f, 64f), SetSfx);
            sfxValue = Label(settingsPage.transform, "", 34, Art.Ink, new Vector2(320f, 100f), new Vector2(140f, 60f));

            Label(settingsPage.transform, "Tốc độ mâm", 36, Art.Ink, new Vector2(0f, -30f), new Vector2(700f, 60f));
            speedImages = new Image[3];
            string[] names = { "Chậm", "Vừa", "Nhanh" };
            for (int i = 0; i < 3; i++)
            {
                int step = i;
                var button = MakeButton(settingsPage.transform, names[i], new Vector2((i - 1) * 250f, -140f), new Vector2(220f, 100f), 32, () => SetSpeed(step));
                speedImages[i] = button.GetComponent<Image>();
            }
            MakeButton(settingsPage.transform, "Xem lại hướng dẫn", new Vector2(0f, -300f), new Vector2(520f, 100f), 34, () => runner.RestartWithTutorials());
            MakeButton(settingsPage.transform, "Đóng", new Vector2(0f, -450f), new Vector2(420f, 100f), 36, ShowHome);
        }

        // Thanh trượt 0–100%: nền mờ, phần đã kéo màu son, nút kéo vuông.
        Slider MakeSlider(Transform parent, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction<float> onChange)
        {
            const float knob = 56f;
            const float track = 18f;
            var go = Child(parent, "Slider");
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            // Vùng chạm phủ cả thanh cho dễ bấm trên điện thoại.
            var hit = go.AddComponent<Image>();
            hit.sprite = White();
            hit.color = new Color(1f, 1f, 1f, 0f);

            var bg = Child(go.transform, "Background");
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(1f, 0.5f);
            bgRect.offsetMin = new Vector2(knob * 0.5f, -track * 0.5f);
            bgRect.offsetMax = new Vector2(-knob * 0.5f, track * 0.5f);
            var bgImage = bg.AddComponent<Image>();
            bgImage.sprite = White();
            bgImage.color = new Color(Art.Ink.r, Art.Ink.g, Art.Ink.b, 0.22f);
            bgImage.raycastTarget = false;

            var fillArea = Child(go.transform, "Fill Area");
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = bgRect.anchorMin;
            fillAreaRect.anchorMax = bgRect.anchorMax;
            fillAreaRect.offsetMin = bgRect.offsetMin;
            fillAreaRect.offsetMax = bgRect.offsetMax;
            var fill = Child(fillArea.transform, "Fill");
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.sprite = White();
            fillImage.color = Art.Son;
            fillImage.raycastTarget = false;

            var handleArea = Child(go.transform, "Handle Slide Area");
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(knob * 0.5f, 0f);
            handleAreaRect.offsetMax = new Vector2(-knob * 0.5f, 0f);
            var handle = Child(handleArea.transform, "Handle");
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(knob, 0f);
            var handleImage = handle.AddComponent<Image>();
            handleImage.sprite = White();
            handleImage.color = Art.Son;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.onValueChanged.AddListener(onChange);
            return slider;
        }

        void SetMusic(float value)
        {
            if (refreshing) return;
            GameSettings.MusicVolume = value;
            RefreshSettings();
        }

        void SetSfx(float value)
        {
            if (refreshing) return;
            GameSettings.SfxVolume = value;
            runner.PreviewSfx();
            RefreshSettings();
        }

        static string Percent(float value)
        {
            return value <= 0.001f ? "Tắt" : Mathf.RoundToInt(value * 100f) + "%";
        }

        void ShowLevels()
        {
            home.SetActive(false);
            settingsPage.SetActive(false);
            levelPage.SetActive(true);
        }

        void ShowSettings()
        {
            home.SetActive(false);
            levelPage.SetActive(false);
            settingsPage.SetActive(true);
            RefreshSettings();
        }

        void SetSpeed(int step)
        {
            GameSettings.SpeedStep = step;
            runner.ApplySpinSetting();
            RefreshSettings();
        }

        void RefreshSettings()
        {
            refreshing = true;
            float music = GameSettings.MusicVolume;
            float effects = GameSettings.SfxVolume;
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(effects);
            if (musicValue != null) musicValue.text = Percent(music);
            if (sfxValue != null) sfxValue.text = Percent(effects);
            refreshing = false;
            if (speedImages == null) return;
            for (int i = 0; i < speedImages.Length; i++)
            {
                if (speedImages[i] == null) continue;
                speedImages[i].color = i == GameSettings.SpeedStep ? Art.Son : Art.Ink;
            }
        }

        GameObject Page(Transform parent, string pageName)
        {
            var page = Child(parent, pageName);
            var rect = page.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return page;
        }

        static GameObject Panel(string panelName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = Child(parent, panelName);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.sprite = White();
            image.color = color;
            image.raycastTarget = true;
            return go;
        }

        static GameObject Child(Transform parent, string childName)
        {
            var go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        Text Label(Transform parent, string text, int size, Color color, Vector2 pos, Vector2 box)
        {
            var go = Child(parent, string.IsNullOrEmpty(text) ? "Label" : text);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = box;
            rect.anchoredPosition = pos;
            var label = go.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        Button MakeButton(Transform parent, string caption, Vector2 pos, Vector2 size, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var go = Child(parent, caption.Replace("\n", " "));
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var image = go.AddComponent<Image>();
            image.sprite = White();
            image.color = Art.Son;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var textGo = Child(go.transform, "Text");
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 8f);
            textRect.offsetMax = new Vector2(-8f, -8f);
            var label = textGo.AddComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Art.Paper;
            label.text = caption;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return button;
        }

        static Sprite White()
        {
            if (quad != null) return quad;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            quad = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return quad;
        }
    }
}
