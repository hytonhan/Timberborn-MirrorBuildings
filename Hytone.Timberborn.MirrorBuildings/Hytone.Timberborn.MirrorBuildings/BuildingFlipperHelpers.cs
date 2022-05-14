using System;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Buildings;
using Timberborn.SlotSystem;
using Timberborn.Warehouses;
using Timberborn.Workshops;
using TimberbornAPI;
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
            var mesh = model.FinishedModel.GetComponent<MeshFilter>().mesh;
            var blockObject = model.GetComponent<BlockObject>();
            var size = blockObject.BlocksSpecification.Size;
            var hasEntrance = blockObject.Entrance.HasEntrance;
            Vector3Int entranceCoords = new Vector3Int();
            if (hasEntrance)
            {
                entranceCoords = blockObject.Entrance.Coordinates;
            }

            if (mesh == null) return;
            FlipMesh(mesh, size);
            if (flipX) FlipNormals(mesh);

            FlipSomeRandomAnimators(gameObject, size);
            FlipBlocks(blockObject, size);
            FlipWaterStuff(gameObject, size);
            FlipEntrance(gameObject, blockObject, size, hasEntrance, entranceCoords);
        }

        /// <summary>
        /// Some high tech bs so flipping a mesh works?
        /// </summary>
        /// <param name="mesh"></param>
        public static void FlipNormals(Mesh mesh)
        {
            int[] tris = mesh.triangles;
            for (int i = 0; i < tris.Length / 3; i++)
            {
                int a = tris[i * 3 + 0];
                int b = tris[i * 3 + 1];
                int c = tris[i * 3 + 2];
                tris[i * 3 + 0] = c;
                tris[i * 3 + 1] = b;
                tris[i * 3 + 2] = a;
            }
            mesh.triangles = tris;
        }

        /// <summary>
        /// Flips the mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="size"></param>
        private static void FlipMesh(Mesh mesh, Vector3Int size)
        {
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 c = verts[i];
                if (flipX) c.x = (c.x * -1) + size.x;
                verts[i] = c;
            }
            mesh.vertices = verts;
        }

        /// <summary>
        /// Some buildings require the animators to be flipped for them to look good
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="size"></param>
        private static void FlipSomeRandomAnimators(GameObject gameObject, Vector3Int size)
        {
            var animator = gameObject.GetComponentInChildren<Animator>();
            if (animator != null &&
                (animator.name.Contains("WaterPump.Folktails") || animator.name.Contains("Gristmill") || animator.name.Contains("PaperMill")))
            {
                animator.transform.localScale = new Vector3(animator.transform.localScale.x * -1,
                                                              animator.transform.localScale.y,
                                                              animator.transform.localScale.z);
                animator.transform.localPosition = new Vector3(size.x - animator.transform.localPosition.x,
                                                               animator.transform.localPosition.y,
                                                               animator.transform.localPosition.z);
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
        private static void FlipEntrance(GameObject gameObject, BlockObject blockObject, Vector3Int size, bool hasEntrance, Vector3Int entranceCoords)
        {
            if (hasEntrance)
            {
                //Some buildings have a BlockObjectNavMeshSettings component which needs the same
                // modification as Entrance. 
                var settingComponent = gameObject.GetComponent<BlockObjectNavMeshSettings>();
                if (settingComponent != null)
                {
                    for (int i = 0; i < settingComponent.AddedEdges.Length; i++)
                    {
                        if (settingComponent.AddedEdges[i].End == blockObject.Entrance.Coordinates &&
                           settingComponent.AddedEdges[i].Start == (blockObject.Entrance.Coordinates - new Vector3Int(0, -1, 0)))
                        {
                            settingComponent.AddedEdges[i].Start = new Vector3Int(size.x - 1 - entranceCoords.x,
                                                                                  settingComponent.AddedEdges[i].Start.y,
                                                                                  settingComponent.AddedEdges[i].Start.z);
                            settingComponent.AddedEdges[i].End = new Vector3Int(size.x - 1 - entranceCoords.x,
                                                                                settingComponent.AddedEdges[i].End.y,
                                                                                settingComponent.AddedEdges[i].End.z);
                        }
                    }
                }

                //This moves the actual entrance which beavers use to enter
                blockObject.Entrance.Coordinates = new Vector3Int(size.x - 1 - entranceCoords.x,
                                                                  entranceCoords.y,
                                                                  entranceCoords.z);

                //This moves the transform component which dictates where beavers sit when idle on workplace
                var transformSlotInit = blockObject.gameObject.GetComponent<TransformSlotInitializer>();
                if (transformSlotInit != null)
                {
                    transformSlotInit.SlotSpecifications.Count();

                    foreach (var item in transformSlotInit.SlotSpecifications)
                    {
                        var slotRetriever = TimberAPI.DependencyContainer.GetInstance<SlotRetriever>();
                        var slots = slotRetriever.GetSlots(blockObject.gameObject, item.SlotKeyword);

                        foreach (Transform transform in slots)
                        {
                            transform.localPosition = new Vector3(size.x - transform.localPosition.x,
                                                                  transform.localPosition.y,
                                                                  transform.localPosition.z);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flicks the blockspecifications so textures and occupations match
        /// </summary>
        /// <param name="blockObject"></param>
        /// <param name="size"></param>
        private static void FlipBlocks(BlockObject blockObject, Vector3Int size)
        {

            for (int i = 0; i < blockObject.BlocksSpecification.Size.y * blockObject.BlocksSpecification.Size.z; i++)
            {
                var counter = Math.Floor((float)blockObject.BlocksSpecification.Size.x / 2);

                for (int j = 0; j < (int)counter; j++)
                {
                    int firstIndex = i * size.x + j;
                    int secondIndex = (i + 1) * size.x - j - 1;
                    var temp = blockObject.BlocksSpecification.BlockSpecifications[firstIndex];
                    blockObject.BlocksSpecification.BlockSpecifications[firstIndex] = blockObject.BlocksSpecification.BlockSpecifications[secondIndex];
                    blockObject.BlocksSpecification.BlockSpecifications[secondIndex] = temp;
                }
            }
            blockObject._blocks = Blocks.From(blockObject.BlocksSpecification);
        }

        /// <summary>
        /// Flips water related stuff on buildings
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="size"></param>
        private static void FlipWaterStuff(GameObject gameObject, Vector3Int size)
        {
            if (gameObject.TryGetComponent<WaterManufactory>(out var waterManufactory))
            {
                waterManufactory.WaterCoordinates = new Vector3Int(size.x - 1 - waterManufactory.WaterCoordinates.x,
                                                                   waterManufactory.WaterCoordinates.y,
                                                                   waterManufactory.WaterCoordinates.z);
            }

            if (gameObject.TryGetComponent<WaterTankWaterLevel>(out var waterlevel))
            {
                waterlevel.WaterPlane.localPosition = new Vector3(size.x - waterlevel.WaterPlane.localPosition.x,
                                                                  waterlevel.WaterPlane.localPosition.y,
                                                                  waterlevel.WaterPlane.localPosition.z);
            }
        }
    }
}
