using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccSurchargeBasisLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeGroupList()
		{
			AssertNotNull(new AccSurchargeBasisLookups(Factory.New<AccSurchargeBasis>()).ChargeGroupList);
		}

		public void TestChargeCodes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			var surchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration.ASC_GC_Company = company.PK;
			var surchargeBasis = surchargeConfiguration.AccSurchargeBasises.AddNew();

			var lookups = new AccSurchargeBasisLookups(surchargeBasis);

			AssertEquals("Charge Code of surchargeBasis will get the charge of surchargeConfiguration company.", true, lookups.ChargeCodes.All(x => ((AccChargeCode)x).AC_GC == surchargeConfiguration.ASC_GC_Company));
			AssertEquals("Charge Code of surchargeConfiguration will get the charge without charge type OVR", true, lookups.ChargeCodes.All(x => ((AccChargeCode)x).AC_ChargeType != "OVR"));
		}
	}
}
