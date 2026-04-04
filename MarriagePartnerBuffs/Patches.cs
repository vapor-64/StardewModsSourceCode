using HarmonyLib;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace MarriagePartnerBuffs.Patches
{
    // ═══════════════════════════════════════════════════════════════════════════
    // HARVEY — No death penalty (items / gold loss on KO)
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class HarveyPatches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.LoseItemsOnDeath)),
                prefix: new HarmonyMethod(typeof(HarveyPatches), nameof(Prefix_LoseItemsOnDeath))
            );
        }

        private static bool Prefix_LoseItemsOnDeath()
        {
            if (ModEntry.CurrentSpouse != "Harvey") return true;

            ModEntry.Instance.Monitor.Log("Harvey buff: death penalty suppressed.", StardewModdingAPI.LogLevel.Debug);
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ELLIOTT — +20% extra treasure chest chance when fishing
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class ElliotPatches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Constructor(typeof(BobberBar),
                    new[] {
                        typeof(string),       // whichFish
                        typeof(float),        // fishSize
                        typeof(bool),         // treasure
                        typeof(List<string>), // bobbers
                        typeof(string),       // setFlagOnCatch
                        typeof(bool),         // isBossFish
                        typeof(string),       // baitID
                        typeof(bool)          // goldenTreasure
                    }),
                postfix: new HarmonyMethod(typeof(ElliotPatches), nameof(Postfix_BobberBarCtor))
            );
        }

        private static void Postfix_BobberBarCtor(BobberBar __instance)
        {
            if (ModEntry.CurrentSpouse != "Elliott") return;
            if (__instance.treasure) return;

            if (Game1.random.NextDouble() < 0.20)
            {
                __instance.treasure = true;
                ModEntry.Instance.Monitor.Log("Elliott buff: treasure chest granted.", StardewModdingAPI.LogLevel.Trace);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SHANE — Egg and egg-product selling price +100%
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class ShanePatches
    {
        private static readonly HashSet<string> EggItemNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Egg", "Large Egg", "Brown Egg", "Large Brown Egg",
            "Void Egg", "Golden Egg",
            "Mayonnaise", "Void Mayonnaise",
            "Gold Quality Mayonnaise"
        };

        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.sellToStorePrice)),
                postfix: new HarmonyMethod(typeof(ShanePatches), nameof(Postfix_SellToStorePrice))
            );
        }

        private static void Postfix_SellToStorePrice(SObject __instance, ref int __result)
        {
            if (ModEntry.CurrentSpouse != "Shane") return;
            if (!IsEggProduct(__instance)) return;

            __result = (int)(__result * 2.0f);
        }

        private static bool IsEggProduct(SObject obj)
        {
            if (obj == null) return false;
            if (EggItemNames.Contains(obj.Name)) return true;
            if (obj.HasContextTag("egg_item")) return true;
            if (obj.HasContextTag("mayo_item")) return true;
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EMILY — Gems worth 50% more
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class EmilyPatches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.sellToStorePrice)),
                postfix: new HarmonyMethod(typeof(EmilyPatches), nameof(Postfix_SellToStorePrice))
            );
        }

        private static void Postfix_SellToStorePrice(SObject __instance, ref int __result)
        {
            if (ModEntry.CurrentSpouse != "Emily") return;
            if (!IsGem(__instance)) return;

            __result = (int)(__result * 1.5f);
        }

        private static bool IsGem(SObject obj)
        {
            if (obj == null) return false;
            return obj.Category == SObject.GemCategory
                || obj.HasContextTag("item_category_gem")
                || obj.HasContextTag("geode_item");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HALEY — +5% silver quality chance, +5% gold quality chance
    //
    // Crop.harvest delivers items via three paths depending on harvest method:
    //   1. Farmer.addItemToInventoryBool  — hand harvest (param: "item")
    //   2. Game1.createItemDebris         — scythe, iridium scythe, tractor mods (param: "item")
    //   3. JunimoHarvester.tryToAddItemToHut — junimo huts (param: "i")
    //
    // Harmony prefix parameter names MUST exactly match the original method's
    // parameter names. Each delivery path gets its own prefix method so the
    // parameter name is always correct.
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class HaleyPatches
    {
        private static bool _harvestInProgress = false;

        internal static void Apply(Harmony harmony)
        {
            // Flag gate — wraps the entire harvest call
            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
                prefix: new HarmonyMethod(typeof(HaleyPatches), nameof(Prefix_Harvest)),
                postfix: new HarmonyMethod(typeof(HaleyPatches), nameof(Postfix_Harvest))
            );

            // Path 1: hand harvest — original param name is "item"
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
                prefix: new HarmonyMethod(typeof(HaleyPatches), nameof(Prefix_AddItemToInventoryBool))
            );

            // Path 2: scythe / iridium scythe / tractor mods — original param name is "item"
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.createItemDebris)),
                prefix: new HarmonyMethod(typeof(HaleyPatches), nameof(Prefix_CreateItemDebris))
            );

            // Path 3: junimo huts — original param name is "i" (NOT "item")
            harmony.Patch(
                original: AccessTools.Method(typeof(JunimoHarvester), nameof(JunimoHarvester.tryToAddItemToHut)),
                prefix: new HarmonyMethod(typeof(HaleyPatches), nameof(Prefix_TryToAddItemToHut))
            );
        }

        private static void Prefix_Harvest()  => _harvestInProgress = ModEntry.CurrentSpouse == "Haley";
        private static void Postfix_Harvest() => _harvestInProgress = false;

        // Shared quality roll — called by all three delivery prefixes.
        private static void TryBumpQuality(Item item)
        {
            if (!_harvestInProgress || item is not SObject obj) return;

            // Crops only: vegetable (-75), fruit (-79), flower (-80)
            if (obj.Category is not ((-75) or (-79) or (-80))) return;

            int before = obj.Quality;

            // Roll 1: normal → silver (10%)
            if (obj.Quality == SObject.lowQuality && Game1.random.NextDouble() < 0.05)
                obj.Quality = SObject.medQuality;

            // Roll 2: silver → gold (5%), including crops just promoted by Roll 1
            if (obj.Quality == SObject.medQuality && Game1.random.NextDouble() < 0.05)
                obj.Quality = SObject.highQuality;

            if (obj.Quality != before)
                ModEntry.Instance.Monitor.Log(
                    $"Haley buff: crop quality {before} → {obj.Quality}.",
                    StardewModdingAPI.LogLevel.Trace);
        }

        // Path 1: Farmer.addItemToInventoryBool(Item item, ...)
        private static void Prefix_AddItemToInventoryBool(Item item) => TryBumpQuality(item);

        // Path 2: Game1.createItemDebris(Item item, ...)
        private static void Prefix_CreateItemDebris(Item item) => TryBumpQuality(item);

        // Path 3: JunimoHarvester.tryToAddItemToHut(Item i)  ← param is "i" not "item"
        private static void Prefix_TryToAddItemToHut(Item i) => TryBumpQuality(i);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LEAH — +3 extra wood from trees
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class LeahPatches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
                postfix: new HarmonyMethod(typeof(LeahPatches), nameof(Postfix_PerformToolAction))
            );
        }

        private static void Postfix_PerformToolAction(Tool t, ref bool __result)
        {
            if (ModEntry.CurrentSpouse != "Leah") return;
            if (t is not Axe) return;
            if (!__result) return;

            Game1.player.addItemToInventory(ItemRegistry.Create("(O)388", 3));
            ModEntry.Instance.Monitor.Log("Leah buff: +3 wood.", StardewModdingAPI.LogLevel.Trace);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // MARU — Farm machines take 5% less time
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class MaruPatches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), "PlaceInMachine"),
                postfix: new HarmonyMethod(typeof(MaruPatches), nameof(Postfix_PlaceInMachine))
            );
        }

        private static void Postfix_PlaceInMachine(SObject __instance, bool probe, bool __result)
        {
            if (ModEntry.CurrentSpouse != "Maru") return;
            if (!__result || probe) return;

            int current = __instance.MinutesUntilReady;
            if (current <= 0) return;

            int reduced = (int)Math.Round(current * 0.95 / 10.0) * 10;
            reduced = Math.Max(10, reduced);
            __instance.MinutesUntilReady = reduced;

            ModEntry.Instance.Monitor.Log($"Maru buff: machine time {current} → {reduced} min.", StardewModdingAPI.LogLevel.Trace);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PENNY — +40% XP gain across all skills
    // ═══════════════════════════════════════════════════════════════════════════
    internal static class PennyPatches
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                prefix: new HarmonyMethod(typeof(PennyPatches), nameof(Prefix_GainExperience))
            );
        }

        private static void Prefix_GainExperience(ref int howMuch)
        {
            if (ModEntry.CurrentSpouse != "Penny") return;
            howMuch = (int)(howMuch * 1.40f);
        }
    }
}
