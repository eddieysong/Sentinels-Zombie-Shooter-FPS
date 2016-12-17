using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{

	[SerializeField]
	private float NAV_AGENT_INITIAL_SPEED = 0.8f;

	public NavMeshAgent navAgent;
	private GameObject player;
	private WeaponController weaponControl;
	private Animator zombieAnimator;

	private AudioSource audioSource;
	private AudioClip[] zombieTalkClips;

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

	private MainGameController gameController;

	[Header ("Boss Settings")]
	[SerializeField]
	private bool isBoss = false;
	[SerializeField]
	private string goToSceneWhenKillBoss;

	// Use this for initialization
	void Start ()
	{

		gameController = GameObject.Find ("GameController").GetComponent<MainGameController> ();

		hitPoints *= gameController.endlessMultiplier;

		damage *= gameController.endlessMultiplier;

		audioSource = GetComponent<AudioSource> ();

		zombieTalkClips = new AudioClip[] { 
			Resources.Load<AudioClip> ("Audio/zombie-talking-1"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-2"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-3"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-4"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-5"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-6"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-7"),
			Resources.Load<AudioClip> ("Audio/zombie-talking-8")
		};

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

		float distanceFromPlayer = Vector3.Distance (player.transform.position, transform.position);

		if (!audioSource.isPlaying) {
			audioSource.clip = zombieTalkClips [Random.Range (0, zombieTalkClips.Length - 1)];
			audioSource.Play ();
		}

		if (audioSource.isPlaying) {
//      audioSource.volume = Mathf.Clamp(1f / Mathf.Max(0.1f, distanceFromPlayer + 20f), 0.01f, 1f);
		}

		timeOfRunStart += Time.deltaTime;

		if (!attacking) {

			if (alive) {
				if (weaponControl.GetThreatMultiplier () < 1.0f) {
					StartCoroutine (Wander ());
				} else {
					if (distanceFromPlayer < 2.5f) {
						Debug.Log ("attacking!");
						StartCoroutine (Attack ());
					} else if (distanceFromPlayer < 10f) {
						running = true;
						timeOfRunStart = 0f;
					}

					if (running) {
						zombieAnimator.speed = initialAnimationSpeed * 1.5f;
						setRandomSpeed (NAV_AGENT_INITIAL_SPEED * 1.5f);

					} else {
						zombieAnimator.speed = initialAnimationSpeed;
						setRandomSpeed (NAV_AGENT_INITIAL_SPEED);
					}

					navAgent.SetDestination (player.transform.position);
					navAgent.Resume ();
//					zombieAnimator.enabled = true;
				}

			} else {
				navAgent.Stop ();
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
				weaponControl.SendMessage ("HeadshotMessage", SendMessageOptions.DontRequireReceiver);
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
//    Debug.Log("method!");
		// set attacking flag to true so Update() won't try to move this object
		attacking = true;
		running = false;

		audioSource.clip = zombieTalkClips [zombieTalkClips.Length - 1];
		audioSource.Play ();

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

	IEnumerator Wander ()
	{
		while (weaponControl.GetThreatMultiplier () < 1.0f) {
			setRandomSpeed (NAV_AGENT_INITIAL_SPEED * 0.4f);
			navAgent.SetDestination (new Vector3 (transform.position.x + Random.Range (0f, 5f), transform.position.y, transform.position.z + Random.Range (0f, 5f)));
			yield return new WaitForSeconds (1.5f);
		}
	}

	void Die ()
	{
		navAgent.radius = 0.01f; // so other zombies can get through
		int curState = 10 + (int)(Random.Range (0f, 3f) % 3);
		zombieAnimator.speed = 1f;
		zombieAnimator.SetInteger ("state", curState);
		timeOfDeath = Time.time;
		alive = false;
		weaponControl.SendMessage ("KillMessage", SendMessageOptions.DontRequireReceiver);
		Destroy (gameObject, 5f);

		if (isBoss) {
			// go to next scene when boss is dead
			gameController.bossDead (gameObject, goToSceneWhenKillBoss);
		} else {
			gameController.zombieDead (gameObject);
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
