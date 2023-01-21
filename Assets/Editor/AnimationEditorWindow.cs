using UnityEditor;
using UnityEngine;

public class AnimationEditorWindow : EditorWindow {

	private AnimationData animData;
	private int editingFrame;
	private int playingFrame;
	private bool isPlaying;
	private float timeStartedPlaying;
	private float previewScale;

	private GUIStyle centerLabelStyle;
	private GUIStyle rightLabelStyle;
	private GUIStyle bottomCenterImageStyle;
	private GUIStyle hurtBoxStyle;

	[MenuItem("Window/Animation Editor")]
	public static void ShowWindow() {
		GetWindow<AnimationEditorWindow>("sick as hell animation editor");
	}

	private void Update() {
		if (Application.isPlaying)
			return;

		if (hasFocus || focusedWindow.name.Equals("Inspector"))
			Repaint();
	}

	private void OnInspectorUpdate() {
		Repaint();
	}

	private void OnLostFocus() {
		isPlaying = false;
	}

	private void OnGUI() {
		centerLabelStyle = new GUIStyle(EditorStyles.label);
		centerLabelStyle.alignment = TextAnchor.MiddleCenter;

		rightLabelStyle = new GUIStyle(EditorStyles.label);
		rightLabelStyle.alignment = TextAnchor.MiddleRight;

		bottomCenterImageStyle = new GUIStyle(GUI.skin.box);
		bottomCenterImageStyle.alignment = TextAnchor.LowerCenter;

		hurtBoxStyle = new GUIStyle(GUIStyle.none);

		if (Selection.activeObject is AnimationData data && data != animData)
			OnNewAnimationSelected(data);

		if (animData == null) {
			GUILayout.Label("hello??? u dont havent selected any animation data :/");
		} else {

			// header
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"{animData.name} (frame {editingFrame + 1})", EditorStyles.largeLabel);
			GUILayout.FlexibleSpace();
			GUILayout.Label("Zoom");
			previewScale = EditorGUILayout.Slider(previewScale, 0.1f, 4, GUILayout.MinWidth(105));
			EditorGUILayout.EndHorizontal();

			// preview area
			DrawPreviewArea();

		}
	}

	private void OnNewAnimationSelected(AnimationData newAnimation) {
		animData = newAnimation;
		editingFrame = 0;
		isPlaying = false;
	}

	private void DrawPreviewArea() {

		EditorGUILayout.BeginVertical();

		// draw the preview
		GUIContent previewContent = new GUIContent("idk, nothin to show ya :O");
		int frameCount = animData?.frames?.Count ?? 0;
		int previewFrame = isPlaying ? playingFrame : editingFrame;
		previewFrame = Mathf.Clamp(previewFrame, 0, frameCount - 1);

		// draw preview and hitbox info
		Rect previewRect = GUILayoutUtility.GetRect(previewContent, bottomCenterImageStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(position.height * 0.5f));
		if (frameCount > 0 && animData.frames[previewFrame].sprite != null)
			DrawPreviewFrame(previewRect, animData.frames[previewFrame]);
		else
			GUI.Box(previewRect, previewContent, bottomCenterImageStyle);

		// draw the control buttons
		GUILayout.BeginHorizontal();
		GUILayoutOption width69 = GUILayout.Width(69);

		bool canPlay = frameCount > 1;

		// play button
		EditorGUI.BeginDisabledGroup(!canPlay);
		if (isPlaying && !canPlay)
			isPlaying = false;

		if (GUILayout.Button(isPlaying ? "Stop" : "Play", width69)) {
			isPlaying = !isPlaying;
			if (isPlaying) {
				timeStartedPlaying = Time.realtimeSinceStartup;
				playingFrame = 0;
			}
		}
		EditorGUI.EndDisabledGroup();

		if (isPlaying) {
			playingFrame = (((int)((Time.realtimeSinceStartup - timeStartedPlaying) * 60)) / animData.simulationFrameDuration) % frameCount;
		}

		// Prev/Next buttons and frame counter
		EditorGUI.BeginDisabledGroup(!canPlay || isPlaying);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Prev", width69)) {
			MoveEditingFrame(-1);
		}

		// frame counter
		GUILayout.Label($"{Mathf.Min(frameCount, previewFrame + 1)}/{frameCount}", centerLabelStyle, GUILayout.Width(40));

		if (GUILayout.Button("Next", width69)) {
			MoveEditingFrame(1);
		}
		GUILayout.FlexibleSpace();
		EditorGUI.EndDisabledGroup();

		// fps counter
		GUILayout.Label($"at {(60 / animData.simulationFrameDuration)} FPS", rightLabelStyle, width69);

		GUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		void MoveEditingFrame(int direction) {
			editingFrame = (editingFrame + direction) % frameCount;
			if (editingFrame < 0)
				editingFrame += frameCount;
		}
	}

	private void DrawPreviewFrame(Rect previewRect, AnimationData.FrameData frameData) {
		// draw sprite
		GUI.Box(previewRect, "", bottomCenterImageStyle);
		float heightOffset = 20;
		Vector2 origin = new Vector2(previewRect.center.x, previewRect.max.y - heightOffset);

		// draw guides
		Color guideColour = new Color(1, 1, 1, 0.4f);
		GUI.DrawTexture(new Rect(previewRect.x, origin.y, previewRect.size.x, 1), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 1f / previewRect.size.x, guideColour, 0, 0);
		GUI.DrawTexture(new Rect(origin.x, previewRect.y, 1, previewRect.size.y), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, previewRect.size.y, guideColour, 0, 0);

		// draw sprite
		float PPU = frameData.sprite.pixelsPerUnit;
		Vector2 spriteSize = frameData.sprite.textureRect.size * ((Simulation.Instance?.simulationScale ?? 128) / PPU) * previewScale;
		Rect spriteRect = new Rect(origin - new Vector2(spriteSize.x / 2f, spriteSize.y) + new Vector2(frameData.spriteOffset.x, -frameData.spriteOffset.y) * previewScale, spriteSize);
		GUI.DrawTexture(spriteRect, frameData.sprite.texture);

		float vBoxAlpha = isPlaying ? 0.2f : 0.3f;

		Color hurtBoxColour = Color.yellow;
		hurtBoxColour.a = vBoxAlpha;

		// hurt boxes
		foreach (Simulation.VBox vBox in frameData.hurtBoxes) {
			Vector2 size = (Vector2)vBox.size * previewScale;
			GUI.DrawTexture(new Rect(origin + (new Vector2(vBox.position.x, -vBox.position.y) * previewScale) - (size / 2f), size), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, size.x / size.y, hurtBoxColour, 0, 0);
		}

		Color hitBoxColour = Color.red;
		hitBoxColour.a = vBoxAlpha;

		// hit boxes
		foreach (Simulation.VBox vBox in frameData.hitBoxes) {
			Vector2 size = (Vector2)vBox.size * previewScale;
			GUI.DrawTexture(new Rect(origin + (new Vector2(vBox.position.x, -vBox.position.y) * previewScale) - (size / 2f), size), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, size.x / size.y, hitBoxColour, 0, 0);
		}
	}
}