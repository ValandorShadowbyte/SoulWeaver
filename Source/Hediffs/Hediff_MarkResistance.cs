using RimWorld;
using Verse;

namespace SoulSerpent
{
    public class Hediff_MarkResistance : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            // Apply temporary resistance effects
            if (pawn != null)
            {
                SoulSerpentUtils.TryAddThought(pawn, SoulSerpentDefs.VS_SoulMarked);
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            
            // Remove the resistance thought
            if (pawn.needs?.mood?.thoughts?.memories != null)
            {
                var memories = pawn.needs.mood.thoughts.memories;
                var resistanceThought = memories.Memories.Find(m => m.def.defName == "VS_MarkResistance");
                
                if (resistanceThought != null)
                {
                    memories.Memories.Remove(resistanceThought);
                }
            }

            if (Severity > 0.1)
                return;

            if (pawn.guest != null && pawn.Faction != Faction.OfPlayer)
            {
                if (pawn.guest.Recruitable)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    pawn.guest.SetGuestStatus(null, 0);
                    pawn.guest.Released = true;
                }
                else
                {
                    pawn.guest.Recruitable = true;
                }
            }
            else if (pawn.IsPrisoner)
            {
                pawn.guest.SetGuestStatus(null, 0);
                pawn.guest.Released = true;
            }

            if (ModLister.IdeologyInstalled)
            {
                pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
        }
    }
}
