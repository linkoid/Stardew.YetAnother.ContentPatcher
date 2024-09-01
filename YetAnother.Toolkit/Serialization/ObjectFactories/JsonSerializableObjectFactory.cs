using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using System.Runtime.Serialization;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization.ObjectFactories;

internal class JsonSerializableObjectFactory : IReadOnlyObjectFactory
{
	public static readonly JsonSerializableObjectFactory Instance = new();

	public bool CanCreate(Type type, [NotNullWhen(true)] out IEnumerable<IParameterDescriptor>? factoryParameters)
	{
		if (!TryGetJsonConstructor(type, out var constructor))
		{
			factoryParameters = null;
			return false;
		}

		var jsonConstructorParameters = GetJsonConstructorParameters(constructor);
		factoryParameters = jsonConstructorParameters.Cast<IParameterDescriptor>();
		return true;
	}

	public object Create(Type type, IReadOnlyDictionary<IParameterDescriptor, object?> factoryParameterValues)
	{
		if (!TryGetJsonConstructor(type, out var constructor))
			throw new InvalidOperationException();

		var parameters = from parameter in GetJsonConstructorParameters(constructor)
						 let value = factoryParameterValues.ContainsKey(parameter) 
							? factoryParameterValues[parameter]
							: parameter.Info.HasDefaultValue ? parameter.Info.DefaultValue 
								: parameter.Type.IsValueType ? Activator.CreateInstance(parameter.Type) : null
						 orderby parameter.Info.Position
						 select value;

		return constructor.Invoke(parameters.ToArray());
	}

	private bool TryGetJsonConstructor(Type type, out ConstructorInfo jsonConstructor)
	{
		int publicConstructorCount = 0;
		ConstructorInfo? publicConstructor = null;
		foreach (var constructor in AccessTools.GetDeclaredConstructors(type, false))
		{
			if (constructor.IsPublic)
			{
				publicConstructorCount++;
				publicConstructor = constructor;
			}

			if (constructor.GetCustomAttribute<JsonConstructorAttribute>() != null)
			{
				jsonConstructor = constructor;
				return true;
			}
		}

		jsonConstructor = publicConstructor;
		return publicConstructorCount == 1;
	}

	private JsonConstructorParameterDescriptor[] GetJsonConstructorParameters(ConstructorInfo constructor)
	{
		var parameters = constructor.GetParameters();
		var jsonConstructorParameters = new JsonConstructorParameterDescriptor[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			jsonConstructorParameters[i] = new JsonConstructorParameterDescriptor(parameters[i]);
		}
		return jsonConstructorParameters;
	}

	private static readonly NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
	private record struct JsonConstructorParameterDescriptor(ParameterInfo Info) : IParameterDescriptor
	{
		public string Name { get; } = Info.Name ?? Info.Position.ToString();
		public Type Type { get; } = Info.ParameterType;
		public bool AllowNulls { get; } = !Info.ParameterType.IsValueType && nullabilityInfoContext.Create(Info).WriteState != NullabilityState.NotNull;
		public Type? ConverterType => null;
	}
}
