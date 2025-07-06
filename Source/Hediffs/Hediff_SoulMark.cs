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

            // Remove mark resistance
            var markResistance = SoulSerpentUtils.TryGetHediff<Hediff_MarkResistance>(pawn, SoulSerpentDefs.VS_MarkResistance);
            if (markResistance != null)
            {
                SoulSerpentUtils.TryRemoveHediff(pawn, markResistance);
            }

            // Remove resurrection exhaustion
            var resurrectionExhaustion = SoulSerpentUtils.TryGetHediff<Hediff_ResurrectionExhaustion>(pawn, SoulSerpentDefs.VS_ResurrectionExhaustion);
            if (resurrectionExhaustion != null)
            {
                SoulSerpentUtils.TryRemoveHediff(pawn, resurrectionExhaustion);
            }
        }
    }
}
