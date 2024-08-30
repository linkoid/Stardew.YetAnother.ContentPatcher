using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

internal static class DictionaryExtensions
{
	public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
	{
		foreach (var pair in keyValuePairs)
		{
			dictionary.Add(pair.Key, pair.Value);
		}
	}

	public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<(TKey, TValue)> keyValuePairs)
	{
		foreach (var pair in keyValuePairs)
		{
			dictionary.Add(pair.Item1, pair.Item2);
		}
	}
}
