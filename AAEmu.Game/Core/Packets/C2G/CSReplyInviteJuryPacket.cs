using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSReplyInviteJuryPacket : GamePacket
    {
        private bool _accept;
        private uint _trial;
        public CSReplyInviteJuryPacket() : base(0x071, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _accept = stream.ReadBoolean();
            _trial = stream.ReadUInt32();
        }

        public override void Execute()
        {
            _log.Warn("ReplyInviteJury, Accept: {0}, Trial: {1}", _accept, _trial);
        }
    }
}
