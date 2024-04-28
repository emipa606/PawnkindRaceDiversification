using UnityEngine;
using Verse;

namespace PawnkindRaceDiversification.UI;

public class WeightAdjustmentWindow : Window
{
    private readonly WeightSettingsWindow parent;
    private readonly string raceAdjusting;
    private readonly Vector2 regularButtonSize = new Vector2(160f, 46f);
    private readonly string windowTitle = "PawnkindRaceDiversity_AdjustmentWindowTitle";
    private Rect btnAccept;
    private float outFlatWeight;
    private string textField;
    private Rect windowDescRect;

    private Rect windowTitleRect;

    public WeightAdjustmentWindow(WeightSettingsWindow parent, string raceAdjusting)
    {
        this.parent = parent;
        this.raceAdjusting = raceAdjusting;
        outFlatWeight = parent.GrabWeightReference(raceAdjusting, parent.windowContext, true);
        textField = outFlatWeight.ToString("0.0##");
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        onlyOneOfTypeAllowed = true;
    }

    public override Vector2 InitialSize => new Vector2(300f, 380f);

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
        //Race being adjusted
        windowDescRect = new Rect(new Vector2(
                inRect.x, inRect.y + 28f),
            new Vector2(
                inRect.width, 40f)
        );
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(windowDescRect, "PawnkindRaceDiversity_TextboxLabel_Adjusting".Translate() + " " + raceAdjusting);
        Text.Font = prevFontSize;
        Text.Anchor = prevAnchor;

        var weightAdjustmentRectLabel = new Rect(inRect.width - 210f, inRect.y + 68f, 105f, 24f);
        var weightAdjustmentRect = new Rect(inRect.width - 110f, inRect.y + 68f, 76f, 24f);
        Widgets.Label(weightAdjustmentRectLabel, "PawnkindRaceDiversity_TextboxLabel_SetFlatWeight".Translate());
        var inp = Widgets.TextField(weightAdjustmentRect, textField);
        textField = inp;
        var valid = float.TryParse(textField, out var value);
        if (valid)
        {
            outFlatWeight = value;
        }

        var moreComingSoonRect = new Rect(inRect.x + 20f, inRect.y + 100f, 220f, 60f);
        Widgets.Label(moreComingSoonRect, "More coming soon on this window. Please be patient.");

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

    public override void PreClose()
    {
        parent.SetWeightReference(raceAdjusting, outFlatWeight);
        base.PreClose();
    }
}