using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Basic FSM")]
public class EntityFSM : ScriptableObject {
	
	[Header("FSM")]
	public EntityType Type;

	[SerializeField]
	private List<EntityState> allStates = new();
	private EntityState transitionState;

	public virtual Entity Initialize(Entity entity) {
		entity.stateIndex = 0;
		entity = allStates[0].OnStateEnter(entity);
		transitionState = null;
		return entity;
	}
	
	/// <summary>
	/// Calls the current state's ProcessCollisions() method and transitions to a new state if required.
	/// </summary>
	/// <param name="entity"></param>
	/// <param name="hitBoxOverlaps"></param>
	/// <returns></returns>
	public virtual Entity ProcessHurtBoxes(Entity entity, List<Simulation.HitData> hitBoxOverlaps) {
		allStates[entity.stateIndex].CurrentFrame = entity.GetCurrentFrameData();
		entity = allStates[entity.stateIndex].ProcessCollisions(entity, ref transitionState);
		if (transitionState != null)
			entity = TransitionToState(entity);
		return entity;
	}

	/// <summary>
	/// Calls UpdateBehaviour on the current state, then transitions to a new one if required. 
	/// </summary>
	public virtual Entity UpdateBehaviour(Entity entity) {
		allStates[entity.stateIndex].CurrentFrame = entity.GetCurrentFrameData();
		entity = allStates[entity.stateIndex].UpdateBehaviour(entity, ref transitionState);
		if (transitionState != null)
			entity = TransitionToState(entity);
		return entity;
	}

	private Entity TransitionToState(Entity entity) {
		entity.stateIndex = allStates.IndexOf(transitionState);
		if (entity.stateIndex == -1)
			throw new System.Exception($"{name} tried to transition to state {transitionState.name} but doesn't have a reference for it!");
		transitionState = null;
		entity = allStates[0].OnStateEnter(entity);
		return entity;
	}
}