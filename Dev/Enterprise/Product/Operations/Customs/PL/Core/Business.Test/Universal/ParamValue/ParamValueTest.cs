using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ParamValueTest : TestCase
{
	public void TestName() => CombineAssertions(() =>
	{
		var paramsValue = new ParamValue("TEST NAME", "VALUE");
		AssertEquals("String", "TEST NAME", paramsValue.Name);

		paramsValue = new ParamValue(Name: null, "VALUE");
		AssertEquals("Null", string.Empty, paramsValue.Name);
	});

	public void TestValue() => CombineAssertions(() =>
	{
		var paramsValue = new ParamValue("NAME", "TEST VALUE");
		AssertEquals("String", "TEST VALUE", paramsValue.StringValue);

		paramsValue = new ParamValue("NAME", "99");
		AssertEquals("Object value", "99", paramsValue.StringValue);

		paramsValue = new ParamValue("NAME", null);
		AssertEquals("null", string.Empty, paramsValue.StringValue);
	});

	public void TestToString()
	{
		var paramsValue = new ParamValue("TEST NAME", "TEST VALUE");
		AssertEquals("TEST NAME : TEST VALUE", paramsValue.ToString());
	}
}
