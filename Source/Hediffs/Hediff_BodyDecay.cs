using RimWorld;
using Verse;

namespace SoulSerpent
{
    public class Hediff_BodyDecay : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }

        public override string Description
        {
            get
            {
                if (Severity >= 1.0f)
                {
                    return "Your physical form has been completely consumed by soul manipulation. This vessel has perished, unable to contain the overwhelming soul energy.";
                }
                else if (Severity >= 0.95f)
                {
                    return "Your body has reached the final stage of decay from soul manipulation. Your physical form is barely holding together, and permanent death seems imminent unless you possess a new vessel.";
                }
                else if (Severity >= 0.9f)
                {
                    return "Your body is critically deteriorating from soul manipulation. Basic bodily functions are failing, and you struggle with even simple tasks. The soul energy is overwhelming your physical form.";
                }
                else if (Severity >= 0.7f)
                {
                    return "Your body is significantly deteriorating from the soul manipulation. Coordination is impaired and mental clarity is reduced as your physical form struggles to contain the soul energy.";
                }
                else if (Severity >= 0.3f)
                {
                    return "The strain of soul manipulation is taking a visible toll. You experience persistent discomfort and a growing sense of physical deterioration.";
                }
                else
                {
                    return "Your body shows the first signs of deterioration from soul manipulation.";
                }
            }
        }

        public override void Tick()
        {
            if (Severity >= 1)
            {
                //try and transfer the soulweaver to a marked pawn
                var soulWeaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);
                soulWeaver?.TransferToBestTarget();

                //kill the orginal host body into a cloud of ash
                var pawnLabel = pawn.LabelIndefinite();
                pawn.Kill(null);
                var corpse = pawn.Corpse;
                FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_Ash, pawnLabel, 5);
                corpse.Destroy(DestroyMode.Vanish);
            }

            base.Tick();
        }
    }
} 