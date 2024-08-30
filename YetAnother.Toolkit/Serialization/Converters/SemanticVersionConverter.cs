using System;
using System.Formats.Asn1;
using System.Text.Json.Serialization;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using StardewModdingAPI;
using StardewModdingAPI.Toolkit.Serialization;
using YamlDotNet.Core.Events;
using StardewModdingAPI.Toolkit;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization.Converters;

/// <summary>Handles deserialization of <see cref="ISemanticVersion"/>.</summary>
internal class SemanticVersionConverter : IYamlTypeConverter
{
	public static readonly SemanticVersionConverter Default = new SemanticVersionConverter();

	/// <summary>Whether to allow non-standard extensions to semantic versioning.</summary>
	protected bool AllowNonStandard { get; set; }

	public bool Accepts(Type type)
	{
		return typeof(ISemanticVersion).IsAssignableFrom(type);
	}

	public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
	{
		if (parser.Accept<MappingStart>(out _))
		{
			return ReadObject(parser);
		}
		else if (parser.TryConsume<Scalar>(out var scalar))
		{
			return ReadString(scalar.Value, scalar);
		}
		else
		{
			throw ParseError($"Can't parse {nameof(ISemanticVersion)} from {parser.Current} event.", parser.Current);
		}
	}

	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
	{
		emitter.Emit(new Scalar((value as ISemanticVersion)?.ToString() ?? string.Empty));
	}

	private ISemanticVersion ReadObject(IParser parser)
	{
		parser.Consume<MappingStart>();

		int major = 0;
		int minor = 0;
		int patch = 0;
		string? prereleaseTag = null;

		while (parser.TryConsume<Scalar>(out var keyNode))
		{
			if (!keyNode.IsKey) throw ParseError($"Can't parse {nameof(ISemanticVersion)} with key {parser.Current}.", keyNode);

			var valueNode = parser.Consume<Scalar>();
			try
			{
				if (keyNode.Value.Equals(nameof(ISemanticVersion.MajorVersion), StringComparison.OrdinalIgnoreCase))
				{
					major = int.Parse(valueNode.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
				}
				else if (keyNode.Value.Equals(nameof(ISemanticVersion.MinorVersion), StringComparison.OrdinalIgnoreCase))
				{
					minor = int.Parse(valueNode.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
				}
				else if (keyNode.Value.Equals(nameof(ISemanticVersion.PatchVersion), StringComparison.OrdinalIgnoreCase))
				{
					patch = int.Parse(valueNode.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
				}
				else if (keyNode.Value.Equals(nameof(ISemanticVersion.PrereleaseTag), StringComparison.OrdinalIgnoreCase))
				{
					prereleaseTag = valueNode.Value;
				}
				else
				{
					throw ParseError($"Can't parse {nameof(ISemanticVersion)} with unknown key '{keyNode.Value}'.", keyNode);
				}
			}
			catch (Exception ex) when (ex is FormatException || ex is OverflowException)
			{
				throw ParseError($"Can't parse {nameof(ISemanticVersion)} with invalid value for '{valueNode.Value}'.", valueNode);
			}
		}

		return new StardewModdingAPI.Toolkit.SemanticVersion(major, minor, patch, prereleaseTag: prereleaseTag);
	}

	private ISemanticVersion? ReadString(string str, ParsingEvent parsingEvent)
	{
		if (string.IsNullOrWhiteSpace(str))
			return null;
		if (!StardewModdingAPI.Toolkit.SemanticVersion.TryParse(str, allowNonStandard: this.AllowNonStandard, out ISemanticVersion? version))
			throw ParseError($"Can't parse semantic version from invalid value '{str}', should be formatted like 1.2, 1.2.30, or 1.2.30-beta.", parsingEvent);
		return version;
	}

	private static SParseException ParseError(string message, ParsingEvent? parsingEvent)
	{
		if (parsingEvent is not null)
		{
			return new SParseException($"{message} (line {parsingEvent?.Start.Line})");
		}
		else
		{
			return new SParseException(message);
		}
	}
}
