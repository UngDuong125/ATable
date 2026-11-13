using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MotMam
{
    // Lớp phủ hướng dẫn: nền tối có một lỗ sáng, hộp thoại, nút Bỏ qua, và nút "?" để xem lại.
    // Tạo hết bằng code giống FrontMenu, không cần dựng trong scene.
    public class TutorialOverlay : MonoBehaviour
    {
        const float CharsPerSecond = 50f;
        const float HoleEase = 10f;
        const float EatPause = 0.7f;
        static readonly Color Shade = new(0.05f, 0.04f, 0.035f, 0.80f);

        enum Slot { Middle, Top, Bottom }

        LevelRunner runner;
        Font font;
        Camera uiCamera;

        RectTransform root;
        readonly RectTransform[] shades = new RectTransform[4];
        RectTransform holeRect;
        Image holeImage;
        Image holeGlow;
        GameObject catcher;

        RectTransform dialog;
        CanvasGroup dialogGroup;
        Image accentBar;
        Image portrait;
        Text nameText;
        Text bodyText;
        Text footText;
        RectTransform bodyRect;
        RectTransform nameRect;

        RectTransform indicator;
        Image indicatorRing;
        Image indicatorPad;
        GameObject indicatorLabel;
        Text indicatorText;

        Button replayButton;

        static Sprite white;
        static Sprite circleHole;
        static Sprite roundedHole;
        static Sprite ring;
        static Sprite softDisc;

        TutorialScript script;
        int step;
        string fullText = "";
        float typed;
        float stepTime;
        bool waiting;
        bool holeReady;
        Vector2 holeCenter;
        Vector2 holeSize;
        Slot slot;

        public bool Running => script != null;

        public void Build(Font uiFont, LevelRunner owner)
        {
            font = uiFont;
            runner = owner;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) uiCamera = canvas.worldCamera;

            root = Stretch(Child(transform, "Tutorial"));

            string[] shadeNames = { "ShadeTop", "ShadeBottom", "ShadeLeft", "ShadeRight" };
            for (int i = 0; i < 4; i++)
            {
                var rect = Child(root, shadeNames[i]);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = Vector2.zero;
                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = White();
                image.color = Shade;
                image.raycastTarget = true;
                shades[i] = rect;
            }

            holeRect = Child(root, "Hole");
            holeRect.anchorMin = holeRect.anchorMax = new Vector2(0.5f, 0.5f);
            holeImage = holeRect.gameObject.AddComponent<Image>();
            holeImage.color = Shade;
            holeImage.raycastTarget = false;

            var glow = Child(holeRect, "Glow");
            Stretch(glow);
            holeGlow = glow.gameObject.AddComponent<Image>();
            holeGlow.sprite = Ring();
            holeGlow.color = new Color(Art.Paper.r, Art.Paper.g, Art.Paper.b, 0.5f);
            holeGlow.raycastTarget = false;

            var catchRect = Stretch(Child(root, "TapCatcher"));
            var catchImage = catchRect.gameObject.AddComponent<Image>();
            catchImage.color = new Color(0f, 0f, 0f, 0f);
            var catchButton = catchRect.gameObject.AddComponent<Button>();
            catchButton.transition = Selectable.Transition.None;
            catchButton.onClick.AddListener(OnCatcherClick);
            catcher = catchRect.gameObject;

            BuildIndicator();
            BuildDialog();

            var skip = MakeButton(root, "Bỏ qua", new Vector2(220f, 80f), 32, Skip);
            var skipRect = (RectTransform)skip.transform;
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(1f, 1f);
            skipRect.pivot = new Vector2(1f, 1f);
            skipRect.anchoredPosition = new Vector2(-36f, -36f);

            replayButton = MakeButton(transform, "?", new Vector2(96f, 72f), 44, () => runner.ReplayTutorial());
            var replayRect = (RectTransform)replayButton.transform;
            replayRect.anchorMin = replayRect.anchorMax = new Vector2(1f, 1f);
            replayRect.pivot = new Vector2(1f, 1f);
            replayRect.anchoredPosition = new Vector2(-36f, -36f);

            root.gameObject.SetActive(false);
        }

        public void Play(TutorialScript next)
        {
            if (next == null || next.steps.Count == 0) return;
            StopAllCoroutines();
            script = next;
            step = -1;
            holeReady = false;
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            runner.OnTutorialStarted();
            Next();
        }

        // Dừng ngay, không đánh dấu đã xem (đổi màn, mở menu).
        public void Abort()
        {
            if (!Running) return;
            End(false);
        }

        public void Skip()
        {
            if (!Running) return;
            runner.PlayTutorialTick();
            End(true);
        }

        // LevelRunner gọi sau mỗi nhát cắn.
        public void OnBite(BiteOutcome outcome)
        {
            if (!Running || waiting || step < 0 || step >= script.steps.Count) return;
            var current = script.steps[step];
            if (current.action == TutorialAction.Say) return;

            if (outcome == BiteOutcome.Eat)
            {
                waiting = true;
                runner.SetTutorialHold(true);
                StartCoroutine(AfterEat());
                return;
            }

            string retry = outcome == BiteOutcome.Foul ? current.retryFoul : current.retryMiss;
            SetText(string.IsNullOrEmpty(retry) ? current.text : retry);
        }

        IEnumerator AfterEat()
        {
            yield return new WaitForSecondsRealtime(EatPause);
            if (!Running) yield break;
            if (runner.LevelCleared) End(true);
            else Next();
        }

        void End(bool markSeen)
        {
            if (markSeen) TutorialContent.MarkSeen(script);
            StopAllCoroutines();
            script = null;
            waiting = false;
            root.gameObject.SetActive(false);
            runner.OnTutorialEnded();
        }

        void Next()
        {
            step++;
            waiting = false;
            if (step >= script.steps.Count)
            {
                End(true);
                return;
            }

            var current = script.steps[step];
            bool say = current.action == TutorialAction.Say;
            catcher.SetActive(say);
            runner.SetTutorialHold(say);
            stepTime = 0f;
            dialogGroup.alpha = 0f;

            bool speaking = current.hasSpeaker;
            Sprite face = null;
            Color faceTint = Color.white;
            if (speaking) runner.TryPortrait(current.speaker, out face, out faceTint);
            portrait.gameObject.SetActive(speaking && face != null);
            if (face != null)
            {
                portrait.sprite = face;
                portrait.color = faceTint;
            }
            nameText.gameObject.SetActive(speaking);
            Color accent = speaking ? Art.Accent(current.speaker) : Art.Son;
            accentBar.color = accent;
            nameText.color = accent;
            if (speaking) nameText.text = GuestName(current.speaker);

            float left = speaking && face != null ? 250f : 44f;
            nameRect.offsetMin = new Vector2(left, nameRect.offsetMin.y);
            bodyRect.offsetMin = new Vector2(left, bodyRect.offsetMin.y);
            bodyRect.offsetMax = new Vector2(-40f, speaking ? -84f : -40f);

            SetText(current.text);
            indicatorText.text = current.action == TutorialAction.Hold ? "Nhấn giữ" : "Chạm";
        }

        void SetText(string text)
        {
            fullText = text ?? "";
            typed = 0f;
            bodyText.text = "";
        }

        void OnCatcherClick()
        {
            if (!Running || stepTime < 0.15f) return;
            if (typed < fullText.Length)
            {
                typed = fullText.Length;
                bodyText.text = fullText;
                return;
            }
            runner.PlayTutorialTick();
            Next();
        }

        void Update()
        {
            if (replayButton != null)
            {
                bool showReplay = !Running && runner != null && runner.CanReplayTutorial;
                if (replayButton.gameObject.activeSelf != showReplay) replayButton.gameObject.SetActive(showReplay);
            }
            if (!Running || step < 0 || step >= script.steps.Count) return;

            if (root.GetSiblingIndex() != root.parent.childCount - 1) root.SetAsLastSibling();

            float dt = Time.unscaledDeltaTime;
            stepTime += dt;
            var current = script.steps[step];

            UpdateHole(current, dt);
            UpdateDialog(current, dt);
            UpdateIndicator(current);
        }

        void UpdateHole(TutorialStep current, float dt)
        {
            Vector2 targetCenter = holeCenter;
            Vector2 targetSize = Vector2.zero;
            bool round = true;
            bool focused = current.focus != TutorialFocus.None &&
                           runner.TryTutorialFocus(current.focus, out Rect screen, out round) &&
                           ToLocal(screen, out targetCenter, out targetSize);
            if (!focused) targetCenter = holeReady ? holeCenter : Vector2.zero;

            if (!holeReady)
            {
                holeCenter = targetCenter;
                holeSize = focused ? targetSize * 1.6f : Vector2.zero;
                holeReady = true;
            }
            float k = 1f - Mathf.Exp(-HoleEase * dt);
            holeCenter = Vector2.Lerp(holeCenter, targetCenter, k);
            holeSize = Vector2.Lerp(holeSize, targetSize, k);

            holeImage.sprite = round ? CircleHole() : RoundedHole();
            holeImage.type = round ? Image.Type.Simple : Image.Type.Sliced;
            holeGlow.sprite = round ? Ring() : null;
            holeGlow.enabled = round && holeSize.x > 4f;
            if (holeGlow.enabled)
            {
                float pulse = 0.25f + 0.2f * Mathf.Sin(Time.unscaledTime * 3f);
                holeGlow.color = new Color(Art.Paper.r, Art.Paper.g, Art.Paper.b, pulse);
            }

            slot = !focused ? Slot.Middle : (targetCenter.y > root.rect.height * 0.05f ? Slot.Bottom : Slot.Top);
            LayoutHole();
        }

        void LayoutHole()
        {
            Rect r = root.rect;
            Vector2 half = holeSize * 0.5f;
            float left = Mathf.Clamp(holeCenter.x - half.x, r.xMin, r.xMax);
            float right = Mathf.Clamp(holeCenter.x + half.x, r.xMin, r.xMax);
            float bottom = Mathf.Clamp(holeCenter.y - half.y, r.yMin, r.yMax);
            float top = Mathf.Clamp(holeCenter.y + half.y, r.yMin, r.yMax);
            Place(shades[0], r.xMin, top, r.xMax, r.yMax);
            Place(shades[1], r.xMin, r.yMin, r.xMax, bottom);
            Place(shades[2], r.xMin, bottom, left, top);
            Place(shades[3], right, bottom, r.xMax, top);

            bool open = holeSize.x > 1f && holeSize.y > 1f;
            holeImage.enabled = open;
            holeRect.anchoredPosition = holeCenter;
            holeRect.sizeDelta = holeSize;
        }

        static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchoredPosition = new Vector2(x0, y0);
            rect.sizeDelta = new Vector2(Mathf.Max(0f, x1 - x0), Mathf.Max(0f, y1 - y0));
        }

        void UpdateDialog(TutorialStep current, float dt)
        {
            if (typed < fullText.Length)
            {
                typed = Mathf.Min(fullText.Length, typed + CharsPerSecond * dt);
                bodyText.text = fullText.Substring(0, Mathf.FloorToInt(typed));
            }
            dialogGroup.alpha = Mathf.MoveTowards(dialogGroup.alpha, 1f, dt * 6f);

            bool say = current.action == TutorialAction.Say;
            bool done = typed >= fullText.Length;
            footText.gameObject.SetActive(say && done);
            if (say && done)
            {
                float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 4f);
                footText.color = new Color(Art.Ink.r, Art.Ink.g, Art.Ink.b, pulse);
            }

            Vector2 anchor;
            Vector2 pos;
            if (slot == Slot.Top) { anchor = new Vector2(0.5f, 1f); pos = new Vector2(0f, -190f); }
            else if (slot == Slot.Bottom) { anchor = new Vector2(0.5f, 0f); pos = new Vector2(0f, 300f); }
            else { anchor = new Vector2(0.5f, 0.5f); pos = Vector2.zero; }
            dialog.anchorMin = dialog.anchorMax = dialog.pivot = anchor;
            dialog.anchoredPosition = pos;
        }

        void UpdateIndicator(TutorialStep current)
        {
            bool act = current.action != TutorialAction.Say && !waiting;
            TutorialFocus spotFocus = current.focus == TutorialFocus.ZoneRight ? TutorialFocus.PressSpotRight : TutorialFocus.PressSpot;
            Vector2 center = Vector2.zero;
            Vector2 size = Vector2.zero;
            bool placed = act && runner.TryTutorialFocus(spotFocus, out Rect spot, out _) &&
                          ToLocal(spot, out center, out size);
            if (indicator.gameObject.activeSelf != placed) indicator.gameObject.SetActive(placed);
            if (!placed) return;
            indicator.anchoredPosition = center;
            indicator.sizeDelta = size;

            // Đang giữ: ngón tay đã đặt lên ô, ẩn vòng và chữ cho khỏi rối.
            bool holding = runner.PlayerHolding;
            indicatorRing.gameObject.SetActive(!holding);
            indicatorLabel.SetActive(!holding);
            indicatorPad.color = new Color(Art.Paper.r, Art.Paper.g, Art.Paper.b, holding ? 0.55f : 0.35f);
            if (holding) return;

            float t = Time.unscaledTime;
            float scale;
            float alpha;
            if (current.action == TutorialAction.Hold)
            {
                float u = Mathf.Repeat(t * 0.8f, 1f);
                scale = Mathf.Lerp(0.6f, 1.25f, u);
                alpha = 1f - u * 0.8f;
            }
            else
            {
                float u = Mathf.Repeat(t * 1.6f, 1f);
                scale = u < 0.25f ? Mathf.Lerp(1f, 0.75f, u / 0.25f) : Mathf.Lerp(0.75f, 1f, (u - 0.25f) / 0.75f);
                alpha = 1f;
            }
            indicatorRing.rectTransform.localScale = new Vector3(scale, scale, 1f);
            indicatorRing.color = new Color(Art.Paper.r, Art.Paper.g, Art.Paper.b, alpha);
        }

        bool ToLocal(Rect screen, out Vector2 center, out Vector2 size)
        {
            center = Vector2.zero;
            size = Vector2.zero;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen.min, uiCamera, out Vector2 a)) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen.max, uiCamera, out Vector2 b)) return false;
            center = (a + b) * 0.5f;
            size = new Vector2(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
            return true;
        }

        void BuildIndicator()
        {
            indicator = Child(root, "TapHere");
            indicator.anchorMin = indicator.anchorMax = new Vector2(0.5f, 0.5f);
            indicator.sizeDelta = new Vector2(200f, 200f);
            // Ô sáng nằm trên lớp tối nhưng nhấn vào vẫn tính là nhấn mâm.
            indicatorPad = indicator.gameObject.AddComponent<Image>();
            indicatorPad.sprite = SoftDisc();
            indicatorPad.raycastTarget = true;
            indicator.gameObject.AddComponent<UiPassThrough>();

            var ringRect = Stretch(Child(indicator, "Ring"));
            indicatorRing = ringRect.gameObject.AddComponent<Image>();
            indicatorRing.sprite = Ring();
            indicatorRing.raycastTarget = false;

            var labelBack = Child(indicator, "LabelBack");
            labelBack.anchorMin = labelBack.anchorMax = new Vector2(0.5f, 0.5f);
            labelBack.sizeDelta = new Vector2(200f, 60f);
            var back = labelBack.gameObject.AddComponent<Image>();
            back.sprite = White();
            back.color = Art.Son;
            back.raycastTarget = false;

            indicatorText = MakeText(labelBack, 32, Art.Paper, TextAnchor.MiddleCenter);
            Stretch(indicatorText.rectTransform);
            indicatorLabel = labelBack.gameObject;
            indicator.gameObject.SetActive(false);
        }

        void BuildDialog()
        {
            dialog = Child(root, "Dialog");
            dialog.sizeDelta = new Vector2(960f, 330f);
            dialogGroup = dialog.gameObject.AddComponent<CanvasGroup>();
            var panel = dialog.gameObject.AddComponent<Image>();
            panel.sprite = White();
            panel.color = Art.Paper;
            panel.raycastTarget = true;
            var outline = dialog.gameObject.AddComponent<Outline>();
            outline.effectColor = Art.Ink;
            outline.effectDistance = new Vector2(4f, -4f);

            var bar = Child(dialog, "Accent");
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.sizeDelta = new Vector2(0f, 12f);
            accentBar = bar.gameObject.AddComponent<Image>();
            accentBar.sprite = White();
            accentBar.raycastTarget = false;

            var face = Child(dialog, "Portrait");
            face.anchorMin = face.anchorMax = new Vector2(0f, 0.5f);
            face.pivot = new Vector2(0f, 0.5f);
            face.sizeDelta = new Vector2(200f, 200f);
            face.anchoredPosition = new Vector2(30f, 0f);
            portrait = face.gameObject.AddComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;

            nameText = MakeText(dialog, 38, Art.Son, TextAnchor.UpperLeft);
            nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.offsetMin = new Vector2(250f, -84f);
            nameRect.offsetMax = new Vector2(-40f, -30f);
            nameText.fontStyle = FontStyle.Bold;

            bodyText = MakeText(dialog, 38, Art.Ink, TextAnchor.UpperLeft);
            bodyRect = bodyText.rectTransform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(250f, 64f);
            bodyRect.offsetMax = new Vector2(-40f, -84f);
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Truncate;
            bodyText.resizeTextForBestFit = true;
            bodyText.resizeTextMinSize = 22;
            bodyText.resizeTextMaxSize = 38;
            bodyText.lineSpacing = 1.1f;

            footText = MakeText(dialog, 26, Art.Ink, TextAnchor.LowerRight);
            var footRect = footText.rectTransform;
            footRect.anchorMin = new Vector2(0f, 0f);
            footRect.anchorMax = new Vector2(1f, 0f);
            footRect.pivot = new Vector2(0.5f, 0f);
            footRect.offsetMin = new Vector2(40f, 18f);
            footRect.offsetMax = new Vector2(-36f, 60f);
            footText.text = "Chạm để tiếp tục";
        }

        static string GuestName(GuestId id)
        {
            switch (id)
            {
                case GuestId.Ho: return "Hổ";
                case GuestId.Me: return "Mê";
                case GuestId.Co: return "Cò";
                case GuestId.Dokkaebi: return "Dokkaebi";
                default: return "Gumiho";
            }
        }

        Text MakeText(RectTransform parent, int size, Color color, TextAnchor align)
        {
            var rect = Child(parent, "Text");
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        Button MakeButton(Transform parent, string caption, Vector2 size, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var rect = Child(parent, "Button " + caption);
            rect.sizeDelta = size;
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = White();
            image.color = Art.Son;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var label = MakeText(rect, fontSize, Art.Paper, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.text = caption;
            return button;
        }

        static RectTransform Child(Transform parent, string childName)
        {
            var go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        static Sprite White()
        {
            if (white != null) return white;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            white = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return white;
        }

        // Ô vuông trắng, giữa có lỗ tròn trong suốt, mép mềm.
        static Sprite CircleHole()
        {
            if (circleHole != null) return circleHole;
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float c = (n - 1) * 0.5f;
            const float feather = 6f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    float a = Mathf.Clamp01((d - (c - feather)) / feather);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            circleHole = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
            return circleHole;
        }

        // Lỗ chữ nhật bo góc, dùng kiểu Sliced để kéo giãn theo khung icon.
        static Sprite RoundedHole()
        {
            if (roundedHole != null) return roundedHole;
            const int n = 64;
            const float radius = 24f;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Max(0f, Mathf.Max(radius - x, x - (n - 1 - radius)));
                    float dy = Mathf.Max(0f, Mathf.Max(radius - y, y - (n - 1 - radius)));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(d - radius + 1f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            roundedHole = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius + 2f, radius + 2f, radius + 2f, radius + 2f));
            return roundedHole;
        }

        // Đĩa tròn sáng, mép mờ dần.
        static Sprite SoftDisc()
        {
            if (softDisc != null) return softDisc;
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float c = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                    float a = 1f - Mathf.SmoothStep(0.55f, 1f, d);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            softDisc = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
            return softDisc;
        }

        static Sprite Ring()
        {
            if (ring != null) return ring;
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float c = (n - 1) * 0.5f;
            float outer = c;
            float inner = c * 0.86f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    float a = d <= outer && d >= inner ? Mathf.Clamp01(Mathf.Min(d - inner, outer - d)) : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            ring = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
            return ring;
        }
    }
}
