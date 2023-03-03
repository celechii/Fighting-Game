using UnityEngine;

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
		entity = base.UpdateBehaviour(entity, ref transitionToState);
		
		currentInput = Simulation.Instance.GetCurrentInput(entity);
		prevInput = Simulation.Instance.GetPrevInput(entity);
		
		
		
		SetAppropriateAnim(ref entity);
		return entity;
	}

	// pick animation depending on velocity and grounded state
	private void SetAppropriateAnim(ref Entity entity) {
		if (entity.velocity.x == 0 && !currentInput.Has(Input.MoveLeft) && !currentInput.Has(Input.MoveRight))
			entity.SetAnimation(idleAnim);
		else
			entity.SetAnimation(runAnim);
	}
}