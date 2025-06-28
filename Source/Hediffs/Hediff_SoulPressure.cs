using Verse;

namespace SoulSerpent
{
    public class Hediff_SoulPressure : HediffWithComps
    {
        public Pawn caster;

        public override string Label => $"{base.Label}: {caster?.LabelShort ?? "Unknown"}";

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            // Refresh the soul pressure thought
            SoulSerpentUtils.TryRefreshThought(pawn, SoulSerpentDefs.VS_SoulPressureThought, caster);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref caster, "caster", false);
        }
    }
} 