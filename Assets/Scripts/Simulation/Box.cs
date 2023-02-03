using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Box {
	public enum BoxType {
		Hitbox,
		HurtBox,
		Pushbox
	}

	[HideInInspector]
	public int ownerEntityHash;
	public Vector2Int position;
	public Vector2Int size;
	public BoxType type;

	public bool OverlapAnyBoxes(IList<Box> boxes) {
		foreach (Box hurtBox in boxes)
			if (OverlapBox(hurtBox))
				return true;
		return false;
	}

	public bool OverlapBox(Box other) {
		Vector2Int posDif = (other.position - position) * 2;
		Vector2Int combinedSize = other.size + size;
		return Mathf.Abs(posDif.x) < combinedSize.x && Mathf.Abs(posDif.y) < combinedSize.y;
	}
}