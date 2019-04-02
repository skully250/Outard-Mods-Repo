using Partiality.Modloader;
using System;
using System.IO;
using UnityEngine;

namespace MPLimitRemover
{
    public class PlayerLimitRemover : PartialityMod
    {

        public static byte PlayerLimit;

        public PlayerLimitRemover()
        {
            this.ModID = "Fae's Player Limit Remover";
            this.Version = "0100";
            this.author = "Faedar";
        }

        public static MPScriptLoad limitRemover;

        public override void OnEnable()
        {
            base.OnEnable();
            LoadSettings();
            MPScriptLoad.plm = this;
            GameObject obj = new GameObject();
            limitRemover = obj.AddComponent<MPScriptLoad>();
            limitRemover.Initialise();
        }

        public void LoadSettings()
        {
            try
            {
                using (StreamReader settings = new StreamReader("mods/FaePlayerSettings.cfg")) {
                    try
                    {
                        PlayerLimit = byte.Parse(settings.ReadLine());
                    }
                    catch (ArgumentNullException)
                    {
                        Debug.Log("Argument Null Exception");
                    }
                    catch (FormatException)
                    {
                        Debug.Log("Format Exception");
                    }
                }
            } catch (FileNotFoundException) {
                Debug.Log("File Not Found Exception");
                PlayerLimit = 4;
            } catch (IOException) {
                Debug.Log("General IO Exception");
                PlayerLimit = 4;
            }
            Debug.Log(PlayerLimit);
        }
    }
}
