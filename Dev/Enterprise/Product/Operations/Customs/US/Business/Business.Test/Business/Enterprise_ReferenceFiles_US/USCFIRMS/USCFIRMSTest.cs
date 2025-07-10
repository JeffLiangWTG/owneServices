using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCFIRMS))]
	sealed class USCFIRMSTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNameAndZipCode()
		{
			USCFIRMS facility = Factory.New<USCFIRMS>();
			facility.US_Name = "BOB THE BUILDER'S WAREHOUSE";
			facility.US_ZipCode = "61200";

			AssertEquals("BOB THE BUILDER'S WAREHOUSE (61200)", facility.NameAndZipCode);
		}

		public void TestFacilityTypeDescription()
		{
			USCFIRMS facility = Factory.New<USCFIRMS>();
			facility.US_FacilityType = "01";
			AssertEquals("Container Freight Station", facility.FacilityTypeDescription);

			facility.US_FacilityType = "02";
			AssertEquals("Foreign Trade Zone", facility.FacilityTypeDescription);

			facility.US_FacilityType = "03";
			AssertEquals("Pier", facility.FacilityTypeDescription);

			facility.US_FacilityType = "04";
			AssertEquals("Bonded Warehouse", facility.FacilityTypeDescription);

			facility.US_FacilityType = "05";
			AssertEquals("Inspection Facility", facility.FacilityTypeDescription);

			facility.US_FacilityType = "06";
			AssertEquals("Importers Premises", facility.FacilityTypeDescription);

			facility.US_FacilityType = "07";
			AssertEquals("Data Processing Site", facility.FacilityTypeDescription);

			facility.US_FacilityType = "08";
			AssertEquals("CBP Administrative Site", facility.FacilityTypeDescription);

			facility.US_FacilityType = "10";
			AssertEquals("Multi Use Bond", facility.FacilityTypeDescription);
		}
	}
}
