using Bindito.Unity;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Serialization;
using TimberApi.DependencyContainerSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Clusters;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.Meshy;
using Timberborn.MeshyAnimations;
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

        private static Dictionary<PlaceableBlockObject, PlaceableBlockObject> _previewCache = new Dictionary<PlaceableBlockObject, PlaceableBlockObject>();


        /// <summary>
        /// Tracks when F is pressed and toggles _flipState when it is pressed
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(BlockObjectTool), nameof(BlockObjectTool.ProcessInput))]
        static void Prefix(BlockObjectTool __instance)
        {
            if (__instance._inputService.IsKeyDown("MirrorBuilding"))
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
            var gameObject = __instance.GameObjectFast;

            if (_flipState && gameObject.transform.localScale.x > 0 ||
                !_flipState && gameObject.transform.localScale.x < 0)
            {
                BuildingFlipperHelpers.Flip(__instance.GameObjectFast);
                __instance.UpdateTransformedBlocks();
                __instance.UpdateTransform();                                            
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
        }

        [HarmonyPatch(typeof(ToolManager), nameof(ToolManager.ExitTool))]
        [HarmonyPostfix]
        static void ExitToolPostfix()
        {
            _flipState = false;
        }

        //[HarmonyPatch(typeof(NodeAnimationUpdater), nameof(NodeAnimationUpdater.UpdateTransform))]
        //public static class AnimationPatch4
        //{
        //    [HarmonyPostfix]
        //    public static bool Prefix(NodeAnimationUpdater __instance, int fromFrame, int toFrame, float weight)
        //    {
        //        var building = __instance.GetComponentInParent<Building>();
        //        if (!(building?.name?.Contains("WaterPump.Folktails") ?? false) &&
        //            !(building?.name?.Contains("Gristmill") ?? false) &&
        //            !(building?.name?.Contains("DeepWaterPump.IronTeeth") ?? false))
        //        {
        //            return true;
        //        }

        //        var mirrorMono = __instance.GetComponentInParent<MirrorBuildingMonobehaviour>();
        //        if (mirrorMono == null)
        //        {
        //            return true;
        //        }

        //        if (!mirrorMono.IsFlipped)
        //        {
        //            return true;
        //        }

        //        var blockObject = __instance.GetComponentInParent<BlockObject>();
        //        var size = blockObject._blocksSpecification.Size;

        //        if (__instance._currentAnimation.HasDifferentScales)
        //        {
        //            __instance._selfTransform.localScale = __instance.GetScale(fromFrame, toFrame, weight);
        //        }

        //        var pos = __instance.GetPosition(fromFrame, toFrame, weight);
        //        __instance._selfTransform.localPosition = new Vector3(size.x - pos.x,
        //                                                              pos.y,
        //                                                              pos.z);
        //        var rota = __instance.GetRotation(fromFrame, toFrame, weight);
        //        __instance._selfTransform.localRotation = Quaternion.Euler(rota.eulerAngles.x,
        //                                                                   rota.eulerAngles.y * -1,
        //                                                                   rota.eulerAngles.z);

        //        return false;
        //    }
        //}

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
