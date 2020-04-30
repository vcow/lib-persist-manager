using System.IO;
using System.Reflection;
using Base.PersistManager;
using UnityEditor;
using UnityEngine;

namespace Sample.Editor
{
	public class ClearSavedData : EditorWindow
	{
		private bool _clearFile = true;
		private bool _clearPlayerPrefs;
		private bool _showMessage;
		private float _showMessageTime;

		[MenuItem("Tools/Clear Saved Data")]
		public static void ShowWindow()
		{
			GetWindow<ClearSavedData>(false, "Clear saved data").Show();
		}

		private void OnGUI()
		{
			var isPlaying = Application.isPlaying;
			if (_showMessage)
			{
				EditorGUILayout.HelpBox(@"Data cleared", MessageType.Info);
				_showMessageTime -= Time.deltaTime;

				if (_showMessageTime <= 0)
				{
					_showMessage = false;
				}
			}
			else
			{
				if (isPlaying)
				{
					EditorGUILayout.HelpBox(@"Can't clear when Application is playing.", MessageType.Error);
				}
				else
				{
					EditorGUILayout.HelpBox(@"Select the data that you want to clear.", MessageType.Info);
				}
			}

			EditorGUI.BeginDisabledGroup(isPlaying);
			_clearFile = EditorGUILayout.Toggle("Clear file", _clearFile);
			_clearPlayerPrefs = EditorGUILayout.Toggle("Clear player prefs", _clearPlayerPrefs);
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(10);

			EditorGUI.BeginDisabledGroup(isPlaying || !_clearFile && !_clearPlayerPrefs);
			if (GUILayout.Button("Clear")) DoClear();
			EditorGUI.EndDisabledGroup();
		}

		private void DoClear()
		{
			if (_clearFile)
			{
				if (File.Exists(PersistManagerBase.SavedDataPath))
				{
					File.Delete(PersistManagerBase.SavedDataPath);
					Debug.Log("Data file was deleted.");
				}

				var snapshots = Directory.GetFiles(Application.persistentDataPath, @"*.png");
				if (snapshots.Length > 0)
				{
					foreach (var snapshot in snapshots)
					{
						File.Delete(snapshot);
					}

					Debug.Log("Fish snapshots was deleted.");
				}
				
				// TODO: Add other files for delete here.
			}

			if (_clearPlayerPrefs)
			{
				var cleared = false;
				if (PlayerPrefs.HasKey(PersistManager.Key))
				{
					PlayerPrefs.DeleteKey(PersistManager.Key);
					cleared = true;
				}
				
				// TODO: Add other PlayerPrefs for delete here.

				if (cleared) Debug.Log("Player prefs was cleared.");
			}

			_showMessage = true;
			_showMessageTime = 0.5f;

			var window = GetWindow<ClearSavedData>();
			if (!IsDocked(window))
			{
				window.Close();
			}
		}

		private bool IsDocked(EditorWindow window)
		{
			if (window == null) return false;
			return (bool) (typeof(EditorWindow).GetProperty("docked",
					               BindingFlags.Public |
					               BindingFlags.NonPublic |
					               BindingFlags.Instance |
					               BindingFlags.Static)?.GetGetMethod(true)
				               .Invoke(this, null) ?? false);
		}
	}
}