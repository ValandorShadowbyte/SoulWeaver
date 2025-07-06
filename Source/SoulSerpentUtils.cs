using System.Linq;
using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
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

        public static void MovePsylink(Pawn source, Pawn dest, bool notifyUpdates = false)
        {
            var sourcePsylink = PawnUtility.GetMainPsylinkSource(source);
            var sourceAbilities = PsycastUtility.Psycasts(source);

            if (sourcePsylink != null)
            {
                source.health.RemoveHediff(sourcePsylink);
                dest.health.hediffSet.hediffs.Add(sourcePsylink);
            }
            
            if (sourceAbilities != null)
            {
                source.health.RemoveHediff(sourceAbilities);
                dest.health.hediffSet.hediffs.Add(sourceAbilities);
            }
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

            if (notifyUpdates)
            {
                NotifyUpdates(dest);
            }
        }

        public static void MergeBestTraitsFromDest(Pawn source, Pawn dest, bool notifyUpdates = false)
        {
            //TODO: do I want to keep good traits of the dest??

            foreach (var trait in dest.story.traits.allTraits.ToList())
            {
                dest.story.traits.RemoveTrait(trait);
            }

            //give dest source traits
            foreach (var trait in source.story.traits.allTraits)
            {
                dest.story.traits.GainTrait(trait);
            }

            if (notifyUpdates)
            {
                NotifyUpdates(dest);
            }
        }

        public static void CopyBackstory(Pawn source, Pawn dest, bool notifyUpdates = false)
        {
            dest.story.Childhood = source.story.Childhood;
            dest.story.Adulthood = source.story.Adulthood;

            if (notifyUpdates)
            {
                NotifyUpdates(dest);
            }
        }

        public static void CopySkills(Pawn source, Pawn dest, bool notifyUpdates = false)
        {
            foreach (var skill in source.skills.skills) {
                var skillToUpdate = dest.skills.GetSkill(skill.def);

                if (skillToUpdate != null) {
                    skillToUpdate.xpSinceLastLevel = skill.xpSinceLastLevel;
                    skillToUpdate.xpSinceMidnight = skill.xpSinceMidnight;
                    skillToUpdate.Level = skill.Level;
                    skillToUpdate.passion = skill.passion;
                }
            }

            if (notifyUpdates)
            {
                NotifyUpdates(dest);
            }
        }

        public static void CopyBeliefs(Pawn source, Pawn dest, bool notifyUpdates = false)
        {
            if (source.Ideo != null && source.Ideo != dest.Ideo) {
                dest.ideo.SetIdeo(source.Ideo);
            }

            if (source.Faction != null && source.Faction != dest.Faction) {
                dest.SetFaction(source.Faction, source);
            }

            if (notifyUpdates)
            {
                NotifyUpdates(dest);
            }
        }

        public static void NotifyUpdates(Pawn pawn)
        {
            pawn.Notify_DisabledWorkTypesChanged();
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, false);
        }

        /// <summary>
        /// Heals a pawn by restoring health, removing harmful hediffs, and restoring consciousness
        /// </summary>
        /// <param name="pawn">The pawn to heal</param>
        public static void HealPawn(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
                return;

            try
            {
                // Restore all body parts to full health
                foreach (var bodyPart in pawn.health.hediffSet.GetNotMissingParts())
                {
                    var partHealth = pawn.health.hediffSet.GetPartHealth(bodyPart);
                    var maxHealth = bodyPart.def.GetMaxHealth(pawn);
                    
                    if (partHealth < maxHealth)
                    {
                        pawn.health.RestorePart(bodyPart);
                    }
                }

                // Remove harmful hediffs (but preserve beneficial ones and mod-specific hediffs)
                var hediffsToRemove = pawn.health.hediffSet.hediffs
                    .Where(h => h.def.isBad && 
                               !h.def.defName.StartsWith("VS_")) // Preserve mod-specific hediffs
                    .ToList();

                foreach (var hediff in hediffsToRemove)
                {
                    TryRemoveHediff(pawn, hediff);
                }

                // Restore needs if they exist
                if (pawn.needs?.food != null)
                {
                    pawn.needs.food.CurLevel = pawn.needs.food.MaxLevel;
                }
                
                if (pawn.needs?.rest != null)
                {
                    pawn.needs.rest.CurLevel = pawn.needs.rest.MaxLevel;
                }

                // Notify the game that the pawn has been updated
                NotifyUpdates(pawn);
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[SoulSerpent] Error healing pawn {pawn?.Name?.ToStringFull ?? "Unknown"}: {ex.Message}");
            }
        }

        public static void SoulSerpentResurrect(Pawn pawn, Pawn resurrectingFrom)
        {
            // Play the resurrection sound
            SoulSerpentDefs.VS_SoulweaverResurrection.PlayOneShot(SoundInfo.InMap((TargetInfo)pawn));

            ResurrectionUtility.TryResurrect(pawn, new ResurrectionParams
            {
                gettingScarsChance = 0f,
                canKidnap = false,
                canTimeoutOrFlee = false,
                canSteal = false,
                useAvoidGridSmart = true
            });

            HealPawn(pawn);
            CreateHusk(pawn);

            // Only skip to the resurrecting pawn if they exist and are valid
            if (resurrectingFrom != null && resurrectingFrom.Map != null)
            {
                SkipUtility.SkipTo(pawn, resurrectingFrom.Position, resurrectingFrom.Map);
            }
        }

        public static void CreateHusk(Pawn pawn)
        {
            // Store original position
            IntVec3 originalPosition = pawn.Position;
            Map originalMap = pawn.Map;

            // Create a PawnGenerationRequest with minimal content
            var request = new PawnGenerationRequest(
                kind: pawn.kindDef,
                faction: null,
                context: PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f,
                forceAddFreeWarmLayerIfNeeded: false,
                allowGay: true,
                allowPregnant: false,
                allowFood: false,
                allowAddictions: false,
                inhabitant: false,
                certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false,
                worldPawnFactionDoesntMatter: false,
                biocodeWeaponChance: 0f,
                biocodeApparelChance: 0f,
                extraPawnForExtraRelationChance: null,
                relationWithExtraPawnChanceFactor: 0f,
                validatorPreGear: null,
                validatorPostGear: null,
                forcedTraits: null,
                prohibitedTraits: null,
                minChanceToRedressWorldPawn: null,
                fixedBiologicalAge: pawn.ageTracker.AgeBiologicalYears,
                fixedChronologicalAge: pawn.ageTracker.AgeChronologicalYears,
                fixedGender: pawn.gender,
                fixedLastName: null,
                fixedBirthName: null,
                fixedTitle: null,
                fixedIdeo: null,
                forceNoIdeo: false,
                forceNoBackstory: false,
                forbidAnyTitle: true,
                forceDead: false,
                forcedXenogenes: null,
                forcedEndogenes: null,
                forcedXenotype: pawn.genes?.Xenotype,
                forcedCustomXenotype: null,
                allowedXenotypes: null,
                forceBaselinerChance: 0f,
                developmentalStages: DevelopmentalStage.Adult,
                pawnKindDefGetter: null,
                excludeBiologicalAgeRange: null,
                biologicalAgeRange: null,
                forceRecruitable: true,
                minimumAgeTraits: 0,
                maximumAgeTraits: 0,
                forceNoGear: true
            );

            // Create the husk with the request
            Pawn husk = PawnGenerator.GeneratePawn(request);

            // Copy the appearance/body
            husk.Name = new NameSingle($"{pawn.Name} (Husk)");

            // Copy backstories
            husk.story.Childhood = pawn.story.Childhood;
            husk.story.Adulthood = pawn.story.Adulthood;

            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                // Copy xenotype
                husk.genes.SetXenotype(pawn.genes.Xenotype);
                husk.genes.xenotypeName = pawn.genes.xenotypeName;
                husk.genes.iconDef = pawn.genes.iconDef;
                husk.genes.hybrid = pawn.genes.hybrid;

                //Clear existing genes first
                foreach (Gene gene in husk.genes.GenesListForReading.ToList())
                {
                    if (gene != null)
                        husk.genes.RemoveGene(gene);
                }

                var melanineGene = pawn.genes.GetMelaninGene();
                var hairColorGene = pawn.genes.GetHairColorGene();

                if (melanineGene != null)
                {
                    husk.genes.AddGene(melanineGene, false);
                }

                if (hairColorGene != null)
                {
                    husk.genes.AddGene(hairColorGene, false);
                }
            }

            // Copy style elements (tattoos, etc.)
            if (ModsConfig.IdeologyActive)
            {
                husk.style.BodyTattoo = pawn.style.BodyTattoo;
                husk.style.FaceTattoo = pawn.style.FaceTattoo;
            }

            husk.story.skinColorOverride = pawn.story.skinColorOverride;
            husk.story.HairColor = pawn.story.HairColor;
            husk.story.hairDef = pawn.story.hairDef;
            husk.story.bodyType = pawn.story.bodyType;
            husk.story.headType = pawn.story.headType;
            husk.story.SkinColorBase = pawn.story.SkinColorBase;
            husk.style.beardDef = pawn.style.beardDef;

            // Remove ALL hediffs (in case any were generated)
            if (husk.health != null && husk.health.hediffSet != null)
            {
                var hediffsToRemove = husk.health.hediffSet.hediffs.ToList();
                foreach (Hediff hediff in hediffsToRemove)
                {
                    if (hediff != null && husk.health.hediffSet.hediffs.Contains(hediff))
                    {
                        husk.health.RemoveHediff(hediff);
                    }
                }
            }

            // Disable all work types by adding them to the disabled list
            husk.workSettings.EnableAndInitialize();
            foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (!husk.GetDisabledWorkTypes().Contains(workType))
                {
                    husk.workSettings.Disable(workType);
                }
            }

            // Clear all skills
            foreach (SkillRecord skill in husk.skills.skills)
            {
                skill.levelInt = 0;
                skill.passion = Passion.None;
                skill.xpSinceLastLevel = 0f;
                skill.xpSinceMidnight = 0f;
            }

            // Kill the husk to create a corpse
            try
            {
                husk.Kill(null);
                
                // Spawn the corpse at original location
                if (husk.Corpse != null && originalMap != null)
                {
                    GenSpawn.Spawn(husk.Corpse, originalPosition, originalMap);
                }
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[SoulSerpent] Error killing husk {husk?.Name?.ToStringFull ?? "Unknown"}: {ex.Message}");
            }
        }

        public static void DestroyPawnIntoGore(Pawn pawn)
        {
            try
            {
                if (pawn != null && !pawn.Dead)
                {
                    pawn.Kill(null);
                }

                var corpse = pawn?.Corpse;
                if (corpse != null && corpse.Spawned && corpse.Map != null)
                {
                    FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_Ash, pawn.LabelIndefinite(), 5);
                    FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_Slime, pawn.LabelIndefinite(), 5);
                    FilthMaker.TryMakeFilth(corpse.Position, corpse.Map, ThingDefOf.Filth_TwistedFlesh, pawn.LabelIndefinite(), 5);

                    corpse.Destroy(DestroyMode.Vanish);
                }
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[SoulSerpent] Error destroying pawn {pawn?.Name?.ToStringFull ?? "Unknown"} into gore: {ex.Message}");
            }
        }

        public static void SoulweaverDeath(Pawn pawn)
        {
            // Clean up all soul marks and related hediffs from marked pawns
            var soulweaverHediff = TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);
            if (soulweaverHediff != null && soulweaverHediff.MarkedPawns != null)
            {
                var markedPawnsToProcess = soulweaverHediff.MarkedPawns.ToList();
                foreach (var markedPawn in markedPawnsToProcess)
                {
                    if (markedPawn != null && !markedPawn.Dead && markedPawn.health?.hediffSet != null)
                    {
                        try
                        {
                            // Remove basic soul mark
                            var basicMark = TryGetHediff<Hediff_SoulMark>(markedPawn, SoulSerpentDefs.VS_SoulMark);
                            if (basicMark != null)
                            {
                                TryRemoveHediff(markedPawn, basicMark);
                            }

                            // Remove awakened soul mark
                            var awakenedMark = TryGetHediff<Hediff_AwakenedSoulMark>(markedPawn, SoulSerpentDefs.VS_AwakenedSoulMark);
                            if (awakenedMark != null)
                            {
                                TryRemoveHediff(markedPawn, awakenedMark);
                            }

                            // Notify updates for the marked pawn
                            NotifyUpdates(markedPawn);
                        }
                        catch (System.Exception ex)
                        {
                            Log.Warning($"[SoulSerpent] Error cleaning up marked pawn {markedPawn?.Name?.ToStringFull ?? "Unknown"}: {ex.Message}");
                        }
                    }
                }

                // Clear the marked pawns list
                soulweaverHediff.MarkedPawns.Clear();
            }

            SoulSerpentDefs.VS_SoulweaverDeath.PlayOneShot(SoundInfo.InMap((TargetInfo) pawn));

            Find.LetterStack.ReceiveLetter(
                "VS.SoulweaverPermanentDeathTitle".Translate(),
                "VS.SoulweaverPermanentDeathLetter".Translate(pawn.LabelShort),
                LetterDefOf.Death,
                pawn
            );

            StrikeLightningAtPawnOrCorpse(pawn);

            DestroyPawnIntoGore(pawn);

            DeathNotificationDisabler.RemovePawnFromSkipList(pawn);
        }

        public static void StrikeLightningAtPawnOrCorpse(Pawn pawn)
        {
            if (pawn == null)
                return;

            IntVec3? pos = null;
            Map map = null;

            if (pawn.Spawned && pawn.Map != null && !pawn.Dead)
            {
                pos = pawn.Position;
                map = pawn.Map;
            }
            else if (pawn.Corpse != null && pawn.Corpse.Spawned && pawn.Corpse.Map != null)
            {
                pos = pawn.Corpse.Position;
                map = pawn.Corpse.Map;
            }

            if (pos != null && map != null)
            {
                map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(map, pos.Value));
            }
        }
    }
}
