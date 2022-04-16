using OWML.Common;
using OWML.ModHelper;

using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace ModTemplate
{
    public class ModTemplate : ModBehaviour
    {
        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"Loaded Open Doors mod", MessageType.Success);

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                var playerBody = FindObjectOfType<PlayerBody>();
                ModHelper.Console.WriteLine($"Found player body, and it's called {playerBody.name}!",
                    MessageType.Success);


                // create a variable that counts the number of objects
                var numObjects = 0;
                // also create a map to count the objects of each type
                var objectCounts = new Dictionary<string, int>();
                foreach (var obj in FindObjectsOfType<GameObject>())
                {
                    numObjects++;
                    if (!objectCounts.ContainsKey(obj.name))
                    {
                        objectCounts[obj.name] = 1;
                    }
                    else
                    {
                        objectCounts[obj.name]++;
                    }
                }
                ModHelper.Console.WriteLine($"Found {numObjects} objects in the scene", MessageType.Info);

                // save the counts to the file
                var saveString = "";
                foreach (var kvp in objectCounts)
                {
                    saveString += $"{kvp.Key}: {kvp.Value}\n";
                }
                File.WriteAllText("objectCounts.txt", saveString);

            };
        }
    }
}
