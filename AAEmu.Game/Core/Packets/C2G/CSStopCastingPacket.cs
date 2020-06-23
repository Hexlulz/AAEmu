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

            NLog.LogManager.GetCurrentClassLogger().Error($"sid {sid}    tl/pid{pid}   objId {objId}");
            //Experimental!
            if (pid != 0)
            {
                Connection.ActiveChar.BroadcastPacket(new SCPlotCastingStoppedPacket(pid, 0, 1), true);
                Connection.ActiveChar.BroadcastPacket(new SCPlotChannelingStoppedPacket(pid, 0, 1), true);
                Connection.ActiveChar.BroadcastPacket(new SCPlotEndedPacket(pid), true);
            }

            if (sid != 0)
            {
                Connection.ActiveChar.BroadcastPacket(new SCSkillEndedPacket(sid), true);
            }

            if (Connection.ActiveChar.ObjId != objId || Connection.ActiveChar.SkillTask == null ||
                Connection.ActiveChar.SkillTask.Skill.TlId != sid)
                return;
            await Connection.ActiveChar.SkillTask.Cancel();
            Connection.ActiveChar.SkillTask.Skill.Stop(Connection.ActiveChar);
        }
    }
}
