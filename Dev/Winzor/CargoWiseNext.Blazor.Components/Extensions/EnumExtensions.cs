using System.ComponentModel;
using System.Reflection;

namespace CargoWiseNext.Blazor.Components;

public static class EnumExtensions
{
	public static string GetDescription(this Enum value)
	{
		var memberInfo = value.GetType().GetMember(value.ToString()).FirstOrDefault();

		return memberInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
	}
}
