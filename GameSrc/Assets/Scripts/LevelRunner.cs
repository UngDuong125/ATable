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

        TableSpinner spinner;
        BiteInput input;
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

        void Awake()
        {
            spinner = GetComponent<TableSpinner>();
            input = GetComponent<BiteInput>();
            if (spinner.Tray == null) spinner.Tray = tray;
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
            PlaceZones();
            PulseWants();
            if (hud == null || level == null) return;
            hud.SetHold(GameRules.HoldU(shownHold), Art.Accent(HoldingGuest()));
            hud.SetStats(level.id, hearts, charges);
            FitCamera(level.guestMode == GuestMode.GumihoHo);
        }

        public void OpenFrontMenu()
        {
            if (front == null) return;
            menuOpen = true;
            pressArmed = false;
            if (spinner.Holding) spinner.EndHold();
            shownHold = 0f;
            biteShowLeft = 0f;
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
        }

        void OnPress(ScreenSide side)
        {
            if (menuOpen || level == null || phase != Phase.Play || spinner.Locked) { pressArmed = false; return; }
            pressArmed = true;
            activeSide = side;
            spinner.BeginHold();
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
            shownHold = hold;
            biteShowLeft = GameRules.FeedbackLockSeconds;

            bool anyForbid = false;
            bool anyWant = false;
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

            switch (GameRules.Judge(anyForbid, anyWant))
            {
                case BiteOutcome.Foul:
                    hearts--;
                    charges--;
                    turnAwayLeft = GameRules.FeedbackLockSeconds;
                    for (int i = 0; i < hits.Count; i++) StartCoroutine(Blink(hits[i]));
                    break;
                case BiteOutcome.Eat:
                    charges--;
                    for (int i = 0; i < hits.Count; i++)
                    {
                        if (!hits[i].WantedBy(biter)) continue;
                        hits[i].MarkEaten();
                        StartCoroutine(Pop(hits[i]));
                    }
                    break;
                default:
                    charges--;
                    break;
            }

            bites++;
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

            StopAllCoroutines();
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
            activeSide = ScreenSide.Left;
            lastBiter = PrimaryGuest();
            sliding = false;
            input.UseSplit(level.guestMode == GuestMode.GumihoHo);
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
            PlaceGuest(coGuest, level.guestMode == GuestMode.Co, GameRules.SingleAnchor, GameRules.GuestDrawRadius);
            PlaceGuest(dokkaGuest, level.guestMode == GuestMode.Dokkaebi, GameRules.SingleAnchor, GameRules.GuestDrawRadius);
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
                ShowCo(GameRules.SingleAnchor, HoldOf(GuestId.Co));
            else
                HideZone(coZone);

            if (level.guestMode == GuestMode.Dokkaebi)
                ShowDokka(GameRules.SingleAnchor, HoldOf(GuestId.Dokkaebi));
            else
                HideZone(dokkaZone);
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
            float angle = anchorDeg + GameRules.DokkaSweep(hold);
            Vector2 center = GameRules.ToWorld(GameRules.Point(angle, dokkaAlong));
            dokkaZone.PlaceCircle(center, GameRules.DokkaRadius);
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
                return GameRules.InCoZone(point, hold, GameRules.SingleAnchor);
            if (biter == GuestId.Dokkaebi)
                return GameRules.InDokkaebiZone(point, hold, GameRules.SingleAnchor, dokkaAlong);
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
            if (menuOpen || sliding || biteShowLeft > 0f || phase != Phase.Play) return;
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
            if (menuOpen || biteShowLeft > 0f || phase != Phase.Play) return;
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
