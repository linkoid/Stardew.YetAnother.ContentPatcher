using ContentPatcher;
using ContentPatcher.Framework;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Patches.EditData;
using HarmonyLib;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Pathoschild_ContentPatcher = ContentPatcher;

namespace Linkoid.Stardew.YetAnother.ContentPatcher;

internal class YetAnotherContentPatcherMod : Mod
{
	public static YetAnotherContentPatcherMod Instance { get; private set; }
	public static new IMonitor Monitor => (Instance as Mod).Monitor;
	public static new IModHelper Helper => (Instance as Mod).Helper;

	private static Harmony harmony;

	public override void Entry(IModHelper helper)
	{
		Instance = this;

		harmony = new Harmony(ModManifest.UniqueID);
		//YetAnotherContentPatcherMod.PatchWith(harmony);
		//GetContentPacksPatches.PatchWith(harmony);
		ContentPackHelperPatches.PatchWith(harmony);
		Framework.RawContentPackPatches.PatchWith(harmony);
		Toolkit.Framework.ContentManagers.ModContentManagerPatches.PatchForYamlFileSupport();

		helper.ContentPacks.GetOwned();
	}

	private static void PatchWith(Harmony harmony)
	{
		Pathoschild_ContentPatcher.Framework.ConfigModels.PatchConfig _;
	}
}
