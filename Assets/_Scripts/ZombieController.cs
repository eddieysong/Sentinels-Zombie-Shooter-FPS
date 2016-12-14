using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{

	private static float NAV_AGENT_INITIAL_SPEED = 0.8f;
	public NavMeshAgent navAgent;
	private GameObject player;
	private WeaponController weaponControl;
	private Animator zombieAnimator;
	[SerializeField]
	private Transform attackTriggerSpawn;
	[SerializeField]
	private GameObject attackTrigger;

	private static float MAX_TIME_RUNNING = 4f;


	private bool alive;
	[SerializeField]
	private float hitPoints = 100f;
	[SerializeField]
	private float damage = 10f;
	private float fadeOutTime = 5f;
	private float timeOfDeath = 0f;
	private float timeOfRunStart = 0f;
	private bool running;
	private bool attacking;
	private float initialAnimationSpeed = 1f;
   
	// Use this for initialization
	void Start ()
	{

		player = GameObject.FindGameObjectWithTag ("Player");
		weaponControl = player.GetComponentInChildren<WeaponController> ();
		zombieAnimator = GetComponent<Animator> ();
		alive = true;
		running = false;
		setRandomSpeed (NAV_AGENT_INITIAL_SPEED);

		// so zombies don't move in exact unison
		initialAnimationSpeed = Random.Range (0.9f, 1.1f);
	}

	void setRandomSpeed (float minimumSpeed)
	{

		navAgent.speed = minimumSpeed + (Random.Range (0f, minimumSpeed * 0.75f));
	}

	// Update is called once per frame
	void Update ()
	{

		timeOfRunStart += Time.deltaTime;

		if (!attacking) {
			if (weaponControl.GetThreatMultiplier () < 1.0f) {
				navAgent.Stop ();
				zombieAnimator.enabled = false;

			} else {

				if (alive) {

					if (Vector3.Distance (player.transform.position, transform.position) < 2.5f) {
						Debug.Log ("attacking!");
						StartCoroutine (Attack ());
					}

					else if (Vector3.Distance (player.transform.position, transform.position) < 10f) {
						running = true;
						timeOfRunStart = 0f;
					}

					if (running) {
						zombieAnimator.speed = initialAnimationSpeed * 2f;
						setRandomSpeed (NAV_AGENT_INITIAL_SPEED * 2f);

					} else {
						zombieAnimator.speed = initialAnimationSpeed;
						setRandomSpeed (NAV_AGENT_INITIAL_SPEED);
					}

					navAgent.SetDestination (player.transform.position);
					navAgent.Resume ();
					zombieAnimator.enabled = true;
				} else {
					navAgent.Stop ();
				}
			}
		}
//		if (hitPoints > 0) {
//			navAgent.SetDestination (player.transform.position);
//		} else {
//			FadeOut ();
//		}

		if (timeOfRunStart > MAX_TIME_RUNNING) {
			running = false;
		}

//		print (" running " + (running ? " true" : " false") + ". State: " + zombieAnimator.GetInteger ("state"));
	}

	// Called when hit by a shot
	void HitByBullet (object[] damageInfo)
	{
		if (hitPoints > 0) {
			string bodyPart = (string)damageInfo [0];
			float damage = (float)damageInfo [1];

			if (bodyPart.Contains ("Head")) {
				hitPoints -= damage * 4f;

			} else if (bodyPart.Contains ("Spine")) {
				hitPoints -= damage;
			} else {
				hitPoints -= damage * 0.5f;
			}
			Debug.Log ("zombieHP: " + hitPoints);
			if (hitPoints <= 0) {
				Die ();
			}
		}
	}

	IEnumerator Attack ()
	{
		Debug.Log ("method!");
		// set attacking flag to true so Update() won't try to move this object
		attacking = true;
		running = false;

		// set trigger to play attack animation
		zombieAnimator.SetInteger ("state", 1);

		yield return new WaitForSeconds (0.6f);

		GameObject trigger = Instantiate (attackTrigger, attackTriggerSpawn.position, Quaternion.identity);
		trigger.SendMessage ("SetDamage", damage, SendMessageOptions.DontRequireReceiver);

		// give time for the animation to finish
		yield return new WaitForSeconds (0.8f);

		if (alive) {
			// set trigger to play walk animation
			zombieAnimator.SetInteger ("state", 0);
		}

		attacking = false;
	}

	void Die ()
	{
		int curState = 10 + (int)(Random.Range (0f, 3f) % 3);
		zombieAnimator.speed = 1f;
		zombieAnimator.SetInteger ("state", curState);
		timeOfDeath = Time.time;
		alive = false;
		Destroy (gameObject, 5f);
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
