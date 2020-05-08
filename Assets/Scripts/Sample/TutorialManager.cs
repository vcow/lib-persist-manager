using System;
using Base.PersistManager;
using Base.PersistManager.TutorialExtensions;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Sample
{
	public class TutorialManager : TutorialManagerBase
	{
		private TutorialLocker _locker;
		private static Transform _pageContentContainer;

#pragma warning disable 649
		[Inject] private readonly DiContainer _container;
		[Inject] private readonly IPersistManager _persistentManager;
#pragma warning restore 649

		[Inject]
		// ReSharper disable once UnusedMember.Local
		private void Construct()
		{
			if (_pageContentContainer == null)
			{
				_pageContentContainer = new GameObject("TutorialManager").transform;
				Object.DontDestroyOnLoad(_pageContentContainer);
			}
			else
			{
				Debug.LogError("Tutorial Manager must be injected as Singleton.");
			}
		}

		protected override void RestoreCompleteTutorialPagesData(Action<CompletedTutorialPagesData> callback)
		{
			callback?.Invoke(_persistentManager.GetPersistentValue<CompletedTutorialPagesData>());
		}

		protected override void PersistCompleteTutorialPagesData(CompletedTutorialPagesData data, Action<bool> readyCallback)
		{
			_persistentManager.Persist(data, true);
			readyCallback?.Invoke(true);
		}

		protected override bool InstantiateCurrentPage()
		{
			if (CurrentPage.InstantiatePage(_pageContentContainer, null))
			{
				CreateLocker();
				return true;
			}

			return false;
		}

		private void CreateLocker()
		{
			if (_locker != null) DestroyLocker();

			_locker = _container.InstantiateComponentOnNewGameObject<TutorialLocker>("TutorialLocker");
			_locker.transform.SetParent(_pageContentContainer, false);
		}

		private void DestroyLocker()
		{
			if (_locker == null) return;

			Object.Destroy(_locker.gameObject);
			_locker = null;
		}

		protected override void OnCompletePage(ITutorialPage page, CompleteTutorialPageEventArgs args)
		{
			base.OnCompletePage(page, args);

			if (args.CloseTutorialPage)
			{
				foreach (Transform child in _pageContentContainer)
				{
					Object.Destroy(child.gameObject);
				}

				DestroyLocker();
			}
		}
	}
}