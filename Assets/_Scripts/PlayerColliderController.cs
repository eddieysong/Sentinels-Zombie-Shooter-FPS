using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderController : MonoBehaviour {

  private GameObject weapons;

  // Use this for initialization
  void Start() {
    weapons = GameObject.Find("Weapons");		
  }
	
  // Update is called once per frame
  void Update() {
		
  }

  void OnTriggerEnter(Collider other) {

    //print("OnTriggerEnter: " + other.gameObject.tag);

    if (other.gameObject.CompareTag("PickupItem")) {
      PickupItemActionDispatcher actionDispatcher = other.gameObject.GetComponent<PickupItemActionDispatcher>();

      if (actionDispatcher != null) {
        actionDispatcher.dispatch(weapons);
      }
    }
  }
}
