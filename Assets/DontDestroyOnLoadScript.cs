using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadScript : MonoBehaviour {

	void Awake() {
		DontDestroyOnLoad (transform.gameObject);
	}
}
