using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotMam
{
    [RequireComponent(typeof(TableSpinner))]
    [RequireComponent(typeof(BiteInput))]
    public class LevelRunner : MonoBehaviour
    {
        enum Phase { Play, Clear, Fail }

        [SerializeField] LevelConfig[] levels;
        [SerializeField] Transform tray;
        [SerializeField] FoodItem foodPrefab;
        [SerializeField] SkillZone zone;
        [SerializeField] GuestView guest;
        [SerializeField] SkillZone hoZone;
        [SerializeField] GuestView hoGuest;
        [SerializeField] SkillZone meZone;
        [SerializeField] GuestView meGuest;
        [SerializeField] SkillZone coZone;
        [SerializeField] GuestView coGuest;
        [SerializeField] SkillZone dokkaZone;
        [SerializeField] GuestView dokkaGuest;
        [SerializeField] LevelHud hud;
        [SerializeField] DishSprite[] dishes;
        [SerializeField] Sprite happySprite;
        [SerializeField] Sprite sadSprite;

        // Biểu cảm hiện ở góc trên-phải khách sau mỗi nhát cắn.
        const float MoodSeconds = 1f;
        const float MoodPopSeconds = 0.2f;
        const float MoodFadeSeconds = 0.25f;
        const float MoodSize = 0.5f;

        TableSpinner spinner;
        BiteInput input;
        Sfx sfx;
        Vector3 trayHome;
        Sprite aimSprite;
        bool hintActive;
        readonly List<FoodItem> foods = new();

        int index;
        LevelConfig level;
        Phase phase;
        bool pressArmed;
        bool ready;
        int hearts;
        int charges;
        int bites;
        bool swapped;
        float shownHold;
        float biteShowLeft;
        float turnAwayLeft;
        ScreenSide activeSide;
        GuestId lastBiter;
        float gumihoAnchor;
        GameObject clearRing;
        Sprite clearRingSprite;
        float hoAnchor;
        float meAnchor;
        float meDisplayAnchor;
        float slideFrom;
        float slideLeft;
        bool sliding;
        float meClock;
        float meSway;
        float dokkaClock;
        float dokkaAlong;
        bool menuOpen;
        FrontMenu front;
        Camera cam;
        float baseOrtho;
        bool orthoRemembered;
        TutorialOverlay tutorial;
        bool tutorialHold;
        bool forgiving;
        SpriteRenderer mood;
        float moodLeft;
        GuestId moodGuest;

        bool TutorialRunning => tutorial != null && tutorial.Running;
        public bool LevelCleared => phase == Phase.Clear;
        public bool PlayerHolding => pressArmed && spinner.Holding;
        public bool CanReplayTutorial => !menuOpen && TutorialContent.ForReplay(levels, index) != null;

        void Awake()
        {
            spinner = GetComponent<TableSpinner>();
            input = GetComponent<BiteInput>();
            sfx = gameObject.AddComponent<Sfx>();
            if (tray != null) trayHome = tray.localPosition;
            if (spinner.Tray == null) spinner.Tray = tray;
            AttachMusic();
            GameSettings.ApplySound();
            EnsureExtraGuests();
            ready = HasReferences();
            if (!ready) enabled = false;
        }

        void OnEnable()
        {
            if (!ready) return;
            input.Pressed += OnPress;
            input.Released += OnRelease;
            input.Cancelled += OnCancel;
        }

        void OnDisable()
        {
            if (input == null) return;
            input.Pressed -= OnPress;
            input.Released -= OnRelease;
            input.Cancelled -= OnCancel;
        }

        void Start()
        {
            if (!ready) return;
            if (hud != null)
            {
                front = hud.gameObject.AddComponent<FrontMenu>();
                front.Build(hud.UiFont, levels, this);
                tutorial = hud.gameObject.AddComponent<TutorialOverlay>();
                tutorial.Build(hud.UiFont, this);
            }
            Load(0);
            OpenFrontMenu();
        }

        void Update()
        {
            if (biteShowLeft > 0f)
            {
                biteShowLeft -= Time.deltaTime;
                if (biteShowLeft <= 0f) shownHold = 0f;
            }
            else if (phase == Phase.Play && spinner.Holding)
            {
                shownHold = spinner.HoldSeconds;
            }

            if (turnAwayLeft > 0f) turnAwayLeft -= Time.deltaTime;
            if (!menuOpen) UpdateSlide();
            RefreshMeSway(true);
            RefreshDokkaAlong(true);
            PlaceGuests();
            ApplyTurnAway();
            TickMood();
            PlaceZones();
            PulseWants();
            UpdateAim();
            if (hud != null) hud.TickHint(hintActive && !menuOpen && !TutorialRunning && phase == Phase.Play);
            if (hud == null || level == null) return;
            hud.SetHold(GameRules.HoldU(shownHold), Art.Accent(HoldingGuest()));
            hud.SetStats(level.id, hearts, charges);
            FitCamera(level.guestMode == GuestMode.GumihoHo);
        }

        public void OpenFrontMenu()
        {
            if (front == null) return;
            if (tutorial != null) tutorial.Abort();
            menuOpen = true;
            pressArmed = false;
            if (spinner.Holding) spinner.EndHold();
            shownHold = 0f;
            biteShowLeft = 0f;
            HideMood();
            spinner.Paused = true;
            input.enabled = false;
            front.ShowHome();
        }

        public void CloseFrontMenu()
        {
            menuOpen = false;
            input.enabled = true;
            if (front != null) front.Hide();
            if (phase == Phase.Play) spinner.Paused = false;
            MaybeAutoTutorial();
        }

        public void ContinueFromMenu()
        {
            if (phase == Phase.Play) CloseFrontMenu();
            else BeginLevel(index);
        }

        public void BeginLevel(int levelIndex)
        {
            menuOpen = false;
            input.enabled = true;
            if (front != null) front.Hide();
            Load(levelIndex);
            MaybeAutoTutorial();
        }

        // Bắt đầu lại từ màn 1 với toàn bộ hướng dẫn (nút trong Cài đặt).
        public void RestartWithTutorials()
        {
            TutorialContent.ResetAll();
            BeginLevel(0);
        }

        // Nút "?": chơi lại màn hiện tại kèm hướng dẫn của màn (hoặc của khách đang ngồi).
        public void ReplayTutorial()
        {
            if (menuOpen || tutorial == null) return;
            var script = TutorialContent.ForReplay(levels, index);
            if (script == null) return;
            Load(index);
            tutorial.Play(script);
        }

        void MaybeAutoTutorial()
        {
            if (tutorial == null || TutorialRunning || menuOpen || level == null) return;
            if (phase != Phase.Play || bites > 0) return;
            var script = TutorialContent.ForLevel(level.id);
            if (script == null || TutorialContent.Seen(script)) return;
            tutorial.Play(script);
        }

        public void OnTutorialStarted()
        {
            forgiving = true;
        }

        public void OnTutorialEnded()
        {
            forgiving = false;
            SetTutorialHold(false);
        }

        // Đang hiện hộp thoại: dừng mâm và không nhận nhát cắn.
        public void SetTutorialHold(bool on)
        {
            tutorialHold = on;
            if (on)
            {
                pressArmed = false;
                if (spinner.Holding) spinner.EndHold();
                if (biteShowLeft <= 0f) shownHold = 0f;
                spinner.Paused = true;
            }
            else if (phase == Phase.Play && !menuOpen)
            {
                spinner.Paused = false;
            }
        }

        public void PlayTutorialTick()
        {
            sfx.Play(SfxKind.Press, 0.6f);
        }

        public bool TryPortrait(GuestId id, out Sprite sprite, out Color tint)
        {
            sprite = null;
            tint = Color.white;
            var view = ViewOf(id);
            if (view == null) return false;
            var paint = view.GetComponentInChildren<SpriteRenderer>(true);
            if (paint == null || paint.sprite == null) return false;
            sprite = paint.sprite;
            tint = paint.color;
            tint.a = 1f;
            return true;
        }

        // Vị trí trên màn hình (pixel) của chỗ cần chiếu sáng. round = lỗ tròn.
        public bool TryTutorialFocus(TutorialFocus focus, out Rect screen, out bool round)
        {
            screen = default;
            round = true;
            if (level == null) return false;
            bool dual = level.guestMode == GuestMode.GumihoHo;
            float r = GameRules.TrayWorldRadius;
            switch (focus)
            {
                case TutorialFocus.Guest:
                case TutorialFocus.GuestRight:
                {
                    var view = dual
                        ? (focus == TutorialFocus.GuestRight ? hoGuest : guest)
                        : ViewOf(PrimaryGuest());
                    if (view == null || !view.gameObject.activeInHierarchy) return false;
                    return WorldCircle(view.transform.position, 0.42f * r, out screen);
                }
                case TutorialFocus.Zone:
                case TutorialFocus.ZoneRight:
                {
                    float anchor = FocusAnchor(focus == TutorialFocus.ZoneRight);
                    float along = dual ? 0.8f : 0.72f;
                    float size = dual ? 0.8f : 0.75f;
                    return WorldCircle(GameRules.ToWorld(GameRules.Point(anchor, along)), size * r, out screen);
                }
                case TutorialFocus.PressSpot:
                case TutorialFocus.PressSpotRight:
                {
                    bool right = focus == TutorialFocus.PressSpotRight;
                    var view = dual ? (right ? hoGuest : guest) : ViewOf(PrimaryGuest());
                    if (view == null || !view.gameObject.activeInHierarchy) return false;
                    float a = FocusAnchor(right) * Mathf.Deg2Rad;
                    var outward = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    // Khách ngồi đáy thì perp hướng sang phải màn hình. Màn hai khách: luôn lệch xuống dưới.
                    var side = dual ? Vector2.down : new Vector2(-Mathf.Sin(a), Mathf.Cos(a));
                    Vector2 spot = (Vector2)view.transform.position + outward * ((dual ? 0f : PressSpotOut) * r) + side * (PressSpotSide * r);
                    return WorldCircle(spot, 0.28f * r, out screen);
                }
                case TutorialFocus.Tray:
                    return WorldCircle(tray != null ? tray.position : Vector3.zero, 1.08f * r, out screen);
                case TutorialFocus.Icons:
                case TutorialFocus.Stats:
                case TutorialFocus.HoldBar:
                    round = false;
                    return hud != null && hud.TryFocusRect(focus, out screen);
                default:
                    return false;
            }
        }

        // Ô nhấn của hướng dẫn: lùi ra ngoài mép mâm và lệch sang bên, đơn vị bán kính mâm.
        const float PressSpotOut = 0.32f;
        const float PressSpotSide = 0.5f;

        float FocusAnchor(bool right)
        {
            switch (level.guestMode)
            {
                case GuestMode.GumihoHo: return right ? hoAnchor : gumihoAnchor;
                case GuestMode.Ho: return hoAnchor;
                case GuestMode.Me: return meDisplayAnchor;
                case GuestMode.Co:
                case GuestMode.Dokkaebi: return SingleAnchor();
                default: return gumihoAnchor;
            }
        }

        bool WorldCircle(Vector3 center, float radius, out Rect screen)
        {
            screen = default;
            if (cam == null) cam = Camera.main;
            if (cam == null) return false;
            Vector3 c = cam.WorldToScreenPoint(new Vector3(center.x, center.y, 0f));
            Vector3 e = cam.WorldToScreenPoint(new Vector3(center.x + radius, center.y, 0f));
            float px = Mathf.Abs(e.x - c.x);
            screen = new Rect(c.x - px, c.y - px, px * 2f, px * 2f);
            return px > 0f;
        }

        GuestView ViewOf(GuestId id)
        {
            switch (id)
            {
                case GuestId.Ho: return hoGuest;
                case GuestId.Me: return meGuest;
                case GuestId.Co: return coGuest;
                case GuestId.Dokkaebi: return dokkaGuest;
                default: return guest;
            }
        }

        public void ApplySpinSetting()
        {
            if (level == null) return;
            spinner.SpinSpeed = level.spinSpeed * GameSettings.SpinScale;
        }

        public void OnNextClicked()
        {
            if (phase == Phase.Clear && index + 1 < levels.Length) Load(index + 1);
            else if (phase == Phase.Clear) Load(0);
            else if (phase == Phase.Fail) Load(index);
            MaybeAutoTutorial();
        }

        void OnPress(ScreenSide side)
        {
            if (menuOpen || tutorialHold || level == null || phase != Phase.Play || spinner.Locked) { pressArmed = false; return; }
            pressArmed = true;
            activeSide = side;
            spinner.BeginHold();
            sfx.Play(SfxKind.Press);
        }

        void OnCancel()
        {
            pressArmed = false;
            spinner.EndHold();
            shownHold = 0f;
            biteShowLeft = 0f;
        }

        void OnRelease(ScreenSide side)
        {
            if (!pressArmed || !spinner.Holding) { pressArmed = false; return; }
            pressArmed = false;
            activeSide = side;
            float hold = spinner.HoldSeconds;
            spinner.EndHold();
            Resolve(hold);
        }

        void Resolve(float hold)
        {
            if (level == null) return;
            GuestId biter = Biter(activeSide);
            lastBiter = biter;
            hintActive = false;
            shownHold = hold;
            biteShowLeft = GameRules.FeedbackLockSeconds;

            bool anyForbid = false;
            bool anyWant = false;
            bool foul = false; // a foul costs a heart, not a bite turn
            var hits = new List<FoodItem>();
            for (int i = 0; i < foods.Count; i++)
            {
                var food = foods[i];
                if (food.Eaten) continue;
                if (!InZone(food, biter, hold)) continue;
                hits.Add(food);
                if (food.ForbiddenTo(biter)) anyForbid = true;
                if (food.WantedBy(biter)) anyWant = true;
            }

            // Lúc đang hướng dẫn: không mất Mạng, không tốn Lượt.
            var outcome = GameRules.Judge(anyForbid, anyWant);
            switch (outcome)
            {
                case BiteOutcome.Foul:
                    if (!forgiving) hearts--;
                    foul = true;
                    turnAwayLeft = GameRules.FeedbackLockSeconds;
                    sfx.Play(SfxKind.Foul);
                    StartCoroutine(ShakeTray());
                    for (int i = 0; i < hits.Count; i++) StartCoroutine(Blink(hits[i]));
                    break;
                case BiteOutcome.Eat:
                    if (!forgiving) charges--;
                    sfx.Play(SfxKind.Eat);
                    for (int i = 0; i < hits.Count; i++)
                    {
                        if (!hits[i].WantedBy(biter)) continue;
                        hits[i].MarkEaten();
                        StartCoroutine(Pop(hits[i]));
                    }
                    break;
                default:
                    if (!forgiving) charges--;
                    sfx.Play(SfxKind.Miss);
                    break;
            }
            // Ăn đúng món thích: vui. Ăn nhầm hoặc cắn trượt: buồn.
            ShowMood(biter, outcome == BiteOutcome.Eat);

            if (!foul && !(forgiving && outcome == BiteOutcome.Miss)) bites++;
            bool slide = level.guestMode == GuestMode.Me &&
                         GameRules.MeSlides(GameRules.MeLength(hold), level.ResolvedCrossAt);
            bool willSwap = level.swapAfterBite > 0 && !swapped && bites >= level.swapAfterBite;
            bool alive = hearts > 0 && charges > 0;
            if (slide) BeginSlide();

            float lockTime = GameRules.FeedbackLockSeconds;
            if (slide) lockTime = Mathf.Max(lockTime, GameRules.MeSlideSeconds);
            if (willSwap && alive) lockTime = Mathf.Max(lockTime, GameRules.SwapBlinkSeconds);

            hud.ShowFoods(foods, level.guestMode);
            if (willSwap && alive) StartCoroutine(BlinkSwap());

            bool cleared = !willSwap && !AnyWantLeft();
            if (cleared && slide)
            {
                spinner.Lock(GameRules.MeSlideSeconds);
                StartCoroutine(FinishAfterSlide());
            }
            else if (cleared) Finish(Phase.Clear, "Qua màn", index + 1 < levels.Length ? "Màn sau" : "Từ đầu");
            else if (hearts <= 0) Finish(Phase.Fail, "Hết mạng", "Chơi lại");
            else if (charges <= 0) Finish(Phase.Fail, "Hết lượt", "Chơi lại");
            else spinner.Lock(lockTime);

            if (tutorial != null) tutorial.OnBite(outcome);
        }

        IEnumerator FinishAfterSlide()
        {
            while (sliding) yield return null;
            if (phase != Phase.Play) yield break;
            Finish(Phase.Clear, "Qua màn", index + 1 < levels.Length ? "Màn sau" : "Từ đầu");
        }

        void Finish(Phase next, string message, string button)
        {
            phase = next;
            spinner.Paused = true;
            sfx.Play(next == Phase.Clear ? SfxKind.Clear : SfxKind.Fail);
            hud.ShowResult(message, button);
            if (next == Phase.Clear) StartCoroutine(FlashClearRing());
        }

        void Load(int levelIndex)
        {
            if (levels == null || levelIndex < 0 || levelIndex >= levels.Length) return;
            var next = levels[levelIndex];
            if (next == null || next.foods == null)
            {
                Debug.LogError("Level asset trống.", this);
                return;
            }
            if (!Supports(next)) return;

            if (tutorial != null) tutorial.Abort();
            StopAllCoroutines();
            if (tray != null) tray.localPosition = trayHome;
            DestroyClearRing();
            index = levelIndex;
            level = next;
            hearts = level.hearts;
            charges = level.charges;
            bites = 0;
            swapped = false;
            phase = Phase.Play;
            pressArmed = false;
            shownHold = 0f;
            biteShowLeft = 0f;
            turnAwayLeft = 0f;
            HideMood();
            activeSide = ScreenSide.Left;
            lastBiter = PrimaryGuest();
            sliding = false;
            input.UseSplit(level.guestMode == GuestMode.GumihoHo);
            input.UseSideTag(level.guestMode == GuestMode.Dokkaebi);
            ResetAnchors();

            for (int i = foods.Count - 1; i >= 0; i--)
            {
                if (foods[i] != null) Destroy(foods[i].gameObject);
            }
            foods.Clear();

            var placed = level.foods;
            for (int i = 0; i < placed.Length; i++)
            {
                var food = Instantiate(foodPrefab, tray);
                food.name = placed[i].dishId;
                var normalized = GameRules.Point(placed[i].angleDeg, placed[i].radius);
                food.transform.localPosition = GameRules.ToWorld(normalized);
                food.transform.localRotation = Quaternion.identity;
                food.Apply(placed[i], SpriteFor(placed[i].dishId), level.guestMode);
                foods.Add(food);
            }

            spinner.SpinSpeed = level.spinSpeed * GameSettings.SpinScale;
            spinner.BrakeTime = level.ResolvedBrakeTime;
            spinner.ResetSpin();
            hud.HideResult();
            hintActive = !string.IsNullOrEmpty(level.hint);
            hud.SetHint(level.hint);
            hud.SetStats(level.id, hearts, charges);
            hud.SetHold(0f, Art.Accent(PrimaryGuest()));
            hud.ShowFoods(foods, level.guestMode);
        }

        void ResetAnchors()
        {
            bool dual = level.guestMode == GuestMode.GumihoHo;
            gumihoAnchor = dual ? GameRules.DualGumihoAnchor : GameRules.SingleAnchor;
            hoAnchor = dual ? GameRules.DualHoAnchor : GameRules.SingleAnchor;
            meAnchor = GameRules.SingleAnchor;
            meDisplayAnchor = meAnchor;
            slideLeft = 0f;
            meClock = 0f;
            meSway = 0f;
            dokkaClock = 0f;
            dokkaAlong = GameRules.DokkaAlong(0f);
            FitCamera(level.guestMode == GuestMode.GumihoHo);
        }

        void BeginSlide()
        {
            slideFrom = meDisplayAnchor;
            meAnchor = slideFrom + 180f;
            slideLeft = GameRules.MeSlideSeconds;
            sliding = true;
        }

        void UpdateSlide()
        {
            if (!sliding) return;
            slideLeft -= Time.deltaTime;
            float u = 1f - Mathf.Clamp01(slideLeft / GameRules.MeSlideSeconds);
            meDisplayAnchor = slideFrom + 180f * Mathf.SmoothStep(0f, 1f, u);
            if (slideLeft <= 0f)
            {
                sliding = false;
                meDisplayAnchor = meAnchor;
            }
        }

        void PlaceGuests()
        {
            if (level == null) return;
            bool dual = level.guestMode == GuestMode.GumihoHo;
            float gumihoRadius = dual ? GameRules.GuestDrawRadiusDual : GameRules.GuestDrawRadius;
            float hoRadius = dual ? GameRules.GuestDrawRadiusDual : GameRules.GuestDrawRadius;
            PlaceGuest(guest, level.guestMode == GuestMode.Gumiho || dual, gumihoAnchor, gumihoRadius);
            PlaceGuest(hoGuest, level.guestMode == GuestMode.Ho || dual, hoAnchor, hoRadius);
            PlaceGuest(meGuest, level.guestMode == GuestMode.Me, meDisplayAnchor, GameRules.GuestDrawRadius);
            PlaceGuest(coGuest, level.guestMode == GuestMode.Co, SingleAnchor(), GameRules.GuestDrawRadius);
            PlaceGuest(dokkaGuest, level.guestMode == GuestMode.Dokkaebi, SingleAnchor(), GameRules.GuestDrawRadius);
        }

        static void PlaceGuest(GuestView view, bool on, float anchorDeg, float radius)
        {
            if (view == null) return;
            if (view.gameObject.activeSelf != on) view.gameObject.SetActive(on);
            if (!on) return;
            Vector2 p = GameRules.ToWorld(GameRules.Point(anchorDeg, radius));
            var t = view.transform;
            t.position = new Vector3(p.x, p.y, t.position.z);
            view.Face(anchorDeg);
        }

        // Mọi AudioSource khác trong scene (ví dụ bài nhạc kéo vào) được coi là nhạc nền.
        void AttachMusic()
        {
            var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < sources.Length; i++)
            {
                var s = sources[i];
                if (s == null || s.gameObject == gameObject) continue;
                if (s.GetComponent<BackgroundMusic>() == null) s.gameObject.AddComponent<BackgroundMusic>();
            }
        }

        float lastPreview = -1f;

        // Kéo thanh "Hiệu ứng" thì kêu thử một tiếng cho người chơi nghe độ to.
        public void PreviewSfx()
        {
            if (sfx == null) return;
            if (Time.unscaledTime - lastPreview < 0.15f) return;
            lastPreview = Time.unscaledTime;
            sfx.Play(SfxKind.Eat, 0.7f);
        }

        void ShowMood(GuestId who, bool happy)
        {
            var sprite = happy ? happySprite : sadSprite;
            if (sprite == null) return;
            if (mood == null)
            {
                var go = new GameObject("Mood");
                mood = go.AddComponent<SpriteRenderer>();
                mood.sortingOrder = 12;
            }
            mood.sprite = sprite;
            moodGuest = who;
            moodLeft = MoodSeconds;
            mood.gameObject.SetActive(true);
            TickMood();
        }

        void HideMood()
        {
            moodLeft = 0f;
            if (mood != null) mood.gameObject.SetActive(false);
        }

        void TickMood()
        {
            if (mood == null || !mood.gameObject.activeSelf) return;
            moodLeft -= Time.deltaTime;
            var view = ViewOf(moodGuest);
            if (moodLeft <= 0f || view == null || !view.gameObject.activeInHierarchy)
            {
                HideMood();
                return;
            }

            // Bám theo góc trên-phải khung hình của khách (theo màn hình, không xoay theo khách).
            Vector3 anchor;
            var body = view.GetComponentInChildren<SpriteRenderer>();
            if (body != null && body.sprite != null)
            {
                var b = body.bounds;
                anchor = b.center + new Vector3(b.extents.x * 0.85f, b.extents.y * 0.85f, 0f);
            }
            else anchor = view.transform.position + new Vector3(0.45f, 0.45f, 0f);

            float age = MoodSeconds - moodLeft;
            float pop = EaseOutBack(Mathf.Clamp01(age / MoodPopSeconds));
            float alpha = Mathf.Clamp01(moodLeft / MoodFadeSeconds);
            float rise = 0.12f * (1f - alpha);

            Vector3 size = mood.sprite.bounds.size;
            float fit = MoodSize / Mathf.Max(0.0001f, Mathf.Max(size.x, size.y));
            var t = mood.transform;
            t.position = new Vector3(anchor.x, anchor.y + rise, view.transform.position.z);
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one * (fit * pop);
            var c = Color.white;
            c.a = alpha;
            mood.color = c;
        }

        static float EaseOutBack(float u)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float x = u - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (happySprite == null) happySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/happy.png");
            if (sadSprite == null) sadSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/sad.png");
        }
#endif

        void ApplyTurnAway()
        {
            bool away = turnAwayLeft > 0f;
            if (guest != null && guest.gameObject.activeSelf) guest.SetTurnedAway(away && lastBiter == GuestId.Gumiho);
            if (hoGuest != null && hoGuest.gameObject.activeSelf) hoGuest.SetTurnedAway(away && lastBiter == GuestId.Ho);
            if (meGuest != null && meGuest.gameObject.activeSelf) meGuest.SetTurnedAway(away && lastBiter == GuestId.Me);
            if (coGuest != null && coGuest.gameObject.activeSelf) coGuest.SetTurnedAway(away && lastBiter == GuestId.Co);
            if (dokkaGuest != null && dokkaGuest.gameObject.activeSelf) dokkaGuest.SetTurnedAway(away && lastBiter == GuestId.Dokkaebi);
        }

        void PlaceZones()
        {
            if (level == null) return;
            bool dual = level.guestMode == GuestMode.GumihoHo;
            float gumihoHold = HoldOf(GuestId.Gumiho);
            float hoHold = HoldOf(GuestId.Ho);
            float meHold = HoldOf(GuestId.Me);

            if (level.guestMode == GuestMode.Gumiho || dual)
                ShowCircle(zone, gumihoAnchor, GameRules.GumihoRadius(gumihoHold, dual));
            else
                HideZone(zone);

            if (level.guestMode == GuestMode.Ho || dual)
                ShowHo(hoAnchor, GameRules.HoHalfWidth(hoHold), dual);
            else
                HideZone(hoZone);

            if (level.guestMode == GuestMode.Me)
                ShowMe(meDisplayAnchor + meSway, GameRules.MeLength(meHold));
            else
                HideZone(meZone);

            if (level.guestMode == GuestMode.Co)
                ShowCo(SingleAnchor(), HoldOf(GuestId.Co));
            else
                HideZone(coZone);

            if (level.guestMode == GuestMode.Dokkaebi)
                ShowDokka(SingleAnchor(), HoldOf(GuestId.Dokkaebi));
            else
                HideZone(dokkaZone);
        }

        float SingleAnchor()
        {
            return GameRules.SingleAnchor + (level != null ? level.anchorOffset : 0f);
        }

        float HoldOf(GuestId id)
        {
            if (shownHold <= 0f) return 0f;
            if (biteShowLeft <= 0f && !spinner.Holding) return 0f;
            return HoldingGuest() == id ? shownHold : 0f;
        }

        GuestId HoldingGuest()
        {
            if (biteShowLeft > 0f || spinner.Holding) return Biter(activeSide);
            return PrimaryGuest();
        }

        void ShowCircle(SkillZone target, float anchorDeg, float radiusNorm)
        {
            if (target == null) return;
            if (!target.gameObject.activeSelf) target.gameObject.SetActive(true);
            Vector2 center = GameRules.ToWorld(GameRules.Point(anchorDeg, GameRules.GumihoCenterDistance));
            target.PlaceCircle(center, radiusNorm);
        }

        void ShowHo(float anchorDeg, float halfWidth, bool dual)
        {
            if (hoZone == null) return;
            if (!hoZone.gameObject.activeSelf) hoZone.gameObject.SetActive(true);
            float minAlong = dual ? GameRules.HoMinAlongDual : GameRules.HoMinAlong;
            float length = 1f - minAlong;
            hoZone.PlaceBand(anchorDeg, minAlong + length * 0.5f, length, halfWidth * 2f);
        }

        void ShowMe(float anchorDeg, float length)
        {
            if (meZone == null) return;
            if (!meZone.gameObject.activeSelf) meZone.gameObject.SetActive(true);
            meZone.PlaceBand(anchorDeg, 1f - length * 0.5f, length, GameRules.MeHalfWidth * 2f);
            meZone.SetReady(GameRules.MeSlides(length, level.ResolvedCrossAt));
        }

        void ShowCo(float anchorDeg, float hold)
        {
            if (coZone == null) return;
            if (!coZone.gameObject.activeSelf) coZone.gameObject.SetActive(true);
            Vector2 center = GameRules.ToWorld(GameRules.Point(anchorDeg, GameRules.CoAlong(hold)));
            coZone.PlaceCircle(center, GameRules.CoRadius);
        }

        void ShowDokka(float anchorDeg, float hold)
        {
            if (dokkaZone == null) return;
            if (!dokkaZone.gameObject.activeSelf) dokkaZone.gameObject.SetActive(true);
            float angle = anchorDeg + DokkaDir() * GameRules.DokkaSweep(hold);
            Vector2 center = GameRules.ToWorld(GameRules.Point(angle, dokkaAlong));
            dokkaZone.PlaceCircle(center, GameRules.DokkaRadius);
        }

        // Neo ở đáy mâm (270°): góc tăng = sang phải màn hình. Nhấn nửa trái thì lướt trái.
        float DokkaDir()
        {
            return activeSide == ScreenSide.Left ? -1f : 1f;
        }

        static void HideZone(SkillZone target)
        {
            if (target != null && target.gameObject.activeSelf) target.gameObject.SetActive(false);
        }

        bool InZone(FoodItem food, GuestId biter, float hold)
        {
            Vector2 point = food.ScreenNorm();
            bool dual = level.guestMode == GuestMode.GumihoHo;
            if (biter == GuestId.Ho)
                return GameRules.InHoZone(point, GameRules.HoHalfWidth(hold), hoAnchor, dual);
            if (biter == GuestId.Me)
                return GameRules.InMeZone(point, GameRules.MeLength(hold), meDisplayAnchor + meSway);
            if (biter == GuestId.Co)
                return GameRules.InCoZone(point, hold, SingleAnchor());
            if (biter == GuestId.Dokkaebi)
                return GameRules.InDokkaebiZone(point, hold, SingleAnchor(), dokkaAlong, DokkaDir());
                return GameRules.InGumihoZone(point, GameRules.GumihoRadius(hold, dual), gumihoAnchor);
        }

        GuestId Biter(ScreenSide side)
        {
            if (level.guestMode == GuestMode.Ho) return GuestId.Ho;
            if (level.guestMode == GuestMode.Me) return GuestId.Me;
            if (level.guestMode == GuestMode.Co) return GuestId.Co;
            if (level.guestMode == GuestMode.Dokkaebi) return GuestId.Dokkaebi;
            if (level.guestMode == GuestMode.GumihoHo && side == ScreenSide.Right) return GuestId.Ho;
            return GuestId.Gumiho;
        }

        GuestId PrimaryGuest()
        {
            if (level.guestMode == GuestMode.GumihoHo) return GuestId.Gumiho;
            return Art.GuestOf(level.guestMode);
        }

        bool AnyWantLeft()
        {
            if (WantsLeft(PrimaryGuest())) return true;
            return level.guestMode == GuestMode.GumihoHo && WantsLeft(GuestId.Ho);
        }

        bool WantsLeft(GuestId id)
        {
            for (int i = 0; i < foods.Count; i++)
            {
                if (foods[i].WantedBy(id)) return true;
            }
            return false;
        }

        Sprite SpriteFor(string dishId)
        {
            if (dishes == null || string.IsNullOrEmpty(dishId)) return null;
            for (int i = 0; i < dishes.Length; i++)
            {
                if (dishes[i].dishId == dishId) return dishes[i].sprite;
            }
            return null;
        }

        // Khi đang giữ: món nào sẽ bị cắn nếu thả ngay thì có vòng xanh (thèm) hoặc đỏ (cấm).
        void UpdateAim()
        {
            bool aiming = level != null && phase == Phase.Play && !menuOpen && spinner.Holding && pressArmed;
            if (aiming && aimSprite == null) aimSprite = ClearRingSprite();
            GuestId biter = aiming ? Biter(activeSide) : GuestId.Gumiho;
            float hold = aiming ? spinner.HoldSeconds : 0f;
            for (int i = 0; i < foods.Count; i++)
            {
                var food = foods[i];
                if (food == null) continue;
                int kind = 0;
                if (aiming && !food.Eaten && InZone(food, biter, hold))
                    kind = food.ForbiddenTo(biter) ? -1 : (food.WantedBy(biter) ? 1 : 0);
                food.SetAim(kind, aimSprite);
            }
        }

        void PulseWants()
        {
            if (level == null || phase != Phase.Play) return;
            float amount = 0.06f * Mathf.Sin(Time.time * 4f);
            for (int i = 0; i < foods.Count; i++)
            {
                var food = foods[i];
                if (food.Eaten) continue;
                bool wanted = food.WantedBy(PrimaryGuest()) ||
                              (level.guestMode == GuestMode.GumihoHo && food.WantedBy(GuestId.Ho));
                food.SetPulse(wanted ? amount : 0f);
            }
        }

        bool HasReferences()
        {
            bool ok = true;
            if (levels == null || levels.Length == 0) ok = Missing("Levels");
            if (tray == null) ok = Missing("Tray");
            if (foodPrefab == null) ok = Missing("Food Prefab");
            if (zone == null) ok = Missing("Zone");
            if (guest == null) ok = Missing("Guest");
            if (hud == null) ok = Missing("Hud");
            return ok;
        }

        bool Supports(LevelConfig next)
        {
            if (next.guestMode == GuestMode.Ho || next.guestMode == GuestMode.GumihoHo)
            {
                if (hoZone == null) return Missing("Ho Zone");
                if (hoGuest == null) return Missing("Ho");
            }
            if (next.guestMode == GuestMode.Me)
            {
                if (meZone == null) return Missing("Me Zone");
                if (meGuest == null) return Missing("Me");
            }
            if (next.guestMode == GuestMode.Co)
            {
                if (coZone == null) return Missing("Co Zone");
                if (coGuest == null) return Missing("Co");
            }
            if (next.guestMode == GuestMode.Dokkaebi)
            {
                if (dokkaZone == null) return Missing("Dokkaebi Zone");
                if (dokkaGuest == null) return Missing("Dokkaebi");
            }
            return true;
        }

        bool Missing(string name)
        {
            Debug.LogError("LevelRunner chưa gán " + name + ". Xem docs/Huong-Dan-Hierarchy.md và docs/Huong-Dan-Man-5-12.md", this);
            return false;
        }

        void RefreshMeSway(bool fromUpdate)
        {
            if (level == null || level.guestMode != GuestMode.Me)
            {
                meSway = 0f;
                return;
            }
            if (menuOpen || tutorialHold || sliding || biteShowLeft > 0f || phase != Phase.Play) return;
            if (spinner.Holding) return;
            SampleMeSway(fromUpdate);
        }

        void SampleMeSway(bool advance)
        {
            if (level == null || level.guestMode != GuestMode.Me)
            {
                meSway = 0f;
                return;
            }
            if (advance) meClock += Time.deltaTime;
            meSway = GameRules.MeSway(meClock);
        }

        void RefreshDokkaAlong(bool fromUpdate)
        {
            if (level == null || level.guestMode != GuestMode.Dokkaebi)
            {
                dokkaAlong = 0f;
                return;
            }
            if (menuOpen || tutorialHold || biteShowLeft > 0f || phase != Phase.Play) return;
            if (spinner.Holding) return;
            SampleDokkaAlong(fromUpdate);
        }

        void SampleDokkaAlong(bool advance)
        {
            if (level == null || level.guestMode != GuestMode.Dokkaebi)
            {
                dokkaAlong = 0f;
                return;
            }
            if (advance) dokkaClock += Time.deltaTime;
            dokkaAlong = GameRules.DokkaAlong(dokkaClock);
        }

        void FitCamera(bool dual)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            if (!orthoRemembered)
            {
                baseOrtho = cam.orthographicSize;
                orthoRemembered = true;
            }
            if (!dual)
            {
                cam.orthographicSize = baseOrtho;
                return;
            }

            float outer = GameRules.GuestDrawRadiusDual * GameRules.TrayWorldRadius + 0.62f;
            float aspect = Mathf.Max(cam.aspect, 0.4f);
            cam.orthographicSize = Mathf.Max(baseOrtho, (outer + 0.2f) / aspect);
        }

        void EnsureExtraGuests()
        {
            GuestView body = meGuest != null ? meGuest : guest;
            if (coGuest == null) coGuest = CloneGuest(body, "Co", Art.Reed);
            if (dokkaGuest == null) dokkaGuest = CloneGuest(body, "Dokkaebi", Art.Flame);
            if (coZone == null) coZone = CloneZone(zone, "CoZone", Art.Reed);
            if (dokkaZone == null) dokkaZone = CloneZone(zone, "DokkaebiZone", Art.Flame);
        }

        static GuestView CloneGuest(GuestView source, string objectName, Color tint)
        {
            if (source == null) return null;
            var copy = Instantiate(source, source.transform.parent);
            copy.name = objectName;
            copy.gameObject.SetActive(false);
            var renderer = copy.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = tint;
            return copy;
        }

        static SkillZone CloneZone(SkillZone source, string objectName, Color tint)
        {
            if (source == null) return null;
            var copy = Instantiate(source, source.transform.parent);
            copy.name = objectName;
            copy.gameObject.SetActive(false);
            var renderer = copy.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                tint.a = 70f / 255f;
                renderer.color = tint;
            }
            return copy;
        }

        IEnumerator BlinkSwap()
        {
            sfx.Play(SfxKind.Swap);
            float t = 0f;
            bool on = true;
            while (t < GameRules.SwapBlinkSeconds)
            {
                t += Time.deltaTime;
                bool next = Mathf.Repeat(t, 0.14f) < 0.07f;
                if (next != on)
                {
                    on = next;
                    hud.SetIconsVisible(on);
                }
                yield return null;
            }

            swapped = true;
            var mode = level.guestMode;
            for (int i = 0; i < foods.Count; i++)
            {
                if (foods[i] != null) foods[i].ApplySwap(mode);
            }
            hud.SetIconsVisible(true);
            hud.ShowFoods(foods, mode);
            if (phase != Phase.Play) yield break;
            if (!AnyWantLeft()) Finish(Phase.Clear, "Qua màn", index + 1 < levels.Length ? "Màn sau" : "Từ đầu");
        }

        // Nhát hỏng: mâm khựng một nhịp ngắn.
        IEnumerator ShakeTray()
        {
            if (tray == null) yield break;
            Vector3 home = trayHome;
            float t = 0f;
            const float length = 0.22f;
            while (t < length && tray != null)
            {
                t += Time.deltaTime;
                float fade = 1f - Mathf.Clamp01(t / length);
                float dx = Mathf.Sin(t * 90f) * 0.07f * fade;
                tray.localPosition = home + new Vector3(dx, 0f, 0f);
                yield return null;
            }
            if (tray != null) tray.localPosition = home;
        }

        IEnumerator Pop(FoodItem food)
        {
            Vector3 scale = food.BaseScale;
            float t = 0f;
            while (t < 1f && food != null)
            {
                t += Time.deltaTime / 0.16f;
                food.transform.localScale = scale * (1f + 0.35f * Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI));
                yield return null;
            }
            if (food != null) food.Hide();
        }

        IEnumerator FlashClearRing()
        {
            if (tray == null) yield break;
            DestroyClearRing();
            clearRing = new GameObject("ClearRing");
            clearRing.transform.SetParent(tray.parent, false);
            clearRing.transform.localPosition = Vector3.zero;
            var renderer = clearRing.AddComponent<SpriteRenderer>();
            renderer.sprite = ClearRingSprite();
            renderer.sortingOrder = 7;
            float t = 0f;
            while (t < GameRules.ClearRingSeconds && clearRing != null)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / GameRules.ClearRingSeconds);
                float size = Mathf.Lerp(5.15f, 6.15f, u);
                clearRing.transform.localScale = new Vector3(size, size, 1f);
                renderer.color = new Color(0.965f, 0.95f, 0.88f, 0.9f * (1f - u));
                yield return null;
            }
            DestroyClearRing();
        }

        void DestroyClearRing()
        {
            if (clearRing == null) return;
            Destroy(clearRing);
            clearRing = null;
        }

        Sprite ClearRingSprite()
        {
            if (clearRingSprite != null) return clearRingSprite;
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float c = (n - 1) * 0.5f;
            float outer = c;
            float inner = c * 0.82f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    float a = 0f;
                    if (d <= outer && d >= inner) a = Mathf.Clamp01(Mathf.Min(d - inner, outer - d));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            clearRingSprite = Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), n);
            return clearRingSprite;
        }

        IEnumerator Blink(FoodItem food)
        {
            Color color = food.Tint;
            for (int i = 0; i < 3 && food != null; i++)
            {
                food.SetTint(Color.white);
                yield return new WaitForSeconds(0.06f);
                if (food == null) yield break;
                food.SetTint(color);
                yield return new WaitForSeconds(0.06f);
            }
        }
    }
}
