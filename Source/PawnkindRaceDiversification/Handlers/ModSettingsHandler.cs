using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using PawnkindRaceDiversification.UI;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static PawnkindRaceDiversification.Data.GeneralLoadingDatabase;

namespace PawnkindRaceDiversification.Handlers;

internal class ModSettingsHandler
{
    internal const string showSettingsValid = "PawnkindRaceDiversity_Category_ShowSettings";
    internal static SettingHandle<bool> DebugMode;
    internal static SettingHandle<bool> OverrideAllHumanPawnkinds;
    internal static SettingHandle<bool> OverrideAllAlienPawnkinds;
    internal static bool OverrideAllAlienPawnkindsFromStartingPawns = false;
    internal static SettingHandle<bool> OverridePawnsWithInconsistentAges;

    internal static Dictionary<string, SettingHandle<bool>> excludedFactions =
        new Dictionary<string, SettingHandle<bool>>();

    internal static readonly Dictionary<string, float> setFlatWeights = new Dictionary<string, float>();
    internal static Dictionary<string, float> setLocalFlatWeights = new Dictionary<string, float>();
    internal static Dictionary<string, float> setLocalWorldWeights = new Dictionary<string, float>();
    internal static Dictionary<string, float> setLocalStartingPawnWeights = new Dictionary<string, float>();
    internal static readonly List<SettingHandle<float>> allHandleReferences = [];
    internal static List<string> evaluatedRaces = [];

    internal void PrepareSettingHandles(ModSettingsPack pack, List<string> races)
    {
        evaluatedRaces = races;
        DebugMode = pack.GetHandle("DebugMode", "PawnkindRaceDiversity_DebugMode_label".Translate(),
            "PawnkindRaceDiversity_DebugMode_description".Translate(), false);
        OverrideAllHumanPawnkinds = pack.GetHandle("OverrideAllHumanPawnkinds",
            "PawnkindRaceDiversity_OverrideAllHumanPawnkinds_label".Translate(),
            "PawnkindRaceDiversity_OverrideAllHumanPawnkinds_description".Translate(), true);
        OverrideAllAlienPawnkinds = pack.GetHandle("OverrideAllAlienPawnkinds",
            "PawnkindRaceDiversity_OverrideAllAlienPawnkinds_label".Translate(),
            "PawnkindRaceDiversity_OverrideAllAlienPawnkinds_description".Translate(), false);
        OverridePawnsWithInconsistentAges = pack.GetHandle("OverridePawnsWithInconsistentAges",
            "PawnkindRaceDiversity_OverridePawnsWithInconsistentAges_label".Translate(),
            "PawnkindRaceDiversity_OverridePawnsWithInconsistentAges_description".Translate(), false);

        //Excluded factions
        ConstructOtherAdjustmentHandles(pack, "RaceOverrideExcludedFaction",
            (from def in factionsWithHumanlikesLoaded select def.defName).ToList(), ref excludedFactions, false, false);
        //Global weights
        ConstructRaceAdjustmentHandles(pack, HandleContext.GENERAL);
        //Per-world weights
        ConstructRaceAdjustmentHandles(pack, HandleContext.WORLD);
        //Starting pawn weights
        ConstructRaceAdjustmentHandles(pack, HandleContext.STARTING);
        //Local weights
        ConstructRaceAdjustmentHandles(pack, HandleContext.LOCAL);

        //Settings category buttons construction
        //Excluded factions
        SettingsButtonCategoryConstructor(pack,
            "PawnkindRaceDiversity_FactionExclusionWindowTitle",
            showSettingsValid,
            "PawnkindRaceDiversity_FactionExclusionWindowDescription",
            delegate { Find.WindowStack.Add(new FactionExclusionWindow()); });
        //----Weights related----
        //Flat weights
        SettingsButtonCategoryConstructor(pack,
            "PawnkindRaceDiversity_WeightWindowTitle_FlatWeights",
            showSettingsValid,
            "PawnkindRaceDiversity_FlatWeights_Category_description",
            delegate { Find.WindowStack.Add(new WeightSettingsWindow(HandleContext.GENERAL)); });
        //Local weights
        SettingsButtonCategoryConstructor(pack,
            "PawnkindRaceDiversity_WeightWindowTitle_FlatWeightsLocal",
            showSettingsValid,
            "PawnkindRaceDiversity_FlatWeightsLocal_Category_description",
            delegate { Find.WindowStack.Add(new WeightSettingsWindow(HandleContext.LOCAL)); },
            () => !isInWorld());
    }

    //Constructs a button in the mod settings that handles custom actions.
    //  Specifically, these are made in order to create special windows.
    private void SettingsButtonCategoryConstructor(ModSettingsPack pack, string labelID, string buttonLabel,
        string desc,
        Action buttonAction, Func<bool> invalidCondition = null)
    {
        var handle = pack.GetHandle(labelID, labelID.Translate(), desc.Translate(), false);
        handle.Unsaved = true;
        handle.CustomDrawer = delegate(Rect rect)
        {
            var invalid = false;
            if (invalidCondition != null)
            {
                invalid = invalidCondition();
            }

            if (invalid)
            {
                GUI.color = new Color(1f, 0.3f, 0.35f);
            }

            var validButtonRes = Widgets.ButtonText(rect, buttonLabel.Translate());
            if (validButtonRes)
            {
                if (!invalid)
                {
                    buttonAction();
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
            }

            GUI.color = Color.white;
            return false;
        };
    }

    //Make race adjustment settings handles. Are never visible, but are adjusted through other means.
    private void ConstructRaceAdjustmentHandles(ModSettingsPack pack, HandleContext context)
    {
        foreach (var race in evaluatedRaces)
        {
            //ID of handle
            var weightID = GetRaceSettingWeightID(context, race);

            //Default value
            var defaultValue = -1f;
            if (race.ToLower() == "human" && context == HandleContext.GENERAL)
            {
                defaultValue = 0.35f;
            }

            //Handle configuration
            var handle = pack.GetHandle(weightID, race, null, defaultValue, Validators.FloatRangeValidator(-1f, 1.0f));
            handle.Unsaved = context is HandleContext.WORLD or HandleContext.STARTING or HandleContext.LOCAL;
            handle.NeverVisible = true; //Never visible because it is handled by custom GUI instead
            handle.ValueChanged += delegate(SettingHandle newHandle)
            {
                float.TryParse(newHandle.StringValue, out var val);

                switch (context)
                {
                    case HandleContext.GENERAL:
                        var handleRace = newHandle.Title;
                        if (val < 0.0f)
                        {
                            setFlatWeights.Remove(handleRace);
                        }
                        else
                        {
                            setFlatWeights.SetOrAdd(handleRace, val);
                        }

                        break;
                    case HandleContext.WORLD:
                        setLocalWorldWeights.SetOrAdd(newHandle.Title, val);
                        break;
                    case HandleContext.STARTING:
                        setLocalStartingPawnWeights.SetOrAdd(newHandle.Title, val);
                        break;
                    case HandleContext.LOCAL:
                        setLocalFlatWeights.SetOrAdd(newHandle.Title, val);
                        break;
                }
            };

            //List constructions
            if (handle.Value >= 0.0f && context == HandleContext.GENERAL)
            {
                setFlatWeights.SetOrAdd(race, handle.Value);
            }
            else if (context == HandleContext.WORLD)
            {
                setLocalWorldWeights.SetOrAdd(race, handle.Value);
            }
            else if (context == HandleContext.STARTING)
            {
                setLocalStartingPawnWeights.SetOrAdd(race, handle.Value);
            }
            else if (context == HandleContext.LOCAL)
            {
                foreach (var lh in allHandleReferences.FindAll(h => WhatContextIsID(h.Name) == HandleContext.LOCAL))
                {
                    lh.Value = setLocalFlatWeights.TryGetValue(lh.Title);
                    lh.StringValue = lh.Value.ToString();
                }
            }

            allHandleReferences.Add(handle);
        }
    }

    //Construct other list-related handles
    private void ConstructOtherAdjustmentHandles(ModSettingsPack pack, string handleName, List<string> elements,
        ref Dictionary<string, SettingHandle<bool>> handleDict, bool defaultValue, bool unsaved)
    {
        foreach (var defName in elements)
        {
            //Handle configuration
            var id = $"{handleName}_{defName}";
            var handle = pack.GetHandle(id, defName, null, defaultValue);
            handle.Unsaved = unsaved;
            handle.NeverVisible = true; //Never visible because it is handled by custom GUI instead
            handleDict.Add(defName, handle);
        }
    }

    internal static string GetRaceSettingWeightID(HandleContext context, string race)
    {
        switch (context)
        {
            case HandleContext.GENERAL:
                return $"flatGenerationWeight_{race}";
            case HandleContext.WORLD:
                return $"flatGenerationWeightPerWorld_{race}";
            case HandleContext.STARTING:
                return $"flatGenerationWeightStartingPawns_{race}";
            case HandleContext.LOCAL:
                return $"flatGenerationWeightLocal_{race}";
        }

        return null;
    }

    internal static HandleContext WhatContextIsID(string id)
    {
        if (id.StartsWith("flatGenerationWeightPerWorld"))
        {
            return HandleContext.WORLD;
        }

        if (id.StartsWith("flatGenerationWeightStartingPawns"))
        {
            return HandleContext.STARTING;
        }

        if (id.StartsWith("flatGenerationWeightLocal"))
        {
            return HandleContext.LOCAL;
        }

        return id.StartsWith("flatGenerationWeight") ? HandleContext.GENERAL : HandleContext.NONE;
    }

    internal static void SyncWorldWeightsIntoLocalWeights()
    {
        foreach (var wv in setLocalWorldWeights)
        {
            setLocalFlatWeights.SetOrAdd(wv.Key, wv.Value);
            var lhandle =
                allHandleReferences.Find(h => h.Title == wv.Key && WhatContextIsID(h.Name) == HandleContext.LOCAL);
            if (lhandle == null)
            {
                continue;
            }

            lhandle.Value = wv.Value;
            lhandle.StringValue = wv.Value.ToString();
        }
    }

    internal static void UpdateHandleReferencesInAllReferences(ref Dictionary<string, float> handle,
        HandleContext context)
    {
        foreach (var handleInAll in allHandleReferences.FindAll(h => WhatContextIsID(h.Name) == context))
        {
            var successful = handle.TryGetValue(handleInAll.Title, out var weight);
            if (!successful)
            {
                continue;
            }

            handleInAll.Value = weight;
            handleInAll.StringValue = weight.ToString();
        }
    }

    //Races are missing when they are newly loaded.
    //  Therefore, this method exists to fix any handles that are missing races.
    internal static void ResolveMissingRaces(ref Dictionary<string, float> handle, float placedWeight)
    {
        var hcopy = new Dictionary<string, float>(handle);

        //Add new races to handler
        foreach (var evalRace in evaluatedRaces)
        {
            if (!handle.ContainsKey(evalRace))
            {
                handle.SetOrAdd(evalRace, placedWeight);
            }
        }

        //Remove missing races from handler
        foreach (var key in hcopy.Keys)
        {
            if (!evaluatedRaces.Contains(key))
            {
                handle.Remove(key);
            }
        }
    }

    internal static void ResetHandle(ref Dictionary<string, float> handle, HandleContext context)
    {
        var hcopy = new Dictionary<string, float>(handle);
        foreach (var key in hcopy.Keys)
        {
            handle[key] = -1.0f;
        }

        UpdateHandleReferencesInAllReferences(ref handle, context);
    }

    private bool isInWorld()
    {
        var game = Current.Game;
        var world = game?.World;
        return world != null;
    }
}