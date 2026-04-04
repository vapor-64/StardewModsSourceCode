using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace AnimalsFertilizeCrops
{
    /// <summary>
    /// The API surface for Generic Mod Config Menu (GMCM).
    /// Only the methods actually used by this mod are declared.
    /// GMCM is a hard/required dependency — the mod will not load without it.
    /// </summary>
    public interface IGenericModConfigMenuApi
    {
        /// <summary>Register a mod whose config can be edited through the UI.</summary>
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        /// <summary>Add a section heading.</summary>
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);

        /// <summary>Add a boolean checkbox option.</summary>
        void AddBoolOption(
            IManifest mod,
            Func<bool> getValue,
            Action<bool> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            string? fieldId = null);

        /// <summary>Add a float slider option.</summary>
        void AddNumberOption(
            IManifest mod,
            Func<float> getValue,
            Action<float> setValue,
            Func<string> name,
            Func<string>? tooltip = null,
            float? min = null,
            float? max = null,
            float? interval = null,
            Func<float, string>? formatValue = null,
            string? fieldId = null);
    }
}
