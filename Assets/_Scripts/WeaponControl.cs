using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// required for UI manipulation
using UnityEngine.UI;

// required for camera effects
using UnityStandardAssets.ImageEffects;

public class WeaponControl : MonoBehaviour
{

	// indicate the player's class
	[SerializeField]
	private string playerClass;
	private float abilityCooldown = 5f;
	private float abilityTimer;

	// indicates which weapon is active
	private int activeWeaponID = 0;

	// indicates which loadout the player is using
	private int primaryWeaponID = 0;
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
	//	[SerializeField]
	//	private GameObject bloodSplat;

	[SerializeField]
	private float playerHitPoints = 100f;


	private string[] weaponNames = new string[] { "AK-47", "M4A1", "UMP-45" };
	private float[] weaponDamages = new float[] { 7.5f, 6.0f, 5.0f };
	private float[] fireCooldowns = new float[] { 0.12f, 0.1f, 0.08f };
	private float[] recoilFactors = new float[] { 0.75f, 0.5f, 0.3f };
	private int[] ammo = new int[3] { 30, 30, 35 };
	private int[] ammoReserve = new int[3] { 120, 150, 210 };
	private int[] ammoMax = new int[3] { 30, 30, 35 };
	private Text ammoDisplay;
	private GameObject ammoReserveDisplay;
	private GameObject crosshair;
	private Text healthDisplay;


	private GameObject weaponsWrapper;
	private AudioSource weaponAudioSource;
	private Animator weaponSwapAnimator;


	private RaycastHit impact;
	private float impactDistance;
	private GameObject impactObject;
	private ZombieControl zombieControl;

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

	// Use this for initialization
	void Start ()
	{

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
		ammoDisplay = GameObject.Find ("AmmoDisplay").GetComponent<Text> ();
		crosshair = GameObject.Find ("Crosshair");
		healthDisplay = GameObject.Find ("HealthDisplay").GetComponent<Text> ();

		// find the camera filters
		motionBlurFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<MotionBlur> ();
		greyscaleFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<Grayscale> ();
		noiseAndGrainFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<NoiseAndGrain> ();
		audioReverbFilter = GameObject.FindGameObjectWithTag ("MainCamera").GetComponentInChildren<AudioReverbFilter> ();

		// turn off the camera filters until we need them
		motionBlurFilter.enabled = false;
		greyscaleFilter.enabled = false;
	}

	// Update is called once per frame
	void Update ()
	{

		// things that happen every frame
		RecoilFalloff ();
		UpdateCrosshairRecoil ();
		UpdateHUD ();

//		if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.TransformDirection (Vector3.forward), out impact)) {
//			impactDistance = impact.distance;
//			impactObject = impact.transform.gameObject;
//		}
		if (weaponSwapAnimator.GetCurrentAnimatorStateInfo (0).IsName ("WeaponSwap2")) {
			foreach (GameObject weapon in weaponModels) {
				weapon.SetActive (false);
			}
			weaponModels [activeWeaponID].SetActive (true);
		}

		bool weaponAnimationFinished = weaponSwapAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle");

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
				CycleWeapon ();
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
			if (threatMultiplier == 1.0F) {
				threatMultiplier = 0.1F;
				greyscaleFilter.enabled = true;
				noiseAndGrainFilter.enabled = true;
			} else {
				threatMultiplier = 1.0F;
				greyscaleFilter.enabled = false;
				noiseAndGrainFilter.enabled = false;
			}
		}



	}

	void UseActiveAbility(){
		if (abilityTimer < Time.time) {
			abilityTimer = Time.time + abilityCooldown;
			switch (playerClass) {
			case "Assassin":
				StartCoroutine(AdrenalineRush ());
				break;
			case "Crusader":
				break;
			case "Shadow":
				if (threatMultiplier == 1.0F) {
					threatMultiplier = 0.1F;
					greyscaleFilter.enabled = true;
				} else {
					threatMultiplier = 1.0F;
					greyscaleFilter.enabled = false;
				}
				break;
			}
		}




	}

	IEnumerator AdrenalineRush() {
		if (Time.timeScale == 1.0F) {
			Time.timeScale = 0.25F;
			weaponSwapAnimator.speed = 1 / Time.timeScale;

			motionBlurFilter.enabled = true;
			audioReverbFilter.enabled = true;

			yield return new WaitForSeconds (1f);

			Time.timeScale = 1.0F;
			weaponSwapAnimator.speed = 1 / Time.timeScale;

			motionBlurFilter.enabled = false;
			audioReverbFilter.enabled = false;
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

		if (impactObject.CompareTag ("Concrete")) {
			Instantiate (concreteImpactPrefab, impact.point, quatAngle);
//			Instantiate(concreteImpactDecal[Random.Range(0, concreteImpactAudio.Length)], impact.point, quatAngle);
//			ParticleSystem decal = (ParticleSystem) Instantiate( concreteImpactDecal[Random.Range(0, concreteImpactAudio.Length)], impact.point + impact.normal * 0.020f, quatAngle );
//			decal.transform.parent = impact.transform.gameObject.transform;	// parent the decal to the object
			AudioSource.PlayClipAtPoint (concreteImpactAudio [Random.Range (0, concreteImpactAudio.Length)], impact.point);
		}


		if (impactObject.transform.root.CompareTag ("Zombie")) {
			Instantiate (fleshImpactPrefab, impact.point, quatAngle);
//			Instantiate(fleshImpactDecal, impact.point, Quaternion.identity);
			AudioSource.PlayClipAtPoint (fleshImpactAudio [Random.Range (0, fleshImpactAudio.Length)], impact.point);
//			Debug.Log ("hit");
//			Instantiate (bloodSplat, impact.point, Quaternion.identity);
			zombieControl = impactObject.GetComponentInParent<ZombieControl> ();
			zombieControl.SendMessage ("HitByBullet", new object [] { impactObject.name, weaponDamages [activeWeaponID] }, SendMessageOptions.DontRequireReceiver);
		}

	}

	// handles firing logic of projectiles that are not instantly triggered (e.g. explosives)
	void FireProjectile ()
	{

	}

	// adds a bit of recoil force
	void RecoilTrigger ()
	{
		recoilMultiplier += recoilFactors [activeWeaponID];
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
		ammoDisplay.text = ammo [activeWeaponID] + " / " + ammoReserve [activeWeaponID];
		healthDisplay.text = Mathf.CeilToInt(playerHitPoints).ToString();
//		healthDisplay.text = weaponSwapAnimator.GetCurrentAnimatorStateInfo (0).tagHash;
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
			weaponSwapAnimator.Play("WeaponReload");

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
	void CycleWeapon ()
	{

		int switchToWeaponId = activeWeaponID + 1;
		if (switchToWeaponId >= weaponModels.Length) {
			switchToWeaponId = 0;
		}

		SwapWeapon (switchToWeaponId);
	}

	// swap to a specific weapon
	void SwapWeapon (int switchToWeaponId)
	{
		
		// required for recoil animation to work properly
		weaponSwapAnimator.applyRootMotion = false;

		// set trigger to play swap animation
		weaponSwapAnimator.Play("WeaponSwap");

		if (switchToWeaponId < 0 || switchToWeaponId >= weaponModels.Length) {
			Debug.Log ("Wrong ID");
		} else {

//			weaponModels [activeWeaponID].SetActive (false);
			activeWeaponID = switchToWeaponId;
//			weaponModels [activeWeaponID].SetActive (true);
            
		}

		// reset recoil
		recoilMultiplier = 1f;
	}

	// Called when hit by a zombie
	void HitByZombie (float damage)
	{
		
		if (playerHitPoints > 0) {


			playerHitPoints -= damage;
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
}
