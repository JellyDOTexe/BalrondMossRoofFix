# Balrond Moss Roof Fix

Fixes the moss/bush roof pieces from **Balrond Constructions** (`bush_roof_bal`, `bush_roof_icorner_bal`, `bush_roof_ocorner_bal`, `bush_roof_top_bal`) rendering as a spiky pile of solid square leaf cards instead of leafy foliage. It also fixes other Balrond pieces whose foliage uses the same material setup (39 materials in Balrond Constructions 1.5.2).

## Cause

Those materials point to a copy of the game's `Custom/Vegetation` shader that is bundled inside Balrond's asset bundle. The copy has the same name as the game's shader but is a different, older shader object, and its leaf transparency cutout does not work (seen on Linux / Vulkan). That leaves every leaf card drawn as a full square.

Balrond's shader-replacement code only swaps shaders whose names start with `Balrond/`, and its built-in moss roof repair checks the shader name only. So this copy is never replaced.

## What this mod does

After Balrond Constructions finishes setting up its pieces at world load, the mod finds every prefab ending in `_bal` that uses a `Custom/Vegetation` shader which is not the game's own. It points those materials at the game's real `Custom/Vegetation` shader and leaves every other setting on the material alone. Pieces you already built are fixed too. Nothing is saved to your world.

- Client-side only; no server install needed.
- Safe to install without Balrond Constructions (it does nothing).
- Config: `BepInEx/config/mossrooffix.balrondconstructions.cfg` → `Enabled`.

## Install

Put `BalrondMossRoofFix.dll` in `BepInEx/plugins/` (r2modman/Thunderstore installs handle this).

Tested with Balrond Constructions 1.5.2 on Valheim with BepInExPack 5.4.2360 (Linux, Vulkan).

## Unrelated issue: untranslated names

If the pieces show names like `[tag_bush_roof_ocorner_bal]`, Balrond's translation files are missing. They live in `BepInEx/config/BalrondConstructions-translation/` (English.json, etc.). Reinstalling Balrond Constructions should restore them. This mod does not affect that.

## Building from source

`src/` holds the whole plugin (one file). The project file references the game and BepInEx DLLs from a Linux Steam install (`$(HOME)/.local/share/Steam/steamapps/common/Valheim`); set `ValheimDir` to your own install path, then run `dotnet build -c Release`.
