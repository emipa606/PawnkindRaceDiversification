using System;
using System.Collections.Generic;
using HugsLib.Utils;
using Verse;

namespace PawnkindRaceDiversification.Handlers;

[Obsolete("No longer used after 1.3.")]
internal class ModSettingsWorldStorage : UtilityWorldObject
{
    public Dictionary<string, float> oldLocalFlatWeights = new Dictionary<string, float>();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref oldLocalFlatWeights, "localFlatWeights", LookMode.Value, LookMode.Value);
    }
}