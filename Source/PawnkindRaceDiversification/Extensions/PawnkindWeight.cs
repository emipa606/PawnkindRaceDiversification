using System.Collections.Generic;
using RimWorld;

namespace PawnkindRaceDiversification.Extensions;

public sealed class PawnkindWeight
{
    public List<string> backstoryCategories;
    public List<BackstoryCategoryFilter> backstoryFilters;
    public bool overrideBackstories = false;
    public string pawnKindDef;

    public float weight;
}