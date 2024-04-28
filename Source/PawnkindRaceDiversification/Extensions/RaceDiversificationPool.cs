using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnkindRaceDiversification.Extensions;

public class RaceDiversificationPool : DefModExtension
{
    public readonly float flatGenerationWeight = 0.0f;
    public readonly bool overrideBackstories = false;
    public List<string> backstoryCategories;
    public List<BackstoryCategoryFilter> backstoryFilters;
    public List<FactionWeight> factionWeights;

    public List<PawnkindWeight> pawnKindWeights;
}