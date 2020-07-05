using AAEmu.Game.Core.Packets.G2C;

namespace AAEmu.Game.Models.Game.Skills.Plots
{
    public class PlotEventCondition
    {
        public PlotCondition Condition { get; set; }
        public int Position { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public bool NotifyFailure { get; set; }

        // TODO 1.2 // public bool NotifyFailure { get; set; }

        public bool CheckCondition(PlotInstance instance)
        {
            if (GetConditionResult(instance, this))
                return true;

            if (NotifyFailure)
                instance.Caster.BroadcastPacket(new SCSkillStoppedPacket(instance.Caster.ObjId, instance.ActiveSkill.Id), true);
            
            return false;

        }
        
        private static bool GetConditionResult(PlotInstance instance, PlotEventCondition condition)
        {
            lock (instance.ConditionLock)
            {
                var not = condition.Condition.NotCondition;
                //Check if condition was cached
                // if (instance.UseConditionCache(condition.Condition))
                // {
                //     var cacheResult = instance.GetConditionCacheResult(condition.Condition);
                //     //Apply not condition
                //     cacheResult = not ? !cacheResult : cacheResult;
                //
                //     return cacheResult;
                // }

                // TODO : Apply Source & Target update here!
                
                //Check 
                var result = condition.Condition.Check(instance.Caster, instance.CasterCaster, instance.Target, instance.TargetCaster, instance.SkillObject, condition);
                if (result)
                {
                    //We need to undo the not condition to store in cache
                    // instance.UpdateConditionCache(condition.Condition, !not);
                    return true;
                }

                // instance.UpdateConditionCache(condition.Condition, not);
                return false;
            }
        }
    }
}
