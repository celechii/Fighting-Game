using UnityEngine;

[System.Serializable]
public class Spring {

	public float acceleration = 5;
	[Range(0, 1)]
	public float attenuation = 0.9f;
	[HideInInspector]
	public float Target = 0;

	public float Position { get; private set; }

	public float Velocity { get; private set; }

	public void Update() => Update(Time.deltaTime);

	public void Update(float deltaTime) {
		float towardsTarget = Target - Position;
		Velocity += acceleration * towardsTarget;
		Velocity *= attenuation;
		Position += Velocity * deltaTime;
	}

	public void Push(float force) {
		Velocity += force;
	}
}