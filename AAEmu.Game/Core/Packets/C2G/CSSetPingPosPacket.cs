using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.World;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSSetPingPosPacket : GamePacket
    {
        private uint _teamId;
        private bool _hasPing;
        private Point _position;
        private uint _insId;
        public CSSetPingPosPacket() : base(0x087, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _teamId = stream.ReadUInt32();
            _hasPing = stream.ReadBoolean();
            _position = new Point(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
            _insId = stream.ReadUInt32();
        }

        public override void Execute()
        {
            var owner = Connection.ActiveChar;
            owner.LocalPingPosition = _position;
            if (_teamId > 0)
            {
                TeamManager.Instance.SetPingPos(owner, _teamId, _hasPing, _position, _insId);
            }
            else
            {
                owner.SendPacket(new SCTeamPingPosPacket(_hasPing, _position, _insId));
            }
        }
    }
}
