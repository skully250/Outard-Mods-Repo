using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CombatChanges
{
    [BepInPlugin(ModID, ModName, ModVersion)]
    public class CombatChanges : BaseUnityPlugin
    {
        public const string ModID = "com.faedar.combatMods";
        public const string ModName = "Fae's Combat Modifications";
        public const string ModVersion = "1.0.0";

        public static CombatChanges instance = null;
        public Harmony harmony = null;

        public void Awake()
        {
            CombatChanges.instance = this;
            harmony = new Harmony(ModID);
            harmony.PatchAll();
            //Additional non-patch code below
        }
    }
}