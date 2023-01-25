using System.Collections.Generic;
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

			GUILayout.EndArea();
		}
	}

	public InputChange[] GenerateInputDataHistory(IList<Input> player1History, IList<Input> player2History) {
		List<InputChange> changes = new() {
			new(0, player1History[0], player2History[0])
		};

		int count = Mathf.Max(player1History.Count, player2History.Count);
		for (int i = 1; i < count; i++) {
			Input player1Delta = player1History[i] | player1History[i - 1];
			Input player2Delta = player2History[i] | player2History[i - 1];

			if (player1Delta > 0 || player2Delta > 0)
				changes.Add(new(i, player1Delta, player2Delta));
		}

		return changes.ToArray();
	}

	public struct InputChange {
		public int FrameNumber;
		public Input Player1Delta;
		public Input Player2Delta;

		public InputChange(int frameNumber, Input player1Delta, Input player2Delta) {
			FrameNumber = frameNumber;
			Player1Delta = player1Delta;
			Player2Delta = player2Delta;
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