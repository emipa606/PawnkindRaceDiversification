using System.Collections.Generic;
using RimWorld;

namespace PawnkindRaceDiversification.Extensions;

public sealed class FactionWeight
{
    public List<string> backstoryCategories;
    public List<BackstoryCategoryFilter> backstoryFilters;
    public string factionDef;
    public bool overrideBackstories = false;

    public float weight;
}