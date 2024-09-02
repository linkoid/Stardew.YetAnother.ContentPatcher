using ContentPatcher.Framework;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Linkoid.Stardew.YetAnother.Toolkit;
using YamlDotNet.Core;
using static Linkoid.Stardew.YetAnother.ContentPatcher.YetAnotherContentPatcherMod;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework;

[HarmonyPatch(typeof(RawContentPack))]
internal class RawContentPackPatches : RawContentPack
{
	public static void PatchWith(Harmony harmony)
	{
		harmony.Patch(AccessTools.DeclaredMethod(typeof(RawContentPack), nameof(RawContentPack.TryReloadContent)),
			postfix: new(((Delegate)TryReloadContent_Postfix).Method));
	}

	public RawContentPackPatches(RawContentPack copyFrom)
		: base(copyFrom)
	{
	}

	public RawContentPackPatches(IContentPack contentPack, int index, Func<ContentConfig, IMigration[]> getMigrations)
		: base(contentPack, index, getMigrations)
	{
	}

	[HarmonyPostfix, HarmonyPatch(nameof(TryReloadContent))]
	internal static void TryReloadContent_Postfix(RawContentPack __instance, [NotNullWhen(true)] ref string? error, ref bool __result)
	{
		try
		{
			TryReloadContent_Postfix_Safe(__instance, ref error, ref __result);
		}
		catch (Exception ex)
		{
			Monitor.Log(ex.ToString(), LogLevel.Error);
			__result = false;
		}
	}

	internal static void TryReloadContent_Postfix_Safe(RawContentPack __instance, [NotNullWhen(true)] ref string? error, ref bool __result)
	{
		if (__result == true) return;

		const string filename = "content.yaml";

		// load raw file
		ContentConfig? content;
		try
		{
			content = __instance.ContentPack.ReadYamlFile<ContentConfig>(filename);//?.ToContentConfig();
		}
		catch (YamlException ex)
		{
			//Monitor.Log($"{ex.Message}\n{ex.StackTrace}", LogLevel.Error);
			error = ex.Message;
			return;
		}

		if (content == null)
		{
			//error = $"content pack has no {filename} file";
			return;
		}

		// validate base fields
		if (content.Format == null)
		{
			error = $"content pack doesn't specify the required {nameof(ContentConfig.Format)} field.";
			return;
		}
		if (!content.Changes.Any() && !content.CustomLocations.Any())
		{
			error = $"content pack must specify the {nameof(ContentConfig.Changes)} or {nameof(ContentConfig.CustomLocations)} fields.";
			return;
		}

		// apply high-level migrations
		// patch-level migrations are applied by PatchLoader
		IRuntimeMigration migrator = new AggregateMigration(content.Format, __instance.GetMigrations(content));
		//if (!migrator.TryMigrateMainContent(content, out error))
		//{
		//	return;
		//}

		// load content
		__instance.ContentImpl = content;
		__instance.MigratorImpl = migrator;
		error = null;
		__result = true;
		return;
	}
}
