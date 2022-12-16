using Bindito.Unity;
using HarmonyLib;
using System;
using System.Linq;
using TimberApi.DependencyContainerSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.PrefabSystem;
using Timberborn.PreviewSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hytone.Timberborn.MirrorBuildings
{
    [HarmonyPatch]
    public static class Patches
    {
        public static bool _flipState = false;

        private static MeshFilter _prefabMeshFilter;

        /// <summary>
        /// Tracks when F is pressed and toggles _flipState when it is pressed
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(BlockObjectTool), nameof(BlockObjectTool.ProcessInput))]
        static void Prefix(BlockObjectTool __instance)
        {

            if (__instance._inputService._keyboard.IsKeyDown(Key.F))
            {
                _flipState = !_flipState;
            }
        }

        /// <summary>
        /// Flips the building if necessary.
        /// Reposition is called every frame when placing a building.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(BlockObject), nameof(BlockObject.Reposition))]
        static void Prefix(BlockObject __instance)
        {
            var model = __instance.gameObject.GetComponent<BuildingModel>();
            if (model == null)
            {
                return;
            }
            var meshFilter = model.FinishedModel.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                return;
            }
            var mesh = meshFilter.mesh;
            if (_prefabMeshFilter == null)
            {
                _prefabMeshFilter = GetPrefabMeshFilter(__instance.gameObject);
            }

            var gameobjectFirstVert = MathF.Round(mesh.vertices.First().x, 4);
            var prefabFirstVert = MathF.Round(_prefabMeshFilter.mesh.vertices.First().x, 4);
            var gameobjectLastVert = MathF.Round(mesh.vertices.Last().x, 4);
            var prefabLasttVert = MathF.Round(_prefabMeshFilter.mesh.vertices.Last().x, 4);

            if ((_flipState == true && gameobjectFirstVert == prefabFirstVert && gameobjectLastVert == prefabLasttVert) ||
                (_flipState == false && (gameobjectFirstVert != prefabFirstVert || gameobjectLastVert != prefabLasttVert)))
            {
                __instance._transformChangeNotifier.NotifyPreChangeListeners();
                BuildingFlipperHelpers.Flip(__instance.gameObject);
                __instance.UpdateTransformedBlocks();
                __instance.UpdateTransform();
                __instance._transformChangeNotifier.NotifyPostChangeListeners();
            }
        }

        /// <summary>
        /// This is called when a building is placed.
        /// This way we can actually save the FlipState
        /// </summary>
        /// <param name="__result"></param>
        [HarmonyPatch(typeof(BlockObjectFactory), nameof(BlockObjectFactory.Create), new Type[]
        {
                typeof(BlockObject),
                typeof(Vector3Int),
                typeof(Orientation)
        })]
        [HarmonyPostfix]
        static void BlockObjectFactoryCreatePostfix(GameObject __result)
        {
            var component = __result.GetComponent<MirrorBuildingMonobehaviour>();
            if (component == null)
            {
                return;
            }
            component.IsFlipped = _flipState;

            // Reset flip state after building is places
            _flipState = false;
            _prefabMeshFilter = null;
        }

        [HarmonyPatch(typeof(ToolManager), nameof(ToolManager.ExitTool))]
        [HarmonyPostfix]
        static void ExitToolPostfix()
        {
            _flipState = false;
            _prefabMeshFilter = null;
        }

        /// <summary>
        /// Helper method to get a gameobject's MeshFilter
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private static MeshFilter GetPrefabMeshFilter(GameObject gameObject)
        {
            var prefabNameRetriever = DependencyContainer.GetInstance<PrefabNameRetriever>();
            var prefabname = prefabNameRetriever.GetPrefabName(gameObject);
            var prefabMapper = DependencyContainer.GetInstance<PrefabNameMapper>();
            var prefab = prefabMapper.GetPrefab(prefabname);

            var previewFactory = DependencyContainer.GetInstance<PreviewFactory>();

            var preview = previewFactory.Create(prefab);

            var prefabModel = preview.GetComponent<BuildingModel>();
            var prefabMesh = prefabModel.FinishedModel.GetComponent<MeshFilter>();

            return prefabMesh;
        }

        [HarmonyPatch(typeof(EntityService), "Instantiate", typeof(GameObject), typeof(Guid))]
        class MinWindStrengthPatch
        {
            public static void Postfix(GameObject __result)
            {
                if (__result.GetComponent<Building>() != null &&
                    !__result.name.Contains("Path"))
                {
                    var instantiator = DependencyContainer.GetInstance<IInstantiator>();
                    instantiator.AddComponent<MirrorBuildingMonobehaviour>(__result);
                }
            }
        }
    }
}
