using HarmonyLib;
using Linkoid.Stardew.YetAnother.ContentPatcher.Framework;
using StardewModdingAPI;
using ContentPatcher.Framework;
using System;

namespace Linkoid.Stardew.YetAnother.ContentPatcher;

internal class YetAnotherContentPatcherMod : Mod
{
	public static YetAnotherContentPatcherMod Instance { get; private set; }
	public static new IMonitor Monitor => (Instance as Mod).Monitor;

	public override void Entry(IModHelper helper)
	{
		Instance = this;

		var harmony = new Harmony(ModManifest.UniqueID);
		harmony.Patch(AccessTools.DeclaredMethod(typeof(RawContentPack), nameof(RawContentPack.TryReloadContent)),
			postfix: new(((Delegate)RawYamlContentPack.TryReloadContent_Postfix).Method));
	}
}
