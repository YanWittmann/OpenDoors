
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using OWML.Utils;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace ModTemplate
{
    public class ModTemplate : ModBehaviour
    {

        private bool ready = false;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            ModHelper.Console.WriteLine($"Loaded Open Doors mod", MessageType.Success);
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem || ready) return;
                createDoorObjectNames();
                ModHelper.Console.WriteLine($"Open Doors mod is now ready", MessageType.Success);
                ready = true;
            };
        }

        private void openDoorsCloseToPlayer()
        {
            //ModHelper.Console.WriteLine("Opening doors close to player...", MessageType.Info);

            var playerBody = FindObjectOfType<PlayerBody>();
            ModHelper.Console.WriteLine($"Found player body, and it's called {playerBody.name}!",
                MessageType.Success);

            var numObjects = 0;
            var saveString = "";
            foreach (var obj in FindObjectsOfType<GameObject>())
            {
                var objectPos = obj.transform.position;
                var playerPos = playerBody.transform.position;
                var distance = Vector3.Distance(objectPos, playerPos);
                var objPosMag = objectPos.magnitude;

                if (objPosMag != 0 && distance <= 4 && distance != 0)
                {
                    numObjects++;
                    saveString += $"{obj.name} with distance {distance.ToString(CultureInfo.InvariantCulture)} at {objectPos.x.ToString(CultureInfo.InvariantCulture)}, {objectPos.y.ToString(CultureInfo.InvariantCulture)}, {objectPos.z.ToString(CultureInfo.InvariantCulture)}\n";
                    foreach (var (key, value) in removeDoorObjects)
                    {
                        if (obj.name.Equals(key))
                        {
                            openDoorByRemovingObject(obj);
                            ModHelper.Console.WriteLine($"Removed {obj.name}", MessageType.Success);
                        }
                    }
                }
            }
            ModHelper.Console.WriteLine($"Found {numObjects} objects close to player", MessageType.Info);

            File.WriteAllText("objectCounts.txt", saveString);
            ModHelper.Console.WriteLine($"Saved objects to file", MessageType.Info);
        }

        private string getClipboardText()
        {
            return GUIUtility.systemCopyBuffer;
        }

        private void openDoorByRemovingObject(GameObject obj)
        {
            ModHelper.Console.WriteLine($"Removing object {obj.name}", MessageType.Info);
            Destroy(obj);
        }

        Dictionary<string, string> removeDoorObjects = new Dictionary<string, string>();
        void createDoorObjectNames()
        {
            removeDoorObjects.Add("Structure_NOM_RotatingDoor_Broken_Panels", "southern observatory entrance door");
            removeDoorObjects.Add("PointLight_NOM_OrbSmall", "general door orb");
        }

        public void Update()
        {
            if (!ready) return;
            if (Keyboard.current[Key.O].wasPressedThisFrame)
            {
                openDoorsCloseToPlayer();
            }
            if (Keyboard.current[Key.I].wasPressedThisFrame)
            {
                var clipboardText = getClipboardText();
                if (clipboardText.Length > 0)
                {
                    ModHelper.Console.WriteLine($"Adding {clipboardText} to removeDoorObjects", MessageType.Info);
                    removeDoorObjects.Add(clipboardText, clipboardText + " (from clipboard)");
                }
            }
        }

    }
}
