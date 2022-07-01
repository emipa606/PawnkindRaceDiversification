using System.Collections.Generic;
using RimWorld;

namespace PawnkindRaceDiversification.Data;

internal sealed class GeneralLoadingDatabase
{
    internal static List<string> impliedRacesLoaded = new List<string>();
    internal static List<string> pawnKindDefsExcluded = new List<string>();
    internal static List<FactionDef> factionsWithHumanlikesLoaded = new List<FactionDef>();
    internal static Dictionary<string, string> pawnKindRaceDefRelations = new Dictionary<string, string>();

    internal static Dictionary<string, List<BackstoryCategoryFilter>> defaultFactionBackstorySettings =
        new Dictionary<string, List<BackstoryCategoryFilter>>();

    internal static Dictionary<string, PrevKindSettings> defaultKindBackstorySettings =
        new Dictionary<string, PrevKindSettings>();

    internal class PrevKindSettings
    {
        public List<string> prevPawnkindBackstoryCategories = null;
        public List<BackstoryCategoryFilter> prevPawnkindBackstoryCategoryFilters = null;
    }
}