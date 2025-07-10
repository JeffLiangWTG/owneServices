using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusReconDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApplicationCodeList()
		{
			var applicationCodeList = lookups.ApplicationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "CLS", applicationCodeList.CodesAsString);
				AssertSame("Cached", applicationCodeList, lookups.ApplicationCodeList);
			});
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var customsOfficeTestHelper = new CustomsOfficeCodeTestHelper(helper);
			Factory.Save();

			using (declaration.Branch.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var zzCodeList1 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofDE.PK);
				var zzCodeList2 = Factory.Load<ZZRefCusCodeListCombined>(customsOfficeTestHelper.CusofIT.PK);
				var completeFilter = declaration.Lookups.CustomsOfficeList.CompleteFilter;
				CombineAssertions(() =>
				{
					AssertEquals("zzCodeList1, correct country", true, zzCodeList1.MatchesFilter(completeFilter));
					AssertEquals("zzCodeList2, invalid country", false, zzCodeList2.MatchesFilter(completeFilter));
				});
			}
		}

		public void TestAuthorizationList()
		{
			var authorizationList = lookups.AuthorizationList;
			CombineAssertions(() =>
			{
				AssertType<CusAuthorisationHeaderCollection>("Type", authorizationList);
				AssertSame("Cached", authorizationList, lookups.AuthorizationList);
			});
		}

		public void TestDeclarationTypeList()
		{
			var declarationTypeList = lookups.DeclarationTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "BWH, FRC, IWP", declarationTypeList.CodesAsString);
				AssertSame("Cached", declarationTypeList, lookups.DeclarationTypeList);
			});
		}

		public void TestDeclarantTypeList()
		{
			AssertEquals(string.Empty, lookups.DeclarantTypeList.CodesAsString);
		}

		public void TestMessageStatusList()
		{
			var list = lookups.MessageStatusList;
			CombineAssertions(() =>
			{
				AssertType<MessageStatusList>("Type", list);
				AssertSame("Cached", list, lookups.MessageStatusList);
			});
		}

		public void TestDeclarantList()
		{
			var declarantList = lookups.DeclarantList;
			CombineAssertions(() =>
			{
				AssertType<OrganisationsFindBoxCollection>("Type", declarantList);
				AssertSame("Cached", declarantList, lookups.DeclarantList);
			});
		}

		public void TestRepresentativeList()
		{
			var representativeList = lookups.RepresentativeList;
			CombineAssertions(() =>
			{
				AssertType<BrokerCollection>("Type", representativeList);
				AssertSame("Cached", representativeList, lookups.RepresentativeList);
			});
		}

		public void TestBuyingAgentList()
		{
			var buyingAgentList = lookups.BuyingAgentList;
			CombineAssertions(() =>
			{
				AssertType<OrganisationsFindBoxCollection>("Type", buyingAgentList);
				AssertSame("Cached", buyingAgentList, lookups.BuyingAgentList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<CusReconDeclaration>();
			declaration.CRD_ApplicationCode = CusReconDeclarationApplicationCodeList.Codes.CLS;
			declaration.CRD_JobReferenceNumber = "1";
			lookups = declaration.Lookups;
		}
		CusReconDeclaration declaration;
		CusReconDeclarationLookups lookups;
	}
}
