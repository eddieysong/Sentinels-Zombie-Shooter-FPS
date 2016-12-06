using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponControl : MonoBehaviour {

    // indicates which weapon is active
    private int activeWeaponID = 0;

    // indicates which loadout the player is using
    private static int primaryWeaponID = 0;

    [SerializeField]
    private GameObject[] weaponModels;
    [SerializeField]
    private AudioClip[] fireAudioClips;
    [SerializeField]
    private Transform muzzleFlashPoint;
    [SerializeField]
    private GameObject muzzleFlash;
    private GameObject weaponsWrapper;

    private AudioSource fireWeaponAudioSource;
    private float fireCooldown = 0.1f;
    private float fireTimer = 0f;

    private Vector3 recoilRotation;
    private float recoilMultiplier = 1f;

    // Use this for initialization
    void Start() {

        foreach (GameObject weapon in weaponModels) {
            weapon.SetActive(false);
        }

        activeWeaponID = primaryWeaponID;
        weaponModels[activeWeaponID].SetActive(true);

        // find audio source on the player
        fireWeaponAudioSource = GameObject.Find("WeaponAudioSource").GetComponent<AudioSource>();

        weaponsWrapper = GameObject.Find("Weapons");
    }

    // Update is called once per frame
    void Update() {

        // left mouse click
        if (Input.GetButton("Fire1")) {
            Fire();
        }

        RecoilFalloff();

        if (Input.GetKeyDown(KeyCode.Q)) {
            SwapWeapon();
        }
    }

    void Fire() {

        if (Time.time > fireTimer) {
            fireTimer = Time.time + fireCooldown;
            fireWeaponAudioSource.PlayOneShot(fireAudioClips[activeWeaponID]);
            Instantiate(muzzleFlash, muzzleFlashPoint.position, Quaternion.identity);
            RecoilTrigger();
        }
    }

    void RecoilTrigger() {
        recoilMultiplier += 0.5f;
        Debug.Log(recoilMultiplier);
        recoilRotation.x += Random.value * recoilMultiplier;
        recoilRotation.y += Random.value * 0.5f * recoilMultiplier;
        recoilRotation.z += Random.value * 0.5f * recoilMultiplier;

        Debug.Log(recoilRotation);
    }

    void RecoilFalloff() {
        recoilMultiplier = Mathf.Clamp(recoilMultiplier - 0.05f, 1f, 6f);
        recoilRotation *= 0.7f;
        weaponsWrapper.transform.localRotation = Quaternion.Euler(recoilRotation);
    }

    void SwapWeapon() {

        int switchToWeaponId = activeWeaponID + 1;
        if (switchToWeaponId >= weaponModels.Length) {
            switchToWeaponId = 0;
        }

        SwapWeapon(switchToWeaponId);
    }

    void SwapWeapon(int switchToWeaponId) {

        if (switchToWeaponId < 0 || switchToWeaponId >= weaponModels.Length) {
            Debug.Log("Wrong ID");
        } else {

            weaponModels[activeWeaponID].SetActive(false);
            weaponModels[switchToWeaponId].SetActive(true);
            activeWeaponID = switchToWeaponId;
        }
    }
}
