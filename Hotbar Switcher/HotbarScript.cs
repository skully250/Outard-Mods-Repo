using System.Collections.Generic;
using UnityEngine;

namespace Hotbar_Switcher
{
    public class HotbarScript : MonoBehaviour
    {
        public static HotbarSwitcher mod;

        public void Initialise()
        {
            Patch();
        }

        public Item[,] savedQuickSlot = new Item[2,8];
        public int quickSlotAmount = 8;

        public void Patch()
        {
            On.Character.ctor += new On.Character.hook_ctor(SetVariables);
            On.Character.Update += new On.Character.hook_Update(updateQuickSlots);
        }

        public void SetVariables(On.Character.orig_ctor original, Character instance)
        {
            original(instance);
        }

        void loadQuickslots(CharacterQuickSlotManager quickslot, int hotbar)
        {
            int quickSlotItem = quickslot.GetQuickSlot(0).ItemID;
            for (int i = 0; i < quickSlotAmount; i++)
            {
                savedQuickSlot[hotbar, i] = quickslot.GetQuickSlot(i).ActiveItem;
                quickslot.SetQuickSlot(i, null, true);
            }
            for (int i = 0; i < quickSlotAmount; i++)
            {
                quickslot.SetQuickSlot(i, savedQuickSlot[(hotbar + 1) % 2, i], true);
            }
        }

        float timer = 3f;
        public void updateQuickSlots(On.Character.orig_Update original, Character instance)
        {
            original(instance);
            timer -= Time.deltaTime;
            CharacterQuickSlotManager quickslot = instance.QuickSlotMngr;

            if (timer <= 0)
            {
                if (Input.GetKeyUp(KeyCode.LeftBracket))
                {
                    loadQuickslots(quickslot, 0);
                    timer = 3f;
                }
                if (Input.GetKeyUp(KeyCode.RightBracket))
                {
                    loadQuickslots(quickslot, 1);
                    timer = 3f;
                }
            }
        }
    }
}
