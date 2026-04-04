using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace RareNaturallySpawningFruitTrees.Patches
{
    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction))]
    internal static class ObjectPlacementActionPatch
    {
        public const string PlayerPlantedKey = "vapor64.RareNaturallySpawningFruitTrees/PlayerPlanted";

        [HarmonyPostfix]
        private static void Postfix(
            StardewValley.Object __instance,
            bool __result,
            GameLocation location,
            int x,
            int y)
        {
            if (!__result || !__instance.IsWildTreeSapling())
                return;

            Vector2 tile = new Vector2(x / 64, y / 64);
            
            if (location.terrainFeatures.TryGetValue(tile, out var feature) && feature is Tree tree)
            {
                tree.modData[PlayerPlantedKey] = "true";
            }
        }
    }
}
