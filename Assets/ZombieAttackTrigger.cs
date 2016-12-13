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
      WeaponControl playerWC = other.transform.GetComponentInChildren<WeaponControl>();
      playerWC.SendMessage("HitByZombie", damage, SendMessageOptions.DontRequireReceiver);
    }
  }
}
