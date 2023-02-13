using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Animation Data")]
public class AnimationData : ScriptableObject, INameableElement {

	public int Hash => ObjectRef.GetHash(this);

	public bool loop;
	[Tooltip("The number of simulation frames that an animation frame lasts.")]
	[Range(1, 10)]
	[OnValueChanged(nameof(UpdateTotalFrameDuration))]
	public int simulationFrameDuration = 2;
	[NameElements]
	[OnValueChanged(nameof(UpdateTotalFrameDuration))]
	public List<FrameData> frames = new() { new FrameData() };
	[NameElements]
	public CancelData[] cancelData;

	/// <summary>
	/// The total number of frames as played in the simulation.
	/// </summary>
	[ShowNativeProperty]
	public int TotalFrames {
		get {
			if (totalFrames == -1)
				UpdateTotalFrameDuration();
			return totalFrames;
		}
	}
	private int totalFrames = -1;

	private int CalculateTotalFrameDuration() {
		int total = 0;
		for (int i = 0; i < frames.Count; i++)
			total += frames[i].frameDuration;
		return total * simulationFrameDuration;
	}

	[ContextMenu("Update Frame Duration")]
	public void UpdateTotalFrameDuration() => totalFrames = CalculateTotalFrameDuration();

	/// <summary>
	/// Get the frame data at the simulation frame index.
	/// </summary>
	public FrameData GetFrame(int frame) {
		if (loop)
			frame = frame % TotalFrames;
		else
			frame = Mathf.Clamp(frame, 0, TotalFrames);

		for (int i = 0; i < frames.Count; i++) {
			int unscaledFrameDuration = frames[i].frameDuration * simulationFrameDuration;
			if (frame < unscaledFrameDuration)
				return frames[i];
			frame -= unscaledFrameDuration;
		}

		throw new System.IndexOutOfRangeException($"There is no frame {frame} for animation {name}!");
	}

	/// <summary>
	/// Returns the next frame index, taking looping or clamping into account.
	/// </summary>
	public int GetNextFrameIndex(int currentFrameIndex) {
		currentFrameIndex++;
		if (currentFrameIndex == TotalFrames) {
			if (loop)
				currentFrameIndex = 0;
			else
				currentFrameIndex--;
		}

		return currentFrameIndex;
	}

	public string GetArrayElementName(int index) => name;

	[System.Serializable]
	public struct FrameData : INameableElement {
		[ShowAssetPreview(128, 128)]
		public Sprite sprite;
		[AllowNesting]
		[MinValue(1)]
		public int frameDuration;
		public Vector2Int spriteOffset;
		public FrameFlags frameFlags;
		public Box pushBox;
		public List<Box> hitBoxes;
		public List<Box> hurtBoxes;
		public List<FrameCommand> frameCommands;

		public bool TryGetFrameCommand<T>(out T command)where T : FrameCommand {
			command = (T)frameCommands.Find(x => x is T);
			if (command == null)
				return false;
			return true;
		}

		public string GetArrayElementName(int index) => $"Frame {index + 1}";
	}

	[System.Serializable]
	public struct CancelData : INameableElement {
		private enum Cancelable {
			Until,
			At
		}

		[SerializeField]
		private List<AnimationData> cancelableAnimations;
		[SerializeField]
		private Cancelable cancelable;
		[SerializeField]
		private int cancelFrame;

		public bool CanCancelIntoAnimation(AnimationData animation, int currentSimulationFrame) {
			if (cancelableAnimations.Contains(animation)) {
				switch (cancelable) {
					case Cancelable.Until:
						return currentSimulationFrame <= cancelFrame;
					case Cancelable.At:
						return currentSimulationFrame >= cancelFrame;
				}
			}
			return false;
		}

		public string GetArrayElementName(int index) {
			string[] animationNames = new string[cancelableAnimations.Count];
			for (int i = 0; i < animationNames.Length; i++)
				animationNames[i] = cancelableAnimations[i]?.name ?? "Null";
			return $"Can Cancel Into {animationNames?.ListValues() ?? "Null"} {cancelable.MakeEnumReadable()} Frame {cancelFrame}";
		}
	}
}