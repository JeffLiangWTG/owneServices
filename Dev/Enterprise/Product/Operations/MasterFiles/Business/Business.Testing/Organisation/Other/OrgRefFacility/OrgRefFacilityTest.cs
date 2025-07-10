using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRefFacility))]
	public class OrgRefFacilityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadRefFacility()
		{
			var org = Factory.New<OrgHeader>();
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			var orgRefFacility = org.OrgRefFacilities.AddNew();
			org.OH_Code = "TSTTSTTST";
			refFacility.RFT_Code = "00000000001";
			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility.OFC_OH_Organization = org.PK;
			orgRefFacility.OFC_RFT_Facility = refFacility.PK;
			Factory.Save();

			AssertEquals(refFacility, orgRefFacility.Facility);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var facility = Factory.NewWithValidTestData<RefFacility>();
			var orgRefFacility = Factory.New<OrgRefFacility>();
			orgRefFacility.OFC_OH_Organization = org.PK;
			orgRefFacility.OFC_RFT_Facility = facility.PK;
			return orgRefFacility;
		}

		#endregion
	}
}

