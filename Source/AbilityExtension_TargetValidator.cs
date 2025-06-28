using RimWorld;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace SoulSerpent
{
    public class AbilityExtension_TargetValidator : AbilityExtension_AbilityMod
    {
        public bool IsDown;
        public bool NotDown;
        public bool IsMarked;
        public bool IsAdvancedMarked;
        public bool NotResistingSoulMark;
        public bool NotMarked;
        public bool NotAwakenedMarked;
        public bool NotSoulMarkAwakening;

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

                if (NotDown && target.Pawn.Downed)
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustNotBeDown".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (IsMarked && !SoulSerpentUtils.HasSoulMark(target.Pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustBeMarked".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (IsAdvancedMarked && !SoulSerpentUtils.HasAwakenedSoulMark(target.Pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustBeAdvancedMarked".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (NotResistingSoulMark && SoulSerpentUtils.IsResistingSoulMark(target.Pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustNotBeResisting".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (NotMarked && SoulSerpentUtils.HasSoulMark(target.Pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetMustNotBeMarked".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (NotAwakenedMarked && SoulSerpentUtils.HasAwakenedSoulMark(target.Pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetAlreadyAwakenedMarked".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }

                if (NotSoulMarkAwakening && SoulSerpentUtils.HasSoulMarkAwakening(target.Pawn))
                {
                    if (throwMessages)
                    {
                        Messages.Message("VS.TargetAlreadyAwakening".Translate(), MessageTypeDefOf.CautionInput);
                    }

                    return false;
                }
            }

            return base.ValidateTarget(target, ability, throwMessages);
        }

    }
}
