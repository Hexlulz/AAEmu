using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Skills.Effects;
using AAEmu.Game.Models.Game.Skills.Static;
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


        private bool СheckСonditions(PlotInstance instance)
        {
            return Conditions
                .AsParallel()
                .All(condition => condition.CheckCondition(instance));
        }

        private bool HasSpecialEffects()
        {
            return Effects
                .Select(eff => SkillManager.Instance.GetEffectTemplate(eff.ActualId, eff.ActualType))
                .OfType<SpecialEffect>()
                .Any();
        }

        private bool ApplyEffects(PlotInstance instance, ref byte flag)
        {
            var appliedEffects = false;
            var skill = instance.ActiveSkill;

            foreach (var eff in Effects)
            {
                eff.ApplyEffect(instance, this, skill, ref flag, ref appliedEffects);
            }

            return appliedEffects;
        }

        public async Task PlayEvent(PlotInstance instance, PlotEventInstance eventInstance, PlotNextEvent cNext)
        {
            byte flag = 2;

            if ((instance.Ct.IsCancellationRequested && (cNext?.Casting ?? false)) || instance.Canceled)
            {
                instance.Canceled = true;
                return;
            }

            //Do tickets
            if (instance.Tickets.ContainsKey(Id))
                instance.Tickets[Id]++;
            else
                instance.Tickets.TryAdd(Id, 1);

            //Check if we hit max tickets
            if (instance.Tickets[Id] > Tickets && Tickets > 1)
            {
                return;
            }

            // Check Conditions
            //TODO Loop for every target in PlotEventInstance
            var pass = СheckСonditions(instance);
            if (pass)
                ApplyEffects(instance, ref flag);
            else
                flag = 0;

            double castTime = NextEvents
                .Where(nextEvent => nextEvent.Casting && (pass ^ nextEvent.Fail))
                .Aggregate(0, (current, nextEvent) => (current > nextEvent.Delay) ? current : (nextEvent.Delay / 10));
            castTime = instance.Caster.ApplySkillModifiers(instance.ActiveSkill, SkillAttribute.CastTime, castTime);
            castTime = Math.Clamp(castTime, 0, double.MaxValue);

            if (HasSpecialEffects())
            {
                var skill = instance.ActiveSkill;
                var unkId = ((cNext?.Casting ?? false) || (cNext?.Channeling ?? false)) ? instance.Caster.ObjId : 0;
                var casterPlotObj = new PlotObject(instance.Caster);
                var targetPlotObj = new PlotObject(instance.Target);
                instance.Caster.BroadcastPacket(
                    new SCPlotEventPacket(skill.TlId, Id, skill.Template.Id, casterPlotObj, targetPlotObj, unkId,
                        (ushort)castTime, flag), true);
            }

            var tasks = NextEvents
                .AsParallel()
                .Where(nextEvent => pass ^ nextEvent.Fail)
                .Select(nextEvent => nextEvent
                    .PlayNextEvent(instance, new PlotEventInstance(eventInstance), instance.Caster, instance.Target,
                        Effects)
                )
                .ToArray();

            // var tasks = new List<Task>();
            // foreach (var nextEvent in NextEvents)
            // {
            //     if (pass ^ nextEvent.Fail)
            //         tasks.Add(nextEvent.PlayNextEvent(instance, new PlotEventInstance(eventInstance), instance.Caster,
            //             instance.Target, Effects));
            // }

            await Task.WhenAll(tasks.ToArray());
        }

        public async Task PlayEvent(PlotInstance instance, PlotEventInstance eventInstance, PlotNextEvent cNext,
            int delay)
        {
            await Task.Delay(delay);
            await PlayEvent(instance, eventInstance, cNext);
        }
    }
}
