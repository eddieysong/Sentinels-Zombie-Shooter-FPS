/// <summary>
/// Instructions.cs
/// Last Modified: 2016-12-17
/// Created By: Thiago
/// Last Modified By: Thiago
/// Summary: this script handles the single button on the "How to Play" screen.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Instructions : MonoBehaviour {

	public Button mainMenuButton;

	void Start ()
	{
		// game levels hide the cursor
		Cursor.visible = true;

		// assign methods to the buttons
		Button mainButton = mainMenuButton.GetComponent<Button> ();
		mainButton.onClick.AddListener (BackToMainMenu);

	}

	// called when Main Menu button is clicked
	void BackToMainMenu ()
	{
		SceneManager.LoadScene ("MainMenu", LoadSceneMode.Single);
	}
}
