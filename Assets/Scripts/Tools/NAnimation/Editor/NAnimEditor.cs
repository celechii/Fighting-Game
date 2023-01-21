using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NAnimation))]
public class NAnimEditor : Editor {

	private new SerializedProperty name;
	private SerializedProperty framerate;
	private SerializedProperty loop;
	private SerializedProperty loopCount;
	private SerializedProperty nextAnimString;

	private bool showNewGUI;
	private string loopStatus => "Lööp: " + (loop.boolValue ? (loopCount.intValue == 0 ? "Infinity" : $"On ({loopCount.intValue})") : "Off");

	private void OnEnable() {
		name = serializedObject.FindProperty("name");
		framerate = serializedObject.FindProperty("framerate");
		loop = serializedObject.FindProperty("lööp");
		loopCount = serializedObject.FindProperty("loopCount");
		nextAnimString = serializedObject.FindProperty("nextAnim");
	}

	public override void OnInspectorGUI() {
		showNewGUI = EditorGUILayout.Toggle("Show New GUI", showNewGUI);
		if (showNewGUI) {

			serializedObject.Update();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(name);
			framerate.intValue = EditorGUILayout.IntField("FPS", framerate.intValue);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(GUI.skin.box);
			loop.boolValue = EditorGUILayout.Toggle("Loop", loop.boolValue);
			EditorGUILayout.BeginFadeGroup(loop.boolValue ? 1 : 0);
			if (loop.boolValue) {
				EditorGUI.indentLevel++;
				loopCount.intValue = Mathf.Max(0, EditorGUILayout.IntField("Loop Count", loopCount.intValue));
				GUI.enabled = loopCount.intValue > 0;
				EditorGUILayout.PropertyField(nextAnimString);
				GUI.enabled = true;
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		} else {
			base.OnInspectorGUI();
		}
	}
}