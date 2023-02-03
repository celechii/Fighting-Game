using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EntityData/Fireball")]
public class FireballEntityData : EntityData {

	public override Entity ProcessHurtBoxes(Entity entity, List<Simulation.HitData> hitBoxOverlaps) {
		return base.ProcessHurtBoxes(entity, hitBoxOverlaps);
	}

	public override Entity UpdateBehaviour(Entity entity) {
		entity.position += entity.velocity;
		return entity;
	}
}