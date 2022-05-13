using Bindito.Core;
using TimberbornAPI.EntityActionSystem;

namespace Hytone.Timberborn.MirrorBuildings
{
    public class MirrorBuildingConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.MultiBind<IEntityAction>().To<EntityActions>().AsSingleton();
        }
    }
}
