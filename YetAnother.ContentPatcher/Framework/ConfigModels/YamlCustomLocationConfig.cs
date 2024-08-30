using System;
using ContentPatcher.Framework.ConfigModels;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

internal class YamlCustomLocationConfig
{
	/// <summary>The unique location name.</summary>
	public string? Name { get; init; }

	/// <summary>The initial map file to load.</summary>
	public string? FromMapFile { get; init; }

	/// <summary>The fallback location names to migrate if no location is found matching <see cref="Name"/>.</summary>
	public string[]? MigrateLegacyNames { get; init; }


	/// <summary>Construct an instance.</summary>
	/// <param name="name">The unique location name.</param>
	/// <param name="fromMapFile">The initial map file to load.</param>
	/// <param name="migrateLegacyNames">The fallback location names to migrate if no location is found matching <paramref name="name"/>.</param>
	public CustomLocationConfig ToCustomLocationConfig()
	{
		return new CustomLocationConfig(
			name: this.Name,
			fromMapFile: this.FromMapFile,
			migrateLegacyNames: this.MigrateLegacyNames ?? Array.Empty<string>()
		);
	}
}
