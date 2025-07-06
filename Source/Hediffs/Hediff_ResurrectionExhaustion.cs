using RimWorld;
using Verse;

namespace SoulSerpent
{
    public class Hediff_ResurrectionExhaustion : HediffWithComps
    {
        public override void PostRemoved()
        {
            base.PostRemoved();
            
            // Make the pawn tired after recovering from resurrection exhaustion
            if (pawn.needs?.rest != null)
            {
                pawn.needs.rest.CurLevel = 0.2f;
            }
        }
    }
} 