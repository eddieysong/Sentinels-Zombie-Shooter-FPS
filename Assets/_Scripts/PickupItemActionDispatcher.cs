using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemActionDispatcher : MonoBehaviour {

  public enum PickupItemActionType {
    AMMO,
    HEALTH
  }

  public PickupItemActionType type;

  // Use this for initialization
  void Start() {
		
  }
	
  // Update is called once per frame
  void Update() {
		
  }

  public void dispatch(GameObject target) {
      
    print("dispatch!" + type);

    if (type == PickupItemActionType.AMMO) {
        
      WeaponController weaponController = target.GetComponent<WeaponController>();
      weaponController.increaseAmmoReserve(50);

    } else if (type == PickupItemActionType.HEALTH) {

      WeaponController weaponController = target.GetComponent<WeaponController>();
      weaponController.increasePlayerStamina(25f);
    }

    StartCoroutine(consume());
  }

  IEnumerator consume() {

    gameObject.SetActive(false);

    // play pickup sound
    yield return new WaitForSeconds(0.5f);
    Destroy(gameObject);
  }
}
