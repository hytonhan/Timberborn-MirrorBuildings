using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace Hytone.Timberborn.MirrorBuildings
{
    [BepInPlugin("hytone.plugins.mirrorbuildings", "MirroredBuildings", "2.0.0")]
    [HarmonyPatch]
    public class MirroredBuildingsPlugin : BaseUnityPlugin, IModEntrypoint
    {
        internal static ManualLogSource Log;

        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            new Harmony("hytone.plugins.buildingtest").PatchAll();
            consoleWriter.LogInfo("Loaded MirroredBuildings.");
        }
    }
}
