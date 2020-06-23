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
            //caster.BroadcastPacket(new SCSkillStartedPacket(skill.Id, skill.TlId, casterCaster, targetCaster, skill, skillObject), true);
            NLog.LogManager.GetCurrentClassLogger().Error($"Plot: {Id} Executing.");
            await Task.Run((() => EventTemplate.PlayEvent(instance, null)));
            NLog.LogManager.GetCurrentClassLogger().Error($"Plot: {Id} Finished.");

            if (!instance.PlotEnded)
            {
                caster.BroadcastPacket(new SCPlotEndedPacket(instance.ActiveSkill.TlId), true);
            }
            if(token.IsCancellationRequested)
            {
                caster.BroadcastPacket(new SCSkillEndedPacket(skill.TlId), true);

            }
        }
    }
}
