using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ParamValuesTest : TestCase
{
	public void TestName() => CombineAssertions(() =>
	{
		var values = new ZString[] { "Value1", "Value2" };
		var paramsValue = new ParamValues("TEST NAME", values);
		AssertEquals("String", "TEST NAME", paramsValue.Name);

		paramsValue = new ParamValues(Name: null, values);
		AssertEquals("Null", string.Empty, paramsValue.Name);
	});

	public void TestValues() => CombineAssertions(() =>
	{
		var values = new ZString[] { "Value1", "Value2" };

		var paramsValue = new ParamValues("NAME", values);
		AssertEquals("String", values, paramsValue.StringValues);

		paramsValue = new ParamValues("NAME", []);
		AssertEquals("null", Array.Empty<ZString>(), paramsValue.StringValues);
	});

	public void TestConstructFromValueTuple() => CombineAssertions(() =>
	{
		var values = new ZString[] { "Value1", "Value2" };

		var paramValues = new ParamValues("TEST NAME", values);
		AssertEquals("name", "TEST NAME", paramValues.Name);
		AssertEquals("value", values, paramValues.StringValues);
	});

	public void TestToString()
	{
		var values = new ZString[] { "Value1", "Value2" };
		var paramsValue = new ParamValues("TEST NAME", values);
		AssertEquals("TEST NAME : [Value1 | Value2]", paramsValue.ToString());
	}
}
