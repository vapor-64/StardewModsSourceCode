using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.TerrainFeatures;
using RareNaturallySpawningFruitTrees.Patches;

namespace RareNaturallySpawningFruitTrees
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;

        private static readonly HashSet<string> VanillaTreeIds = new()
        {
            "628", // Cherry      - Spring  (mainland)
            "629", // Apricot     - Spring  (mainland)
            "630", // Orange      - Summer  (mainland)
            "631", // Peach       - Summer  (mainland)
            "632", // Pomegranate - Fall    (mainland)
            "633", // Apple       - Fall    (mainland)
            "69",  // Banana      - Summer  (Island)
            "835", // Mango       - Summer  (Island)
        };


        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            new Harmony(this.ModManifest.UniqueID).PatchAll();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted   += this.OnDayStarted;

            this.Monitor.Log($"{this.ModManifest.Name} loaded.", LogLevel.Info);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            this.RegisterWithGmcm();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            bool isYearOneDayOne = Game1.year == 1 && Game1.dayOfMonth == 1 && Game1.season == Season.Spring;

            Season currentSeason = Game1.season;
            List<string> eligibleTreeIds = this.GetEligibleTreeIds(currentSeason);

            if (eligibleTreeIds.Count == 0)
                return;  // Winter - no vanilla trees have a Winter season.

            foreach (GameLocation location in this.GetTargetLocations())
            {
                bool isFarmOnDay1 = isYearOneDayOne && location.Name == "Farm";

                if (isFarmOnDay1)
                    this.SpawnByReplacingWildTrees(location, eligibleTreeIds);
                else
                    this.TrySpawnTreesInLocation(location, eligibleTreeIds);
            }
        }
        
        private void TrySpawnTreesInLocation(GameLocation location, List<string> eligibleTreeIds)
        {
            if (Game1.random.NextDouble() > this.Config.SpawnChancePerDay)
                return;

            List<Vector2> plantable = this.GetPlantableCandidateTiles(location);

            if (plantable.Count > 0)
            {
                for (int attempt = 0; attempt < this.Config.SpawnsPerSuccessfulRoll; attempt++)
                {
                    if (plantable.Count == 0)
                        break;

                    int idx = Game1.random.Next(plantable.Count);
                    Vector2 tile = plantable[idx];
                    plantable.RemoveAt(idx);

                    string treeId = eligibleTreeIds[Game1.random.Next(eligibleTreeIds.Count)];

                    if (this.TryPlaceTree(location, tile, treeId, forceFullyGrown: false))
                    {
                        this.Monitor.Log(
                            $"Fruit tree '{treeId}' naturally spawned at {location.Name} ({(int)tile.X},{(int)tile.Y}).",
                            LogLevel.Debug);
                    }
                }
            }
            else
            {
                this.SpawnByReplacingWildTrees(location, eligibleTreeIds);
            }
        }

        private List<Vector2> GetPlantableCandidateTiles(GameLocation location)
        {
            int mapWidth  = location.map?.Layers[0]?.LayerWidth  ?? 0;
            int mapHeight = location.map?.Layers[0]?.LayerHeight ?? 0;

            const string SaplingId = "628";

            var candidates = new List<Vector2>();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var tile = new Vector2(x, y);

                    if (location.terrainFeatures.ContainsKey(tile))
                        continue;
                    if (location.IsTileOccupiedBy(tile))
                        continue;
                    if (FruitTree.IsTooCloseToAnotherTree(tile, location))
                        continue;
                    if (FruitTree.IsGrowthBlocked(tile, location))
                        continue;

                    bool canDig       = location.doesTileHaveProperty(x, y, "Diggable", "Back") != null;
                    string? tileType  = location.doesTileHaveProperty(x, y, "Type", "Back");
                    bool canPlantHere = location.doesEitherTileOrTileIndexPropertyEqual(x, y, "CanPlantTrees", "Back", "T");

                    bool tileIsValid;
                    if (location is Farm)
                    {
                        tileIsValid = (canDig || tileType == "Grass" || tileType == "Dirt" || canPlantHere)
                                   && (!location.IsNoSpawnTile(tile, "Tree") || canPlantHere);
                    }
                    else
                    {
                        tileIsValid = (canDig || tileType == "Stone")
                                   && location.CanPlantTreesHere(SaplingId, x, y, out _);
                    }

                    if (tileIsValid)
                        candidates.Add(tile);
                }
            }

            return candidates;
        }
        
        private void SpawnByReplacingWildTrees(GameLocation location, List<string> eligibleTreeIds)
        {
            var candidates = new List<Vector2>();
            foreach (var (tile, feature) in location.terrainFeatures.Pairs)
            {
                if (feature is Tree wildTree
                    && wildTree.growthStage.Value >= 5
                    && !wildTree.stump.Value
                    && !wildTree.modData.ContainsKey(ObjectPlacementActionPatch.PlayerPlantedKey))
                {
                    candidates.Add(tile);
                }
            }

            if (candidates.Count == 0)
                return;

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = Game1.random.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int spawned = 0;
            foreach (Vector2 tile in candidates)
            {
                if (spawned >= this.Config.SpawnsPerSuccessfulRoll)
                    break;

                string treeId = eligibleTreeIds[Game1.random.Next(eligibleTreeIds.Count)];

                location.terrainFeatures.Remove(tile);

                var fruitTree = new FruitTree(treeId, FruitTree.treeStage);
                location.terrainFeatures.Add(tile, fruitTree);

                for (int i = 0; i < FruitTree.maxFruitsOnTrees; i++)
                    fruitTree.TryAddFruit();

                spawned++;
                this.Monitor.Log(
                    $"Fruit tree '{treeId}' naturally spawned at {location.Name} ({(int)tile.X},{(int)tile.Y}) (replaced wild tree).",
                    LogLevel.Debug);
            }
        }

        private List<string> GetEligibleTreeIds(Season season)
        {
            var eligible = new List<string>();

            foreach (var (id, data) in Game1.fruitTreeData)
            {
                if (!this.Config.IncludeModdedTrees && !VanillaTreeIds.Contains(id))
                    continue;

                if (data.Seasons == null || data.Seasons.Count == 0)
                    continue;

                if (data.Seasons.Contains(season))
                    eligible.Add(id);
            }

            return eligible;
        }

        private IEnumerable<GameLocation> GetTargetLocations()
        {
            var enabled = new HashSet<string>(
                this.Config.GetEnabledLocations(),
                StringComparer.OrdinalIgnoreCase);

            if (enabled.Count == 0)
                return Game1.locations.Where(loc => loc.IsOutdoors && loc.terrainFeatures != null);

            return Game1.locations
                .Where(loc => loc.terrainFeatures != null && enabled.Contains(loc.Name));
        }

        private bool TryPlaceTree(GameLocation location, Vector2 tile, string treeId, bool forceFullyGrown)
        {
            if (location.terrainFeatures.ContainsKey(tile))
                return false;
            if (location.IsTileOccupiedBy(tile))
                return false;
            if (FruitTree.IsTooCloseToAnotherTree(tile, location))
                return false;
            if (FruitTree.IsGrowthBlocked(tile, location))
                return false;

            int startStage = forceFullyGrown
                ? FruitTree.treeStage
                : (this.Config.RandomizeStartingGrowthStage ? Game1.random.Next(0, 4) : 0);

            var tree = new FruitTree(treeId, startStage);
            location.terrainFeatures.Add(tile, tree);

            if (forceFullyGrown)
            {
                for (int i = 0; i < FruitTree.maxFruitsOnTrees; i++)
                    tree.TryAddFruit();
            }

            return true;
        }

        private void RegisterWithGmcm()
        {
            var gmcm = this.Helper.ModRegistry.GetApi<IGmcmApi>("spacechase0.GenericModConfigMenu");
            if (gmcm is null) return;

            gmcm.Register(
                mod:   this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save:  () => this.Helper.WriteConfig(this.Config)
            );

            gmcm.AddSectionTitle(mod: this.ModManifest, text: () => "Spawn Chance");

            gmcm.AddNumberOption(
                mod:         this.ModManifest,
                getValue:    () => (float)this.Config.SpawnChancePerDay,
                setValue:    v  => this.Config.SpawnChancePerDay = v,
                name:        () => "Spawn Chance Per Day",
                tooltip:     () => "Chance that a fruit tree spawns in a given location on a given day. Independent of map size. Default: 0.5%.",
                min:         0f,
                max:         1f,
                interval:    0.001f,
                formatValue: v => $"{v * 100:0.0}%"
            );

            gmcm.AddNumberOption(
                mod:      this.ModManifest,
                getValue: () => this.Config.SpawnsPerSuccessfulRoll,
                setValue: v  => this.Config.SpawnsPerSuccessfulRoll = v,
                name:     () => "Trees Per Successful Roll",
                tooltip:  () => "How many trees to place when the daily roll succeeds. Default: 1.",
                min:      1,
                max:      10
            );

            gmcm.AddSectionTitle(mod: this.ModManifest, text: () => "Behaviour");

            gmcm.AddBoolOption(
                mod:      this.ModManifest,
                getValue: () => this.Config.RandomizeStartingGrowthStage,
                setValue: v  => this.Config.RandomizeStartingGrowthStage = v,
                name:     () => "Randomize Starting Growth Stage",
                tooltip:  () => "If enabled, spawned trees start at a random young stage (seed to bush) and grow naturally. If disabled, trees always start as seeds."
            );

            gmcm.AddBoolOption(
                mod:      this.ModManifest,
                getValue: () => this.Config.IncludeModdedTrees,
                setValue: v  => this.Config.IncludeModdedTrees = v,
                name:     () => "Include Modded Trees",
                tooltip:  () => "If enabled, modded fruit trees (from other mods) are also eligible to spawn. If disabled, only the 8 vanilla trees can spawn."
            );

            gmcm.AddSectionTitle(
                mod:     this.ModManifest,
                text:    () => "Allowed Locations",
                tooltip: () => "Choose which locations fruit trees can naturally spawn in. If all are disabled, trees can spawn in any outdoor location."
            );

            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnFarm,      v => this.Config.SpawnOnFarm      = v, () => "Farm");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnForest,    v => this.Config.SpawnOnForest    = v, () => "Forest");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnTown,      v => this.Config.SpawnOnTown      = v, () => "Town");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnMountain,  v => this.Config.SpawnOnMountain  = v, () => "Mountain");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnBusStop,   v => this.Config.SpawnOnBusStop   = v, () => "Bus Stop");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnRailroad,  v => this.Config.SpawnOnRailroad  = v, () => "Railroad");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnBeach,     v => this.Config.SpawnOnBeach     = v, () => "Beach");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnWoods,     v => this.Config.SpawnOnWoods     = v, () => "Secret Woods");
            gmcm.AddBoolOption(this.ModManifest, () => this.Config.SpawnOnBackwoods, v => this.Config.SpawnOnBackwoods = v, () => "Backwoods");
        }
    }
}
