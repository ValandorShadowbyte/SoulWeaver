using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VEF.Abilities.Ability;

namespace SoulSerpent
{
    public class Ability_SoulTransfer : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            
            foreach (GlobalTargetInfo target in targets)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    PerformSoulTransfer(targetPawn);
                }
            }
        }

        private void PerformSoulTransfer(Pawn targetPawn)
        {
            // Get the soulweaver hediff from the caster
            Hediff_Soulweaver soulWeaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);
            
            if (soulWeaver != null)
            {
                // Perform the soul transfer using the existing transfer logic
                Pawn transferredTo = soulWeaver.TransferToTarget(targetPawn);
                
                if (transferredTo != null)
                {
                    // Success message
                    Messages.Message("VS.SoulTransferSuccess".Translate(pawn.LabelShort, targetPawn.LabelShort), MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    // Failure message
                    Messages.Message("VS.SoulTransferFailed".Translate(targetPawn.LabelShort), MessageTypeDefOf.RejectInput);
                }
            }
        }
    }
} 