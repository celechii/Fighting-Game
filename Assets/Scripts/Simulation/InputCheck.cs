using UnityEngine;
using UnityEngine.InputSystem;

public class InputCheck : MonoBehaviour {

	public Input Current;

	public void OnMoveLeft(InputAction.CallbackContext ctx) => UpdateInput(ctx, ref Current.moveLeft);
	public void OnMoveRight(InputAction.CallbackContext ctx) => UpdateInput(ctx, ref Current.moveRight);
	public void OnJump(InputAction.CallbackContext ctx) => UpdateInput(ctx, ref Current.jump);
	public void OnChangeStance(InputAction.CallbackContext ctx) => UpdateInput(ctx, ref Current.changeStance);

	private void UpdateInput(InputAction.CallbackContext ctx, ref bool value) {
		if (ctx.performed)
			value = true;
		else if (ctx.canceled)
			value = false;
	}

	private void OnGUI() {
		if (Application.isPlaying && Simulation.Instance.FrameByFrame) {
			GUIStyle textStyle = new GUIStyle(GUI.skin.toggle);
			textStyle.fontSize = 25;

			GUILayout.BeginArea(new Rect(0, Screen.height / 2f, Screen.width / 2f, Screen.height / 2f));
			Current.moveLeft = GUILayout.Toggle(Current.moveLeft, "move left", textStyle);
			Current.moveRight = GUILayout.Toggle(Current.moveRight, "move right", textStyle);
			Current.jump = GUILayout.Toggle(Current.jump, "jump", textStyle);
			Current.changeStance = GUILayout.Toggle(Current.changeStance, "change stance", textStyle);
			GUILayout.EndArea();
		}
	}
}

[System.Serializable]
public struct Input {
	public bool isConfirmed;
	public bool moveLeft;
	public bool moveRight;
	public bool jump;
	public bool changeStance;

	public int MovementInput => (moveLeft ? -1 : 0) + (moveRight ? 1 : 0);
}