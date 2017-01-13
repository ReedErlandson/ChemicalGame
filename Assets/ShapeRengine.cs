using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeRengine : MonoBehaviour {

	public LineRenderer Renderer;
	public int numSides;
	public float size;
	public float thickness;
	public float zOffset;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		Renderer.startWidth = thickness;
		Renderer.endWidth = thickness;

		Vector3 currentPos = this.transform.position;
		List<Vector3> tempCoordsList = new List<Vector3> ();

		for (int i = 0; i <= numSides; i++) {
			Vector3 newPos = new Vector3 (currentPos.x + size * Mathf.Cos(360/numSides*i*Mathf.Deg2Rad),currentPos.y + size * Mathf.Sin(360/numSides*i*Mathf.Deg2Rad),currentPos.z);
			tempCoordsList.Add (newPos);
		}

		Vector3[] pushList = tempCoordsList.ToArray ();

		for (int i = 0; i < pushList.Length; i++) {
			pushList [i].z += zOffset;
		}

		Renderer.numPositions = pushList.Length;
		Renderer.SetPositions (pushList);
	}
		

}
