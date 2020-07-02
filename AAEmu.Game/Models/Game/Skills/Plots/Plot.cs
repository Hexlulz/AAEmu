using System.Threading;
using System.Threading.Tasks;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Core.Managers.Id;

namespace AAEmu.Game.Models.Game.Skills.Plots
{
    public class Plot
    {
        public uint Id { get; set; }
        public uint TargetTypeId { get; set; }

        public PlotEventTemplate EventTemplate { get; set; }

        public async Task Execute(Unit caster, SkillCaster casterCaster, BaseUnit target, SkillCastTarget targetCaster, SkillObject skillObject, Skill skill)
        {
            var token = new CancellationTokenSource();
            CancellationToken ct = token.Token;
            PlotInstance instance = new PlotInstance(caster, casterCaster, target, targetCaster, skillObject, skill, ct);

            if (caster is Character character)
            {
                bool test = character.CastingCancellationTokens.TryAdd(skill.TlId, token);
                if (!test)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error("Could not add cancel token to dictionary");
                    return;
                }
            }

            NLog.LogManager.GetCurrentClassLogger().Debug($"Plot: {Id} tl: {skill.TlId} Executing.");
            await Task.Run((() => EventTemplate.PlayEvent(instance, new PlotEventInstance(instance), null)));
            NLog.LogManager.GetCurrentClassLogger().Debug($"Plot: {Id} tl: {skill.TlId} Finished.");

            if (caster is Character character2)
            {
                if (!ct.IsCancellationRequested)
                {
                    character2.CastingCancellationTokens.TryRemove(skill.TlId, out var notused);
                    caster.BroadcastPacket(new SCPlotEndedPacket(instance.ActiveSkill.TlId), true);
                }
            }
            TlIdManager.Instance.ReleaseId(skill.TlId);
        }
    }
}
