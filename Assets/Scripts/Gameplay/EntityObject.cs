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
		Color hurtboxColour = Color.yellow;
		Color hitboxColour = Color.red;
		hurtboxColour.a = 0.2f;
		hitboxColour.a = 0.2f;

		AnimationData.FrameData currentAnimFrame = latestEntity.GetCurrentFrameData();

		// draw hurtboxes
		Gizmos.color = hurtboxColour;
		foreach (Box hurtbox in currentAnimFrame.GetBoxes(Box.BoxType.HurtBox))
			Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredBox(hurtbox).position), Simulation.Instance.GetWorldVector(hurtbox.size));

		// draw hitboxes
		Gizmos.color = hitboxColour;
		foreach (Box hitbox in currentAnimFrame.GetBoxes(Box.BoxType.Hitbox))
			Gizmos.DrawCube(Simulation.Instance.GetWorldVector(latestEntity.position + latestEntity.GetMirroredBox(hitbox).position), Simulation.Instance.GetWorldVector(hitbox.size));
	}
}