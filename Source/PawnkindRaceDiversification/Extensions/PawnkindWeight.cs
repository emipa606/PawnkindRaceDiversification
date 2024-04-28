using System.Collections.Generic;
using RimWorld;

namespace PawnkindRaceDiversification.Extensions;

public sealed class PawnkindWeight
{
    public readonly bool overrideBackstories = false;
    public List<string> backstoryCategories;
    public List<BackstoryCategoryFilter> backstoryFilters;
    public string pawnKindDef;

    public float weight;
}