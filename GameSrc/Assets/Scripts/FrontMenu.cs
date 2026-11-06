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
        Text soundLabel;
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
            Label(settingsPage.transform, "Cài đặt", 56, Art.Ink, new Vector2(0f, 460f), new Vector2(800f, 80f));
            Label(settingsPage.transform, "Âm thanh", 36, Art.Ink, new Vector2(0f, 280f), new Vector2(700f, 60f));
            var sound = MakeButton(settingsPage.transform, "Bật", new Vector2(0f, 180f), new Vector2(420f, 100f), 36, ToggleSound);
            soundLabel = sound.GetComponentInChildren<Text>();
            Label(settingsPage.transform, "Tốc độ mâm", 36, Art.Ink, new Vector2(0f, 40f), new Vector2(700f, 60f));
            speedImages = new Image[3];
            string[] names = { "Chậm", "Vừa", "Nhanh" };
            for (int i = 0; i < 3; i++)
            {
                int step = i;
                var button = MakeButton(settingsPage.transform, names[i], new Vector2((i - 1) * 250f, -80f), new Vector2(220f, 100f), 32, () => SetSpeed(step));
                speedImages[i] = button.GetComponent<Image>();
            }
            MakeButton(settingsPage.transform, "Đóng", new Vector2(0f, -360f), new Vector2(420f, 100f), 36, ShowHome);
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

        void ToggleSound()
        {
            GameSettings.Sound = !GameSettings.Sound;
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
            if (soundLabel != null) soundLabel.text = GameSettings.Sound ? "Bật" : "Tắt";
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

        void Label(Transform parent, string text, int size, Color color, Vector2 pos, Vector2 box)
        {
            var go = Child(parent, text);
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
