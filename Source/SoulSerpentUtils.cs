using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;

namespace SoulSerpent
{
    public static class SoulSerpentUtils
    {
        public static T TryAddHediff<T>(Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPart = null) where T : Hediff
        {
            if (!(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is T hediff))
            {
                hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart) as T;
                pawn.health.AddHediff(hediff);
                return hediff;
            }

            return hediff;
        }

        public static T TryGetHediff<T>(Pawn pawn, HediffDef hediffDef) where T : Hediff
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is T hediff)
            {
                return hediff;
            }

            return null;
        }

        public static void ApplyComa(Pawn pawn, float hours)
        {
            Hediff hediff = HediffMaker.MakeHediff(VPE_DefOf.PsychicComa, pawn);
            HediffUtility.TryGetComp<HediffComp_Disappears>(hediff).ticksToDisappear = Mathf.FloorToInt(hours * 2500f);
            pawn.health.AddHediff(hediff);
        }
    }
}
