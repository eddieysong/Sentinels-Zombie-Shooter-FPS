using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour {

  private Animator doorAnimator;
  private float lastDoorChange;
  private static float minimumPeriod = 1f;

  // Use this for initialization
  void Start() {
    doorAnimator = GetComponent<Animator>();
    lastDoorChange = 0f;
  }
	
  // Update is called once per frame
  void Update() {

    lastDoorChange += Time.deltaTime;

    if (lastDoorChange >= minimumPeriod && Input.GetKey(KeyCode.C)) {
      doorAnimator.SetInteger("state", doorAnimator.GetInteger("state") == 0 ? 1 : 0);
    }
  }
}