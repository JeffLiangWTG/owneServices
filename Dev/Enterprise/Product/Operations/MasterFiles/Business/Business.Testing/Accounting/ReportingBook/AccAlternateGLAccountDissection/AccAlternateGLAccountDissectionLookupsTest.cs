using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAlternateGLAccountDissectionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAttributeList()
		{
			var alternateGLAccountDissection = Factory.NewWithValidTestData<AccAlternateGLAccountDissection>();
			var lookups = new AccAlternateGLAccountDissectionLookups(alternateGLAccountDissection);

			AssertEquals(lookups.AttributeList.Count, 6);
			AssertEquals("The AttributeList should contain 'ORG'", true, lookups.AttributeList.ContainsCode("ORG"));
			AssertEquals("The AttributeList should contain 'OCG'", true, lookups.AttributeList.ContainsCode("OCG"));
			AssertEquals("The AttributeList should contain 'TIC'", true, lookups.AttributeList.ContainsCode("TIC"));
			AssertEquals("The AttributeList should contain 'LFO'", true, lookups.AttributeList.ContainsCode("LFO"));
			AssertEquals("The AttributeList should contain 'LFE'", true, lookups.AttributeList.ContainsCode("LFE"));
			AssertEquals("The AttributeList should contain 'SPR'", true, lookups.AttributeList.ContainsCode("SPR"));
		}

		public void TestGetNonGlobalAttributeList()
		{
			var nonGlobalAttributeList = AccAlternateGLAccountDissectionLookups.GetNonGlobalAttributeList();

			AssertEquals(nonGlobalAttributeList.Count, 1);
			AssertEquals("The NonGlobalAttributeList should contain 'ORG'", true, nonGlobalAttributeList.ContainsCode("ORG"));
		}
	}
}
