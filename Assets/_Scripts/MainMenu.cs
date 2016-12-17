using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

	public Dropdown levelDropdown;
	public Dropdown classDropdown;
	public Button startButton;
	public static int classID;

	void Start ()
	{
//		DontDestroyOnLoad (transform.gameObject);
		Button btn = startButton.GetComponent<Button> ();
		btn.onClick.AddListener (StartGame);
		Cursor.visible = true;
	}

	void StartGame ()
	{
		classID = classDropdown.value;
		if (levelDropdown.value == 0) {
			SceneManager.LoadScene ("HospitalLevel", LoadSceneMode.Single);
		} else if (levelDropdown.value == 1) {
//			SceneManager.LoadScene ("MazeRunnerLevel", LoadSceneMode.Single);
		} else if (levelDropdown.value == 2) {
			SceneManager.LoadScene ("MazeRunnerLevel", LoadSceneMode.Single);
		}
	}
}