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
                    ApplyAwakenedSoulMark(targetPawn);
                }
            }
        }

        private void ApplyAwakenedSoulMark(Pawn targetPawn)
        {
            // Check if target already has an awakened soul mark
            if (SoulSerpentUtils.HasAwakenedSoulMark(targetPawn))
            {
                Messages.Message("Target already has an awakened soul mark.", MessageTypeDefOf.RejectInput);
                return;
            }

            // Remove basic soul mark if present and replace with awakened version
            Hediff existingSoulMark = SoulSerpentUtils.TryGetHediff<Hediff>(targetPawn, SoulSerpentDefs.VS_SoulMark);
            if (existingSoulMark != null)
            {
                targetPawn.health.RemoveHediff(existingSoulMark);
            }

            // Add or update soulweaver hediff to caster and link to target
            Hediff_Soulweaver soulWeaver = SoulSerpentUtils.TryAddHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);
            if (!soulWeaver.markedPawns.Contains(targetPawn))
            {
                soulWeaver.markedPawns.Add(targetPawn);
            }

            // Add awakened soul mark to target
            Hediff_AwakenedSoulMark awakenedMark = SoulSerpentUtils.TryAddHediff<Hediff_AwakenedSoulMark>(targetPawn, SoulSerpentDefs.VS_AwakenedSoulMark);
            awakenedMark.master = pawn;

            // Apply soul mark awakening effect (temporary consciousness reduction)
            SoulSerpentUtils.TryAddHediff<HediffWithComps>(targetPawn, SoulSerpentDefs.VS_SoulMarkAwakening);
        }
    }
} 