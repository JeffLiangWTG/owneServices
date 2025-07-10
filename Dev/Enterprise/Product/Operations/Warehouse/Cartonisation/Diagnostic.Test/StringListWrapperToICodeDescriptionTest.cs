using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	public class StringListWrapperToICodeDescriptionTest : TestCase
	{
		public void TestConstructor()
		{
			var list = new StringListWrapperToICodeDescription(new[] { "AAA", "BBB" });
			AssertEquals(2, list.Count);
			AssertEquals("AAA", list[0].Code);
			AssertEquals("AAA", list[0].Description);
			AssertEquals("BBB", list[1].Code);
			AssertEquals("BBB", list[1].Description);
		}
	}
}
