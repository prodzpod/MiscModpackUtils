using BepInEx.Configuration;
using HarmonyLib;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MiscModpackUtils.Patches
{
    public class SkillOrder
    {
        public static ConfigEntry<string> OverridesRaw;
        public static List<string> Overrides = [];
        public static void Init()
        {
            OverridesRaw = Main.Config.Bind("Reordering", "Skill Ordering", "", "skill names, separated by commas. ones that does not appear on the list keeps its original ordering.");
            if (string.IsNullOrWhiteSpace(OverridesRaw.Value)) return;
            foreach (var entry in OverridesRaw.Value.Split(",").Where(x => !string.IsNullOrWhiteSpace(x))) Overrides.Add(entry.Trim().ToUpper());
            RoR2Application.onLoad += () =>
            {
                ExpansionDef DLC1 = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
                // StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                try
                {
                    IEnumerable<GameObject> bodyPrefabs = BodyCatalog.allBodyPrefabs;
                    foreach (var bodyPrefab in bodyPrefabs)
                    {
                        if (bodyPrefab == null) continue;
                        BodyIndex idx = BodyCatalog.FindBodyIndex(bodyPrefab);
                        if (idx == BodyIndex.None) continue;
                        GenericSkill[] skills = BodyCatalog.GetBodyPrefabSkillSlots(idx);
                        if (skills == null) continue;
                        for (var i = 0; i < skills.Length; i++)
                        {
                            GenericSkill skill = skills[i];
                            if (skill?.skillFamily?.variants == null) continue;
                            List<SkillFamily.Variant> variants = skill.skillFamily.variants.ToList();
                            if (variants.Count == 0) continue;
                            bool modified = false;
                            uint dvi = skill.skillFamily.defaultVariantIndex;
                            foreach (var name in Overrides)
                            {
                                int variantIndex = variants.FindIndex(variant => variant.skillDef?.skillNameToken.Trim().ToUpper() == name);
                                if (variantIndex == -1) continue;
                                SkillFamily.Variant variant = variants[variantIndex];
                                if (dvi == variantIndex) dvi = (uint)(skill.skillFamily.variants.Length - 1);
                                else if (dvi > variantIndex) dvi--;
                                variants.Remove(variant);
                                variants.Add(variant);
                                modified = true;
                            }
                            List<string> names = [];
                            foreach (var variant in variants) if (variant.skillDef?.skillNameToken != null) names.Add(variant.skillDef.skillNameToken.Trim().ToUpper());
                            // if (names.Count > 0 && names[0].Trim().Length > 0) stringBuilder.AppendLine("Skills for " + BodyCatalog.GetBodyName(idx) + ", Family " + SkillCatalog.GetSkillFamilyName(skill.skillFamily.catalogIndex) + ": " + names.Join());
                            if (modified)
                            {
                                int skillIndex = SkillCatalog.allSkillFamilies.ToList().FindIndex(x => x.variants == skill.skillFamily.variants);
                                if (skillIndex != -1)
                                {
                                    SkillCatalog._allSkillFamilies[skillIndex].variants = variants.ToArray();
                                    SkillCatalog._allSkillFamilies[skillIndex].defaultVariantIndex = dvi;
                                }
                                skill._skillFamily.variants = variants.ToArray();
                                skills[i] = skill;
                                SkillLocator sl = bodyPrefab.GetComponent<SkillLocator>();
                                if (sl.primary != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.primary == sl?.allSkills[i]) sl.primary = skill;
                                if (sl.secondary != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.secondary == sl?.allSkills[i]) sl.secondary = skill;
                                if (sl.utility != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.utility == sl?.allSkills[i]) sl.utility = skill;
                                if (sl.special != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.special == sl?.allSkills[i]) sl.special = skill;
                                if (sl.primaryBonusStockOverrideSkill != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.primaryBonusStockOverrideSkill == sl?.allSkills[i]) sl.primaryBonusStockOverrideSkill = skill;
                                if (sl.secondaryBonusStockOverrideSkill != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.secondaryBonusStockOverrideSkill == sl?.allSkills[i]) sl.secondaryBonusStockOverrideSkill = skill;
                                if (sl.utilityBonusStockOverrideSkill != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.utilityBonusStockOverrideSkill == sl?.allSkills[i]) sl.utilityBonusStockOverrideSkill = skill;
                                if (sl.specialBonusStockOverrideSkill != null && sl.allSkills != null && sl.allSkills.Length >= i && sl.specialBonusStockOverrideSkill == sl?.allSkills[i]) sl.specialBonusStockOverrideSkill = skill;
                                if (sl.allSkills != null && sl.allSkills.Length >= i) sl.allSkills[i] = skill;
                            }
                        }
                    }
                }
                catch (Exception ex) { Main.Log.LogError(ex); }
                try
                {
                    IEnumerable<GameObject> bodyPrefabs = BodyCatalog.allBodyPrefabs;
                    foreach (var bodyPrefab in bodyPrefabs)
                    {
                        if (bodyPrefab == null) continue;
                        BodyIndex idx = BodyCatalog.FindBodyIndex(bodyPrefab);
                        SkinDef[] skins = BodyCatalog.GetBodySkins(idx);
                        if (skins.Length == 0) continue;
                        List<SkinDef> skinsList = new(skins);
                        bool modified = false;
                        foreach (var name in Overrides)
                        {
                            SkinDef skin = skinsList.Find((skin) => skin.nameToken.Trim().ToUpper() == name);
                            if (skin == null) continue;
                            skinsList.Remove(skin);
                            skinsList.Add(skin);
                            modified = true;
                        }
                        List<string> names = new();
                        foreach (var skin in skinsList) names.Add(skin.nameToken.Trim().ToUpper());
                        // stringBuilder.AppendLine("Skins for " + BodyCatalog.GetBodyName(idx) + ": " + names.Join());
                        if (modified)
                        {
                            BodyCatalog.skins[(int)idx] = skinsList.ToArray();
                            SkinCatalog.skinsByBody[(int)idx] = skinsList.ToArray();
                            bodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<ModelSkinController>().skins = skinsList.ToArray(); // ??
                        }
                    }
                }
                catch (Exception ex) { Main.Log.LogError(ex); }
                // Main.Log.LogDebug(stringBuilder.ToString());
                // HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            };
        }
    }
}