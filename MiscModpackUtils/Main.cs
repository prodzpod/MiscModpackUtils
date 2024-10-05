using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MiscModpackUtils.Patches;
using RoR2;
using RoR2.Skills;
using System.Linq;
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace MiscModpackUtils
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = "zzz." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "prodzpod";
        public const string PluginName = "MiscModpackUtils";
        public const string PluginVersion = "1.0.0";
        public static ManualLogSource Log;
        public static PluginInfo pluginInfo;
        public static Harmony Harmony;
        public static ConfigFile Config;
        public static ConfigEntry<bool> Debug;

        public void Awake()
        {
            pluginInfo = Info;
            Log = Logger;
            Harmony = new(PluginGUID);
            Config = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, PluginGUID + ".cfg"), true);
            Debug = Config.Bind("General", "Debug Mode", true, "Prints necessary info");
            LanguageOverride.Init();
            ItemLookSwap.Init();
            ItemAchievementSwap.Init();
            EquipmentCooldown.Init();
            SkillAchievementSwap.Init();
            EliteLookSwap.Init();
            SkillOrder.Init();
            PickupLogbookOrder.Init();
            MonsterLogbookOrder.Init();
            StageLogbookOrder.Init();
            SurvivorLogbookOrder.Init();
            AchievementLogbookOrder.Init();
            PauseScreen.Init();
            MainScreen.Init();
            AddArtifactCodes.Init();
            RoR2Application.onLoad += () =>
            {
                OnBasicallyEverythingLoaded();
                if (!Debug.Value) return;
                Log.LogInfo("Debug Dumped");
                Log.LogInfo("Items: " + string.Join(", ", ItemCatalog.allItemDefs.Select(x => x.nameToken)));
                Log.LogInfo("Equipments: " + string.Join(", ", EquipmentCatalog.equipmentDefs.Select(x => x.nameToken)));
                Log.LogInfo("Entities: " + string.Join(", ", BodyCatalog.allBodyPrefabBodyBodyComponents.Select(x => x.baseNameToken)));
                Log.LogInfo("Survivors: " + string.Join(", ", SurvivorCatalog.cachedSurvivorNames));
                Log.LogInfo("Elites: " + string.Join(", ", EquipmentCatalog.equipmentDefs.Where(x => x.passiveBuffDef?.eliteDef != null).Select(x => x.name)));
                Log.LogInfo("Skills: " + string.Join(", ", SkillCatalog.allSkillFamilies.Select(x => string.Join(", ", x.variants.Select(x => x.skillDef.skillNameToken)))));
                Log.LogInfo("Skins: " + string.Join(", ", SkinCatalog.allSkinDefs.Select(x => x.nameToken)));
                Log.LogInfo("Stages: " + string.Join(", ", SceneCatalog.allStageSceneDefs.Select(x => x.cachedName)));
                Log.LogInfo("Artifacts: " + string.Join(", ", ArtifactCatalog.artifactDefs.Select(x => x.nameToken)));
                Log.LogInfo("Achievements: " + string.Join(", ", AchievementManager.allAchievementDefs.Select(x => x.nameToken)));
            };
        }
        public static void OnBasicallyEverythingLoaded()
        {
            ItemLookSwap.OnBasicallyEverythingLoaded();
            ItemAchievementSwap.OnBasicallyEverythingLoaded();
            SkillAchievementSwap.OnBasicallyEverythingLoaded();
            AddArtifactCodes.OnBasicallyEverythingLoaded();
        }
    }
}
