﻿using System.Collections.Generic;
using RimWorld;

namespace PawnkindRaceDiversification.Data;

internal sealed class GeneralLoadingDatabase
{
    internal static readonly List<string> impliedRacesLoaded = [];
    internal static readonly List<string> pawnKindDefsExcluded = [];
    internal static readonly List<FactionDef> factionsWithHumanlikesLoaded = [];
    internal static readonly Dictionary<string, string> pawnKindRaceDefRelations = new Dictionary<string, string>();

    internal static readonly Dictionary<string, List<BackstoryCategoryFilter>> defaultFactionBackstorySettings =
        new Dictionary<string, List<BackstoryCategoryFilter>>();

    internal static readonly Dictionary<string, PrevKindSettings> defaultKindBackstorySettings =
        new Dictionary<string, PrevKindSettings>();

    internal class PrevKindSettings
    {
        public List<string> prevPawnkindBackstoryCategories = null;
        public List<BackstoryCategoryFilter> prevPawnkindBackstoryCategoryFilters = null;
    }
}