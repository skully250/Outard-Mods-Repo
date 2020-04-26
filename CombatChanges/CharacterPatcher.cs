using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace CombatChanges
{
    [HarmonyPatch(typeof(Character), "StabilityHit", new Type[] { typeof(float), typeof(float), typeof(bool), typeof(Character) })]
    public class StabilityPatcher
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
            if (Time.time - ___m_timeOfLastStabilityHit > 3f && !___m_impactImmune)
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

    [HarmonyPatch(typeof(Character), "SlowDown", new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) })]
    class SlowDownPatcher
    {
        [HarmonyPrefix]
        static void ChangeVariables(float _slowVal, float _timeTo, float _timeStay, float _timeFrom)
        {
            _slowVal *= 0.3f;
            _timeTo *= 0.3f;
            _timeStay *= 0.3f;
            _timeFrom *= 0.3f;
        }
    }

    //Dodge Patcher code courtesy of Elec - Used with permission
    [HarmonyPatch(typeof(Character), "SendDodgeTriggerTrivial", new Type[] { typeof(Vector3) })]
    class DodgePatcher
    {
        public static float min_dodge = 0.0f;
        public static float min_restricted_dodge = 0.2f;
        public static float min_bag_num = 0.4f; //40% bag fill before slower dodge
        public static float max_dodge = 1.0f;

        //When transpiling is fixed only the DodgeDirection if statement will be kept
        [HarmonyPrefix]
        static bool DodgeTrigger(Vector3 _direction, Character __instance, ref Animator ___m_animator, ref bool ___m_currentlyChargingAttack, 
            ref SoundPlayer ___m_dodgeSoundPlayer, ref bool ___m_dodging, ref CharacterSoundManager ___m_characterSoundManager)
        {
            MethodInfo SendCancelCharging = AccessTools.Method("Character:SendCancelCharging");
            MethodInfo StopBlocking = AccessTools.Method("Character:StopBlocking");
            if (__instance.HasDodgeDirection)
                ___m_animator.SetFloat("DodgeBlend", !__instance.DodgeRestricted ? 0.0f : getDodgeRestrictedAmount(__instance));
            ___m_animator.SetTrigger("Dodge");
            if (___m_currentlyChargingAttack)
            {
                SendCancelCharging.Invoke(__instance, new object[] { });
            }
            ___m_dodgeSoundPlayer.Play(false);
            ___m_dodging = true;
            StopBlocking.Invoke(__instance, new object[] { });
            __instance.OnDodgeEvent?.Invoke();
            if (___m_characterSoundManager != null)
            {
                Global.AudioManager.PlaySoundAtPosition(___m_characterSoundManager.GetDodgeSound(), __instance.transform, 0f, 1f, 1f, 1f, 1f);
            }
            __instance.SendMessage("DodgeTrigger", _direction, SendMessageOptions.DontRequireReceiver);
            return false;
        }

        private static float getDodgeRestrictedAmount(Character self)
        {
            float cur_dodge = min_dodge;
            if (!self.DodgeRestricted)
                return min_dodge;

            Bag bag = getCharacterBag(self);
            if (bag == null)
                return min_dodge;

            float cap = bag.BagCapacity;
            float weight = bag.Weight;

            cur_dodge = ((Mathf.Max(weight - (min_bag_num * cap), 0) / cap) + min_restricted_dodge) * max_dodge;
            return cur_dodge;
        }

        private static Bag getCharacterBag(Character self)
        {
            EquipmentSlot[] equipSlots = self.Inventory.Equipment.EquipmentSlots;
            for (int i = 0; i < equipSlots.Length; i++)
            {
                if (equipSlots[i] == null || equipSlots[i].EquippedItem == null)
                    continue;
                Item equippedItem = equipSlots[i].EquippedItem;
                if (!(equippedItem is Equipment))
                    continue;

                Equipment equipment = (Equipment)equippedItem;
                if (!(equipment is Bag))
                    continue;
                return (Bag)equipment;
            }
            return null;
        }

        //[HarmonyTranspiler]
        //This code will not run until a fix for transpiling is put in the game
        static IEnumerable<CodeInstruction> TranspileMethod(IEnumerable<CodeInstruction> instructions)
        {
            /* We are looking for the following two lines, and we want to skip them
             * if (this.HasDodgeDirection)
             *     this.m_animator.SetFloat("DodgeBlend", !this.DodgeRestricted ? 0.0f : 1f);
             *    
             * The compiler switches the condition into a branch-if-false, so what we're going to do is find that brfalse
             * and switch it to br, which is unconditional branch, which will skip the SetFloat method we don't want.
             * One problem with this is that brfalse consumes an item from the stack, which was loaded by the last command:
             *       IL_0001: call instance bool Character::get_HasDodgeDirection()
             * 
             * Whereas br does not consume anything. This leaves an extra value on the stack that throws an error relating to the 'ret'
             * command at the end of the method, because this method is void and shouldn't return anything. So, we need to 'pop'
             * the extra value off *before* we jump.
             */
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Brfalse)
                    continue;

                if (codes[i -1].operand.ToString().Contains("HasDodgeDirection"))
                {
                    codes[i].opcode = OpCodes.Br;
                    codes.Insert(i, new CodeInstruction(OpCodes.Pop));
                }
                break;
            }
            return codes.AsEnumerable();
        }
    }
}