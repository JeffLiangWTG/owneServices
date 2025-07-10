using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal class RatingHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCompaniesList()
		{
			var rateHeader = Factory.New<RatingHeader>();
			AssertNotNull("Companies list should exist", rateHeader.Lookups.Companies);
			foreach (var company in rateHeader.Lookups.Companies)
			{
				Assert("Shouldn't contain Demo company", company.GC_Code.ToLower().IndexOf("dem") == -1);
			}
		}

		public void TestClientsList()
		{
			var org1 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			var org2 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			org2.OH_IsConsignee = true;
			var org3 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			org3.OH_IsConsignor = true;
			var org4 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			org4.OH_IsSalesLead = true;
			var org5 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			org5.OH_IsWarehouseClient = true;
			var org6 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			org6.OH_IsTransportClient = true;
			var org7 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			org7.OH_IsBroker = true;
			var org8 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			var company1 = Factory.New<GlbCompany>();
			company1.GC_OH_OrgProxy = org8.PK;
			var org9 = Helper.NewOrgHeaderWithoutSettingDebtorCreditor();
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch1.GB_OH_OrgProxy = org9.PK;
			Factory.Save();

			var collection = Factory.New<Quote>().Lookups.Clients;
			collection.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "TESTORG"));
			Assert(!collection.Contains(org1));
			Assert(collection.Contains(org2));
			Assert(collection.Contains(org3));
			Assert(collection.Contains(org4));
			Assert(collection.Contains(org5));
			Assert(collection.Contains(org6));
			Assert(!collection.Contains(org7));
			Assert(!collection.Contains(org8));
			Assert(!collection.Contains(org9));

			collection = Factory.New<Costing>().Lookups.Clients;
			collection.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "TESTORG"));
			Assert(!collection.Contains(org1));
			Assert(!collection.Contains(org2));
			Assert(!collection.Contains(org3));
			Assert(!collection.Contains(org4));
			Assert(!collection.Contains(org5));
			Assert(!collection.Contains(org6));
			Assert(collection.Contains(org7));
			Assert(!collection.Contains(org8));
			Assert(!collection.Contains(org9));

			collection = Factory.New<ClientRate>().Lookups.Clients;
			collection.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "TESTORG"));
			Assert(!collection.Contains(org1));
			Assert(collection.Contains(org2));
			Assert(collection.Contains(org3));
			Assert(collection.Contains(org4));
			Assert(collection.Contains(org5));
			Assert(collection.Contains(org6));
			Assert(!collection.Contains(org7));
			Assert(!collection.Contains(org8));
			Assert(!collection.Contains(org9));

			collection = Factory.New<IntercompanyTariff>().Lookups.Clients;
			collection.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "TESTORG"));
			Assert(!collection.Contains(org1));
			Assert(!collection.Contains(org2));
			Assert(!collection.Contains(org3));
			Assert(!collection.Contains(org4));
			Assert(!collection.Contains(org5));
			Assert(!collection.Contains(org6));
			Assert(!collection.Contains(org7));
			Assert(collection.Contains(org8));
			Assert(collection.Contains(org9));
		}

		public void TestRateClientsNotificationMessageWhenAdditionalFilterNotMet()
		{
			var org1 = Helper.NewOrgHeader();

			var collection = Factory.New<ClientRate>().Lookups.Clients;
			AssertEquals(ErrorMessages.InvalidClientRateHeader, collection.GetAllNotificationsWhenAdditionalFilterNotMet(org1));
		}

		#region Implementation

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
