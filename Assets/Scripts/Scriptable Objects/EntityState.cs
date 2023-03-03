using UnityEngine;

public abstract class EntityState : ScriptableObject {

	public AnimationData.FrameData CurrentFrame;

	/// <summary>
	/// Called when either the FSM or another entity has defined this as the state to enter.
	/// </summary>
	/// <param name="entity">The entity in it's current state.</param>
	/// <returns>The modified entity.</returns>
	public abstract Entity OnStateEnter(Entity entity);

	/// <summary>
	/// Called during the collision processing update loop. This should check if any collision data is relevant to the state and if so, determine what to do with it.
	/// </summary>
	/// <param name="entity">The entity in it's current state.</param>
	/// <param name="transitionToState">A state to be set in the event ProcessCollisions determines the entity should switch states.</param>
	/// <returns>The modified entity.</returns>
	public abstract Entity ProcessCollisions(Entity entity, ref EntityState transitionToState);

	/// <summary>
	/// Called during the behaviour update loop. This is where states should do their regular behaviour processing!
	/// </summary>
	/// <param name="entity">The entity in it's current state.</param>
	/// <param name="transitionToState">A state to be set in the event ProcessCollisions determines the entity should switch states.</param>
	/// <returns>The modified entity.</returns>
	public abstract Entity UpdateBehaviour(Entity entity, ref EntityState transitionToState);
}