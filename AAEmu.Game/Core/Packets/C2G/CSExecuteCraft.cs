using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Crafts;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSExecuteCraft : GamePacket
    {
        private uint _craftId;
        private uint _objId;
        private int _count;
        public CSExecuteCraft() : base(0x0f8, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _craftId = stream.ReadUInt32();
            _objId = stream.ReadBc();
            _count = stream.ReadInt32();
        }

        public override void Execute()
        {
            
            _log.Debug("CSExecuteCraft, craftId : {0} , objId : {1}, count : {2}", _craftId, _objId, _count);
        
            var craft = CraftManager.Instance.GetCraftById(_craftId);
            var character = Connection.ActiveChar;
            character.Craft.Craft(craft, _count, _objId);
        }
    }
}
