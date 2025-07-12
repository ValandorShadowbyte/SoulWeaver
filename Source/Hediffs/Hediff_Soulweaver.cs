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
        public bool? OriginalBody = null;

        public override void PostAdd(DamageInfo? dinfo)
        {
            if (OriginalBody is null)
            {
                OriginalBody = true;
            }

            base.PostAdd(dinfo);
            
            // Apply body decay when soulweaver hediff is added
            if (pawn != null && !pawn.Dead)
            {
                DeathNotificationDisabler.AddPawnToSkipList(pawn);
                SoulSerpentUtils.TryAddHediff<Hediff_BodyDecay>(pawn, SoulSerpentDefs.VS_BodyDecay);
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref MarkedPawns, "MarkedPawns", LookMode.Reference, Array.Empty<object>());
            Scribe_Values.Look(ref OriginalBody, "OriginalBody", true);
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            ResurrectOnDeath();
        }

        /// <summary>
        /// If the soulweaver has a marked pawn, they will be resurrected. The marked pawn will be downed for a while
        /// </summary>
        public void ResurrectOnDeath()
        {
            if (pawn.Dead)
            {
                var resurrectingFrom = BestMarkedPawn();

                // Only attempt resurrection if we have a valid target
                if (resurrectingFrom != null)
                {
                    SoulSerpentUtils.SoulSerpentResurrect(pawn, resurrectingFrom);

                    // Apply resurrection exhaustion to the marked pawn that was used for resurrection
                    if (!resurrectingFrom.Dead)
                    {
                        // Check if the target already has resurrection exhaustion - if so, kill them
                        var existingExhaustion = SoulSerpentUtils.TryGetHediff<Hediff_ResurrectionExhaustion>(resurrectingFrom, SoulSerpentDefs.VS_ResurrectionExhaustion);
                        if (existingExhaustion != null)
                        {
                            resurrectingFrom.Kill(null);
                            Messages.Message("VS.ResurrectionTargetKilled".Translate(resurrectingFrom.LabelShort), MessageTypeDefOf.CautionInput);
                        }
                        else
                        {
                            SoulSerpentUtils.TryAddHediff<Hediff_ResurrectionExhaustion>(resurrectingFrom, SoulSerpentDefs.VS_ResurrectionExhaustion);
                        }
                    }

                    Find.LetterStack.ReceiveLetter("VS.SoulweaverResurrectedTitle".Translate(), "VS.SoulweaverResurrectedLetter".Translate(pawn.LabelShort), LetterDefOf.PositiveEvent, pawn);
                }
                else
                {
                    SoulSerpentUtils.SoulweaverDeath(pawn);
                }
            }
        }

        /// <summary>
        /// A death transfer happens when the main body finally gives out, this will kill the pawn the soulweaver transfers to
        /// </summary>
        public void DeathTransferFromBodyDecay()
        {
            Pawn target = null;

            try
            {
                target = TransferToBestTarget();
            }
            catch (Exception e)
            {
                Log.Error($"[SoulSerpent] {e.Message}");
            }


            if (target == null) 
            {
                SoulSerpentUtils.SoulweaverDeath(pawn);
            }
        }

        /// <returns>The pawn that was transfered to, or null if we couldn't transfer</returns>
        public Pawn TransferToTarget(Pawn target)
        {
            if (target != null && MarkedPawns.Contains(target))
            {
                var soulWeaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);

                // Remove resurrection exhaustion from target if they have it
                var exhaustion = SoulSerpentUtils.TryGetHediff<Hediff_ResurrectionExhaustion>(target, SoulSerpentDefs.VS_ResurrectionExhaustion);
                if (exhaustion != null)
                {
                    SoulSerpentUtils.TryRemoveHediff(target, exhaustion);
                }

                // Remove mark resistance from target if they have it
                var markResistance = SoulSerpentUtils.TryGetHediff<Hediff_MarkResistance>(target, SoulSerpentDefs.VS_MarkResistance);
                if (markResistance != null)
                {
                    SoulSerpentUtils.TryRemoveHediff(target, markResistance);
                }

                SoulSerpentUtils.MovePsylink(pawn, target);
                SoulSerpentUtils.CopyNonPsycastAbilities(pawn, target);
                SoulSerpentUtils.CopyBackstory(pawn, target);
                SoulSerpentUtils.MergeBestTraitsFromDest(pawn, target);
                SoulSerpentUtils.CopySkills(pawn, target);
                SoulSerpentUtils.CopyBeliefs(pawn, target);
                UpdateChronoTime(pawn, target);
                SoulSerpentUtils.CopyWorkSettings(pawn, target);
                SoulSerpentUtils.CopyAllPolicies(pawn, target);
                BoundWeaponTransferUtility.TransferWeaponBindings(pawn, target);

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

                // Send letter notification about the transfer
                Find.LetterStack.ReceiveLetter(
                    "VS.SoulweaverTransferTitle".Translate(), 
                    "VS.SoulweaverTransferLetter".Translate(pawn.LabelShort, target.LabelShort), 
                    LetterDefOf.NeutralEvent, 
                    target
                );

                //copy nickname
                if (pawn.Name is NameTriple sourceNameTriple && target.Name is NameTriple targetNameTriple)
                {
                    target.Name = new NameTriple(
                        targetNameTriple.First,
                        sourceNameTriple.Nick,
                        targetNameTriple.Last
                    );
                }

                SoulSerpentUtils.CreateHusk(pawn);
                SoulSerpentUtils.DestroyPawnIntoGore(pawn);

                CleanupSelfMemories(pawn, target);

                return target;
            }

            return null;
        }

        public Pawn TransferToBestTarget()
        {
            return TransferToTarget(BestMarkedPawn());
        }

        public Pawn BestMarkedPawn()
        {
            // Prioritize pawns without resurrection exhaustion and without mark resistance
            var availablePawns = MarkedPawns.Where(p => p != null && !p.Dead && 
                SoulSerpentUtils.TryGetHediff<Hediff_MarkResistance>(p, SoulSerpentDefs.VS_MarkResistance) == null).ToList();
            
            if (availablePawns.Count > 0)
            {
                // First try to find non-exhausted pawns on the same map
                var nonExhaustedSameMapPawns = availablePawns.Where(p => 
                    SoulSerpentUtils.TryGetHediff<Hediff_ResurrectionExhaustion>(p, SoulSerpentDefs.VS_ResurrectionExhaustion) == null &&
                    p.Map == pawn.Map
                ).ToList();
            
                // If we have non-exhausted pawns on the same map, return the first one
                if (nonExhaustedSameMapPawns.Count > 0)
                    return nonExhaustedSameMapPawns.First();
            
                // Then try to find any non-exhausted pawns (regardless of map)
                var nonExhaustedPawns = availablePawns.Where(p => 
                    SoulSerpentUtils.TryGetHediff<Hediff_ResurrectionExhaustion>(p, SoulSerpentDefs.VS_ResurrectionExhaustion) == null
                ).ToList();
            
                // If we have non-exhausted pawns, return the first one
                if (nonExhaustedPawns.Count > 0)
                    return nonExhaustedPawns.First();
            
                // If all pawns have exhaustion, prioritize same map pawns
                var sameMapPawns = availablePawns.Where(p => p.Map == pawn.Map).ToList();
                if (sameMapPawns.Count > 0)
                    return sameMapPawns.First();
            }
            
            // Fall back to the first available pawn
            return availablePawns.Count > 0 ? availablePawns.First() : null;
        }

        private void UpdateChronoTime(Pawn source, Pawn dest)
        {
            if (OriginalBody.HasValue && !OriginalBody.Value)
            {
                dest.ageTracker.AgeChronologicalTicks = dest.ageTracker.AgeChronologicalTicks;
            }
            else
            {
                // first takeover. can just set to the original body age
                dest.ageTracker.AgeChronologicalTicks = source.ageTracker.AgeBiologicalTicks;
            }
        }

        /// <summary>
        /// the soulweaver shouldn't care that they "died" since they didn't
        /// </summary>
        private void CleanupSelfMemories(Pawn source, Pawn dest)
        {
            foreach (var thought in dest.needs.mood.thoughts.memories.Memories.ToList())
            {
                if (thought.otherPawn == source)
                {
                    dest.needs.mood.thoughts.memories.RemoveMemory(thought);
                }
            }
        }


    }
}
