using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler {

	private Selectable selectable;

	private void Awake() {
		selectable = GetComponent<Selectable>();
	}

	public void OnPointerEnter(PointerEventData eventData) {
		if (selectable.isActiveAndEnabled && selectable.interactable)
			selectable.Select();
	}
}