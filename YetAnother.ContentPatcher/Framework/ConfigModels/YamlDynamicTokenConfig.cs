using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;

namespace Linkoid.Stardew.YetAnother.ContentPatcher.Framework.ConfigModels;

/// <summary>A user-defined token whose value may depend on other tokens.</summary>
internal class YamlDynamicTokenConfig
{
	/*********
    ** Accessors
    *********/
	/// <summary>The name of the token to set.</summary>
	public string? Name { get; init; }

	/// <summary>The value to set.</summary>
	public string? Value { get; init; }

	/// <summary>The criteria to apply. See the README for valid values.</summary>
	public InvariantDictionary<string?> When { get; init; }


	/*********
    ** Public methods
    *********/
	/// <summary>Construct an instance.</summary>
	/// <param name="name">The name of the token to set.</param>
	/// <param name="value">The value to set.</param>
	/// <param name="when">The criteria to apply. See the README for valid values.</param>
	public DynamicTokenConfig ToDynamicTokenConfig()
	{
		return new DynamicTokenConfig(
			name : this.Name,
			value: this.Value,
			when : this.When 
		);
	}
}
