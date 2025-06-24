using Verse;

namespace SoulSerpent
{
    public class HediffCompProperties_CauseSoulweaverMentalState : HediffCompProperties_CauseMentalState
    {
        public float maxSeverity;

        public HediffCompProperties_CauseSoulweaverMentalState()
        {
            this.compClass = typeof(HediffComp_CauseSoulweaverMentalState);
        }
    }

    public class HediffComp_CauseSoulweaverMentalState : HediffComp_CauseMentalState
    {
        public new HediffCompProperties_CauseSoulweaverMentalState Props
        {
            get => (HediffCompProperties_CauseSoulweaverMentalState) props;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (((double) parent.Severity < (double) Props.minSeverity) || ((double) parent.Severity > (double) Props.maxSeverity))
                return;

            base.CompPostTick(ref severityAdjustment);
        }
    }
}
