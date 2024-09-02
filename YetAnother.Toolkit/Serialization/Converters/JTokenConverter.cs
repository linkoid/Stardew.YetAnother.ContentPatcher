using System;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using YamlDotNet.Core.Events;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization.Converters;

internal class JTokenConverter : IYamlTypeConverter
{
	public static readonly JTokenConverter Instance = new JTokenConverter();

	private readonly JsonSerializer jsonSerializer;

	public JTokenConverter()
	{
		jsonSerializer = new JsonSerializer();
		jsonSerializer.Converters.Add(YamlNodeConverter.Instance);
	}

	public bool Accepts(Type type)
	{
		return typeof(JToken).IsAssignableFrom(type);
	}

	public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
	{
		if (typeof(JObject  ).IsAssignableFrom(type)) return ReadObject  (rootDeserializer);
		if (typeof(JArray   ).IsAssignableFrom(type)) return ReadList    (rootDeserializer);
		if (typeof(JProperty).IsAssignableFrom(type)) return ReadProperty(rootDeserializer);
		if (typeof(JValue   ).IsAssignableFrom(type)) return ReadValue   (rootDeserializer);

		if (parser.Accept<MappingStart> (out _)) return ReadObject(rootDeserializer);
		if (parser.Accept<SequenceStart>(out _)) return ReadList(rootDeserializer);
		if (parser.Accept<Scalar>(out var scalar))
		{
			if (scalar.IsKey)
			{
				return ReadProperty(rootDeserializer);
			}
			else
			{
				return ReadValue(rootDeserializer);
			}
		}

		throw new NotSupportedException($"Could not parse yaml event '{parser.Current}' as {type}");
	}

	private static object ReadValue(ObjectDeserializer rootDeserializer)
	{
		//Console.WriteLine($"parsing a value...");
		var value = rootDeserializer.Invoke<object>();
		//Console.WriteLine($"({value}) value type = {value?.GetType().ToString() ?? "null"}");
		return new JValue(value);
	}

	private static object ReadProperty(ObjectDeserializer rootDeserializer)
	{
		//Console.WriteLine($"parsing a property...");
		string? name = rootDeserializer.Invoke<string>();
		object? content = rootDeserializer.Invoke<JToken>();
		//Console.WriteLine($"({name}: {content}) content type = {content?.GetType().ToString() ?? "null"}");
		return new JProperty(name, content);
	}

	private static object ReadList(ObjectDeserializer rootDeserializer)
	{
		//Console.WriteLine($"parsing a list...");
		var list = rootDeserializer.Invoke<IList<JToken>>();
		//Console.WriteLine($"({list}) list type = {list?.GetType().ToString() ?? "null"}");
		return JArray.FromObject(list);
	}

	private static object ReadObject(ObjectDeserializer rootDeserializer)
	{
		//Console.WriteLine($"parsing a dictionary...");
		var dictionary = rootDeserializer.Invoke<IDictionary<JValue, JToken>>();
		//Console.WriteLine($"({dictionary}) dictionary type = {dictionary?.GetType().ToString() ?? "null"}");
		return JObject.FromObject(dictionary);
	}

	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
	{
		if (value is not null || value is not JToken jtoken)
			throw new ArgumentException($"{nameof(JTokenConverter)} cannot write type {type}");

		if (jtoken is JValue jvalue)
		{
			serializer.Invoke(jvalue.Value);
		}
		else if (jtoken is JArray jarray)
		{
			serializer.Invoke(jarray, typeof(IList<JToken>));
		}
		else if (jtoken is JObject jobject)
		{
			serializer.Invoke(jobject, typeof(IDictionary<string, JToken?>));
		}
		else
		{
			throw new NotSupportedException($"Cannot write JToken of type {jtoken.GetType()}");
		}
	}

}
