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
        public Item[] savedItems = new Item[8];

        public void Patch()
        {
            On.Character.ctor += new On.Character.hook_ctor(SetVariables);
            On.Character.Update += new On.Character.hook_Update(updateQuickSlots);
        }

        public void SetVariables(On.Character.orig_ctor original, Character instance)
        {
            original(instance);
        }

        float timer = 5f;
        public void updateQuickSlots(On.Character.orig_Update original, Character instance)
        {
            original(instance);
            timer -= Time.deltaTime;
            CharacterQuickSlotManager quickslot = instance.QuickSlotMngr;
            //4000230 = Bird Egg
            //4000020 = Crabeye Seed
            if (timer <= 0)
            {
                if (Input.GetKeyUp(KeyCode.F10))
                {
                    //Save quickslot
                    //Debug.Log(quickslot.GetQuickSlot(0).ItemID);
                    if (quickslot.GetQuickSlot(0).ItemID == 4000230)
                    {
                        for (int i = 0; i < quickSlotAmount; i++)
                        {
                            savedQuickSlot[0, i] = quickslot.GetQuickSlot(i).ActiveItem;
                            Debug.Log("Quickslot 2 Item: " + savedQuickSlot[1, i]);
                            quickslot.SetQuickSlot(i, null, true);
                            quickslot.SetQuickSlot(i, savedQuickSlot[1, i], true);
                        }
                    }
                    if (quickslot.GetQuickSlot(0).ItemID == 4000020)
                    {
                        for (int i = 0; i < quickSlotAmount; i++)
                        {
                            savedQuickSlot[1, i] = quickslot.GetQuickSlot(i).ActiveItem;
                            Debug.Log("Quickslot 1 Item: " + savedQuickSlot[0, i]);
                            quickslot.SetQuickSlot(i, null, true);
                            quickslot.SetQuickSlot(i, savedQuickSlot[0, i], true);
                        }
                    }
                    timer = 5f;
                }
            }
        }

    }
}
