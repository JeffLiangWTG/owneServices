using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgTradeDetailJobCommonGroupingElements))]
	sealed class OrgTradeDetailJobCommonGroupingElementsTest : BusinessObjectCollectionTestCase
	{
		#region Default Values

		public void TestDefaultsForNewChild()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			grouping.TradeMode = "AAA";
			grouping.TradeType = "BBB";

			var elements = new OrgTradeDetailJobCommonGroupingElements(grouping);
			var newElement = elements.AddNew();

			AssertEquals("AAA", newElement.PA_TradeMode);
			AssertEquals("BBB", newElement.PA_TradeType);
		}

		#endregion

		#region Add / Remove

		public void TestAdd_ShouldAlsoAddToInnerTradeDetailsCollection()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var entitySales = salesHeader.FilterableEntitySalesCollection.AddNew();
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			var elements = new OrgTradeDetailJobCommonGroupingElements(grouping);

			var element = elements.AddNew();
			AssertCollectionContains(element, entitySales.EntityTradeDetails);
		}

		public void TestAddUncommitted()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var entitySales = salesHeader.FilterableEntitySalesCollection.AddNew();
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			var elements = new OrgTradeDetailJobCommonGroupingElements(grouping);

			var element = (OrgTradeDetail)elements.AddNew();
			AssertEquals(product, element.SalesProduct);
		}

		public void TestAddUncommittedShouldSetTradeStatus()
		{
			var defaultValue = new OpportunityStatusCollection(defaultBoolForNewChild: false, defaultEffectiveAgreementForNewChild: false);
			defaultValue.Add("CRT", (NoResString)"Current", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			defaultValue.Add("WON", (NoResString)"Won", effectiveAgreement: true, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Successful);

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();

			opp.P8_Status = "WON";

			var salesHeaderCollection = (SalesHeaderCollection)opp.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var entitySales = salesHeader.FilterableEntitySalesCollection.AddNew();
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			var elements = new OrgTradeDetailJobCommonGroupingElements(grouping);

			var element = (OrgTradeDetail)elements.AddNew();
			AssertEquals(OpportunityTradeStatus.Codes.Successful, element.PA_Status);
		}

		public void TestRemove_ShouldAlsoRemoveFromInnerTradeDetailsCollection()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var entitySales = salesHeader.FilterableEntitySalesCollection.AddNew();
			var entityTradeDetail = entitySales.EntityTradeDetailsCollection.AddNew();

			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product, "", "", new[] { entityTradeDetail });

			AssertCollectionContains("Precondition", entityTradeDetail, grouping.Elements);

			grouping.Elements.Remove(entityTradeDetail);

			AssertCollectionNotContains(entityTradeDetail, entitySales.EntityTradeDetailsCollection);
		}

		#endregion

		#region Overrides

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var product = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			var grouping = new OrgTradeDetailJobCommonGrouping(entitySales.EntityTradeDetailsCompanyView, product);
			return new OrgTradeDetailJobCommonGroupingElements(grouping);
		}

		#endregion
	}
}
