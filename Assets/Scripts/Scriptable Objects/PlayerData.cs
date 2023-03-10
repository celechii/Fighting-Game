using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player")]
public class PlayerData : ScriptableObject {
	[Header("Character Data")]
	public string DisplayName;

	[Header("Movement")]
	public int GroundAccel = 1;
	public int GroundDecel = 1;
	public int AirAccel = 1;
	public int AirDecel = 1;
	
	[Header("Gravity")]
	public int DefaultGravity = 5;
	public int JumpGravity = 4;
}
