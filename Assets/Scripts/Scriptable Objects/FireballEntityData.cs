using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EntityData/Fireball")]
public class FireballEntityData : EntityData {

	public override Simulation.Entity ProcessHurtBoxes(Simulation.Entity entity, List<Simulation.HitBoxOverlap> hitBoxOverlaps) {
		return base.ProcessHurtBoxes(entity, hitBoxOverlaps);
	}

	public override Simulation.Entity UpdateBehaviour(Simulation.Entity entity) {
		entity.position += entity.velocity;
		return entity;
	}
}