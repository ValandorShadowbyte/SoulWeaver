using Verse;

namespace SoulSerpent
{
    public class Hediff_SoulMark : HediffWithComps
    {
        public Pawn master;
        public bool pawnKilled;

        public override string Label => $"{base.Label}: {master.LabelShort}";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref master, "master", false);
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();

            if (pawn.health.hediffSet.hediffs.Contains(this))
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            if (master != null)
            {
                Hediff_Soulweaver soulweaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(master, SoulSerpentDefs.VS_Soulweaver);

                if (soulweaver != null)
                {
                    soulweaver.markedPawns.Remove(pawn);
                }
            }
        }
    }
}
