using System.Collections.Generic;

namespace RareNaturallySpawningFruitTrees
{

    public class ModConfig
    {

        public double SpawnChancePerDay { get; set; } = 0.01;
        
        public int SpawnsPerSuccessfulRoll { get; set; } = 1;
        
        public bool RandomizeStartingGrowthStage { get; set; } = true;

        public bool IncludeModdedTrees { get; set; } = true;

        public bool SpawnOnFarm       { get; set; } = true;
        public bool SpawnOnForest     { get; set; } = true;
        public bool SpawnOnTown       { get; set; } = true;
        public bool SpawnOnMountain   { get; set; } = true;
        public bool SpawnOnBusStop    { get; set; } = true;
        public bool SpawnOnRailroad   { get; set; } = true;
        public bool SpawnOnBeach      { get; set; } = false;
        public bool SpawnOnWoods      { get; set; } = false;
        public bool SpawnOnBackwoods  { get; set; } = false;

        public IEnumerable<string> GetEnabledLocations()
        {
            if (this.SpawnOnFarm)       yield return "Farm";
            if (this.SpawnOnForest)     yield return "Forest";
            if (this.SpawnOnTown)       yield return "Town";
            if (this.SpawnOnMountain)   yield return "Mountain";
            if (this.SpawnOnBusStop)    yield return "BusStop";
            if (this.SpawnOnRailroad)   yield return "Railroad";
            if (this.SpawnOnBeach)      yield return "Beach";
            if (this.SpawnOnWoods)      yield return "Woods";
            if (this.SpawnOnBackwoods)  yield return "Backwoods";
        }
    }
}
