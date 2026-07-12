using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ForageAutomator.Rendering
{
    /// <summary>
    /// World overlays on RenderedWorld use viewport-local coordinates via GlobalToLocal.
    /// Zoom is applied once when the game composites the world buffer to the screen.
    /// </summary>
    internal static class OverlayDrawHelper
    {
        private const float DefaultLineThicknessPx = 3f;
        private const float DefaultMarkerSizePx = 8f;
        private const float MinZoomDivisor = 0.01f;
        private static readonly Vector2 PixelOrigin = new(0.5f, 0.5f);

        private static Texture2D? pixelTexture;

        public static float Zoom => Math.Max(MinZoomDivisor, Game1.options.zoomLevel);

        public static void EnsurePixelTexture()
        {
            if (pixelTexture != null && !pixelTexture.IsDisposed)
                return;

            pixelTexture = Game1.fadeToBlackRect ?? Game1.staminaRect;
            if (pixelTexture != null)
                return;

            pixelTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        public static Vector2 WorldToViewportLocal(Vector2 worldPosition)
        {
            return Game1.GlobalToLocal(Game1.viewport, worldPosition);
        }

        public static Vector2 GetPlayerWorldAnchor(Farmer player)
        {
            Vector2 local = GetPlayerViewportAnchor(player);
            return local + new Vector2(Game1.viewport.X, Game1.viewport.Y);
        }

        public static Vector2 GetPlayerViewportAnchor(Farmer player)
        {
            Vector2 local = player.getLocalPosition(Game1.viewport);
            return local + new Vector2(Game1.tileSize / 2f, Game1.tileSize / 4f);
        }

        public static void DrawLine(SpriteBatch batch, Vector2 startWorld, Vector2 endWorld, Color color, float thicknessPx = DefaultLineThicknessPx)
        {
            DrawLineSegment(
                batch,
                WorldToViewportLocal(startWorld),
                WorldToViewportLocal(endWorld),
                color,
                thicknessPx / Zoom,
                GetViewportLayerDepth(startWorld, endWorld));
        }

        public static void DrawMarker(SpriteBatch batch, Vector2 worldCenter, Color color, float sizePx = DefaultMarkerSizePx)
        {
            DrawFilledMarker(
                batch,
                WorldToViewportLocal(worldCenter),
                color,
                sizePx / Zoom,
                GetViewportLayerDepth(worldCenter, worldCenter));
        }

        private static Texture2D? GetPixel()
        {
            EnsurePixelTexture();
            return pixelTexture;
        }

        private static void DrawFilledMarker(SpriteBatch batch, Vector2 center, Color color, float sizePx, float layerDepth)
        {
            Texture2D? pixel = GetPixel();
            if (pixel == null)
                return;

            float size = Math.Max(4f, sizePx);
            batch.Draw(pixel, center, null, color, 0f, PixelOrigin, new Vector2(size, size), SpriteEffects.None, layerDepth);
        }

        private static void DrawLineSegment(SpriteBatch batch, Vector2 start, Vector2 end, Color color, float thicknessPx, float layerDepth)
        {
            Texture2D? pixel = GetPixel();
            if (pixel == null)
                return;

            Vector2 edge = end - start;
            float length = edge.Length();
            if (length < 0.5f)
                return;

            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float thickness = Math.Max(1f, thicknessPx);

            batch.Draw(
                pixel,
                start,
                null,
                color,
                angle,
                new Vector2(0f, PixelOrigin.Y),
                new Vector2(length, thickness),
                SpriteEffects.None,
                layerDepth);
        }

        private static float GetViewportLayerDepth(Vector2 startWorld, Vector2 endWorld)
        {
            float y = Math.Max(startWorld.Y, endWorld.Y);
            return (y + Game1.tileSize) / 10000f + 0.001f;
        }
    }

}
