using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EntityData/Character")]
public class PlayerEntityData : EntityData {

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

	private bool isGrounded;
	private Input prevInput;
	private Input currentInput;

	public override Simulation.Entity ProcessHurtBoxes(Simulation.Entity entity, List<Simulation.HitBoxOverlap> hitBoxOverlaps) {
		// check if we can skip *damage*
		bool canSkipDamage = false;
		AnimationData.FrameData frameData = entity.GetCurrentFrameData();
		if (frameData.frameFlags.HasFlag(FrameFlags.Invincible) || Simulation.Instance.CustomEntityData[entity.ID][EntityVar.IFrames] > 0)
			canSkipDamage = true;
		else if (frameData.TryGetFrameCommand(out DamageCommand damage) && Simulation.Instance.TryGetCustomData(entity.ID, EntityVar.Health, out int currentHealth)) {
			// deal damage
			int newHealth = Mathf.Max(currentHealth - damage.damage, 0);
			Simulation.Instance.SetCustomData(entity, EntityVar.Health, newHealth);
		}

		// deal knockback
		

		return entity;
	}

	/// <summary>
	/// Process input, update player data, and return a new entity with updated state.
	/// </summary>
	public override Simulation.Entity UpdateBehaviour(Simulation.Entity entity) {
		isGrounded = entity.position.y == 0;

		AnimationData.FrameData currentFrame = entity.GetCurrentFrameData();

		currentInput = entity.playerOwner == 1 ? Simulation.Instance.CurrentLocalInput : Simulation.Instance.CurrentRemoteInput;
		prevInput = entity.playerOwner == 1 ? Simulation.Instance.PrevLocalInput : Simulation.Instance.PrevRemoteInput;

		// check jump
		if (CheckBufferedAction(() => isGrounded && currentInput.Has(Input.Jump), () => currentInput.Has(Input.Jump) && !prevInput.Has(Input.Jump), entity, EntityVar.BufferedJump, jumpBufferFrames))
			entity.velocity.y = jumpForce;

		if (!currentFrame.frameFlags.HasFlag(FrameFlags.FreezeFall))
			UpdateFalling(ref entity);

		UpdateMovementInput(ref entity);

		UpdatePosition(ref entity);

		return entity;
	}

	protected bool CheckBufferedAction(System.Func<bool> condition, System.Func<bool> activator, Simulation.Entity entity, EntityVar bufferVariable, int maxBufferFrames) {
		int bufferFrames = Simulation.Instance.CustomEntityData[entity.ID][bufferVariable];

		if (activator.Invoke())
			bufferFrames = maxBufferFrames + 1;

		if (bufferFrames > 0) { // if we want to do it
			// if we can do it then go!
			if (condition.Invoke())
				return true;

			// if we can't, decrement the buffer frames for later
			Simulation.Instance.SetCustomData(entity, bufferVariable, bufferFrames - 1);
		}

		return false;
	}

	protected virtual void UpdateFalling(ref Simulation.Entity entity) {
		if (isGrounded)
			return;

		entity.velocity.y -= (currentInput.Has(Input.Jump) && entity.velocity.y > 0) ? jumpDecel : fallAccel;
	}

	protected virtual void UpdateMovementInput(ref Simulation.Entity entity) {
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

	protected virtual void UpdatePosition(ref Simulation.Entity entity) {
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
}