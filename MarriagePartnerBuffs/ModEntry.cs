using HarmonyLib;
using MarriagePartnerBuffs.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;
using System;

namespace MarriagePartnerBuffs
{
    public class ModEntry : Mod
    {
        // ─── Singleton ───────────────────────────────────────────────────────────
        internal static ModEntry Instance { get; private set; } = null!;

        // ─── Shared state (read by Patches) ──────────────────────────────────────
        /// <summary>NPC name of the current spouse, or null if unmarried.</summary>
        internal static string? CurrentSpouse { get; private set; }

        /// <summary>True between 10 PM and midnight when Sebastian is spouse.</summary>
        internal static bool SebastianNightActive { get; private set; }

        /// <summary>True while the player is inside Pelican Town and Sam is spouse.</summary>
        internal static bool SamTownActive { get; private set; }

        // ─── Custom icon ─────────────────────────────────────────────────────────
        private static Texture2D? _buffIcon;

        // ─── Buff IDs ────────────────────────────────────────────────────────────
        internal const string BUFF_ALEX      = "vapor64.MPB.Alex";
        internal const string BUFF_ELLIOT    = "vapor64.MPB.Elliot";
        internal const string BUFF_HARVEY    = "vapor64.MPB.Harvey";
        internal const string BUFF_SAM_TOWN  = "vapor64.MPB.Sam.Town";
        internal const string BUFF_SEB_NIGHT = "vapor64.MPB.Sebastian.Night";
        internal const string BUFF_SHANE     = "vapor64.MPB.Shane";
        internal const string BUFF_ABIGAIL   = "vapor64.MPB.Abigail";
        internal const string BUFF_EMILY     = "vapor64.MPB.Emily";
        internal const string BUFF_HALEY     = "vapor64.MPB.Haley";
        internal const string BUFF_LEAH      = "vapor64.MPB.Leah";
        internal const string BUFF_MARU      = "vapor64.MPB.Maru";
        internal const string BUFF_PENNY     = "vapor64.MPB.Penny";

        // ─── Entry ───────────────────────────────────────────────────────────────
        public override void Entry(IModHelper helper)
        {
            Instance = this;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded   += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted   += OnDayStarted;
            helper.Events.GameLoop.TimeChanged  += OnTimeChanged;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped         += OnWarped;

            var harmony = new Harmony("vapor64.MarriagePartnerBuffs");
            HarveyPatches.Apply(harmony);
            ElliotPatches.Apply(harmony);
            ShanePatches.Apply(harmony);
            EmilyPatches.Apply(harmony);
            HaleyPatches.Apply(harmony);
            LeahPatches.Apply(harmony);
            MaruPatches.Apply(harmony);
            PennyPatches.Apply(harmony);

            Monitor.Log("Marriage Partner Buffs loaded.", LogLevel.Info);
        }

        // ─── Event handlers ──────────────────────────────────────────────────────

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            _buffIcon = Helper.ModContent.Load<Texture2D>("assets/buffIcon.png");
            Monitor.Log("Buff icon loaded.", LogLevel.Debug);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            RefreshSpouse();
            ApplyDailyBuffs();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            RefreshSpouse();
            ApplyDailyBuffs();
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady || CurrentSpouse != "Sebastian") return;

            bool after10pm = Game1.timeOfDay >= 2200;
            if (after10pm == SebastianNightActive) return;

            SebastianNightActive = after10pm;
            if (after10pm)
                ApplySebastianNightBuff();
            else
                Game1.player.buffs.Remove(BUFF_SEB_NIGHT);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !e.IsMultipleOf(30)) return;
            if (CurrentSpouse != "Sam") return;

            bool inTown = Game1.player.currentLocation?.Name == "Town";
            if (inTown == SamTownActive) return;

            SamTownActive = inTown;
            if (inTown)
                ApplySamTownBuff();
            else
                Game1.player.buffs.Remove(BUFF_SAM_TOWN);
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || CurrentSpouse != "Sam") return;

            bool inTown = e.NewLocation?.Name == "Town";
            if (inTown == SamTownActive) return;

            SamTownActive = inTown;
            if (inTown)
                ApplySamTownBuff();
            else
                Game1.player.buffs.Remove(BUFF_SAM_TOWN);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static void RefreshSpouse()
        {
            CurrentSpouse = null;
            SebastianNightActive = false;
            SamTownActive = false;
            _alexStaminaBonus = 0;

            var player = Game1.player;
            if (player == null) return;

            string spouseName = player.spouse ?? string.Empty;
            if (!string.IsNullOrEmpty(spouseName))
            {
                CurrentSpouse = spouseName;
                Instance.Monitor.Log($"Spouse detected: {CurrentSpouse}", LogLevel.Debug);
            }
        }

        private static void ApplyDailyBuffs()
        {
            if (CurrentSpouse == null) return;

            switch (CurrentSpouse)
            {
                case "Alex":     ApplyAlexBuff(Game1.player); break;
                case "Elliott":  ApplyElliotBuff();            break;
                case "Harvey":   ApplyHarveyBuff();            break;
                case "Shane":    ApplyShaneBuff();             break;
                case "Abigail":  ApplyAbigailBuff();           break;
                case "Emily":    ApplyEmilyBuff();             break;
                case "Haley":    ApplyHaleyBuff();             break;
                case "Leah":     ApplyLeahBuff();              break;
                case "Maru":     ApplyMaruBuff();              break;
                case "Penny":    ApplyPennyBuff();             break;
                // Sam and Sebastian are reactive — applied in time/location handlers.
            }
        }

        // ── Helper: build a Buff using the shared custom icon ────────────────────
        private static Buff MakeIndicatorBuff(
            string id,
            string description,
            BuffEffects? effects = null)
        {
            return new Buff(
                id: id,
                source: "MarriagePartnerBuffs",
                displayName: null,
                description: description,
                duration: Buff.ENDLESS,
                iconTexture: _buffIcon,
                iconSheetIndex: 0,
                effects: effects
            );
        }

        // ── Translation shorthand ─────────────────────────────────────────────────
        private static string T(string key) =>
            Instance.Helper.Translation.Get(key);

        private static string T(string key, object tokens) =>
            Instance.Helper.Translation.Get(key, tokens);


        private static int _alexStaminaBonus = 0;

        private static void ApplyAlexBuff(Farmer player)
        {
            if (_alexStaminaBonus > 0)
                player.maxStamina.Value -= _alexStaminaBonus;

            int bonus = (int)Math.Round(player.maxStamina.Value * 0.1f);
            player.maxStamina.Value += bonus;
            _alexStaminaBonus = bonus;

            player.Stamina = player.maxStamina.Value;

            player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_ALEX,
                description: T("buff.alex.description", new { bonus })
            ));

            Instance.Monitor.Log($"Alex buff: +{bonus} max stamina (now {player.maxStamina.Value}).", LogLevel.Debug);
        }


        private static void ApplyElliotBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_ELLIOT,
                description: T("buff.elliott.description")
            ));
        }

        private static void ApplyHarveyBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_HARVEY,
                description: T("buff.harvey.description")
            ));
        }


        private static void ApplyShaneBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_SHANE,
                description: T("buff.shane.description")
            ));
        }
        
        private static void ApplyAbigailBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_ABIGAIL,
                description: T("buff.abigail.description"),
                effects:     new BuffEffects { Attack = { Value = 1 } }
            ));
        }
        
        private static void ApplyEmilyBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_EMILY,
                description: T("buff.emily.description")
            ));
        }
        
        private static void ApplyHaleyBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_HALEY,
                description: T("buff.haley.description")
            ));
        }
        
        private static void ApplyLeahBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_LEAH,
                description: T("buff.leah.description")
            ));
        }
        
        private static void ApplyMaruBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_MARU,
                description: T("buff.maru.description")
            ));
        }
        
        private static void ApplyPennyBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_PENNY,
                description: T("buff.penny.description")
            ));
        }
        
        private static void ApplySamTownBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_SAM_TOWN,
                description: T("buff.sam.description"),
                effects:     new BuffEffects { Speed = { Value = 1 } }
            ));
        }
        
        private static void ApplySebastianNightBuff()
        {
            Game1.player.applyBuff(MakeIndicatorBuff(
                id:          BUFF_SEB_NIGHT,
                description: T("buff.sebastian.description"),
                effects:     new BuffEffects
                {
                    Speed     = { Value = 1 },
                    LuckLevel = { Value = 1 }
                }
            ));
        }
    }
}
