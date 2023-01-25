using UnityEngine;
using UnityEngine.InputSystem;

public class InputCheck : MonoBehaviour {

	public Input Current;

	public void OnMoveLeft(InputAction.CallbackContext ctx) => UpdateInput(ctx, Input.MoveLeft);
	public void OnMoveRight(InputAction.CallbackContext ctx) => UpdateInput(ctx, Input.MoveRight);
	public void OnJump(InputAction.CallbackContext ctx) => UpdateInput(ctx, Input.Jump);
	public void OnChangeStance(InputAction.CallbackContext ctx) => UpdateInput(ctx, Input.ChangeStance);

	private void UpdateInput(InputAction.CallbackContext ctx, Input input) {
		if (ctx.performed)
			Current.Add(input);
		else if (ctx.canceled)
			Current.Remove(input);
	}

	private void OnGUI() {
		if (Application.isPlaying && Simulation.Instance.FrameByFrame) {
			GUIStyle textStyle = new GUIStyle(GUI.skin.toggle);
			textStyle.fontSize = 25;

			GUILayout.BeginArea(new Rect(0, Screen.height / 2f, Screen.width / 2f, Screen.height / 2f));

			Input[] allInputs = (Input[])System.Enum.GetValues(typeof(Input));
			for (int i = 0; i < allInputs.Length; i++)
				Current.Set(allInputs[i], GUILayout.Toggle(Current.Has(allInputs[i]), allInputs[i].MakeEnumReadable(), textStyle));

			// bool moveLeft = GUILayout.Toggle(Current.moveLeft, "move left", textStyle);
			// bool moveRight = GUILayout.Toggle(Current.moveRight, "move right", textStyle);
			// bool jump = GUILayout.Toggle(Current.jump, "jump", textStyle);
			// bool changeStance = GUILayout.Toggle(Current.changeStance, "change stance", textStyle);
			GUILayout.EndArea();
		}
	}
}

[System.Flags]
public enum Input {
	IsConfirmed = 1 << 0,
	MoveLeft = 1 << 1,
	MoveRight = 1 << 2,
	Jump = 1 << 3,
	ChangeStance = 1 << 4
}