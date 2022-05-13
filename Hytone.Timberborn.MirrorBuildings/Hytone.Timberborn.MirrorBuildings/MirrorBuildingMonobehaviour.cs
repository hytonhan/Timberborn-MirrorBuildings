using Timberborn.BlockSystem;
using Timberborn.Persistence;
using UnityEngine;

namespace Hytone.Timberborn.MirrorBuildings
{
    /// <summary>
    /// This class handles stuff relating to flipping buildings
    /// </summary>
    public class MirrorBuildingMonobehaviour : MonoBehaviour, IPersistentEntity
    {
        //Keys used in data saving/loading
        private static readonly ComponentKey MirrorBuildingKey = new ComponentKey(nameof(MirrorBuildingMonobehaviour));
        private static readonly PropertyKey<bool> IsFlippedKey = new PropertyKey<bool>(nameof(IsFlipped));

        public bool IsFlipped { get; set; }

        /// <summary>
        /// Save the FLipped status
        /// </summary>
        /// <param name="entitySaver"></param>
        public void Save(IEntitySaver entitySaver)
        {
            IObjectSaver component = entitySaver.GetComponent(MirrorBuildingKey);
            component.Set(IsFlippedKey, IsFlipped);
        }

        /// <summary>
        /// Load the Flipped status and possible do the Flip
        /// </summary>
        /// <param name="entityLoader"></param>
        public void Load(IEntityLoader entityLoader)
        {
            IsFlipped = false;
            if (!entityLoader.HasComponent(MirrorBuildingKey))
            {
                return;
            }
            IObjectLoader component = entityLoader.GetComponent(MirrorBuildingKey);
            if (component.Has(IsFlippedKey))
            {
                IsFlipped = component.Get(IsFlippedKey);
            }

            if(IsFlipped)
            {
                DoFlip();
            }
        }

        /// <summary>
        /// Flips the building.
        /// </summary>
        private void DoFlip()
        {
            var blockObject = GetComponent<BlockObject>();
            BuildingFlipperHelpers.Flip(gameObject);
            blockObject.UpdateTransformedBlocks();
            blockObject.UpdateTransform();
        }
    }
}
