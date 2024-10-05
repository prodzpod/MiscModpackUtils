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
                obj.transform.localPosition = new(0, 200, 0);
                obj.transform.localScale = new(2, 2, 2);
                obj.GetComponent<Image>().sprite = Resources.Load(Path.Combine(Paths.ConfigPath, "icon.png")) as Sprite;
                Main.Log.LogInfo("Changed Logo Image");
            };
        }
    }
}