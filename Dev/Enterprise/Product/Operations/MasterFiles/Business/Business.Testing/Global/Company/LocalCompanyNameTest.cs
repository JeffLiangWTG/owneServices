using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LocalCompanyNameTest : TestCaseWithFactory
	{
		public void TestGetCompanyNameChina()
		{
			AssertEquals("CHINACOMPANY", LocalCompanyName.GetCurrentCompanyLocalName());
		}
		public void TestGetLocalCompanyName()
		{
			AssertEquals("CHINACOMPANY", LocalCompanyName.GetLocalCompanyName(GlbCompany.CurrentCompany.OrgProxy, OrgConstants.AddressType.Receivables));
			AssertEquals("CompanyForPayables", LocalCompanyName.GetLocalCompanyName(GlbCompany.CurrentCompany.OrgProxy, OrgConstants.AddressType.Payables));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			CreateOrgProxy();
		}

		void CreateOrgProxy()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			OrgHeader testOrgHeader = factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;

			OrgAddress testAddress1 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress2 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress3 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress4 = testOrgHeader.Addresses.AddNew();
			OrgAddress testAddress5 = testOrgHeader.Addresses.AddNew();

			testAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			testAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			testAddress3.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Miscellaneous);
			testAddress4.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			testAddress5.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			testAddress1.OA_CompanyNameOverride = "CompanyForPayables";
			testAddress2.OA_CompanyNameOverride = "";
			testAddress3.OA_CompanyNameOverride = "USCompany";
			testAddress4.OA_CompanyNameOverride = "CHINACOMPANYNonDefault";
			testAddress5.OA_CompanyNameOverride = "CHINACOMPANY";

			testAddress1.OA_Address1 = "Address1";
			testAddress2.OA_Address1 = "EmptyName";
			testAddress3.OA_Address1 = "Address3";
			testAddress4.OA_Address1 = "Address4";
			testAddress5.OA_Address1 = "Address5";

			testAddress1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Payables);
			testAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			testAddress3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Miscellaneous);
			testAddress4.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Receivables);
			testAddress5.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrgHeader.PK;
		}

		#endregion

	}
}
