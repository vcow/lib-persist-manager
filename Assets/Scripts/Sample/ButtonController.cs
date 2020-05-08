using System;
using Base.Activatable;
using Base.PersistManager.TutorialExtensions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample
{
	[DisallowMultipleComponent]
	public class ButtonController : MonoBehaviour, ITutorialPage
	{
#pragma warning disable 649
		[Inject] private readonly DiContainer _container;
		[Inject] private readonly ITutorialManager _tutorialManager;
#pragma warning restore 649

		// ITutorialPage

		public string Id => "test_button_tutorial";

		public bool InstantiatePage(Transform pageContainer, string metadata, Action<GameObject> callback)
		{
			var page = _container.InstantiatePrefabResourceForComponent<ButtonTutorialPage>(
				"ButtonTutorialPage", pageContainer, new object[] {GetComponent<Button>()});
			page.ActivatableStateChangedEvent += PageOnActivatableStateChangedEvent;
			page.Activate();
			callback?.Invoke(page.gameObject);
			return true;
		}

		private void PageOnActivatableStateChangedEvent(object sender, EventArgs args)
		{
			var activatableArgs = (ActivatableStateChangedEventArgs) args;

			if (activatableArgs.CurrentState != ActivatableState.Inactive) return;

			var activatable = (IActivatable) sender;
			activatable.ActivatableStateChangedEvent -= PageOnActivatableStateChangedEvent;
			CompleteTutorialPageEvent?.Invoke(this, new CompleteTutorialPageEventArgs());
		}

		public event EventHandler<CompleteTutorialPageEventArgs> CompleteTutorialPageEvent;

		// \ITutorialPage

		private void Start()
		{
			if (_tutorialManager.SetCurrentPage(this))
			{
			}
		}
	}
}