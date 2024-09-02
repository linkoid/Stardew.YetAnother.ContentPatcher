using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization.Converters;

internal class YamlNodeConverter : JsonConverter<YamlNode>
{
	public static readonly YamlNodeConverter Instance = new YamlNodeConverter();

	public override YamlNode? ReadJson(JsonReader reader, Type objectType, YamlNode? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, YamlNode? node, JsonSerializer serializer)
	{
		if (node == null)
		{
			writer.WriteNull();
			return;
		}

		switch (node.NodeType)
		{
			case YamlNodeType.Alias:
				throw new NotImplementedException();
			case YamlNodeType.Mapping:
				serializer.Serialize(writer, ((YamlMappingNode)node).Children);
				break;
			case YamlNodeType.Sequence:
				serializer.Serialize(writer, ((YamlSequenceNode)node).Children);
				break;
			case YamlNodeType.Scalar:
				var scalarNode = (YamlScalarNode)node;
				JToken jtoken;
				try
				{
					jtoken = JToken.Parse(scalarNode.Value);
				}
				catch
				{
					jtoken = JValue.CreateString(scalarNode.Value);
				}
				serializer.Serialize(writer, jtoken);
				break;
		}
	}
}
