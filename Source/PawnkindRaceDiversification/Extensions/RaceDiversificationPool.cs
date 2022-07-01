using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PawnkindRaceDiversification.Extensions;

public class RaceDiversificationPool : DefModExtension
{
    public List<string> backstoryCategories;
    public List<BackstoryCategoryFilter> backstoryFilters;
    public List<FactionWeight> factionWeights;

    public float flatGenerationWeight = 0.0f;
    public bool overrideBackstories = false;

    public List<PawnkindWeight> pawnKindWeights;
}