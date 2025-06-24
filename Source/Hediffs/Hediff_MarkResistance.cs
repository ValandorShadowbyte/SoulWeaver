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
                if (pawn.needs?.mood?.thoughts?.memories != null)
                {
                    Thought_Memory thought = (Thought_Memory) ThoughtMaker.MakeThought(ThoughtDef.Named("VS_SoulMarked"));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
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

            if ((double) Severity < 0.9)
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
        }
    }
}
