using Partiality.Modloader;
using System;
using System.IO;
using UnityEngine;

namespace Hotbar_Switcher
{
    public class HotbarSwitcher : PartialityMod
    {

        public HotbarSwitcher()
        {
            this.ModID = "Hotbar Switcher";
            this.Version = "0100";
            this.author = "Faedar";
        }

        public static HotbarScript hotbarScript;

        public override void OnEnable()
        {
            base.OnEnable();
            HotbarScript.mod = this;
            GameObject obj = new GameObject();
            hotbarScript = obj.AddComponent<HotbarScript>();
            hotbarScript.Initialise();
        }

        public void loadHotbars()
        {
            //Start fileReader and read hotbar data
            try
            {
                using (StreamReader hotbars = new StreamReader("cfg/HotbarSaves.ini"))
                {
                    try
                    {
                        //some code
                    } catch (ArgumentNullException)
                    {

                    }
                }
            } catch (FileNotFoundException)
            {
                Debug.Log("Hotbars: File not found defaulting");
                //Add code for resetting
            }
        }

    }
}