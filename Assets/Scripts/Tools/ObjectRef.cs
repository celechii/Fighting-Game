using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ObjectRef : MonoBehaviour {

	private static ObjectRef control;

	[SerializeField]
	private ScriptableObject[] allObjects;

	private Dictionary<int, ScriptableObject> objects;

	private void Awake() {
		control = this;

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.playModeStateChanged += x => {
			if (x == UnityEditor.PlayModeStateChange.ExitingEditMode || x == UnityEditor.PlayModeStateChange.EnteredEditMode)
				UpdateList();
		};

		UpdateList();
		#endif

		BuildDictionary();
	}

	#if UNITY_EDITOR
	[Button]
	private void UpdateList() {
		allObjects = null;
		string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:ScriptableObject", new string[] { "Assets/Data" });
		allObjects = new ScriptableObject[guids.Length];
		for (int i = 0; i < guids.Length; i++)
			allObjects[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));
	}
	
	[Button]
	private void PrintHashList() {
		foreach(ScriptableObject so in allObjects)
			print($"{so.name} ({so.GetType()}): {GetHash(so)}");
	}
	#endif

	/// <summary>Returns a list of all of the scriptable objects of a type.</summary>
	/// <param name="objectArray">The list to be written to.</param>
	public static void GetAllScriptableObjects<T>(out List<T> objects)where T : ScriptableObject {
		objects = new List<T>();
		for (int i = 0; i < control.allObjects.Length; i++) {
			if (control.allObjects[i] is T)
				objects.Add(control.allObjects[i] as T);
		}
	}

	public static int GetHash(ScriptableObject scriptableObject) {
		if (scriptableObject == null)
			return 0;
		return HashFromNameAndType(scriptableObject.name, typeof(ScriptableObject));
	}

	public static int[] GetHashes(ScriptableObject[] scriptableObjects) {
		if (scriptableObjects == null)
			return null;
		int[] results = new int[scriptableObjects.Length];
		for (int i = 0; i < results.Length; i++)
			results[i] = GetHash(scriptableObjects[i]);
		return results;
	}

	public static T GetObject<T>(int hash)where T : ScriptableObject {
		if (hash == 0)
			return null;
		try {
			return (T)control.objects[hash];
		} catch (System.Collections.Generic.KeyNotFoundException) {
			throw new System.Collections.Generic.KeyNotFoundException($"Couldn't find {typeof(T).Name} key {hash}!!");
		}
	}

	public static T[] GetObjects<T>(int[] hash)where T : ScriptableObject {
		if (hash == null)
			return null;
		T[] results = new T[hash.Length];
		for (int i = 0; i < results.Length; i++)
			results[i] = GetObject<T>(hash[i]);
		return results;
	}

	private void BuildDictionary() {
		objects = new Dictionary<int, ScriptableObject>();
		foreach (ScriptableObject so in allObjects)
			objects.Add(GetHash(so), so);
	}

	private static int HashFromNameAndType(string name, System.Type type) {
		return (name + type.ToString()).GetHashCode();
	}
}