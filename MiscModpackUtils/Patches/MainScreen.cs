using BepInEx;
using BepInEx.Configuration;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace MiscModpackUtils.Patches
{
    public class MainScreen
    {
        public static ConfigEntry<bool> Override;
        public static ConfigEntry<string> CameraPosition;
        public static ConfigEntry<string> CameraRotation;
        public static ConfigEntry<string> ColorHSV;
        public static ConfigEntry<string> ColorGain;
        public static ConfigEntry<string> ColorBC;
        public static ConfigEntry<string> VersionAddition;

        public static void Init()
        {
            Override = Main.Config.Bind("Main Screen", "Enable Custom Logo", true, "change the logo at config/logo.png, then set this to true");
            CameraPosition = Main.Config.Bind("Main Screen", "Camera Position", "0, 1, -10", "RM: -20, 5, -10");
            CameraRotation = Main.Config.Bind("Main Screen", "Camera Rotation", "0, 0, 0", "RM: 348, 4, 357");
            ColorHSV = Main.Config.Bind("Main Screen", "Color HSV addition", "0, -2.5, 0", "RM: -200, 100, 0");
            ColorGain = Main.Config.Bind("Main Screen", "Color Gain addition", "1, 1, 1, 0", "RM: 1, 1, 1, 0");
            ColorBC = Main.Config.Bind("Main Screen", "Color Brightness/Contrast", "0, 42.2", "RM: 0, 150");
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += (orig, self, mainMenuController) =>
            {
                orig(self, mainMenuController);
                if (Override.Value)
                {
                    GameObject obj = GameObject.Find("LogoImage");
                    if (obj == null) return;
                    obj.transform.localPosition = new(0, 20, 0);
                    obj.transform.localScale = new(1.5f, 1.5f, 1.5f);
                    obj.GetComponent<Image>().sprite = Utils.Load(Path.Combine(Paths.ConfigPath, "logo.png"));
                    Main.Log.LogInfo("Changed Logo Image");
                }
                if (mainMenuController.titleMenuScreen != null)
                {
                    Main.Log.LogDebug("Changing Title Camera");
                    var pos = ToVector3(CameraPosition.Value); 
                    var rot = ToVector3(CameraRotation.Value);
                    if (pos != new Vector3(0, 1, -10)) mainMenuController.titleMenuScreen.desiredCameraTransform.position = pos;
                    if (rot != Vector3.zero) mainMenuController.titleMenuScreen.desiredCameraTransform.eulerAngles = rot;
                }
                GameObject volume = GameObject.Find("GlobalPostProcessVolume");
                if (volume == null) return; // what is this place lmfao
                ColorGrading cgrade = volume.GetComponent<PostProcessVolume>().profile.GetSetting<ColorGrading>();
                if (cgrade != null)
                {
                    Main.Log.LogDebug("Changing PostProcessing");
                    var HSV = ToVector3(ColorHSV.Value);
                    var Gain = ToVector4(ColorGain.Value);
                    var BC = ToVector2(ColorBC.Value);
                    var newGain = Gain + new Vector4(HSV.z, HSV.z, HSV.z, 0);
                    if (newGain != new Vector4(1, 1, 1, 0)) { cgrade.gain.value = newGain; cgrade.gain.overrideState = true; }
                    if (HSV.x != 0) { cgrade.hueShift.value = HSV.x; cgrade.hueShift.overrideState = true; }
                    if (HSV.y != -2.5f) { cgrade.saturation.value = HSV.y; cgrade.saturation.overrideState = true; }
                    if (BC.x != 0) { cgrade.brightness.value = BC.x; cgrade.brightness.overrideState = true; }
                    if (BC.y != 42.2f) { cgrade.contrast.value = BC.y; cgrade.contrast.overrideState = true; }
                }
            };
            VersionAddition = Main.Config.Bind("Main Screen", "Version Suffix", "", "version text to add after the ror version.");
            if (!string.IsNullOrWhiteSpace(VersionAddition.Value)) On.RoR2.UI.SteamBuildIdLabel.Start += (orig, self) =>
            {
                orig(self);
                int[] vers = Main.PluginVersion.Split('.').ToList().ConvertAll(x => int.Parse(x)).ToArray();
                self.GetComponent<TextMeshProUGUI>().text += " " + VersionAddition.Value;
            };
        }

        public static Vector2 ToVector2(string raw)
        {
            try
            {
                float[] temp = raw.Split(",").Select(x => float.Parse(x.Trim())).ToArray();
                return new(temp[0], temp[1]);
            }
            catch { Main.Log.LogWarning("Malformed config " + raw + "!"); return Vector2.zero; }
        }

        public static Vector3 ToVector3(string raw)
        {
            try
            {
                float[] temp = raw.Split(",").Select(x => float.Parse(x.Trim())).ToArray();
                return new(temp[0], temp[1], temp[2]);
            }
            catch { Main.Log.LogWarning("Malformed config " + raw + "!"); return Vector3.zero; }
        }

        public static Vector4 ToVector4(string raw)
        {
            try
            {
                float[] temp = raw.Split(",").Select(x => float.Parse(x.Trim())).ToArray();
                return new(temp[0], temp[1], temp[2], temp[3]);
            }
            catch { Main.Log.LogWarning("Malformed config " + raw + "!"); return Vector4.zero; }
        }
    }
}