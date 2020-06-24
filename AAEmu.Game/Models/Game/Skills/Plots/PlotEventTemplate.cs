using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Skills.Effects;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Utils;

namespace AAEmu.Game.Models.Game.Skills.Plots
{
    public class PlotEventTemplate
    {
        public uint Id { get; set; }
        public uint PlotId { get; set; }
        public int Position { get; set; }
        public uint SourceUpdateMethodId { get; set; }
        public uint TargetUpdateMethodId { get; set; }
        public int TargetUpdateMethodParam1 { get; set; }
        public int TargetUpdateMethodParam2 { get; set; }
        public int TargetUpdateMethodParam3 { get; set; }
        public int TargetUpdateMethodParam4 { get; set; }
        public int TargetUpdateMethodParam5 { get; set; }
        public int TargetUpdateMethodParam6 { get; set; }
        public int TargetUpdateMethodParam7 { get; set; }
        public int TargetUpdateMethodParam8 { get; set; }
        public int TargetUpdateMethodParam9 { get; set; }
        public int Tickets { get; set; }
        public bool AoeDiminishing { get; set; }
        public LinkedList<PlotEventCondition> Conditions { get; set; }
        public LinkedList<PlotEventEffect> Effects { get; set; }
        public LinkedList<PlotNextEvent> NextEvents { get; set; }

        public PlotEventTemplate()
        {
            Conditions = new LinkedList<PlotEventCondition>();
            Effects = new LinkedList<PlotEventEffect>();
            NextEvents = new LinkedList<PlotNextEvent>();
        }

        private bool GetConditionResult(PlotInstance instance,PlotCondition condition)
        {
            lock (instance.ConditionLock)
            {
                var not = condition.NotCondition;
                //Check if condition was cached
                if (instance.UseConditionCache(condition))
                {
                    try
                    {
                        var cacheResult = instance.GetConditionCacheResult(condition);
                        //Apply not condition
                        cacheResult = not ? !cacheResult : cacheResult;

                        return cacheResult;
                    }
                    catch { NLog.LogManager.GetCurrentClassLogger().Error("GetCondition Failed."); }
                }

                //Check 
                if (condition.Check(instance.Caster, instance.CasterCaster, instance.Target, instance.TargetCaster, instance.SkillObject))
                {
                    //We need to undo the not condition to store in cache
                    try
                    {
                        instance.UpdateConditionCache(condition, not ? false : true);
                    } catch { NLog.LogManager.GetCurrentClassLogger().Error("UpdateCondition Failed."); }
                    return true;
                }
                else
                {
                    try
                    {
                        instance.UpdateConditionCache(condition, not ? true : false);
                    } catch { NLog.LogManager.GetCurrentClassLogger().Error("UpdateCondition Failed."); }
                    return false;
                }
            }
        }

        private bool СheckСonditions(PlotInstance instance)
        {
            foreach (var condition in Conditions)
            {
                try
                {
                    if (!GetConditionResult(instance, condition.Condition))
                    {
                        if (condition.NotifyFailure)
                            instance.Caster.BroadcastPacket(new SCSkillStoppedPacket(instance.Caster.ObjId, instance.ActiveSkill.Id), true);
                        return false;
                    }
                }
                catch { NLog.LogManager.GetCurrentClassLogger().Error("CheckConditions Failed."); }
            }

            return true;
        }

        private int GetProjectileDelay(PlotNextEvent nextEvent, BaseUnit caster, BaseUnit target)
        {
            try
            {
                if (nextEvent == null)
                    return 0;
                if (nextEvent.Speed > 0)
                {
                    var dist = MathUtil.CalculateDistance(caster.Position, target.Position, true);
                    //We want damage to be applied when the projectile hits target.
                    return (int)Math.Round((dist / nextEvent.Speed) * 1000.0f);
                }
            } catch { NLog.LogManager.GetCurrentClassLogger().Error("GetProjectileDelay Failed."); }
            return 0;
        }

        private int GetAnimDelay(PlotNextEvent cNext)
        {
            try
            {
                if (cNext?.AddAnimCsTime ?? false)
                {
                    foreach (var effect in Effects)
                    {
                        var template = SkillManager.Instance.GetEffectTemplate(effect.ActualId, effect.ActualType);
                        if (template is SpecialEffect specialEffect)
                        {
                            if (specialEffect.SpecialEffectTypeId == SpecialType.Anim)
                            {
                                var anim = AnimationManager.Instance.GetAnimation((uint)specialEffect.Value1);
                                return anim.CombatSyncTime;
                            }
                        }
                    }
                }
                return 0;
            }catch { NLog.LogManager.GetCurrentClassLogger().Error("GetAnimDelay Failed."); }
            return 0;
        }
        private bool HasSpecialEffects()
        {
            bool has = false;
            foreach (var eff in Effects)
            {
                var template = SkillManager.Instance.GetEffectTemplate(eff.ActualId, eff.ActualType);
                if (template is SpecialEffect)
                {
                    has = true;
                }
            }
            return has;
        }

        private bool ApplyEffects(PlotInstance instance, ref byte flag)
        {
            var appliedEffects = false;
            var skill = instance.ActiveSkill;
            foreach (var eff in Effects)
            {
                var template = SkillManager.Instance.GetEffectTemplate(eff.ActualId, eff.ActualType);

                if (template is BuffEffect)
                    flag = 6; //idk what this does?  
                if (template is SpecialEffect)
                    appliedEffects = true;

                template.Apply(
                    instance.Caster,
                    instance.CasterCaster,
                    instance.Target,
                    instance.TargetCaster,
                    new CastPlot(PlotId, skill.TlId, Id, skill.Template.Id), skill, instance.SkillObject, DateTime.Now);
            }
            return appliedEffects;
        }

        public async Task PlayEvent(PlotInstance instance, PlotNextEvent cNext)
        {
            byte flag = 2;

            if (instance.Ct.IsCancellationRequested)
                return;

            //Do tickets
            if (instance.Tickets.ContainsKey(Id))
                instance.Tickets[Id]++;
            else
                instance.Tickets.TryAdd(Id, 1);

            //Check if we hit max tickets
            if (instance.Tickets[Id] > Tickets && Tickets > 1)
            {
                //Max Recursion. Leave Scope
                return;
            }

            // Check Conditions
            bool appliedEffects = false;
            
            bool pass = СheckСonditions(instance);
            if (pass)
                appliedEffects = ApplyEffects(instance,ref flag);
            else
                flag = 0;

            int castTime = 0;
            foreach (var nextEvent in NextEvents)
            {
                if (nextEvent.Casting && (pass ^ nextEvent.Fail))
                    castTime = (castTime > nextEvent.Delay) ? castTime : (nextEvent.Delay / 10);
            }
            if (HasSpecialEffects())
            {
                /*if (NextEvents.Count == 0)
                    flag = 0;*/
                var skill = instance.ActiveSkill;
                var unkId = ((cNext?.Casting ?? false) || (cNext?.Channeling ?? false)) ? instance.Caster.ObjId : 0;
                var casterPlotObj = new PlotObject(instance.Caster);
                var targetPlotObj = new PlotObject(instance.Target);
                instance.Caster.BroadcastPacket(new SCPlotEventPacket(skill.TlId, Id, skill.Template.Id, casterPlotObj, targetPlotObj, unkId, (ushort)castTime, flag), true);
                NLog.LogManager.GetCurrentClassLogger()
                    .Error($"Sent Event Packet - Id:{Id} Src:{casterPlotObj.UnitId} Trgt:{targetPlotObj.UnitId}  tl:{skill.TlId} flag:{flag}");
            }

            List<Task> tasks = new List<Task>();
            foreach (var nextEvent in NextEvents)
            {
                if (pass ^ nextEvent.Fail)
                {
                    int animTime = GetAnimDelay(nextEvent);
                    int projectileTime = GetProjectileDelay(nextEvent, instance.Caster, instance.Target);
                    int delay = animTime + projectileTime + nextEvent.Delay;

                    var task = Task.Run(() => nextEvent.Event.PlayEvent(instance, nextEvent, delay));
                    tasks.Add(task);
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        public async Task PlayEvent(PlotInstance instance, PlotNextEvent cNext ,int delay)
        {
            await Task.Delay(delay);
            await PlayEvent(instance, cNext);
        }

        public virtual bool СheckСonditions(Unit caster, SkillCaster casterCaster, BaseUnit target, SkillCastTarget targetCaster, SkillObject skillObject)
        {
            var result = true;
            foreach (var condition in Conditions)
            {
                if (condition.Condition.Check(caster, casterCaster, target, targetCaster, skillObject))
                    continue;
                result = false;
                break;
            }

            return result;
        }
    }
}
