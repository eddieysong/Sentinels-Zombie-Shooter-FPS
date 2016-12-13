using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttackTrigger : MonoBehaviour {

	public float damage = 5f;

	// Use this for initialization
	void Start () {
		Destroy (gameObject, 0.8f);
	}
	
	// Update is called once per frame
	void SetDamage (float zombieDamage) {
		damage = zombieDamage;
	}

	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		
		if (other.transform.root.CompareTag("Player")) {
			WeaponControl playerWC = other.transform.GetComponentInChildren<WeaponControl>();
//			Debug.Log (other.name);
			playerWC.SendMessage("HitByZombie", damage, SendMessageOptions.DontRequireReceiver);
		}
	}
}
