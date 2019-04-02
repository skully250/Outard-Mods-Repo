using Partiality.Modloader;
using UnityEngine;

namespace CombatChanges
{
    public class CombatChanges : PartialityMod
    {
        public CombatChanges()
        {
            this.ModID = "Fae's Combat Modifications";
            this.Version = "0100";
            this.author = "Faedar";
        }

        public static ScriptLoad combatHooks;

        public override void OnEnable()
        {
            base.OnEnable();
            ScriptLoad.mod = this;
            GameObject obj = new GameObject();
            combatHooks = obj.AddComponent<ScriptLoad>();
            combatHooks.Initialise();
        }

        public override void OnLoad()
        {
            combatHooks.Load();
        }
    }
}