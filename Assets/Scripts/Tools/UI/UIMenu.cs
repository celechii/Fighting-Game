using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class UIMenu : MonoBehaviour {

	public enum FocusSelection {
		None,
		LastSelected,
		FirstSelected
	}

	private static List<UIMenu> MenuStack = new();

	public static UIMenu FocusedMenu => MenuStack.Count > 0 ? MenuStack[ ^ 1] : null;
	public static event Action NewMenuFocused = delegate {};

	public GameObject FirstSelected;
	public FocusSelection SelectOnFocus = FocusSelection.None;

	[Foldout("Open/Close")]
	[SerializeField]
	private UnityEvent onOpen;
	[Foldout("Open/Close")]
	[SerializeField]
	private UnityEvent onClose;

	[Foldout("Focus")]
	[SerializeField]
	private UnityEvent onFocus;
	[Foldout("Focus")]
	[SerializeField]
	private UnityEvent onLoseFocus;

	public bool IsFocused { get; private set; }
	public bool IsOpen { get; private set; }

	private Canvas canvas;
	private GraphicRaycaster graphicRaycaster;
	private GameObject lastSelectedGameObject;

	private void Awake() {
		canvas = GetComponent<Canvas>();
		if (canvas != null)
			canvas.enabled = false;

		graphicRaycaster = GetComponent<GraphicRaycaster>();
		if (graphicRaycaster != null)
			graphicRaycaster.enabled = false;
	}

	private void OnEnable() {
		NewMenuFocused += OnNewMenuFocused;
	}

	private void OnDisable() {
		NewMenuFocused -= OnNewMenuFocused;
	}

	private void Update() {
		if (IsFocused)
			lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
	}

	private void OnNewMenuFocused() {
		bool isNowFocused = MenuStack.Count > 0 && MenuStack[ ^ 1] == this;
		if (graphicRaycaster != null)
			graphicRaycaster.enabled = isNowFocused;

		if (isNowFocused != IsFocused) {
			IsFocused = isNowFocused;

			if (IsFocused) {
				onFocus.Invoke();
				if (SelectOnFocus == FocusSelection.LastSelected)
					EventSystem.current.SetSelectedGameObject(lastSelectedGameObject ?? FirstSelected);
				else if (SelectOnFocus == FocusSelection.FirstSelected)
					EventSystem.current.SetSelectedGameObject(FirstSelected);
			} else {
				onLoseFocus.Invoke();
			}
		}
	}

	[Button]
	public void Open() {
		if (!IsOpen) {
			MenuStack.Add(this);
			OnOpen();
			NewMenuFocused.Invoke();
		} else {
			StealFocus();
		}
	}

	[Button]
	public void Switch() {
		if (!IsFocused) {
			FocusedMenu.OnClose();
			MenuStack.Remove(FocusedMenu);
			Open();
		}
	}

	[Button]
	public void Close() {
		if (IsOpen) {
			MenuStack.Remove(this);
			OnClose();
			NewMenuFocused.Invoke();
		}
	}

	[Button]
	public void StealFocus() {
		if (IsOpen) {
			MenuStack.Remove(this);
			MenuStack.Add(this);
			NewMenuFocused.Invoke();
		}
	}

	private void OnOpen() {
		canvas.enabled = true;
		IsOpen = true;
		onOpen.Invoke();
		EventSystem.current.SetSelectedGameObject(FirstSelected);
	}

	private void OnClose() {
		canvas.enabled = false;
		IsOpen = false;
		onClose.Invoke();
	}
}