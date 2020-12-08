using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Managers.UnitManagers;
using System.Collections.Generic;


namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSRollDicePacket : GamePacket
    {
        private uint _max;
        public CSRollDicePacket() : base(0x0bd, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _max = stream.ReadUInt32();
        }

        public override void Execute()
        {
            CharacterManager.Instance.PlayerRoll(Connection.ActiveChar, int.Parse(_max.ToString())); 
        }
    }
}
