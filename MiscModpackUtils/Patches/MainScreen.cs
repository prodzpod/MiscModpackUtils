using BepInEx;
using BepInEx.Configuration;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MiscModpackUtils.Patches
{
    public class MainScreen
    {
        public static ConfigEntry<bool> Override;
        public static void Init()
        {
            Override = Main.Config.Bind("General", "Enable Custom Logo", false, "change the logo at config/logo.png, then set this to true");
            if (!Override.Value) return;
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += (orig, self, mainMenuController) =>
            {
                orig(self, mainMenuController);
                GameObject obj = GameObject.Find("LogoImage");
                if (obj == null) return;
                obj.transform.localPosition = new(0, 20, 0);
                obj.transform.localScale = new(1.5f, 1.5f, 1.5f);
                Texture2D texture = new(1589, 964, TextureFormat.RGB24, false);
                texture.filterMode = FilterMode.Trilinear;
                texture.LoadImage(File.ReadAllBytes(Path.Combine(Paths.ConfigPath, "logo.png")));
                obj.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, 1589, 964), new Vector2(0.5f, 0.5f), 3f);
                Main.Log.LogInfo("Changed Logo Image");
            };
        }
    }
}