using System;
using HugsLib.Utils;
using RimWorld.Planet;
using Verse;

namespace PawnkindRaceDiversification.Handlers;

internal class ModWorldSettingsHandler : WorldComponent
{
    public ModWorldSettingsHandler(World world) : base(world)
    {
        //Transfer over the old settings in the world, if they exist.
        //  In case UtilityWorldObjectManager is completely removed, the try catch block will
        //  prevent displaying a harmless error.
        try
        {
            ModSettingsWorldStorage oldWorldSettings = null;
            if (UtilityWorldObjectManager.UtilityWorldObjectExists<ModSettingsWorldStorage>())
            {
                oldWorldSettings = UtilityWorldObjectManager.GetUtilityWorldObject<ModSettingsWorldStorage>();
            }

            if (oldWorldSettings == null)
            {
                return;
            }

            PawnkindRaceDiversification.Logger.Message(
                "The old local settings object holder has been identified in this world. Transfering the local settings over to the new world component.");
            ModSettingsHandler.setLocalFlatWeights.Clear();
            ModSettingsHandler.setLocalFlatWeights.AddRange(oldWorldSettings.oldLocalFlatWeights);
            ResolveMissingWeights();
            ModSettingsHandler.UpdateHandleReferencesInAllReferences(ref ModSettingsHandler.setLocalFlatWeights,
                HandleContext.LOCAL);
            Find.WorldObjects.Remove(oldWorldSettings);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref ModSettingsHandler.setLocalFlatWeights, "localFlatWeights", LookMode.Value,
            LookMode.Value);
        ResolveMissingWeights();
        ModSettingsHandler.UpdateHandleReferencesInAllReferences(ref ModSettingsHandler.setLocalFlatWeights,
            HandleContext.LOCAL);
    }

    //Races added in ongoing saves are set with a weight of 0.0.
    private void ResolveMissingWeights()
    {
        ModSettingsHandler.ResolveMissingRaces(ref ModSettingsHandler.setLocalFlatWeights, 0.0f);
    }
}