using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class ExplicitNavigation : MonoBehaviour {
	
	private enum NavigationDirection {
		Vertical,
		Horizontal
	}

	[SerializeField]
	private NavigationDirection navigationDirection;
	[SerializeField]
	private bool wrapAround;

	public Selectable[] selectables;
	
	[Button]
	public void GenerateExplicitNavigation() {
		for (int i = 0; i < selectables.Length; i++) {
			
			if (!selectables[i].isActiveAndEnabled || !selectables[i].interactable)
				continue;

			Navigation nav = selectables[i].navigation;
			nav.mode = Navigation.Mode.Explicit;

			Selectable nextSelectable = null;
			
			int checkCount = wrapAround ? selectables.Length : selectables.Length - i;
			for (int j = 1; j < checkCount; j++) {
				nextSelectable = selectables[(i + j) % selectables.Length];
				if (nextSelectable.isActiveAndEnabled && nextSelectable.interactable)
					break;
			}
			
			if (navigationDirection == NavigationDirection.Vertical) {
				nav.selectOnDown = nextSelectable;
				
				if (nextSelectable != null) {
					Navigation nextNav = nextSelectable.navigation;
					nextNav.selectOnUp = selectables[i];
					nextSelectable.navigation = nextNav;
				}
			} else if (navigationDirection == NavigationDirection.Horizontal) {
				nav.selectOnRight = nextSelectable;
				
				if (nextSelectable != null) {
					Navigation nextNav = nextSelectable.navigation;
					nextNav.selectOnLeft = selectables[i];
					nextSelectable.navigation = nextNav;
				}
			}

			selectables[i].navigation = nav;
		}
	}
	
	[Button]
	private void FindSelectables() {
		selectables = GetComponentsInChildren<Selectable>(true);
	}
}