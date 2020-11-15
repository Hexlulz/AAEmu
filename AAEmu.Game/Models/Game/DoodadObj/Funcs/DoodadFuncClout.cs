﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.UnitManagers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.DoodadObj.Templates;
using AAEmu.Game.Models.Game.Skills;
using AAEmu.Game.Models.Game.Skills.Effects;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.DoodadObj.Funcs
{
    public class DoodadFuncClout : DoodadFuncTemplate
    {
        public int Duration { get; set; }
        public int Tick { get; set; }
        public uint TargetRelationId { get; set; }
        public uint BuffId { get; set; }
        public uint ProjectileId { get; set; }
        public bool ShowToFriendlyOnly { get; set; }
        public uint NextPhase { get; set; }
        public uint AoeShapeId { get; set; }
        public uint TargetBuffTagId { get; set; }
        public uint TargetNoBuffTagId { get; set; }
        public bool UseOriginSource { get; set; }
        public List<uint> Effects { get; set; }

        public override void Use(Unit caster, Doodad owner, uint skillId)
        {
            _log.Debug("DoodadFuncClout : Duration {0}, Tick {1}, TargetRelationId {2}, BuffId {3}," +
                       " ProjectileId {4}, ShowToFriendlyOnly {5}, NextPhase {6}, AoeShapeId {7}," +
                       " TargetBuffTagId {8}, TargetNoBuffTagId {9}, UseOriginSource {10}",
                Duration, Tick, TargetRelationId, BuffId, ProjectileId, ShowToFriendlyOnly, NextPhase, AoeShapeId, TargetBuffTagId, TargetNoBuffTagId, UseOriginSource);

            if (Duration > 0)
            {
                // TODO : Add a proper delay in here
                Task.Run(async () =>
                {
                    await Task.Delay(Duration);
                    if (NextPhase == 0) 
                        owner.Delete();
                    DoodadManager.Instance.TriggerFunc(GetType().Name, caster, owner, skillId, NextPhase);
                });
            }
            
            // Tick logic
            // If tick is > 0 we apply the BuffId to every player in the AoeShape 
            if (Tick > 0)
            {
                Task.Run(async () =>
                {
                    await ScheduleTick(caster, owner);
                });
            }
            
            // Area Trigger logic
            // If tick is == 0 we create an AreaTrigger which applies the effect everytime someone enters the area
        }

        private async Task ScheduleTick(Unit caster, Doodad owner)
        {
            if (owner == null) return;
            await Task.Delay(Tick);
            var buff = SkillManager.Instance.GetBuffTemplate(BuffId);
            var units = WorldManager.Instance.GetAroundByShape<Unit>(owner,
                WorldManager.Instance.GetAreaShapeById(AoeShapeId));
            
            foreach (var effectId in Effects)
            {
                var effect = SkillManager.Instance.GetEffectTemplate(effectId);
                foreach (var unit in units)
                {
                    var existingEffect = unit.Effects.GetEffectByTemplate(effect);
                    if (existingEffect == null)
                    {
                        unit.Effects.AddEffect(new Effect(unit, caster, new SkillCasterUnit(caster.ObjId), effect, null,
                            DateTime.Now));
                    }
                    else
                    {
                        // Refresh duration of buff
                    }
                }
            }

            ScheduleTick(caster, owner);
        }
    }
}
