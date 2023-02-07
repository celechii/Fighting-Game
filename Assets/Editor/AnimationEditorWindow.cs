using UnityEditor;
using UnityEngine;

public class AnimationEditorWindow : EditorWindow {

	private Color noActiveFramesColour = Color.gray;
	private Color startupFrameColour;
	private Color activeFrameColour;
	private Color recoveryFrameColour;

	private AnimationData animData;
	private int editingFrame;
	private int playingFrame;
	private bool isPlaying;
	private float timeStartedPlaying;
	private float rawPreviewScale;
	private const float scaleMultiplier = 0.1f;
	private float PreviewScale => rawPreviewScale * scaleMultiplier;

	private GUIStyle centerLabelStyle;
	private GUIStyle rightLabelStyle;
	private GUIStyle bottomCenterImageStyle;
	private GUIStyle hurtBoxStyle;

	[MenuItem("Window/Animation Editor")]
	public static void ShowWindow() {
		GetWindow<AnimationEditorWindow>("sick as hell animation editor");
	}

	private void OnEnable() {
		ColorUtility.TryParseHtmlString("#36B37E", out startupFrameColour);
		ColorUtility.TryParseHtmlString("#FF5D5D", out activeFrameColour);
		ColorUtility.TryParseHtmlString("#0069B6", out recoveryFrameColour);
	}

	private void Update() {
		if (Application.isPlaying)
			return;

		if (isPlaying || Selection.activeObject == animData)
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
			float animationDuration = (float)animData.TotalFrames / Simulation.FrameRate;

			GUILayout.Label($"{animData.name} (duration: {animationDuration.ToString("0.##")}s)");
			if (Selection.activeObject != animData && EditorGUILayout.LinkButton($"select"))
				Selection.SetActiveObjectWithContext(animData, null);
			// EditorGUILayout.Separator();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Zoom");
			rawPreviewScale = EditorGUILayout.Slider(rawPreviewScale, 1, 4, GUILayout.MinWidth(105), GUILayout.MaxWidth(200));
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
		int frameCount = isPlaying ? (animData?.TotalFrames ?? 0) : (animData?.frames?.Count ?? 0);
		int previewFrame = isPlaying ? playingFrame : editingFrame;
		previewFrame = Mathf.Clamp(previewFrame, 0, frameCount - 1);

		// draw preview and hitbox info
		Rect previewRect = GUILayoutUtility.GetRect(previewContent, bottomCenterImageStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(position.height * 0.5f));

		AnimationData.FrameData previewFrameData = isPlaying ? animData.GetFrame(playingFrame) : animData.frames[previewFrame];

		if (frameCount > 0 && previewFrameData.sprite != null)
			DrawPreviewFrame(previewRect, previewFrameData);
		else
			GUI.Box(previewRect, previewContent, bottomCenterImageStyle);

		// draw the control buttons
		GUILayout.BeginHorizontal();
		GUILayoutOption width = GUILayout.Width(69);

		bool canPlay = frameCount > 1;

		// play button
		EditorGUI.BeginDisabledGroup(!canPlay);
		if (isPlaying && !canPlay)
			isPlaying = false;

		if (GUILayout.Button(isPlaying ? "Stop" : "Play", width)) {
			isPlaying = !isPlaying;
			if (isPlaying) {
				timeStartedPlaying = Time.realtimeSinceStartup;
				playingFrame = 0;
				animData.UpdateTotalFrameDuration();
			}
		}
		EditorGUI.EndDisabledGroup();

		if (isPlaying) {
			playingFrame = ((int)((Time.realtimeSinceStartup - timeStartedPlaying) * Simulation.FrameRate)) % animData.TotalFrames;
		}

		// Prev/Next buttons and frame counter
		EditorGUI.BeginDisabledGroup(!canPlay || isPlaying);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Prev", width)) {
			MoveEditingFrame(-1);
		}

		// frame counter
		GUILayout.Label($"{Mathf.Min(frameCount, previewFrame + 1)}/{frameCount}", centerLabelStyle, GUILayout.Width(40));

		if (GUILayout.Button("Next", width)) {
			MoveEditingFrame(1);
		}
		GUILayout.FlexibleSpace();
		EditorGUI.EndDisabledGroup();

		void MoveEditingFrame(int direction) {
			editingFrame = (editingFrame + direction) % frameCount;
			if (editingFrame < 0)
				editingFrame += frameCount;
		}

		// fps counter
		GUILayout.Label($"at {(Simulation.FrameRate / animData.simulationFrameDuration)} FPS", rightLabelStyle, width);

		GUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		DrawFrameData();
	}

	private enum FrameType {
		None,
		Startup,
		Active,
		Recovery
	}

	private void DrawFrameData() {
		Vector2 frameSize = new Vector2(10, 15);
		float spacing = 5f;
		Rect frameDataRect = GUILayoutUtility.GetRect(GUIContent.none, bottomCenterImageStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(frameSize.y));

		FrameType[] frameTypes = GetFrameTypeData();
		if (frameTypes == null)
			return;

		// draw currently selected box
		if (!isPlaying) {
			int editingFrameIndex = 0;
			for (int i = 0; i < editingFrame; i++)
				editingFrameIndex += animData.frames[i].frameDuration;
			editingFrameIndex *= animData.simulationFrameDuration;

			Vector2 size = new Vector2(animData.frames[editingFrame].frameDuration * animData.simulationFrameDuration * (frameSize.x + spacing) - spacing, 2f);

			GUI.DrawTexture(new Rect(frameDataRect.position + new Vector2((spacing + frameSize.x) * editingFrameIndex, frameSize.y + 2f), size), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, size.x / size.y, Color.gray, 0, 0);
		} else {
			Vector2 size = new Vector2(frameSize.x, 2f);

			GUI.DrawTexture(new Rect(frameDataRect.position + new Vector2((spacing + frameSize.x) * playingFrame, frameSize.y + 2f), size), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, size.x / size.y, Color.white, 0, 0);
		}

		// draw each box
		for (int i = 0; i < frameTypes.Length; i++) {
			Color colour = frameTypes[i]
			switch {
				FrameType.Active => activeFrameColour,
					FrameType.Recovery => recoveryFrameColour,
					FrameType.Startup => startupFrameColour,
					_ => noActiveFramesColour
			};
			GUI.DrawTexture(new Rect(frameDataRect.position + Vector2.right * ((spacing + frameSize.x) * i), frameSize), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, frameSize.x / frameSize.y, colour, 0, 0);
		}

		GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
		labelStyle.richText = true;

		if (frameTypes[0] != FrameType.None) {
			int startupFrames = 0;
			int activeFrames = 0;
			int recoveryFrames = 0;
			for (int i = 0; i < frameTypes.Length; i++) {
				if (frameTypes[i] == FrameType.Startup)
					startupFrames++;
				else if (frameTypes[i] == FrameType.Active)
					activeFrames++;
				else if (frameTypes[i] == FrameType.Recovery)
					recoveryFrames++;
			}

			EditorGUILayout.LabelField($"Startup: <color=white>{startupFrames}</color>", labelStyle);
			EditorGUILayout.LabelField($"Active: <color=white>{activeFrames}</color>", labelStyle);
			EditorGUILayout.LabelField($"Recovery: <color=white>{recoveryFrames}</color>", labelStyle);
		}

		EditorGUILayout.LabelField($"<b>Total: <color=white>{frameTypes.Length}</color></b>", labelStyle);
	}

	private FrameType[] GetFrameTypeData() {
		FrameType[] frameTypes = new FrameType[animData.TotalFrames];
		if (frameTypes.Length == 0)
			return null;
		bool hasActiveFrames = false;
		for (int i = 0; i < frameTypes.Length; i++) {
			bool hasHitBox = animData.GetFrame(i).hitBoxes.Count > 0;
			if (!hasActiveFrames)
				hasActiveFrames = hasHitBox;
			frameTypes[i] = hasHitBox ? FrameType.Active : hasActiveFrames ? FrameType.Recovery : FrameType.Startup;
		}

		if (!hasActiveFrames)
			for (int i = 0; i < frameTypes.Length; i++)
				frameTypes[i] = FrameType.None;
		return frameTypes;
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
		Vector2 pivotOffset = frameData.sprite.pivot * ((Simulation.Instance?.simulationScale ?? 128) / PPU) * PreviewScale;
		Vector2 spriteSize = new Vector2(frameData.sprite.texture.width, frameData.sprite.texture.height) * ((Simulation.Instance?.simulationScale ?? 128) / PPU) * PreviewScale;
		Rect spriteRect = new Rect(origin - new Vector2(0, spriteSize.y) - new Vector2(pivotOffset.x, -pivotOffset.y) + new Vector2(frameData.spriteOffset.x, -frameData.spriteOffset.y) * PreviewScale, spriteSize);
		GUI.DrawTexture(spriteRect, frameData.sprite.texture);

		float boxAlpha = isPlaying ? 0.1f : 0.3f;

		// push box
		DrawBox(frameData.pushBox, Color.yellow);
		foreach(Box box in frameData.hurtBoxes)
			DrawBox(box, Color.green);
		foreach(Box box in frameData.hitBoxes)
			DrawBox(box, Color.red);

		void DrawBox(Box box, Color colour) {
			colour.a = boxAlpha;
			Vector2 size = (Vector2)box.size * PreviewScale;
			GUI.DrawTexture(new Rect(origin + (new Vector2(box.position.x, -box.position.y) * PreviewScale) - (size / 2f), size), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, size.x / size.y, colour, 0, 0);
		}
	}
}