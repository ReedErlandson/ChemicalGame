using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoLSDK;

public class Molecule : MonoBehaviour {
	public int playerOwner;
	public bool isReacting;
	public GameObject[] atomBlueprints;
	public GameObject atom1;
	public GameObject atom2;
	public GameObject debuncher;

	public Atom atom1script;
	public Atom atom2script;
	public List<Atom> atomList;
	BoxCollider2D lineCollider;
	public LineRenderer rengine;
	Color[] colorArray;
	public AudioSource molSpeaker;

	// Use this for initialization
	void Start () {
		atomBlueprints = new GameObject[] {null,null};
		rengine = GetComponent<LineRenderer> ();
		rengine.startWidth = (0.1f);
		rengine.endWidth = (0.1f);
		lineCollider = gameObject.AddComponent<BoxCollider2D> ();
		lineCollider.offset = new Vector2(0,0);
		lineCollider.size = new Vector2 (gameObject.transform.localScale.x,gameObject.transform.localScale.y*0.75f);
		colorArray = new Color[]{Color.blue,Color.red};
		randomSpawn ();
		populate ();
		isReacting = false;
		updateColor ();
	}

	// Update is called once per frame
	void Update () {
		updateRengine();
	}

	public void randomSpawn() {
		playerOwner = 2;
		/*if (GameManager.instance.startingMoleculeOwnerEqui == 0) {
			playerOwner = Random.Range (0, 2);
			if (playerOwner == 0) {
				GameManager.instance.startingMoleculeOwnerEqui = -1;
			} else {
				GameManager.instance.startingMoleculeOwnerEqui = 1;
			}
		} else if (GameManager.instance.startingMoleculeOwnerEqui == 1) {
			playerOwner = 0;
			GameManager.instance.startingMoleculeOwnerEqui = 0;
		} else {
			playerOwner = 1;
			GameManager.instance.startingMoleculeOwnerEqui = 0;
		}*/
		if (Random.Range (0, 2) == 0 && GameManager.instance.systemHydrogens < GameManager.instance.startingMolecules) {
			atomBlueprints [0] = GameManager.instance.hydrogenObj;
			GameManager.instance.systemHydrogens += 1;
		} else if (GameManager.instance.systemChlorines < GameManager.instance.startingMolecules) {
			atomBlueprints [0] = GameManager.instance.chlorineObj;
			GameManager.instance.systemChlorines += 1;
		} else {
			atomBlueprints [0] = GameManager.instance.hydrogenObj;
			GameManager.instance.systemHydrogens += 1;
		}
		if (Random.Range (0, 2) == 0 && GameManager.instance.systemHydrogens<GameManager.instance.startingMolecules) {
			atomBlueprints[1] = GameManager.instance.hydrogenObj;
			GameManager.instance.systemHydrogens += 1;
		} else if (GameManager.instance.systemChlorines < GameManager.instance.startingMolecules) {
			atomBlueprints[1] = GameManager.instance.chlorineObj;
			GameManager.instance.systemChlorines += 1;
		} else {
			atomBlueprints[1] = GameManager.instance.hydrogenObj;
			GameManager.instance.systemHydrogens += 1;
		}
	}

	public void populate() {
		atom1 = Instantiate (atomBlueprints[0], this.transform) as GameObject;
		atom2 = Instantiate (atomBlueprints[1], this.transform) as GameObject;
		atom1.GetComponent<CircleCollider2D> ().radius = 1.25f;
		atom2.GetComponent<CircleCollider2D> ().radius = 1.25f;
		atom1script = atom1.GetComponent<Atom> ();
		atom2script = atom2.GetComponent<Atom> ();
		setAtoms ();
	}

	void setRadius() {
		atom1.GetComponent<CircleCollider2D> ().radius = 0.5f;
		atom2.GetComponent<CircleCollider2D> ().radius = 0.5f;
	}

	void updateRengine() {
		if (!isReacting) {
			rengine.material.SetColor ("_Color", Color.white);
			rengine.material.SetColor ("_EmissionColor", Color.white);
			Vector3 tempLine = Vector3.Normalize(atom2.transform.position - atom1.transform.position);
			float aTypeMod = 1f * atom1script.atomBorder.wiggle;
			if (atom1script.atomType == 1) {
				aTypeMod = 0.7f * atom1script.atomBorder.wiggle;
			}
			Vector3 BondAnchor1 = atom1.transform.position + tempLine * GameManager.instance.bondXmod*aTypeMod;
			aTypeMod = 1f * atom2script.atomBorder.wiggle;
			if (atom2script.atomType == 1) {
				aTypeMod = 0.7f * atom2script.atomBorder.wiggle;
			}
			Vector3 BondAnchor2 = atom2.transform.position - tempLine * GameManager.instance.bondXmod*aTypeMod;
			BondAnchor1.z -= 3;
			BondAnchor2.z -= 3;

			rengine.SetPositions (new Vector3[] { BondAnchor1, BondAnchor2});
		}
	}

	public void updateColor() {
		atom1.transform.GetChild(0).transform.GetComponent<LineRenderer>().material.SetColor("_Color",GameManager.instance.gmColorList [playerOwner + 2]);
		atom2.transform.GetChild(0).transform.GetComponent<LineRenderer> ().material.SetColor("_Color",GameManager.instance.gmColorList [playerOwner + 2]);
	}

	public void setAtoms() {
		atom1.transform.localPosition = new Vector3 (-0.7f, 0, 0);
		atom2.transform.localPosition = new Vector3 (0.7f, 0, 0);
		atomList.Clear ();
		atomList.Add (atom1.GetComponent<Atom>());
		atomList.Add (atom2.GetComponent<Atom>());
		Invoke ("setRadius", 1f);
	}

	public void reactMove(bool isExo, GameObject aObj1, GameObject aObj2, Vector3 target1, Vector3 target2) {
		StartCoroutine (atomMove (isExo, aObj1, aObj2, target1, target2));
	}

	public IEnumerator atomMove(bool isExo, GameObject atomSlug, GameObject atomSlug2, Vector3 targetPos, Vector3 targetPos2) {
		gameObject.GetComponent<Rigidbody2D> ().isKinematic = true;
		Vector3 startPos = atomSlug.transform.position;
		Vector3 startPos2 = atomSlug2.transform.position;
		if (!isExo) {
			GameManager.instance.systemEnergy -= 1;
			GameManager.instance.resolveEnergy (this.transform.position, true);
			yield return new WaitForSeconds (0.5f);
		}
		if (isExo) {
			GameManager.instance.systemEnergy += 1;
			GameManager.instance.resolveEnergy (this.transform.position, false);
		}
		isReacting = true;

		GameManager.instance.breakBondParticle.transform.position = transform.position;
		GameManager.instance.breakBondParticle.transform.rotation = transform.rotation;
		ParticleSystem.ShapeModule shape = GameManager.instance.breakBondParticle.shape;
		shape.box = new Vector3 (lineCollider.size.x, .1f, 0);
		GameManager.instance.breakBondParticle.Emit (30); 

		rengine.SetPositions (new Vector3[] {new Vector3 (0,0,0),new Vector3 (0,0,0)});
		//GameManager.instance.managerSpeaker.PlayOneShot (GameManager.instance.match, 0.6f);
		LOLSDK.Instance.PlaySound("Light.mp3");
		float scrubTime = 0;
		float totalCurveArea = 0;
		float totalCurveProgress = 0;
		float progress = 0f;
		float yBump = 2f;
		float amplitude = 1f;
		float atomMoveTime = 1.25f;
		//get total curve area
		//totalCurveArea = 1f;
		totalCurveArea = (1/ (Mathf.PI * 2))* ((Mathf.PI * 2*(yBump*amplitude) + Mathf.Sin (Mathf.PI * 2)*amplitude));
		yield return new WaitForSeconds (2);
		//GameManager.instance.managerSpeaker.PlayOneShot (GameManager.instance.moveSwoosh, 0.5f);
		LOLSDK.Instance.PlaySound("Move_Swoosh.mp3");
		while (scrubTime <= 1) {
			scrubTime += Time.deltaTime/atomMoveTime;
			float curveAdd = (Time.deltaTime/atomMoveTime) * amplitude*((Mathf.Cos (scrubTime*360*Mathf.Deg2Rad) + yBump));
			totalCurveProgress += curveAdd;
			progress = totalCurveProgress / totalCurveArea;
			atomSlug.transform.position = Vector3.Slerp(startPos, targetPos, progress);
			atomSlug2.transform.position = Vector3.Slerp (startPos2, targetPos2, progress);
			yield return null;
		}
		atom1 = atomSlug;
		atom2 = atomSlug2;
		atomSlug.transform.SetParent (this.transform,false);
		atomSlug2.transform.SetParent (this.transform,false);
		setAtoms ();
		playerOwner = GameManager.instance.currentPlayer;
		updateColor ();
		//yield return new WaitForSeconds (0.5f);
		//manager wrap
		gameObject.GetComponent<Rigidbody2D> ().isKinematic = false;
		LOLSDK.Instance.PlaySound("OK.mp3");
		GameManager.instance.finish ();
	}

	public void reactFizzle() {
		gameObject.GetComponent<Rigidbody2D> ().isKinematic = true;
		//GameManager.instance.managerSpeaker.PlayOneShot (GameManager.instance.match);
		LOLSDK.Instance.PlaySound("Light.mp3");
		isReacting = true;
		rengine.SetPositions (new Vector3[] {new Vector3 (0,0,0),new Vector3 (0,0,0)});
		Invoke ("fizzleResolve", 4.0f);
	}

	public void fizzleResolve() {
		gameObject.GetComponent<Rigidbody2D> ().isKinematic = false;
		GameManager.instance.finish();
		isReacting = false;
		molSpeaker.clip = GameManager.instance.fizzle;
		//molSpeaker.PlayOneShot (GameManager.instance.fizzle);
		LOLSDK.Instance.PlaySound("Fizzle.mp3");
	}

	public void debunchTrigger() {
		StartCoroutine ("molDebunch");
	}

	public IEnumerator molDebunch() {
		float startTime = 0f;
		float totalTime = 1.5f;
		float startSize = 0.5f;
		float maxSize = 2f;
		debuncher.SetActive (true);
		startTime = Time.time;
		/*while (Time.time < startTime+totalTime) {
			debuncher.transform.localScale = Vector3.Lerp (new Vector3(startSize,startSize,1), new Vector3(maxSize,maxSize,1),(Time.time-startTime)/totalTime);
			yield return null;
		}*/
		debuncher.transform.localScale = new Vector3 (2f,2f,1f);
		yield return new WaitForSeconds (2f);
		debuncher.SetActive (false);
	}

}
