using RimWorld;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace SoulSerpent
{
    public class AbilityExtension_TargetValidator : AbilityExtension_AbilityMod
    {
        public bool IsDown;
        public bool IsMarked;
        public bool IsAdvancedMarked;

        public HediffDef requiredHediffOnTarget;

        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            //only ever want to target pawns with this
            if (target.Pawn is null)
            {
                return false;
            }

            else if (target.HasThing)
            {
                if (IsDown && !target.Pawn.Downed)
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustBeDown".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (IsMarked)
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustBeMarked".Translate(), MessageTypeDefOf.CautionInput);
                    }
                }

                if (IsAdvancedMarked)
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustBeAdvancedMarked".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }
            }

            return base.ValidateTarget(target, ability, throwMessages);
        }

    }
}
