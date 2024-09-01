using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Linkoid.Stardew.YetAnother.Toolkit.Serialization;

internal static class ReflectionExtensions
{
	public static bool IsRequired(this MemberInfo member)
	{
		return member.GetCustomAttributes(true).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");
	}


	private static readonly NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
	public static bool AcceptsNull(this ParameterInfo info)
	{
		var nullabilityInfo = nullabilityInfoContext.Create(info);
		return !info.ParameterType!.IsValueType && nullabilityInfo.WriteState != NullabilityState.NotNull;
	}
	public static bool AcceptsNull(this PropertyInfo info)
	{
		var nullabilityInfo = nullabilityInfoContext.Create(info);
		return !info.PropertyType!.IsValueType && nullabilityInfo.WriteState != NullabilityState.NotNull;
	}
	public static bool AcceptsNull(this EventInfo info)
	{
		var nullabilityInfo = nullabilityInfoContext.Create(info);
		return !info.EventHandlerType!.IsValueType && nullabilityInfo.WriteState != NullabilityState.NotNull;
	}
	public static bool AcceptsNull(this FieldInfo info)
	{
		var nullabilityInfo = nullabilityInfoContext.Create(info);
		return !info.FieldType.IsValueType && nullabilityInfo.WriteState != NullabilityState.NotNull;
	}
}