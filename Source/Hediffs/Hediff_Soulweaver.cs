using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SoulSerpent
{
    public class Hediff_Soulweaver : HediffWithComps
    {
        public List<Pawn> MarkedPawns = new List<Pawn>();
        public bool OriginalBody;

        public override void PostAdd(DamageInfo? dinfo)
        {
            OriginalBody = false;

            base.PostAdd(dinfo);
            
            // Apply body decay when soulweaver hediff is added
            if (pawn != null && !pawn.Dead)
            {
                SoulSerpentUtils.TryAddHediff<Hediff_BodyDecay>(pawn, SoulSerpentDefs.VS_BodyDecay);
                OriginalBody = true;
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref MarkedPawns, "MarkedPawns", LookMode.Reference, Array.Empty<object>());
            Scribe_Values.Look(ref OriginalBody, "OriginalBody", true);
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
        }

        public void DeathTransfer()
        {
            try
            {
                TransferToBestTarget();
            }
            catch (Exception e)
            {
                Log.Error($"[SoulSerpent] {e.Message}");
            }

            //kill the orginal host body into a cloud of ash
            var pawnLabel = pawn.LabelIndefinite();
            pawn.Kill(null);
            var corpse = pawn.Corpse;
            FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_Ash, pawnLabel, 5);
            corpse.Destroy(DestroyMode.Vanish);
        }

        public Pawn TransferToTarget(Pawn target)
        {
            if (target != null && MarkedPawns.Contains(target))
            {
                var soulWeaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);

                SoulSerpentUtils.MovePsylink(pawn, target);
                SoulSerpentUtils.CopyBackstory(pawn, target);
                SoulSerpentUtils.MergeBestTraitsFromDest(pawn, target);
                SoulSerpentUtils.CopySkills(pawn, target);
                SoulSerpentUtils.TransferRelations(pawn, target);
                UpdateChronoTime(pawn, target);

                //get the mark
                Hediff mark = SoulSerpentUtils.TryGetHediff<Hediff_SoulMark>(target, SoulSerpentDefs.VS_SoulMark) ??
                    SoulSerpentUtils.TryGetHediff<Hediff_AwakenedSoulMark>(target, SoulSerpentDefs.VS_AwakenedSoulMark);

                //remove the mark
                SoulSerpentUtils.TryRemoveHediff(target, mark);

                //remove the soulweaver data from the source body
                SoulSerpentUtils.TryRemoveHediff(pawn, soulWeaver);

                //make the target a soulweaver
                soulWeaver = SoulSerpentUtils.TryAddHediff(target, soulWeaver);
                soulWeaver.OriginalBody = false;

                SoulSerpentUtils.NotifyUpdates(target);

                return target;
            }

            return null;
        }

        public Pawn TransferToBestTarget()
        {
            //TODO: make this more complex?
            var target = MarkedPawns.FirstOrDefault();

            return TransferToTarget(target);
        }

        private void UpdateChronoTime(Pawn source, Pawn dest)
        {
            if (!OriginalBody)
            {
                dest.ageTracker.AgeChronologicalTicks = dest.ageTracker.AgeChronologicalTicks;
            }
            else
            {
                // first takeover. can just set to the original body age
                dest.ageTracker.AgeChronologicalTicks = source.ageTracker.AgeBiologicalTicks;
            }
        }
    }
}
