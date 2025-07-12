using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SoulSerpent
{
    public static class BoundWeaponTransferUtility
    {
        /// <summary>
        /// Transfers weapon bindings from source pawn to target pawn without moving the weapons
        /// </summary>
        /// <param name="sourcePawn">The pawn to transfer bindings from</param>
        /// <param name="targetPawn">The pawn to transfer bindings to</param>
        public static void TransferWeaponBindings(Pawn sourcePawn, Pawn targetPawn)
        {
            if (sourcePawn?.equipment == null || targetPawn?.equipment == null)
            {
                Log.Warning("TransferWeaponBindings: Source or target pawn equipment tracker is null");
                return;
            }

            // Transfer bonded weapon reference
            if (sourcePawn.equipment.bondedWeapon != null)
            {
                // Cast to ThingWithComps for the transfer
                ThingWithComps bondedWeapon = sourcePawn.equipment.bondedWeapon as ThingWithComps;
                if (bondedWeapon != null)
                {
                    TransferWeaponBinding(bondedWeapon, sourcePawn, targetPawn);
                }
            }

            // Transfer any other bound weapons in equipment
            foreach (ThingWithComps weapon in sourcePawn.equipment.AllEquipmentListForReading)
            {
                if (IsBoundWeapon(weapon, sourcePawn))
                {
                    TransferWeaponBinding(weapon, sourcePawn, targetPawn);
                }
            }
        }

        /// <summary>
        /// Transfers the binding of a specific weapon from one pawn to another
        /// </summary>
        /// <param name="weapon">The weapon to rebind</param>
        /// <param name="oldPawn">The current owner</param>
        /// <param name="newPawn">The new owner</param>
        private static void TransferWeaponBinding(ThingWithComps weapon, Pawn oldPawn, Pawn newPawn)
        {
            if (weapon == null || oldPawn == null || newPawn == null)
                return;

            // Handle bladelink weapon binding
            CompBladelinkWeapon bladelinkComp = weapon.TryGetComp<CompBladelinkWeapon>();
            if (bladelinkComp != null && bladelinkComp.CodedPawn == oldPawn)
            {
                // Unbind from old pawn
                if (oldPawn.equipment.bondedWeapon == weapon)
                {
                    oldPawn.equipment.bondedWeapon = null;
                }

                // Uncode the weapon (removes binding)
                bladelinkComp.UnCode();

                // Rebind to new pawn
                bladelinkComp.CodeFor(newPawn);

                Log.Message($"Transferred bladelink binding for {weapon.Label} from {oldPawn.LabelShort} to {newPawn.LabelShort}");
            }

            // Handle biocoded weapons
            if (CompBiocodable.IsBiocoded(weapon) && CompBiocodable.IsBiocodedFor(weapon, oldPawn))
            {
                CompBiocodable biocodableComp = weapon.TryGetComp<CompBiocodable>();
                if (biocodableComp != null)
                {
                    // Uncode from old pawn
                    biocodableComp.UnCode();

                    // Recode for new pawn
                    biocodableComp.CodeFor(newPawn);

                    Log.Message($"Transferred biocode binding for {weapon.Label} from {oldPawn.LabelShort} to {newPawn.LabelShort}");
                }
            }
        }

        /// <summary>
        /// Checks if a weapon is bound to a specific pawn
        /// </summary>
        /// <param name="weapon">The weapon to check</param>
        /// <param name="pawn">The pawn to check binding against</param>
        /// <returns>True if the weapon is bound to the pawn</returns>
        public static bool IsBoundWeapon(ThingWithComps weapon, Pawn pawn)
        {
            if (weapon == null || pawn?.equipment == null)
                return false;

            // Check if it's a bladelink weapon bound to this pawn
            CompBladelinkWeapon bladelinkComp = weapon.TryGetComp<CompBladelinkWeapon>();
            if (bladelinkComp != null && bladelinkComp.CodedPawn == pawn)
                return true;

            // Check if it's the pawn's bonded weapon
            if (pawn.equipment.bondedWeapon == weapon)
                return true;

            // Check if it's biocoded for this pawn
            if (CompBiocodable.IsBiocodedFor(weapon, pawn))
                return true;

            return false;
        }

        /// <summary>
        /// Gets all weapons that are bound to a specific pawn
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <returns>List of bound weapons</returns>
        public static List<ThingWithComps> GetBoundWeapons(Pawn pawn)
        {
            List<ThingWithComps> boundWeapons = new List<ThingWithComps>();

            if (pawn?.equipment == null)
                return boundWeapons;

            foreach (ThingWithComps weapon in pawn.equipment.AllEquipmentListForReading)
            {
                if (IsBoundWeapon(weapon, pawn))
                {
                    boundWeapons.Add(weapon);
                }
            }

            return boundWeapons;
        }
    }
}
