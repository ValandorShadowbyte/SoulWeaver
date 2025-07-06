using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SoulSerpent
{
    public static class DeathNotificationDisabler
    {
        /// <summary>
        /// Add a pawn to the skip list for death notifications
        /// </summary>
        public static void AddPawnToSkipList(Pawn pawn)
        {
            if (pawn != null)
            {
                DeathNotificationDisablerComponent.Instance.AddPawnToSkipList(pawn);
            }
        }

        /// <summary>
        /// Remove a pawn from the skip list for death notifications
        /// </summary>
        public static void RemovePawnFromSkipList(Pawn pawn)
        {
            if (pawn != null)
            {
                DeathNotificationDisablerComponent.Instance.RemovePawnFromSkipList(pawn);
            }
        }

        /// <summary>
        /// Check if a pawn is in the skip list
        /// </summary>
        public static bool IsPawnInSkipList(Pawn pawn)
        {
            return pawn != null && DeathNotificationDisablerComponent.Instance.IsPawnInSkipList(pawn);
        }

        /// <summary>
        /// Clear all pawns from the skip list
        /// </summary>
        public static void ClearSkipList()
        {
            DeathNotificationDisablerComponent.Instance.ClearSkipList();
        }

        /// <summary>
        /// Get the current number of pawns in the skip list
        /// </summary>
        public static int GetSkipListCount()
        {
            return DeathNotificationDisablerComponent.Instance.GetSkipListCount();
        }
    }

    /// <summary>
    /// Game component that handles saving/loading the death notification skip list
    /// </summary>
    public class DeathNotificationDisablerComponent : GameComponent
    {
        private static DeathNotificationDisablerComponent instance;
        public static DeathNotificationDisablerComponent Instance => instance;

        // List of pawn IDs that should skip death notifications
        private HashSet<int> pawnIdsToSkipDeathNotification = new HashSet<int>();

        public DeathNotificationDisablerComponent(Game game) : base()
        {
            instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            // Convert HashSet<int> to List<int> for saving
            List<int> pawnIdsList = new List<int>();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                pawnIdsList.AddRange(pawnIdsToSkipDeathNotification);
            }

            Scribe_Collections.Look(ref pawnIdsList, "pawnIdsToSkipDeathNotification", LookMode.Value);

            // Convert back to HashSet when loading
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                pawnIdsToSkipDeathNotification.Clear();
                if (pawnIdsList != null)
                {
                    foreach (int pawnId in pawnIdsList)
                    {
                        pawnIdsToSkipDeathNotification.Add(pawnId);
                    }
                }
            }
        }

        /// <summary>
        /// Add a pawn to the skip list for death notifications
        /// </summary>
        public void AddPawnToSkipList(Pawn pawn)
        {
            if (pawn != null && pawn.thingIDNumber != -1)
            {
                pawnIdsToSkipDeathNotification.Add(pawn.thingIDNumber);
                Log.Message($"DeathNotificationDisabler: Added {pawn.LabelShortCap} (ID: {pawn.thingIDNumber}) to death notification skip list");
            }
        }

        /// <summary>
        /// Remove a pawn from the skip list for death notifications
        /// </summary>
        public void RemovePawnFromSkipList(Pawn pawn)
        {
            if (pawn != null && pawn.thingIDNumber != -1)
            {
                if (pawnIdsToSkipDeathNotification.Remove(pawn.thingIDNumber))
                {
                    Log.Message($"DeathNotificationDisabler: Removed {pawn.LabelShortCap} (ID: {pawn.thingIDNumber}) from death notification skip list");
                }
            }
        }

        /// <summary>
        /// Check if a pawn is in the skip list
        /// </summary>
        public bool IsPawnInSkipList(Pawn pawn)
        {
            return pawn != null && pawn.thingIDNumber != -1 && pawnIdsToSkipDeathNotification.Contains(pawn.thingIDNumber);
        }

        /// <summary>
        /// Clear all pawns from the skip list
        /// </summary>
        public void ClearSkipList()
        {
            int count = pawnIdsToSkipDeathNotification.Count;
            pawnIdsToSkipDeathNotification.Clear();
            Log.Message($"DeathNotificationDisabler: Cleared {count} pawns from death notification skip list");
        }

        /// <summary>
        /// Get the current number of pawns in the skip list
        /// </summary>
        public int GetSkipListCount()
        {
            return pawnIdsToSkipDeathNotification.Count;
        }

        /// <summary>
        /// Clean up dead pawns from the skip list
        /// </summary>
        public override void GameComponentTick()
        {
            base.GameComponentTick();

            // Clean up dead pawns every 60000 ticks (about once per day)
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                CleanupDeadPawns();
            }
        }

        private void CleanupDeadPawns()
        {
            List<int> pawnIdsToRemove = new List<int>();

            foreach (int pawnId in pawnIdsToSkipDeathNotification)
            {
                Pawn pawn = Find.WorldPawns.AllPawnsAlive.FirstOrDefault(p => p.thingIDNumber == pawnId)
                         ?? Find.WorldPawns.AllPawnsDead.FirstOrDefault(p => p.thingIDNumber == pawnId);
                if (pawn == null || pawn.Dead)
                {
                    pawnIdsToRemove.Add(pawnId);
                }
            }

            foreach (int pawnId in pawnIdsToRemove)
            {
                pawnIdsToSkipDeathNotification.Remove(pawnId);
            }

            if (pawnIdsToRemove.Count > 0)
            {
                Log.Message($"DeathNotificationDisabler: Cleaned up {pawnIdsToRemove.Count} dead pawns from skip list");
            }
        }
    }

    /// <summary>
    /// Harmony patch for Pawn_HealthTracker.NotifyPlayerOfKilled
    /// </summary>
    [HarmonyPatch(typeof(Pawn_HealthTracker), "NotifyPlayerOfKilled")]
    public static class Pawn_HealthTracker_NotifyPlayerOfKilled_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            // Check if the pawn should skip death notifications
            if (DeathNotificationDisabler.IsPawnInSkipList(___pawn))
            {
                Log.Message($"DeathNotificationDisabler: Skipping death notification for {___pawn.LabelShortCap}");
                return false; // Skip the original method
            }
            return true; // Continue with the original method
        }
    }

    /// <summary>
    /// Harmony patch for RitualObligationTrigger_MemberDied.Notify_MemberDied
    /// </summary>
    [HarmonyPatch(typeof(RitualObligationTrigger_MemberDied), "Notify_MemberDied")]
    public static class RitualObligationTrigger_MemberDied_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn p)
        {
            // Check if the pawn should skip funeral notifications
            if (DeathNotificationDisabler.IsPawnInSkipList(p))
            {
                Log.Message($"DeathNotificationDisabler: Skipping funeral obligation for {p.LabelShortCap}");
                return false; // Skip the original method
            }
            return true; // Continue with the original method
        }
    }

    /// <summary>
    /// Harmony patch for RitualObligationTrigger_MemberCorpseDestroyed.Notify_MemberCorpseDestroyed
    /// </summary>
    [HarmonyPatch(typeof(RitualObligationTrigger_MemberCorpseDestroyed), "Notify_MemberCorpseDestroyed")]
    public static class RitualObligationTrigger_MemberCorpseDestroyed_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn p)
        {
            // Check if the pawn should skip funeral notifications
            if (DeathNotificationDisabler.IsPawnInSkipList(p))
            {
                Log.Message($"DeathNotificationDisabler: Skipping corpse destroyed funeral obligation for {p.LabelShortCap}");
                return false; // Skip the original method
            }
            return true; // Continue with the original method
        }
    }

    /// <summary>
    /// Harmony patch for PawnDiedOrDownedThoughtsUtility.TryGiveThoughts
    /// </summary>
    [HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "TryGiveThoughts",
        new Type[] { typeof(Pawn), typeof(DamageInfo?), typeof(PawnDiedOrDownedThoughtsKind) })]
    public static class PawnDiedOrDownedThoughtsUtility_TryGiveThoughts_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind)
        {
            // Check if the pawn should skip mood thoughts
            if (DeathNotificationDisabler.IsPawnInSkipList(victim))
            {
                Log.Message($"DeathNotificationDisabler: Skipping mood thoughts for {victim.LabelShortCap}");
                return false; // Skip the original method
            }
            return true; // Continue with the original method
        }
    }
}