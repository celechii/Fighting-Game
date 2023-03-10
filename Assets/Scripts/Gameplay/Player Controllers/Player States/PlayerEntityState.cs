using UnityEngine;
using NaughtyAttributes;

public abstract class PlayerEntityState : EntityState {

	[Header("Player State")]
	[Expandable]
	[SerializeField]
	protected PlayerData playerData;
	[SerializeField]
	private bool applyGravity = true;
	[SerializeField]
	private bool applyVelocity = true;
	[SerializeField]
	private bool applyGroundCollision = true;
	[SerializeField]
	private bool updateFacingDirection = true;

	public override Entity ProcessCollisions(Entity entity, ref EntityState transitionToState) {
		
		// process collisions
		
		return entity;
	}
	
	public override Entity UpdateBehaviour(Entity entity, ref EntityState transitionToState) {
		// apply gravity and/or velocity
		if (applyVelocity) {
			if (applyGravity)
				ApplyGravity(ref entity);
			
			entity.position += entity.velocity;
			if (updateFacingDirection && Mathf.Abs(entity.velocity.x) > 0)
				entity.isFacingRight = entity.velocity.x > 0;
		}

		if (applyGroundCollision)
			ApplyGroundCollision(ref entity);

		return entity;
	}
	
	protected Entity ApplyGravity(ref Entity entity) {
		if (!CurrentFrame.frameFlags.Has(FrameFlags.FreezeFall))
			entity.velocity.y -= (Simulation.Instance.GetCurrentInput(entity).Has(Input.Jump) && entity.velocity.y > 0) ? playerData.JumpGravity : playerData.DefaultGravity;
		return entity;
	}
	
	protected Entity ApplyGroundCollision(ref Entity entity) {
		int pushBoxHeight = Simulation.GetPushBoxHeight(CurrentFrame.pushBox, entity.position.y);
		if (pushBoxHeight <= 0) {
			entity.position.y -= pushBoxHeight;
			if (entity.velocity.y < 0)
				entity.velocity.y = 0;
		}
		return entity;
	}
}