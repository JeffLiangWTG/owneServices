using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class EntryKeyParserAndGeneratorTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			EntryKeyGenerator generator = new EntryKeyGenerator("3242H", "JSA", new ZDateTime(2005, 2, 1));
			EntryKeyParser parser = new EntryKeyParser(generator.EntryKey);
			AssertEquals("EntryNo", "3242H", parser.EntryNumber);
			AssertEquals("District", "JSA", parser.DistrictOfficeCode);
			AssertEquals("Date part", "02.05", parser.DatePart);
		}

		public void TestGeneration()
		{
			EntryKeyGenerator generator = new EntryKeyGenerator("3242H", "JSA", new ZDateTime(2005, 2, 1));
			AssertEquals("3242H/JSA/02.05", generator.EntryKey);
		}

		public void TestValidParse()
		{
			EntryKeyParser parser = new EntryKeyParser("3242H/JSA/02.05");
			AssertEquals("IsValid", true, parser.IsValid);
			AssertEquals("EntryKey", "3242H/JSA/02.05", parser.EntryKey);
			AssertEquals("EntryNumber", "3242H", parser.EntryNumber);
			AssertEquals("DistrictOfficeCode", "JSA", parser.DistrictOfficeCode);
			AssertEquals("DatePart", "02.05", parser.DatePart);
		}

		public void TestInvalidParse()
		{
			EntryKeyParser parser = new EntryKeyParser("junk");
			AssertEquals("IsValid", false, parser.IsValid);
			AssertEquals("EntryKey", "junk", parser.EntryKey);
			AssertEquals("EntryNumber", "", parser.EntryNumber);
			AssertEquals("DistrictOfficeCode", "", parser.DistrictOfficeCode);
			AssertEquals("DatePart", "", parser.DatePart);
		}
	}
}
