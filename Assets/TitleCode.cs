using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;

public class TitleCode : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		LOLSDK.Init ("com.ReedErlandson.Parity");
		LOLSDK.Instance.SubmitProgress(0, 0, 14);
		LOLSDK.Instance.PlaySound ("EnergyMove.mp3");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
