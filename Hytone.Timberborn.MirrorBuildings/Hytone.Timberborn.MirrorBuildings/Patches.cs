using Bindito.Unity;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Clusters;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.Meshy;
using Timberborn.PrefabSystem;
using Timberborn.PreviewSystem;
using Timberborn.TemplateSystem;
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

        private static Dictionary<PlaceableBlockObject, PlaceableBlockObject> _previewCache = new Dictionary<PlaceableBlockObject, PlaceableBlockObject>();


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

            var model = __instance.GameObjectFast.GetComponent<BuildingModel>();
            if (model == null)
            {
                return;
            }
            var meshFilter = model.FinishedModel.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null)
            {
                return;
            }
            var mesh = meshFilter.mesh;
            if (_prefabMeshFilter == null)
            {
                _prefabMeshFilter = GetPrefabMeshFilter(__instance);
            }

            var gameobjectFirstVert = MathF.Round(mesh.vertices.First().x, 4);
            var prefabFirstVert = MathF.Round(_prefabMeshFilter.mesh.vertices.First().x, 4);
            var gameobjectLastVert = MathF.Round(mesh.vertices.Last().x, 4);
            var prefabLasttVert = MathF.Round(_prefabMeshFilter.mesh.vertices.Last().x, 4);

            if ((_flipState == true && gameobjectFirstVert == prefabFirstVert && gameobjectLastVert == prefabLasttVert) ||
                (_flipState == false && (gameobjectFirstVert != prefabFirstVert || gameobjectLastVert != prefabLasttVert)))
            {
                //__instance._transformChangeNotifier.NotifyPreChangeListeners();
                BuildingFlipperHelpers.Flip(__instance.GameObjectFast);
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
        static void BlockObjectFactoryCreatePostfix(BlockObject __result)
        {
            var component = __result.GetComponentFast<MirrorBuildingMonobehaviour>();
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
        /// <param name="baseComponent"></param>
        /// <returns></returns>
        private static MeshFilter GetPrefabMeshFilter(BaseComponent baseComponent)
        {
            var prefabNameRetriever = DependencyContainer.GetInstance<PrefabNameRetriever>();
            var prefabname = prefabNameRetriever.GetPrefabName(baseComponent);
            var prefabMapper = DependencyContainer.GetInstance<PrefabNameMapper>();
            var prefab = prefabMapper.GetPrefab(prefabname);

            TemplateInstantiator templateInstantiator = DependencyContainer.GetInstance<TemplateInstantiator>();
            var previewFactory = DependencyContainer.GetInstance<PreviewFactory>();

            CachedTemplate cachedTemplate = templateInstantiator.GetCachedTemplate(prefab.GameObjectFast);
            var gameObject = templateInstantiator._baseInstantiator.InstantiateInactive(cachedTemplate.Prefab, previewFactory.transform);

            TemplateInstantiator.GetComponentContainers(gameObject, templateInstantiator._temporaryContainerCache);
            for (int i = 0; i < templateInstantiator._temporaryContainerCache.Count; i++)
            {
                templateInstantiator._temporaryContainerCache[i].GetComponents(templateInstantiator._temporaryComponentCache);
                ImmutableArray<CachedTemplateInitializer> initializers = cachedTemplate.Initializers;
                for (int j = 0; j < initializers.Length; j++)
                {
                    CachedTemplateInitializer cachedTemplateInitializer = initializers[j];
                    if (cachedTemplateInitializer.ContainerIndex == i)
                    {
                        cachedTemplateInitializer.Method(templateInstantiator._temporaryComponentCache[cachedTemplateInitializer.SubjectIndex], templateInstantiator._temporaryComponentCache[cachedTemplateInitializer.DecoratorIndex]);
                    }
                }
                templateInstantiator._temporaryComponentCache.Clear();
            }
            templateInstantiator._temporaryContainerCache.Clear();

            var meshyDescs = gameObject.GetComponentsInChildren<MeshyDescription>();
            var meshFilter = meshyDescs.First()
                                       .GetComponentInChildren<MeshFilter>();
            return meshFilter;
        }

        [HarmonyPatch(typeof(EntityService), "Instantiate", typeof(BaseComponent), typeof(Guid))]
        class MinWindStrengthPatch
        {
            public static void Postfix(BaseComponent __result)
            {
                if (__result.GetComponentFast<Building>() != null &&
                    !__result.name.Contains("Path"))
                {
                    var instantiator = DependencyContainer.GetInstance<BaseInstantiator>();
                    instantiator.AddComponent<MirrorBuildingMonobehaviour>(__result.GameObjectFast);
                }
            }
        }
    }
}
