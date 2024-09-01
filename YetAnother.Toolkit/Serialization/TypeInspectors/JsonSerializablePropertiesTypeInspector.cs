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
using Newtonsoft.Json;
using StardewValley.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization.TypeInspectors;

/// <summary>
/// Returns the properties of a type that are writable.
/// </summary>
public sealed class JsonSerializablePropertiesTypeInspector : TypeInspectorSkeleton
{
	private readonly ITypeInspector innerTypeInspector;
	public JsonSerializablePropertiesTypeInspector(ITypeInspector innerTypeInspector)
	{
		this.innerTypeInspector = innerTypeInspector;
	}

	public override string GetEnumName(Type enumType, string name)
	{
		return innerTypeInspector.GetEnumName(enumType, name);
	}

	public override string GetEnumValue(object enumValue)
	{
		return innerTypeInspector.GetEnumValue(enumValue);
	}

	public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
	{
		var existingProperties = innerTypeInspector.GetProperties(type, container);
		var jsonProperties =
			from property in type.GetProperties(AccessTools.all)
			let jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>()
			where jsonPropertyAttribute != null
			where !existingProperties.Any(p => string.Equals(p.Name, property.Name, StringComparison.Ordinal))
			select new JsonPropertyDescriptor(property, jsonPropertyAttribute);

		var properties = existingProperties.Concat(jsonProperties);	

		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		foreach (var prop in properties)
		{
			stringBuilder.Append(prop.Name).Append(", ");
		}
		Console.Error.WriteLine(stringBuilder.ToString());
		return properties;
	}

	private sealed class JsonPropertyDescriptor : IPropertyDescriptor
	{
		private readonly PropertyInfo property;
		private readonly JsonPropertyAttribute attribute;

		public JsonPropertyDescriptor(PropertyInfo property, JsonPropertyAttribute attribute)
		{
			Console.Error.WriteLine($"JsonPropertyDescriptor({property.Name})");
			this.property = property ?? throw new ArgumentNullException(nameof(property));
			this.attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
		}

		public string Name => property.Name;
		public bool AllowNulls => property.AcceptsNull();
		public bool CanWrite => true;
		public Type Type => property.PropertyType;
		public Type? TypeOverride { get; set; }
		public int Order { get; set; }
		public ScalarStyle ScalarStyle { get; set; }
		public bool Required => attribute.Required != Newtonsoft.Json.Required.Default || property.IsRequired();
		public Type? ConverterType => null; //property.ConverterType;

		public T? GetCustomAttribute<T>() where T : Attribute
		{
			return property.GetCustomAttribute<T>();
		}

		public IObjectDescriptor Read(object target)
		{
			var value = property.GetValue(target);
			return new JsonPropertyObjectDescriptor(this, value);
		}

		public void Write(object target, object? value)
		{
			if (attribute.ObjectCreationHandling == ObjectCreationHandling.Reuse
				|| (attribute.ObjectCreationHandling == ObjectCreationHandling.Auto
					&& !property.CanWrite))
			{
				var existingObject = Read(target);
				var addMethod = AccessTools.Method(existingObject.Type, "Add");
				if (existingObject != null && addMethod != null
					&& value is IDictionary valueDictionary)
				{
					foreach (dynamic item in valueDictionary)
					{
						addMethod.Invoke(existingObject.Value, new[] { item.Key, item.Value });;
					}
				}
				else if (existingObject != null && addMethod != null
					&& value is ICollection valueCollection)
				{
					foreach (var item in valueCollection)
					{
						addMethod.Invoke(existingObject.Value, new[] { item });
					}
				}
				else
				{
					throw new NotImplementedException($"Could not write to property '{property.Name}' of type '{property.PropertyType}'." +
						$" This type is not supported yet.");
				}
			}
			else
			{
				property.SetValue(target, value);
			}
		}
	
		private record JsonPropertyObjectDescriptor(
			JsonPropertyDescriptor PropertyDescriptor,
			object? Value
		) : IObjectDescriptor
		{
			public Type Type => PropertyDescriptor.TypeOverride ?? PropertyDescriptor.Type;
			public Type StaticType => PropertyDescriptor.Type;
			public ScalarStyle ScalarStyle => PropertyDescriptor.ScalarStyle;
		}
	}
}