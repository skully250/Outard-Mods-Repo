using HarmonyLib;
using UnityEngine;

namespace CombatChanges
{
    [HarmonyPatch(typeof(Skill), "StartInit")]
    class SkillPatcher
    {
        [HarmonyPostfix]
        static void ChangeCooldown(Skill __instance)
        {
            switch (__instance.Name)
            {
                case "Brace":
                    __instance.Cooldown = 120;
                    break;
                case "Counterstrike":
                    __instance.Cooldown = 10;
                    break;
                case "Serpent's Parry":
                    __instance.Cooldown = 10;
                    break;
                case "Simeon's Gambit":
                    __instance.Cooldown = 10;
                    break;
                case "Pommel Counter":
                    __instance.Cooldown = 10;
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(ResourcesPrefabManager), "Load")]
    class ResourcesPatcher
    {
        [HarmonyPostfix]
        static void addStatus(ResourcesPrefabManager __instance)
        {
            int[] itemIDs = { 8100261, 8100260, 8100362 };
            for (int i = 0; i < itemIDs.Length; i++)
            {
                var selectedSkill = __instance.GetItemPrefab(itemIDs[i]);
                var effects = new GameObject("Effects");
                effects.transform.parent = selectedSkill.transform;
                var addStatus = effects.AddComponent<AddStatusEffect>();
                addStatus.Status = __instance.GetStatusEffectPrefab("Pain");
                addStatus.Status.StatusData.LifeSpan = 1f;
                addStatus.SetChanceToContract(100); 
                GameObject.DontDestroyOnLoad(selectedSkill.gameObject);
            }
        }
    }
}
