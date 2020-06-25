using System.Threading;
using System.Threading.Tasks;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Core.Packets.G2C;

namespace AAEmu.Game.Models.Game.Skills.Plots
{
    public class Plot
    {
        public uint Id { get; set; }
        public uint TargetTypeId { get; set; }

        public PlotEventTemplate EventTemplate { get; set; }

        public async Task Execute(Unit caster, SkillCaster casterCaster, BaseUnit target, SkillCastTarget targetCaster, SkillObject skillObject, Skill skill ,CancellationToken token)
        {
            PlotInstance instance = new PlotInstance(caster, casterCaster, target, targetCaster, skillObject, skill, token);

            if (skill.Template.CastingTime == 0)
            {
                //caster.BroadcastPacket(new SCSkillStartedPacket(skill.Id, skill.TlId, casterCaster, targetCaster, skill, skillObject), true);
                //caster.BroadcastPacket(new SCSkillFiredPacket(skill.Id, skill.TlId, casterCaster, targetCaster, skill, skillObject), true);
            }
            NLog.LogManager.GetCurrentClassLogger().Debug($"Plot: {Id} tl: {skill.TlId} Executing.");
            await Task.Run((() => EventTemplate.PlayEvent(instance, null)));
            NLog.LogManager.GetCurrentClassLogger().Debug($"Plot: {Id} tl: {skill.TlId} Finished.");
            caster.BroadcastPacket(new SCPlotEndedPacket(instance.ActiveSkill.TlId), true);
        }
    }
}
