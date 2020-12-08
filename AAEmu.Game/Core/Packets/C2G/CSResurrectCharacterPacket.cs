using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSResurrectCharacterPacket : GamePacket
    {
        private bool _inPlace;
        public CSResurrectCharacterPacket() : base(0x04e, 1)
        {
        }

        public override void Read(PacketStream stream)
        {
            _inPlace = stream.ReadBoolean();
        }

        public override void Execute()
        {
            
            _log.Debug("ResurrectCharacter, InPlace: {0}", _inPlace);

            if (_inPlace)
            {
                Connection.ActiveChar.Hp = (int)(Connection.ActiveChar.MaxHp * (Connection.ActiveChar.ResurrectHpPercent / 100.0f));
                Connection.ActiveChar.Mp = (int)(Connection.ActiveChar.MaxMp * (Connection.ActiveChar.ResurrectMpPercent / 100.0f));
                Connection.ActiveChar.ResurrectHpPercent = 1;
                Connection.ActiveChar.ResurrectMpPercent = 1;
            }
            else
            {
                Connection.ActiveChar.Hp = (int)(Connection.ActiveChar.MaxHp * 0.1);
                Connection.ActiveChar.Mp = (int)(Connection.ActiveChar.MaxMp * 0.1);
            }

            Connection.ActiveChar.BroadcastPacket(
                new SCCharacterResurrectedPacket(
                    Connection.ActiveChar.ObjId,
                    Connection.ActiveChar.Position.X,
                    Connection.ActiveChar.Position.Y,
                    Connection.ActiveChar.Position.Z,
                    0
                ),
                true
            );

            Connection.ActiveChar.BroadcastPacket(
                new SCUnitPointsPacket(
                    Connection.ActiveChar.ObjId,
                    Connection.ActiveChar.Hp,
                    Connection.ActiveChar.Mp
                ),
                true
            );
            Connection.ActiveChar.StartRegen();
        }
    }
}
