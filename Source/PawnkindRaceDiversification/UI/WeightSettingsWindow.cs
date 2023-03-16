using System.Collections.Generic;
using System.Linq;
using HugsLib.Settings;
using PawnkindRaceDiversification.Handlers;
using UnityEngine;
using Verse;
using static PawnkindRaceDiversification.Extensions.ExtensionDatabase;

namespace PawnkindRaceDiversification.UI;

public class WeightSettingsWindow : Window
{
    private readonly Dictionary<string, string> inputBoxRaceValue = new Dictionary<string, string>();
    private readonly Dictionary<string, bool> prevAdjustedRaces = new Dictionary<string, bool>();
    private readonly Vector2 regularButtonSize = new Vector2(160f, 46f);
    private readonly Dictionary<string, float> spawnChancesVisual = new Dictionary<string, float>();
    private readonly string windowDesc = "No description";
    private readonly string windowTitle = "Weight Settings Window";
    private Rect btnAccept;
    private Vector2 listButtonSize = new Vector2(84f, 24f);
    private bool quickAdjust;
    private bool quickAdjustInitializedFlag;
    private Rect quickAdjustRect;

    private Vector2 scrollPosition = new Vector2(0f, 0f);
    public HandleContext windowContext;
    private Rect windowDescRect;
    public List<SettingHandle<float>> windowHandles;

    private Rect windowTitleRect;

    public WeightSettingsWindow(HandleContext windowContext)
    {
        this.windowContext = windowContext;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        onlyOneOfTypeAllowed = true;
        switch (windowContext)
        {
            case HandleContext.GENERAL:
                windowTitle = "PawnkindRaceDiversity_WeightWindowTitle_FlatWeights";
                windowDesc = "PawnkindRaceDiversity_WeightWindowDesc_FlatWeights";
                break;
            case HandleContext.WORLD:
                windowTitle = "PawnkindRaceDiversity_WeightWindowTitle_FlatWeightsPerWorldGen";
                windowDesc = "PawnkindRaceDiversity_WeightWindowDesc_FlatWeightsPerWorldGen";
                break;
            case HandleContext.STARTING:
                windowTitle = "PawnkindRaceDiversity_WeightWindowTitle_FlatWeightsStartingPawns";
                windowDesc = "PawnkindRaceDiversity_WeightWindowDesc_FlatWeightsStartingPawns";
                break;
            case HandleContext.LOCAL:
                windowTitle = "PawnkindRaceDiversity_WeightWindowTitle_FlatWeightsLocal";
                windowDesc = "PawnkindRaceDiversity_WeightWindowDesc_FlatWeightsLocal";
                break;
        }

        windowHandles = ModSettingsHandler.allHandleReferences;
        EvaluateWhichDefsAreAdjusted();
    }

    public override Vector2 InitialSize => new Vector2(760f, 730f);

    public override void DoWindowContents(Rect inRect)
    {
        //Default text settings
        var prevFontSize = Text.Font;
        var prevAnchor = Text.Anchor;

        //Window title
        windowTitleRect = new Rect(new Vector2(
                inRect.x, inRect.y),
            new Vector2(
                inRect.width, 40f)
        );
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(windowTitleRect, windowTitle.Translate());
        var windowTitleElementsYOffset = 28f;
        //Window description
        windowDescRect = new Rect(new Vector2(
                inRect.x, inRect.y + windowTitleElementsYOffset),
            new Vector2(
                inRect.width, 40f)
        );
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(windowDescRect, windowDesc.Translate());
        Text.Font = prevFontSize;
        Text.Anchor = prevAnchor;

        var listingStandardRectOuter
            = new Rect(new Vector2(inRect.x + 5f, inRect.y + 40f + windowTitleElementsYOffset),
                new Vector2(inRect.width - 10f, inRect.height - 120f - windowTitleElementsYOffset));
        var listingStandardRectInner
            = new Rect(new Vector2(inRect.x + 10f, inRect.y + 45f + windowTitleElementsYOffset),
                new Vector2(inRect.width - 20f, inRect.height - 130f - windowTitleElementsYOffset));
        Widgets.DrawMenuSection(listingStandardRectOuter);
        //Column title delimiter
        var tableStart = listingStandardRectOuter.x + 2f;
        var YrowStart = listingStandardRectOuter.y + 2f;
        var YrowEnd = listingStandardRectOuter.height - 2f;
        Widgets.DrawLineHorizontal(tableStart, listingStandardRectOuter.y + 20f, listingStandardRectOuter.width - 24f);
        //Name column delimiter
        var columnStart1 = tableStart + 180f;
        Widgets.DrawLineVertical(columnStart1, YrowStart, YrowEnd);
        //Flat weight column delimiter
        var columnStart2 = columnStart1 + 78f;
        Widgets.DrawLineVertical(columnStart2, YrowStart, YrowEnd);
        //Spawn chance column delimiter
        var columnStart3 = columnStart2 + 118f;
        Widgets.DrawLineVertical(columnStart3, YrowStart, YrowEnd);
        //Def adjusted delimiter
        var columnStartLast = columnStart3 + 118f;
        Widgets.DrawLineVertical(columnStartLast, YrowStart, YrowEnd);

        var tableEnd = listingStandardRectOuter.width;

        var elementSize = 36f;
        GUI.BeginGroup(listingStandardRectInner);
        Widgets.BeginScrollView(
            new Rect(0f, 20f, listingStandardRectInner.width - 2f, listingStandardRectInner.height - 2f),
            ref scrollPosition,
            new Rect(listingStandardRectInner.x, listingStandardRectInner.y - (elementSize * 1.6f),
                listingStandardRectInner.width,
                (ModSettingsHandler.evaluatedRaces.Count * elementSize) +
                ((elementSize * (1.6f / 4f)) - (windowTitleElementsYOffset / 2f))));
        var element = 0;
        CalculateSpawnChances();
        foreach (var race in ModSettingsHandler.evaluatedRaces.OrderBy(s => s))
        {
            var yPos = (element * elementSize) + (windowTitleElementsYOffset / 2f);
            element++;
            var innerContentRect = new Rect(listingStandardRectInner.x, yPos, 180f, elementSize);
            var raceDef = ThingDef.Named(race);
            var raceName = race;
            var toolTip = string.Empty;
            if (raceDef != null)
            {
                raceName = raceDef.LabelCap;
                toolTip = raceDef.modContentPack?.Name;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            //Race
            Widgets.Label(innerContentRect, raceName);
            TooltipHandler.TipRegion(innerContentRect, toolTip);
            Text.Anchor = TextAnchor.MiddleCenter;
            //Weight
            innerContentRect = new Rect(columnStart1, yPos, 78f, elementSize);
            if (!quickAdjust)
            {
                Widgets.Label(innerContentRect, GrabWeightReference(race, windowContext).ToString("0.0##"));
            }
            else
            {
                if (quickAdjust && !quickAdjustInitializedFlag)
                {
                    inputBoxRaceValue.Add(race, GrabWeightReference(race, windowContext).ToString("0.0##"));
                }

                innerContentRect = new Rect(columnStart1 + 2f, yPos + 6f, 78f - 4f, (elementSize / 2f) + 6f);
                var inp = Widgets.TextField(innerContentRect, inputBoxRaceValue[race]);
                inputBoxRaceValue[race] = inp;
                var valid = float.TryParse(inputBoxRaceValue[race], out var value);
                if (valid && value != GrabWeightReference(race, windowContext))
                {
                    SetWeightReference(race, value);
                }
            }

            //Spawn Chance
            innerContentRect = new Rect(columnStart2 + 2f, yPos, 118f, elementSize);
            Widgets.Label(innerContentRect, spawnChancesVisual[race].ToStringPercent());
            //Prev Adjusted
            innerContentRect = new Rect(columnStart3 + 2f, yPos, 114f, elementSize);
            if (!(race.ToLower() == "human" && windowContext == HandleContext.GENERAL))
            {
                var checkboxTmp = prevAdjustedRaces[race];
                Widgets.Checkbox(new Vector2(innerContentRect.x + (118f / 3f) + 5f, innerContentRect.y + 5f),
                    ref checkboxTmp, 24f, false, true);
                prevAdjustedRaces[race] = checkboxTmp;
            }

            //Actions
            innerContentRect = new Rect(columnStartLast + 4f, yPos, tableEnd - columnStartLast - 28f, elementSize);
            var showAdjustment = Widgets.ButtonText(innerContentRect,
                "PawnkindRaceDiversity_Category_ShowAdjustments".Translate());
            if (showAdjustment)
            {
                Find.WindowStack.Add(new WeightAdjustmentWindow(this, race));
            }
        }

        Text.Anchor = prevAnchor;
        Widgets.EndScrollView();
        GUI.EndGroup();

        //Column labels
        Text.Anchor = TextAnchor.MiddleCenter;
        var columnLabel = new Rect(tableStart + 2f, YrowStart,
            columnStart1 - 14f, 18f);
        Widgets.Label(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnLabel_RaceDef".Translate());
        HighlightableInfoMouseover(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnTooltip_RaceDef");
        columnLabel = new Rect(columnStart1 + 4f, YrowStart,
            72f, 18f);
        Widgets.Label(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnLabel_FlatWeight".Translate());
        HighlightableInfoMouseover(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnTooltip_FlatWeight");
        columnLabel = new Rect(columnStart2 + 4f, YrowStart,
            112f, 18f);
        Widgets.Label(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnLabel_SpawnChance".Translate());
        HighlightableInfoMouseover(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnTooltip_SpawnChance");
        columnLabel = new Rect(columnStart3 + 4f, YrowStart,
            112f, 18f);
        Widgets.Label(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnLabel_AllowPrevAdjusted".Translate());
        HighlightableInfoMouseover(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnTooltip_AllowPrevAdjusted");
        columnLabel = new Rect(columnStartLast + 4f, YrowStart,
            listingStandardRectOuter.x + (tableEnd - columnStartLast - 28f), 18f);
        Widgets.Label(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnLabel_Action".Translate());
        HighlightableInfoMouseover(columnLabel, "PawnkindRaceDiversity_WeightSettingColumnTooltip_Action");
        Text.Anchor = prevAnchor;

        //Other actions
        //Accept button
        btnAccept = new Rect(new Vector2(
                inRect.x + (inRect.width / 2.6f),
                inRect.height - regularButtonSize.y - 10),
            regularButtonSize);
        if (Widgets.ButtonText(btnAccept, "Accept".Translate().CapitalizeFirst()))
        {
            Close();
        }

        //Quick adjust
        if (quickAdjust && !quickAdjustInitializedFlag)
        {
            quickAdjustInitializedFlag = true;
        }

        quickAdjustRect = new Rect(inRect.x + 12f, inRect.height - 65f, 116f, 28f);
        Widgets.CheckboxLabeled(quickAdjustRect, "PawnkindRaceDiversity_Checkbox_QuickAdjust".Translate(),
            ref quickAdjust);
        if (Mouse.IsOver(quickAdjustRect))
        {
            TooltipHandler.TipRegion(quickAdjustRect, "PawnkindRaceDiversity_CheckboxTooltip_QuickAdjust".Translate());
        }

        if (!quickAdjust && quickAdjustInitializedFlag)
        {
            inputBoxRaceValue.Clear();
            quickAdjustInitializedFlag = false;
        }

        //Set all to 0
        var resetRect = new Rect(inRect.width - 200f, inRect.height - 65f, 185f, 28f);
        var setToZero = Widgets.ButtonText(resetRect, "PawnkindRaceDiversity_Button_SetToZero".Translate());
        if (setToZero)
        {
            Find.WindowStack.Add(new Dialog_MessageBox
            (
                "PawnkindRaceDiversity_Button_SetToZero_Confirmation".Translate(),
                "Yes".Translate(),
                delegate
                {
                    foreach (var race in ModSettingsHandler.evaluatedRaces)
                    {
                        SetWeightReference(race, 0.0f);
                    }
                },
                "No".Translate(),
                null,
                null, true
            ));
        }

        //Reset all settings here
        resetRect = new Rect(inRect.width - 200f, inRect.height - 30f, 185f, 28f);
        var resetAll = Widgets.ButtonText(resetRect, "PawnkindRaceDiversity_Button_ResetToDefaults".Translate());
        if (resetAll)
        {
            Find.WindowStack.Add(new Dialog_MessageBox
            (
                "PawnkindRaceDiversity_Button_ResetToDefaults_Confirmation".Translate(),
                "Yes".Translate(),
                delegate
                {
                    foreach (var race in ModSettingsHandler.evaluatedRaces)
                    {
                        SetWeightReference(race, -1.0f);
                    }
                },
                "No".Translate(),
                null,
                null, true
            ));
        }
    }

    private void HighlightableInfoMouseover(Rect rect, string label)
    {
        if (!Mouse.IsOver(rect))
        {
            return;
        }

        Widgets.DrawHighlight(rect);
        TooltipHandler.TipRegion(rect, label.Translate());
    }

    public float GrabWeightReference(string race, HandleContext context, bool returnNegative = false)
    {
        var fullID = ModSettingsHandler.GetRaceSettingWeightID(context, race);
        if (fullID == null)
        {
            return 0.0f;
        }

        var handle = windowHandles.FirstOrFallback(h => h.Name == fullID);
        if (handle == null)
        {
            return 0.0f;
        }

        if ((!prevAdjustedRaces[race] || windowContext != context) &&
            (!(handle.Value < 0.0f) || windowContext == context))
        {
            return handle.Value >= 0.0f ? handle.Value : 0.0f;
        }

        if (returnNegative)
        {
            return -1.0f;
        }

        switch (context)
        {
            case HandleContext.STARTING:
                return GrabWeightReference(race, HandleContext.WORLD);
            case HandleContext.WORLD:
            case HandleContext.LOCAL:
                //This is performed so that it can also return the human's race setting
                return GrabWeightReference(race, HandleContext.GENERAL);
            case HandleContext.GENERAL:
                var data = racesDiversified.FirstOrFallback(r => r.Key == race);
                if (data.Key != null)
                {
                    return data.Value.flatGenerationWeight;
                }

                break;
        }

        //This does not display negatives (checkbox will check for that instead)
        return handle.Value >= 0.0f ? handle.Value : 0.0f;
    }

    public void SetWeightReference(string race, float value)
    {
        var fullID = ModSettingsHandler.GetRaceSettingWeightID(windowContext, race);
        if (fullID == null)
        {
            return;
        }

        var handle = windowHandles.FirstOrFallback(h => h.Name == fullID);
        if (handle == null)
        {
            return;
        }

        if (value > 1.0f)
        {
            value = 1.0f;
        }

        if (value >= 0.0f)
        {
            prevAdjustedRaces[race] = false;
        }
        else if (value < 0.0f)
        {
            if (race.ToLower() == "human" && windowContext == HandleContext.GENERAL)
            {
                value = handle.DefaultValue;
            }
            else
            {
                prevAdjustedRaces[race] = true;
            }
        }

        handle.Value = value;
    }

    private void EvaluateWhichDefsAreAdjusted()
    {
        foreach (var race in ModSettingsHandler.evaluatedRaces)
        {
            var fullID = ModSettingsHandler.GetRaceSettingWeightID(windowContext, race);
            var handle = windowHandles.FirstOrFallback(h => h.Name == fullID);
            if (handle.Value < 0.0f)
            {
                prevAdjustedRaces[race] = true;
            }
            else
            {
                prevAdjustedRaces[race] = false;
            }
        }
    }

    private void CalculateSpawnChances()
    {
        spawnChancesVisual.Clear();
        float sum = 0;
        foreach (var race in ModSettingsHandler.evaluatedRaces)
        {
            sum += GrabWeightReference(race, windowContext);
        }

        if (sum == 0)
        {
            foreach (var race in ModSettingsHandler.evaluatedRaces)
            {
                spawnChancesVisual.Add(race, 0.0f);
            }

            return;
        }

        foreach (var race in ModSettingsHandler.evaluatedRaces)
        {
            spawnChancesVisual.Add(race, GrabWeightReference(race, windowContext) / sum);
        }
    }

    public override void PreClose()
    {
        foreach (var handle in windowHandles)
        {
            //Should only update values within this window's context
            if (ModSettingsHandler.WhatContextIsID(handle.Name) != windowContext)
            {
                continue;
            }

            float value;
            if (prevAdjustedRaces[handle.Title])
            {
                value = -1f;
            }
            else
            {
                value = GrabWeightReference(handle.Title, windowContext);
            }

            ModSettingsHandler.allHandleReferences.Find(h => h.Name == handle.Name).Value = value;
        }

        base.PreClose();
    }
}