using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Framework.ContentManagers;
using Linkoid.Stardew.YetAnother.Toolkit.Serialization;

namespace Linkoid.Stardew.YetAnother.Toolkit.Framework.ContentManagers;

public static class ModContentManagerPatches
{
	private static readonly string harmonyId = typeof(ModContentManagerPatches).FullName;

	public static void PatchForYamlFileSupport()
	{
		if (Harmony.HasAnyPatches(harmonyId))
			return;

		Harmony harmony = new Harmony(harmonyId);
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ModContentManager), nameof(ModContentManager.HandleUnknownFileType)),
			prefix: new HarmonyMethod(((Delegate)HandleUnknownFileType).Method));

	}

	static bool HandleUnknownFileType(ModContentManager __instance, IAssetName assetName, FileInfo file, Type assetType, ref object? __result)
	{
		if (file.Extension == ".yaml")
		{
			__result = YamlHelper.Instance.ReadYamlFile(file.FullName, assetType);
			return false;
		}

		return true;
	}
}
