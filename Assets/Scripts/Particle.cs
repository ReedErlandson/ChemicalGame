using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine ("wipeTimer");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator wipeTimer() {
		yield return new WaitForSeconds (1);
		Destroy (this.gameObject);
	}
}
