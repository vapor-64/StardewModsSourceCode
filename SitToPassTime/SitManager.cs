using StardewUI.Framework;
using StardewModdingAPI;
using StardewValley;

namespace SitToPassTime
{
    internal class SitManager
    {
        private readonly ModConfig Config;
        private readonly IMonitor  Monitor;
        
        private IViewEngine ViewEngine = null!;

        private bool MenuShownThisSession;


        public SitManager(ModConfig config, IMonitor monitor)
        {
            Config  = config;
            Monitor = monitor;
        }
        
        public void SetViewEngine(IViewEngine viewEngine) => ViewEngine = viewEngine;

        public void Reset() => MenuShownThisSession = false;

        public void Update()
        {
            Farmer player = Game1.player;

            bool sittingOnFurniture =
                player.IsSitting()
                && !player.isStopSitting
                && player.sittingFurniture != null;

            if (!sittingOnFurniture)
            {
                if (MenuShownThisSession)
                {
                    MenuShownThisSession = false;
                    Monitor.Log("Player stood up; sit session reset.", LogLevel.Trace);
                }
                return;
            }

            if (MenuShownThisSession || Game1.activeClickableMenu != null)
                return;
            
            if (Config.DisablePopup)
                return;

            if (Game1.timeOfDay >= 2600)
            {
                Monitor.Log("Past 2am — skipping menu.", LogLevel.Trace);
                MenuShownThisSession = true;
                return;
            }

            Monitor.Log("Player sat on furniture — opening time-picker.", LogLevel.Debug);
            MenuShownThisSession      = true;
            Game1.activeClickableMenu = TimeSkipMenu.Create(ViewEngine);
        }
    }
}
