using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace SoulSerpent
{
    public class Ability_SoulPressure : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            
            foreach (GlobalTargetInfo target in targets)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    ApplySoulPressure(targetPawn);
                }
            }
        }

        private void ApplySoulPressure(Pawn targetPawn)
        {
            // Apply soul pressure effect to the marked target
            if (SoulSerpentUtils.HasSoulMark(targetPawn))
            {
                // Add soul pressure hediff to the target
                Hediff_SoulPressure soulPressure = SoulSerpentUtils.TryAddHediff<Hediff_SoulPressure>(targetPawn, SoulSerpentDefs.VS_SoulPressure);
                soulPressure.caster = pawn;
            }
        }
    }
} 