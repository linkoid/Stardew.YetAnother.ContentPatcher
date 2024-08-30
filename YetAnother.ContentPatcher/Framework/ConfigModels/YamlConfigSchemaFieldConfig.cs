using ContentPatcher.Framework.ConfigModels;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

/// <summary>The raw schema for a field in the <c>config.json</c> file.</summary>
internal class YamlConfigSchemaFieldConfig
{
	/*********
	** Accessors
	*********/
	/// <summary>The comma-delimited values to allow.</summary>
	public string? AllowValues { get; init; }

	/// <summary>The default value if the field is missing or (if <see cref="AllowBlank"/> is <c>false</c>) blank.</summary>
	public string? Default { get; init; }

	/// <summary>Whether to allow blank values.</summary>
	public bool AllowBlank { get; init; }

	/// <summary>Whether the player can specify multiple values for this field.</summary>
	public bool AllowMultiple { get; init; }

	/// <summary>An optional explanation of the config field for players.</summary>
	public string? Description { get; init; }

	/// <summary>An optional section key to group related fields.</summary>
	public string? Section { get; init; }


	public ConfigSchemaFieldConfig ToConfigSchemaFieldConfig()
	{
		return new ConfigSchemaFieldConfig(
			allowValues: this.AllowValues,
			@default: this.Default,
			allowBlank: this.AllowBlank,
			allowMultiple: this.AllowMultiple,
			description: this.Description,
			section: this.Section
		);
	}
}
