using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace ImbuableHands
{
    public class HandsModule : LevelModule
    {
        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onPossess += EventManager_onPossess;
            return base.OnLoadCoroutine();
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd) creature.gameObject.AddComponent<HandsPlayer>();
        }
    }
    public class HandsPlayer : MonoBehaviour
    {
        Creature player;
        public void Start()
        {
            player = GetComponent<Creature>();
            player.handLeft.gameObject.AddComponent<Hands>();
            player.handRight.gameObject.AddComponent<Hands>();
        }
    }
    public class Hands : MonoBehaviour
    {
        Item item;
        RagdollHand hand;
        SpellCaster caster;
        public void Start()
        {
            GameObject gameObject = new GameObject();
            item = gameObject.AddComponent<Item>();
            item.data = new ItemData
            {
                tier = 4
            };
            item.lastHandler = hand;
            item.gameObject.transform.position = Vector3.zero;
            item.rb.isKinematic = true;
            hand = GetComponent<RagdollHand>();
            caster = hand.caster;
            hand.colliderGroup.Load(Catalog.GetData<ColliderGroupData>("CrystalStaff"));
            hand.colliderGroup.gameObject.AddComponent<HandsColliderGroup>();
            hand.collisionHandler.OnCollisionStartEvent += CollisionHandler_OnCollisionStartEvent;
            hand.collisionHandler.OnCollisionStopEvent += CollisionHandler_OnCollisionStopEvent;
            hand.collisionHandler.item = item;
            hand.colliderGroup.imbueShoot = hand.grip;
        }

        private void CollisionHandler_OnCollisionStopEvent(CollisionInstance collisionInstance)
        {
            if (hand.colliderGroup.imbue?.spellCastBase != null)
                hand.colliderGroup.imbue.spellCastBase.OnImbueCollisionStop(collisionInstance);
        }

        private void CollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (hand.colliderGroup.imbue?.spellCastBase != null)
                hand.colliderGroup.imbue.spellCastBase.OnImbueCollisionStart(collisionInstance);
        }

        public void Update()
        {
            if(hand.playerHand?.controlHand != null && hand.playerHand.controlHand.gripPressed && caster?.spellInstance != null && caster.isFiring && hand.grabbedHandle == null &&
                typeof(SpellCastCharge).IsAssignableFrom(caster.spellInstance.GetType()) && Catalog.GetData<SpellCastCharge>(caster.spellInstance.id).imbueEnabled)
            {
                if (!hand.renderers.IsNullOrEmpty())
                {
                    hand.colliderGroup.imbueEffectRenderer = hand.renderers[0].renderer;
                    hand.colliderGroup.imbueEmissionRenderer = hand.renderers[0].renderer;
                }
                SpellCastCharge instance = Catalog.GetData<SpellCastCharge>(caster.spellInstance.id);
                hand.colliderGroup.imbue.Transfer(instance, instance.imbueRate * Time.deltaTime);
            }
        }
    }
    public class HandsColliderGroup : MonoBehaviour
    {
        ColliderGroup group;
        public void Start()
        {
            group = GetComponent<ColliderGroup>();
        }
        public void OnEnable()
        {
            if(group != null && group?.imbue != null && group?.imbue?.spellCastBase != null && group?.imbue?.spellCastBase?.imbueEffect != null && group?.imbue?.energy > 0)
            {
                group?.imbue?.spellCastBase?.imbueEffect?.Play();
            }
        }
    }
}
