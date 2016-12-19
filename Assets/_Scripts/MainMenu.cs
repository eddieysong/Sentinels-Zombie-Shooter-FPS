using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

	public Dropdown levelDropdown;
	public Dropdown classDropdown;
	public Button startButton;
	public Button instructionsButton;
	public Button exitButton;

	// use a static variable to store the player's class selection
	// then instantiate this class (MainMenu) during the level to read this variable
	// because DontDestroyOnLoad causes buggy behavior.
	public static int classID;


	void Start ()
	{
		// game levels hide the cursor
		Cursor.visible = true;

		// assign methods to the buttons
		Button start = startButton.GetComponent<Button> ();
		start.onClick.AddListener (StartGame);

		Button instructions = instructionsButton.GetComponent<Button> ();
		instructions.onClick.AddListener (ViewInstructions);

		Button exit = exitButton.GetComponent<Button> ();
		exit.onClick.AddListener (ExitGame);

	}

	// called when Start Game button is clicked
	void StartGame ()
	{
		// store class selection in static variable classID
		classID = classDropdown.value;

		// go to level specified by dropdown
		if (levelDropdown.value == 0) {
			SceneManager.LoadScene ("HospitalLevel", LoadSceneMode.Single);
		} else if (levelDropdown.value == 1) {
			SceneManager.LoadScene ("InfectedForest", LoadSceneMode.Single);
		} else if (levelDropdown.value == 2) {
			SceneManager.LoadScene ("MazeRunnerLevel", LoadSceneMode.Single);
		}
	}

	// called when Instructions button is clicked
	void ViewInstructions ()
	{
		SceneManager.LoadScene ("Instructions", LoadSceneMode.Single);
	}

	// called when Exit button is clicked
	void ExitGame ()
	{
		// Tom's requirements include this function,
		// even though modern applications, esp. mobile apps don't do this very often,
		// but he's the boss, and I'm just a lowly code peasant.
		// He's never gonna read this though, I bet.
		Application.Quit();
	}
}