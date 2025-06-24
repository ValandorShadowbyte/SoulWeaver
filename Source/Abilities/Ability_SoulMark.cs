using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace SoulSerpent
{
    public class Ability_SoulMark : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            
            foreach (GlobalTargetInfo target in targets)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    ApplySoulMark(targetPawn);
                }
            }
        }

        private void ApplySoulMark(Pawn targetPawn)
        {
            RestUtility.WakeUp(targetPawn, true);

            // Add soulweaver hediff to caster and link to target
            Hediff_Soulweaver soulWeaver = SoulSerpentUtils.TryAddHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);
            soulWeaver.markedPawns.Add(targetPawn);

            // Add soul mark to target
            Hediff_SoulMark soulMark = SoulSerpentUtils.TryAddHediff<Hediff_SoulMark>(targetPawn, SoulSerpentDefs.VS_SoulMark);
            soulMark.master = pawn;

            SoulSerpentUtils.TryAddHediff<Hediff_MarkResistance>(targetPawn, SoulSerpentDefs.VS_MarkResistance);
        }
    }
}
