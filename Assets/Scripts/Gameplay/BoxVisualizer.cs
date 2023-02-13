using System.Collections.Generic;
using UnityEngine;

public class BoxVisualizer : MonoBehaviour {

	private static BoxVisualizer instance;

	[SerializeField]
	private bool showBoxes;
	[Range(0, 1)]
	[SerializeField]
	private float alpha;
	[SerializeField]
	private Color pushBoxColour = Color.yellow;
	[SerializeField]
	private Color hitBoxColour = Color.red;
	[SerializeField]
	private Color hurtBoxColour = Color.green;
	[SerializeField]
	private SpriteRenderer boxPrefab;

	private Transform visualizerTrans;

	private List<Box> pushBoxes = new();
	private List<Box> hitBoxes = new();
	private List<Box> hurtBoxes = new();

	private void Awake() {
		instance = this;
		visualizerTrans = transform;
	}

	public static void RegisterPushBox(Box pushBox, Vector2Int offset) => RegisterBox(pushBox, offset, instance.pushBoxes);
	public static void RegisterHitBox(Box hitBox, Vector2Int offset) => RegisterBox(hitBox, offset, instance.hitBoxes);
	public static void RegisterHurtBox(Box hurtBox, Vector2Int offset) => RegisterBox(hurtBox, offset, instance.hurtBoxes);

	private static void RegisterBox(Box box, Vector2Int offset, List<Box> boxList) {
		box.position += offset;
		boxList.Add(box);
	}

	private void Update() {
		ClearLists();
	}

	private void LateUpdate() {
		if (!showBoxes)
			ClearLists();

		int totalBoxes = pushBoxes.Count + hitBoxes.Count + hurtBoxes.Count;

		// instantiate neew boxes
		for (int i = visualizerTrans.childCount; i < totalBoxes; i++)
			Instantiate(boxPrefab, visualizerTrans);

		// hide extra
		if (visualizerTrans.childCount > 0)
			for (int i = totalBoxes; i < visualizerTrans.childCount; i++)
				visualizerTrans.GetChild(i).gameObject.SetActive(false);

		foreach (Box box in pushBoxes) {
			totalBoxes--;
			ShowBoxAt(box, pushBoxColour, totalBoxes);
		}
		foreach (Box box in hitBoxes) {
			totalBoxes--;
			ShowBoxAt(box, hitBoxColour, totalBoxes);
		}
		foreach (Box box in hurtBoxes) {
			totalBoxes--;
			ShowBoxAt(box, hurtBoxColour, totalBoxes);
		}

	}

	private void ClearLists() {
		pushBoxes.Clear();
		hitBoxes.Clear();
		hurtBoxes.Clear();
	}

	private void ShowBoxAt(Box box, Color colour, int index) {
		Transform targetTrans = visualizerTrans.GetChild(index);
		SpriteRenderer spriteRenderer = targetTrans.GetComponent<SpriteRenderer>();
		colour.a = alpha;

		targetTrans.localPosition = Simulation.Instance.GetWorldVector(box.position);
		spriteRenderer.size = Simulation.Instance.GetWorldVector(box.size);
		spriteRenderer.color = colour;
		targetTrans.gameObject.SetActive(true);
	}
}