using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.DoodadObj;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSChallengeDuelPacket : GamePacket
    {
        private uint _challengedId;
        private uint _challengerId;
        public CSChallengeDuelPacket() : base(0x050, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _challengedId = stream.ReadUInt32(); // Id the one we challenged to a duel
            _challengerId = Connection.ActiveChar.Id;
        }

        public override void Execute()
        {
            Connection.ActiveChar.BroadcastPacket(new SCDuelChallengedPacket(_challengerId), false); // only to the enemy
            _log.Warn("ChallengeDuel, challengedId: {0}", _challengedId);
        }
    }
}
