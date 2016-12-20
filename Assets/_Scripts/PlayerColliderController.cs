/// <summary>
/// PlayerColliderControl.cs
/// Last Modified: 2016-12-17
/// Created By: Thiago
/// Last Modified By: Thiago
/// Summary: this script handles item pickup logic on the player
/// </summary>

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
