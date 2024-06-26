﻿using System;
using System.Collections.Generic;
using System.Linq;
using PawnkindRaceDiversification.Extensions;
using PawnkindRaceDiversification.Handlers;
using RimWorld;
using Verse;
using static PawnkindRaceDiversification.Data.GeneralLoadingDatabase;
using static PawnkindRaceDiversification.Extensions.ExtensionDatabase;

namespace PawnkindRaceDiversification.Patches;

public static class PawnkindGenerationHijacker
{
    //This can be set to true to prevent pawns from being generated with race weights.
    private static bool weightGeneratorPaused;
    public static bool IsPawnOfPlayerFaction { get; } = false;

    public static void PauseWeightGeneration()
    {
        weightGeneratorPaused = true;
    }

    public static bool IsKindValid(PawnGenerationRequest request, bool checkingIfValidAtAll)
    {
        //These steps make sure whether it is really necessary to modify this pawn
        //   or not.
        /*Precautions taken:
         *  1.) kindDef isn't null
         *  2.) kindDef is a humanlike
         *  3.) kindDef isn't an excluded kind def
         *  4.) faction is excluded from the list of factions blacklisted from being overridden
         *  5.) raceDef isn't an implied race (pawnmorpher compatibility)
         *  6.) The weight generator isn't paused
         *  7.) faction isn't the pawnmorpher factions (pawnmorpher compatibility)
         *  8.) Prepare Carefully isn't doing anything
         *  9.) The age of this request is consistent with the age of the race
         *  10.) Validator is checking if this request is valid at all from the above statements
         *       OR OTHERWISE:
         *           kindDef is human and settings want to override all human pawnkinds
         *               OR kindDef is not a human and settings want to override all alien pawnkinds
         *               OR kindDef is not a human and world settings allow starting pawnkinds to be overridden
         * */
        return request.KindDef is { RaceProps.Humanlike: true }
               && !pawnKindDefsExcluded.Contains(request.KindDef.defName)
               && !(request.Faction != null && factionsWithHumanlikesLoaded.Contains(request.Faction.def)
                                            && ModSettingsHandler.excludedFactions.ContainsKey(request.Faction.def
                                                .defName) &&
                                            ModSettingsHandler.excludedFactions[request.Faction.def.defName])
               && !impliedRacesLoaded.Contains(request.KindDef.race.defName)
               && !weightGeneratorPaused
               && !(request.Faction != null &&
                    request.Faction.def.defName is "PawnmorpherPlayerColony" or "PawnmorpherEnclave")
               && PrepareCarefullyTweaks.loadedAlienRace == "none"
               && IsValidAge(request.KindDef)
               && (checkingIfValidAtAll
                   || request.KindDef.race == ThingDefOf.Human && ModSettingsHandler.OverrideAllHumanPawnkinds
                   || request.KindDef.race != ThingDefOf.Human && ModSettingsHandler.OverrideAllAlienPawnkinds
                   || request.KindDef.race != ThingDefOf.Human && request.Faction != null &&
                   request.Faction.def.isPlayer && ModSettingsHandler.OverrideAllAlienPawnkindsFromStartingPawns);
    }

    //Harmony manual prefix method
    public static void DetermineRace(PawnGenerationRequest request)
    {
        //Resetting procedure was moved to "before determining race" now.
        //  Seems to solve a lot more problems than it causes on paper.
        ResetRequest(request);

        try
        {
            if (IsKindValid(request, false))
            {
                if (ModSettingsHandler.DebugMode)
                {
                    PawnkindRaceDiversification.Logger.Message($"Selecting race against {request.KindDef.defName}...");
                }

                //Change this kindDef's race to the selected race temporarily.
                request.KindDef.race = WeightedRaceSelectionProcedure(request.KindDef, request.Faction);
                if (ModSettingsHandler.DebugMode)
                {
                    PawnkindRaceDiversification.Logger.Message(
                        $"The race was chosen to be: {request.KindDef.race.defName}");
                }

                //Everything below has reliant changes to the prevKindsSet.
                //StyleFixProcedure(request.KindDef);
                BackstoryInjectionProcedure(request.KindDef, request.Faction?.def);

                if (ModSettingsHandler.DebugMode)
                {
                    PawnkindRaceDiversification.Logger.Message(
                        $"Race selected successfully to {request.KindDef.race.defName} against pawnkind {request.KindDef.defName}");
                }
            }
        }
        catch (Exception e)
        {
            if (PawnkindRaceDiversification.IsDebugModeInSettingsActive())
            {
                var err = "PRD encountered an error generating a pawn! Stacktrace: \n";
                err += e.ToString();
                PawnkindRaceDiversification.Logger.Error(err);
            }
            else
            {
                PawnkindRaceDiversification.Logger.Error(e.StackTrace);
            }
        }

        //Unpause the weight generator after one pawn was generated.
        //  This is repaused if something is still generating pawns.
        weightGeneratorPaused = false;
    }

    //Reset the PawnGenerationRequest.
    private static void ResetRequest(PawnGenerationRequest request)
    {
        if (!IsKindValid(request, true))
        {
            return;
        }

        //Reset this kindDef's race
        var raceDefName = pawnKindRaceDefRelations.TryGetValue(request.KindDef.defName);
        request.KindDef.race = raceDefName != null
            ? racesLoaded.TryGetValue(raceDefName)
            : racesLoaded.First(r => r.Key.ToLower() == "human").Value;
        //Always reset the pawn's backstory information before determining race.
        if (request.Faction != null)
        {
            request.Faction.def.backstoryFilters =
                defaultFactionBackstorySettings[request.Faction.def.defName].ListFullCopyOrNull();
        }

        request.KindDef.backstoryCategories = defaultKindBackstorySettings[request.KindDef.defName]
            .prevPawnkindBackstoryCategories.ListFullCopyOrNull();
        request.KindDef.backstoryFilters = defaultKindBackstorySettings[request.KindDef.defName]
            .prevPawnkindBackstoryCategoryFilters.ListFullCopyOrNull();
    }

    public static ThingDef WeightedRaceSelectionProcedure(PawnKindDef pawnKind, Faction faction)
    {
        /*      Precedences for weights (first-to-last):
         *          1.) Flat weight (user settings per-save)
         *              ~ World Load
         *          2.) Flat weight (user settings global)
         *          *3.) Flat weight (set by AlienRace XML)
         *              + **Pawnkind weight
         *              + **Faction weight
         *      *chance increases from these conditions, but flat weight in settings overrides these
         *      **If either of these weights are negative, then this pawn cannot spawn in these conditions
         * */
        var determinedWeights = new Dictionary<string, float>();
        var dbgstrList = "";

        foreach (var data in racesDiversified)
        {
            //Faction weight
            FactionWeight factionWeight = null;
            if (faction != null)
            {
                factionWeight = data.Value.factionWeights?.Find(f => f.factionDef == faction.def.defName);
            }

            var fw = factionWeight?.weight ?? 0.0f;
            //Negative value would mean that this pawn shouldn't generate with this faction.
            //  Skip this race.
            if (fw < 0.0f)
            {
                determinedWeights.Remove(data.Key);
                continue;
            }

            determinedWeights.SetOrAdd(data.Key, fw);

            //Pawnkind weight
            var pawnkindWeight = data.Value.pawnKindWeights?.Find(p => p.pawnKindDef == pawnKind.defName);
            var pw = pawnkindWeight?.weight ?? 0.0f;
            //Negative value would mean that this pawn shouldn't generate as this pawnkind.
            //  Skip this race.
            if (pw < 0.0f)
            {
                determinedWeights.Remove(data.Key);
                continue;
            }

            determinedWeights.SetOrAdd(data.Key, pw + fw);

            //Flat generation weight
            var w = data.Value.flatGenerationWeight;
            //Negative value means that this is not modifiable in the mod options, but this race wants
            //  to add weights on its own.
            if (w < 0.0f)
            {
                determinedWeights.SetOrAdd(data.Key, pw + fw);
            }
            else
            {
                determinedWeights.SetOrAdd(data.Key, w + pw + fw);
            }
        }

        //Flat generation weight, determined by user settings (applied generally)
        //  Overrides previous weight calculations.
        //  Prevented if race has a negative flat weight.
        foreach (var kv in ModSettingsHandler.setFlatWeights)
        {
            if (!(kv.Value >= 0.0f))
            {
                continue;
            }

            determinedWeights.SetOrAdd(kv.Key, kv.Value);
            if (ModSettingsHandler.DebugMode)
            {
                dbgstrList += $"{kv.Key}: {kv.Value}\n";
            }
        }

        if (ModSettingsHandler.DebugMode)
        {
            PawnkindRaceDiversification.Logger.Message($"Flat weights found: \n{dbgstrList}");
            dbgstrList = "";
        }

        //Flat generation weight, determined by user settings (applied locally)
        //  Overrides previous weight calculations.
        //  Prevented if race has a negative flat weight.
        foreach (var kv in ModSettingsHandler.setLocalFlatWeights)
        {
            if (!(kv.Value >= 0.0f))
            {
                continue;
            }

            determinedWeights.SetOrAdd(kv.Key, kv.Value);
            if (ModSettingsHandler.DebugMode)
            {
                dbgstrList += $"{kv.Key}: {kv.Value}\n";
            }
        }

        if (ModSettingsHandler.DebugMode)
        {
            PawnkindRaceDiversification.Logger.Message($"Local save weights found: \n{dbgstrList}");
            dbgstrList = "";
        }

        //Flat generation weight, determined by user settings (applied for starting pawns - any pawn that is of player faction)
        //  Overrides previous weight calculations.
        //  Prevented if race has a negative flat weight.
        if (faction != null && faction.def.isPlayer)
        {
            foreach (var kv in ModSettingsHandler.setLocalStartingPawnWeights)
            {
                if (!(kv.Value >= 0.0f))
                {
                    continue;
                }

                determinedWeights.SetOrAdd(kv.Key, kv.Value);
                if (ModSettingsHandler.DebugMode)
                {
                    dbgstrList += $"{kv.Key}: {kv.Value}\n";
                }
            }
        }
        else
        {
            if (ModSettingsHandler.DebugMode)
            {
                PawnkindRaceDiversification.Logger.Warning(
                    $"This pawn wasn't a part of the player faction (of {faction.ToStringSafe()}, {(faction?.def?.isPlayer).ToStringSafe()}) ");
            }
        }

        if (ModSettingsHandler.DebugMode)
        {
            PawnkindRaceDiversification.Logger.Message($"Starting pawn weights found: \n{dbgstrList}");
            dbgstrList = "";
            foreach (var w in determinedWeights)
            {
                dbgstrList += $"{w.Key}: {w.Value}\n";
            }

            PawnkindRaceDiversification.Logger.Message($"Final determined weights: \n{dbgstrList}");
        }

        //Calculate race selection with a weighting procedure
        var sumOfWeights = 0.0f;
        foreach (var w in determinedWeights)
        {
            sumOfWeights += w.Value;
        }

        var rnd = Rand.Value * sumOfWeights;
        try
        {
            foreach (var w in determinedWeights)
            {
                if (rnd < w.Value)
                {
                    return racesLoaded[w.Key];
                }

                rnd -= w.Value;
            }
        }
        catch (Exception)
        {
            //If you see this, contact me about this.
            PawnkindRaceDiversification.Logger.Warning(
                "Failed to assign weighted race! Defaulting to the original race from the pawnkind instead.");
        }

        //Return the original pawnkind race if no race selected
        return pawnKind.race;
    }

    /* Good news! Looking at HAR's code, this seems to have been made redundant.
     * This will be commented out in case it is needed ever again.
    private static void StyleFixProcedure(PawnKindDef pawnkindDef)
    {
        //HAR does not handle hair generation for pawnkinds, therefore I will fix this myself.
        //  To revert to default behavior that HAR already does with factions, I can temporarily set
        //  the pawnkind hairtags to null in order to stop forced hair generation.
        //Pawns that are allowed to have forced hair are pawns that already do spawn with hair (will change this later).
        //  However, pawns that are not supposed to spawn with hair should not have forced pawnkind hair gen.
        if (pawnkindDef?.styleItemTags != null)
        {
            Dictionary<Type, StyleSettings> loadedStyleSettings = raceStyleData.TryGetValue(pawnkindDef.race.defName);
            if (loadedStyleSettings != null)
            {
                List<StyleItemTagWeighted> insertedStyleItemTags = new List<StyleItemTagWeighted>();
                List<StyleItemTagWeighted> overriddenStyleItemTags = new List<StyleItemTagWeighted>();
                bool foundOverrides = false;
                prevPawnkindItemSettings = pawnkindDef.styleItemTags;
                //Start finding tag overrides first
                foreach (KeyValuePair<Type, StyleSettings> ts in loadedStyleSettings)
                {
                    if (ts.Value != null
                        && ts.Value.hasStyle
                        && (ts.Key == typeof(TattooDef) && ModLister.IdeologyInstalled)
                        && ((ts.Value.styleTags?.Count > 0) || (ts.Value.styleTagsOverride?.Count > 0)))
                    {
                        if (ts.Value.styleTags != null)
                            foreach (string s in ts.Value.styleTags)
                            {
                                insertedStyleItemTags.Add(new StyleItemTagWeighted(s, 1.0f));
                                overriddenStyleItemTags.Add(new StyleItemTagWeighted(s, 1.0f));
                            }
                        if (ts.Value.styleTagsOverride != null)
                            foreach (string s in ts.Value.styleTagsOverride)
                            {
                                overriddenStyleItemTags.Add(new StyleItemTagWeighted(s, 1.0f));
                                foundOverrides = true;
                            }
                    }
                }
                if (foundOverrides)
                    pawnkindDef.styleItemTags = overriddenStyleItemTags;
                else if (insertedStyleItemTags.Count > 0)
                    pawnkindDef.styleItemTags?.AddRange(insertedStyleItemTags);
            }
        }
    }
    */

    private static void BackstoryInjectionProcedure(PawnKindDef pawnkindDef, FactionDef factionDef)
    {
        /*  Backstory precedences (first-to-last):
         *      1.) Pawnkind backstories
         *      2.) Faction backstories
         *      3.) General backstories
         */
        var race = pawnkindDef.race.defName; //Because the race should've been assigned to already

        if (racesDiversified.TryGetValue(race, out var raceExtensionData))
        {
            if (ModSettingsHandler.DebugMode)
            {
                PawnkindRaceDiversification.Logger.Message($"Correcting backstory information for {race}... ");
            }

            //Extension data
            FactionWeight factionWeightData = null;
            if (factionDef != null)
            {
                factionWeightData =
                    raceExtensionData.factionWeights?.FirstOrFallback(w => w.factionDef == factionDef.defName);
            }

            var pawnkindWeightData =
                raceExtensionData.pawnKindWeights?.FirstOrFallback(w => w.pawnKindDef == pawnkindDef.defName);

            //Handled backstory data
            var backstoryCategories = new List<string>();
            var backstoryPawnkindFilters = new List<BackstoryCategoryFilter>();
            var backstoryFactionFilters = new List<BackstoryCategoryFilter>();
            var factionBackstoryOverride = false;

            //Procedure
            backstoryCategories.AddRange(pawnkindWeightData?.backstoryCategories ?? []);
            backstoryPawnkindFilters.AddRange(pawnkindWeightData?.backstoryFilters ??
                                              []);
            var pawnkindBackstoryOverride = pawnkindWeightData?.overrideBackstories ?? false;
            if (!pawnkindBackstoryOverride
                && !raceExtensionData.overrideBackstories)
            {
                backstoryCategories.AddRange(pawnkindDef.backstoryCategories ?? []);
                backstoryPawnkindFilters.AddRange(pawnkindDef.backstoryFilters ?? []);
            }

            if (!pawnkindBackstoryOverride)
            {
                backstoryCategories.AddRange(factionWeightData?.backstoryCategories ?? []);
                backstoryFactionFilters.AddRange(factionWeightData?.backstoryFilters ??
                                                 []);
                factionBackstoryOverride = factionWeightData?.overrideBackstories ?? false;
                if (!factionBackstoryOverride
                    && !raceExtensionData.overrideBackstories)
                {
                    backstoryFactionFilters.AddRange(
                        factionDef?.backstoryFilters ?? []);
                }
            }

            if (!pawnkindBackstoryOverride
                && !factionBackstoryOverride)
            {
                backstoryCategories.AddRange(raceExtensionData.backstoryCategories ?? []);
                backstoryFactionFilters.AddRange(raceExtensionData.backstoryFilters ??
                                                 []);
            }

            //Failsafe
            if (backstoryFactionFilters.Count == 0
                && backstoryPawnkindFilters.Count == 0
                && backstoryCategories.Count == 0)
            {
                //Nothing happened here.
                if (ModSettingsHandler.DebugMode)
                {
                    PawnkindRaceDiversification.Logger.Warning("Race did not get any backstories!");
                }

                return;
            }

            //Assignment
            if (factionDef != null)
            {
                factionDef.backstoryFilters = backstoryFactionFilters;
            }

            pawnkindDef.backstoryCategories = backstoryCategories;
            pawnkindDef.backstoryFilters = backstoryPawnkindFilters;

            if (ModSettingsHandler.DebugMode)
            {
                PawnkindRaceDiversification.Logger.Message("Backstories selected successfully.");
            }
        }
        else if (ModSettingsHandler.DebugMode)
        {
            PawnkindRaceDiversification.Logger.Message(
                $"{race} does not have any extension data, therefore it cannot override backstories.");
        }
    }

    //Returns false if any conditions are met that would invalidate the age.
    private static bool IsValidAge(PawnKindDef kindDef)
    {
        /*
        if (ModSettingsHandler.DebugMode)
            PawnkindRaceDiversification.Logger.Message("Generated biological age: " + biologicalAge.ToString() + "\n"
                + "Minimum allowed age: " + kindDef.race.race.ageGenerationCurve.Points[0].x.ToString() + "\n"
                + "Maximum allowed age: " + kindDef.race.race.ageGenerationCurve.Points[kindDef.race.race.ageGenerationCurve.Points.Capacity - 1].x.ToString());
        */
        //Only invalidates if settings don't allow age overriding
        if (ModSettingsHandler.OverridePawnsWithInconsistentAges)
        {
            return true;
        }

        //Invalid if younger than or older than what's supposed to be generated (doesn't appear to be used)
        /*
            if (kindDef.race.race.ageGenerationCurve.Points[0].x > biologicalAge
                || kindDef.race.race.ageGenerationCurve.Points[kindDef.race.race.ageGenerationCurve.Points.Capacity - 1].x < biologicalAge)
            {
                return false;
            }
            */
        //Invalid if generated as a newborn or the kindDef's min and max generated age is 0
        //  Assumed to be a child
        return kindDef.minGenerationAge != 0 || kindDef.maxGenerationAge != 0;
    }
}