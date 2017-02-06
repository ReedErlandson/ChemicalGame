using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LoLSDK;

public class TutorialManager : MonoBehaviour {
	bool isDrawing = false;

	Vector3 clickStartPos;
	Vector3 clickEndPos;
	GameObject currentTouchInd;
	bool turnReady = false;
	public int currentPlayer = 1;
	int waitForMolecules = 0;
	public int systemEnergy = 0;
	int energyIndex = 0;
	int reactionTarget = 2;
	List<tutMolecule> exoList;
	List<tutMolecule> endoList;
	List<GameObject> moleculeList;
	public Material dragLineRenMat;
	public List<GameObject> energyObjList;
	public GameObject energyObj;
	public GameObject energyParticle;
	public AudioSource managerSpeaker;
	public AudioSource chatterSpeaker;
	public AudioClip match;
	public AudioClip fizzle;
	public AudioClip moveSwoosh;
	public AudioClip energySwoosh;

	public Shake camShaker;

	public SpriteRenderer quizAsprite;
	public SpriteRenderer quizBsprite;
	public SpriteRenderer quizCsprite;
	public SpriteRenderer quizVsprite;

	public ParticleSystem breakBondParticle;

	public TextMesh systemEnUI;
	public TextMesh redEnUI;
	public TextMesh blueEnUI;
	public SpriteRenderer redInd;
	public SpriteRenderer blueInd;
	public SpriteRenderer goldInd;
	public SpriteRenderer guiInd;
	public int blueEnergy;
	public int redEnergy;
	public int systemDisplayEnergy;

	public Text tuText;
	public GameObject touchInd;
	public GameObject moleculeObj;
	public GameObject hydAt;
	public GameObject chlAt;
	public Color[] gmColorList;
	public static TutorialManager instance;

	List<tutMolecule> reactionMoleculeList;
	public LineRenderer touchRengine;
	public LayerMask drawCollisionMask;
	public LayerMask bondCollisionMask;

	public float bondXmod;
	public float minDragDist;
	int tuPhase = 0;


	// Use this for initialization
	void Start () {
		if (!LOLSDK.Instance.IsInitialized) {
			LOLSDK.Init ("com.ReedErlandson.Parity");
		}
		LOLSDK.Instance.SubmitProgress(0, 1, 14);
		LOLSDK.Instance.PlaySound ("Chatter_1.mp3", true, true);
		instance = this;
		reactionMoleculeList = new List<tutMolecule> ();
		exoList = new List<tutMolecule> ();
		endoList = new List<tutMolecule> ();
		energyObjList = new List<GameObject> ();
		moleculeList = new List<GameObject> ();
		changeText("Hello, Doctors! Welcome to the lab. We’ll get right to it — I’m sure you’re both eager to start.");
		Invoke ("stage1", 8f);
		dragLineRenMat.SetColor("_Color", gmColorList [currentPlayer + 2]);
		//lolprogress & lolsound

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0) && turnReady && !isDrawing || Input.touchCount>0 && turnReady && !isDrawing) {
			if (Input.touchCount > 0) { //touch one
				clickStartPos = Camera.main.ScreenToWorldPoint (Input.GetTouch(0).position);
				isDrawing = true;
			} else {
				clickStartPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			}
			clickStartPos.z = -4;
			GameObject newTouchInd = Instantiate (touchInd, clickStartPos, Quaternion.identity) as GameObject;
			newTouchInd.transform.position = new Vector3 (newTouchInd.transform.position.x, newTouchInd.transform.position.y, -3);
			newTouchInd.GetComponent<SpriteRenderer> ().color = gmColorList [currentPlayer + 2];
			currentTouchInd = newTouchInd;
		}

		if (Input.GetMouseButton (0) && turnReady || Input.touchCount>0 && turnReady) {
			drawLine ();
		}

		if (Input.GetMouseButtonUp (0) || Input.touchCount<1 && isDrawing) {
			if (Vector3.Distance (clickStartPos, clickEndPos) < minDragDist) {
				LOLSDK.Instance.PlaySound ("Fail.mp3");
			} else if (turnReady && reactionMoleculeList.Count==reactionTarget) {
				catalyseReaction ();
				turnReady = false;
			}
			touchRengine.SetPositions (new Vector3[] { new Vector3 (0, 0), new Vector3 (0, 0) });
			Destroy (currentTouchInd);
			isDrawing = false;
		}
}
		
	void stage1() {
		changeText("Check it out: molecules! These ones are all yours, Dr. Blue—you can tell by the way they glow around the edge.");
		GameObject newMolecule = Instantiate (moleculeObj,new Vector3(0,-1,0),Quaternion.identity) as GameObject;
		newMolecule.GetComponent<tutMolecule> ().controlledSpawn (1, chlAt, chlAt);
		newMolecule.GetComponent<tutMolecule> ().populate ();
		newMolecule.GetComponent<tutMolecule> ().updateColor ();
		newMolecule.transform.rotation = Quaternion.Euler(0,0,40);
		moleculeList.Add (newMolecule);
		GameObject newMolecule2 = Instantiate (moleculeObj,new Vector3(-1.7f,2,0),Quaternion.identity) as GameObject;
		newMolecule2.GetComponent<tutMolecule> ().controlledSpawn (1, hydAt, hydAt);
		newMolecule2.GetComponent<tutMolecule> ().populate ();
		newMolecule2.GetComponent<tutMolecule> ().updateColor ();
		newMolecule2.transform.rotation = Quaternion.Euler(0,0,45);
		moleculeList.Add (newMolecule2);

		Invoke ("stage2", 8f);
	}

	void stage2() {
		tuPhase = 2;
		changeText("Why don’t you show your friend Dr. Red how to cause a reaction? It’s easy: tap and drag to draw a line and cut the chemical bonds. Be sure to break both bonds at once!");
		turnReady = true;
	}

	void drawLine() {
		if (Input.touchCount > 0) {
			clickEndPos = Camera.main.ScreenToWorldPoint (Input.GetTouch(0).position);
		} else {
			clickEndPos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		}
		clickEndPos.z = -4;
		RaycastHit2D rengineCollision = Physics2D.Linecast (clickStartPos,clickEndPos, drawCollisionMask.value);
		if (rengineCollision.collider != null) {
			clickEndPos = rengineCollision.point;
		}
		Vector3[] touchRenArr = new Vector3[] {clickStartPos, clickEndPos};
		RaycastHit2D[] bondCollision = Physics2D.LinecastAll (clickStartPos, clickEndPos, bondCollisionMask.value);
		if (turnReady) {
			reactionMoleculeList.Clear ();
			foreach (RaycastHit2D hitBond in bondCollision) {
				if (hitBond.transform.gameObject.GetComponent<tutMolecule> () != null) {
					reactionMoleculeList.Add (hitBond.transform.gameObject.GetComponent<tutMolecule> ());
				}
			}
		}
		touchRengine.SetPositions (touchRenArr);
	}

	void catalyseReaction() {
		waitForMolecules = 0;
		energyIndex = systemEnergy;
		exoList.Clear ();
		endoList.Clear ();
		foreach (tutMolecule aMol in reactionMoleculeList) {
			if (aMol.atom1.GetComponent<Atom>().atomType==aMol.atom2.GetComponent<Atom>().atomType) {
				exoList.Add (aMol);
			} else {
				endoList.Add (aMol);
			}
		}

		List<Atom> exoAtomList = new List<Atom> ();
		List<Atom> endoAtomList = new List<Atom> ();

		foreach (tutMolecule aMol in reactionMoleculeList) {
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
		foreach (tutMolecule aMol in reactionMoleculeList) {
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
			foreach (tutMolecule aMol in reactionMoleculeList) {
				aMol.isReacting = false;
				aMol.updateRengine();
				aMol.transform.position += new Vector3 (0, 0, 6);
			}
			changePlayer ();
		}
	}

	public void changePlayer() {
		//change player
		if (currentPlayer == 0) {
			currentPlayer = 1;
		} else {
			currentPlayer = 0;
		}
		dragLineRenMat.SetColor("_Color", gmColorList [currentPlayer + 2]);
		changeTuPhase ();
	}

	void changeText(string text){
		StartCoroutine (changeTextCoroutine(text));
	}

	IEnumerator changeTextCoroutine(string text){
		//chatterSpeaker.volume = 1f;
		LOLSDK.Instance.ConfigureSound(1, 1, 1);
		for (int i = 0; i < text.Length; i++) {
			tuText.text = text.Substring (0, i);
			yield return new WaitForSeconds(.04f);
		}
		tuText.text = text;
		//chatterSpeaker.volume = 0;
		LOLSDK.Instance.ConfigureSound(1, 0, 0);
	}


	public void changeTuPhase() {
		if (tuPhase == 2) {
			changeText("Neat! You caused an EXOTHERMIC REACTION—your starting molecules stored more energy than the ones you produced, so some energy was released back into the system.");
			tuPhase = 3;
			Invoke ("changeTuPhase", 10f);
			//lolprogress
			LOLSDK.Instance.SubmitProgress(0, 2, 14);
		} else if (tuPhase == 3) {
			changeText("See? Watch these little guys. They’ll bounce around and shake things up.");
			tuPhase = 4;
			StartCoroutine ("delayedEngMove");
		} else if (tuPhase == 5) {
			changeText("Okay, Dr. Red, your turn. Time to claim those molecules from Dr. Blue. You know what to do—tap and drag to draw a line and break both of the bonds.");
			tuPhase = 6;
			turnReady = true;
			reactionTarget = 2;
		} else if (tuPhase == 6) {
			changeText("Revenge! Those molecules are yours now—note the healthy red glow.");
			tuPhase = 7;
			Invoke ("changeTuPhase", 6f);
		} else if (tuPhase == 7) {
			changeText("That was an ENDOTHERMIC REACTION, so the molecules drank up the energy you released earlier! Of course—because energy is never created or destroyed, it’s always just getting shuffled around.");
			tuPhase = 8;
			Invoke ("changeTuPhase", 12f);
		} else if (tuPhase == 8) {
			changeText("SHAKEUP! I found another molecule. I think it’s your turn, Dr. Blue. Try to cut all three bonds at once.");
			tuPhase = 9;

			GameObject newMolecule = Instantiate (moleculeObj,new Vector3(-0.8f,0.45f,0),Quaternion.identity) as GameObject;
			newMolecule.GetComponent<tutMolecule> ().controlledSpawn (1, chlAt, hydAt);
			newMolecule.GetComponent<tutMolecule> ().populate ();
			newMolecule.GetComponent<tutMolecule> ().playerOwner = 0;
			newMolecule.GetComponent<tutMolecule> ().updateColor ();
			newMolecule.transform.rotation = Quaternion.Euler(0,0,35);
			moleculeList.Add (newMolecule);

			turnReady = true;
			reactionTarget = 3;
		} else if (tuPhase == 9) {
			changeText("Bummer! That extra molecule just fizzled—it didn’t have a mate to react with. Guess that means it still belongs to Red.");
			tuPhase = 10;
			Invoke ("changeTuPhase", 8f);
		} else if (tuPhase == 10) {
			changeText("BONUS QUIZ!!! Who can tell me which molecule holds the most energy?");
			tuPhase = 11;
			foreach (GameObject aMol in moleculeList) {
				Destroy (aMol);
			}
			foreach (GameObject anEn in energyObjList) {
				Destroy (anEn);
			}
			StartCoroutine ("bonusQuiz");
		} else if (tuPhase == 11) {
			changeText("TRICK QUESTION! The answer is D: Both Dichloride and Dihydrogen are worth TWO energy points, where hydrochloric acid is only worth ONE. At least, in this virtual lab—that’s not QUITE how it works in real life.");
			tuPhase = 12;
			quizAsprite.gameObject.SetActive (false);
			quizBsprite.gameObject.SetActive (false);
			quizCsprite.gameObject.SetActive (false);
			quizVsprite.gameObject.SetActive (true);
			//lolprogress
			LOLSDK.Instance.SubmitProgress(0, 3, 14);
			Invoke ("changeTuPhase", 15f);
		} else if (tuPhase == 12) {
			changeText("Pay attention—I’m keeping score! See? Up here: this is the scoreboard.");
			tuPhase = 16;
			quizVsprite.gameObject.SetActive (false);
			guiInd.gameObject.SetActive (true);
			systemEnUI.gameObject.SetActive (true);
			blueEnUI.gameObject.SetActive (true);
			redEnUI.gameObject.SetActive (true);
			redEnUI.text = "1";
			blueEnUI.text = "2";
			systemEnUI.text = "2";
			Invoke ("changeTuPhase", 5f);

		} else if (tuPhase == 16) {
			changeText("It shows how many energy points belong to Dr. Red");
			tuPhase = 17;
			redInd.gameObject.SetActive (true);
			Invoke ("changeTuPhase", 6f);
		} else if (tuPhase == 17) {
			changeText("Dr. Blue");
			tuPhase = 18;
			redInd.gameObject.SetActive (false);
			blueInd.gameObject.SetActive (true);
			Invoke ("changeTuPhase", 4f);
		} else if (tuPhase == 18) {
			changeText(tuText.text = "and how much neutral energy exists in the system.");
			tuPhase = 13;
			blueInd.gameObject.SetActive (false);
			goldInd.gameObject.SetActive (true);
			Invoke ("changeTuPhase", 5f);
		} else if (tuPhase == 13) {
			goldInd.gameObject.SetActive (false);
			changeText("You two geniuses get to go head to head, causing all kinds of wild reactions until the turn timer runs out and I tally up who’s got the most stored energy.");
			tuPhase = 14;
			guiInd.gameObject.SetActive (false);
			Invoke ("changeTuPhase", 10f);
		} else if (tuPhase == 14) {
			changeText("STRATEGY! SKILL! EXCITING! Ready? LET’S DO IT!");
			Invoke ("startGame", 4f);
			//lolprogress
			LOLSDK.Instance.SubmitProgress(0, 4, 14);
		} 
	}

	IEnumerator delayedEngMove() {
		yield return new WaitForSeconds (5f);
		LOLSDK.Instance.PlaySound ("EnergyMove.mp3");
		//managerSpeaker.PlayOneShot (energySwoosh);
		camShaker.shake (2f, 2f);
		energyObjList [0].GetComponent<Energy>().controlledMove (5.2f, 2.8f);
		energyObjList [1].GetComponent<Energy>().controlledMove (4.0f, 2.1f);
		tuPhase = 5;
		Invoke ("changeTuPhase", 3f);
	}

	IEnumerator bonusQuiz() {
		yield return new WaitForSeconds (5f);
		changeText("Is it A: Hydrochloric Acid");
		GameObject newMolecule = Instantiate (moleculeObj,new Vector3(-4f,0,0),Quaternion.identity) as GameObject;
		newMolecule.GetComponent<tutMolecule> ().controlledSpawn (1, chlAt, hydAt);
		newMolecule.GetComponent<tutMolecule> ().populate ();
		newMolecule.GetComponent<tutMolecule> ().playerOwner = 0;
		newMolecule.GetComponent<tutMolecule> ().updateColor ();
		newMolecule.transform.rotation = Quaternion.Euler(0,0,60);
		quizAsprite.gameObject.SetActive (true);
		yield return new WaitForSeconds (4f);
		changeText("B: Dichloride");
		GameObject newMolecule2 = Instantiate (moleculeObj,new Vector3(0,0,0),Quaternion.identity) as GameObject;
		newMolecule2.GetComponent<tutMolecule> ().controlledSpawn (1, chlAt, chlAt);
		newMolecule2.GetComponent<tutMolecule> ().populate ();
		newMolecule2.GetComponent<tutMolecule> ().playerOwner = 1;
		newMolecule2.GetComponent<tutMolecule> ().updateColor ();
		newMolecule2.transform.rotation = Quaternion.Euler(0,0,90);
		quizBsprite.gameObject.SetActive (true);
		yield return new WaitForSeconds (4f);
		changeText("or C: Dihydrogen?");
		GameObject newMolecule3 = Instantiate (moleculeObj,new Vector3(4,0,0),Quaternion.identity) as GameObject;
		newMolecule3.GetComponent<tutMolecule> ().controlledSpawn (2, hydAt, hydAt);
		newMolecule3.GetComponent<tutMolecule> ().populate ();
		newMolecule3.GetComponent<tutMolecule> ().playerOwner = 2;
		newMolecule3.GetComponent<tutMolecule> ().updateColor ();
		newMolecule3.transform.rotation = Quaternion.Euler(0,0,120);
		quizCsprite.gameObject.SetActive (true);
		yield return new WaitForSeconds (4f);
		changeTuPhase ();
	}

	public void startGame() {
		SceneManager.LoadScene ("Main");
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
			//GameObject newParticle = Instantiate (TutorialManager.instance.energyParticle, fedPos, Quaternion.identity) as GameObject;
			energyObjList.Add (newEnergy);
			newEnergy.GetComponent<Energy> ().isTutEng = true;
			newEnergy.GetComponent<Energy> ().controlledMove (1f,1f);
		}
	}

}
