using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization;

internal static class ObjectDeserializerExtensions
{
	public static T? Invoke<T>(this ObjectDeserializer objectDeserializer)
	{
		return (T?)objectDeserializer.Invoke(typeof(T));
	}
}
