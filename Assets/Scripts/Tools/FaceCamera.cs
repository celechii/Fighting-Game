using UnityEngine;

public class FaceCamera : MonoBehaviour {

	private static Transform mainCamTrans;

	[SerializeField]
	private bool useLocalRotation = true;
	[Space]
	[SerializeField]
	private bool matchX = false;
	[SerializeField]
	private bool matchY = true;
	[SerializeField]
	private bool matchZ = false;

	private void Awake() {
		if (mainCamTrans == null)
			mainCamTrans = Camera.main.transform;
	}

	private void LateUpdate() {
		Vector3 rotation = useLocalRotation ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles;
		Vector3 camRotation = mainCamTrans.localRotation.eulerAngles;

		if (matchX)
			rotation.x = camRotation.x;
		if (matchY)
			rotation.y = camRotation.y;
		if (matchZ)
			rotation.z = camRotation.z;

		if (useLocalRotation)
			transform.localRotation = Quaternion.Euler(rotation);
		else
			transform.rotation = Quaternion.Euler(rotation);
	}
}