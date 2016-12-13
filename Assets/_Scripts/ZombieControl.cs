using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieControl : MonoBehaviour {

  private static float NAV_AGENT_INITIAL_SPEED = 0.8f;
  public NavMeshAgent navAgent;
  private GameObject player;
  private WeaponControl weaponControl;
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
   
  // Use this for initialization
  void Start() {

    player = GameObject.FindGameObjectWithTag("Player");
    weaponControl = player.GetComponentInChildren<WeaponControl>();
    zombieAnimator = GetComponent<Animator>();
    alive = true;
    running = false;
    setRandomSpeed(NAV_AGENT_INITIAL_SPEED);
  }

  void setRandomSpeed(float minimumSpeed) {

    navAgent.speed = minimumSpeed + (Random.Range(0f, minimumSpeed * 0.75f));
  }

  // Update is called once per frame
  void Update() {

    timeOfRunStart += Time.deltaTime;

    if (!attacking) {
      if (weaponControl.GetThreatMultiplier() < 1.0f) {
        navAgent.Stop();
        zombieAnimator.enabled = false;

      } else {

        if (alive) {

          if (Vector3.Distance(player.transform.position, transform.position) < 2.5f) {
            Debug.Log("attacking!");
            StartCoroutine(Attack());
          }

          if (Vector3.Distance(player.transform.position, transform.position) < 10f) {
            running = true;
            timeOfRunStart = 0f;
          }

          if (running) {
            zombieAnimator.speed = 2f;
            setRandomSpeed(NAV_AGENT_INITIAL_SPEED * 2f);

          } else {
            zombieAnimator.speed = 1f;
            setRandomSpeed(NAV_AGENT_INITIAL_SPEED);
          }

          navAgent.SetDestination(player.transform.position);
          navAgent.Resume();
          zombieAnimator.enabled = true;
        } else {
          navAgent.Stop();
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

//    print(" running " + (running ? " true" : " false") + ". State: " + zombieAnimator.GetInteger("state"));
  }

  // Called when hit by a shot
  void HitByBullet(object[] damageInfo) {
    if (hitPoints > 0) {
      string bodyPart = (string)damageInfo[0];
      float damage = (float)damageInfo[1];

      if (bodyPart.Contains("Head")) {
        hitPoints -= damage * 4f;

      } else if (bodyPart.Contains("Spine")) {
        hitPoints -= damage;
      } else {
        hitPoints -= damage * 0.5f;
      }

      if (hitPoints <= 0) {
        Die();
      }
    }
  }

  IEnumerator Attack() {
    Debug.Log("method!");
    // set attacking flag to true so Update() won't try to move this object
    attacking = true;

    // required for recoil animation to work properly
//		weaponSwapAnimator.applyRootMotion = false;

    // set trigger to play attack animation
    zombieAnimator.SetInteger("state", 1);

    GameObject trigger = Instantiate(attackTrigger, attackTriggerSpawn.position, Quaternion.identity);
    trigger.SendMessage("SetDamage", damage, SendMessageOptions.DontRequireReceiver);

    // give time for the animation to finish
    yield return new WaitForSeconds(1.5f);

    // set trigger to play walk animation
    zombieAnimator.SetInteger("state", 0);

    attacking = false;
  }

  void Die() {
    int curState = 10 + (int)(Random.Range(0f, 3f) % 3);
    zombieAnimator.speed = 1f;
    zombieAnimator.SetInteger("state", curState);
    timeOfDeath = Time.time;
    alive = false;
    Destroy(gameObject, 5f);
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
