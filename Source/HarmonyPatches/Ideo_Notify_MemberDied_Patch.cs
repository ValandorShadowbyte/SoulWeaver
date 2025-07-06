using HarmonyLib;
using RimWorld;
using Verse;

namespace SoulSerpent.HarmonyPatches
{
    [HarmonyPatch(typeof(Ideo), "Notify_MemberDied")]
    public class Ideo_Notify_MemberDied_Patch
    {
        public static bool Prefix(Pawn member)
        {
            if (PatchControls.SkipFuneral != member)
            {
                return true;
            }

            return false;
        }
    }
}
