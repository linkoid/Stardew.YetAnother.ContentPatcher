using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using System.Collections.Generic;
using System;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

internal class YamlContentConfig
{
	/// <summary>The format version.</summary>
	public ISemanticVersion? Format { get; init; }

	/// <summary>The user-defined tokens whose values may depend on other tokens.</summary>
	public YamlDynamicTokenConfig[]? DynamicTokens { get; init; }

	/// <summary>The user-defined alias token names.</summary>
	public InvariantDictionary<string?>? AliasTokenNames { get; init; }

	/// <summary>The custom locations to add to the game.</summary>
	public YamlCustomLocationConfig?[]? CustomLocations { get; init; }

	/// <summary>The changes to make.</summary>
	public YamlPatchConfig[]? Changes { get; private set; }

	/// <summary>The schema for the <c>config.json</c> file (if any).</summary>
	public InvariantDictionary<YamlConfigSchemaFieldConfig?>? ConfigSchema { get; init; }

	public YamlContentConfig() { }

	public ContentConfig ToContentConfig()
	{
		return new ContentConfig(
			Format,
			DynamicTokens?.Select(static x => x?.ToDynamicTokenConfig()).ToArray(),
			AliasTokenNames,
			CustomLocations?.Select(static x => x?.ToCustomLocationConfig()).ToArray(),
			Changes?.Select(static x => x.ToPatchConfig()).ToArray(),
			new(ConfigSchema?.Select(static x => KeyValuePair.Create(x.Key!, x.Value.ToConfigSchemaFieldConfig()))
				?? Array.Empty<KeyValuePair<string, ConfigSchemaFieldConfig?>>())
		);
	}
}
