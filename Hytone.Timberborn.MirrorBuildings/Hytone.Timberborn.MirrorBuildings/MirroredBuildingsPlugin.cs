using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace Hytone.Timberborn.MirrorBuildings
{
    [HarmonyPatch]
    public class MirroredBuildingsPlugin : IModEntrypoint
    {
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            new Harmony("hytone.plugins.buildingtest").PatchAll();
            consoleWriter.LogInfo("Loaded MirroredBuildings.");
        }
    }
}
