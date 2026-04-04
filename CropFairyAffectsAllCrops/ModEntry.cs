using HarmonyLib;
using StardewModdingAPI;

namespace CropFairyAffectsAllCrops;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.PatchAll();
        this.Monitor.Log("Crop Fairy Affects All Crops loaded.", LogLevel.Info);
    }
}