using ContentPatcher;
using ContentPatcher.Framework;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Patches;
using HarmonyLib;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Linkoid.Stardew.YetAnother.ContentPatcher.YetAnotherContentPatcherMod;

namespace Linkoid.Stardew.YetAnother.ContentPatcher;

internal static class GetContentPacksPatches
{
	internal static void PatchWith(Harmony harmony)
	{
		var moveNext = AccessTools.EnumeratorMoveNext(
			AccessTools.DeclaredMethod(typeof(ModEntry), nameof(ModEntry.GetContentPacks)));

		try
		{
			var exampleMoveNext = AccessTools.EnumeratorMoveNext(
				AccessTools.DeclaredMethod(typeof(GetContentPacksPatches), nameof(EXAMPLE_2))
			);

			// This errors
			//harmony.Patch(exampleMoveNext,
			//	transpiler: new HarmonyMethod(typeof(GetContentPacksPatches), nameof(GetContentPacksEnumerator_MoveNext_Transpiler2)));

			// This errors too.
			harmony.CreateReversePatcher(exampleMoveNext,
				new HarmonyMethod(typeof(GetContentPacksPatches), nameof(GetContentPacksEnumerator_MoveNext_ReversePatch)))
				.Patch();
		}
		catch (InvalidProgramException ex)
		{
			Monitor.Log(ex.ToString(), LogLevel.Error);
			;
		}

		try
		{
			// This errors
			harmony.Patch(moveNext);

			// This errors too.
			//harmony.CreateReversePatcher(moveNext,
			//	new HarmonyMethod(typeof(GetContentPacksPatches), nameof(GetContentPacksEnumerator_MoveNext_ReversePatch)))
			//	.Patch();
		}
		catch (InvalidProgramException ex)
		{
			Monitor.Log(ex.ToString(), LogLevel.Error);
			;
		}
		

		//harmony.Patch(moveNext,
		//	prefix: new HarmonyMethod(typeof(GetContentPacksPatches), nameof(GetContentPacksEnumerator_MoveNext_Prefix)));
		//	//transpiler: new HarmonyMethod(typeof(GetContentPacksPatches), nameof(GetContentPacksEnumerator_MoveNext_Transpiler)));
	}


	[HarmonyDebug]
	static void GetContentPacksEnumerator_MoveNext_Prefix()
	{

	}

	[HarmonyDebug]
	static IEnumerable<CodeInstruction> GetContentPacksEnumerator_MoveNext_Transpiler(MethodBase targetMethod, IEnumerable<CodeInstruction> instructions)
	{
		var method_IContentPackHelper_GetOwned = AccessTools.DeclaredMethod(typeof(IContentPackHelper), nameof(IContentPackHelper.GetOwned)); // ((Delegate)Helper.ContentPacks.GetOwned).Method.;
		var smethod_ConcatOwnedContentPacks = AccessTools.DeclaredMethod(typeof(GetContentPacksPatches), nameof(ConcatOwnedContentPacks));

		CodeMatcher codeMatcher = new CodeMatcher(instructions);

		// Convert:
		//    ???.GetOwned()
		// To:
		//    ConcatOwnedContentPacks(???.GetOwned())
		codeMatcher.MatchEndForward(
			new CodeMatch(OpCodes.Callvirt, method_IContentPackHelper_GetOwned)
		);
		codeMatcher.AssertValid(targetMethod);
		codeMatcher.Advance(1);
		codeMatcher.Insert(
			new CodeInstruction(OpCodes.Call, smethod_ConcatOwnedContentPacks)
		);
		codeMatcher.AssertValid(targetMethod);

		return codeMatcher.InstructionEnumeration();
	}

	[HarmonyDebug]
	static IEnumerable<CodeInstruction> GetContentPacksEnumerator_MoveNext_Transpiler2(MethodBase targetMethod, IEnumerable<CodeInstruction> instructions)
	{
		var method_IContentPackHelper_GetOwned = AccessTools.DeclaredMethod(typeof(IContentPackHelper), nameof(IContentPackHelper.GetOwned)); // ((Delegate)Helper.ContentPacks.GetOwned).Method.;
		var smethod_GetOwnedAndShared = AccessTools.DeclaredMethod(typeof(GetContentPacksPatches), nameof(GetOwnedAndSharedEnumerator));

		CodeMatcher codeMatcher = new CodeMatcher(instructions);

		// Convert:
		//    ???.GetOwned()
		// To:
		//    ConcatOwnedContentPacks(???.GetOwned())
		codeMatcher.MatchStartForward(
			new CodeMatch(OpCodes.Callvirt, method_IContentPackHelper_GetOwned)
		);
		codeMatcher.AssertValid(targetMethod);
		codeMatcher.RemoveInstruction();
		codeMatcher.Set(OpCodes.Call, smethod_GetOwnedAndShared);

		return codeMatcher.InstructionEnumeration();
	}

	[HarmonyDebug]
	static bool GetContentPacksEnumerator_MoveNext_ReversePatch(object _)//object __instance)
	{
		throw new NotImplementedException();
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
	private static IEnumerable<object> EXAMPLE_2(List<object> enumerable)
	{
		foreach (var _ in enumerable)
		{
			yield return null;
		}
	}

	static IEnumerable<IContentPack> ConcatOwnedContentPacks(IEnumerable<IContentPack> contentPacks)
	{
		return contentPacks.Concat(Helper.ContentPacks.GetOwned());
	}

	static IEnumerable<IContentPack> GetOwnedAndShared(this IContentPackHelper contentPackHelper)
	{
		return contentPackHelper.GetOwned().Concat(Helper.ContentPacks.GetOwned());
	}

	static IEnumerator<IContentPack> GetOwnedAndSharedEnumerator(this IContentPackHelper contentPackHelper)
	{
		return contentPackHelper.GetOwned().Concat(Helper.ContentPacks.GetOwned()).GetEnumerator();
	}
}
