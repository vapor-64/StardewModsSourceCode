using Microsoft.Xna.Framework;
using StardewUI.Framework;
using StardewValley;
using StardewValley.Menus;

using XnaColor = Microsoft.Xna.Framework.Color;

namespace SitToPassTime
{
    internal static class TimeSkipMenu
    {
        private const string ViewAsset = "Mods/vapor64.SitToPassTime/Views/TimePickerMenu";
        
        public static IClickableMenu Create(IViewEngine viewEngine)
        {
            var vm = new TimePickerViewModel(
                onConfirm: targetTime =>
                {
                    Game1.activeClickableMenu = null;
                    SafelySkipTimeTo(targetTime);
                },
                onCancel: () =>
                {
                    Game1.activeClickableMenu = null;
                    Game1.playSound("bigDeSelect");
                }
            );

            return viewEngine.CreateMenuFromAsset(ViewAsset, vm);
        }
        
        internal static void SafelySkipTimeTo(int targetTime)
        {
            if (targetTime <= Game1.timeOfDay)
                return;

            int intervals = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, targetTime) / 10;
            for (int i = 0; i < intervals; i++)
                Game1.performTenMinuteClockUpdate();

            Game1.outdoorLight     = XnaColor.White;
            Game1.ambientLight     = XnaColor.White;
            Game1.gameTimeInterval = 0;
            Game1.UpdateGameClock(Game1.currentGameTime);

            Game1.playSound("healSound");
        }
    }
}
