using RimWorld;
using Verse;

namespace SoulWeaver
{
    public class Hediff_MarkResistance : HediffWithComps
    {
        public override void PostRemoved()
        {
            base.PostRemoved();

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
