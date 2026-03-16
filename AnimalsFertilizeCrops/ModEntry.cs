// ModEntry.cs
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace AnimalsFertilizeCrops
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;
        private const int FertilizeRadius = 1;
        private const float FriendshipBonusPerHundred = 0.0025f;
        private const string BasicFertilizer   = "(O)368";
        private const string QualityFertilizer = "(O)369";
        private const string DeluxeFertilizer  = "(O)919";

        private static readonly List<string> FertilizerTiers = new()
        {
            BasicFertilizer,
            QualityFertilizer,
            DeluxeFertilizer,
        };

        private static readonly (string Suffix, string FertilizerId)[] SuffixToFertilizer =
        {
            ("Chicken",  BasicFertilizer),
            ("Duck",     BasicFertilizer),
            ("Cow",      QualityFertilizer),
            ("Goat",     QualityFertilizer),
            ("Sheep",    DeluxeFertilizer),
            ("Pig",      DeluxeFertilizer),
            ("Rabbit",   DeluxeFertilizer),
            ("Ostrich",  DeluxeFertilizer),
            ("Dinosaur", DeluxeFertilizer),
        };

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            RegisterWithGmcm();
        }

        private void RegisterWithGmcm()
        {
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                "spacechase0.GenericModConfigMenu");

            if (gmcm is null)
            {
                Monitor.Log("Generic Mod Config Menu not found — in-game config UI unavailable.", LogLevel.Debug);
                return;
            }

            gmcm.Register(
                mod:   ModManifest,
                reset: () => Config = new ModConfig(),
                save:  () => Helper.WriteConfig(Config)
            );

            gmcm.AddSectionTitle(
                mod:  ModManifest,
                text: () => Helper.Translation.Get("config.settings.title")
            );

            gmcm.AddBoolOption(
                mod:      ModManifest,
                getValue: () => Config.Enabled,
                setValue: v  => Config.Enabled = v,
                name:     () => Helper.Translation.Get("config.settings.enabled-key"),
                tooltip:  () => Helper.Translation.Get("config.settings.enabled-tooltip")
            );

            gmcm.AddNumberOption(
                mod:         ModManifest,
                getValue:    () => Config.FertilizeChance * 100f,
                setValue:    v  => Config.FertilizeChance = v / 100f,
                name:        () => Helper.Translation.Get("config.settings.chance-key"),
                tooltip:     () => Helper.Translation.Get("config.settings.chance-tooltip"),
                min:         0.1f,
                max:         100f,
                interval:    0.1f,
                formatValue: v => $"{v:F1}%"
            );

            Monitor.Log("Registered with Generic Mod Config Menu.", LogLevel.Info);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Config.Enabled) return;
            if (!e.IsOneSecond || !Context.IsWorldReady) return;

            var farm = Game1.getFarm();

            foreach (var animal in farm.animals.Values)
            {
                string? fertilizerId = ResolveFertilizer(animal.type.Value);
                if (fertilizerId is null) continue;

                float effectiveChance = EffectiveChance(animal);
                if (Game1.random.NextDouble() >= effectiveChance) continue;

                TryFertilizeTile(farm, animal.Tile, fertilizerId);
            }
        }

        private float EffectiveChance(FarmAnimal animal)
        {
            int friendship = animal.friendshipTowardFarmer.Value;
            float friendshipBonus = (friendship / 100f) * FriendshipBonusPerHundred;
            return Config.FertilizeChance + friendshipBonus;
        }

        private static string? ResolveFertilizer(string animalType)
        {
            foreach (var (suffix, id) in SuffixToFertilizer)
                if (animalType.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return id;
            return null;
        }

        private void TryFertilizeTile(GameLocation location, Vector2 tile, string fertilizerId)
        {
            if (!location.terrainFeatures.TryGetValue(tile, out var feature)) return;
            if (feature is not HoeDirt dirt) return;

            string existing = dirt.fertilizer.Value ?? string.Empty;
            if (existing != string.Empty && !IsUpgrade(existing, fertilizerId)) return;

            dirt.fertilizer.Value = fertilizerId;
            Monitor.Log($"Applied {fertilizerId} at {tile} (hasCrop={dirt.crop is not null})", LogLevel.Trace);

            PlayFertilizeSparkles(location, tile, fertilizerId);
        }

        private void PlayFertilizeSparkles(GameLocation location, Vector2 tile, string fertilizerId)
        {
            Color sparkleColor = fertilizerId switch
            {
                BasicFertilizer   => new Color(180, 255, 120), // light green
                QualityFertilizer => new Color(120, 200, 255), // light blue
                DeluxeFertilizer  => new Color(255, 180, 255), // light purple
                _                 => Color.White
            };

            var r = Game1.random;
            const int numSprinkles = 6;
            const int msBetween    = 80;

            for (int i = 0; i < numSprinkles; i++)
            {
                // Pick a random sub-tile position within the fertilized tile.
                // Subtract 64 on Y to compensate for the sprite anchor being
                // one tile below the position coordinate for these sprite indices.
                Vector2 sprinklePos = (tile * 64f) + new Vector2(
                    (float)(r.NextDouble() * 64),
                    (float)(r.NextDouble() * 64) - 64f);

                location.temporarySprites.Add(new TemporaryAnimatedSprite(
                    r.Next(10, 12),
                    sprinklePos,
                    sparkleColor,
                    8,
                    flipped: false,
                    50f)
                {
                    layerDepth              = 1f,
                    delayBeforeAnimationStart = msBetween * i,
                    interval                = 100f,
                    motion                  = new Vector2(
                        (float)(r.NextDouble() - 0.5) * 0.5f,
                        -0.75f),
                    alphaFade               = 0.02f,
                });
            }
        }

        private static bool IsUpgrade(string existing, string incoming)
        {
            int existingTier = FertilizerTiers.IndexOf(existing);
            int incomingTier = FertilizerTiers.IndexOf(incoming);
            if (existingTier == -1 || incomingTier == -1) return false;
            return incomingTier > existingTier;
        }
    }

    [HarmonyPatch(typeof(PathFindController))]
    [HarmonyPatch(MethodType.Constructor,
        typeof(Character),
        typeof(GameLocation),
        typeof(PathFindController.isAtEnd),
        typeof(int),
        typeof(PathFindController.endBehavior),
        typeof(int),
        typeof(Point),
        typeof(bool))]
    internal static class PathFindController_Ctor_Patch
    {
        static void Prefix(Character c, ref int limit)
        {
            if (c is FarmAnimal)
                limit *= 2;
        }
    }
}
