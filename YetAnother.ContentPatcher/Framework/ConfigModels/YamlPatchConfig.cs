using ContentPatcher.Framework.ConfigModels;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

/// <summary>The input settings for a patch from the configuration file.</summary>
internal class YamlPatchConfig
{
	/*********
	** Accessors
	*********/
	/****
	** All actions
	****/
	/// <summary>A name for this patch shown in log messages.</summary>
	public string? LogName { get; set; }

	/// <summary>The patch type to apply.</summary>
	public string? Action { get; set; }

	/// <summary>The asset key to change.</summary>
	public string? Target { get; set; }

	/// <summary>Indicates when a patch should be updated.</summary>
	public string? Update { get; set; }

	/// <summary>The local file to load.</summary>
	public string? FromFile { get; set; }

	/// <summary>Whether to apply this patch.</summary>
	/// <remarks>This must be a string to support config tokens.</remarks>
	public string? Enabled { get; set; }

	/// <summary>The criteria to apply. See readme for valid values.</summary>
	public InvariantDictionary<string?> When { get; init; } = new();

	/// <summary>The priority for this patch when multiple patches apply.</summary>
	public string? Priority { get; set; }

	/****
	** Multiple actions
	****/
	/// <summary>The text operations to apply.</summary>
	public List<TextOperationConfig?> TextOperations { get; init; } = new();

	/****
	** EditImage
	****/
	/// <summary>The sprite area from which to read an image.</summary>
	public YamlPatchRectangleConfig? FromArea { get; set; }

	/// <summary>The sprite area to overwrite.</summary>
	public YamlPatchRectangleConfig? ToArea { get; set; }

	/// <summary>Indicates how the image should be patched.</summary>
	public string? PatchMode { get; set; }

	/****
	** EditData
	****/
	/// <summary>The data records to edit.</summary>
	public InvariantDictionary<JToken?> Entries { get; init; } = new();

	/// <summary>The individual fields to edit in data records.</summary>
	public InvariantDictionary<InvariantDictionary<JToken?>?> Fields { get; init; } = new();

	/// <summary>The records to reorder, if the target is a list asset.</summary>
	public List<PatchMoveEntryConfig?> MoveEntries { get; init; } = new();

	/// <summary>The field within the data asset to which edits should be applied, or empty to apply to the root asset.</summary>
	public List<string> TargetField { get; init; } = new();

	/****
	** EditMap
	****/
	/// <summary>The map properties to edit.</summary>
	public InvariantDictionary<string?> MapProperties { get; init; } = new();

	/// <summary>The warps to add to the location.</summary>
	public List<string?> AddWarps { get; init; } = new();

	/// <summary>The map tiles to edit.</summary>
	public List<PatchMapTileConfig?> MapTiles { get; init; } = new();


	/*********
	** Public methods
	*********/
	/// <summary>Construct an instance.</summary>
	[JsonConstructor]
	public YamlPatchConfig() { }

	/// <summary>Construct an instance.</summary>
	public PatchConfig ToPatchConfig()
	{
		PatchConfig patchConfig = new();

		// all actions
		patchConfig.LogName = this.LogName;
		patchConfig.Action = this.Action;
		patchConfig.Target = this.Target;
		patchConfig.Update = this.Update;
		patchConfig.FromFile = this.FromFile;
		patchConfig.Enabled = this.Enabled;
		patchConfig.When.AddRange(this.When);
		patchConfig.Priority = this.Priority;

		// multiple actions
		patchConfig.TextOperations.AddRange(this.TextOperations.Select(p => p != null ? new TextOperationConfig(p) : null));

		// EditImage
		patchConfig.FromArea = this.FromArea?.ToPatchRectangleConfig();
		patchConfig.ToArea = this.ToArea?.ToPatchRectangleConfig();
		patchConfig.PatchMode = this.PatchMode;

		// EditData
		patchConfig.Entries.AddRange(this.Entries.Clone(value => value?.DeepClone()));
		patchConfig.Fields.AddRange(this.Fields.Clone(
			entryFields => entryFields?.Clone(value => value?.DeepClone())
		));
		patchConfig.MoveEntries.AddRange(this.MoveEntries.Select(p => p != null ? new PatchMoveEntryConfig(p) : null));
		patchConfig.TargetField.AddRange(this.TargetField);

		// EditMap
		patchConfig.MapProperties.AddRange(this.MapProperties);
		patchConfig.AddWarps.AddRange(this.AddWarps);
		patchConfig.MapTiles.AddRange(this.MapTiles.Select(p => p != null ? new PatchMapTileConfig(p) : null));

		return patchConfig;
	}
}