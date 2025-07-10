using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobTradeLaneVoyage))]
	sealed class JobTradeLaneVoyageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNB_EJ()
		{
			AssertHasCustomAttribute(typeof(JobTradeLaneVoyage),
				JobTradeLaneVoyageSchema.Constants.NB_EJ, false, (ListAttribute la) => la.ListDataSourceMember == "Lookups.JobTradeLaneList");
			AssertHasCustomAttribute(typeof(JobTradeLaneVoyage),
				JobTradeLaneVoyageSchema.Constants.NB_EJ, false, (RelatedBusinessObjectAttribute rbo) => rbo.RelatedBizObjName == "TradeLane");

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUPER";
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			tradeLane.EJ_Code = "STL";
			tradeLane.EJ_Location1 = "AUSYD";
			tradeLane.EJ_Location2 = "HKHKG";
			tradeLane.EJ_OH_RelatedOrg = principal.PK;
			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			jobTradeLaneVoyage.NB_JV = voyage.PK;
			jobTradeLaneVoyage.NB_EJ = tradeLane.PK;

			Factory.Save();

			AssertEquals("Principal should be proxied from tradelane", jobTradeLaneVoyage.NB_OH, principal.PK);
		}

		public void TestTradeLane()
		{
			JobTradeLane tradeLane = Factory.New<JobTradeLane>();
			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.NewWithValidTestData<JobTradeLaneVoyage>();
			jobTradeLaneVoyage.NB_EJ = tradeLane.PK;
			AssertEquals(tradeLane, jobTradeLaneVoyage.TradeLane);
		}

		public void TestVoyage()
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.NewWithValidTestData<JobTradeLaneVoyage>();
			jobTradeLaneVoyage.NB_JV = voyage.PK;
			AssertEquals(voyage, jobTradeLaneVoyage.Voyage);
		}

		public void TestPrincipal()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			jobTradeLaneVoyage.NB_OH = principal.PK;

			AssertEquals(principal, jobTradeLaneVoyage.Principal);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var voyage = factory.NewWithValidTestData<JobVoyage>();
			var principal = factory.NewWithValidTestData<OrgHeader>();

			var tradeLane = factory.NewWithValidTestData<JobTradeLane>();
			tradeLane.EJ_OH_RelatedOrg = principal.PK;

			var result = factory.New<JobTradeLaneVoyage>();
			result.NB_JV = voyage.PK;
			result.NB_EJ = tradeLane.PK;

			return result;
		}
	}
}
