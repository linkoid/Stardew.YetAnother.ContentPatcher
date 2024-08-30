using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentPatcher.Framework.ConfigModels;
using Newtonsoft.Json;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

/// <summary>The input settings for a Rectangle field in <see cref="PatchConfig"/>.</summary>
internal class YamlPatchRectangleConfig
{
	/*********
    ** Accessors
    *********/
	/// <summary>The X position of the rectangle.</summary>
	public string? X { get; init; }

	/// <summary>The Y position of the rectangle.</summary>
	public string? Y { get; init; }

	/// <summary>The width of the rectangle.</summary>
	public string? Width { get; init; }

	/// <summary>The height of the rectangle.</summary>
	public string? Height { get; init; }

	public PatchRectangleConfig ToPatchRectangleConfig()
	{
		return new PatchRectangleConfig(
			x: this.X,
			y: this.Y,
			width: this.Width,
			height: this.Height
		);
	}
}

