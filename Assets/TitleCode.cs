using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;

public class TitleCode : MonoBehaviour {
	public GameObject moleculeObj;
	public GameObject chlAt;
	public GameObject hydAt;
	public static TitleCode Instance;
	public Color[] ColorList;

	// Use this for initialization
	void Start () {
		Instance = this;

		LOLSDK.Init ("com.ReedErlandson.Parity");
		LOLSDK.Instance.SubmitProgress(0, 0, 14);

		GameObject newMolecule = Instantiate (moleculeObj,new Vector3(4.1f,-3.4f,0),Quaternion.identity) as GameObject;
		newMolecule.GetComponent<titleMolecule> ().controlledSpawn (1, chlAt, chlAt);
		newMolecule.GetComponent<titleMolecule> ().populate ();
		newMolecule.GetComponent<titleMolecule> ().updateColor ();
		newMolecule.transform.rotation = Quaternion.Euler(0,0,40);

		GameObject newMolecule2 = Instantiate (moleculeObj,new Vector3(0,-2.6f,0),Quaternion.identity) as GameObject;
		newMolecule2.GetComponent<titleMolecule> ().controlledSpawn (0, hydAt, hydAt);
		newMolecule2.GetComponent<titleMolecule> ().populate ();
		newMolecule2.GetComponent<titleMolecule> ().updateColor ();
		newMolecule2.transform.rotation = Quaternion.Euler(0,0,-17);

		GameObject newMolecule3 = Instantiate (moleculeObj,new Vector3(-3.2f,-3.1f,0),Quaternion.identity) as GameObject;
		newMolecule3.GetComponent<titleMolecule> ().controlledSpawn (2, hydAt, chlAt);
		newMolecule3.GetComponent<titleMolecule> ().populate ();
		newMolecule3.GetComponent<titleMolecule> ().updateColor ();
		newMolecule3.transform.rotation = Quaternion.Euler(0,0,191);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
