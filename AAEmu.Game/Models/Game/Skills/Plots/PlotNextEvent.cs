using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Skills.Effects;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Utils;

namespace AAEmu.Game.Models.Game.Skills.Plots
{
    public class PlotNextEvent
    {
        public PlotEventTemplate Event { get; set; }
        public int Position { get; set; }
        public bool PerTarget { get; set; }
        public bool Casting { get; set; }
        public int Delay { get; set; }
        public int Speed { get; set; }
        public bool Channeling { get; set; }
        public int CastingInc { get; set; }
        public bool AddAnimCsTime { get; set; }
        public bool CastingDelayable { get; set; }
        public bool CastingCancelable { get; set; }
        public bool CancelOnBigHit { get; set; }
        public bool UseExeTime { get; set; }
        public bool Fail { get; set; }
        
        private int GetAnimDelay(IEnumerable<PlotEventEffect> effects)
        {
            if (!AddAnimCsTime)
                return 0;

            foreach (var effect in effects)
            {
                var template = SkillManager.Instance.GetEffectTemplate(effect.ActualId, effect.ActualType);
                if (!(template is SpecialEffect specialEffect))
                    continue;

                if (specialEffect.SpecialEffectTypeId != SpecialType.Anim)
                    continue;

                var anim = AnimationManager.Instance.GetAnimation((uint)specialEffect.Value1);
                return anim.CombatSyncTime;
            }

            return 0;
        }
        
        private int GetProjectileDelay(BaseUnit caster, BaseUnit target)
        {
            if (Speed <= 0)
                return 0;

            var dist = MathUtil.CalculateDistance(caster.Position, target.Position, true);
            //We want damage to be applied when the projectile hits target.
            return (int)Math.Round((dist / Speed) * 1000.0f);

        }

        public Task PlayNextEvent(PlotInstance instance, PlotEventInstance eventInstance, BaseUnit caster,
            BaseUnit target, IEnumerable<PlotEventEffect> effects)
        {
            var animTime = GetAnimDelay(effects);
            var projectileTime = GetProjectileDelay(caster, target);
            var delay = animTime + projectileTime;
            if (Casting)
                delay += (int)instance.Caster.ApplySkillModifiers(instance.ActiveSkill, Static.SkillAttribute.CastTime,
                    Delay);
            else
                delay += Delay;
            delay = Math.Clamp(delay, 0, int.MaxValue);
            return delay > 0 ? Event.PlayEvent(instance, eventInstance, this, delay) : Event.PlayEvent(instance, eventInstance, this);
        }
    }
}
