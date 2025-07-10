using System.ComponentModel;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

sealed class ZNullableCalcEditColumnStyleInfoTest : TestCase
{
	public void TestShowEmptyStringForEmptyValue()
		=> Assert(new ZNullableCalcEditColumnStyleInfo().ShowEmptyStringForEmptyValue);

	public void TestShowEmptyStringForEmptyValueDefault()
		=> Assert((bool)typeof(ZNullableCalcEditColumnStyleInfo)
					.GetProperty(nameof(ZNullableCalcEditColumnStyleInfo.ShowEmptyStringForEmptyValue))!
					.GetCustomAttribute<DefaultValueAttribute>().Value);
}
