using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EntityData/Character")]
public class PlayerEntityData : EntityData {

	private enum CustomPlayerData {
		Health,
		HitStunFrames,
		HitStopFrames
	}

	[Header("Character Data")]
	public string displayName;

	[Header("Movement")]
	public int groundSpeed = 10;
	public int groundAccel = 1;
	public int groundDecel = 1;
	public int airAccel = 1;
	public int airDecel = 1;

	[Header("Jump")]
	public int jumpForce;
	public int jumpDecel;
	public int fallAccel;
	public int jumpBufferFrames;

	[Header("Animation Commands")]
	[NameElements]
	[SerializeField]
	private Command[] attackCommands;

	[Header("Animations")]
	[SerializeField]
	private AnimationData idleAnim;
	[SerializeField]
	private AnimationData runAnim;
	[SerializeField]
	private AnimationData jumpAnim;
	[SerializeField]
	private AnimationData airPeakAnim;
	[SerializeField]
	private AnimationData airFallAnim;

	private AnimationData.FrameData currentFrame;
	private bool isGrounded;
	private Input prevInput;
	private Input currentInput;

	public override Entity ProcessHurtBoxes(Entity entity, List<Simulation.HitData> hitBoxOverlaps) {
		Simulation.HitData hitData = hitBoxOverlaps[0];

		// check if we can skip *damage*
		bool canSkipDamage = false;
		currentFrame = entity.GetCurrentFrameData();
		if (currentFrame.frameFlags.HasFlag(FrameFlags.Invincible))
			canSkipDamage = true;

		// deal damage
		if (!canSkipDamage && Simulation.Instance.TryGetCustomData(entity, CustomPlayerData.Health, out int currentHealth)) {
			int newHealth = Mathf.Max(0, currentHealth - hitData.damage);
			// Simulation.Instance.SetCustomData(entity, CustomPlayerData.Health, newHealth);
		}

		// deal knockback as simple set velocity
		entity.velocity = hitData.knockback;

		// apply hitstun
		// this OVERWRITES the number of stunframes
		// Simulation.Instance.SetCustomData(entity, CustomPlayerData.HitStunFrames, hitData.stunFrames);

		return entity;
	}

	/// <summary>
	/// Process input, update player data, and return a new entity with updated state.
	/// </summary>
	public override Entity UpdateBehaviour(Entity entity) {
		currentFrame = entity.GetCurrentFrameData();

		isGrounded = Simulation.IsPushBoxGrounded(currentFrame.pushBox, entity.position.y);

		currentInput = entity.playerOwner == 1 ? Simulation.Instance.CurrentLocalInput : Simulation.Instance.CurrentRemoteInput;
		prevInput = entity.playerOwner == 1 ? Simulation.Instance.PrevLocalInput : Simulation.Instance.PrevRemoteInput;

		// check jump
		// if (CheckBufferedAction(() => isGrounded && currentInput.Has(Input.Jump), () => currentInput.Has(Input.Jump) && !prevInput.Has(Input.Jump), entity, EntityVar.BufferedJump, jumpBufferFrames))
		// 	entity.velocity.y = jumpForce;

		UpdateFalling(ref entity);

		UpdateMovementInput(ref entity);

		UpdatePosition(ref entity);

		return entity;
	}

	// protected bool CheckBufferedAction(System.Func<bool> condition, System.Func<bool> activator, Entity entity, EntityVar bufferVariable, int maxBufferFrames) {
	// 	int bufferFrames = Simulation.Instance.CustomEntityData[entity.ID][bufferVariable];

	// 	if (activator.Invoke())
	// 		bufferFrames = maxBufferFrames + 1;

	// 	if (bufferFrames > 0) { // if we want to do it
	// 		// if we can do it then go!
	// 		if (condition.Invoke()) {
	// 			Simulation.Instance.SetCustomData(entity, bufferVariable, 0);
	// 			return true;
	// 		}

	// 		// if we can't, decrement the buffer frames for later
	// 		Simulation.Instance.SetCustomData(entity, bufferVariable, bufferFrames - 1);
	// 	}

	// 	return false;
	// }

	protected virtual void UpdateFalling(ref Entity entity) {
		if (isGrounded || currentFrame.frameFlags.Has(FrameFlags.FreezeFall))
			return;

		entity.velocity.y -= (currentInput.Has(Input.Jump) && entity.velocity.y > 0) ? jumpDecel : fallAccel;
	}

	protected virtual void UpdateMovementInput(ref Entity entity) {
		int accel = isGrounded ? groundAccel : airAccel;
		int decel = isGrounded ? groundDecel : airDecel;

		int movementInput = (currentInput.Has(Input.MoveLeft) ? -1 : 0) + (currentInput.Has(Input.MoveRight) ? 1 : 0);

		if (movementInput != 0) {
			entity.velocity.x = Mathf.Clamp(entity.velocity.x + (accel * movementInput), -groundSpeed, groundSpeed);
		} else {
			if (Mathf.Abs(entity.velocity.x) > 0)
				entity.velocity.x -= Mathf.Min(Mathf.Abs(entity.velocity.x), decel) * (int)Mathf.Sign(entity.velocity.x);
		}
	}

	protected virtual void UpdatePosition(ref Entity entity) {
		// update facing direction
		if (Mathf.Abs(entity.velocity.x) > 10)
			entity.isFacingRight = entity.velocity.x > 0;

		entity.position += entity.velocity;

		// if you've collided with the ground
		if (entity.position.y <= 0) {
			entity.position.y = 0;
			entity.velocity.y = 0;
		}
	}

	[System.Serializable]
	private struct Command : INameableElement {
		[System.Flags]
		public enum Condition {
			IsGrounded = 1 << 0,
			IsAirborn = 1 << 1
		}

		public Input requiredInput;
		public Condition additionalConditions;
		public AnimationData animation;

		public string GetArrayElementName(int index) => $"{requiredInput.MakeEnumReadable()}: {animation.name}";
	}
}