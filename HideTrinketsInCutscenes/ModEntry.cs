using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Objects.Trinkets;
using Microsoft.Xna.Framework;

namespace HideTrinketsInCutscenes;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony("vapor64.HideTrinketsInCutscenes");

        var drawPrefix = new HarmonyMethod(typeof(ModEntry), nameof(Companion_Draw_Prefix));
        var updatePrefix = new HarmonyMethod(typeof(ModEntry), nameof(Companion_Update_Prefix));
        
        harmony.Patch(
            original: AccessTools.Method(typeof(FlyingCompanion), nameof(FlyingCompanion.Draw)),
            prefix: drawPrefix
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(FlyingCompanion), nameof(FlyingCompanion.Update)),
            prefix: updatePrefix
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(HungryFrogCompanion), nameof(HungryFrogCompanion.Draw)),
            prefix: drawPrefix
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(HungryFrogCompanion), nameof(HungryFrogCompanion.Update)),
            prefix: updatePrefix
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(HoppingCompanion), nameof(HoppingCompanion.Draw)),
            prefix: drawPrefix
        );
        
        harmony.Patch(
            original: AccessTools.Method(typeof(Trinket), nameof(Trinket.Update)),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Trinket_Update_Prefix))
        );

        Monitor.Log("Trinket cutscene suppression patches applied.", LogLevel.Debug);
    }

    private static bool IsCutsceneActive()
    {
        return Game1.eventUp
            && Game1.CurrentEvent != null
            && !Game1.CurrentEvent.isFestival
            && !Game1.CurrentEvent.playerControlSequence;
    }
    
    private static bool Companion_Draw_Prefix()
    {
        return !IsCutsceneActive();
    }

    private static bool Companion_Update_Prefix()
    {
        return !IsCutsceneActive();
    }
    
    private static bool Trinket_Update_Prefix()
    {
        return !IsCutsceneActive();
    }
}
