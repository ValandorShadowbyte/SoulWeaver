using System;
using System.Collections.Generic;
using System.Linq;
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


        public Pawn TransferToTarget(Pawn target)
        {
            if (target != null && markedPawns.Contains(target))
            {
                var soulWeaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);

                SoulSerpentUtils.CopyPsylink(pawn, target);
                SoulSerpentUtils.CopyBackstory(pawn, target);
                SoulSerpentUtils.MergeBestTraitsFromDest(pawn, target);
                SoulSerpentUtils.CopySkills(pawn, target);
                TransferRelations(pawn, target);
                UpdateChronoTime(pawn, target);


                //get the mark
                Hediff mark = SoulSerpentUtils.TryGetHediff<Hediff_SoulMark>(target, SoulSerpentDefs.VS_SoulMark) ??
                    SoulSerpentUtils.TryGetHediff<Hediff_AwakenedSoulMark>(target, SoulSerpentDefs.VS_AwakenedSoulMark);

                //remove the mark
                SoulSerpentUtils.TryRemoveHediff(target, mark);

                //remove the soulweaver data from the source body
                SoulSerpentUtils.TryRemoveHediff(pawn, soulWeaver);

                //make the target a soulweaver
                SoulSerpentUtils.TryAddHediff(target, soulWeaver);

                return target;
            }

            return null;
        }

        public Pawn TransferToBestTarget()
        {
            //TODO: make this more complex?
            var target = markedPawns.FirstOrDefault();

            return TransferToTarget(target);
        }

        private void TransferRelations(Pawn source, Pawn dest)
        {

        }

        private void UpdateChronoTime(Pawn source, Pawn dest)
        {

        }
    }
}
