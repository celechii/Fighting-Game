// taken from Toulouse de Margerie
// https://github.com/Unity-Technologies/GenLockProofOfConcept/blob/master/Assets/ForceRenderRate.cs

using System.Collections;
using System.Threading;
using UnityEngine;

public class ForceRenderRate : MonoBehaviour {

	[SerializeField]
	private float rate = 60.0f;
	[SerializeField]
	private bool bypass;
	private float currentFrameTime;

	private IEnumerator Start() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 9999;
		currentFrameTime = Time.realtimeSinceStartup;

		while (true) {
			if (bypass) {
				yield return null;
				continue;
			}
			
			yield return new WaitForEndOfFrame();
			currentFrameTime += 1.0f / rate;
			float t = Time.realtimeSinceStartup;
			float sleepTime = currentFrameTime - t - 0.01f;
			if (sleepTime > 0)
				Thread.Sleep((int)(sleepTime * 1000));
			while (t < currentFrameTime)
				t = Time.realtimeSinceStartup;
		}
	}
}