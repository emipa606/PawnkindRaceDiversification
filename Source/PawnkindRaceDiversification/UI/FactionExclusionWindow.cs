using System.Linq;
using PawnkindRaceDiversification.Handlers;
using UnityEngine;
using Verse;
using static PawnkindRaceDiversification.Data.GeneralLoadingDatabase;

namespace PawnkindRaceDiversification.UI;

public class FactionExclusionWindow : Window
{
    private readonly Vector2 regularButtonSize = new Vector2(160f, 46f);
    private Rect btnAccept;
    private Vector2 scrollPosition = new Vector2(0f, 0f);
    private Rect windowDescRect;
    protected string windowDescription = "PawnkindRaceDiversity_FactionExclusionWindowDescription";
    protected string windowTitle = "PawnkindRaceDiversity_FactionExclusionWindowTitle";

    private Rect windowTitleRect;

    public FactionExclusionWindow()
    {
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        onlyOneOfTypeAllowed = true;
    }

    public override Vector2 InitialSize => new Vector2(340f, 720f);

    public override void DoWindowContents(Rect inRect)
    {
        //Default text settings
        var prevFontSize = Text.Font;
        var prevAnchor = Text.Anchor;

        //Window title
        if (windowTitle != null)
        {
            windowTitleRect = new Rect(new Vector2(
                    inRect.x, inRect.y),
                new Vector2(
                    inRect.width, 40f)
            );
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(windowTitleRect, windowTitle.Translate());
        }

        var windowTitleElementsYOffset = 28f;
        //Window description
        if (windowDescription != null)
        {
            windowDescRect = new Rect(new Vector2(
                    inRect.x, inRect.y + 32f),
                new Vector2(
                    inRect.width, 68f)
            );
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(windowDescRect, windowDescription.Translate());
        }

        windowTitleElementsYOffset += 38f;
        Text.Font = prevFontSize;
        Text.Anchor = prevAnchor;

        if (factionsWithHumanlikesLoaded.Count != 0)
        {
            var listingStandardRectInner = new Rect(
                new Vector2(inRect.x + 10f, inRect.y + 45f + windowTitleElementsYOffset),
                new Vector2(inRect.width - 20f, inRect.height - 130f - windowTitleElementsYOffset));
            var elementSize = 24f;
            GUI.BeginGroup(listingStandardRectInner);
            Widgets.BeginScrollView(
                new Rect(0f, 20f, listingStandardRectInner.width - 2f, listingStandardRectInner.height - 2f),
                ref scrollPosition,
                new Rect(listingStandardRectInner.x, listingStandardRectInner.y - (elementSize * 1.6f),
                    listingStandardRectInner.width,
                    (ModSettingsHandler.excludedFactions.Count * elementSize) +
                    ((elementSize * 1.6f) - (windowTitleElementsYOffset / 2f))));
            var element = 0;
            foreach (var handle in ModSettingsHandler.excludedFactions.Values.OrderBy(handle => handle.Title))
            {
                var def = factionsWithHumanlikesLoaded.Find(f => f.defName == handle.Title);
                if (def == null)
                {
                    continue;
                }

                var yPos = (element * elementSize) + windowTitleElementsYOffset;
                element++;
                var innerContentRect = new Rect(listingStandardRectInner.x, yPos, 180f, elementSize);
                Text.Anchor = TextAnchor.MiddleLeft;
                //Faction
                Widgets.Label(innerContentRect, def.LabelCap);
                TooltipHandler.TipRegion(innerContentRect, def.modContentPack?.Name);
                Text.Anchor = TextAnchor.MiddleCenter;
                //Excluded
                innerContentRect = new Rect(80f, yPos, 114f, elementSize);
                var tmpExcludedCheck = handle.Value;
                Widgets.Checkbox(new Vector2(innerContentRect.x + 160f, innerContentRect.y + 5f),
                    ref tmpExcludedCheck, 24f, false, true);
                handle.Value = tmpExcludedCheck;
            }

            Text.Anchor = prevAnchor;
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        //Accept button
        btnAccept = new Rect(new Vector2(
                inRect.x + (inRect.width / 4.6f),
                inRect.height - regularButtonSize.y - 10),
            regularButtonSize);
        if (Widgets.ButtonText(btnAccept, "Accept".Translate().CapitalizeFirst()))
        {
            Close();
        }
    }
}