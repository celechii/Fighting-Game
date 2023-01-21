using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EntityManager : MonoBehaviour {

	private List<EntityObject> activeEntities = new();
	private ObjectPool<EntityObject> entityPool;

	[SerializeField]
	private EntityObject entityPrefab;

	private Transform managerTrans;

	private void Awake() {
		managerTrans = transform;

		entityPool = new(
			() => Instantiate(entityPrefab, managerTrans),
			OnGetPooledEntity,
			OnReleasePooledEntity,
			x => Destroy(x)
		);
	}

	private void OnGetPooledEntity(EntityObject entity) {
		entity.gameObject.SetActive(true);
		activeEntities.Add(entity);
	}

	private void OnReleasePooledEntity(EntityObject entity) {
		entity.gameObject.SetActive(false);
		activeEntities.Remove(entity);
	}

	public void UpdateEntities(List<Simulation.Entity> entities) {
		// assign all 
		for (int i = 0; i < entities.Count; i++) {
			EntityObject entityObject;
			if (activeEntities.Count > i)
				entityObject = activeEntities[i];
			else
				entityObject = entityPool.Get(); // creates a new entity object if there isn't one
			entityObject.UpdateEntity(entities[i]);
		}
		
		// disable all extra entities
		for (int i = entities.Count; i < activeEntities.Count;)
			entityPool.Release(activeEntities[i]);
	}
}