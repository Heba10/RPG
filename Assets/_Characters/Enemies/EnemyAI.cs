﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters
{
    [RequireComponent(typeof(HealthSystem))]
    [RequireComponent(typeof(WeaponSystem))]
    [RequireComponent(typeof(Character))]
    public class EnemyAI : MonoBehaviour//no Idamageable because we are going fron interface to component
    {

        [SerializeField] float chaseRadious = 6f;
        [SerializeField] float waypointTolerance = 1.5f;
        [SerializeField] WaypointContainer patrolPath;
        [SerializeField] float waypointDwellTime = 2.0f;

        enum State { idle, attacking, patrolling, chasing };
        State state = State.idle;
        float distanceToPlayer;
        float currentWeaponRange = 3f;
        int nextWaypointIndex;
        PlayerControl player = null;
        Character character;
        WeaponSystem weaponSystem;

        private void Start()
        {
            character = GetComponent<Character>();
            player = FindObjectOfType<PlayerControl>();
            weaponSystem = GetComponent<WeaponSystem>();
        }

        private void Update()
        {
            distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            currentWeaponRange = weaponSystem.GetCurrentWeapon().GetMaxAttackRange();

            if(distanceToPlayer > chaseRadious && state != State.patrolling)
            {
                //stop what we'are doing
                StopAllCoroutines();
                //Stop weaponSystem Coroutine.
               // weaponSystem.StopAllCoroutines();
                //start patrolling
                StartCoroutine(Patrol());
            }
            if (distanceToPlayer <=  chaseRadious && state != State.chasing)
            {
                //stop what we'are doing
                StopAllCoroutines();
                //weaponSystem.StopAllCoroutines();
                //start chasing
                StartCoroutine(ChasePlayer());
            }
            if(distanceToPlayer <= currentWeaponRange && state != State.attacking)
            {
                //stop what we're doing
                StopAllCoroutines();
                //start attacking
                StartCoroutine(AttackPlayer());
            }
        }

        IEnumerator Patrol()
        {
            state = State.patrolling;

            while(patrolPath != null)
            {
                //work out where to go next
                Vector3 nextWaypointPos = patrolPath.transform.GetChild(nextWaypointIndex).position;
                //tell character to go there
                character.SetDestination(nextWaypointPos);
                //cycle waypoints
                CycleWaypointWhenClose(nextWaypointPos);
                //wait at a waypoint
                yield return new WaitForSeconds(waypointDwellTime); 
            }       
        }

        IEnumerator ChasePlayer()
        {
            state = State.chasing;
            while (distanceToPlayer >= currentWeaponRange)
            {
                character.SetDestination(player.transform.position);
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator AttackPlayer()
        {
            state = State.attacking;
            while(distanceToPlayer <= currentWeaponRange)
            {
                weaponSystem.AttackTarget(player.gameObject);
                yield return new WaitForEndOfFrame();
            }
        }

        private void CycleWaypointWhenClose(Vector3 nextWaypointPos)
        {
            if(Vector3.Distance(transform.position, nextWaypointPos) <= waypointTolerance)
            {
                nextWaypointIndex = (nextWaypointIndex + 1) % patrolPath.transform.childCount;
            }
        }

        //TODO , Consider re-working this Method to make it fire an arrow that if it enterd the Player it will dealy damage
        // otherwise , the damage wont be dealt , also considering making damage on AnimationEvents.
        //void FireProjectile()
        //{
        //    GameObject newProjectile = Instantiate(projectileToUse, projectileSocket.transform.position, Quaternion.identity);
        //    Projectile projectileComponant = newProjectile.GetComponent<Projectile>();
        //    projectileComponant.damageCaused = damagePerShot;
        //    projectileComponant.SetShooter(gameObject);
        //    Vector3 unitVectorToPlayer = (player.transform.position + aimOffset - projectileSocket.transform.position).normalized;
        //    float projectileSpeed = projectileComponant.GetDefaultLaunchSpeed();
        //    newProjectile.GetComponent<Rigidbody>().velocity = unitVectorToPlayer * projectileSpeed;
        //}


        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(255f, 0f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, currentWeaponRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseRadious);
        }
    }
}
