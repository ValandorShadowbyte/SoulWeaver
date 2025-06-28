using System;
using System.Collections.Generic;
using Verse;

namespace SoulSerpent
{
    public class Hediff_Soulweaver : HediffWithComps
    {
        public List<Pawn> markedPawns = new List<Pawn>();
        
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            // Apply body decay when soulweaver hediff is added
            if (pawn != null && !pawn.Dead)
            {
                SoulSerpentUtils.TryAddHediff<Hediff_BodyDecay>(pawn, SoulSerpentDefs.VS_BodyDecay);
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref markedPawns, "markedPawns", LookMode.Reference, Array.Empty<object>());
        }
    }
}
