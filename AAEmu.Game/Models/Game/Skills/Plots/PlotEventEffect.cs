using System;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Skills.Effects;

namespace AAEmu.Game.Models.Game.Skills.Plots
{
    public class PlotEventEffect
    {
        public int Position { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public uint ActualId { get; set; }
        public string ActualType { get; set; }

        public void ApplyEffect(PlotInstance instance, PlotEventTemplate evt, Skill skill, ref byte flag, ref bool appliedEffects)
        {
            var template = SkillManager.Instance.GetEffectTemplate(ActualId, ActualType);

            if (template is BuffEffect)
                flag = 6; //idk what this does?  
            if (template is SpecialEffect)
                appliedEffects = true;

            // TODO: Update Source and Target here.
            // Given how source/target update is the same for Effects and Conditions, either use a common object and update above, or extension methods
            
            template.Apply(
                instance.Caster,
                instance.CasterCaster,
                instance.Target,
                instance.TargetCaster,
                new CastPlot(evt.PlotId, skill.TlId, evt.Id, skill.Template.Id), skill, instance.SkillObject, DateTime.Now);
        }
    }
}
