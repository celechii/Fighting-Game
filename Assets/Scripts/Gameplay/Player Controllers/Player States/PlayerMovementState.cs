using UnityEngine;

[CreateAssetMenu(menuName = "Entity State/Player/Movement")]
public class PlayerMovementState : PlayerEntityState {
	[Header("Movement")]
	[SerializeField]
	private int maxSpeed;

	[Header("Animations")]
	[SerializeField]
	private AnimationData idleAnim;
	[SerializeField]
	private AnimationData runAnim;

	private Input currentInput;
	private Input prevInput;

	public override Entity OnStateEnter(Entity entity) {
		currentInput = Simulation.Instance.GetCurrentInput(entity);
		prevInput = Simulation.Instance.GetPrevInput(entity);

		SetAppropriateAnim(ref entity);
		return entity;
	}

	public override Entity UpdateBehaviour(Entity entity, ref EntityState transitionToState) {
		currentInput = Simulation.Instance.GetCurrentInput(entity);
		prevInput = Simulation.Instance.GetPrevInput(entity);
		
		bool isGrounded = Simulation.IsPushBoxGrounded(CurrentFrame.pushBox, entity.position.y);
		
		int accel = isGrounded ? playerData.GroundAccel : playerData.AirAccel;
		int decel = isGrounded ? playerData.GroundDecel : playerData.AirDecel;
		
		int movementInput = (currentInput.Has(Input.MoveLeft) ? -1 : 0) + (currentInput.Has(Input.MoveRight) ? 1 : 0);
		
		if (movementInput != 0) {
			entity.velocity.x = Mathf.Clamp(entity.velocity.x + (accel * movementInput), -maxSpeed, maxSpeed);
		} else {
			if (Mathf.Abs(entity.velocity.x) > 0)
				entity.velocity.x -= Mathf.Min(Mathf.Abs(entity.velocity.x), decel) * (int)Mathf.Sign(entity.velocity.x);
		}
		
		entity = base.UpdateBehaviour(entity, ref transitionToState);
		
		SetAppropriateAnim(ref entity);
		return entity;
	}

	// pick animation depending on velocity and grounded state
	private void SetAppropriateAnim(ref Entity entity) {
		if (Sign(entity.velocity.x) == Sign((currentInput.Has(Input.MoveLeft) ? -1 : 0) + (currentInput.Has(Input.MoveRight) ? 1 : 0)))
			entity.SetAnimation(runAnim);
		else
			entity.SetAnimation(idleAnim);
	}

	private int Sign(int value) => value > 0 ? 1 : value < 0 ? -1 : 0;
}