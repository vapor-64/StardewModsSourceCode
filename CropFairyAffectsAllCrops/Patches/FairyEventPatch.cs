using HarmonyLib;
using StardewValley;
using StardewValley.Events;
using StardewValley.TerrainFeatures;

namespace CropFairyAffectsAllCrops.Patches;

[HarmonyPatch(typeof(FairyEvent), nameof(FairyEvent.makeChangesToLocation))]
internal class FairyEventPatch
{
    public static bool Prefix()
    {
        if (!Game1.IsMasterGame)
            return false;

        Farm farm = Game1.getFarm();

        foreach (var pair in farm.terrainFeatures.Pairs)
        {
            if (pair.Value is HoeDirt { crop: not null } dirt)
                dirt.crop.growCompletely();
        }

        return false;
    }
}