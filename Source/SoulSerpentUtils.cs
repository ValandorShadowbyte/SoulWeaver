using System.Linq;
using RimWorld;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
using VEF.Abilities;
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
            if (pawn != null && hediff != null && pawn.health != null && pawn.health.hediffSet != null)
            {
                try
                {
                    // Check if the hediff is actually in the pawn's hediff set before trying to remove it
                    if (pawn.health.hediffSet.hediffs.Contains(hediff))
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warning($"[SoulSerpent] Error removing hediff {hediff?.def?.defName ?? "Unknown"} from {pawn?.Name?.ToStringFull ?? "Unknown"}: {ex.Message}");
                }
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
        /// Checks if a pawn has a basic soul mark from a specific caster
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <param name="caster">The caster to check against</param>
        /// <returns>True if the pawn has a basic soul mark from the specified caster</returns>
        public static bool HasOwnSoulMark(Pawn pawn, Pawn caster)
        {
            var soulMark = TryGetHediff<Hediff_SoulMark>(pawn, SoulSerpentDefs.VS_SoulMark);
            if (soulMark != null && soulMark.Master == caster)
            {
                return true;
            }

            var awakenedSoulMark = TryGetHediff<Hediff_SoulMark>(pawn, SoulSerpentDefs.VS_AwakenedSoulMark);
            return awakenedSoulMark != null && awakenedSoulMark.Master == caster;
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
        /// Checks if a pawn has an awakened soul mark from a specific caster
        /// </summary>
        /// <param name="pawn">The pawn to check</param>
        /// <param name="caster">The caster to check against</param>
        /// <returns>True if the pawn has an awakened soul mark from the specified caster</returns>
        public static bool HasOwnAwakenedSoulMark(Pawn pawn, Pawn caster)
        {
            var awakenedSoulMark = TryGetHediff<Hediff_SoulMark>(pawn, SoulSerpentDefs.VS_AwakenedSoulMark);
            return awakenedSoulMark != null && awakenedSoulMark.Master == caster;
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
            try
            {
                var sourcePsylink = PawnUtility.GetMainPsylinkSource(source);
                var sourceAbilities = PsycastUtility.Psycasts(source);

                if (sourcePsylink != null && source.health != null && dest.health != null)
                {
                    source.health.RemoveHediff(sourcePsylink);
                    if (dest.health.hediffSet != null)
                    {
                        dest.health.hediffSet.hediffs.Add(sourcePsylink);
                    }
                }
                
                if (sourceAbilities != null && source.health != null && dest.health != null)
                {
                    source.health.RemoveHediff(sourceAbilities);
                    if (dest.health.hediffSet != null)
                    {
                        dest.health.hediffSet.hediffs.Add(sourceAbilities);
                    }
                }

                if (source.psychicEntropy != null && dest.psychicEntropy != null)
                {
                    dest.psychicEntropy.OffsetPsyfocusDirectly(source.psychicEntropy.CurrentPsyfocus);
                    dest.psychicEntropy.TryAddEntropy(source.psychicEntropy.EntropyValue);
                    dest.psychicEntropy.SetPsyfocusTarget(source.psychicEntropy.TargetPsyfocus);
                    dest.psychicEntropy.limitEntropyAmount = source.psychicEntropy.limitEntropyAmount;
                }

                var sourceComp = source.GetComp<CompAbilities>();
                var destComp = dest.GetComp<CompAbilities>();
                if (sourceComp != null && destComp != null)
                {
                    foreach (var ability in sourceComp.LearnedAbilities.ToList())
                    {
                        if (ability.def.GetModExtension<AbilityExtension_Psycast>() != null)
                        {
                            destComp.GiveAbility(ability.def);
                        }
                    }
                }

                if (notifyUpdates)
                {
                    NotifyUpdates(dest);
                }
            }
            catch (System.Exception ex)
            {
                Log.Warning($"[SoulSerpent] Error moving psylink from {source?.Name?.ToStringFull ?? "Unknown"} to {dest?.Name?.ToStringFull ?? "Unknown"}: {ex.Message}");
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

        public static void CopyWorkSettings(Pawn sourcePawn, Pawn targetPawn)
        {
            // Copy work schedule (from previous answer)
            CopyWorkSchedule(sourcePawn, targetPawn);
            
            // Copy work priorities
            CopyWorkPriorities(sourcePawn, targetPawn);
            
            // Copy player settings (areas, medical care, etc.)
            CopyPlayerSettings(sourcePawn, targetPawn);
        }

        /// <summary>
        /// Copies the work schedule from the source pawn to the target pawn
        /// </summary>
        /// <param name="sourcePawn">The pawn to copy the schedule from</param>
        /// <param name="targetPawn">The pawn to copy the schedule to</param>
        public static void CopyWorkSchedule(Pawn sourcePawn, Pawn targetPawn)
        {
            if (sourcePawn.timetable == null || targetPawn.timetable == null)
                return;
                
            // Copy the 24-hour schedule
            for (int i = 0; i < 24; i++)
            {
                targetPawn.timetable.times[i] = sourcePawn.timetable.times[i];
            }
        }

        public static void CopyWorkPriorities(Pawn sourcePawn, Pawn targetPawn)
        {
            if (sourcePawn.workSettings == null || targetPawn.workSettings == null)
                return;
                
            // Ensure work settings are initialized
            if (!targetPawn.workSettings.Initialized)
                targetPawn.workSettings.EnableAndInitialize();
                
            // Copy all work type priorities
            var allWorkTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for (int i = 0; i < allWorkTypes.Count; i++)
            {
                WorkTypeDef workType = allWorkTypes[i];
                
                // Only copy if the target pawn can do this work type
                if (!targetPawn.WorkTypeIsDisabled(workType))
                {
                    int priority = sourcePawn.WorkTypeIsDisabled(workType) ? 0 : sourcePawn.workSettings.GetPriority(workType);
                    targetPawn.workSettings.SetPriority(workType, priority);
                }
            }
        }

        public static void CopyPlayerSettings(Pawn sourcePawn, Pawn targetPawn)
        {
            if (sourcePawn.playerSettings == null || targetPawn.playerSettings == null)
                return;
                
            // Copy medical care settings
            targetPawn.playerSettings.medCare = sourcePawn.playerSettings.medCare;
            
            // Copy self-tend setting
            targetPawn.playerSettings.selfTend = sourcePawn.playerSettings.selfTend;
            
            // Copy hostility response mode
            targetPawn.playerSettings.hostilityResponse = sourcePawn.playerSettings.hostilityResponse;
            
            // Copy area restrictions (if on same map)
            if (sourcePawn.Map == targetPawn.Map && sourcePawn.playerSettings.AreaRestrictionInPawnCurrentMap != null)
            {
                targetPawn.playerSettings.AreaRestrictionInPawnCurrentMap = sourcePawn.playerSettings.AreaRestrictionInPawnCurrentMap;
            }
            
            // Copy follow settings
            targetPawn.playerSettings.followDrafted = sourcePawn.playerSettings.followDrafted;
            targetPawn.playerSettings.followFieldwork = sourcePawn.playerSettings.followFieldwork;
        }

        public static void CopyAllPolicies(Pawn sourcePawn, Pawn targetPawn)
        {
            // Copy drug policy
            if (sourcePawn.drugs?.CurrentPolicy != null && targetPawn.drugs != null)
            {
                targetPawn.drugs.CurrentPolicy = sourcePawn.drugs.CurrentPolicy;
            }
            
            // Copy reading policy
            if (sourcePawn.reading?.CurrentPolicy != null && targetPawn.reading != null)
            {
                targetPawn.reading.CurrentPolicy = sourcePawn.reading.CurrentPolicy;
            }

            // Copy food restriction policy
            if (sourcePawn.foodRestriction?.CurrentFoodPolicy != null && targetPawn.foodRestriction != null)
            {
                targetPawn.foodRestriction.CurrentFoodPolicy = sourcePawn.foodRestriction.CurrentFoodPolicy;
            }

            // Copy apparel policy
            if (sourcePawn.outfits?.CurrentApparelPolicy != null && targetPawn.outfits != null)
            {
                targetPawn.outfits.CurrentApparelPolicy = sourcePawn.outfits.CurrentApparelPolicy;
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
                context: PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                fixedBiologicalAge: pawn.ageTracker.AgeBiologicalYears,
                fixedChronologicalAge: pawn.ageTracker.AgeChronologicalYears,
                fixedGender: pawn.gender,
                forceNoIdeo: true,
                forbidAnyTitle: true,
                forcedXenotype: pawn.genes?.Xenotype,
                developmentalStages: DevelopmentalStage.Adult,
                forceRecruitable: true,
                maximumAgeTraits: 0,
                forceNoGear: true
            );

            // Create the husk with the request
            Pawn husk = PawnGenerator.GeneratePawn(request);

            // Ensure the husk was created successfully
            if (husk == null)
            {
                Log.Error("[SoulSerpent] Failed to generate husk pawn");
                return;
            }

            // Copy the appearance/body
            husk.Name = new NameSingle($"{pawn.Name} (Husk)");

            // Copy backstories
            if (husk.story != null && pawn.story != null)
            {
                husk.story.Childhood = pawn.story.Childhood;
                husk.story.Adulthood = pawn.story.Adulthood;
            }

            if (ModsConfig.BiotechActive && pawn.genes != null && husk.genes != null)
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
            if (ModsConfig.IdeologyActive && husk.style != null && pawn.style != null)
            {
                husk.style.BodyTattoo = pawn.style.BodyTattoo;
                husk.style.FaceTattoo = pawn.style.FaceTattoo;
            }

            if (husk.story != null && pawn.story != null)
            {
                husk.story.skinColorOverride = pawn.story.skinColorOverride;
                husk.story.HairColor = pawn.story.HairColor;
                husk.story.hairDef = pawn.story.hairDef;
                husk.story.bodyType = pawn.story.bodyType;
                husk.story.headType = pawn.story.headType;
                husk.story.SkinColorBase = pawn.story.SkinColorBase;
            }

            if (husk.style != null && pawn.style != null)
            {
                husk.style.beardDef = pawn.style.beardDef;
            }

            // Remove ALL hediffs (in case any were generated)
            if (husk.health != null && husk.health.hediffSet != null)
            {
                // Use a safer approach to avoid index out of range exceptions
                while (husk.health.hediffSet.hediffs.Count > 0)
                {
                    var hediff = husk.health.hediffSet.hediffs[0];
                    if (hediff != null)
                    {
                        husk.health.RemoveHediff(hediff);
                    }
                    else
                    {
                        // If we get a null hediff, remove it directly from the list to avoid infinite loop
                        husk.health.hediffSet.hediffs.RemoveAt(0);
                    }
                }
            }

            // Disable all work types by adding them to the disabled list
            if (husk.workSettings != null)
            {
                husk.workSettings.EnableAndInitialize();
                foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    if (!husk.GetDisabledWorkTypes().Contains(workType))
                    {
                        husk.workSettings.Disable(workType);
                    }
                }
            }

            // Clear all skills
            if (husk.skills != null)
            {
                foreach (SkillRecord skill in husk.skills.skills)
                {
                    skill.levelInt = 0;
                    skill.passion = Passion.None;
                    skill.xpSinceLastLevel = 0f;
                    skill.xpSinceMidnight = 0f;
                }
            }

            // Kill the husk to create a corpse
            try
            {
                // Ensure the husk is in a valid state before killing
                if (husk != null && !husk.Dead && husk.health != null)
                {
                    // Double-check that all hediffs are removed
                    if (husk.health.hediffSet != null && husk.health.hediffSet.hediffs.Count > 0)
                    {
                        Log.Warning($"[SoulSerpent] Husks still has {husk.health.hediffSet.hediffs.Count} hediffs before killing, attempting to remove them");
                        while (husk.health.hediffSet.hediffs.Count > 0)
                        {
                            var hediff = husk.health.hediffSet.hediffs[0];
                            if (hediff != null)
                            {
                                husk.health.RemoveHediff(hediff);
                            }
                            else
                            {
                                husk.health.hediffSet.hediffs.RemoveAt(0);
                            }
                        }
                    }
                    
                    husk.Kill(null);
                    
                    // Spawn the corpse at original location
                    if (husk.Corpse != null && originalMap != null)
                    {
                        GenSpawn.Spawn(husk.Corpse, originalPosition, originalMap);
                    }
                }
                else
                {
                    Log.Warning($"[SoulSerpent] Cannot kill husk - it is null, dead, or has no health component");
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
