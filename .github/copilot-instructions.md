# GitHub Copilot Instructions for RimWorld Modding - Pawnkind Race Diversification (Continued)

Welcome to the GitHub Copilot Instructions for the Pawnkind Race Diversification (Continued) mod. This document provides guidelines to help you understand the mod's purpose, key features, coding patterns, XML integration, Harmony patching, and suggestions for effective use of GitHub Copilot in your modding projects.

## 1. Mod Overview and Purpose

**Mod Name:** Pawnkind Race Diversification (Continued)  
**Author:** Solid E. Wires  
**Description:**  
This mod aims to simplify the process of race diversification in RimWorld by allowing modders to seamlessly fit races into existing pawnkind definitions. It eliminates the need for creating additional pawnkinds for each race and instead provides a more flexible way to adjust race weights and generation settings.

## 2. Key Features and Systems

### 2.1 Race Weight Customization
- **General Settings:** Adjust the weight of races for all games.
- **Per New World Settings:** Set weights for new worlds, which are applied to local save weights.
- **Local Save Settings:** Adjust weights specific to a particular save or world.

### 2.2 Mod Extensions
- **RaceDiversificationPool:** Patch this extension to adjust generation weight values for races.
- **RaceRandomizationExcluded:** Prevent specific pawnkinds from being overwritten by the mod.

### 2.3 Backstory Customization
- Modders can add backstories to pawns generated through the mod extension, similar to how factions and pawnkinds do it.

### 2.4 Reducing Redundancies
- This mod helps reduce the need for duplicate pawnkinds, allowing specialized pawns (e.g., mercenaries) to be added to custom racial factions without being strictly human.

## 3. Coding Patterns and Conventions

### 3.1 C# Files
The mod includes several C# files that handle various aspects of race diversification. Key files include:
- **GeneralLoadingDatabase.cs:** Manages general loading settings.
- **ExtensionDatabase.cs:** Handles mod extensions.
- **FactionWeight.cs:** Manages faction weight settings.
- **PawnkindWeight.cs:** Manages pawnkind weight settings.
- **RaceDiversificationPool.cs:** Allows adjustment of generation weight values.

### 3.2 XML Files
XML files are crucial for defining mod settings and dependencies. Key files include:
- **About.xml:** Contains mod metadata and dependencies.
- **AndroidTiers.xml, AnthromorphRaces.xml, ChJeeAndroids.xml:** Example patches for specific races.

### 3.3 Naming Conventions
- Use descriptive and consistent naming conventions for classes, methods, and variables.
- Follow RimWorld's conventions for XML and C# code.

## 4. XML Integration

### 4.1 Defs and Dependencies
- **Def Files:** XML files define various aspects of the mod, such as faction definitions and pawnkind definitions.
- **Dependencies:** Ensure all required dependencies are listed in the `About.xml` file. For example:
  xml
  <dependencies>
    <dependency>erdelf.HumanoidAlienRaces</dependency>
    <dependency>UnlimitedHugs.HugsLib</dependency>
  </dependencies>
  

### 4.2 Example Patch
Here's an example of an XML patch for the AndroidTiers race:
xml
<Defs>
  <FactionDef Name="AndroidTiers" factionGraphicPath="AndroidTiers/AndroidTiers" description="Android Tiers faction." />
  <PawnKindDef Name="AndroidTiers" race="AndroidTiers" />
</Defs>


## 5. Harmony Patching

### 5.1 Basics of Harmony
Harmony is used to patch existing RimWorld code without modifying it directly. This allows for greater flexibility and compatibility with other mods.

### 5.2 Example Harmony Patch
Here's an example of a Harmony patch in C#:
csharp
using HarmonyLib;
using RimWorld;

public class ExamplePatch
{
    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn))]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        public static void Prefix(PawnGenerationRequest request)
        {
            // Your custom logic here
        }
    }
}


### 5.3 Applying Harmony Patches
- Ensure that the Harmony library is included in your mod's dependencies.
- Apply patches in the `ModEntry.cs` or similar initialization file.

## 6. Suggestions for Copilot

### 6.1 XML Snippets
- Use Copilot to generate XML snippets for defs and dependencies. For example:
  xml
  <Def>
    <Name>ExampleDef</Name>
    <Type>ExampleType</Type>
    <Description>Example description.</Description>
  </Def>
  

### 6.2 Harmony Patches
- Use Copilot to generate Harmony patch snippets. For example:
  csharp
  using HarmonyLib;

  public class ExamplePatch
  {
      [HarmonyPatch(typeof(TargetingParameters), nameof(TargetingParameters.CanTarget))]
      public static class TargetingParameters_CanTarget_Patch
      {
          public static void Postfix(ref bool __result, Thing target, Pawn caster)
          {
              // Your custom logic here
          }
      }
  }
  

### 6.3 C# Code Snippets
- Use Copilot to generate C# code snippets for various tasks, such as:
  - Initializing mod extensions.
  - Modifying pawn weights.
  - Adding backstories to pawns.

## 7. Additional Resources

- **RimWorld Documentation:** [RimWorld Wiki](https://rimworldwiki.com/wiki/RimWorld_Wiki)
- **Harmony Documentation:** [Harmony GitHub](https://github.com/pardeike/Harmony)
- **RimWorld Modding Forum:** [RimWorld Modding Forum](https://rimworld.subforum.net/)

## 8. License

This mod is licensed under the MIT license. You are allowed to use this in your modpacks.

## 9. Support

For best support, please use the Discord-channel for error-reporting. Do not report errors by making a discussion-thread, as the mod author may not receive notifications.

## 10. Conclusion

By following these guidelines, you can effectively utilize GitHub Copilot to enhance your modding projects for Pawnkind Race Diversification (Continued). Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

