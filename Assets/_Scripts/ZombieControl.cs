using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieControl : MonoBehaviour {

	public NavMeshAgent navAgent;
	private GameObject player;
	private WeaponControl weaponControl;
	private Animator zombieAnimator;

	private float hitPoints = 100f;
	private float fadeOutTime = 5f;
	private float timeOfDeath = 0f;


	// Use this for initialization
	void Start() {

		player = GameObject.FindGameObjectWithTag ("Player");
		weaponControl = player.GetComponentInChildren<WeaponControl>();

		zombieAnimator = GetComponent<Animator> ();
	}

	// Update is called once per frame
	void Update() {

		if (weaponControl.GetThreatMultiplier () < 1.0f) {
			navAgent.Stop ();
			zombieAnimator.enabled = false;
		} else {
			navAgent.SetDestination (player.transform.position);
			navAgent.Resume ();
			zombieAnimator.enabled = true;
		}
//		if (hitPoints > 0) {
//			navAgent.SetDestination (player.transform.position);
//		} else {
//			FadeOut ();
//		}



	}

	// Called when hit by a shot
	void HitByBullet(object [] damageInfo) {
		if (hitPoints > 0) {
			string bodyPart = (string)damageInfo [0];
			float damage = (float)damageInfo [1];

			if (bodyPart.Contains ("Head")) {
				hitPoints -= damage * 2f;
			} else if (bodyPart.Contains ("Spine")) {
				hitPoints -= damage;
			} else {
				hitPoints -= damage * 0.5f;
			}

			if (hitPoints <= 0) {
				zombieAnimator.SetInteger ("state", 10);
				timeOfDeath = Time.time;
				navAgent.Stop ();
				Destroy (gameObject, 5f);
			}
		}
	}

//	void FadeOut() {
//		float alpha = (fadeOutTime - (Time.time - timeOfDeath)) / fadeOutTime;
//		if (alpha > 0) {
//			foreach (MeshRenderer child in this.GetComponentsInChildren<MeshRenderer>()) {
//				Color matCol = child.material.color;
//				child.material.SetColor ("_Color", new Color (matCol.r, matCol.g, matCol.b, alpha));
//			}
//
//		} else {
//			Destroy (this);
//		}
//	}
}
