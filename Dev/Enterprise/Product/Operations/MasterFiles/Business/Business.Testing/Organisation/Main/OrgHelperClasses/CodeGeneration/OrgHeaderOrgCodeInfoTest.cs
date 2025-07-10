using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderOrgCodeInfoTest : TestCaseWithFactory
	{
		OrgHeaderOrgCodeInfo info;
		OrgHeader organisation;

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			info = new OrgHeaderOrgCodeInfo(organisation);
		}

		public void TestCountryCode()
		{
			organisation.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("AU", info.CountryCode);
		}

		public void TestCountryName()
		{
			organisation.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Australia", info.CountryName);
		}

		public void TestIataCodeComesFromClosestPort()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AUYUX";
			unloco.RL_IATA = "XYZ";
			organisation.OH_RL_NKClosestPort = "AUYUX";
			AssertEquals("XYZ", info.IataCode);
		}

		public void TestIataCodeIsEmptyByDefault()
		{
			AssertEquals(string.Empty, info.IataCode);
		}

		public void TestIsCreditorForAnyCompanyIsFalseByDefault()
		{
			AssertEquals(false, info.IsCreditorForAnyCompany);
		}

		public void TestIsCreditorForAnyCompanyIsTrueWhenThereAreCreditorCompanyDataRecords()
		{
			var companyData1 = organisation.CompanyDataCollection.AddNew();
			var companyData2 = organisation.CompanyDataCollection.AddNew();
			companyData1.OB_IsCreditor = true;
			AssertEquals(true, info.IsCreditorForAnyCompany);
		}

		public void TestIsDebtorForAnyCompanyIsFalseByDefault()
		{
			AssertEquals(false, info.IsDebtorForAnyCompany);
		}

		public void TestIsDebtorForAnyCompanyIsTrueWhenThereAreDebtorCompanyDataRecords()
		{
			var companyData1 = organisation.CompanyDataCollection.AddNew();
			var companyData2 = organisation.CompanyDataCollection.AddNew();
			companyData1.OB_IsDebtor = true;
			AssertEquals(true, info.IsDebtorForAnyCompany);
		}

		public void TestOH_Code()
		{
			organisation.OH_Code = "XYZ999";
			AssertEquals("XYZ999", info.OH_Code);
		}

		public void TestOH_FullName()
		{
			organisation.OH_FullName = "My Company";
			AssertEquals("My Company", info.OH_FullName);
		}

		public void TestOH_IsBroker()
		{
			organisation.OH_IsBroker = true;
			AssertEquals(true, info.OH_IsBroker);
		}

		public void TestOH_IsCompetitor()
		{
			organisation.OH_IsCompetitor = true;
			AssertEquals(true, info.OH_IsCompetitor);
		}

		public void TestOH_IsConsignee()
		{
			organisation.OH_IsConsignee = true;
			AssertEquals(true, info.OH_IsConsignee);
		}

		public void TestOH_IsConsignor()
		{
			organisation.OH_IsConsignor = true;
			AssertEquals(true, info.OH_IsConsignor);
		}

		public void TestOH_IsForwarder()
		{
			organisation.OH_IsForwarder = true;
			AssertEquals(true, info.OH_IsForwarder);
		}

		public void TestOH_IsGlobalAccount()
		{
			organisation.OH_IsGlobalAccount = true;
			AssertEquals(true, info.OH_IsGlobalAccount);
		}

		public void TestOH_IsMiscFreightServices()
		{
			organisation.OH_IsMiscFreightServices = true;
			AssertEquals(true, info.OH_IsMiscFreightServices);
		}

		public void TestOH_IsNationalAccount()
		{
			organisation.OH_IsNationalAccount = true;
			AssertEquals(true, info.OH_IsNationalAccount);
		}

		public void TestOH_IsSalesLead()
		{
			organisation.OH_IsSalesLead = true;
			AssertEquals(true, info.OH_IsSalesLead);
		}

		public void TestOH_IsShippingProvider()
		{
			organisation.OH_IsShippingProvider = true;
			AssertEquals(true, info.OH_IsShippingProvider);
		}

		public void TestOH_IsTransportClient()
		{
			organisation.OH_IsTransportClient = true;
			AssertEquals(true, info.OH_IsTransportClient);
		}

		public void TestOH_IsWarehouseClient()
		{
			organisation.OH_IsWarehouseClient = true;
			AssertEquals(true, info.OH_IsWarehouseClient);
		}

		public void TestOH_Language()
		{
			organisation.OH_Language = "XYZ";
			AssertEquals("XYZ", info.OH_Language);
		}

		public void TestPK()
		{
			AssertEquals(organisation.PK, info.PK);
		}

		public void TestPortName()
		{
			organisation.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("Sydney", info.PortName);
		}

		public void TestUnlocoCodeIsEmptyByDefault()
		{
			AssertEquals(string.Empty, info.UnlocoCode);
		}

		public void TestUnlocoCodeComesFromClosestPort()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AUYUX";
			unloco.RL_IATA = "XYZ";
			organisation.OH_RL_NKClosestPort = "AUYUX";
			AssertEquals("AUYUX", info.UnlocoCode);
		}
	}
}
