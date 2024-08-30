﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization;

/// <summary>Encapsulates YetAnotherToolkit's YAML file parsing.</summary>
public class YamlHelper
{
	/// <summary>The YAML settings to use when serializing files.</summary>
	public SerializerBuilder SerializerSettings { get; } = YamlHelper.CreateDefaultBuilder<SerializerBuilder>();

	/// <summary>The YAML settings to use when deserializing files.</summary>
	public DeserializerBuilder DeserializerSettings { get; } = YamlHelper.CreateDefaultBuilder<DeserializerBuilder>();

	/// <summary>Create an instance of the default YAML serializer settings.</summary>
	public static TBuilder CreateDefaultBuilder<TBuilder>()
		where TBuilder : BuilderSkeleton<TBuilder>, new()
	{
		return new TBuilder()
			.WithYamlFormatter(YamlFormatter.Default)
			.WithTypeConverter(Converters.SemanticVersionConverter.Default)
			.WithNamingConvention(NullNamingConvention.Instance);
	}

	/// <summary>Create an instance of the default YAML deserializer settings.</summary>
	public static DeserializerBuilder CreateDefaultDeserializer()
	{
		return new DeserializerBuilder()
			.WithYamlFormatter(YamlFormatter.Default)
			.WithTypeConverter(Converters.SemanticVersionConverter.Default)
			.WithNamingConvention(NullNamingConvention.Instance);
	}

	/// <summary>Read a YAML file.</summary>
	/// <typeparam name="TModel">The model type.</typeparam>
	/// <param name="fullPath">The absolute file path.</param>
	/// <param name="result">The parsed content model.</param>
	/// <returns>Returns false if the file doesn't exist, else true.</returns>
	/// <exception cref="ArgumentException">The given <paramref name="fullPath"/> is empty or invalid.</exception>
	/// <exception cref="YamlException">The file contains invalid YAML.</exception>
	public bool ReadYamlFileIfExists<TModel>(string fullPath, [NotNullWhen(true)] out TModel? result)
	{
		// validate
		if (string.IsNullOrWhiteSpace(fullPath))
			throw new ArgumentException("The file path is empty or invalid.", nameof(fullPath));

		if (!File.Exists(fullPath))
		{
			result = default;
			return false;
		}

		// deserialize model
		try
		{
			using var fileReader = new StreamReader(fullPath);
			result = GetDeserializer().Deserialize<TModel>(fileReader);
			return result != null;
		}
		catch (Exception ex)
		{
			string error = $"Can't parse YAML file at {fullPath}.";
			if (ex is YamlException yex)
			{
				error += " This doesn't seem to be valid YAML.";
				error += $"\nTechnical details: {ex.Message}";
				if (ex.InnerException != null)
				{
					error += $"\n{ex.InnerException}";
				}
				throw new YamlException(yex.Start, yex.End, error, ex);
			}
			else
			{
				error += $"\n{ex}";
				throw new YamlException(error, ex);
			}
		}
	}

	/// <summary>Save to a YAML file.</summary>
	/// <typeparam name="TModel">The model type.</typeparam>
	/// <param name="fullPath">The absolute file path.</param>
	/// <param name="model">The model to save.</param>
	/// <exception cref="InvalidOperationException">The given path is empty or invalid.</exception>
	public void WriteYamlFile<TModel>(string fullPath, TModel model)
		where TModel : class
	{
		// validate
		if (string.IsNullOrWhiteSpace(fullPath))
			throw new ArgumentException("The file path is empty or invalid.", nameof(fullPath));

		// create directory if needed
		string dir = Path.GetDirectoryName(fullPath)!;
		if (dir == null)
			throw new ArgumentException("The file path is invalid.", nameof(fullPath));
		if (!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		// write file
		string yaml = this.Serialize(model);
		File.WriteAllText(fullPath, yaml);
	}

	/// <summary>Deserialize YAML text if possible.</summary>
	/// <typeparam name="TModel">The model type.</typeparam>
	/// <param name="yaml">The raw YAML text.</param>
	public TModel Deserialize<TModel>(string yaml)
	{
		return GetDeserializer().Deserialize<TModel>(yaml);
	}

	/// <summary>Serialize a model to YAML text.</summary>
	/// <typeparam name="TModel">The model type.</typeparam>
	/// <param name="model">The model to serialize.</param>
	public string Serialize<TModel>(TModel model)
	{
		return GetSerializer().Serialize(model);
	}

	/// <summary>Get a low-level YAML serializer matching the <see cref="SerializerSettings"/>.</summary>
	public ISerializer GetSerializer()
	{
		return SerializerSettings.Build();
	}

	/// <summary>Get a low-level YAML deserializer matching the <see cref="DeserializerSettings"/>.</summary>
	public IDeserializer GetDeserializer()
	{
		return DeserializerSettings.Build();
	}
}
