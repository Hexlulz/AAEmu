using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSStopCastingPacket : GamePacket
    {
        public CSStopCastingPacket() : base(0x054, 1)
        {
        }

        public override async void Read(PacketStream stream)
        {
            var sid = stream.ReadUInt16(); // sid
            var pid = stream.ReadUInt16(); // tl; pid
            var objId = stream.ReadBc();

            NLog.LogManager.GetCurrentClassLogger().Debug($"sid {sid}    tl/pid{pid}   objId {objId}");
            //Experimental!
            var tokens = Connection.ActiveChar.CastingCancellationTokens;

            if (pid != 0)
            {
                foreach (var token in tokens)
                {
                    if (pid == token.Key)
                    {
                        token.Value.Cancel();
                        Connection.ActiveChar.BroadcastPacket(new SCPlotCastingStoppedPacket(pid, 0, 1), true);
                        Connection.ActiveChar.BroadcastPacket(new SCPlotChannelingStoppedPacket(pid, 0, 1), true);
                        Connection.ActiveChar.BroadcastPacket(new SCPlotEndedPacket(pid), true);
                        tokens.TryRemove(pid, out var notused);
                        return;
                    }
                }
                Connection.ActiveChar.BroadcastPacket(new SCPlotCastingStoppedPacket(pid, 0, 1), true);
                Connection.ActiveChar.BroadcastPacket(new SCPlotChannelingStoppedPacket(pid, 0, 1), true);
                Connection.ActiveChar.BroadcastPacket(new SCPlotEndedPacket(pid), true);
                NLog.LogManager.GetCurrentClassLogger().Error("Could not find the associated id in the dictionary.");
                return;
            }

            if (Connection.ActiveChar.ObjId != objId || Connection.ActiveChar.SkillTask == null ||
                Connection.ActiveChar.SkillTask.Skill.TlId != sid)
                return;
            await Connection.ActiveChar.SkillTask.Cancel();
            Connection.ActiveChar.SkillTask.Skill.Stop(Connection.ActiveChar);
        }
    }
}
