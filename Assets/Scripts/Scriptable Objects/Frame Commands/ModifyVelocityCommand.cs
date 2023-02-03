using UnityEngine;

[CreateAssetMenu(menuName = "Frame Command/Modify Velocity")]
public class ModifyVelocityCommand : FrameCommand {
	
	public enum ModificationMode {
		Add,
		Set
	}

	public ModificationMode mode;
	public Vector2Int velocity;
	
	public Entity Modify(Entity entity) {
		Vector2Int velocity = this.velocity * (entity.isFacingRight ? new Vector2Int(-1, 1) : Vector2Int.one);
		switch (mode) {
			case ModificationMode.Add:
				entity.velocity += velocity;
				break;
			case ModificationMode.Set:
				entity.velocity = velocity;
				break;
		}
		return entity;
	}

}
