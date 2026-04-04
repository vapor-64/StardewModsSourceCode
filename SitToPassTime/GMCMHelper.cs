using System;
using StardewModdingAPI;

namespace SitToPassTime
{
    internal static class GMCMHelper
    {
        public static void Register(IModHelper helper, IManifest manifest, ModConfig config)
        {
            var gmcm = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                "spacechase0.GenericModConfigMenu");

            if (gmcm is null) return;

            gmcm.Register(
                mod:   manifest,
                reset: () => config.DisablePopup = new ModConfig().DisablePopup,
                save:  () => helper.WriteConfig(config)
            );

            gmcm.AddBoolOption(
                mod:      manifest,
                getValue: () => config.DisablePopup,
                setValue: v  => config.DisablePopup = v,
                name:     () => "Disable time-skip popup",
                tooltip:  () => "When enabled, sitting on furniture will not open the time-skip menu."
            );
        }
    }

    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddBoolOption(
            IManifest mod,
            Func<bool> getValue,
            Action<bool> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string? fieldId = null);
    }
}
