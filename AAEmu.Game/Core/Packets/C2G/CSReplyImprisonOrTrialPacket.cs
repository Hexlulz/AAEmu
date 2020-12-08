using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSReplyImprisonOrTrialPacket : GamePacket
    {
        private bool _trial;
        public CSReplyImprisonOrTrialPacket() : base(0x06f, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _trial = stream.ReadBoolean();
        }

        public override void Execute()
        {
            _log.Warn("ReplyImprisonOrTrial, Trial: {0}", _trial);
        }
    }
}
