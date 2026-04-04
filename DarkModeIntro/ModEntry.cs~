using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace DarkModeIntro
{
    public class ModEntry : Mod
    {
        private Texture2D _blackTexture;
        private Texture2D _originalStaminaRect;
        private FieldInfo _logoFadeTimer;
        private FieldInfo _fadeFromWhiteTimer;

        public override void Entry(IModHelper helper)
        {
            _logoFadeTimer = typeof(TitleMenu).GetField("logoFadeTimer", BindingFlags.Public | BindingFlags.Instance);
            _fadeFromWhiteTimer = typeof(TitleMenu).GetField("fadeFromWhiteTimer", BindingFlags.Public | BindingFlags.Instance);

            if (_logoFadeTimer == null || _fadeFromWhiteTimer == null)
            {
                Monitor.Log("Failed to find TitleMenu timer fields - mod will not work.", LogLevel.Error);
                return;
            }
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_blackTexture == null && Game1.graphics?.GraphicsDevice != null)
            {
                _blackTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                _blackTexture.SetData(new[] { new Color(0x12, 0x12, 0x12) });
            }

            if (_blackTexture == null) return;

            if (Game1.activeClickableMenu is TitleMenu tm)
            {
                int logoFade = (int)_logoFadeTimer.GetValue(tm);
                int fadeFromWhite = (int)_fadeFromWhiteTimer.GetValue(tm);
                bool introActive = logoFade > 0 || fadeFromWhite > 0;

                if (introActive && _originalStaminaRect == null)
                {
                    _originalStaminaRect = Game1.staminaRect;
                    Game1.staminaRect = _blackTexture;
                }
                else if (!introActive && _originalStaminaRect != null)
                {
                    Game1.staminaRect = _originalStaminaRect;
                    _originalStaminaRect = null;
                }
            }
            else if (_originalStaminaRect != null)
            {
                Game1.staminaRect = _originalStaminaRect;
                _originalStaminaRect = null;
            }
        }
        
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Minigames/TitleButtons"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    var patchTexture = Helper.ModContent.Load<Texture2D>("assets/TitleButtons.png");

                    editor.PatchImage(
                        source: patchTexture,
                        sourceArea: new Rectangle(0, 306, 400, 138),  // pull from same region in your full-size PNG
                        targetArea: new Rectangle(0, 306, 400, 138),  // paste into same region in game texture
                        patchMode: PatchMode.Replace
                    );
                });
            }
        }
    }
}