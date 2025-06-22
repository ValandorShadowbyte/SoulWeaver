using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace SoulWeaver
{
    public class Ability_SoulMark : Ability
    {
        public override void Init()
        {
            base.Init();

            SoulWeaverUtils.TryAddHediff<Hediff_Soulweaver>(pawn, SoulWeaverDefs.SW_Soulweaver);
        }

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

            Hediff_Soulweaver soulWeaver = SoulWeaverUtils.TryAddHediff<Hediff_Soulweaver>(pawn, SoulWeaverDefs.SW_Soulweaver);
            soulWeaver.markedPawns.Add(targetPawn);
            
            Hediff_SoulMark soulMark = SoulWeaverUtils.TryAddHediff<Hediff_SoulMark>(targetPawn, SoulWeaverDefs.SW_SoulMark);
            soulMark.master = pawn;

            ApplyResistanceDebuffs(targetPawn);
        }

        private void ApplyResistanceDebuffs(Pawn targetPawn)
        {
            // Apply a temporary mood debuff to the target (resistance to the mark)
            if (targetPawn.needs?.mood?.thoughts?.memories != null)
            {
                Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("SW_SoulMarked"));
                targetPawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }

            SoulWeaverUtils.TryAddHediff<Hediff_MarkResistance>(targetPawn, SoulWeaverDefs.SW_MarkResistance);
        }
    }
}
