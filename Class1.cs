using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ImbuableHands
{
    public class HandsModule : ThunderScript
    {
        [ModOption(name: "Allow Imbuing", tooltip: "Enables/disables imbuing your hands", valueSourceName: nameof(booleanOption), defaultValueIndex = 0)]
        public static bool Enabled = true;
        public static ModOptionBool[] booleanOption =
        {
            new ModOptionBool("Enabled", true),
            new ModOptionBool("Disabled", false)
        };
        public static Dictionary<string, Imbue> rightImbues = new Dictionary<string, Imbue>();
        public static Dictionary<string, Imbue> leftImbues = new Dictionary<string, Imbue>();
        public override void ScriptEnable()
        {
            base.ScriptEnable();
            EventManager.OnSpellUsed += EventManager_OnSpellUsed;
        }
        private void EventManager_OnSpellUsed(string spellId, Creature creature, Side side)
        {
            if (creature?.player != null && creature?.GetHand(side) is RagdollHand hand && !hand.caster.isFiring && hand.playerHand.controlHand.gripPressed && Catalog.GetData<SpellCastCharge>(spellId) is SpellCastCharge spell && spell.imbueEnabled && Enabled && hand.grabbedHandle == null)
            {
                if (hand.GetComponent<Item>() == null)
                {
                    Item item = hand.gameObject.AddComponent<Item>();
                    item.Load(Catalog.GetData<ItemData>("Hand"));
                    hand.collisionHandler.item = item;
                    hand.colliderGroup.collisionHandler.item = item;
                }
                if (hand.colliderGroup?.modifier?.imbueType != ColliderGroupData.ImbueType.Crystal)
                {
                    hand.colliderGroup.modifier.imbueType = ColliderGroupData.ImbueType.Crystal;
                    hand.colliderGroup.imbueShoot = hand.grip;
                }
                if (hand.colliderGroup?.GetComponent<HandsColliderGroup>() == null)
                {
                    hand.colliderGroup?.gameObject.AddComponent<HandsColliderGroup>();
                }
                foreach (Creature.RendererData renderer in hand.renderers)
                {
                    if (!hand.otherHand.renderers.Contains(renderer))
                    {
                        hand.colliderGroup.imbueEffectRenderer = renderer.renderer;
                        hand.colliderGroup.imbueEmissionRenderer = renderer.renderer;
                        break;
                    }
                }
                if (hand.colliderGroup?.imbue?.spellCastBase?.id != spellId && !(side == Side.Right ? rightImbues : leftImbues).ContainsKey(spellId))
                {
                    hand.colliderGroup.imbue = hand.colliderGroup.gameObject.AddComponent<Imbue>();
                    (side == Side.Right ? rightImbues : leftImbues).Add(spellId, hand.colliderGroup.imbue);
                }
                (side == Side.Right ? rightImbues : leftImbues)[spellId]?.Transfer(spell, (side == Side.Right ? rightImbues : leftImbues)[spellId].maxEnergy);
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
