using System.Collections.Generic;
using AlienRace;

namespace PawnkindRaceDiversification.Extensions;

internal sealed class ExtensionDatabase
{
    internal static Dictionary<string, RaceDiversificationPool> racesDiversified =
        new Dictionary<string, RaceDiversificationPool>();

    internal static Dictionary<string, ThingDef_AlienRace> racesLoaded = new Dictionary<string, ThingDef_AlienRace>();
}