using System;
using System.Collections.Immutable;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Buildings;
using Timberborn.Clusters;
using Timberborn.Coordinates;
using Timberborn.WaterWorkshops;
using UnityEngine;

namespace Hytone.Timberborn.MirrorBuildings
{
    /// <summary>
    /// Methods to flip a mesh around.
    /// source: https://stackoverflow.com/questions/51100346/flipping-3d-gameobjects-in-unity3d
    /// </summary>
    public static class BuildingFlipperHelpers
    {
        public static bool flipX = true;

        /// <summary>
        /// Flips a mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="size"></param>
        /// <param name="hasEntrance"></param>
        /// <param name="entranceCoords"></param>
        /// <param name="blockObject"></param>
        public static void Flip(GameObject gameObject)
        {
            var model = gameObject.GetComponent<BuildingModel>();
            var blockObject = model.GetComponentFast<BlockObject>();
            var size = blockObject._blocksSpecification.Size;
            var hasEntrance = blockObject.Entrance.HasEntrance;
            Vector3Int entranceCoords = new Vector3Int();
            if (hasEntrance)
            {
                entranceCoords = blockObject.Entrance.Coordinates;
            }

            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,
                                                          gameObject.transform.localScale.y,
                                                          gameObject.transform.localScale.z);

            model.FinishedModel.transform.localPosition = new Vector3(-size.x - model.FinishedModel.transform.localPosition.x,
                                                                      model.FinishedModel.transform.localPosition.y,
                                                                      model.FinishedModel.transform.localPosition.z);
            if (model.UnfinishedModel != null)
            {
                model.UnfinishedModel.transform.localPosition = new Vector3(-size.x - model.UnfinishedModel.transform.localPosition.x,
                                                                            model.UnfinishedModel.transform.localPosition.y,
                                                                            model.UnfinishedModel.transform.localPosition.z);
            }
            FlipBlocks(blockObject, size);
            FlipWaterStuff(gameObject, size);
            FlipEntrance(gameObject, blockObject, size, hasEntrance, entranceCoords);
            FlipSomeRandomPositions(gameObject, size);
            FlipPowerConnections(gameObject, size);
        }

        /// <summary>
        /// Metal Platforms need to have their local position adjusted
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="meshfilter"></param>
        /// <param name="size"></param>
        private static void FlipSomeRandomPositions(GameObject gameObject, Vector3Int size)
        {
            if (gameObject.name.Contains("MetalPlatform"))
            {
                var model = gameObject.GetComponent<BuildingModel>();
                model.FinishedModel.transform.localPosition = new Vector3(model.FinishedModel.transform.localPosition.x + size.x -1,
                                                                          model.FinishedModel.transform.localPosition.y,
                                                                          model.FinishedModel.transform.localPosition.z);
            }
            if (gameObject.name.Contains("Carousel"))
            {
                var model = gameObject.GetComponent<BuildingModel>();
                model.FinishedModel.transform.localPosition = new Vector3(model.FinishedModel.transform.localPosition.x + size.x  -3 ,
                                                                          model.FinishedModel.transform.localPosition.y,
                                                                          model.FinishedModel.transform.localPosition.z);
            }
        }

        /// <summary>
        /// Flops the entrance
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="blockObject"></param>
        /// <param name="size"></param>
        /// <param name="hasEntrance"></param>
        /// <param name="entranceCoords"></param>
        private static void FlipEntrance(GameObject gameObject,
                                         BlockObject blockObject,
                                         Vector3Int size,
                                         bool hasEntrance,
                                         Vector3Int entranceCoords)
        {
            if (hasEntrance)
            {
                //Some buildings have a BlockObjectNavMeshSettings component which needs the same
                // modification as Entrance. 
                var settingComponent = gameObject.GetComponent<BlockObjectNavMeshSettings>();
                if (settingComponent != null)
                {
                    for (int i = 0; i < settingComponent._addedEdges.Length; i++)
                    {
                        if (settingComponent._addedEdges[i].End == blockObject.Entrance.Coordinates &&
                           settingComponent._addedEdges[i].Start == (blockObject.Entrance.Coordinates - new Vector3Int(0, -1, 0)))
                        {
                            settingComponent._addedEdges[i]._start = new Vector3Int(size.x - 1 - entranceCoords.x,
                                                                                  settingComponent._addedEdges[i].Start.y,
                                                                                  settingComponent._addedEdges[i].Start.z);
                            settingComponent._addedEdges[i]._end = new Vector3Int(size.x - 1 - entranceCoords.x,
                                                                                settingComponent._addedEdges[i].End.y,
                                                                                settingComponent._addedEdges[i].End.z);
                        }
                    }
                }

                //This moves the actual entrance which beavers use to enter
                blockObject._entrance._coordinates = new Vector3Int(size.x - 1 - entranceCoords.x,
                                                                    entranceCoords.y,
                                                                    entranceCoords.z);
            }
        }

        /// <summary>
        /// Flicks the blockspecifications so textures and occupations match
        /// </summary>
        /// <param name="blockObject"></param>
        /// <param name="size"></param>
        private static void FlipBlocks(BlockObject blockObject, Vector3Int size)
        {
            for (int i = 0; i < blockObject._blocksSpecification.Size.y * blockObject._blocksSpecification.Size.z; i++)
            {
                var counter = Math.Floor((float)blockObject._blocksSpecification.Size.x / 2);

                for (int j = 0; j < (int)counter; j++)
                {
                    int firstIndex = i * size.x + j;
                    int secondIndex = (i + 1) * size.x - j - 1;
                    var temp = blockObject._blocksSpecification.BlockSpecifications[firstIndex];
                    blockObject._blocksSpecification.BlockSpecifications[firstIndex] = blockObject._blocksSpecification.BlockSpecifications[secondIndex];
                    blockObject._blocksSpecification.BlockSpecifications[secondIndex] = temp;
                }
            }
            blockObject._blocks = Blocks.From(blockObject._blocksSpecification);
        }

        /// <summary>
        /// Flips water related stuff on buildings
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="size"></param>
        private static void FlipWaterStuff(GameObject gameObject, Vector3Int size)
        {
            if (gameObject.TryGetComponent<WaterInputManufactoryLimiter>(out var waterManufactory))
            {
                waterManufactory._waterInput._waterCoordinates = new Vector3Int(size.x - 1 - waterManufactory._waterInput._waterCoordinates.x,
                                                                                waterManufactory._waterInput._waterCoordinates.y,
                                                                                waterManufactory._waterInput._waterCoordinates.z);
            }
            if (gameObject.TryGetComponent<WaterInputContaminationManufactoryLimiter>(out var contaminationManufactory))
            {
                contaminationManufactory._waterInput._waterCoordinates = new Vector3Int(size.x - 1 - contaminationManufactory._waterInput._waterCoordinates.x,
                                                                                contaminationManufactory._waterInput._waterCoordinates.y,
                                                                                contaminationManufactory._waterInput._waterCoordinates.z);
            }
        }

        private static void FlipPowerConnections(GameObject gameObject, Vector3Int size)
        {
            var mask = Directions3D.Left | Directions3D.Right;

            if (gameObject.TryGetComponent<ClusterElement>(out var clusterElement))
            {
                var blocks = clusterElement._clusterElementSpecification._connectableBlocks;
                for (int i = 0; i < blocks.Count(); i++)
                {
                    var spec = blocks[i];
                    spec._coordinates = new Vector3Int(size.x - 1 - spec._coordinates.x,
                                                       spec._coordinates.y,
                                                       spec._coordinates.z);
                    if ((spec._connectableDirections & mask) == mask ||
                        (spec._connectableDirections & mask) == Directions3D.None)
                    {
                        continue;
                    }
                    else
                    {
                        spec._connectableDirections = spec._connectableDirections ^ mask;
                    }

                    blocks[i] = spec;
                }
                clusterElement._clusterElementSpecification._connectableBlocks = blocks;
                clusterElement._clusterElementSpecification._transputSpecifications = clusterElement._clusterElementSpecification.CreateTransputSpecifications().ToImmutableArray();
            }
        }
    }
}
