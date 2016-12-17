using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// required to switch scenes
using UnityEngine.SceneManagement;

// required for UI manipulation
using UnityEngine.UI;

public class MainGameController : MonoBehaviour
{

	private GameObject player;
	private WeaponController weaponControl;
	private Text statusText;
	private Color statusColor;
	[HideInInspector]
	public int numZombies;
	public int maxNumZombies;
	public int killsRequired;
	[HideInInspector]
	public int zombiesKilled = 0;
	public bool endless;
	[HideInInspector]
	public float endlessMultiplier = 1f;


	// Use this for initialization
	void Start ()
	{
		player = GameObject.FindGameObjectWithTag ("Player");
		weaponControl = player.GetComponentInChildren<WeaponController> ();
		statusText = GameObject.Find ("StatusMessage").GetComponent<Text> ();
		statusColor = statusText.color;   
		statusText.text = "";
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKey (KeyCode.Escape)) {
			SceneManager.LoadScene ("MainMenu", LoadSceneMode.Single);
		}
	}

	public void bossDead (GameObject boss, string nextScene)
	{

		StartCoroutine (setStatusFor ("Boss is DEAD!", true, 5f));
		print ("Boss is dead.. go to next scene, which is: " + nextScene);
	}



	public void zombieDead (GameObject zombie)
	{

		zombiesKilled++;
		numZombies--;
		if (zombiesKilled < 5) {
			StartCoroutine (setStatusFor ("Zombie killed (" + zombiesKilled + ")", false, 2f));
		} else if (zombiesKilled >= 5 && zombiesKilled < 10) {
			StartCoroutine (setStatusFor ("Zombie EXERMINATOR! (" + zombiesKilled + ")", false, 2f));
		} else if (zombiesKilled >= 10 && zombiesKilled < 14) {
			StartCoroutine (setStatusFor ("Hey you zombies, run to the hill!! (" + zombiesKilled + ")", false, 2f));
		} else {
			StartCoroutine (setStatusFor ("Good bye blood sky! (" + zombiesKilled + ")", false, 2f));
		}
	
		if (weaponControl.playerHitPoints <= 0) {
			StartCoroutine (setStatusFor ("You died!", true, 5f));
			StartCoroutine (MissionComplete ());
		}

		if (endless) {
			endlessMultiplier *= (1 + Mathf.Floor ((float)zombiesKilled / 12) * 0.1f);
		}
		else if (!endless && zombiesKilled >= killsRequired) {
			StartCoroutine (setStatusFor ("Mission Complete!", true, 5f));
			StartCoroutine (MissionComplete ());
		}
	}

	public IEnumerator setStatusFor (string message, bool important, float seconds)
	{

		if (important) {
			statusText.fontStyle = FontStyle.Bold;
		} else {
			statusText.fontStyle = FontStyle.Normal;
		}

		statusText.text = message;
		yield return new WaitForSeconds (seconds);

		statusText.CrossFadeAlpha (0f, 1f, false);
		yield return new WaitForSeconds (1f);

		statusText.text = "";
		statusText.canvasRenderer.SetAlpha (1f);
	}


	IEnumerator MissionComplete ()
	{
		Time.timeScale = 0.2f;
		yield return new WaitForSeconds (0.5f);
		Time.timeScale = 1f;
		SceneManager.LoadScene ("MainMenu", LoadSceneMode.Single);
	}

}
