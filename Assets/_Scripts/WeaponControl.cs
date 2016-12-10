using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponControl : MonoBehaviour {

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
	private float [] fireCooldowns;
	[SerializeField]
	private float [] recoilFactors;
    [SerializeField]
    private Transform muzzleFlashPoint;
    [SerializeField]
    private GameObject muzzleFlash;
	[SerializeField]
	private GameObject brickImpactPrefab;
	[SerializeField]
	private GameObject brickImpactDecal;
	[SerializeField]
	private AudioClip[] brickImpactAudio;
	[SerializeField]
	private AudioClip reloadSound;

	private int [] ammo = new int[3] {30, 30, 35};
	private int [] ammoReserve = new int[3] {120, 150, 210};
	private int [] ammoMax = new int[3] {30, 30, 35};
	private Text ammoDisplay;
	private GameObject ammoReserveDisplay;
	private GameObject crosshair;

    private GameObject weaponsWrapper;
	private AudioSource weaponAudioSource;
	private Animator weaponSwapAnimator;


	private RaycastHit impact;
	private float targetDistance;
	private GameObject targetObject;





    private float fireTimer = 0f;
    private Vector3 recoilRotation;
    private float recoilMultiplier = 1f;

    // Use this for initialization
    void Start() {

		// set all weapons to invisible then set active to primary and show it
        foreach (GameObject weapon in weaponModels) {
            weapon.SetActive(false);
        }

        activeWeaponID = primaryWeaponID;
        weaponModels[activeWeaponID].SetActive(true);

		// find the weapon wrapper
		weaponsWrapper = GameObject.Find("Weapons");

		// find audio source on the weapon wrapper
		weaponAudioSource = weaponsWrapper.GetComponent<AudioSource>();

		// find the animator used in weapon swapping
		weaponSwapAnimator = weaponsWrapper.GetComponent<Animator> ();

        



		// find the HUD elements
		ammoDisplay = GameObject.Find("AmmoDisplay").GetComponent<Text>();
		crosshair = GameObject.Find("Crosshair");
    }

    // Update is called once per frame
    void Update() {

		// things that happen every frame
		RecoilFalloff ();
		UpdateCrosshairRecoil ();
		UpdateHUD ();



		if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out impact)) {
			targetDistance = impact.distance;
			targetObject = impact.transform.gameObject;



//			if (targetDistance < allowedRange) {
//
//				UpdateTooltip ();
//
//
//
//			} else {
//				
//			}
		}

		// actions that are only available if weapon not on cooldown
		if (Time.time > fireTimer) {
			// left mouse click
			if (Input.GetButton ("Fire1")) {
				if (ammo[activeWeaponID] > 0) {
					Fire ();
					FireRay ();
				} else {
					StartCoroutine(ReloadWeapon ());
				}

			}

			// swap weapon
			if (Input.GetKeyDown (KeyCode.Q)) {
				CycleWeapon ();
			}


			// reload weapon
			if (Input.GetKeyDown (KeyCode.R)) {
				StartCoroutine(ReloadWeapon ());
			}
		}





    }

    void Fire() {

		// put weapon on cooldown according to which type of weapon it is
		fireTimer = Time.time + fireCooldowns[activeWeaponID];

		// play weapon sound once
        weaponAudioSource.PlayOneShot(fireAudioClips[activeWeaponID]);

		// instantiate a muzzle flash
		Instantiate(muzzleFlash, muzzleFlashPoint.position, Random.rotation);

		// adds some recoil with every shot fired
        RecoilTrigger();

		ammo[activeWeaponID] -= 1;

    }

	// handles firing logic related to instant projectile hits (e.g. guns)
	void FireRay () {
//		Quaternion quatAngle = Quaternion.LookRotation( impact.normal );
//		Instantiate( brickImpactPrefab, impact.point, quatAngle );
//		ParticleSystem decal = (ParticleSystem) Instantiate( brickImpactDecal, impact.point + impact.normal * 0.020f, quatAngle );
//		decal.transform.parent = impact.transform.gameObject.transform;	// parent the decal to the object
		Instantiate(brickImpactPrefab, impact.point, Quaternion.identity);
		Instantiate(brickImpactDecal, impact.point, Quaternion.identity);
		AudioSource.PlayClipAtPoint (brickImpactAudio[Random.Range(0, brickImpactAudio.Length)], impact.point);

	}

	// handles firing logic of projectiles that are not instantly triggered (e.g. explosives)
	void FireProjectile () {

	}

	// adds a bit of recoil force
    void RecoilTrigger() {
		recoilMultiplier += recoilFactors[activeWeaponID];
        recoilRotation.x += Random.value * recoilMultiplier;
        recoilRotation.y += Random.value * 0.5f * recoilMultiplier;
        recoilRotation.z += Random.value * 0.5f * recoilMultiplier;

        
    }

	// reduces recoil by a factor every frame
    void RecoilFalloff() {
		recoilMultiplier = Mathf.Clamp(recoilMultiplier - 0.05f, 1f, 5f);
        recoilRotation *= 0.7f;
        weaponsWrapper.transform.localRotation = Quaternion.Euler(recoilRotation);

    }

	// updates scale of crosshair to indicate recoil
	void UpdateCrosshairRecoil() {
		float crosshairRecoilMultiplier = Mathf.Sqrt(recoilMultiplier);
		crosshair.transform.localScale = new Vector3 (crosshairRecoilMultiplier, crosshairRecoilMultiplier, crosshairRecoilMultiplier);
	}

	// updates scale of crosshair to indicate recoil
	void UpdateHUD() {
		ammoDisplay.text = ammo [activeWeaponID] + " / " + ammoReserve [activeWeaponID];
	}

	// swap to a specific weapon
	IEnumerator ReloadWeapon() {

		if (ammo [activeWeaponID] < ammoMax [activeWeaponID] && ammoReserve [activeWeaponID] > 0) {
			// disable firing for a second while the weapon is being reloaded
			fireTimer = Time.time + 2.5f;

			// required for recoil animation to work properly
			weaponSwapAnimator.applyRootMotion = false;

			// set trigger to play swap animation
			weaponSwapAnimator.SetTrigger("ReloadWeapon");

			// give time for the first half of animation to finish before changing visible gun model
			yield return new WaitForSeconds (0.7f);

			int newClip = Mathf.Min (ammoReserve [activeWeaponID], ammoMax [activeWeaponID] - ammo[activeWeaponID]);
			ammoReserve [activeWeaponID] -= newClip;
			ammo [activeWeaponID] += newClip;
			weaponAudioSource.PlayOneShot (reloadSound);

			// give time for the second half of animation to finish
			yield return new WaitForSeconds (1.8f);

			// reset recoil
			recoilMultiplier = 1f;

			// required for recoil animation to work properly
			weaponSwapAnimator.applyRootMotion = true;
		}


	}

	// swap to next weapon
    void CycleWeapon() {

        int switchToWeaponId = activeWeaponID + 1;
        if (switchToWeaponId >= weaponModels.Length) {
            switchToWeaponId = 0;
        }

		StartCoroutine(SwapWeapon(switchToWeaponId));
    }

	// swap to a specific weapon
	IEnumerator SwapWeapon(int switchToWeaponId) {
		
		// disable firing for a second while the weapon is being swapped
		fireTimer = Time.time + 1f;



		// required for recoil animation to work properly
		weaponSwapAnimator.applyRootMotion = false;

		// set trigger to play swap animation
		weaponSwapAnimator.SetTrigger("SwapWeapon");

		// give time for the first half of animation to finish before changing visible gun model
		yield return new WaitForSeconds (0.33f);

        if (switchToWeaponId < 0 || switchToWeaponId >= weaponModels.Length) {
            Debug.Log("Wrong ID");
        } else {

            weaponModels[activeWeaponID].SetActive(false);
			activeWeaponID = switchToWeaponId;
			weaponModels[activeWeaponID].SetActive(true);
            
        }

		// give time for the second half of animation to finish
		yield return new WaitForSeconds (0.66f);

		// reset recoil
		recoilMultiplier = 1f;

		// required for recoil animation to work properly
		weaponSwapAnimator.applyRootMotion = true;
    }

	// allows another script to define which weapons to use
	public void SwapLoadout(int primary, int secondary) {
		primaryWeaponID = primary;
		secondaryWeaponID = secondary;
	}
}
