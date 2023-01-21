using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Frame Command/Damage")]
public class DamageCommand : FrameCommand, IComparable<DamageCommand> {
	public enum KnockbackDirection {
		Up,
		BackAndUp,
		Back,
		BackAndDown,
		Down
	}

	public int priority;

	[MinValue(0)]
	public int damage;
	[MinValue(0)]
	public int stunFrames;

	[Header("Knockback")]
	[Label("Direction")]
	public KnockbackDirection knockbackDirection = KnockbackDirection.Back;
	public int force;

	public Vector2Int GetKnockbackVector(bool isFacingRight) {
		int diagonalComponentMagnitude = (force * 7071) / 10000;
		
		Vector2Int direction = knockbackDirection switch
		{
			KnockbackDirection.Up => Vector2Int.up * force,
			KnockbackDirection.BackAndUp => Vector2Int.one * diagonalComponentMagnitude,
			KnockbackDirection.Back => Vector2Int.right * force,
			KnockbackDirection.BackAndDown => new Vector2Int(diagonalComponentMagnitude, -diagonalComponentMagnitude),
			KnockbackDirection.Down => Vector2Int.down * force,
			_ => throw new System.NotImplementedException("oh shit i dont have a direction for " + knockbackDirection)
		};
		
		if (!isFacingRight)
			direction.x *= -1;
			
		return direction;
	}

	public int CompareTo(DamageCommand other) => -priority.CompareTo(other.priority);
}