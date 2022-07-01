using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlienRace;
using HarmonyLib;
using HugsLib;
using HugsLib.Utils;
using PawnkindRaceDiversification.Extensions;
using PawnkindRaceDiversification.Handlers;
using RimWorld;
using Verse;
using static PawnkindRaceDiversification.Data.GeneralLoadingDatabase;
using static PawnkindRaceDiversification.Extensions.ExtensionDatabase;
using HarmonyPatches = PawnkindRaceDiversification.Patches.HarmonyPatches;

namespace PawnkindRaceDiversification;

public class PawnkindRaceDiversification : ModBase
{
    internal static int versionID = 35;
    internal static readonly List<SeekedMod> activeSeekedMods = new List<SeekedMod>();

    private static readonly Dictionary<string, SeekedMod> seekedModAssemblies = new Dictionary<string, SeekedMod>
    {
        { "Pawnmorph", SeekedMod.PAWNMORPHER },
        { "AlteredCarbon", SeekedMod.ALTERED_CARBON },
        { "EdBPrepareCarefully", SeekedMod.PREPARE_CAREFULLY },
        { "Androids", SeekedMod.ANDROIDS },
        { "CharacterEditor", SeekedMod.CHARACTER_EDITOR }
    };

    internal static readonly Dictionary<SeekedMod, Assembly> referencedModAssemblies =
        new Dictionary<SeekedMod, Assembly>();

    private PawnkindRaceDiversification()
    {
        Instance = this;
    }

    internal static PawnkindRaceDiversification Instance { get; private set; }
    internal static Harmony harmony => new Harmony("SEW_PRD_Harmony");
    internal ModSettingsHandler SettingsHandler { get; private set; }

    public override string ModIdentifier => "PawnkindRaceDiversification";

    protected override bool HarmonyAutoPatch => false;

    private ModLogger GetLogger => base.Logger;
    internal static ModLogger Logger => Instance.GetLogger;

    public static bool IsDebugModeInSettingsActive()
    {
        return ModSettingsHandler.DebugMode.Value;
    }

    internal static void LogValues(params object[] values)
    {
        if (values.Length > 0)
        {
            var ind = "\n\t";
            var msg = "";
            msg += "Value output: " + ind;
            foreach (var v in values)
            {
                try
                {
                    msg += v.GetType().Name + ": " + v + ind;
                }
                catch (Exception)
                {
                    msg += "Value errored" + ind;
                }
            }

            Logger.Message(msg);
        }
        else
        {
            Logger.Warning("No values to output.");
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        //Print the version of PRD
        Logger.Message("Initialized PRD version " + versionID);

        //Find all active mods that this mod seeks.
        var mods = HugsLibUtility.GetAllActiveAssemblies().ToList();
        foreach (var m in mods)
        {
            //Logger.Message(m.GetName().Name);
            var successful = seekedModAssemblies.TryGetValue(m.GetName().Name, out var modFound);
            if (!successful)
            {
                continue;
            }

            activeSeekedMods.Add(modFound);
            referencedModAssemblies.Add(modFound, m);
        }

        HarmonyPatches.PostInitPatches();
    }

    public override void DefsLoaded()
    {
        base.DefsLoaded();
        if (!ModIsActive)
        {
            return;
        }

        //Finds def of all races currently loaded (courtesy goes to DubWise)
        //  Also selects race settings to cherry pick a few things off of it
        //  and looks through all loaded pawnkind defs to reassign its defaults after modifying them at runtime.
        var raceNames = new List<string>();
        var alienRaceDefs = (from x in DefDatabase<ThingDef_AlienRace>.AllDefs
            where x.race != null
            select x).ToList();
        var raceSettingsDefs = (from x in DefDatabase<RaceSettings>.AllDefs
            select x).ToList();
        var kindDefs = (from x in DefDatabase<PawnKindDef>.AllDefs
            select x).ToList();
        var factionDefs = (from x in DefDatabase<FactionDef>.AllDefs
            select x).ToList();
        //Search through all alien race defs
        foreach (var def in alienRaceDefs)
        {
            //Know the file name of the def, this helps a lot sometimes
            var fileName = "";
            if (def.fileName != null)
            {
                fileName = def.fileName.Contains('.')
                    ? def.fileName.Substring(0, def.fileName.IndexOf('.'))
                    : def.fileName;
            }

            //Pawnmorpher compatibility
            if (activeSeekedMods.Contains(SeekedMod.PAWNMORPHER))
            {
                //Implied defs are automatically skipped...
                //  Y'know, it would be a lot of work to patch ALL that author's implied defs!
                if (fileName == "ImpliedDefs"
                    || fileName == "Cobra_Hybrid")
                {
                    impliedRacesLoaded.Add(def.defName);
                    continue;
                }
            }

            //Add this race to the databases
            raceNames.Add(def.defName);
            racesLoaded.Add(def.defName, def);

            //Style settings (made obsolete thanks to HAR)
            /*
                foreach (KeyValuePair<Type, StyleSettings> style in def.alienRace?.styleSettings)
                    GeneralLoadingDatabase.AddOrInsertStyle(def.defName, style.Key, style.Value);
                */

            //Get all values from extensions
            var ext = def.GetModExtension<RaceDiversificationPool>();
            if (ext == null)
            {
                continue;
            }

            //You can exclude a race from being modified in the settings by making the flat generation weight negative (-1)
            if (ext.flatGenerationWeight < 0.0f)
            {
                raceNames.Remove(def.defName);
            }
            else
            {
                racesDiversified.Add(def.defName, ext);
            }

            //Logger.Message("Def loaded: " + def.defName + ", extension values logged after");
            //LogValues(ext.factionWeights[0].faction.defName, ext.pawnKindWeights[0].pawnkind.defName, ext.flatGenerationWeight);
        }

        //Remove irrelevant race settings
        foreach (var s in raceSettingsDefs)
        {
            //I was a complete buffoon for trying to remove these. Just made their chances 0% instead.
            var slaveKindEntries = (from w in s.pawnKindSettings.alienslavekinds
                where w.chance > 0
                select w).ToList();
            var refugeeKindEntries = (from w in s.pawnKindSettings.alienrefugeekinds
                where w.chance > 0
                select w).ToList();
            foreach (var slaveKind in slaveKindEntries)
            {
                slaveKind.chance = 0.0f;
            }

            foreach (var refugeeKind in refugeeKindEntries)
            {
                refugeeKind.chance = 0.0f;
            }

            var startingColonistEntries = (from w in s.pawnKindSettings.startingColonists
                where w.factionDefs.Count > 0
                select w).ToList();
            var wandererEntries = (from w in s.pawnKindSettings.alienwandererkinds
                where w.factionDefs.Count > 0
                select w).ToList();
            /*  So I didn't completely remove these race settings for two reasons:
                 *      1.) Some race mods want these so that they have different pawnkind varieties for their
                 *          specific faction races.
                 *      2.) It would be destructive and barbaric to assume that ALL race mods bother the player
                 *          colony factions.
                 *  Therefore, all this does is remove the player factions from race settings that try to
                 *  modify it. Settings without factions specified don't do anything (therefore, this is a
                 *  safe procedure).
                 * */
            foreach (var e in startingColonistEntries)
            {
                e.factionDefs.RemoveAll(f => f.defName == "PlayerColony" || f.defName == "PlayerTribe");
            }

            foreach (var e in wandererEntries)
            {
                e.factionDefs.RemoveAll(f => f.defName == "PlayerColony" || f.defName == "PlayerTribe");
            }
        }

        //Look through all existing pawnkind defs
        foreach (var def in kindDefs)
        {
            pawnKindRaceDefRelations.Add(def.defName, def.race.defName);
            if (def.GetModExtension<RaceRandomizationExcluded>() != null)
            {
                pawnKindDefsExcluded.Add(def.defName);
            }

            //Backstory database for pawnkinds
            var kindBackstorySettings = new PrevKindSettings
            {
                prevPawnkindBackstoryCategoryFilters = def.backstoryFilters,
                prevPawnkindBackstoryCategories = def.backstoryCategories
            };
            defaultKindBackstorySettings.Add(def.defName, kindBackstorySettings);
        }

        foreach (var def in factionDefs)
        {
            //Backstory database for factions
            defaultFactionBackstorySettings.Add(def.defName, def.backstoryFilters);

            //Add this to the list of humanlike factions if humanlike and not a player.
            if (def.humanlikeFaction && !def.isPlayer)
            {
                factionsWithHumanlikesLoaded.Add(def);
            }
        }

        SettingsHandler = new ModSettingsHandler();
        SettingsHandler.PrepareSettingHandles(Instance.Settings, raceNames);
    }

    internal enum SeekedMod
    {
        NONE,
        PAWNMORPHER,
        ALTERED_CARBON,
        PREPARE_CAREFULLY,
        ANDROIDS,
        CHARACTER_EDITOR
    }
}