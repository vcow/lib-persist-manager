using System;
using Base.GameService;
using Base.GameTask;
using Base.PersistManager;
using Base.PersistManager.TutorialExtensions;
using UnityEngine.Assertions;
using Zenject;

namespace Sample
{
	public class SampleSceneInstaller : MonoInstaller<SampleSceneInstaller>
	{
#pragma warning disable 649
		[Inject] private readonly IPersistManager _persistentManager;
		[Inject] private readonly ITutorialManager _tutorialManager;
#pragma warning restore 649

		public override void InstallBindings()
		{
		}

		public override void Start()
		{
			var initialQueue = new GameTaskQueue();
			initialQueue.Add(new GameTaskInitService(_persistentManager));
			initialQueue.Add(new GameTaskInitService(_tutorialManager));
			initialQueue.CompleteEvent += OnInitialComplete;
			initialQueue.Start();
		}

		private void OnInitialComplete(object sender, EventArgs args)
		{
			var task = (IGameTask) sender;
			task.CompleteEvent -= OnInitialComplete;

			var input = Container.InstantiatePrefabResource("UI").GetComponent<InputController>();
			Assert.IsNotNull(input);
			Container.Inject(input);
		}
	}
}