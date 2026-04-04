using StardewUI.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SitToPassTime
{
    public class ModEntry : Mod
    {
        private ModConfig Config    = null!;
        private SitManager SitManager = null!;
        
        public override void Entry(IModHelper helper)
        {
            Config     = helper.ReadConfig<ModConfig>();
            SitManager = new SitManager(Config, Monitor);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded   += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted   += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            Monitor.Log($"{ModManifest.Name} loaded.", LogLevel.Info);
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var viewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI")!;

            viewEngine.RegisterViews(
                "Mods/vapor64.SitToPassTime/Views",
                "assets/views");

            viewEngine.RegisterSprites(
                "Mods/vapor64.SitToPassTime/Sprites",
                "assets/sprites");

            SitManager.SetViewEngine(viewEngine);
            Monitor.Log("StardewUI registered.", LogLevel.Debug);

            GMCMHelper.Register(Helper, ModManifest, Config);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SitManager.Reset();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            SitManager.Reset();
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return;
            SitManager.Update();
        }
    }
}
