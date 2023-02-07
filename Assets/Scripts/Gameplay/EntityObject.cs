using NaughtyAttributes;
using UnityEngine;

public class EntityObject : MonoBehaviour {
	public int EntityID { get; private set; } = -1;

	[ReadOnly]
	[SerializeField]
	private Entity latestEntity;

	private Transform entityTrans;
	private SpriteRenderer spriteRenderer;

	private void Awake() {
		entityTrans = transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void UpdateEntity(Entity entity) {
		EntityID = entity.ID;
		AnimationData.FrameData currentFrame = entity.GetCurrentFrameData();
		spriteRenderer.sprite = currentFrame.sprite;
		spriteRenderer.flipX = !entity.isFacingRight;
		entityTrans.localPosition = Simulation.Instance.GetWorldVector(entity.position + currentFrame.spriteOffset);

		#if UNITY_EDITOR
		latestEntity = entity;
		gameObject.name = $"{ObjectRef.GetObject<ScriptableObject>(entity.entityHash).name} ({entity.ID})";
		#endif
	}

	private void OnDrawGizmos() {
		// draw entity position
		Gizmos.color = Color.white;
		float lineLength = 1f;
		Gizmos.DrawRay(transform.localPosition, Vector2.up * lineLength);
		Gizmos.DrawRay(transform.localPosition, Vector2.down * lineLength);
		Gizmos.DrawRay(transform.localPosition, Vector2.left * lineLength);
		Gizmos.DrawRay(transform.localPosition, Vector2.right * lineLength);

		// draw hitboxes/hurtboxes		
		Color pushBoxColour = Color.yellow;
		Color hurtBoxColour = Color.green;
		Color hitBoxColour = Color.red;
		pushBoxColour.a = 0.2f;
		hurtBoxColour.a = 0.2f;
		hitBoxColour.a = 0.2f;

		AnimationData.FrameData currentAnimFrame = latestEntity.GetCurrentFrameData();

		// draw hurtboxes
		Gizmos.color = pushBoxColour;
		Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredBox(currentAnimFrame.pushBox).position), Simulation.Instance.GetWorldVector(currentAnimFrame.pushBox.size));
			
		// draw hurtboxes
		Gizmos.color = hurtBoxColour;
		foreach (Box hurtBox in currentAnimFrame.hurtBoxes)
			Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredBox(hurtBox).position), Simulation.Instance.GetWorldVector(hurtBox.size));

		// draw hitboxes
		Gizmos.color = hitBoxColour;
		foreach (Box hitBox in currentAnimFrame.hitBoxes)
			Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredBox(hitBox).position), Simulation.Instance.GetWorldVector(hitBox.size));
	}
}