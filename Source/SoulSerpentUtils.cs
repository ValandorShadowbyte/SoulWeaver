using System.Linq;
using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using PsycastUtility = VanillaPsycastsExpanded.PsycastUtility;


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

        public static T TryAddHediff<T>(Pawn pawn, T hediff) where T : Hediff
        {
            return TryAddHediff<T>(pawn, hediff.def);
        }

        public static T TryGetHediff<T>(Pawn pawn, HediffDef hediffDef) where T : Hediff
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is T hediff)
            {
                return hediff;
            }

            return null;
        }

        public static void TryRemoveHediff(Pawn pawn, Hediff hediff)
        {
            if (pawn != null && hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
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
            return TryGetHediff<Hediff>(pawn, SoulSerpentDefs.VS_MarkResistance) != null;
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

        public static void CopyPsylink(Pawn source, Pawn dest)
        {
            var sourcePsylink = PawnUtility.GetMainPsylinkSource(source);
            var sourceAbilities = PsycastUtility.Psycasts(source);

            dest.health.hediffSet.hediffs.Add(sourcePsylink);
            dest.health.hediffSet.hediffs.Add(sourceAbilities);
            dest.psychicEntropy.OffsetPsyfocusDirectly(source.psychicEntropy.CurrentPsyfocus);
            dest.psychicEntropy.TryAddEntropy(source.psychicEntropy.EntropyValue);
            dest.psychicEntropy.SetPsyfocusTarget(source.psychicEntropy.TargetPsyfocus);
            dest.psychicEntropy.limitEntropyAmount = source.psychicEntropy.limitEntropyAmount;

            foreach (var ability in source.GetComp<CompAbilities>().LearnedAbilities.ToList())
            {
                if (ability.def.GetModExtension<AbilityExtension_Psycast>() != null)
                {
                    dest.GetComp<CompAbilities>().GiveAbility(ability.def);
                }
            }

            PawnComponentsUtility.AddAndRemoveDynamicComponents(dest, false);
        }

        public static void MergeBestTraitsFromDest(Pawn source, Pawn dest)
        {

        }

        public static void CopyBackstory(Pawn source, Pawn dest)
        {

        }

        public static void CopySkills(Pawn source, Pawn dest)
        {

        }
    }
}
