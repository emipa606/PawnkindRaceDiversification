# PawnkindRaceDiversification

![Image](https://i.imgur.com/buuPQel.png)

Update of Solid E. Wiress mod
https://steamcommunity.com/sharedfiles/filedetails/?id=2460358429

- Will throw some errors if the alien age is not within generation range, should be okay anyway.
- Will throw errors regarding tatoo/beard/hair-styles if the race does not have these defined, there is nothing this mod can really do about it.

![Image](https://i.imgur.com/pufA0kM.png)

	
![Image](https://i.imgur.com/Z4GOv8H.png)

# Discontinued

Due to losing interest in Rimworld, and because of the overwhelming nature of this mod, I've decided to cut development to it. If anyone is interested in picking this mod back up, feel free to do so.
# Description

Ever played with a bunch of race mods and think "Gee, I really wish I could see this race more often," or "I wish I could start a playthrough on a Rimworld without humans," or even "I really wish I could use race mods, but I don't want a whole bunch of bloat from factions and pawnkinds in my game."
Well, those are the thoughts that explained why I made this mod.
I introduce you to my first mod... Well, technically not my first, but the first I published to the workshop.

What this mod does is simply allow you and other modders to seamlessly fit races into the existing pawnkind defs. Currently, pawnkinds are limited to a race / tag that only allows specifying one race. Sure, you could make an extension def off of that pawnkind, but you would just be creating another pawnkind in a game with dozens of them that specify roles in combat, defense, and trading. It gets confusing really fast when you realize that, in order to make your race show up in the game, you would have to add your pawnkind def as one of the kinds that will show up in the groups / tags of faction defs. And this can get very overwhelming and can hinder the possibilities of raider variety (which I plan to make a mod that does that).

This mod aims to simplify everything about race diversification. You could argue that HAR already has the AlienRace.RaceSettings / def to influence this a little bit... but this is underwhelmingly not enough. You could only influence the weights of slaves, refugees, wanderers, and starting pawns. I wanted to do more than that. I wanted to change what race ANY pawnkind appears as.
Modders can patch the mod extensions that I created to allow their races to appear anywhere, on specific pawnkinds, or in specific factions. You can adjust the weight settings of all races at any time.

# Feature breakdown



  - You can change the general (previously known as global), per new world, starting pawns, or local save settings of pawn weights. Per new world weights apply to local save weights every time you create a world. Local save weights are applied to your specific world, or saved game, and loaded in that playthrough's save. General settings fill in the gaps of any "empty" weight.
  - Modders can patch a new mod extension called "PawnkindRaceDiversification.Extensions.RaceDiversificationPool" and adjust generation weight values from there (demonstrated in preview).
  - Modders can also patch a one-branch mod extension to pawnkinds called "PawnkindRaceDiversification.Extensions.RaceRandomizationExcluded" to prevent pawnkinds from being overwritten (Override alien races is set to false to prevent this problem from occuring, but set that to true in your own risk!)
  - Modders can exclude races from being adjusted by setting their race flat weights to negative.
  - Modders can also add backstories to pawns generated through the mod extension (similar way as to how factions and pawnkinds do it - see source code's extension classes for more information).
  - Potential to reduce redundancies with pawnkinds from using this mod, now specialized pawns (e.g. mercenary gunners or drifters) can be added to custom racial factions without being strictly human. This mod does not do that on its own, but it adds the possibility of it for other modders.



# Current limitations identified



  - Faction weights and backstories for aliens cannot be manually assigned in settings yet.
  - Weird behavior is to be expected if aliens have their own behavior of spawning their races. Any mod that makes special pawnkinds with specific races intended should have the mod extension that I added. Otherwise, this mod will overwrite ALL alien pawns that try to generate if "Override all alien races" is set to true.
  - Just because this mod reduces the redundancies of extra pawnkinds, **DO NOT, UNDER ANY CIRCUMSTANCES, REMOVE PAWNKINDS UNLESS YOU KNOW WHAT YOU'RE DOING - THIS GOES TO MODDERS AS WELL**. Your previous saves, using the deleted pawnkinds, **will be incompatible** because **your colonists, world pawns, even faction leaders will be deleted**. Unless you want to do this dramatic change and plan to manually replace the pawns in your save, don't do it. This is not this mod's fault, but this seems to be a failsafe that the game does to prevent loading in broken pawns.



# Incompatibilities (see more on the github)



  - Mods that also generate pawns with weight generation will probably override this mod. Disable those mod's pawn generation in their settings so that this mod can do its work instead.



# Load order

[olist]
  - Harmony (required by Hugslib)
  - Hugslib
  - Humanoid Alien Races
  - Pawnkind Race Diversification (this mod)
  - all your race mods
[/olist]

# Credits

**Rimworld** is owned by Tynan Sylvester and it is thanks to him for this wonderful game.
**Android Tiers (Atlas, ARandomKiwi, and APurpleApple)** for providing this mod for testing purposes (preview images displays their androids - this mod does NOT redistribute them - If this is still a problem, please let me know).
**Anthromorph Races (AveryTheKitty, Erin, and Ravenholme)** for also providing this mod so that I can test it (preview images display their races - this mod does NOT redistribute them - if this is still a problem, please let me know).
**Brrainz** for providing the **Harmony library**. Without it, this mod wouldn't have been possible.
**Void's** source from **Character Editor** for helping me figure out how to patch the pawn generator.
**Dubwise's** source from **Bad Hygiene** for helping me identify how to access loaded defs.
**Erdelf** for providing **Human

![Image](https://i.imgur.com/PwoNOj4.png)



-  See if the the error persists if you just have this mod and its requirements active.
-  If not, try adding your other mods until it happens again.
-  Post your error-log using https://steamcommunity.com/workshop/filedetails/?id=818773962]HugsLib and command Ctrl+F12
-  For best support, please use the Discord-channel for error-reporting.
-  Do not report errors by making a discussion-thread, I get no notification of that.
-  If you have the solution for a problem, please post it to the GitHub repository.


