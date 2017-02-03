using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeRengine : MonoBehaviour {

	public LineRenderer Renderer;
	public int numSides;
	public float size;
	public float thickness;
	public float zOffset;
	Vector3[] pushList;
	public float offset;
	public float wiggle;

	void Awake(){
		offset = Random.value * Mathf.PI;
	}
	// Use this for initialization
	void Start () {
		Renderer.startWidth = thickness;
		Renderer.endWidth = thickness;


		if (transform.parent != null && transform.parent.GetComponent<ShapeRengine> () != null) {
			offset = transform.parent.GetComponent<ShapeRengine> ().offset + Mathf.PI / 3;
		}
		List<Vector3> tempCoordsList = new List<Vector3> ();

		for (int i = 0; i <= numSides; i++) {
			Vector3 newPos = new Vector3 ( size * Mathf.Cos(360/numSides*i*Mathf.Deg2Rad), size * Mathf.Sin(360/numSides*i*Mathf.Deg2Rad), -1f);
			tempCoordsList.Add (newPos);
		}

		pushList = tempCoordsList.ToArray ();

		for (int i = 0; i < pushList.Length; i++) {
			pushList [i].z += zOffset;
		}
		Renderer.numPositions = pushList.Length;
		Renderer.SetPositions (pushList);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3[] newlist = new Vector3[pushList.Length];
		for (int i = 0; i < pushList.Length; i++) {
			newlist [i] = pushList[i] * (1 + Mathf.Sin (Time.time+offset)/10);
		}
		wiggle = (.85f + Mathf.Sin (Time.time + offset) / 10);
		Renderer.numPositions = newlist.Length;
		Renderer.SetPositions (newlist);
	
	}
		

}
