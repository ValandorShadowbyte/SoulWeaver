using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using System.Linq;

namespace SoulSerpent
{
    public static class SoulSerpentUtils
    {
        public static T TryAddHediff<T>(Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPart = null) where T : Hediff
        {
            if (!(pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is T hediff))
            {
                hediff = HediffMaker.MakeHediff(hediffDef, pawn, bodyPart) as T;
                pawn.health.AddHediff(hediff);
                return hediff;
            }

            return hediff;
        }

        public static T TryGetHediff<T>(Pawn pawn, HediffDef hediffDef) where T : Hediff
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is T hediff)
            {
                return hediff;
            }

            return null;
        }

        /// <summary>
        /// Adds a thought memory to a pawn
        /// </summary>
        /// <param name="pawn">The pawn to add the thought to</param>
        /// <param name="thoughtDef">The thought definition to add</param>
        /// <param name="otherPawn">Optional other pawn for the thought (e.g., the caster)</param>
        public static void TryAddThought(Pawn pawn, RimWorld.ThoughtDef thoughtDef, Pawn otherPawn = null)
        {
            if (pawn?.needs?.mood?.thoughts?.memories != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, otherPawn);
            }
        }

        /// <summary>
        /// Removes a thought memory from a pawn
        /// </summary>
        /// <param name="pawn">The pawn to remove the thought from</param>
        /// <param name="thoughtDef">The thought definition to remove</param>
        public static void TryRemoveThought(Pawn pawn, RimWorld.ThoughtDef thoughtDef)
        {
            if (pawn?.needs?.mood?.thoughts?.memories != null)
            {
                var memories = pawn.needs.mood.thoughts.memories;
                var thoughtToRemove = memories.Memories.FirstOrDefault(m => m.def == thoughtDef);
                if (thoughtToRemove != null)
                {
                    memories.RemoveMemory(thoughtToRemove);
                }
            }
        }

        /// <summary>
        /// Refreshes a thought memory by removing and re-adding it
        /// </summary>
        /// <param name="pawn">The pawn to refresh the thought for</param>
        /// <param name="thoughtDef">The thought definition to refresh</param>
        /// <param name="otherPawn">Optional other pawn for the thought (e.g., the caster)</param>
        public static void TryRefreshThought(Pawn pawn, RimWorld.ThoughtDef thoughtDef, Pawn otherPawn = null)
        {
            if (pawn?.needs?.mood?.thoughts?.memories != null)
            {
                TryRemoveThought(pawn, thoughtDef);
                TryAddThought(pawn, thoughtDef, otherPawn);
            }
        }

        /// <summary>
        /// Checks if a pawn has a basic soul mark
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>True if the pawn has a basic soul mark or awakened soul mark</returns>
        public static bool HasSoulMark(Pawn pawn)
        {
            return TryGetHediff<Hediff>(pawn, SoulSerpentDefs.VS_SoulMark) != null ||
                   TryGetHediff<Hediff>(pawn, SoulSerpentDefs.VS_AwakenedSoulMark) != null;
        }

        /// <summary>
        /// Checks if a pawn has an awakened (advanced) soul mark
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>True if the pawn has an awakened soul mark</returns>
        public static bool HasAwakenedSoulMark(Pawn pawn)
        {
            return TryGetHediff<Hediff>(pawn, SoulSerpentDefs.VS_AwakenedSoulMark) != null;
        }

        /// <summary>
        /// Checks if a pawn is actively resisting their soul mark
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>True if the pawn has mark resistance</returns>
        public static bool IsResistingSoulMark(Pawn pawn)
        {
            return TryGetHediff<Hediff_MarkResistance>(pawn, SoulSerpentDefs.VS_MarkResistance) != null;
        }

        /// <summary>
        /// Checks if a pawn is currently experiencing soul mark awakening
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>True if the pawn has soul mark awakening</returns>
        public static bool HasSoulMarkAwakening(Pawn pawn)
        {
            return TryGetHediff<Hediff>(pawn, SoulSerpentDefs.VS_SoulMarkAwakening) != null;
        }
    }
}
