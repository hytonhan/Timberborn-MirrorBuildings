using Bindito.Unity;
using Timberborn.Buildings;
using TimberbornAPI.EntityActionSystem;
using UnityEngine;

namespace Hytone.Timberborn.MirrorBuildings
{
    public class EntityActions : IEntityAction
    {
        private readonly IInstantiator _instantiator;

        public EntityActions(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        /// <summary>
        /// Add a custom mirror handling class to all buildings except Paths
        /// </summary>
        /// <param name="entity"></param>
        public void ApplyToEntity(GameObject entity)
        {
            if (entity.GetComponent<Building>() != null &&
                !entity.name.Contains("Path"))
            {
                _instantiator.AddComponent<MirrorBuildingMonobehaviour>(entity);
            }
        }
    }
}
