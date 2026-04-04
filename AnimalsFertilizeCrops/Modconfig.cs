namespace AnimalsFertilizeCrops
{
    /// <summary>Mod settings saved to config.json and exposed in Generic Mod Config Menu.</summary>
    public sealed class ModConfig
    {
        /// <summary>
        /// When false, all mod effects are completely disabled.
        /// Default: true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Probability (0.0–1.0) that an animal fertilizes its tile each second.
        /// Default: 0.025 (2.5%).
        /// </summary>
        public float FertilizeChance { get; set; } = 0.025f;
    }
}
