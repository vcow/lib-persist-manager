using System;
using UnityEngine;

namespace Base.PersistManager.TutorialExtensions
{
	/// <summary>
	/// Данные события завершения страницы туториала.
	/// </summary>
	public class CompleteTutorialPageEventArgs : EventArgs
	{
		/// <summary>
		/// Флаг, указывающий на то, что страница туториала должна быть закрыта.
		/// </summary>
		public bool CloseTutorialPage { get; }
		
		/// <summary>
		/// Флаг, указывающий на то, что страница туториала должна быть отмечена как заверщенная.
		/// </summary>
		public bool MarkPageAsFinished { get; }
		
		/// <summary>
		/// Метаданные, которые сохраняются и передаются странице во время следующего ее открытия.
		/// </summary>
		public string Metadata { get; }

		public CompleteTutorialPageEventArgs(bool closeTutorialPage = true,
			bool markPageAsFinished = true, string metadata = "")
		{
			CloseTutorialPage = closeTutorialPage;
			MarkPageAsFinished = markPageAsFinished;
			Metadata = metadata;
		}
	}

	/// <summary>
	/// Интерфейс страницы туториала.
	/// </summary>
	public interface ITutorialPage
	{
		/// <summary>
		/// Уникальный идентификатор страницы туториала.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Инстанцировать представление страницы.
		/// </summary>
		/// <param name="pageContainer">Контейнер страницы в менеджере туториала.</param>
		/// <param name="callback">Коллбек, в который будет возвращен экземпляр созданной страницы.</param>
		/// <returns>Возвращает <code>true</code>, если страница будет создана.</returns>
		bool InstantiatePage(Transform pageContainer, Action<GameObject> callback);

		/// <summary>
		/// Событие завершения страницы туториала.
		/// </summary>
		event EventHandler<CompleteTutorialPageEventArgs> CompleteTutorialPageEvent;
	}
}