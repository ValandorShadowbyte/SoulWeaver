using HarmonyLib;
using RimWorld;
using Verse;

namespace SoulSerpent.HarmonyPatches
{
    [HarmonyPatch(typeof(RitualObligationTrigger_MemberCorpseDestroyed), "Notify_MemberCorpseDestroyed")]
    public static class RitualObligationTrigger_MemberCorpseDestroyed_Patch
    {
        public static bool Prefix(Pawn p)
        {
            return PatchControls.SkipFuneral != p;
        }
    }
}
