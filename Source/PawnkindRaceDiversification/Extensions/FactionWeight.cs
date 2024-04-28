using System.Collections.Generic;
using RimWorld;

namespace PawnkindRaceDiversification.Extensions;

public sealed class FactionWeight
{
    public readonly bool overrideBackstories = false;
    public List<string> backstoryCategories;
    public List<BackstoryCategoryFilter> backstoryFilters;
    public string factionDef;

    public float weight;
}