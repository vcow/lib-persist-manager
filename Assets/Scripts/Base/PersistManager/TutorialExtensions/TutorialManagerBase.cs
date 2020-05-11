//#define DISABLE_TUTORIAL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace Base.PersistManager.TutorialExtensions
{
	[Serializable]
	public class CompletedTutorialPagesRecord : ICloneable, IEquatable<CompletedTutorialPagesRecord>
	{
		// ReSharper disable InconsistentNaming
		public string PageId;
		public bool IsFinished;
		public string Metadata;
		// ReSharper restore InconsistentNaming

		public bool Equals(CompletedTutorialPagesRecord other)
		{
			return other != null && other.PageId == PageId &&
			       other.IsFinished == IsFinished && other.Metadata == Metadata;
		}

		public void SetValue(string pageId, bool isFinished, string metadata)
		{
			PageId = pageId;
			IsFinished = isFinished;
			Metadata = metadata;
		}

		public void SetValue(CompletedTutorialPagesRecord other)
		{
			SetValue(other.PageId, other.IsFinished, other.Metadata);
		}

		public object Clone()
		{
			return new CompletedTutorialPagesRecord
			{
				PageId = PageId,
				IsFinished = IsFinished,
				Metadata = Metadata
			};
		}
	}

	[Serializable]
	public class CompletedTutorialPagesData : IPersistent<CompletedTutorialPagesData>
	{
		public string PersistentId => "completed_tutorial_pages";

		// ReSharper disable once InconsistentNaming
		public List<CompletedTutorialPagesRecord> CompletedPages = new List<CompletedTutorialPagesRecord>();

		public void Restore<T1>(T1 data) where T1 : IPersistent<CompletedTutorialPagesData>
		{
			var src = data as CompletedTutorialPagesData;
			Assert.IsNotNull(src);

			CompletedPages.Clear();
			CompletedPages.AddRange(src.CompletedPages.Select(record => record.Clone())
				.Cast<CompletedTutorialPagesRecord>());
		}
	}

	public abstract class TutorialManagerBase : ITutorialManager
	{
		private bool _isReady;
		private bool _tutorialIsActive;

		private bool _validate;
		private bool _waitForPersist;
		private readonly Mutex _mutex = new Mutex();

		protected ITutorialPage CurrentPage { get; private set; }

		protected readonly CompletedTutorialPagesData CompletedData = new CompletedTutorialPagesData();

		// ITutorialManager

		public virtual void Initialize(params object[] args)
		{
#if DISABLE_TUTORIAL
			IsReady = true;
#else
			RestoreCompleteTutorialPagesData(data =>
			{
				if (data != null)
				{
					_mutex.WaitOne();
					CompletedData.Restore(data);
					_mutex.ReleaseMutex();
				}
				else
				{
					Debug.LogError("Failed to restore complete tutorial pages data.");
				}

				IsReady = true;
			});
#endif
		}

		protected abstract void RestoreCompleteTutorialPagesData(Action<CompletedTutorialPagesData> callback);

		public bool IsReady
		{
			get => _isReady;
			protected set
			{
				if (value == _isReady) return;
				_isReady = value;
				Assert.IsTrue(_isReady);
				ReadyEvent?.Invoke(this, new ReadyEventArgs(true));
			}
		}

		public event EventHandler<ReadyEventArgs> ReadyEvent;

		public bool SetCurrentPage(ITutorialPage page)
		{
			if (!IsReady)
			{
				throw new Exception("TutorialManager must be initialized.");
			}

			Assert.IsNotNull(page);

#if DISABLE_TUTORIAL
			return false;
#else
			if (CurrentPage?.Id == page.Id) return true;

			if (GetPageState(page.Id))
			{
				// Trying to open tutorial page, but this page was already completed.
				return false;
			}

			if (CurrentPage != null)
			{
				Debug.LogErrorFormat("Current tutorial page {0} wasn't closed before the next page.",
					CurrentPage.Id);
				OnCompletePageHandler(CurrentPage, new CompleteTutorialPageEventArgs(markPageAsFinished: false));
			}

			CurrentPage = page;
			CurrentPage.CompleteTutorialPageEvent += OnCompletePageHandler;
			TutorialIsActive = true;

			var record = GetRecord(page.Id);

			return InstantiateCurrentPage(record?.Metadata ?? "");
#endif
		}

		protected abstract bool InstantiateCurrentPage(string metadata);

		public bool GetPageState(string pageId)
		{
			return GetPageState(pageId, out _);
		}

		public bool GetPageState(string pageId, out string metadata)
		{
			if (!IsReady)
			{
				throw new Exception("TutorialManager must be initialized.");
			}
#if DISABLE_TUTORIAL
			metadata = "";
			return true;
#else
			var record = GetRecord(pageId);
			if (record != null)
			{
				metadata = record.Metadata;
				return record.IsFinished;
			}

			metadata = "";
			return false;
#endif
		}

		private CompletedTutorialPagesRecord GetRecord(string pageId)
		{
			CompletedTutorialPagesRecord result = null;
			_mutex.WaitOne();
			var record = CompletedData.CompletedPages.SingleOrDefault(pagesRecord => pagesRecord.PageId == pageId);
			if (record != null)
			{
				result = new CompletedTutorialPagesRecord();
				result.SetValue(record);
			}

			_mutex.ReleaseMutex();
			return result;
		}

		public bool TutorialIsActive
		{
			get => _tutorialIsActive;
			private set
			{
				if (value == _tutorialIsActive) return;
				_tutorialIsActive = value;
				TutorialIsActiveChangeEvent?.Invoke(this, _tutorialIsActive);
			}
		}

		public event TutorialIsActiveHandler TutorialIsActiveChangeEvent;

		public string CurrentPageId => CurrentPage?.Id;

		// \ITutorialManager

		private void OnCompletePageHandler(object sender, EventArgs args)
		{
			Assert.IsNotNull(CurrentPage);

			var page = (ITutorialPage) sender;
			var completeTutorialArgs = (CompleteTutorialPageEventArgs) args;
			Assert.IsTrue(CurrentPage == page);

			OnCompletePage(page, completeTutorialArgs);
		}

		protected virtual void OnCompletePage(ITutorialPage page, CompleteTutorialPageEventArgs args)
		{
			var record = GetRecord(page.Id);
			var newRecord = new CompletedTutorialPagesRecord
			{
				PageId = page.Id,
				IsFinished = args.MarkPageAsFinished,
				Metadata = args.Metadata
			};

			bool doPersist;
			if (record == null)
			{
				doPersist = true;

				_mutex.WaitOne();
				CompletedData.CompletedPages.Add(newRecord);
				_mutex.ReleaseMutex();
			}
			else
			{
				doPersist = !newRecord.Equals(record);
				if (doPersist) record.SetValue(newRecord);
			}

			if (args.CloseTutorialPage)
			{
				CurrentPage.CompleteTutorialPageEvent -= OnCompletePageHandler;
				CurrentPage = null;
				TutorialIsActive = false;
			}

			if (doPersist) DoPersist();
		}

		private void DoPersist()
		{
			_validate = true;
			if (_waitForPersist) return;

			_mutex.WaitOne();
			var dataClone = new CompletedTutorialPagesData();
			dataClone.Restore(CompletedData);
			_mutex.ReleaseMutex();

			_validate = false;
			_waitForPersist = true;

			PersistCompleteTutorialPagesData(dataClone, b =>
			{
				if (!b) Debug.LogError("Failed to persist TutorialManager data.");

				_waitForPersist = false;
				if (_validate) DoPersist();
			});
		}

		protected abstract void PersistCompleteTutorialPagesData(CompletedTutorialPagesData data,
			Action<bool> readyCallback);
	}
}