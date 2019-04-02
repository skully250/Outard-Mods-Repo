using System.Collections.Generic;
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

        public void Load()
        {
            On.Character.Update += CharacterUpdate;
        }

        public void Unload()
        {
            On.Character.Update -= CharacterUpdate;
        }

        public void Patch()
        {
            On.Character.StabilityHit += new On.Character.hook_StabilityHit(StabilityHit);
            On.Character.ctor += new On.Character.hook_ctor(Character);
            StaggerTimeCount = new Dictionary<Character, float>();
        }

        public Dictionary<Character, float> StaggerTimeCount;

        public void Character(On.Character.orig_ctor original, Character character)
        {
            original.Invoke(character);
            float StaggerTimer = 0f;
            StaggerTimeCount.Add(character, StaggerTimer);
        }

        public void StabilityHit(On.Character.orig_StabilityHit original, Character character, float _knockValue, float _angle, bool _block)
        {
            original.Invoke(character, _knockValue, _angle, _block);
            float staggerTime;
            StaggerTimeCount.TryGetValue(character, out staggerTime);

            float num = _knockValue;
            if (num < 0f)
            {
                num = 0f;
            }
            if (!character.m_impactImmune && num > 0f)
            {
                if (character.Stats.CurrentStamina < 1f)
                {
                    float num2 = character.BlockStability + character.Stability - 49f;
                    if (num < num2)
                    {
                        num = num2;
                    }
                }
                character.m_timeOfLastStabilityHit = Time.time;
                if (character.CharacterCamera != null && num > 0f)
                {
                    character.CharacterCamera.Hit(num * 6f);
                }
                if (_block && character.BlockStability > 0f)
                {
                    if (num > character.BlockStability)
                    {
                        character.Stability -= num - character.BlockStability;
                    }
                    character.BlockStability = Mathf.Clamp(character.BlockStability - num, 0f, 50f);
                }
                else
                {
                    character.Stability = Mathf.Clamp(character.Stability - num, 0f, 100f);
                }
                if (character.Stability <= 0f)
                {
                    if (character.IsAI || character.photonView.isMine)
                    {
                        character.photonView.RPC("SendKnock", PhotonTargets.All, new object[]
                        {
                    true,
                    character.Stability
                        });
                    }
                    else
                    {
                        character.Knock(true);
                    }
                    character.Stability = 0f;
                    if (character.IsPhotonPlayerLocal)
                    {
                        character.BlockInput(false);
                    }
                }
                else if (staggerTime <= 0f)
                {
                    staggerTime = 3f;
                    if (character.IsAI || character.photonView.isMine)
                    {
                        character.photonView.RPC("SendKnock", PhotonTargets.All, new object[]
                        {
                    false,
                    character.Stability
                        });
                    }
                    else
                    {
                        character.Knock(false);
                    }
                    if (character.IsPhotonPlayerLocal && _block)
                    {
                        character.BlockInput(false);
                    }
                }
                else if (!_block)
                {
                    if (character.m_knockHurtAllowed)
                    {
                        character.m_hurtType = character.CharHurtType;
                        if (character.CurrentlyChargingAttack)
                        {
                            character.CancelCharging();
                        }
                        character.Animator.SetTrigger("Knockhurt");
                        character.StopCoroutine("KnockhurtRoutine");
                        character.StartCoroutine(character.KnockhurtRoutine(num));
                    }
                }
                else
                {
                    character.m_hurtType = Character.HurtType.NONE;
                    if (character.InLocomotion)
                    {
                        character.Animator.SetTrigger("BlockHit");
                    }
                }
                character.Animator.SetInteger("KnockAngle", (int)_angle);
                if (character.StabilityHitCall != null)
                {
                    character.StabilityHitCall();
                }
            }
        }

        public void CharacterUpdate(On.Character.orig_Update original, Character character)
        {
            original.Invoke(character);
            float staggerTime;
            StaggerTimeCount.TryGetValue(character, out staggerTime);
            staggerTime -= Time.deltaTime;
            StaggerTimeCount.Remove(character);
            StaggerTimeCount.Add(character, staggerTime);
        }
    }
}