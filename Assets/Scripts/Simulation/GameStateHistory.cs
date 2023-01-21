public class GameStateHistory {

	private Simulation.GameState[] stateHistory;
	private int lastFrameRecorded = 0;
	public int FramesAvailable { get; private set; }

	public GameStateHistory(int size) {
		stateHistory = new Simulation.GameState[size];
	}

	public void RecordState(Simulation.GameState gameState) {
		lastFrameRecorded = gameState.frame;
		stateHistory[lastFrameRecorded % stateHistory.Length] = gameState;
		if (FramesAvailable < stateHistory.Length)
			FramesAvailable++;
	}

	public Simulation.GameState RevertToFrame(int frame) {
		if (frame < 0)
			frame = 0;

		int frameDifference = (lastFrameRecorded + 1) - frame;
		if (frameDifference > FramesAvailable) {
			UnityEngine.Debug.LogWarning($"Tried to revert to frame {frame} but could only move back {FramesAvailable} frames (from frame {lastFrameRecorded})");
			frame += frameDifference - FramesAvailable;
			FramesAvailable = 0;
		} else
			FramesAvailable -= frameDifference;

		lastFrameRecorded = frame - 1;
		return stateHistory[frame % stateHistory.Length];
	}
}