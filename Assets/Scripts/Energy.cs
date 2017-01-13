﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour {
	public bool isReacting = false;
	void Start() {

	}

	void Update() {

	}

	public void endoMove(Vector3 targetPos) {
		StartCoroutine (endoCo (targetPos));
	}

	public IEnumerator endoCo(Vector3 targetPos) {
		isReacting = true;
		float progress = 0f;
		float TimeScale = 2f;
		Vector3 startPos = transform.position;
		this.GetComponent<Rigidbody2D> ().isKinematic = true;
		while (progress <= 1) {
			transform.position = Vector3.Lerp(startPos, targetPos, progress);
			progress += Time.deltaTime * TimeScale;
			yield return null;
		}
		this.GetComponent<Rigidbody2D> ().isKinematic = false;
		GameManager.instance.energyObjList.Remove (this.gameObject);
		GameObject newParticle = Instantiate (GameManager.instance.energyParticle, targetPos, Quaternion.identity) as GameObject;
		Destroy (this.gameObject);
	}

	public void shakeMove() {
		float xRoll = 0f;
		float yRoll = 0f;
		while (xRoll == 0f || yRoll == 0f) {
			xRoll = Random.Range (-1f, 2f);
			yRoll = Random.Range (-1f, 2f);
		}
		this.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (xRoll*GameManager.instance.baseMotion*GameManager.instance.motionMultiplier*GameManager.instance.systemEnergy, yRoll*GameManager.instance.baseMotion*GameManager.instance.motionMultiplier*GameManager.instance.systemEnergy));
	}
}