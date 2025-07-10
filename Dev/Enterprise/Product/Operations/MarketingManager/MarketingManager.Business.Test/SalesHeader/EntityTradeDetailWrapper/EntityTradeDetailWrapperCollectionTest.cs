using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntityTradeDetailWrapperCollection))]
	sealed class EntityTradeDetailWrapperCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Relationship

		public void TestRelationship_Warehouse_Opportunity()
		{
			var anotherFactory = new BusinessObjectFactory();
			var warehouseProduct = anotherFactory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var orgInOtherFactory = anotherFactory.NewWithValidTestData<OrgHeader>();
			var oppInOtherFactory = orgInOtherFactory.SalesOpportunities.AddNew();
			var salesInOtherFactory = orgInOtherFactory.SalesCollection.AddNew();
			salesInOtherFactory.OW_MP_Product = warehouseProduct.PK;
			var tradeDetail1InOtherFactory = salesInOtherFactory.TradeDetails.AddNew();
			var tradeDetail2InOtherFactory = salesInOtherFactory.TradeDetails.AddNew();
			var tradeDetail3InOtherFactory = salesInOtherFactory.TradeDetails.AddNew();
			tradeDetail1InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(oppInOtherFactory);
			tradeDetail2InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(oppInOtherFactory);
			tradeDetail3InOtherFactory.SalesAssociationPivotCollectionGlobal.DeleteAll();

			anotherFactory.Save();

			var org = Factory.Load<OrgHeader>(orgInOtherFactory.PK);
			var opp = Factory.Load<OrgOpportunity>(oppInOtherFactory.PK);
			var sales = Factory.Load<OrgSales>(salesInOtherFactory.PK);
			var oppSales = EntitySalesWrapper.Get(sales, opp);
			var oppTradeDetails = new EntityTradeDetailWrapperCollection(oppSales);
			oppTradeDetails.Load();
			AssertContainsExactElementsInAnyOrder("Should only include trade details associated to opportunity",
				new[] { tradeDetail1InOtherFactory, tradeDetail2InOtherFactory }.Select(x => x.PK),
				oppTradeDetails.Select(x => x.PK));

			var newTradeDetail = oppTradeDetails.AddNew();
			Factory.Save();

			AssertEquals("Should include the 3 details which are a part of the OrgSales", 3, newTradeDetail.SalesAssociationPivotCollectionGlobal.Count);
			var salesAssocationPivot = newTradeDetail.SalesAssociationPivotCollectionGlobal.Single(x => x.SVP_TradeId == newTradeDetail.PK);
			AssertEquals(opp.PK, salesAssocationPivot.SVP_ActivityId);
			AssertEquals(opp.TablePrefix, salesAssocationPivot.SVP_ActivityTableCode);
		}

		public void TestRelationship_Warehouse_Org()
		{
			var anotherFactory = new BusinessObjectFactory();
			var warehouseProduct = anotherFactory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var orgInOtherFactory = anotherFactory.NewWithValidTestData<OrgHeader>();
			var oppInOtherFactory = orgInOtherFactory.SalesOpportunities.AddNew();
			var salesInOtherFactory = orgInOtherFactory.SalesCollection.AddNew();
			salesInOtherFactory.OW_MP_Product = warehouseProduct.PK;
			var tradeDetail1InOtherFactory = salesInOtherFactory.TradeDetails.AddNew();
			var tradeDetail2InOtherFactory = salesInOtherFactory.TradeDetails.AddNew();
			var tradeDetail3InOtherFactory = salesInOtherFactory.TradeDetails.AddNew();
			tradeDetail1InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(oppInOtherFactory);
			tradeDetail2InOtherFactory.SalesAssociationPivotCollectionGlobal.AddNew(oppInOtherFactory);
			tradeDetail3InOtherFactory.SalesAssociationPivotCollectionGlobal.DeleteAll();

			anotherFactory.Save();

			var org = Factory.Load<OrgHeader>(orgInOtherFactory.PK);
			var opp = Factory.Load<OrgOpportunity>(oppInOtherFactory.PK);
			var sales = Factory.Load<OrgSales>(salesInOtherFactory.PK);
			var orgSales = EntitySalesWrapper.Get(sales, org);
			var orgTradeDetails = new EntityTradeDetailWrapperCollection(orgSales);
			orgTradeDetails.Load();
			AssertContainsExactElementsInAnyOrder("Should include all trade details associated",
				new[] { tradeDetail1InOtherFactory, tradeDetail2InOtherFactory, tradeDetail3InOtherFactory }.Select(x => x.PK),
				orgTradeDetails.Select(x => x.PK));

			var newTradeDetail = orgTradeDetails.AddNew();
			Factory.Save();

			AssertEquals("Should include the 3 details which are a part of the OrgSales", 3, newTradeDetail.SalesAssociationPivotCollectionGlobal.Count);
			var salesAssocationPivot = newTradeDetail.SalesAssociationPivotCollectionGlobal.Single(x => x.SVP_TradeId == newTradeDetail.PK);
			AssertEquals(org.PK, salesAssocationPivot.SVP_ActivityId);
			AssertEquals(org.TablePrefix, salesAssocationPivot.SVP_ActivityTableCode);

			orgTradeDetails.Remove(newTradeDetail);
			AssertEquals("Should have removed the association", 2, newTradeDetail.SalesAssociationPivotCollectionGlobal.Count);
		}

		public void TestAddNewShouldSetTradeStatus()
		{
			var defaultValue = new OpportunityStatusCollection(defaultBoolForNewChild: false, defaultEffectiveAgreementForNewChild: false);
			defaultValue.Add("CRT", (NoResString)"Current", effectiveAgreement: false, booleanValue: false, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Active);
			defaultValue.Add("WON", (NoResString)"Won", effectiveAgreement: true, booleanValue: true, enabled: true, tradeStatus: OpportunityTradeStatus.Codes.Successful);

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();
			var opp = org.SalesOpportunities.AddNew();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var salesCollection = salesHeader.EntitySalesCollectionProductView;
			var sales = salesCollection.AddNew();

			opp.P8_Status = "WON";

			var orgSales = EntitySalesWrapper.Get(sales, opp);
			var tradeDetailCollection = orgSales.EntityTradeDetailsCollection;
			var tradeDetail1 = tradeDetailCollection.AddNew();
			AssertEquals(OpportunityTradeStatus.Codes.Successful, tradeDetail1.PA_Status);

			opp.P8_Status = "CRT";
			var tradeDetail2 = tradeDetailCollection.AddNew();
			AssertEquals(OpportunityTradeStatus.Codes.Active, tradeDetail2.PA_Status);
		}

		#endregion

		#region Sales Matching Property Changed Event

		public void TestSalesMatchingPropertyChangedEvent()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var salesCollection = salesHeader.EntitySalesCollectionProductView;
			var sales = salesCollection.AddNew();
			var tradeDetailCollection = sales.EntityTradeDetailsCollection;
			var tradeDetail1 = tradeDetailCollection.AddNew();
			var tradeDetail2 = tradeDetailCollection.AddNew();

			var salesMatchingPropertyChangedEventFiredSenders = new List<EntityTradeDetailWrapper>();
			tradeDetailCollection.SalesMatchingPropertyChanged += (sender, e) =>
			{
				salesMatchingPropertyChangedEventFiredSenders.Add((EntityTradeDetailWrapper)sender);
			};

			tradeDetail1.PA_OP = Factory.New<OrgSupplierPart>().PK;
			AssertArrayEqualsByElements(
				new[] { tradeDetail1 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			tradeDetail2.PA_OP = Factory.New<OrgSupplierPart>().PK;
			AssertArrayEqualsByElements(
				new[] { tradeDetail1, tradeDetail2 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			tradeDetail1.PA_OP = ZGuid.Empty;
			AssertArrayEqualsByElements(
				new[] { tradeDetail1, tradeDetail2, tradeDetail1 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			var tradeDetail3 = tradeDetailCollection.AddNew();
			tradeDetail3.PA_OP = Factory.New<OrgSupplierPart>().PK;
			AssertArrayEqualsByElements(
				new[] { tradeDetail1, tradeDetail2, tradeDetail1, tradeDetail3 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			var uncommittedTradeDetail = ((EntityTradeDetailWrapper)((IBindingList)tradeDetailCollection).AddNew());
			uncommittedTradeDetail.PA_OP = Factory.New<OrgSupplierPart>().PK;
			AssertArrayEqualsByElements(
				new[] { tradeDetail1, tradeDetail2, tradeDetail1, tradeDetail3, uncommittedTradeDetail },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());
		}

		#endregion

		#region Overrides

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			return new EntityTradeDetailWrapperCollection(entitySales);
		}

		#endregion
	}
}
