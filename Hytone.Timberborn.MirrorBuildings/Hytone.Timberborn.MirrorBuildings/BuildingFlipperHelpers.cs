using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Buildings;
using Timberborn.SlotSystem;
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
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 c = verts[i];
                if (flipX) c.x = (c.x * -1) + size.x;
                verts[i] = c;
            }

            mesh.vertices = verts;
            if (flipX) FlipNormals(mesh);

            if (hasEntrance)
            {
                //Some buildings have a BlockObjectNavMeshSettings component which needs the same
                // modification as Entrance. 
                var settingComponent = gameObject.GetComponent<BlockObjectNavMeshSettings>();
                if (settingComponent != null)
                {
                    for (int i = 0; i < settingComponent.AddedEdges.Length; i++)
                    {
                        if(settingComponent.AddedEdges[i].End == blockObject.Entrance.Coordinates &&
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
    }
}
