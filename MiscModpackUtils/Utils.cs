using UnityEngine;

namespace MiscModpackUtils
{
    public class Utils
    {
        public static Color TRANSPARENT = new(0, 0, 0, 0);
        public static Sprite GetComposite(Texture2D bg, Texture2D fg)
        {
            Vector2 newSize = new(Mathf.Max(bg.width, fg.width), Mathf.Max(bg.height, fg.height));
            Vector2 offsetBG = new(Mathf.Floor((bg.width - newSize.x) / 2), Mathf.Floor((bg.width - newSize.y) / 2));
            Vector2 offsetFG = new(Mathf.Floor((fg.width - newSize.x) / 2), Mathf.Floor((fg.width - newSize.y) / 2));
            var tex = new Texture2D((int)newSize.x, (int)newSize.y);
            for (int x = 0; x < newSize.x; x++) for (int y = 0; y < newSize.y; y++) 
                tex.SetPixel(x, y, Over(Pixel(bg, x + offsetBG.x, y + offsetBG.y), Pixel(fg, x + offsetFG.x, y + offsetFG.y)));
            return Sprite.Create(tex, new(0, 0, newSize.x, newSize.y), new Vector2(0.5f, 0.5f), 3f);
        }
        public static Color Pixel(Texture2D img, float x, float y)
        {
            if (0 > x || x >= img.width || 0 > y || y >= img.height) return TRANSPARENT;
            return img.GetPixel((int)x, (int)y);
        }
        public static Color Over(Color bg, Color fg)
        {
            var a = bg.a * (1 - fg.a) + fg.a;
            return ((Vector4)fg * fg.a + (Vector4)bg * bg.a * (1 - fg.a)) / a;
        }
    }
}
