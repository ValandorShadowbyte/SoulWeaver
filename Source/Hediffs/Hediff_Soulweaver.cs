using System;
using System.Collections.Generic;
using Verse;

namespace SoulSerpent
{
    public class Hediff_Soulweaver : HediffWithComps
    {
        public List<Pawn> markedPawns = new List<Pawn>();
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref markedPawns, "markedPawns", LookMode.Reference, Array.Empty<object>());
        }
    }
}
