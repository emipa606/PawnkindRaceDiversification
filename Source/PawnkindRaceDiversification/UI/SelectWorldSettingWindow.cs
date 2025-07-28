using PawnkindRaceDiversification.Handlers;
using UnityEngine;
using Verse;

namespace PawnkindRaceDiversification.UI;

public class SelectWorldSettingWindow : Window
{
    private readonly Vector2 regularButtonSize = new(260f, 28f);
    private Rect btnStartingAdjustments;
    private Rect btnWorldAdjustments;
    private Rect overrideStartingAlienPawnkindsRect;

    private Rect windowTitleRect;

    public SelectWorldSettingWindow()
    {
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        onlyOneOfTypeAllowed = true;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new(300f, 250f);

    public override void DoWindowContents(Rect inRect)
    {
        //Default text settings
        var prevFontSize = Text.Font;
        var prevAnchor = Text.Anchor;

        //Window title
        windowTitleRect = new Rect(new Vector2(
                inRect.x, inRect.y + 8f),
            new Vector2(
                inRect.width, 40f)
        );
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(windowTitleRect, "PawnkindRaceDiversity_SelectSetting".Translate());

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        var yOffset = 32f;
        //World settings button
        btnWorldAdjustments = new Rect(new Vector2(
                inRect.x + (inRect.width - regularButtonSize.x),
                regularButtonSize.y + yOffset),
            regularButtonSize);
        var openedWorldAdjustments = Widgets.ButtonText(btnWorldAdjustments,
            "PawnkindRaceDiversity_WeightWindowTitle_FlatWeightsPerWorldGen".Translate().CapitalizeFirst());
        if (openedWorldAdjustments)
        {
            Find.WindowStack.Add(new WeightSettingsWindow(HandleContext.WORLD));
        }


        yOffset += regularButtonSize.y + 14f;
        //Starting pawns button
        btnStartingAdjustments = new Rect(new Vector2(
                inRect.x + (inRect.width - regularButtonSize.x),
                regularButtonSize.y + yOffset),
            regularButtonSize);
        var openedStartingAdjustments = Widgets.ButtonText(btnStartingAdjustments,
            "PawnkindRaceDiversity_WeightWindowTitle_FlatWeightsStartingPawns".Translate().CapitalizeFirst());
        if (openedStartingAdjustments)
        {
            Find.WindowStack.Add(new WeightSettingsWindow(HandleContext.STARTING));
        }

        yOffset += regularButtonSize.y + 14f;
        //Override starting alien pawnkinds checkmark
        overrideStartingAlienPawnkindsRect = new Rect(new Vector2(
                inRect.x + (inRect.width - regularButtonSize.x),
                regularButtonSize.y + yOffset),
            regularButtonSize);
        if (ModSettingsHandler.OverrideAllAlienPawnkinds)
        {
            GUI.color = Color.gray * new Color(1f, 1f, 1f, 0.3f);
        }

        Widgets.CheckboxLabeled(overrideStartingAlienPawnkindsRect,
            "PawnkindRaceDiversity_OverrideAllStartingAlienPawnkinds_label".Translate(),
            ref ModSettingsHandler.OverrideAllAlienPawnkindsFromStartingPawns,
            ModSettingsHandler.OverrideAllAlienPawnkinds);
        GUI.color = Color.white;

        Text.Font = prevFontSize;
        Text.Anchor = prevAnchor;
    }
}