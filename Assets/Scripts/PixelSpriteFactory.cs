using System.Collections.Generic;
using UnityEngine;

namespace ElysiumMall
{
    public static class PixelSpriteFactory
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private static void Pixel(Color32[] pixels, int size, int x, int y, Color32 color)
        {
            if (x >= 0 && x < size && y >= 0 && y < size) pixels[y * size + x] = color;
        }

        private static void Box(Color32[] pixels, int size, int x0, int y0, int x1, int y1, Color32 color)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++) Pixel(pixels, size, x, y, color);
        }

        public static Sprite Create(string kind)
        {
            kind = string.IsNullOrEmpty(kind) ? "buddy" : kind.ToLowerInvariant();
            if (Cache.TryGetValue(kind, out Sprite cached))
            {
                return cached;
            }

            const int size = 16;
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 blue = new Color32(20, 100, 220, 255);
            Color32 red = new Color32(220, 45, 50, 255);
            Color32 gold = new Color32(255, 205, 40, 255);
            Color32 white = new Color32(245, 245, 235, 255);
            Color32 dark = new Color32(25, 30, 50, 255);
            Color32 cyan = new Color32(40, 210, 220, 255);

            Color32[] pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

            switch (kind)
            {
                case "ring":
                    for (int y = 3; y < 13; y++)
                        for (int x = 3; x < 13; x++)
                        {
                            int dx = x - 8;
                            int dy = y - 8;
                            int distance = dx * dx + dy * dy;
                            if (distance >= 10 && distance <= 32) Pixel(pixels, size, x, y, gold);
                        }
                    break;
                case "shoe":
                    Box(pixels, size, 3, 6, 11, 11, blue);
                    Box(pixels, size, 7, 4, 12, 8, blue);
                    Box(pixels, size, 2, 11, 13, 13, white);
                    Box(pixels, size, 5, 5, 6, 6, white);
                    break;
                case "tech":
                    Box(pixels, size, 4, 3, 11, 12, cyan);
                    Box(pixels, size, 6, 5, 9, 8, dark);
                    Box(pixels, size, 3, 6, 4, 9, red);
                    Box(pixels, size, 11, 6, 12, 9, red);
                    break;
                case "jacket":
                    Box(pixels, size, 4, 4, 11, 12, red);
                    Box(pixels, size, 3, 6, 4, 11, red);
                    Box(pixels, size, 11, 6, 12, 11, red);
                    Box(pixels, size, 7, 4, 8, 12, white);
                    break;
                default:
                    Box(pixels, size, 5, 5, 10, 11, white);
                    Box(pixels, size, 4, 3, 6, 5, cyan);
                    Box(pixels, size, 9, 3, 11, 5, cyan);
                    Box(pixels, size, 6, 7, 6, 7, dark);
                    Box(pixels, size, 9, 7, 9, 7, dark);
                    Box(pixels, size, 6, 10, 9, 10, gold);
                    break;
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Generated_" + kind,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 8f);
            sprite.name = "GeneratedSprite_" + kind;
            Cache[kind] = sprite;
            return sprite;
        }
    }
}
