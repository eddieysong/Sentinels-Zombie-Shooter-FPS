using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour {

    public NavMeshAgent navAgent;
    private GameObject player;

    // Use this for initialization
    void Start() {

        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update() {

        navAgent.SetDestination(player.transform.position);

    }
}
