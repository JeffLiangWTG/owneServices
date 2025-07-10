using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class StringExtensionsTest : TestCase
{
	public void TestIfNullOrEmpty() => CombineAssertions(() =>
	{
		string nullString = null;
		AssertEquals("Null", "Second", nullString.IfNullOrEmpty(() => "Second"));
		AssertEquals("Empty", "Second", string.Empty.IfNullOrEmpty(() => "Second"));
		AssertEquals("Not empty", "First", "First".IfNullOrEmpty(() => "Second"));
	});

	public void TestIfNullOrEmpty_IsLazy()
	{
		var runCount = 0;
		_ = "First".IfNullOrEmpty(() =>
		{
			runCount++;
			return "Second";
		});
		AssertEquals(0, runCount);
	}
}
