using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public struct Entity {
	public static int entityID;
	[ReadOnly]
	public int ID;

	public int playerOwner; // 0 for no owner, 1 for player 1, 2 for player 2
	public int entityHash;
	public EntityType type;
	public int stateIndex;
	public int animationHash;
	public int animationFrame;
	public Vector2Int position;
	public Vector2Int velocity;
	public bool isFacingRight;

	public Entity(EntityFSM entityController, int playerOwner) {
		ID = entityID;
		entityID++;

		type = entityController.Type;
		this.playerOwner = playerOwner;
		this.entityHash = ObjectRef.GetHash(entityController);
		this.animationHash = 0;

		stateIndex = 0;
		animationFrame = 0;
		position = Vector2Int.zero;
		velocity = Vector2Int.zero;
		isFacingRight = true;
	}

	public AnimationData.FrameData GetCurrentFrameData() {
		return ObjectRef.GetObject<AnimationData>(animationHash).GetFrame(animationFrame);
	}

	public Box GetMirroredBox(Box box) {
		if (isFacingRight)
			return box;
		else {
			box.position = new Vector2Int(-box.position.x, box.position.y);
			return box;
		}
	}

	public void SetAnimation(AnimationData animationData) {
		int hash = ObjectRef.GetHash(animationData);
		if (animationHash != hash) // set the animation to the start if if's a new animation
			animationFrame = 0;
		animationHash = hash;
	}

	public void NextAnimationFrame() {
		animationFrame = ObjectRef.GetObject<AnimationData>(animationHash).GetNextFrameIndex(animationFrame);
	}
}

public enum EntityType {
	Player,
	Projectile,
	Effect,
	Utility
}