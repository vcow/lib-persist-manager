using UnityEngine;
using Zenject;

namespace Sample
{
    public class SampleSceneInstaller : MonoInstaller<SampleSceneInstaller>
    {
#pragma warning disable 649
        [SerializeField] private InputController _input;
#pragma warning restore 649

        public override void InstallBindings()
        {
        }

        public override void Start()
        {
            Container.Inject(_input);
        }
    }
}
