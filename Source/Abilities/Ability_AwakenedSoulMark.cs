using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace SoulSerpent
{
    public class Ability_AwakenedSoulMark : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            
            foreach (GlobalTargetInfo target in targets)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    // Apply awakened soul mark effect
                    ApplyAwakenedSoulMark(targetPawn);
                }
            }
        }

        private void ApplyAwakenedSoulMark(Pawn targetPawn)
        {
            // Check if target already has a basic soul mark
            Hediff existingMark = targetPawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VS_SoulMark"));
            
            if (existingMark != null)
            {
                // Remove the basic mark first
                targetPawn.health.RemoveHediff(existingMark);
            }
            
            // Apply awakened soul mark hediff
            Hediff awakenedMark = HediffMaker.MakeHediff(HediffDef.Named("VS_AwakenedSoulMark"), targetPawn);
            targetPawn.health.AddHediff(awakenedMark);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Pawn != null)
            {
                // Check if target is already awakened marked
                Hediff awakenedMark = target.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("VS_AwakenedSoulMark"));
                if (awakenedMark != null)
                {
                    if (throwMessages)
                    {
                        Messages.Message("Target already has an awakened soul mark.", MessageTypeDefOf.CautionInput);
                    }
                    return false;
                }
            }

            return base.ValidateTarget(target, throwMessages);
        }
    }
} 