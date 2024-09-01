
// This file is part of YamlDotNet - A .NET library for YAML.
// Copyright (c) Antoine Aubry and contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using HarmonyLib;
using StardewValley;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using xTile.ObjectModel;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.Utilities;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization.NodeDeserializers;

internal sealed class JsonSerializableObjectNodeDeserializer : INodeDeserializer
{
	private readonly ObjectNodeDeserializer objectNodeDeserializer;
	private readonly IReadOnlyObjectFactory readOnlyObjectFactory;

	private readonly IObjectFactory objectFactory;
	private readonly ITypeInspector typeInspector;
	private readonly bool ignoreUnmatched;
	private readonly bool duplicateKeyChecking;
	private readonly ITypeConverter typeConverter;
	private readonly INamingConvention enumNamingConvention;
	private readonly bool enforceNullability;
	private readonly bool caseInsensitivePropertyMatching;
	private readonly bool enforceRequiredProperties;
	private readonly IEnumerable<IYamlTypeConverter> typeConverters;

	public JsonSerializableObjectNodeDeserializer(IReadOnlyObjectFactory readOnlyObjectFactory, ObjectNodeDeserializer objectNodeDeserializer)
	{
		this.objectNodeDeserializer = objectNodeDeserializer;
		this.readOnlyObjectFactory = readOnlyObjectFactory;

		this.objectFactory                   = (IObjectFactory                 )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(objectFactory                  )).GetValue(objectNodeDeserializer)!;
		this.typeInspector                   = (ITypeInspector                 )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(typeInspector                  )).GetValue(objectNodeDeserializer)!;
		this.ignoreUnmatched                 = (bool                           )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(ignoreUnmatched                )).GetValue(objectNodeDeserializer)!;
		this.duplicateKeyChecking            = (bool                           )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(duplicateKeyChecking           )).GetValue(objectNodeDeserializer)!;
		this.typeConverter                   = (ITypeConverter                 )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(typeConverter                  )).GetValue(objectNodeDeserializer)!;
		this.enumNamingConvention            = (INamingConvention              )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(enumNamingConvention           )).GetValue(objectNodeDeserializer)!;
		this.enforceNullability              = (bool                           )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(enforceNullability             )).GetValue(objectNodeDeserializer)!;
		this.caseInsensitivePropertyMatching = true; // (bool                           )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(caseInsensitivePropertyMatching)).GetValue(objectNodeDeserializer)!;
		this.enforceRequiredProperties       = (bool                           )AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(enforceRequiredProperties      )).GetValue(objectNodeDeserializer)!;
		this.typeConverters                  = (IEnumerable<IYamlTypeConverter>)AccessTools.DeclaredField(typeof(ObjectNodeDeserializer), nameof(typeConverters                 )).GetValue(objectNodeDeserializer)!;
	}

	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		return DeserializeInternal(parser, expectedType, nestedObjectDeserializer, out value, rootDeserializer)
			|| objectNodeDeserializer.Deserialize(parser, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
	}

	private bool DeserializeInternal(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		if (!parser.TryConsume<MappingStart>(out var mapping))
		{
			value = null;
			return false;
		}

		// Strip off the nullable & fsharp option type, if present. This is needed for nullable structs.
		var implementationType = Nullable.GetUnderlyingType(expectedType)
			?? FsharpHelper.GetOptionUnderlyingType(expectedType)
			?? expectedType;

		if (!readOnlyObjectFactory.CanCreate(implementationType, out IEnumerable<IParameterDescriptor>? parameters))
		{
			value = null;
			return false;
		}

		var builder = new Builder(this, parser, implementationType, nestedObjectDeserializer, rootDeserializer);

		var objectOrPromise = builder.CreateObjectOrPromise(parameters, out var collectedParameters, out var collectedProperties, out MappingEnd mappingEnd);

		if (objectOrPromise is IValuePromise objectPromise)
		{
			value = new ValuePromiseWrapper(objectPromise, DeserializeObjectProperties!);
			return true;
		}
		else
		{
			value = DeserializeObjectProperties(objectOrPromise);
			return true;
		}

		object DeserializeObjectProperties(object createdObject)
		{
			objectFactory.ExecuteOnDeserializing(createdObject);

			ISet<string> consumedObjectProperties = builder.SetObjectProperties(createdObject, collectedProperties);

			if (enforceRequiredProperties)
			{
				//TODO: Get properties marked as required on the object
				//TODO: Compare those properties agains the consumedObjectProperties, throw if any are missing.
				var properties = typeInspector.GetProperties(implementationType, createdObject);
				var missingPropertyNames = new List<string>();
				foreach (var property in properties)
				{
					if (property.Required && !consumedObjectProperties.Contains(property.Name))
					{
						missingPropertyNames.Add(property.Name);
					}
				}

				if (missingPropertyNames.Count > 0)
				{
					var propertyNames = string.Join(",", missingPropertyNames);
					throw new YamlException(mapping.Start, mappingEnd.End, $"Missing properties, '{propertyNames}' in source yaml.");
				}
			}

			objectFactory.ExecuteOnDeserialized(createdObject);

			return createdObject;
		}
	}

	private sealed class ValuePromiseWrapper : IValuePromise
	{
		public event Action<object?>? ValueAvailable;
		public Func<object?, object?> GetFinalValue;

		public ValuePromiseWrapper(IValuePromise valuePromise, Func<object?, object?> getFinalValue)
		{
			GetFinalValue = getFinalValue;
			ValueAvailable += OnPromiseValueAvailable;
		}

		private void OnPromiseValueAvailable(object? value)
		{
			var finalValue = GetFinalValue.Invoke(value);
			ValueAvailable?.Invoke(finalValue);
		}
	}

	private sealed class GroupValuePromise : IValuePromise
	{
		public Func<object?>? GetFinalValue;
		public event Action<object?>? ValueAvailable;

		private readonly HashSet<IValuePromise> promises = new();

		public int Count => promises.Count;

		public bool Add(IValuePromise promise, Action<object?> action)
		{
			promise.ValueAvailable += value =>
			{
				action.Invoke(value);
				OnPromiseValueAvailable(promise);
			};
			return promises.Add(promise);
		}

		private void OnPromiseValueAvailable(IValuePromise promise)
		{
			promises.Remove(promise);
			if (promises.Count == 0)
			{
				ValueAvailable?.Invoke(GetFinalValue?.Invoke());
			}
		}
	}

	private class Builder
	{
		private readonly JsonSerializableObjectNodeDeserializer deserializer;
		private readonly IParser parser;
		private readonly Type implementationType;
		private readonly Func<IParser, Type, object?> nestedObjectDeserializer;
		private readonly ObjectDeserializer rootDeserializer;

		public Builder(JsonSerializableObjectNodeDeserializer deserializer, IParser parser, Type implementationType, Func<IParser, Type, object?> nestedObjectDeserializer, ObjectDeserializer rootDeserializer)
		{
			this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
			this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
			this.implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
			this.nestedObjectDeserializer = nestedObjectDeserializer ?? throw new ArgumentNullException(nameof(nestedObjectDeserializer));
			this.rootDeserializer = rootDeserializer ?? throw new ArgumentNullException(nameof(rootDeserializer));
		}

		public object CreateObjectOrPromise(
			IEnumerable<IParameterDescriptor> parameters,
			out IReadOnlyDictionary<IParameterDescriptor, object?> collectedParameters,
			out IReadOnlyDictionary<IPropertyDescriptor, (Scalar, object?)> collectedProperties,
			out MappingEnd mappingEnd)
		{
			Dictionary<IParameterDescriptor, object?> collectedParametersDict = new();
			Dictionary<IPropertyDescriptor, (Scalar, object?)> collectedPropertiesDict = new();
			collectedParameters = collectedParametersDict;
			collectedProperties = collectedPropertiesDict;

			var consumedKeys = new HashSet<string>(StringComparer.Ordinal);
			var groupValuePromise = new GroupValuePromise();

			StringComparison parameterComparison = deserializer.caseInsensitivePropertyMatching ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			var start = parser.Current!.Start;
			while (!parser.TryConsume<MappingEnd>(out mappingEnd!))
			{
				var parameterName = parser.Consume<Scalar>();
				if (deserializer.duplicateKeyChecking && !consumedKeys.Add(parameterName.Value))
				{
					throw new YamlException(parameterName.Start, parameterName.End, $"Encountered duplicate key {parameterName.Value}");
				}
				try
				{
					var parameter = parameters.FirstOrDefault(p => string.Equals(p.Name, parameterName.Value, parameterComparison));
					if (parameter is null)
					{
						var propertyName = parameterName;
						if (TryConsumePropertyValue(propertyName, out var property, out object? propertyValue))
						{
							collectedPropertiesDict.Add(property, (propertyName, propertyValue));
						}
						continue;
					}

					object? parameterValue = DeserializeNestedValue(parameter.Type, parameter.ConverterType);

					if (parameterValue is IValuePromise parameterValuePromise)
					{
						groupValuePromise.Add(parameterValuePromise, v =>
						{
							object? convertedValue = ChangeType(v, parameter.Type);
							NullCheck(convertedValue, parameter, parameterName);
							collectedParametersDict.Add(parameter, convertedValue);
						});
					}
					else
					{
						var convertedValue = ChangeType(parameterValue, parameter.Type);
						NullCheck(convertedValue, parameter, parameterName);
						collectedParametersDict.Add(parameter, convertedValue);
					}
				}
				catch (SerializationException ex)
				{
					throw new YamlException(parameterName.Start, parameterName.End, ex.Message);
				}
				catch (YamlException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new YamlException(parameterName.Start, parameterName.End, "Exception during deserialization", ex);
				}
			}
			var end = mappingEnd.End;

			if (groupValuePromise.Count > 0)
			{
				groupValuePromise.GetFinalValue = CreateFinalValue;
				return groupValuePromise;
			}
			else
			{
				return CreateFinalValue();
			}

			object CreateFinalValue()
			{
				try
				{
					return deserializer.readOnlyObjectFactory.Create(implementationType, collectedParametersDict);
				}
				catch (SerializationException ex)
				{
					throw new YamlException(start, end, ex.Message);
				}
				catch (YamlException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new YamlException(start, end, "Exception during deserialization", ex);
				}
			}
		}


		public ISet<string> SetObjectProperties(object value, IReadOnlyDictionary<IPropertyDescriptor, (Scalar, object?)> collectedProperties)
		{
			var consumedObjectProperties = new HashSet<string>(StringComparer.Ordinal);
			foreach ((var property, (var propertyName, var propertyValue)) in collectedProperties)
			{
				try
				{
					consumedObjectProperties.Add(property.Name);

					if (propertyValue is IValuePromise propertyValuePromise)
					{
						var valueRef = value;
						propertyValuePromise.ValueAvailable += v =>
						{
							var convertedValue = ChangeType(v, property.Type);
							NullCheck(convertedValue, property, propertyName);
							property.Write(valueRef, convertedValue);
						};
					}
					else
					{
						var convertedValue = ChangeType(propertyValue, property.Type);
						NullCheck(convertedValue, property, propertyName);
						property.Write(value, convertedValue);
					}
				}
				catch (SerializationException ex)
				{
					throw new YamlException(propertyName.Start, propertyName.End, ex.Message);
				}
				catch (YamlException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new YamlException(propertyName.Start, propertyName.End, "Exception during deserialization", ex);
				}
			}

			return consumedObjectProperties;
		}

		private object? ChangeType(object? v, Type type)
		{
			return deserializer.typeConverter.ChangeType(v, type, deserializer.enumNamingConvention, deserializer.typeInspector);
		}

		bool TryConsumePropertyValue(Scalar propertyName, [NotNullWhen(true)] out IPropertyDescriptor? property, out object? propertyValue)
		{
			property = deserializer.typeInspector.GetProperty(implementationType, null, propertyName.Value, deserializer.ignoreUnmatched, deserializer.caseInsensitivePropertyMatching);
			if (property == null)
			{
				parser.SkipThisAndNestedEvents();
				propertyValue = null;
				return false;
			}
			else
			{
				propertyValue = DeserializeNestedValue(property.Type, property.ConverterType);
				return true;
			}
		}

		private object? DeserializeNestedValue(Type type, Type? converterType)
		{
			object? nestedValue = null;
			if (converterType != null)
			{
				var typeConverter = deserializer.typeConverters.Single(x => x.GetType() == converterType);
				nestedValue = typeConverter.ReadYaml(parser, type, rootDeserializer);
			}
			else
			{
				nestedValue = nestedObjectDeserializer.Invoke(parser, type);
			}
			return nestedValue;
		}

		private void NullCheck([NotNull] object? value, IParameterDescriptor parameter, Scalar parameterName)
		{
			if (deserializer.enforceNullability &&
				value == null &&
				!parameter.AllowNulls)
			{
				throw new YamlException(parameterName.Start, parameterName.End, "Strict nullability enforcement error.", new NullReferenceException("Yaml value is null when target property requires non null values."));
			}
		}

		private void NullCheck([NotNull] object? value, IPropertyDescriptor property, Scalar propertyName)
		{
			if (deserializer.enforceNullability &&
				value == null &&
				!property.AllowNulls)
			{
				throw new YamlException(propertyName.Start, propertyName.End, "Strict nullability enforcement error.", new NullReferenceException("Yaml value is null when target property requires non null values."));
			}
		}

	}
}
