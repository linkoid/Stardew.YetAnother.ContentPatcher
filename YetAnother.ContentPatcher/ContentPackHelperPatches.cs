using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Framework.ModHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linkoid.Stardew.YetAnother.ContentPatcher;

internal static class ContentPackHelperPatches
{
	public static void PatchWith(Harmony harmony)
	{
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ContentPackHelper), nameof(IContentPackHelper.GetOwned)),
			postfix: new(typeof(ContentPackHelperPatches), nameof(GetOwned_Postfix)));
	}

	static void GetOwned_Postfix(IContentPackHelper __instance, ref IEnumerable<IContentPack> __result)
	{
		try
		{
			if (__instance.ModID == "Pathoschild.ContentPatcher")
			{
				__result = __result.Concat(YetAnotherContentPatcherMod.Helper.ContentPacks.GetOwned());
			}
		}
		catch (Exception ex)
		{
			YetAnotherContentPatcherMod.Monitor.Log(ex.ToString(), LogLevel.Error);
		}
	}
}
