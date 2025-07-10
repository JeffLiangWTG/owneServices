using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgTradeDetailJobCommonGrouping))]
	sealed class OrgTradeDetailForwardingShipmentGroupingTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues_CustomsBrokerage()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var salesEntity = EntitySalesWrapper.Get(sales, org);
			var brokerageProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var grouping = new OrgTradeDetailJobCommonGrouping(salesEntity.EntityTradeDetailsCompanyView, brokerageProduct);

			AssertEquals(OrgTradeDetailLookups.CustomsBrokerageTradeTypes.Import, grouping.TradeType);
		}

		#endregion

		#region Properties

		public void TestTradeMode_ChangesAlsoUpdatesAllElements()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var orgTradeDetailCollection = sales.TradeDetails;
			var tradeDetails1 = orgTradeDetailCollection.AddNew();
			tradeDetails1.PA_TradeMode = "MMM";
			var tradeDetails2 = orgTradeDetailCollection.AddNew();
			tradeDetails2.PA_TradeMode = "MMM";

			var product = Factory.New<OrgSalesProduct>();
			var salesEntity = EntitySalesWrapper.Get(sales, org);
			var entityTradeDetail1 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetails1.PK);
			var entityTradeDetail2 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetails2.PK);
			var grouping = new OrgTradeDetailJobCommonGrouping(salesEntity.EntityTradeDetailsCompanyView, product, "MMM", "", new[] { entityTradeDetail1, entityTradeDetail2 });
			grouping.TradeMode = "NNN";

			CombineAssertions("Changing grouping TradeMode should also update the trade mode for all elements", () =>
			{
				AssertEquals("tradeDetails1.PA_TradeMode", "NNN", tradeDetails1.PA_TradeMode);
				AssertEquals("tradeDetails2.PA_TradeMode", "NNN", tradeDetails2.PA_TradeMode);
			});
		}

		public void TestTradeType_ChangesAlsoUpdatesAllElements()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var orgTradeDetailCollection = sales.TradeDetails;
			var tradeDetails1 = orgTradeDetailCollection.AddNew();
			tradeDetails1.PA_TradeType = "TTT";
			var tradeDetails2 = orgTradeDetailCollection.AddNew();
			tradeDetails2.PA_TradeType = "TTT";

			var product = Factory.New<OrgSalesProduct>();
			var salesEntity = EntitySalesWrapper.Get(sales, org);
			var entityTradeDetail1 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetails1.PK);
			var entityTradeDetail2 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetails2.PK);
			var grouping = new OrgTradeDetailJobCommonGrouping(salesEntity.EntityTradeDetailsCompanyView, product, "", "TTT", new[] { entityTradeDetail1, entityTradeDetail2 });
			grouping.TradeType = "UUU";

			CombineAssertions("Changing grouping TradeType should also update the trade type for all elements", () =>
			{
				AssertEquals("tradeDetails1.PA_TradeType", "UUU", tradeDetails1.PA_TradeType);
				AssertEquals("tradeDetails2.PA_TradeType", "UUU", tradeDetails2.PA_TradeType);
			});
		}

		#endregion

		#region SalesValueAssociationPivotCollection

		public void TestSalesValueAssociationPivotCollectionAdditionalFilterShouldChangeOnFactorySaving()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var orgTradeDetailCollection = sales.TradeDetails;
			var tradeDetail1 = orgTradeDetailCollection.AddNew();
			tradeDetail1.PA_TradeType = "TTT";
			var tradeDetail2 = orgTradeDetailCollection.AddNew();
			tradeDetail2.PA_TradeType = "TTT";

			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var salesEntity = EntitySalesWrapper.Get(sales, org);
			var entityTradeDetail1 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetail1.PK);
			var entityTradeDetail2 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetail2.PK);
			var grouping = new OrgTradeDetailJobCommonGrouping(salesEntity.EntityTradeDetailsCompanyView, product, "", "TTT", new[] { entityTradeDetail1, entityTradeDetail2 });
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(org, entityTradeDetail1);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(org, entityTradeDetail2);

			Factory.Save();

			const string completeFilter = @"(
	SVP_TradeTableCode = 'PA' 
	AND
	SVP_TradeId IN 
	(
		SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OW = CONVERT
		(
			'{0}', 'System.Guid'
		)
	)
)
AND
(
	SVP_TradeId IN 
	(
		SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OW = CONVERT
		(
			'{0}', 'System.Guid'
		)
		AND
		PA_TradeMode = '{1}' 
		AND
		PA_TradeType = '{2}'
	)
)
";
			var initialFilter = string.Format(CultureInfo.InvariantCulture, completeFilter, sales.PK, ZString.Empty, "TTT");
			var finalFilter = string.Format(CultureInfo.InvariantCulture, completeFilter, sales.PK, "NNN", "MMM");

			AssertEquals("Precondition", ZString.Empty, grouping.TradeMode);
			AssertEquals("Precondition", "TTT", grouping.TradeType);
			AssertEquals("AdditionalFilter when mode and type are empty", FormattableString.Invariant($"SVP_TradeId IN (SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OW = CONVERT('{sales.PK}', 'System.Guid') and PA_TradeMode = '' and PA_TradeType = 'TTT')"), grouping.SalesAssociationPivotCollectionCompanyView.AdditionalFilter.LiteralTextADO);

			AssertEquals("CompleteFilter when mode and type are empty", initialFilter, grouping.SalesAssociationPivotCollectionCompanyView.CompleteFilter.LiteralTextADOFormatted);
			AssertEquals("Collection should include these trade details", 2, grouping.SalesAssociationPivotCollectionCompanyView.Count);

			grouping.TradeMode = "NNN";
			grouping.TradeType = "MMM";

			AssertEquals("AdditionalFilter should not change before saving", FormattableString.Invariant($"SVP_TradeId IN (SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OW = CONVERT('{sales.PK}', 'System.Guid') and PA_TradeMode = '' and PA_TradeType = 'TTT')"), grouping.SalesAssociationPivotCollectionCompanyView.AdditionalFilter.LiteralTextADO);
			AssertEquals("CompleteFilter should not change before saving", initialFilter, grouping.SalesAssociationPivotCollectionCompanyView.CompleteFilter.LiteralTextADOFormatted);
			AssertEquals("Collection should include this trade detail since additional filter has not been updated yet", 2, grouping.SalesAssociationPivotCollectionCompanyView.Count);

			Factory.Save();

			AssertEquals("AdditionalFilter after saving", FormattableString.Invariant($"SVP_TradeId IN (SELECT PA_PK FROM dbo.OrgTradeDetail WHERE PA_OW = CONVERT('{sales.PK}', 'System.Guid') and PA_TradeMode = 'NNN' and PA_TradeType = 'MMM')"), grouping.SalesAssociationPivotCollectionCompanyView.AdditionalFilter.LiteralTextADO);
			AssertEquals("CompleteFilter after saving", finalFilter, grouping.SalesAssociationPivotCollectionCompanyView.CompleteFilter.LiteralTextADOFormatted);
			AssertEquals("Reloaded collection should include this trade detail", 2, grouping.SalesAssociationPivotCollectionCompanyView.Count);
		}

		public void TestSalesValueAssociationPivotCollectionShouldBeRefreshedOnTradeTypeChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = Factory.NewWithValidTestData<OrgSales>();
			var orgTradeDetailCollection = sales.TradeDetails;
			var tradeDetail = orgTradeDetailCollection.AddNew();

			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var salesEntity = EntitySalesWrapper.Get(sales, org);
			var entityTradeDetail = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetail.PK);
			var grouping = new OrgTradeDetailJobCommonGrouping(salesEntity.EntityTradeDetailsCompanyView, product);
			grouping.Elements.Add(entityTradeDetail);
			OrgSalesValueAssociationPivot.AddPivotIfNotExist(org, entityTradeDetail);

			AssertEquals(ZString.Empty, grouping.TradeType);
			AssertEquals("Collection should include this trade detail", 1, grouping.SalesAssociationPivotCollectionCompanyView.Count);

			grouping.TradeType = "NNN";
			Factory.Save();

			AssertEquals("Reloaded collection should include this trade detail", 1, grouping.SalesAssociationPivotCollectionCompanyView.Count);
			AssertEquals("Reloaded collection should include this trade detail", tradeDetail.PK, grouping.SalesAssociationPivotCollectionCompanyView[0].SVP_TradeId);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var orgTradeDetailCollection = sales.TradeDetails;
			var tradeDetails1 = orgTradeDetailCollection.AddNew();
			tradeDetails1.PA_TradeMode = "MMM";
			tradeDetails1.PA_TradeType = "TTT";
			var tradeDetails2 = orgTradeDetailCollection.AddNew();
			tradeDetails2.PA_TradeMode = "MMM";
			tradeDetails2.PA_TradeType = "TTT";

			var product = Factory.New<OrgSalesProduct>();
			var salesEntity = EntitySalesWrapper.Get(sales, org);
			var entityTradeDetail1 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetails1.PK);
			var entityTradeDetail2 = salesEntity.EntityTradeDetails.Single(x => x.PK == tradeDetails2.PK);
			var grouping = new OrgTradeDetailJobCommonGrouping(salesEntity.EntityTradeDetailsCompanyView, product, "MMM", "TTT", new[] { entityTradeDetail1, entityTradeDetail2 });
			grouping.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("tradeDetails1.IsDeleted", true, tradeDetails1.IsDeleted);
				AssertEquals("tradeDetails2.IsDeleted", true, tradeDetails2.IsDeleted);
				AssertEquals("orgTradeDetailCollection.Count", 0, orgTradeDetailCollection.Count);
			});
		}

		public void TestCanDelete()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = product.PK;
			var tradeDetails11 = tradeLane1.TradeDetails.AddNew();
			tradeDetails11.PA_TradeMode = "MMM";
			tradeDetails11.PA_TradeType = "TTT";
			var tradeDetails12 = tradeLane1.TradeDetails.AddNew();
			tradeDetails12.PA_TradeMode = "MMM";
			tradeDetails12.PA_TradeType = "TTT";

			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = product.PK;
			var tradeDetails21 = tradeLane2.TradeDetails.AddNew();
			tradeDetails21.PA_TradeMode = "AAA";
			tradeDetails21.PA_TradeType = "CCC";
			var tradeDetails22 = tradeLane2.TradeDetails.AddNew();
			tradeDetails22.PA_TradeMode = "AAA";
			tradeDetails22.PA_TradeType = "CCC";

			var opportunity1 = org.SalesOpportunities.AddNew();
			tradeLane1.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity1);
			tradeDetails11.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity1);
			tradeDetails12.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity1);

			tradeLane2.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity1);
			tradeDetails21.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity1);
			tradeDetails22.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity1);

			var opportunity2 = org.SalesOpportunities.AddNew();
			tradeLane1.SalesAssociationPivotCollectionCompanyView.AddNew(opportunity2);

			Factory.Save();

			var entitySales1 = EntitySalesWrapper.Get(tradeLane1, opportunity1);
			var entitySales2 = EntitySalesWrapper.Get(tradeLane2, opportunity1);

			var entityTradeDetails11 = entitySales1.EntityTradeDetails.Single(x => x.PK == tradeDetails11.PK);
			var entityTradeDetails12 = entitySales1.EntityTradeDetails.Single(x => x.PK == tradeDetails12.PK);
			var entityTradeDetails21 = entitySales2.EntityTradeDetails.Single(x => x.PK == tradeDetails21.PK);
			var entityTradeDetails22 = entitySales2.EntityTradeDetails.Single(x => x.PK == tradeDetails22.PK);
			var grouping1 = new OrgTradeDetailJobCommonGrouping(entitySales1.EntityTradeDetailsCompanyView, product, "MMM", "TTT", new[] { entityTradeDetails11, entityTradeDetails12 });
			var grouping2 = new OrgTradeDetailJobCommonGrouping(entitySales2.EntityTradeDetailsCompanyView, product, "AAA", "CCC", new[] { entityTradeDetails21, entityTradeDetails22 });

			AssertEquals(false, grouping1.CanDelete);
			AssertEquals(true, grouping2.CanDelete);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var product = Factory.New<OrgSalesProduct>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			return new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
		}

		#endregion
	}
}
