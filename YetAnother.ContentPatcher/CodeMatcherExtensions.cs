using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Linkoid.Stardew.YetAnother.ContentPatcher;

internal static class CodeMatcherExtensions
{
	public static void AssertValid(this CodeMatcher codeMatcher, MethodBase methodBase)
	{
		if (codeMatcher.IsValid) return;

		string error = null;
		codeMatcher.ReportFailure(methodBase, str => error = str);

		throw new Exception(error);
	}
}
