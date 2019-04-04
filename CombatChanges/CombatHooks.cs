using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CombatChanges
{
    public class ScriptLoad : MonoBehaviour
    {
        public static CombatChanges mod;
        public void Initialise()
        {
            Patch();
        }

        public void Patch()
        {
            On.Character.StabilityHit += new On.Character.hook_StabilityHit(StabilityHit);
            On.Character.ctor += new On.Character.hook_ctor(Characterctor);
            StaggerTimeCount = new Dictionary<Character, float>();
        }

        public Dictionary<Character, float> StaggerTimeCount;

        public void Characterctor(On.Character.orig_ctor original, Character character)
        {
            original.Invoke(character);
            float StaggerTimer = 0f;
            StaggerTimeCount.Add(character, StaggerTimer);
        }

        public void StabilityHit(On.Character.orig_StabilityHit original, Character character, float _knockValue, float _angle, bool _block)
        {
            //Working out how to do this in an easy unconflicting way
        }
    }
}