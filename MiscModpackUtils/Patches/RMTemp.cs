using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MiscModpackUtils.Patches
{
    public class RMTemp
    {
        public static void Init()
        {
            if (Main.Config.Bind("General", "Enable RM", false, "will be moved to rmbase in next version probably").Value)
            {
                On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += (orig, self, mainMenuController) =>
                {
                    orig(self, mainMenuController);
                    if (mainMenuController.titleMenuScreen != null)
                    {
                        Main.Log.LogDebug("Changing Title Camera");
                        mainMenuController.titleMenuScreen.desiredCameraTransform.position = new Vector3(-20, 5, -10);
                        mainMenuController.titleMenuScreen.desiredCameraTransform.rotation = new Quaternion(-0.104f, 0.130f, -0.014f, 0.986f);
                    }
                    GameObject volume = GameObject.Find("GlobalPostProcessVolume");
                    if (volume == null) return; // what is this place lmfao
                    ColorGrading cgrade = volume.GetComponent<PostProcessVolume>().profile.GetSetting<ColorGrading>();
                    if (cgrade != null)
                    {
                        Main.Log.LogDebug("Changing PostProcessing");
                        cgrade.gain.value = new Vector4(1, 1, 1, 0);
                        cgrade.gain.overrideState = true;
                        cgrade.hueShift.value = -200;
                        cgrade.hueShift.overrideState = true;
                        cgrade.saturation.value = 100;
                        cgrade.saturation.overrideState = true;
                        cgrade.contrast.value = 150;
                        cgrade.contrast.overrideState = true;
                    }
                };
                On.RoR2.UI.SteamBuildIdLabel.Start += (orig, self) =>
                {
                    orig(self);
                    int[] vers = Main.PluginVersion.Split('.').ToList().ConvertAll(x => int.Parse(x)).ToArray();
                    self.GetComponent<TextMeshProUGUI>().text += $" <style=cIsDamage>+ RM 2.0 Pre-Release Candidate 1</style>";
                };
            }
        }
    }
}
