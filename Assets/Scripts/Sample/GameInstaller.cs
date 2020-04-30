using Base.PersistManager;
using Zenject;

namespace Sample
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IPersistManager>().FromComponentInNewPrefabResource(@"PersistManager").AsSingle();
        }
    }
}
