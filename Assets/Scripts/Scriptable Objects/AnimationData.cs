using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Animation Data")]
public class AnimationData : ScriptableObject, INameableElement {
	
	public int Hash => ObjectRef.GetHash(this);

	public bool loop;
	[Tooltip("The number of simulation frames that an animation frame lasts.")]
	[Range(1, 10)]
	public int simulationFrameDuration = 2;
	[NameElements]
	public List<FrameData> frames = new() { new FrameData() };

	public Animation[] transitionableAnimations;

	public FrameData GetFrame(int frame) {
		frame = frame / simulationFrameDuration;
		if (loop)
			return frames[frame % frames.Count];
		else
			return frames[Mathf.Clamp(frame, 0, frames.Count)];
	}
	
	public int GetNextFrameIndex(int currentFrameIndex) {
		currentFrameIndex++;
		int animationFrameIndex = currentFrameIndex / simulationFrameDuration;
		if (animationFrameIndex >= frames.Count) {
			if (loop)
				return 0;
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
		public Vector2Int spriteOffset;
		public FrameFlags frameFlags;
		public List<Simulation.VBox> hitBoxes;
		public List<Simulation.VBox> hurtBoxes;
		public List<FrameCommand> frameCommands;
		
		public bool TryGetFrameCommand<T>(out T command) where T : FrameCommand {
			command = (T)frameCommands.Find(x => x is T);
			if (command == null)
				return false;
			return true;
		}

		public string GetArrayElementName(int index) => $"Frame {index + 1}";
	}
}