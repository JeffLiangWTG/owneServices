using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Core.Testing
{
	internal class LocationComponentParserTest : TestCase
	{
		public void TestParse()
		{
			AssertEquals((short)2, LocationComponentParser.Parse("2", false));
			AssertEquals((short)2, LocationComponentParser.Parse("2", true));
			AssertEquals((short)2, LocationComponentParser.Parse("B", true));
			AssertEquals((short)2, LocationComponentParser.Parse("b", true));
			AssertEquals((short)22, LocationComponentParser.Parse("22", false));
			AssertEquals((short)22, LocationComponentParser.Parse("22", true));
			AssertEquals((short)-1, LocationComponentParser.Parse("B", false));
			AssertEquals((short)-1, LocationComponentParser.Parse("b", false));
			AssertEquals((short)-1, LocationComponentParser.Parse("BB", true));
			AssertEquals((short)-1, LocationComponentParser.Parse("321212121", false));
		}
	}
}
