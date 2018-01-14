using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class textBoardManager : MonoBehaviour {

	enum Difficulties{easy, medium, hard, nightmare};
	Difficulties difficulty;

	enum Directions{up, down, left, right, nothing};
	Directions direction;
	Directions lastMove;

	public GameObject snek;
	public GameObject snekTailPrefab;
	private Snek snekCtrl;
	public float snekSpeed;
	public int snekLength;

	public GameObject textDisplay;
	public GameObject keyDisplay;
	public GameObject textLetterMarker;
	public GameObject keyLetterMarker;
	public GameObject scoreLabel;
	public GameObject border;

	public List<GameObject> levels;
	int level;
	int score;

	string currentMessage;
	string currentKey;
	string loopedKey;
	int msgIndex;
	int keyIndex;
	int goalsReached;

	public int snekRow;
	public int snekCol;

	bool begunMoving;

	// Use this for initialization
	void Start () {
		snekCtrl = snek.GetComponent<Snek> ();
		snekSpeed = snekCtrl.defaultSpeed;

		difficulty = Difficulties.easy;
		level = 0;
		score = 0;

		newLevel ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			if (lastMove != Directions.right) {
				direction = Directions.left;
			}
			if (!begunMoving) {
				InvokeRepeating ("moveSnek", 0, snekSpeed);
				begunMoving = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			if (lastMove != Directions.left) {
				direction = Directions.right;
			}
			if (!begunMoving) {
				InvokeRepeating ("moveSnek", 0, snekSpeed);
				begunMoving = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			if (lastMove != Directions.down) {
				direction = Directions.up;
			}
			if (!begunMoving) {
				InvokeRepeating ("moveSnek", 0, snekSpeed);
				begunMoving = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			if (lastMove != Directions.up) {
				direction = Directions.down;
			}
			if (!begunMoving) {
				InvokeRepeating ("moveSnek", 0, snekSpeed);
				begunMoving = true;
			}
		}

		snekRow = (int)Mathf.Abs (snek.GetComponent<Transform> ().position.y);
		snekCol = (int)Mathf.Abs(snek.GetComponent<Transform> ().position.x);

	}

	void moveSnek() {
		Vector3 prevPosition = new Vector3(snek.transform.position.x, snek.transform.position.y, snek.transform.position.z);


		#region directional processing
		if (direction == Directions.up) {
			if (snek.transform.position.y >= 0) { // hitting edge
				if (difficulty == Difficulties.easy) { // wrap around
					snek.transform.position = new Vector2 (snek.transform.position.x, -25);
				} else { // game over
					gameOver();
				}

			} else { // normal up move
				snek.transform.position = new Vector2 (snek.transform.position.x, snek.transform.position.y + 1);
			}
			lastMove = Directions.up;
		}
		if (direction == Directions.down) {
			if (snek.transform.position.y <= -25) { // hitting edge
				if (difficulty == Difficulties.easy) { // wrap around
					snek.transform.position = new Vector2 (snek.transform.position.x, 0);
				} else { // game over
					gameOver();
				}

			} else { // normal down move
				snek.transform.position = new Vector2 (snek.transform.position.x, snek.transform.position.y - 1);
			}			lastMove = Directions.down;
		}
		if (direction == Directions.left) {
			if (snek.transform.position.x <= 0) { // hitting edge
				if (difficulty == Difficulties.easy) { // wrap around
					snek.transform.position = new Vector2 (25, snek.transform.position.y);
				} else { // game over
					gameOver();
				}

			} else { // normal left move
				snek.transform.position = new Vector2 (snek.transform.position.x - 1, snek.transform.position.y);
			}
			lastMove = Directions.left;
		}
		if (direction == Directions.right) {
			if (snek.transform.position.x >= 25) { // hitting edge
				if (difficulty == Difficulties.easy) { // wrap around
					snek.transform.position = new Vector2 (0, snek.transform.position.y);
				} else { // game over
					gameOver();
				}

			} else { // normal right move
				snek.transform.position = new Vector2 (snek.transform.position.x + 1, snek.transform.position.y);
			}
			lastMove = Directions.right;
		}
		#endregion

		// create tail piece
		GameObject clone = GameObject.Instantiate (snekTailPrefab, prevPosition, Quaternion.identity);
		float cloneTTL = snekLength * snekSpeed;
		Destroy (clone, cloneTTL);

		// check tail collisions
		GameObject[] tails = GameObject.FindGameObjectsWithTag("Tail");
		foreach (GameObject tail in tails) {
			if (snek.transform.position.x == tail.transform.position.x && snek.transform.position.y == tail.transform.position.y) {
				gameOver ();
			}
		}

		if (checkGoal()) {
			score += 10;
			scoreLabel.GetComponent<TextMeshPro> ().text = string.Format ("score: {0}", score);

			goalsReached++;
			snekLength += 2;
			if (goalsReached < currentMessage.Length) {
				updateGoalIndexes (goalsReached);
			} else {
				CancelInvoke ("moveSnek");
				level++;
				snekSpeed /= 1.75f; // higher the number, smaller the speed increase
				newLevel ();
			}
		}
	}

	void newLevel() {
		if (level == levels.Count) {
			SceneManager.LoadScene ("winScreen");
			return;
		}

		goalsReached = 0;
		begunMoving = false;
		lastMove = Directions.nothing;

		// destroy old tail pieces
		GameObject[] oldTails = GameObject.FindGameObjectsWithTag("Tail");
		foreach (GameObject tail in oldTails) {
			Destroy (tail);
		}

		updateDifficulty ();

		currentMessage = getMessage(level);
		currentKey = getKey (level);
		setMessages (currentMessage, currentKey);
		updateGoalIndexes (0);

		// reset snek
		snek.transform.position = new Vector2 (12f, -12f);
		direction = Directions.nothing;
		snekLength = snekCtrl.defaultLength;
	}

	bool checkGoal() { // returns true if player moves onto the right space for the current letter
		int snekX = (int)Mathf.Abs(snek.GetComponent<Transform> ().position.x);
		int snekY = (int)Mathf.Abs (snek.GetComponent<Transform> ().position.y);
		if (snekX == keyIndex && snekY == msgIndex) {
			return true;
		} else {
			return false;
		}
	}

	void updateDifficulty() {
		// check the level which is being loaded. Difficulty changes at specific levels
		if (level == 2) { // go to medium difficulty
			difficulty = Difficulties.medium;
			border.GetComponent<TextMeshPro> ().color = new Color (255, 255, 255, 255);
		}
		if (level == 4) { // go to hard difficulty
			difficulty = Difficulties.hard;
			textLetterMarker.GetComponent<MeshRenderer> ().material.color = new Color(0F, 0F, 0F, 0F);
			keyLetterMarker.GetComponent<MeshRenderer> ().material.color = new Color (0F, 0F, 0F, 0F);
		}
		if (level == 6) { // go to nightmare difficulty
			difficulty = Difficulties.nightmare;
		}
	}

	public void gameOver() {
		SceneManager.LoadScene ("gameOver");
	}

	void updateGoalIndexes(int index) {
		msgIndex = getLetterIndex (currentMessage [index]);
		keyIndex = getLetterIndex (loopedKey [index]);
		textLetterMarker.GetComponent<Transform> ().localPosition = new Vector3 (0.3f + index, 1.5f, 1f);
		keyLetterMarker.GetComponent<Transform> ().localPosition = new Vector3 (0.3f + index, 1.5f, 1f);
	}

	string getMessage(int lvl) {
		return (levels [lvl].GetComponent<LevelManager> ().text);
	}
	string getKey (int lvl) {
		return (levels [lvl].GetComponent<LevelManager> ().key);
	}

	void setMessages(string msg, string key) {
		textDisplay.GetComponent<TextMeshPro> ().text = msg;

		string displayKey = "";
		int keyIndexer = 0;
			for (int i = 0; i < msg.Length; i++) {
				if (keyIndexer >= key.Length) {
					keyIndexer = 0;
				}
				displayKey += key.Substring(keyIndexer, 1);
				keyIndexer++;
			}
		loopedKey = displayKey;
		if (difficulty == Difficulties.nightmare) { // show looping key
			keyDisplay.GetComponent<TextMeshPro>().text = key;
		} else { // show key as-is
			keyDisplay.GetComponent<TextMeshPro>().text = displayKey;
		}
	}

	int getLetterIndex(char c) {
		string s = c.ToString ();
		s = s.ToLower ();
		switch (s) {
		case "a":
			return 0;
		case "b":
			return 1;
		case "c":
			return 2;
		case "d":
			return 3;
		case "e":
			return 4;
		case "f":
			return 5;
		case "g":
			return 6;
		case "h":
			return 7;
		case "i":
			return 8;
		case "j":
			return 9;
		case "k":
			return 10;
		case "l":
			return 11;
		case "m":
			return 12;
		case "n":
			return 13;
		case "o":
			return 14;
		case "p":
			return 15;
		case "q":
			return 16;
		case "r":
			return 17;
		case "s":
			return 18;
		case "t":
			return 19;
		case "u":
			return 20;
		case "v":
			return 21;
		case "w":
			return 22;
		case "x":
			return 23;
		case "y":
			return 24;
		case "z":
			return 25;
		default:
			return -1;
		}
	}

	/* Future developments
	 * - Difficulty ideas
	 *   - completed "goal" spaces become hazards
	 *   - letters on message and key axis displays disappear
	 *   - encrypted message given. player must find original message
	 * - Difficulty displayed?
	 * - Tutorial
	 * - Game over screen w/ score
	 * - Victory screen w/ score
	 * */
}
