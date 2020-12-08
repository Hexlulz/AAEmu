using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSChangeMateNamePacket : GamePacket
    {
        private ushort _tlId;
        private string _name;
        public CSChangeMateNamePacket() : base(0x0a6, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _tlId = stream.ReadUInt16();
            _name = stream.ReadString();
        }

        public override void Execute()
        {
            //_log.Warn("ChangeMateName, TlId: {0}, Name: {1}", tlId, name);
            MateManager.Instance.RenameMount(Connection, _tlId, _name);
        }
    }
}
