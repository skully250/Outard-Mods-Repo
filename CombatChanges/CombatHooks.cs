using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;

namespace CombatChanges
{
    [HarmonyPatch(typeof(Character), "StabilityHit", new Type[] { typeof(float), typeof(float), typeof(bool), typeof(Character) })]
    public class CharacterPatcher
    {
        static bool stagger = true;


        [HarmonyPrefix]
        public static bool prefixPatch(Character __instance, float _knockValue, float _angle, bool _block, Character _dealerChar,
            ref bool ___m_impactImmune, ref float ___m_shieldStability, ref float ___m_stability, ref float ___m_timeOfLastStabilityHit,
            ref float ___m_knockbackCount, ref bool ___m_knockHurtAllowed, ref Character.HurtType ___m_hurtType, ref bool ___m_currentlyChargingAttack,
            ref Animator ___m_animator)
        {
            MethodInfo knockMethod = AccessTools.Method("Character:Knock", new Type[] { typeof(bool) });
            MethodInfo knockRoutine = AccessTools.Method("Character:KnockhurtRoutine", new Type[] { typeof(float) });
            bool playerCheck = (!__instance.IsAI && __instance.photonView.isMine) || (__instance.IsAI && (_dealerChar == null || _dealerChar.photonView.isMine));
            float knock = 0f;
            if (_knockValue < 0f)
                knock = 0f;
            if (Time.time - ___m_timeOfLastStabilityHit > 4f)
                stagger = true;
            /*if (!___m_impactImmune && knock > 0f)
            {
                if (__instance.Stats.CurrentStamina < 1f)
                {
                    float stab = ___m_shieldStability + ___m_stability - 49f;
                    if (_knockValue < stab)
                        knock = stab;
                }
                if (__instance.CharacterCamera != null && knock > 0f)
                    __instance.CharacterCamera.Hit(knock * 6f);
                if (_block && ___m_shieldStability > 0f)
                {
                    if (knock > ___m_shieldStability)
                        ___m_stability -= knock - ___m_shieldStability;
                    ___m_shieldStability = Mathf.Clamp(___m_shieldStability - knock, 0f, 50f);
                }
                else
                    ___m_stability = Mathf.Clamp(___m_stability - knock, 0f, 100f);
                //Knock down isnt needed, only knock back
                if (___m_stability <= 0f || ___m_knockbackCount >= 3f)
                {
                    if (playerCheck)
                        __instance.photonView.RPC("SendKnock", PhotonTargets.All, new object[] { true, ___m_stability });
                    else
                        knockMethod.Invoke(__instance, new object[] { true });
                    ___m_stability = 0f;
                    if (__instance.IsPhotonPlayerLocal)
                        __instance._blockInput(false);
                }*/
            if (stagger)
            {
                if (playerCheck)
                {
                    __instance.photonView.RPC("SendKnock", PhotonTargets.All, new object[] { false, ___m_stability });
                    stagger = false;
                    ___m_timeOfLastStabilityHit = Time.time;
                }
                else
                {
                    knockMethod.Invoke(__instance, new object[] { false });
                    stagger = false;
                    ___m_timeOfLastStabilityHit = Time.time;
                }
                if (__instance.IsPhotonPlayerLocal && _block)
                    __instance.BlockInput(false);
            }
            else if (!_block)
            {
                if (___m_knockHurtAllowed)
                {
                    ___m_hurtType = Character.HurtType.Hurt;
                    if (___m_currentlyChargingAttack)
                        __instance.CancelCharging();
                    ___m_animator.SetTrigger("Knockhurt");
                    __instance.StopCoroutine("KnockhurtRoutine");
                    knockRoutine.Invoke(__instance, new object[] { _knockValue });
                }
            }
            else
            {
                ___m_hurtType = Character.HurtType.NONE;
                if (__instance.InLocomotion)
                    ___m_animator.SetTrigger("BlockHit");
                ___m_animator.SetInteger("KnockAngle", (int)_angle);
                __instance.StabilityHitCall?.Invoke();
            }
            return false;
        }
    }
}