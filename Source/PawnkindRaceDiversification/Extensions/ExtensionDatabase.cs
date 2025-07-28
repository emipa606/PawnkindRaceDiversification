using System.Collections.Generic;
using AlienRace;

namespace PawnkindRaceDiversification.Extensions;

internal sealed class ExtensionDatabase
{
    internal static readonly Dictionary<string, RaceDiversificationPool> racesDiversified = new();

    internal static readonly Dictionary<string, ThingDef_AlienRace> racesLoaded = new();
}