using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgRefFacilityLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActiveAddressChangesWithOrganisation()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.MainAddress.OA_Address1 = "111 first street";

			var orgRefFacility = Factory.New<OrgRefFacility>();
			orgRefFacility.OFC_OH_Organization = orgHeader1.PK;

			AssertEquals(orgRefFacility.Lookups.ActiveAddresses[0], orgHeader1.ActiveOrAllAddresses[0]);

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.MainAddress.OA_Address1 = "222 second street";
			orgRefFacility.OFC_OH_Organization = orgHeader2.PK;

			AssertEquals(orgRefFacility.Lookups.ActiveAddresses[0], orgHeader2.ActiveOrAllAddresses[0]);
		}

		public void TestRefFacilities()
		{
			var org = Factory.New<OrgHeader>();
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			var orgRefFacility = org.OrgRefFacilities.AddNew();
			org.OH_Code = "TSTTSTTST";
			refFacility.RFT_Code = "00000000001";
			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility.OFC_OH_Organization = org.PK;
			orgRefFacility.OFC_RFT_Facility = refFacility.PK;

			var lookups = orgRefFacility.Lookups;
			AssertEquals(1, lookups.RefFacilities.Count);

			Assert(lookups.RefFacilities.Contains(refFacility));
		}
	}
}
