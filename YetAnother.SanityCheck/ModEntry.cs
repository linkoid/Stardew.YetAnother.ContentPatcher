using HarmonyLib;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Reflection;

namespace Linkoid.Stardew.YetAnother.SanityCheck;

internal class ModEntry : Mod
{
	public override void Entry(IModHelper helper)
	{
		Harmony harmony = new Harmony(ModManifest.UniqueID);

		MethodInfo EXAMPLE_1_Enumerator_MoveNext = AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(ModEntry), nameof(EXAMPLE_1)));
		MethodInfo EXAMPLE_2_Enumerator_MoveNext = AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(ModEntry), nameof(EXAMPLE_2)));

		//Harmony.DEBUG = true;

		Monitor.Log("Patching Example 1...");
		harmony.Patch(EXAMPLE_1_Enumerator_MoveNext);
		Monitor.Log("Patched Example 1!");

		Monitor.Log("Patching Example 2...");
		harmony.Patch(EXAMPLE_2_Enumerator_MoveNext);
		Monitor.Log("Patched Example 2!");

		Harmony.DEBUG = false;
	}

	// This method can be easily patched
	private static IEnumerable<object> EXAMPLE_1(object[] enumerable)
	{
		foreach (var _ in enumerable)
		{
			yield return null;
		}
	}

	// Trying to patch this method throws an InvalidProgramException
	private static IEnumerable<object> EXAMPLE_2(IEnumerable<object> enumerable)
	{
		foreach (var _ in enumerable)
		{
			yield return null;
		}
	}
}
