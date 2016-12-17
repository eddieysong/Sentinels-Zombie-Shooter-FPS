using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// required for UI manipulation
using UnityEngine.UI;

// required for camera effects
using UnityStandardAssets.ImageEffects;

// required to access FPSController script
using UnityStandardAssets.Characters.FirstPerson;

public class WeaponController : MonoBehaviour
{

	// indicate the player's class
	[SerializeField]
	private string playerClass;


	private string[] playerClasses = new string [] { "Assassin", "Crusader", "Shadow" };
	private int playerClassIndex = 0;
	private float[] classMoveSpeed = new float[] { 4.5f, 3f, 3.5f };
	private string[] classAbilities = new string [] { "Adrenaline Rush", "Fortress", "Phase Walk" };
	private float[] classAbilityCooldown = new float[] { 30f, 30f, 30f };
	private int[] classPrimaryWeaponID = new int[] { 1, 0, 2 };
	private int[] classSecondaryWeaponID = new int[] { 2, 2, 2 };


	private float damageReduction = 0f;
	private float difficultyDmgMultPlayer = 1f;
	private float difficultyDmgMultZombie = 1f;


	private float abilityTimer;

	// we need to change a few values in FirstPersonController
	private FirstPersonController FPSController;

	// indicates which weapon is active
	private int activeWeaponID = 0;

	// indicates which loadout the player is using
	private int primaryWeaponID = 1;
	private int secondaryWeaponID = 2;

	// things that need to be defined in the inspector
	[SerializeField]
	private GameObject[] weaponModels;
	[SerializeField]
	private AudioClip[] fireAudioClips;

	[SerializeField]
	private Transform muzzleFlashPoint;
	[SerializeField]
	private ParticleSystem muzzleFlash;
	[SerializeField]
	private ParticleSystem concreteImpactPrefab;
	[SerializeField]
	private ParticleSystem[] concreteImpactDecal;
	[SerializeField]
	private AudioClip[] concreteImpactAudio;
	[SerializeField]
	private ParticleSystem fleshImpactPrefab;
	[SerializeField]
	private ParticleSystem fleshImpactDecal;
	[SerializeField]
	private AudioClip[] fleshImpactAudio;
	[SerializeField]
	private AudioClip reloadSound;
	[SerializeField]
	private AudioClip playerHurtSound;

	//	[SerializeField]
	//	private GameObject bloodSplat;

	[SerializeField]
	public float playerHitPoints = 100f;
	[SerializeField]
	private float playerStamina = 100f;



	private string[] weaponNames = new string[] { "AK-47", "M4A1", "UMP-45" };
	private float[] weaponDamages = new float[] { 15.0f, 12.0f, 7.5f };
	private float[] fireCooldowns = new float[] { 0.12f, 0.1f, 0.08f };
	private float[] recoilFactors = new float[] { 0.75f, 0.5f, 0.3f };
	private int[] ammo = new int[3] { 30, 30, 35 };
	private int[] ammoReserve = new int[3] { 120, 150, 210 };
	private int[] ammoMax = new int[3] { 30, 30, 35 };
	private GameObject crosshair;
	private Text healthDisplay;
	private Text weaponNameDisplay;
	private Text ammoDisplay;
	private Text skillText;
	private Animator mainMessage;
	private Animator mainMessageOuter;
	private Text mainMessageText;
	private Text mainMessageOuterText;




	private GameObject weaponsWrapper;
	private AudioSource weaponAudioSource;
	private Animator weaponSwapAnimator;


	private RaycastHit impact;
	private float impactDistance;
	private GameObject impactObject;
	private ZombieController zombieControl;

	// find the camera filters
	private MotionBlur motionBlurFilter;
	private Grayscale greyscaleFilter;
	private NoiseAndGrain noiseAndGrainFilter;
	private AudioReverbFilter audioReverbFilter;



	private float fireTimer = 0f;
	private Vector3 recoilRotation;
	private float recoilMultiplier = 1f;
	private float weaponSpreadMultiplier = 1f;
	private float threatMultiplier = 1.0f;
	private int killStreak = 0;
	private int headshotStreak = 0;
	private float lastKillTime;
	private float lastHeadshotTime;

	private MainMenu mainMenu;








	// Use this for initialization
	void Start ()
	{
		// find FPSController script
		FPSController = GameObject.FindGameObjectWithTag ("Player").GetComponent<FirstPersonController> ();

		// find main menu script
//		GameObject mainMenuObject = GameObject.Find ("MainMenuScript");
//		if (mainMenuObject) {
//			mainMenu = mainMenuObject.GetComponent<MainMenu> ();
//			// find class ID
//			playerClassIndex = mainMenu.classDropdown.value;
//		}
		playerClassIndex = MainMenu.classID;
		// initialize class
		InitializeClass (playerClassIndex);
		playerClass = playerClasses [playerClassIndex];

		// set all weapons to invisible then set active to primary and show it
		foreach (GameObject weapon in weaponModels) {
			weapon.SetActive (false);
		}

		// select primary weapon and set it to active
		activeWeaponID = primaryWeaponID;
		weaponModels [activeWeaponID].SetActive (true);

		// find the weapon wrapper
		weaponsWrapper = GameObject.Find ("Weapons");

		// find audio source on the weapon wrapper
		weaponAudioSource = weaponsWrapper.GetComponent<AudioSource> ();

		// find the animator used in weapon swapping
		weaponSwapAnimator = weaponsWrapper.GetComponent<Animator> ();

		// find the HUD elements
		crosshair = GameObject.Find ("Crosshair");
		healthDisplay = GameObject.Find ("HealthDisplay").GetComponent<Text> ();
		ammoDisplay = GameObject.Find ("AmmoDisplay").GetComponent<Text> ();
		weaponNameDisplay = GameObject.Find ("WeaponName").GetComponent<Text> ();
		skillText = GameObject.Find ("SkillText").GetComponent<Text> ();
		mainMessage = GameObject.Find ("MainMessage").GetComponent<Animator> ();
		mainMessageOuter = GameObject.Find ("MainMessageOuter").GetComponent<Animator> ();
		mainMessageText = GameObject.Find ("MainMessage").GetComponent<Text> ();
		mainMessageOuterText = GameObject.Find ("MainMessageOuter").GetComponent<Text> ();




		// find the camera filters
		motionBlurFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<MotionBlur> ();
		greyscaleFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<Grayscale> ();
		noiseAndGrainFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<NoiseAndGrain> ();
		audioReverbFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<AudioReverbFilter> ();

		// turn off the camera filters until we need them
		motionBlurFilter.enabled = false;
		greyscaleFilter.enabled = false;
		noiseAndGrainFilter.enabled = false;
		audioReverbFilter.enabled = false;
	}

	void InitializeClass (int classIndex)
	{
		primaryWeaponID = classPrimaryWeaponID [classIndex];
		secondaryWeaponID = classSecondaryWeaponID [classIndex];

		FPSController.SendMessage ("SetSpeed", classMoveSpeed [playerClassIndex], SendMessageOptions.DontRequireReceiver);
	}

	// Update is called once per frame
	void Update ()
	{

		// things that happen every frame
		RecoilFalloff ();
		UpdateCrosshairRecoil ();
		UpdateHUD ();


    if (!FPSController.m_IsWalking) {
      playerStamina = Mathf.Clamp(playerStamina - Time.deltaTime * 10, 0f, 100f);
      if (playerStamina <= 0) {
        FPSController.exhausted = true;
      }
    } else {
      playerStamina = Mathf.Clamp(playerStamina + Time.deltaTime * 5, 0f, 100f);
      if (playerStamina >= 30) {
        FPSController.exhausted = false;
      }
    }
		
		if (weaponSwapAnimator.GetCurrentAnimatorStateInfo (0).IsName ("WeaponSwap2")) {
			foreach (GameObject weapon in weaponModels) {
				weapon.SetActive (false);
			}
			weaponModels [activeWeaponID].SetActive (true);
		}

		bool weaponAnimationFinished = weaponSwapAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Idle");

		// required for recoil animation to work properly
		weaponSwapAnimator.applyRootMotion = weaponAnimationFinished;

		// actions that are only available if weapon not on cooldown
		if (Time.time > fireTimer && weaponAnimationFinished) {
			// left mouse click
			if (Input.GetButton ("Fire1")) {
				if (ammo [activeWeaponID] > 0) {
					Fire ();
				} else {
					ReloadWeapon ();
				}
			}

			// swap weapon
			if (Input.GetKeyDown (KeyCode.Q)) {
				SwapWeapon ();
			}

			// reload weapon
			if (Input.GetKeyDown (KeyCode.R)) {
				ReloadWeapon ();
			}
		}

		if (Input.GetKeyDown (KeyCode.F)) {
			UseActiveAbility ();
		}

		if (Input.GetKeyDown (KeyCode.V)) {
			StartCoroutine ("PhaseWalk");

		}
	}

	// uses active ability according to the player's class
	void UseActiveAbility ()
	{
		if (abilityTimer < Time.time) {
			abilityTimer = Time.time + classAbilityCooldown [playerClassIndex];
			switch (playerClass) {
			case "Assassin":
				StartCoroutine ("AdrenalineRush");
				break;
			case "Crusader":
				StartCoroutine ("Fortress");
				break;
			case "Shadow":
				StartCoroutine ("PhaseWalk");
				break;
			}
		}
	}

	IEnumerator AdrenalineRush ()
	{
		if (Time.timeScale == 1.0F) {
			Time.timeScale = 0.25F;
			weaponSwapAnimator.speed = 1 / Time.timeScale;

			motionBlurFilter.enabled = true;
			audioReverbFilter.enabled = true;

			yield return new WaitForSeconds (2.5f);

			Time.timeScale = 1.0F;
			weaponSwapAnimator.speed = 1 / Time.timeScale;

			motionBlurFilter.enabled = false;
			audioReverbFilter.enabled = false;
		}
	}

	IEnumerator Fortress ()
	{
		damageReduction = 0.5f;
		weaponSpreadMultiplier = 0.25f;
		FPSController.SendMessage ("SetSpeed", classMoveSpeed [playerClassIndex] * 0.1f, SendMessageOptions.DontRequireReceiver);
		GameObject mainCam = GameObject.Find ("FirstPersonCharacter");
		Vector3 mainPos = mainCam.transform.localPosition;
		mainCam.transform.localPosition = new Vector3 (mainPos.x, mainPos.y, mainPos.z + 0.5f);
		Vector3 weaponsPos = weaponsWrapper.transform.localPosition;
		weaponsWrapper.transform.localPosition = new Vector3 (weaponsPos.x, weaponsPos.y, weaponsPos.z - 0.3f);

		yield return new WaitForSeconds (10.0f);

		damageReduction = 0f;
		weaponSpreadMultiplier = 1f;
		FPSController.SendMessage ("SetSpeed", classMoveSpeed [playerClassIndex], SendMessageOptions.DontRequireReceiver);
		mainCam.transform.localPosition = mainPos;
		weaponsWrapper.transform.localPosition = weaponsPos;

	}

	IEnumerator PhaseWalk ()
	{
		if (threatMultiplier == 1.0F) {
			threatMultiplier = 0.1F;
			greyscaleFilter.enabled = true;
			noiseAndGrainFilter.enabled = true;

			yield return new WaitForSeconds (10.0f);

			threatMultiplier = 1.0F;
			greyscaleFilter.enabled = false;
			noiseAndGrainFilter.enabled = false;
		}
	}

	void Fire ()
	{
		
		float scaleLimit = (recoilMultiplier - 1) * weaponSpreadMultiplier;
			
		//  Generate a random XY point inside a circle:
		Vector3 direction = Random.insideUnitCircle * scaleLimit;
		direction.z = 35f; // circle is at Z units 
		direction = Camera.main.transform.TransformDirection (direction.normalized);    

		// use FireRay method only if the raycast returns a hit, otherwise don't bother.
		if (Physics.Raycast (Camera.main.transform.position, direction, out impact)) {
			impactDistance = impact.distance;
			impactObject = impact.transform.gameObject;
			FireRay ();
		}
		// put weapon on cooldown according to which type of weapon it is
		fireTimer = Time.time + fireCooldowns [activeWeaponID];

		// play weapon sound once
		weaponAudioSource.PlayOneShot (fireAudioClips [activeWeaponID]);

		// instantiate a muzzle flash
		Instantiate (muzzleFlash, muzzleFlashPoint.position, Random.rotation);

		// adds some recoil with every shot fired
		RecoilTrigger ();

		// use one round of ammo
		ammo [activeWeaponID] -= 1;

	}

	// handles firing logic related to instant projectile hits (e.g. guns)
	void FireRay ()
	{

		Quaternion quatAngle = Quaternion.LookRotation (impact.normal);

//		Quaternion quatAngle = Quaternion.LookRotation( impact.normal );
//		Instantiate( concreteImpactPrefab, impact.point, quatAngle );

		if (impactObject.transform.root.CompareTag ("Concrete")) {
			Instantiate (concreteImpactPrefab, impact.point, quatAngle);
//			Instantiate(concreteImpactDecal[Random.Range(0, concreteImpactAudio.Length)], impact.point, quatAngle);
//			ParticleSystem decal = (ParticleSystem) Instantiate( concreteImpactDecal[Random.Range(0, concreteImpactAudio.Length)], impact.point + impact.normal * 0.020f, quatAngle );
//			decal.transform.parent = impact.transform.gameObject.transform;	// parent the decal to the object

      AudioSource.PlayClipAtPoint(concreteImpactAudio[Random.Range(0, concreteImpactAudio.Length)], impact.point);
    }

//    if (impactObject.transform.root.CompareTag("Zombie")) {
    if (hasParentWithTag(impactObject, "Zombie")) {
      Instantiate(fleshImpactPrefab, impact.point, quatAngle);
//			Instantiate(fleshImpactDecal, impact.point, Quaternion.identity);
			AudioSource.PlayClipAtPoint (fleshImpactAudio [Random.Range (0, fleshImpactAudio.Length)], impact.point);
//			Debug.Log ("hit");
//			Instantiate (bloodSplat, impact.point, Quaternion.identity);
			zombieControl = impactObject.GetComponentInParent<ZombieController> ();
			zombieControl.SendMessage ("HitByBullet", new object [] { impactObject.name, weaponDamages [activeWeaponID] }, SendMessageOptions.DontRequireReceiver);
		}
	}

	// this fixes a bug that happens when there is an object grouping the enemies
	bool hasParentWithTag (GameObject gameObject, string tag)
	{

    Transform root = gameObject.transform.root;
    Transform current = gameObject.transform;

    try {
      do {

 if (current.CompareTag(tag)) {
          return true;
        }
        current = current.parent;

      } while (current != root);
    } catch (System.NullReferenceException ex) {
      return false;
    }

    if (current.CompareTag(tag)) {
      return true;
    }

    return false;
  }

	// handles firing logic of projectiles that are not instantly triggered (e.g. explosives)
	void FireProjectile ()
	{

	}

	// adds a bit of recoil force
	void RecoilTrigger ()
	{
		recoilMultiplier += recoilFactors [activeWeaponID] * weaponSpreadMultiplier;
		recoilRotation.x += Random.value * recoilMultiplier;
		recoilRotation.y += Random.value * 0.5f * recoilMultiplier;
		recoilRotation.z += Random.value * 0.5f * recoilMultiplier;
	}

	// reduces recoil by a factor every frame
	void RecoilFalloff ()
	{
		recoilMultiplier = Mathf.Clamp (recoilMultiplier - 0.05f, 1f, 5f);
		recoilRotation *= 0.7f;
		weaponsWrapper.transform.localRotation = Quaternion.Euler (recoilRotation);
	}

	// updates scale of crosshair to indicate recoil
	void UpdateCrosshairRecoil ()
	{
		float crosshairRecoilMultiplier = Mathf.Sqrt (recoilMultiplier);
		crosshair.transform.localScale = new Vector3 (crosshairRecoilMultiplier, crosshairRecoilMultiplier, crosshairRecoilMultiplier);
	}

	// updates scale of crosshair to indicate recoil
	void UpdateHUD ()
	{
		weaponNameDisplay.text = weaponNames [activeWeaponID];
		ammoDisplay.text = ammo [activeWeaponID] + " / " + ammoReserve [activeWeaponID];
		healthDisplay.text = Mathf.CeilToInt (playerHitPoints).ToString ();

		if (abilityTimer > Time.time) {
			skillText.text = classAbilities [playerClassIndex] + " will be available in " +
			Mathf.CeilToInt (abilityTimer - Time.time).ToString () + " seconds...";
		} else {
			skillText.text = classAbilities [playerClassIndex] + " is available!";
		}
	}

	void KillMessage ()
	{
		if (Time.time < lastKillTime + 5f) {
			killStreak += 1;
		} else {
			killStreak = 1;
		}

		lastKillTime = Time.time;

		mainMessageText.text = "Kill Streak x " + killStreak + " !";
		mainMessageOuterText.text = mainMessageText.text;
		mainMessage.Play ("MainMessagePopup", -1, 0f);
		mainMessageOuter.Play ("MainMessageOuterPopup", -1, 0f);
	}

	void HeadshotMessage ()
	{
		
		if (lastHeadshotTime > 0) {
			headshotStreak += 1;
		} else {
			headshotStreak = 1;
		}

		lastHeadshotTime = Time.time;

		mainMessageText.text = "Head Shot x " + headshotStreak + " !";
		mainMessageOuterText.text = mainMessageText.text;
		mainMessage.Play ("MainMessagePopup", -1, 0f);
		mainMessageOuter.Play ("MainMessageOuterPopup", -1, 0f);
		Debug.Log ("HS method called");
	}


	// swap to a specific weapon
	void ReloadWeapon ()
	{

		if (ammo [activeWeaponID] < ammoMax [activeWeaponID] && ammoReserve [activeWeaponID] > 0) {
			// disable firing for a second while the weapon is being reloaded
//			fireTimer = Time.time + 2.5f;

			// required for recoil animation to work properly
			weaponSwapAnimator.applyRootMotion = false;

			// set trigger to play swap animation
			weaponSwapAnimator.Play ("WeaponReload");

			int newClip = Mathf.Min (ammoReserve [activeWeaponID], ammoMax [activeWeaponID] - ammo [activeWeaponID]);
			ammoReserve [activeWeaponID] -= newClip;
			ammo [activeWeaponID] += newClip;
			weaponAudioSource.clip = reloadSound;
			weaponAudioSource.PlayDelayed (0.7f);

			// reset recoil
			recoilMultiplier = 1f;
		}


	}

	// swap to next weapon
	void SwapWeapon ()
	{
		// if only one weapon, don't bother
		// if primary active, switch to secondary
		// if secondary active, switch to primary
		if (primaryWeaponID != secondaryWeaponID) {
			if (activeWeaponID == primaryWeaponID) {
				SwapWeapon (secondaryWeaponID);
			} else {
				SwapWeapon (primaryWeaponID);
			}
		}
	}

	// swap to a specific weapon
	void SwapWeapon (int weaponID)
	{

		
		// required for recoil animation to work properly
		weaponSwapAnimator.applyRootMotion = false;

		// set trigger to play swap animation
		weaponSwapAnimator.Play ("WeaponSwap");

		if (weaponID < 0 || weaponID >= weaponModels.Length) {
			Debug.Log ("Wrong ID");
		} else {

//			weaponModels [activeWeaponID].SetActive (false);
			activeWeaponID = weaponID;
//			weaponModels [activeWeaponID].SetActive (true);
            
		}

		// reset recoil
		recoilMultiplier = 1f;
	}

	// Called when hit by a zombie
	void HitByZombie (float damage)
	{
		
		if (playerHitPoints > 0) {

			AudioSource.PlayClipAtPoint (playerHurtSound, transform.position);
			playerHitPoints -= damage * (1 - damageReduction);
//			playerHitPoints -= damage * (1 - damageReduction) * difficultyDmgMultZombie;

			Debug.Log (playerHitPoints);
			if (playerHitPoints <= 0) {
				Debug.Log ("player dead!");
			}
		}
	}

	// allows another script to define which weapons to use
	public void SwapLoadout (int primary, int secondary)
	{
		primaryWeaponID = primary;
		secondaryWeaponID = secondary;
	}

	// get the player's threat level, if higher than threshold, zombie will chase player
	public float GetThreatMultiplier ()
	{
		return threatMultiplier;
	}


public void increaseAmmoReserve(int value) {

    weaponAudioSource.clip = reloadSound;
    weaponAudioSource.Play();

    for (int i = 0; i < ammoReserve.Length; i++) {
      ammoReserve[i] = Mathf.Clamp(ammoReserve[i] + value, 0, 400);
    }
  }

  public void increasePlayerStamina(float value) {
    playerStamina = Mathf.Clamp(playerStamina + value, 1f, 100f);
  }
}
