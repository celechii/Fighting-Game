using System;

[System.Serializable]
public struct CustomData<TEnum> : INameableElement where TEnum : struct, IConvertible {
	public TEnum variable;
	public int defaultValue;
	public bool isBool;

	public string GetArrayElementName(int index) {
		return $"{variable.MakeEnumReadable()}: {(isBool ? (defaultValue == 1 ? "True" : "False") : defaultValue)}";
	}

	public void RegisterDefaultValue(Entity entity) {
		Simulation.Instance.SetCustomData(entity, Convert.ToInt32(variable), defaultValue);
	}
}