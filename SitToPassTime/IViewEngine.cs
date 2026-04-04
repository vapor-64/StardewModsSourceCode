using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewUI.Framework;


public interface IViewEngine
{
    IViewDrawable CreateDrawableFromAsset(string assetName);
    IViewDrawable CreateDrawableFromMarkup(string markup);
    IMenuController CreateMenuControllerFromAsset(string assetName, object? context = null);
    IMenuController CreateMenuControllerFromMarkup(string markup, object? context = null);
    IClickableMenu CreateMenuFromAsset(string assetName, object? context = null);
    IClickableMenu CreateMenuFromMarkup(string markup, object? context = null);
    void EnableHotReloading(string? sourceDirectory = null);
    void PreloadAssets();
    void PreloadModels(params Type[] types);
    void RegisterCustomData(string assetPrefix, string modDirectory);
    void RegisterSprites(string assetPrefix, string modDirectory);
    void RegisterViews(string assetPrefix, string modDirectory);
}

public interface IViewDrawable : IDisposable
{
    Vector2 ActualSize { get; }
    object? Context { get; set; }
    Vector2? MaxSize { get; set; }
    void Draw(SpriteBatch b, Vector2 position);
}

public interface IMenuController : IDisposable
{
    event Action Closed;
    event Action Closing;
    Func<bool>? CanClose { get; set; }
    Action? CloseAction { get; set; }
    Vector2 CloseButtonOffset { get; set; }
    bool CloseOnOutsideClick { get; set; }
    string CloseSound { get; set; }
    float DimmingAmount { get; set; }
    IClickableMenu Menu { get; }
    Func<Point>? PositionSelector { get; set; }
    void ClearCursorAttachment();
    void Close();
    void EnableCloseButton(Texture2D? texture = null, Rectangle? sourceRect = null, float scale = 4f);
    void SetCursorAttachment(Texture2D texture, Rectangle? sourceRect = null, Point? size = null, Point? offset = null, Color? tint = null);
    void SetGutters(int left, int top, int right = -1, int bottom = -1);
}
