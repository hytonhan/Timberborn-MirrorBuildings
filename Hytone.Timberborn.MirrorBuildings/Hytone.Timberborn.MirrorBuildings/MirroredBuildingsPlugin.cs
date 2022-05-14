using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberbornAPI;
using TimberbornAPI.Common;

namespace Hytone.Timberborn.MirrorBuildings
{
    [BepInPlugin("hytone.plugins.mirrorbuildings", "MirroredBuildings", "1.1.0")]
    [BepInDependency("com.timberapi.timberapi")]
    [HarmonyPatch]
    public class MirroredBuildingsPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;
            // Harmony patches
            new Harmony("hytone.plugins.buildingtest").PatchAll();

            TimberAPI.DependencyRegistry.AddConfigurator(new MirrorBuildingConfigurator(), SceneEntryPoint.InGame);
            Log.LogInfo("Loaded MirroredBuildings.");
        }
    }
}
