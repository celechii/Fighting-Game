using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Player FSM")]
public class PlayerEntityController : EntityFSM {

	[Header("Player Data")]
	[NameElements]
	[SerializeField]
	private CustomData<CustomPlayerData>[] defaultData;

	private enum CustomPlayerData {
		Health,
		HitStunFrames,
		HitStopFrames
	}

	private void Reset() {
		CustomPlayerData[] values = (CustomPlayerData[])System.Enum.GetValues(typeof(CustomPlayerData));
		defaultData = new CustomData<CustomPlayerData>[values.Length];
		for (int i = 0; i < defaultData.Length; i++)
			defaultData[i].variable = values[i];
	}

	public override Entity Initialize(Entity entity) {
		foreach (CustomData<CustomPlayerData> data in defaultData)
			data.RegisterDefaultValue(entity);
		return base.Initialize(entity);
	}
}