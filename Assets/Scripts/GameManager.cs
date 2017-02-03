using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LoLSDK; 

public class GameManager : MonoBehaviour {
	int lolProgress = 4;

	public static GameManager instance;
	public Material dragLineRenMat;
	public SpriteRenderer bgSprite;
	GameObject currentTouchInd;
	public GameObject touchInd;
	public GameObject energyParticle;
	public GameObject moleculeObj;
	public GameObject hydrogenObj;
	public GameObject chlorineObj;
	public GameObject energyObj;
	public Color[] gmColorList;
	public LineRenderer touchRengine;
	Vector3 clickStartPos;
	Vector3 clickEndPos;
	public List<GameObject> energyObjList;
	public List<GameObject> moleculeList;
	List<Molecule> reactionMoleculeList;
	List<Molecule> exoList;
	List<Molecule> endoList;
	public LayerMask drawCollisionMask;
	public LayerMask bondCollisionMask;

	public Shake camShaker;

	public TextMesh systemEnUI;
	public TextMesh redEnUI;
	public TextMesh blueEnUI;
	public int blueEnergy;
	public int redEnergy;
	public int systemDisplayEnergy;
	public Image timerImage;
	public Image menuBG;
	public Text winHeader;
	public Button resetBtn;
	public Color[] timerColors;

	public AudioSource managerSpeaker;
	public AudioClip fizzle;
	public AudioClip match;
	public AudioClip engMove;
	public AudioClip moveSwoosh;

	public SpriteRenderer fade;
	int energyIndex = 0;
	public int currentPlayer = 0;
	public int inactivePlayer = 1;
	public int systemEnergy;
	public int startingMolecules;
	public int startingMoleculeOwnerEqui;
	public int systemHydrogens = 0;
	public int systemChlorines = 0;
	public float baseMotion;
	public float motionMultiplier;
	public float minDragDist;
	public float bondXmod;
	public int gameLength;
	int gameStage = 0;
	public float turnLength;
	public float turnStartTime;
	public float turnCurrentTime;
	public bool turnActive;
	public float barFillMod;
	public float minTurnTimer;
	public bool turnReady = true;

	public int waitForMolecules = 0;

	public ParticleSystem breakBondParticle;
	// Use this for initialization
	void Start () {
		instance = this;
		resetBtn.gameObject.SetActive (false);
		energyObjList = new List<GameObject> ();
		moleculeList = new List<GameObject> ();
		reactionMoleculeList = new List<Molecule> ();
		exoList = new List<Molecule> ();
		endoList = new List<Molecule> ();
		spawnEnergy ();
		dragLineRenMat.SetColor("_Color",gmColorList [currentPlayer + 2]);
		for (int i = 0; i < startingMolecules; i++) {
			spawnMolecule ();
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0) && turnReady) {
			clickStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			clickStartPos.z = -4;
			GameObject newTouchInd = Instantiate (touchInd, clickStartPos, Quaternion.identity) as GameObject;
			newTouchInd.transform.position = new Vector3 (newTouchInd.transform.position.x, newTouchInd.transform.position.y, -3);
			newTouchInd.GetComponent<SpriteRenderer> ().color = gmColorList [currentPlayer + 2];
			currentTouchInd = newTouchInd;
		}

		if (Input.GetMouseButton (0) && turnReady) {
			drawLine ();
		}

		if (Input.GetMouseButtonUp (0)) {
			if (Vector3.Distance (clickStartPos, clickEndPos) >= minDragDist && turnReady) {
				catalyseReaction ();
				turnReady = false;
			}
			touchRengine.SetPositions (new Vector3[] { new Vector3 (0, 0), new Vector3 (0, 0) });
			Destroy (currentTouchInd);
		}

		bgSprite.color = Color.Lerp (gmColorList[0],gmColorList[1],systemEnergy/(startingMolecules*2f));
		timeTurn ();
		updateHud ();
	}

	public void timeTurn() {
		if (turnActive) {
			timerImage.color = bgSprite.color + timerColors[currentPlayer];

			turnCurrentTime = (Time.time - turnStartTime) / turnLength;
			if (turnCurrentTime >= 1) {
				changePlayer ();
				turnActive = false;
				StartCoroutine ("fillTimer");
			}
		} else {
			timerImage.color = bgSprite.color + new Color (0.02f, 0.02f, 0.02f, 1);
		}
	}

	void turnFlag() {
		turnReady = true;
		checkEnd ();
	}

	void checkEnd() {
		gameStage += 1;
		if (gameStage == gameLength) {
			endMatch ();
		}
	}

	void updateHud() {
		redEnergy = 0;
		blueEnergy = 0;
		systemDisplayEnergy = 0;
		foreach (GameObject aMol in moleculeList) {
			int enVal = 0;
			if (aMol.GetComponent<Molecule> ().atomList [0].atomType == aMol.GetComponent<Molecule> ().atomList [1].atomType) {
				enVal = 2;
			} else {
				enVal = 1;
			}
			if (aMol.GetComponent<Molecule> ().playerOwner == 0) {
				blueEnergy += enVal;
			} else if (aMol.GetComponent<Molecule> ().playerOwner == 1) {
				redEnergy += enVal;
			} else {
				systemDisplayEnergy += enVal;
			}
		}
		redEnUI.text = redEnergy.ToString();
		blueEnUI.text = blueEnergy.ToString();
		systemEnUI.text = (systemEnergy+systemDisplayEnergy).ToString();
		timerImage.fillAmount = 1-turnCurrentTime;
	}

	void spawnMolecule() {
		GameObject newMolecule = Instantiate (moleculeObj,new Vector3(Random.Range(-3,4),Random.Range(-3,4),0),Quaternion.identity) as GameObject;
		newMolecule.transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));
		moleculeList.Add (newMolecule);
	}

	void spawnEnergy() {
		for (int i = 0; i < systemEnergy; i++) {
			GameObject newEnergy = Instantiate (energyObj,new Vector3(Random.Range(-5f,5f),Random.Range(-5f,5f),0),Quaternion.identity) as GameObject;
			energyObjList.Add (newEnergy);
		}
	}

	void moveEnergy() {
		managerSpeaker.PlayOneShot (engMove);
		camShaker.shake (2f, 2f);
		foreach (GameObject aEn in energyObjList) {
			aEn.GetComponent<Energy>().shakeMove();
		}
	}

	void drawLine() {
		clickEndPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		clickEndPos.z = -4;
		RaycastHit2D rengineCollision = Physics2D.Linecast (clickStartPos,clickEndPos,drawCollisionMask.value);
		if (rengineCollision.collider != null) {
			clickEndPos = rengineCollision.point;
		}
		Vector3[] touchRenArr = new Vector3[] {clickStartPos, clickEndPos};
		RaycastHit2D[] bondCollision = Physics2D.LinecastAll (clickStartPos, clickEndPos, bondCollisionMask.value);
		if (turnReady) {
			reactionMoleculeList.Clear ();
			foreach (RaycastHit2D hitBond in bondCollision) {
				if (hitBond.transform.gameObject.GetComponent<Molecule> () != null) {
					reactionMoleculeList.Add (hitBond.transform.gameObject.GetComponent<Molecule> ());
				}
			}
		}
		touchRengine.SetPositions (touchRenArr);
	}
	IEnumerator fadeIn(){
		fade.enabled = true;
		float t = 0; 
		Color empty = new Color (0, 0, 0, 0);
		Color full = new Color (0, 0, 0, .75f);
		while (t < 1) {
			t += Time.deltaTime*3;
			fade.color = Color.Lerp (empty,full, t);
			yield return null;
		}
		fade.color = full;
	}

	IEnumerator fadeOut(){
		fade.enabled = true;
		float t = 0; 
		Color empty = new Color (0, 0, 0, 0);
		Color full = new Color (0, 0, 0, .8f);
		while (t < 1) {
			t += Time.deltaTime*3;
			fade.color = Color.Lerp (full,empty, t);
			yield return null;
		}
		fade.color = empty;
		fade.enabled = false;
	}

	void catalyseReaction() {
		StartCoroutine (fadeIn ());
		turnActive = false;
		StartCoroutine ("fillTimer");
		waitForMolecules = 0;
		energyIndex = systemEnergy;
		exoList.Clear ();
		endoList.Clear ();
		foreach (Molecule aMol in reactionMoleculeList) {
			if (aMol.atom1.GetComponent<Atom>().atomType==aMol.atom2.GetComponent<Atom>().atomType) {
				exoList.Add (aMol);
			} else {
				endoList.Add (aMol);
			}
		}

		List<Atom> exoAtomList = new List<Atom> ();
		List<Atom> endoAtomList = new List<Atom> ();

		foreach (Molecule aMol in reactionMoleculeList) {
			aMol.transform.position -= new Vector3 (0, 0, 6);
			foreach (Atom anAtom in aMol.atomList) {
				if (exoList.Contains(aMol)) {
					exoAtomList.Add (anAtom);
				} else {
					endoAtomList.Add (anAtom);
				}
			}
		}
		List<Atom> pairedExoAtoms = new List<Atom>();
		foreach (Atom anAtom in exoAtomList) {
			for (int i = 0; i < exoAtomList.Count; i++) {
				if (anAtom!=exoAtomList[i] && anAtom.atomType!=exoAtomList[i].atomType && !pairedExoAtoms.Contains(anAtom) && !pairedExoAtoms.Contains(exoAtomList[i])) {
					pairedExoAtoms.Add(anAtom);
					pairedExoAtoms.Add(exoAtomList[i]);
				}
			}
		}
		List<Atom> pairedEndoAtoms = new List<Atom>();
		foreach (Atom anAtom in endoAtomList) {
			for (int i = 0; i < endoAtomList.Count; i++) {
				if (anAtom!=endoAtomList[i] && anAtom.atomType==endoAtomList[i].atomType && !pairedEndoAtoms.Contains(anAtom) && !pairedEndoAtoms.Contains(endoAtomList[i])) {
					pairedEndoAtoms.Add(anAtom);
					pairedEndoAtoms.Add(endoAtomList[i]);
				}
			}
		}
		foreach (Atom exAtom in pairedExoAtoms) {
			exoAtomList.Remove (exAtom);
		}
		foreach (Atom enAtom in pairedEndoAtoms) {
			endoAtomList.Remove (enAtom);
		}
		foreach (Molecule aMol in reactionMoleculeList) {
			waitForMolecules++;
			if (pairedEndoAtoms.Count >= 2 && endoList.Contains(aMol)) {
				aMol.atom1.transform.SetParent (null);
				aMol.atom2.transform.SetParent (null);
				aMol.reactMove (false, pairedEndoAtoms [0].gameObject, pairedEndoAtoms [1].gameObject, aMol.atom1.transform.position, aMol.atom2.transform.position);
				pairedEndoAtoms.RemoveRange (0, 2);
			} else if (pairedExoAtoms.Count >= 2 && exoList.Contains(aMol)) {
				aMol.atom1.transform.SetParent (null);
				aMol.atom2.transform.SetParent (null);
				aMol.reactMove (true, pairedExoAtoms [0].gameObject, pairedExoAtoms [1].gameObject, aMol.atom1.transform.position, aMol.atom2.transform.position);
				pairedExoAtoms.RemoveRange (0, 2);
			} else if (endoAtomList.Count >= 2 && endoList.Contains(aMol)) {
				aMol.reactFizzle ();
				/*aMol.atom1 = endoAtomList [0].gameObject;
				aMol.atom2 = endoAtomList [1].gameObject;
				endoAtomList.RemoveRange (0, 2);*/
			} else if (exoAtomList.Count >= 2) {
				aMol.reactFizzle ();
				/*aMol.atom1 = exoAtomList [0].gameObject;
				aMol.atom2 = exoAtomList [1].gameObject;
				exoAtomList.RemoveRange (0, 2);*/
			}
		}
		//waitForMolecules minimum guarantee
		waitForMolecules+=1;
		finish();
	}

	public void finish(){
		waitForMolecules--;
		if (waitForMolecules <= 0) {
			foreach (Molecule aMol in reactionMoleculeList) {
				aMol.isReacting = false;
				aMol.transform.position += new Vector3 (0, 0, 6);
			}
			Invoke ("changePlayer", 0.5f);
			StartCoroutine(fadeOut ());
		}
	}

	public void changePlayer() {
		//change player
		Invoke ("turnFlag", 2.5f);
		if (currentPlayer == 0) {
			currentPlayer = 1;
			inactivePlayer = 0;
		} else {
			currentPlayer = 0;
			inactivePlayer = 1;
		}
		Invoke("moveEnergy", .0f);
		Invoke ("startTimer", 2.5f);
		dragLineRenMat.SetColor("_Color",gmColorList [currentPlayer + 2]);
		//lolprogress
		lolProgress += 1;
		if (lolProgress < 15) {
			LOLSDK.Instance.SubmitProgress (0, lolProgress, 14);
		}
	}

	public void startTimer() {
		turnActive = true;
		turnStartTime = Time.time;
	}

	IEnumerator fillTimer() {
		float startFillTime = Time.time;
		while (turnCurrentTime > 0) {
			turnCurrentTime -= (Time.time - startFillTime) * barFillMod;
			yield return null;
		}
	}

	public void resolveEnergy(Vector3 fedPos, bool isEndo) {
		if (isEndo) {
			float bestDist = 9999f;
			GameObject bestEn = null;
			foreach (GameObject enObj in energyObjList) {
				float thisDist = Vector3.Distance (fedPos,enObj.transform.position);
				if (thisDist < bestDist && !enObj.GetComponent<Energy>().isReacting) {
					bestDist = thisDist;
					bestEn = enObj;
				}
			}
			bestEn.GetComponent<Energy>().endoMove(fedPos);
		} else if (!isEndo) {
			GameObject newEnergy = Instantiate (energyObj,fedPos,Quaternion.identity) as GameObject;
			GameObject newParticle = Instantiate (GameManager.instance.energyParticle, fedPos, Quaternion.identity) as GameObject;
			energyObjList.Add (newEnergy);
			newEnergy.GetComponent<Energy> ().shakeMove ();
		}
	}

	public void endMatch() {
		turnActive = false;
		turnReady = false;
		redEnUI.gameObject.SetActive (false);
		blueEnUI.gameObject.SetActive (false);
		systemEnUI.gameObject.SetActive (false);

		if (redEnergy>blueEnergy) {
			winHeader.text = "RED PLAYER WINS!";
		} else if (blueEnergy>redEnergy) {
			winHeader.text = "BLUE PLAYER WINS!";
		} else {
			winHeader.text = "TIE GAME!";
		}
		StartCoroutine ("menuFadeIn");
	}

	public void resetGame() {
		LOLSDK.Instance.CompleteGame();
		//SceneManager.LoadScene ("Main");
	}

	IEnumerator menuFadeIn() {
		float fadeTime = 1f;
		float targetTime = Time.time + fadeTime;
		resetBtn.gameObject.SetActive (true);
		foreach (GameObject anEn in energyObjList) {
			Destroy (anEn);
		}
		while (Time.time<targetTime) {
			menuBG.color = Color.Lerp (Color.white, new Color (255,255,255,0), (targetTime-Time.time)/fadeTime);
			winHeader.color = Color.Lerp (Color.black, new Color (0,0,0,0), (targetTime-Time.time)/fadeTime);
			yield return null;
		}
		menuBG.color = Color.white;
		winHeader.color = Color.black;
	}
}
