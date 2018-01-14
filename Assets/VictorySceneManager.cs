using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VictorySceneManager : MonoBehaviour {

	GameObject scoreLabel;
	string scoreString;
	int score;

	public GameObject victoryMessage; // so it can be customised with some random fun messages
	public GameObject victoryScore;

	// Use this for initialization
	void Start () {
		// get the value of score then remove the old label
		scoreLabel = GameObject.FindGameObjectWithTag ("Score");
		scoreString = scoreLabel.GetComponent<TextMeshPro> ().text.Substring (6);
		score = int.Parse (scoreString);
		scoreLabel.SetActive (false);

		victoryScore.GetComponent<TextMeshPro> ().text = string.Format ("score:\n{0}", score);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
