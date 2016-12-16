using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// required for UI manipulation
using UnityEngine.UI;

public class MainGameController : MonoBehaviour {

  private GameObject player;
  private WeaponController weaponControl;
  private Text statusText;
  private Color statusColor;
	[HideInInspector]
	public int numZombies;
	public int maxNumZombies;

  // Use this for initialization
  void Start() {
    player = GameObject.FindGameObjectWithTag("Player");
    weaponControl = player.GetComponentInChildren<WeaponController>();
    statusText = GameObject.Find("StatusMessage").GetComponent<Text>();
    statusColor = statusText.color;   
    statusText.text = "";
  }
	
  // Update is called once per frame
  void Update() {
		
  }

  public void bossDead(GameObject boss, string nextScene) {

    StartCoroutine(setStatusFor("Boss is DEAD!", true, 5f));
    print("Boss is dead.. go to next scene, which is: " + nextScene);
  }

  private int zombiesKilled = 0;

  public void zombieDead(GameObject zombie) {

    zombiesKilled++;
		numZombies--;
    if (zombiesKilled < 5) {
      StartCoroutine(setStatusFor("Zombie killed (" + zombiesKilled + ")", false, 2f));
    } else if (zombiesKilled >= 5 && zombiesKilled < 10) {
      StartCoroutine(setStatusFor("Zombie EXERMINATOR! (" + zombiesKilled + ")", false, 2f));
    } else if (zombiesKilled >= 10 && zombiesKilled < 14) {
      StartCoroutine(setStatusFor("Hey you zombies, run to the hill!! (" + zombiesKilled + ")", false, 2f));
    } else {
      StartCoroutine(setStatusFor("Good bye blood sky! (" + zombiesKilled + ")", false, 2f));
    }
  }

  public IEnumerator setStatusFor(string message, bool important, float seconds) {

    if (important) {
      statusText.fontStyle = FontStyle.Bold;
    } else {
      statusText.fontStyle = FontStyle.Normal;
    }

    statusText.text = message;
    yield return new WaitForSeconds(seconds);

    statusText.CrossFadeAlpha(0f, 1f, false);
    yield return new WaitForSeconds(1f);

    statusText.text = "";
    statusText.canvasRenderer.SetAlpha(1f);
  }

}
