using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "EntityData/Basic Entity")]
public class EntityData : ScriptableObject {
	
	public int Hash => ObjectRef.GetHash(this);

	public EntityType type;
	[NameElements]
	public CustomData[] customData;
	public AnimationData initialAnimation;

	public virtual Entity ProcessHurtBoxes(Entity entity, List<Simulation.HitData> hitBoxOverlaps) => entity;
	public virtual Entity UpdateBehaviour(Entity entity) => entity;

	[System.Serializable]
	public struct CustomData : INameableElement {
		public EntityVar variable;
		public int value;
		[SerializeField]
		private bool isBoolean;

		public string GetArrayElementName(int index) => $"{Utils.MakeEnumReadable(variable)}: {(isBoolean ? (value == 0 ? "false" : "true") : value)}";
	}
}