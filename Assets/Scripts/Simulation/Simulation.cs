using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Simulation : MonoBehaviour {
	public static Simulation Instance { get; private set; }
	public const int FrameRate = 60;

	public List<Input> LocalInputHistory = new(FrameRate * 60 * 10); // 10 minutes of initial input history
	public List<Input> RemoteInputHistory = new(FrameRate * 60 * 10); // 10 minutes of initial input history

	public Input CurrentLocalInput => LocalInputHistory[CurrentFrame];
	public Input PrevLocalInput => LocalInputHistory[Mathf.Max(0, CurrentFrame - 1)];
	public Input CurrentRemoteInput => RemoteInputHistory[CurrentFrame];
	public Input PrevRemoteInput => RemoteInputHistory[Mathf.Max(0, CurrentFrame - 1)];

	public bool FrameByFrame;
	[Tooltip("How many frames should pass before the input should be processed?")]
	[SerializeField]
	private int inputBufferFrames = 3;
	[Tooltip("How many seconds of history should the simulation keep track of?")]
	[SerializeField]
	private int maxRollbackFrames = 10;
	[Tooltip("This is how many simulation units there are in 1 Unity unit.")]
	public int simulationScale = 128;
	[SerializeField]
	private int startSeparationDistance;
	[Space]
	[SerializeField]
	private EntityManager entityManager;
	public bool isLocalPlayer1 = true;

	[ShowNativeProperty]
	public int CurrentFrame { get; private set; } = 0;
	[ShowNativeProperty]
	public int TargetFrame { get; private set; } = 0;
	public Dictionary<int, Dictionary<EntityVar, int>> CustomEntityData = new();

	[SerializeField]
	private PlayerEntityData player1EntityData;
	[SerializeField]
	private PlayerEntityData player2EntityData;

	private List<Entity> entities = new();

	private InputCheck inputCheck;
	private GameStateHistory stateHistory;
	private bool isRunning;
	private float runtime;

	public Vector2 GetWorldVector(Vector2Int simulationPosition) => (Vector2)simulationPosition / simulationScale;

	private void Awake() {
		Instance = this;
		inputCheck = GetComponent<InputCheck>();
	}

	private void OnDestroy() {
		Instance = null;
	}

	[Button("Start Game", NaughtyAttributes.EButtonEnableMode.Playmode)]
	public void SetupGame() {
		// create history for however many seconds we need
		stateHistory = new GameStateHistory(maxRollbackFrames);

		LocalInputHistory.Clear();
		RemoteInputHistory.Clear();

		entities.Clear();
		CreateEntity(player1EntityData, isLocalPlayer1 ? 1 : 2);
		CreateEntity(player2EntityData, isLocalPlayer1 ? 2 : 1);

		Entity player1 = entities[0];
		Entity player2 = entities[1];
		player1.position.Set(-startSeparationDistance, 0);
		player2.position.Set(startSeparationDistance, 0);
		player2.isFacingRight = false;
		entities[0] = player1;
		entities[1] = player2;

		CurrentFrame = 0;
		TargetFrame = 0;

		isRunning = true;
		runtime = 0;

		RenderFrame();
	}

	public int CreateEntity(EntityData entityData, int playerOwner) {
		// create new entity
		Entity entity = new Entity(entityData, playerOwner);
		entities.Add(entity);

		// start tracking its custom data
		if (entityData.customData != null) {
			Dictionary<EntityVar, int> customData = new();
			foreach (EntityData.CustomData variable in entityData.customData)
				customData.Add(variable.variable, variable.value);
			CustomEntityData.Add(entity.ID, customData);
		}

		return entity.ID;
	}

	public void SetCustomData(Entity entity, EntityVar variable, bool value) => SetCustomData(entity, variable, value ? 1 : 0);
	public void SetCustomData(Entity entity, EntityVar variable, int value) {
		if (!CustomEntityData[entity.ID].ContainsKey(variable))
			CustomEntityData[entity.ID].Add(variable, 0);
		CustomEntityData[entity.ID][variable] = value;
	}

	public void DestroyEntity(int entityID) {
		entities.RemoveAt(entities.FindIndex(x => x.ID == entityID));
		CustomEntityData.Remove(entityID);
	}

	public bool TryGetCustomData(int entityID, EntityVar variable, out int data) {
		data = 0;
		if (CustomEntityData.ContainsKey(entityID) && CustomEntityData[entityID].ContainsKey(variable)) {
			data = CustomEntityData[entityID][variable];
			return true;
		}
		return false;
	}

	private void Update() {
		if (!isRunning)
			return;

		if (!FrameByFrame)
			TargetFrame = TimeToFrame(runtime);

		ReadAndQueueInput();

		// TODO: check for previous input and resimulate
		while (CurrentFrame < TargetFrame)
			Simulate();

		RenderFrame();
		runtime += Time.deltaTime;
	}

	private void ReadAndQueueInput() {
		// create input frames at least up to target + buffer frames
		while (LocalInputHistory.Count <= TargetFrame + inputBufferFrames)
			LocalInputHistory.Add(new());
		while (RemoteInputHistory.Count <= TargetFrame + inputBufferFrames)
			RemoteInputHistory.Add(new());
		
		// from the next frame until the target frame, add current input
		for (int i = CurrentFrame + 1; i <= TargetFrame; i++)
			LocalInputHistory[i + inputBufferFrames] = inputCheck.Current;
	}

	private void RenderFrame() {
		entityManager.UpdateEntities(entities);
	}

	private int TimeToFrame(float timeSeconds) => (int)(timeSeconds * 60f);

	private void RevertToFrame(int frameNumber) {
		GameState previousState = stateHistory.RevertToFrame(frameNumber);
		CurrentFrame = previousState.frame;
		inputCheck.Current = LocalInputHistory[CurrentFrame];
		entities = new List<Entity>(previousState.entities);
		CustomEntityData = previousState.customEntityData;
	}

	/// <summary>
	/// Runs the simulation for 1 frame
	/// </summary>
	public void Simulate() {
		// record previous frame
		stateHistory.RecordState(new GameState(CurrentFrame, entities.ToArray(), CustomEntityData));

		CurrentFrame++;

		ProcessHurtBoxes();
		UpdateEntityBehaviours();
		UpdateEntityAnimations();
	}

	private void ProcessHurtBoxes() {
		Dictionary<int, List<HitBoxOverlap>> hitBoxOverlaps = new();

		// sort entities
		List<Entity> player1Entities = new();
		List<Entity> player2Entities = new();
		foreach (Entity entity in entities) {
			if (entity.playerOwner != 1)
				player2Entities.Add(entity);
			if (entity.playerOwner != 2)
				player1Entities.Add(entity);
		}

		// generate overlaps
		GenerateOverlaps(player1Entities, player2Entities);
		GenerateOverlaps(player2Entities, player1Entities);

		// update every entity that has had collision
		for (int i = 0; i < entities.Count; i++) {
			if (!hitBoxOverlaps.ContainsKey(entities[i].ID))
				continue;

			EntityData entityData = ObjectRef.GetObject<EntityData>(entities[i].entityHash);
			hitBoxOverlaps[entities[i].ID].Sort();
			entities[i] = entityData.ProcessHurtBoxes(entities[i], hitBoxOverlaps[entities[i].ID]);
		}

		void GenerateOverlaps(List<Entity> hurtBoxEntities, List<Entity> hitBoxEntities) {
			foreach (Entity hurtBoxEntity in hurtBoxEntities) {
				foreach (VBox hurtBox in hurtBoxEntity.GetCurrentFrameData().hurtBoxes) {
					foreach (Entity hitBoxEntity in hitBoxEntities) {
						AnimationData.FrameData hitBoxFrameData = hitBoxEntity.GetCurrentFrameData();
						DamageCommand damage;
						if (!hitBoxFrameData.TryGetFrameCommand(out damage))
							continue;

						if (hurtBox.OverlapAnyVBoxes(hitBoxFrameData.hitBoxes)) {
							if (!hitBoxOverlaps.ContainsKey(hurtBoxEntity.ID))
								hitBoxOverlaps.Add(hurtBoxEntity.ID, new());
							hitBoxOverlaps[hurtBoxEntity.ID].Add(new HitBoxOverlap(hurtBoxEntity.ID, damage, hitBoxEntity.isFacingRight));
						}
					}
				}
			}
		}
	}

	private void UpdateEntityBehaviours() {
		for (int i = 0; i < entities.Count; i++) {
			EntityData entityData = ObjectRef.GetObject<EntityData>(entities[i].entityHash);
			entities[i] = entityData.UpdateBehaviour(entities[i]);
		}
	}

	private void UpdateEntityAnimations() {
		foreach (Entity entity in entities)
			entity.NextAnimationFrame();
	}

	private void OnGUI() {
		GUIStyle textStyle = new GUIStyle(GUI.skin.label);
		textStyle.fontSize = 25;
		GUILayout.Label($"frame: {CurrentFrame} ({stateHistory?.FramesAvailable ?? 0} recorded)", textStyle);
		GUILayout.Label($"FPS: {Mathf.RoundToInt(1f / Time.unscaledDeltaTime)}", textStyle);

		if (FrameByFrame) {
			GUILayout.BeginArea(new Rect((Screen.width / 2f) - 400, 50, 800, 50));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Prev", GUILayout.Width(69)) && stateHistory.FramesAvailable > 0) {
				int newFrame = Mathf.Max(0, TargetFrame - 1);
				TargetFrame = newFrame;
				RevertToFrame(newFrame);
				RenderFrame();
			}

			if (GUILayout.Button("Next", GUILayout.Width(69))) {
				TargetFrame++;
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}

	public struct GameState {
		public int frame;
		public Entity[] entities;
		public Dictionary<int, Dictionary<EntityVar, int>> customEntityData;

		public GameState(int frame, Entity[] entities, Dictionary<int, Dictionary<EntityVar, int>> customData) {
			this.frame = frame;
			this.entities = entities;

			customEntityData = new();
			foreach (KeyValuePair<int, Dictionary<EntityVar, int>> data in customData)
				customEntityData.Add(data.Key, new(data.Value));
		}
	}

	[System.Serializable]
	public struct Entity {
		public static int entityID;
		[ReadOnly]
		public int ID;

		public int playerOwner; // 0 for no owner, 1 for player 1, 2 for player 2
		public int entityHash;
		public int animationHash;
		public int animationFrame;
		public Vector2Int position;
		public Vector2Int velocity;
		public bool isFacingRight;

		public Entity(EntityData entityData, int playerOwner) {
			ID = entityID;
			entityID++;

			this.playerOwner = playerOwner;
			this.entityHash = ObjectRef.GetHash(entityData);
			this.animationHash = ObjectRef.GetHash(entityData.initialAnimation);

			animationFrame = 0;
			position = Vector2Int.zero;
			velocity = Vector2Int.zero;
			isFacingRight = true;
		}

		public AnimationData.FrameData GetCurrentFrameData() {
			return ObjectRef.GetObject<AnimationData>(animationHash).GetFrame(animationFrame);
		}

		public VBox GetMirroredVBox(VBox vBox) {
			if (isFacingRight)
				return vBox;
			else {
				vBox.position = new Vector2Int(-vBox.position.x, vBox.position.y);
				return vBox;
			}
		}

		public void SetAnimation(AnimationData animationData) {
			animationHash = ObjectRef.GetHash(animationData);
		}

		public void NextAnimationFrame() {
			animationFrame = ObjectRef.GetObject<AnimationData>(animationHash).GetNextFrameIndex(animationFrame);
		}
	}

	[System.Serializable]
	public struct VBox {
		[HideInInspector]
		public int ownerEntityHash;
		public Vector2Int position;
		public Vector2Int size;

		public bool OverlapAnyVBoxes(IList<VBox> vBoxes) {
			foreach (VBox hurtBox in vBoxes)
				if (OverlapVBox(hurtBox))
					return true;
			return false;
		}

		public bool OverlapVBox(VBox other) {
			Vector2Int posDif = (other.position - position) * 2;
			Vector2Int combinedSize = other.size + size;
			return Mathf.Abs(posDif.x) < combinedSize.x && Mathf.Abs(posDif.y) < combinedSize.y;
		}
	}

	public struct HitBoxOverlap {
		public int priority;
		public int entityHitID;
		public int damage;
		public Vector2Int knockback;
		public int stunFrames;

		public HitBoxOverlap(int entityID, DamageCommand damageCommand, bool isFacingRight) {
			entityHitID = entityID;
			priority = damageCommand.priority;
			damage = damageCommand.damage;
			knockback = damageCommand.GetKnockbackVector(isFacingRight);
			stunFrames = damageCommand.stunFrames;
		}
	}
}