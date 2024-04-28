namespace PawnkindRaceDiversification.Patches;

public static class AnyModGeneratedPawn
{
    //Harmony manual prefix method
    public static void OnModGeneratingPawn()
    {
        PawnkindGenerationHijacker.PauseWeightGeneration();
    }
}