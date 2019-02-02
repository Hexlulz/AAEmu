using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.Id;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Crafts;
using AAEmu.Game.Models.Game.Items;
using AAEmu.Game.Models.Game.Items.Actions;
using System.Collections.Generic;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSExecuteCraft : GamePacket
    {
        public CSExecuteCraft() : base(0x0f5, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            var craftId = stream.ReadUInt32();
            var objId = stream.ReadBc();  // no idea what this one is boys.
            var count = stream.ReadInt32();

            _log.Debug("CSExecuteCraft, craftId : {0} , objId : {1}, count : {2}", craftId, objId, count);
        
        
            /*      tests        */

            Craft craft = CraftManager.Instance.GetCraftById(craftId);
            List<CraftMaterial> mats = CraftManager.Instance.GetMaterialsForCraftId(craftId);
            CraftProduct result = CraftManager.Instance.GetResultForCraftId(craftId);
            

            //need to check that player has all the required items in inventory
            //TODO

            //this snippet adds the resulting item to player inventory
            Character character = Connection.ActiveChar;
            Item resultItem = ItemManager.Instance.Create(result.ItemId, result.Amount, 0);
            
            var res = character.Inventory.AddItem(resultItem);
            if (res == null)
            {
                ItemIdManager.Instance.ReleaseId((uint) resultItem.Id);
                return;
            }

            var tasks = new List<ItemTask>();
            if (res.Id != resultItem.Id)
                tasks.Add(new ItemCountUpdate(res, resultItem.Count));
            else
                tasks.Add(new ItemAdd(resultItem));
            character.SendPacket(new SCItemTaskSuccessPacket(ItemTaskType.AutoLootDoodadItem, tasks, new List<ulong>()));
        }
    }
}
