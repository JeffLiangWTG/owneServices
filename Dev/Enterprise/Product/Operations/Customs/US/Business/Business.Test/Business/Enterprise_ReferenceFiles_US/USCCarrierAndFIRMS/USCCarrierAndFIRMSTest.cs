using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCCarrierAndFIRMS))]
	sealed class USCCarrierAndFIRMSTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			// Not applicable to this class
		}

		public void TestFacilityTypeDescription()
		{
			var facility = Factory.New<USCCarrierAndFIRMS>();
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
