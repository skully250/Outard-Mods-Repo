using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CombatChanges
{
    public class ScriptLoad : MonoBehaviour
    {
        float staggerTimer;

        public static CombatChanges mod;
        public void Initialise()
        {
            Patch();
        }

        public void Patch()
        {
            On.Character.StabilityHit += new On.Character.hook_StabilityHit(StabilityHit);
        }

        [PunRPC]
        public void setStaggerTimer(float timer)
        {
            staggerTimer = 3f;
        }

        public void StabilityHit(On.Character.orig_StabilityHit original, Character character, float _knockValue, float _angle, bool _block)
        {
            //Working out how to do this in an easy unconflicting way
            character.photonView.RPC("setStaggerTimer", PhotonTargets.All, 3f);
        }
    }
}