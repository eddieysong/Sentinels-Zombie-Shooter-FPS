/// <summary>
/// ZombieAttackTrigger.cs
/// Last Modified: 2016-12-15
/// Created By: Eddie Song
/// Last Modified By: Eddie Song
/// Summary: this script is attached to the hitbox that is generated when a zombie attacks,
/// 		it checks for collision with the player, and if it finds the collision,
/// 		the player loses health.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackTrigger : MonoBehaviour {

  public float damage = 5f;

  // Use this for initialization
  void Start() {
    Destroy(gameObject, 0.8f);
  }
	
  // Update is called once per frame
  void Update() {
    
  }

  void SetDamage(float zombieDamage) {
    damage = zombieDamage;
  }

  void OnTriggerEnter(Collider other) {
    Debug.Log(other.name);
    if (other.CompareTag("Player")) {
      WeaponController playerWC = other.transform.GetComponentInChildren<WeaponController>();
      playerWC.SendMessage("HitByZombie", damage, SendMessageOptions.DontRequireReceiver);
    }
  }
}
