using NaughtyAttributes;
using UnityEngine;

public class EntityObject : MonoBehaviour {
	public int EntityID { get; private set; } = -1;

	[ReadOnly]
	[SerializeField]
	private Simulation.Entity latestEntity;

	private Transform entityTrans;
	private SpriteRenderer spriteRenderer;

	private void Awake() {
		entityTrans = transform;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void UpdateEntity(Simulation.Entity entity) {
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
		Color hurtboxColour = Color.yellow;
		Color hitboxColour = Color.red;
		hurtboxColour.a = 0.2f;
		hitboxColour.a = 0.2f;

		AnimationData.FrameData currentAnimFrame = latestEntity.GetCurrentFrameData();

		// draw hurtboxes
		Gizmos.color = hurtboxColour;
		foreach (Simulation.VBox hurtbox in currentAnimFrame.hurtBoxes)
			Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredVBox(hurtbox).position), Simulation.Instance.GetWorldVector(hurtbox.size));

		// draw hitboxes
		Gizmos.color = hitboxColour;
		foreach (Simulation.VBox hitbox in currentAnimFrame.hitBoxes)
			Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredVBox(hitbox).position), Simulation.Instance.GetWorldVector(hitbox.size));
	}
}