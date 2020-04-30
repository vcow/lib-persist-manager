using Base.PersistManager;
using Base.PersistManager.TutorialExtensions;
using Zenject;

namespace Sample
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IPersistManager>().FromComponentInNewPrefabResource(@"PersistManager").AsSingle();
            Container.Bind<ITutorialManager>().To<TutorialManager>().AsSingle();
        }
    }
}
