using Verse;

namespace SoulSerpent
{
    public class Hediff_SoulMark : HediffWithComps
    {
        public Pawn Master;

        public override string Label => $"{base.Label}: {Master.LabelShort}";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref Master, "master", false);
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

            if (Master != null)
            {
                Hediff_Soulweaver soulweaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(Master, SoulSerpentDefs.VS_Soulweaver);

                if (soulweaver != null)
                {
                    soulweaver.MarkedPawns.Remove(pawn);
                }
            }

            SoulSerpentUtils.TryRemoveThought(pawn, SoulSerpentDefs.VS_SoulMarked);
        }
    }
}
