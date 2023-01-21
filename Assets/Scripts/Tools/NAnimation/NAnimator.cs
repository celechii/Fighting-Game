using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NAnimator : MonoBehaviour {

	[Tooltip("Will play the first animation on enable")]
	public bool startOnEnable;
	public bool showWarnings = true;
	public NAnimation[] anims;

	public bool isPaused;

	private Dictionary<string, NAnimation> animations;
	private SpriteRenderer spriteRenderer;
	private Image image;
	private int animIndex;
	private Coroutine currentAnimPlaying;
	private bool useUI;

	private void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		image = GetComponent<Image>();

		useUI = spriteRenderer == null;

		animations = new Dictionary<string, NAnimation>();
		for (int i = 0; i < anims.Length; i++) {
			animations.Add(anims[i].name, anims[i]);
			anims[i].index = i;
		}
	}

	private void OnEnable() {
		if (startOnEnable)
			PlayAnim(0);
	}

	public void PlayAnim(string name) {
		if (animations.ContainsKey(name)) {
			NAnimation a = animations[name];
			animIndex = a.index;
			if (currentAnimPlaying != null)
				StopCoroutine(currentAnimPlaying);
			currentAnimPlaying = StartCoroutine(Play(a));
		} else if (showWarnings) {
			Debug.LogError($"Can't play {name} cause its not in the list of anims");
		}
	}

	public void PlayAnim(int index) {
		animIndex = index;
		if (currentAnimPlaying != null)
			StopCoroutine(currentAnimPlaying);
		currentAnimPlaying = StartCoroutine(Play(anims[index]));

	}

	public void PlayNext() {
		PlayAnim((animIndex + 1) % anims.Length);
	}

	public void PlayPrev() {
		if (animIndex == 0)
			animIndex = anims.Length - 1;
		else
			animIndex--;
		PlayAnim(animIndex);
	}

	private void SetAnimSprite(Sprite frame) {
		if (useUI)
			image.sprite = frame;
		else
			spriteRenderer.sprite = frame;
	}

	public void PlayAnim(NAnimation anim) {
		if (currentAnimPlaying != null)
			StopCoroutine(currentAnimPlaying);
		currentAnimPlaying = StartCoroutine(Play(anim));
	}

	public void Stop() {
		if (currentAnimPlaying != null)
			StopCoroutine(currentAnimPlaying);
	}

	private IEnumerator Play(NAnimation anim) {
		if (anim == null)
			yield break;

		int frameIndex = 0;
		int loopsRemaining = anim.loopCount;

		if (!anim.events.callStartEveryLoop)
			anim.events.OnStart.Invoke();

		while (anim.frames.Length > 0) {
			while (isPaused)
				yield return null;
			if (anim.events.callStartEveryLoop)
				anim.events.OnStart.Invoke();

			SetAnimSprite(anim.frames[frameIndex]);
			anim.CallEvents(frameIndex);

			yield return StartCoroutine(PauseableWait(1f / anim.framerate));
			if (frameIndex == anim.frames.Length - 1) { // end of the animation?
				if (anim.events.callFinishEveryLoop)
					anim.events.OnFinish.Invoke();

				if (!anim.lööp || loopsRemaining <= 0 && anim.loopCount != 0)
					break;
				else if (anim.loopCount != 0)
					loopsRemaining--;
			}
			frameIndex = (frameIndex + 1) % anim.frames.Length;
		}

		if (!anim.events.callFinishEveryLoop)
			anim.events.OnFinish.Invoke();

		currentAnimPlaying = null;
		if (anim.nextAnim != "")
			PlayAnim(anim.nextAnim);
	}

	private IEnumerator PauseableWait(float duration) {
		for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime) {
			while (isPaused)
				yield return null;
			yield return null;
		}
	}
}