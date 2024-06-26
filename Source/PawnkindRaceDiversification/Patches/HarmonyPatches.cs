﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PawnkindRaceDiversification.Patches;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
    static HarmonyPatches()
    {
        //Pawn generation hijacker
        Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", [
            typeof(PawnGenerationRequest)
        ]), typeof(PawnkindGenerationHijacker).GetMethod("DetermineRace"));
        //World related settings
        Patch(AccessTools.Method(typeof(WorldGenerator), "GenerateWorld"),
            typeof(WorldRelatedPatches).GetMethod("OnGeneratingWorld"));
        Patch(AccessTools.Method(typeof(Page_CreateWorldParams), "DoWindowContents", [
                typeof(Rect)
            ]),
            null, null, typeof(WorldRelatedPatches).GetMethod("WorldWeightSettingsInWorldPage"));
        //World params will reset on CreateWorldParams resetting, ConfigureStartingPawns going next,
        //  or from entering the main menu.
        Patch(AccessTools.Method(typeof(Page_CreateWorldParams), "Reset"),
            null, typeof(WorldParamsReset).GetMethod("OnResetCreateWorldParams"));
        Patch(AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DoNext"),
            typeof(WorldParamsReset).GetMethod("OnResetCreateWorldParams"));
        Patch(AccessTools.Method(typeof(GameDataSaveLoader), "LoadGame", [typeof(string)]),
            typeof(WorldParamsReset).GetMethod("OnResetCreateWorldParams"));
    }

    private static Harmony harmony => PawnkindRaceDiversification.harmony;

    internal static void PostInitPatches()
    {
        //Altered Carbon
        //ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.ALTERED_CARBON, "CustomizeSleeveWindow", "GetNewPawn",
        //    null,
        //    typeof(AnyModGeneratedPawn).GetMethod("OnModGeneratingPawn"));
        //Prepare Carefully
        ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.PREPARE_CAREFULLY, "ColonistSaver", "SaveToFile", null,
            null, null, typeof(PrepareCarefullyTweaks).GetMethod("SavingMethodInsertionTranspiler"));
        ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.PREPARE_CAREFULLY, "ColonistLoader", "LoadFromFile",
            null,
            null, null, typeof(PrepareCarefullyTweaks).GetMethod("LoadingMethodInsertionTranspiler"));
        ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.PREPARE_CAREFULLY, "ExtensionsPawn", "Copy", null,
            typeof(PrepareCarefullyTweaks).GetMethod("PawnPreCopy"),
            typeof(PrepareCarefullyTweaks).GetMethod("PawnPostCopy"));
        ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.PREPARE_CAREFULLY, "CustomPawn", "InitializeWithPawn",
            null,
            typeof(PrepareCarefullyTweaks).GetMethod("OnInitializeNewPawn"));
        //Chjee's Androids
        ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.ANDROIDS, "DroidUtility", "MakeDroidTemplate", null,
            null, null, typeof(ChjeeDroidFixes).GetMethod("PawnHostilitySettingFix"));
        //Character Editor
        //ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.CHARACTER_EDITOR, "PresetPawn", "GeneratePawn", null,
        //    typeof(AnyModGeneratedPawn).GetMethod("OnModGeneratingPawn"));
        //ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod.CHARACTER_EDITOR, "PawnxTool",
        //    "ReplacePawnWithPawnOfSameRace", null,
        //    typeof(AnyModGeneratedPawn).GetMethod("OnModGeneratingPawn"));
    }

    private static void ApplyPatchIntoMod(PawnkindRaceDiversification.SeekedMod modToPatch, string className,
        string targetMethod, Type[] parameters = null,
        MethodInfo prefixMethod = null,
        MethodInfo postfixMethod = null,
        MethodInfo transpiler = null,
        MethodInfo finalizer = null)
    {
        if (PawnkindRaceDiversification.activeSeekedMods?.Contains(modToPatch) == false)
        {
            return;
        }

        //If this specific method is called, then Altered Carbon generated a pawn. We don't want to touch
        //  this pawn.
        var a = PawnkindRaceDiversification.referencedModAssemblies[modToPatch];
        Patch(AccessTools.Method(a.GetTypes().First(t => t.Name == className), targetMethod, parameters),
            prefixMethod, postfixMethod, transpiler, finalizer);
    }

    //A more straightforward way to patch things.
    private static void Patch(MethodInfo methodToPatch,
        MethodInfo prefixMethod = null, MethodInfo postfixMethod = null,
        MethodInfo transpiler = null, MethodInfo finalizer = null)
    {
        //Set up basic method patches
        var prem = prefixMethod != null ? new HarmonyMethod(prefixMethod) : null;
        var pom = postfixMethod != null ? new HarmonyMethod(postfixMethod) : null;
        var trans = transpiler != null ? new HarmonyMethod(transpiler) : null;
        var fin = finalizer != null ? new HarmonyMethod(finalizer) : null;
        //Use harmony to manually patch the given method in the given type
        //Logger.Message("Patching " + type.Name + "...");
        harmony.Patch(methodToPatch, prem, pom, trans, fin);
    }

    //Patch all classes that inherit from and have overrides on a certain method (in one assembly).
    private static void MultipatchInherited(Type masterType, IEnumerable<Assembly> otherAssemblies,
        string methodToPatch,
        MethodInfo prefixMethod = null, MethodInfo postfixMethod = null,
        MethodInfo transpiler = null, MethodInfo finalizer = null)
    {
        var typeAssembly = Assembly.GetAssembly(masterType);
        var classes = new List<Type>();
        //Look through each inherited class in this type's assembly
        foreach (var type in
                 typeAssembly.GetTypes()
                     .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(masterType)))
        {
            classes.Add(type);
        }

        //Look for this inherited class in other assemblies (if provided)
        foreach (var otherAssembly in otherAssemblies)
        {
            foreach (var type in
                     otherAssembly.GetTypes()
                         .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(masterType)))
            {
                classes.Add(type);
            }
        }

        //Patch each class found
        foreach (var c in classes)
        {
            //Make sure that this method originates from this type
            if (c.GetMethod(methodToPatch)?.DeclaringType == c)
            {
                Patch(c.GetMethod(methodToPatch), prefixMethod, postfixMethod, transpiler, finalizer);
            }
        }
    }
}