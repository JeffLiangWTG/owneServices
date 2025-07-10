using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesValueAssociationPivot))]
	sealed class OrgSalesValueAssociationPivotTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2011, 1, 1)]
		public void TestOnSaving_ShouldSetLatestProspectDateIfAttachingOpportunityToOrgSales()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var tradeLane = Factory.NewWithValidTestData<OrgSales>();
			tradeLane.SalesAssociationPivotCollectionGlobal.AddNew(opportunity);
			AssertEquals(ZDate.Empty, tradeLane.OW_LatestProspectDate);

			Factory.Save();
			AssertEquals(new ZDate(2011, 1, 1), tradeLane.OW_LatestProspectDate);
		}

		[TestDate(2011, 1, 1)]
		public void TestOnSaving_ShouldNotSetLatestProspectDateIfAttachingOrgToOrgSales()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tradeLane = Factory.NewWithValidTestData<OrgSales>();
			tradeLane.SalesAssociationPivotCollectionGlobal.AddNew(org);
			AssertEquals(ZDate.Empty, tradeLane.OW_LatestProspectDate);

			Factory.Save();
			AssertEquals(ZDate.Empty, tradeLane.OW_LatestProspectDate);
		}

		#region Default Values

		public void TestDefaultValues()
		{
			var pivot = Factory.New<OrgSalesValueAssociationPivot>();
			AssertEquals("", pivot.SVP_TradeTableCode);
		}

		#endregion

		#region Fetch Strategy

		public void TestFetchStrategy()
		{
			var pivot = Factory.New<OrgSalesValueAssociationPivot>();
			AssertType(typeof(SalesValueAssociationPivotFetchStrategy), pivot.FetchStrategy);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			var pivot = Factory.NewWithValidTestData<OrgSalesValueAssociationPivot>();
			pivot.SVP_ActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot.SVP_ActivityId = opp.PK;
			pivot.SVP_TradeTableCode = OrgSalesSchema.Constants.Prefix;
			pivot.SVP_TradeId = sales.PK;
			return pivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
