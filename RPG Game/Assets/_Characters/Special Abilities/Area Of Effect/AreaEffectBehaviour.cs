﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;

namespace RPG.Characters
{
    public class AreaEffectBehaviour : AbilityBehaviour {

        public override void Use(GameObject target)
        {
            if (canCastAbility)
            {
	            DealRadialDamage();
	            PlayAbilitySound();
	            PlayParticleEffect();
	            PlayAbilityAnimation();
                float timeSpellCasted = Time.time;
                StartCoroutine(StartCooldown(this));
                StartCoroutine(StartUICooldown(1, timeSpellCasted));

            }
        }

        private void DealRadialDamage()
        {
            //static sphere cast for targets
            RaycastHit[] hits = Physics.SphereCastAll(
                    transform.position,
                    (config as AreaEffectConfig).GetRadius(),
                    Vector3.up,
                    (config as AreaEffectConfig).GetRadius()
                );
            foreach (RaycastHit hit in hits)
            {
                var damagable = hit.collider.gameObject.GetComponent<HealthSystem>();
                bool hitPlayer = hit.collider.gameObject.GetComponent<PlayerControl>();
                if (damagable != null && !hitPlayer)
                {
                    float damageToTake = (config as AreaEffectConfig).GetDamageToEachTarget();
                    damagable.TakeDamage(damageToTake);
                }
            }
        }
    }
}
