using System.Collections.Generic;
using System.ComponentModel;
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
	[TestedType(typeof(EntitySalesWrapperCollectionProductView))]
	sealed class EntitySalesWrapperCollectionProductViewTest : BusinessObjectCollectionViewTestCase<EntitySalesWrapperCollectionProductView>
	{
		#region Default Values

		public void TestDefaultValuesForNewChild()
		{
			var salesProductAAA = Factory.New<OrgSalesProduct>();
			salesProductAAA.MP_Code = "AAA";
			var salesProductBBB = Factory.New<OrgSalesProduct>();
			salesProductBBB.MP_Code = "BBB";

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUMEL";
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeaderAAA = salesHeaderCollection.AddNew(salesProductAAA);
			var collectionAAAProductView = salesHeaderAAA.EntitySalesCollectionProductView;
			var sales1 = collectionAAAProductView.AddNew();
			AssertEquals(salesProductAAA.PK, sales1.OW_MP_Product);
			AssertEquals("AUMEL", sales1.OriginCode);

			var salesHeaderBBB = salesHeaderCollection.AddNew(salesProductBBB);
			var collectionBBBProductView = salesHeaderBBB.EntitySalesCollectionProductView;
			var sales2 = collectionBBBProductView.AddNew();
			AssertEquals(salesProductBBB.PK, sales2.OW_MP_Product);
			AssertEquals("AUMEL", sales2.OriginCode);
		}

		#endregion

		#region Relationship

		public void TestRelationship()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesProduct1 = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesProduct2 = Factory.NewWithValidTestData<OrgSalesProduct>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader1 = salesHeaderCollection.AddNew(salesProduct1);
			var salesHeader2 = salesHeaderCollection.AddNew(salesProduct2);

			var sales1A = salesHeader1.EntitySalesCollectionProductView.AddNew();
			var sales1B = salesHeader1.EntitySalesCollectionProductView.AddNew();

			var sales2A = salesHeader2.EntitySalesCollectionProductView.AddNew();
			var sales2B = salesHeader2.EntitySalesCollectionProductView.AddNew();

			Factory.Save();

			var collectionProductView = salesHeader1.EntitySalesCollectionProductView;
			AssertContainsExactElementsInAnyOrder(new[] { sales1A, sales1B }, collectionProductView);

			collectionProductView = salesHeader2.EntitySalesCollectionProductView;
			AssertContainsExactElementsInAnyOrder(new[] { sales2A, sales2B }, collectionProductView);
		}

		public void TestHasChangesFromDelete()
		{
			var shipments = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var opportunitySalesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			var shipmentHeader = opportunitySalesHeaderCollection.AddNew(shipments);
			var sales = shipmentHeader.EntitySalesCollectionProductView.AddNew();
			sales.OW_MP_Product = shipments.PK;

			Factory.Save();

			var collection = shipmentHeader.EntitySalesCollectionProductView;
			AssertEquals("Precondition", 1, collection.Count);
			AssertEquals(false, collection.HasChanges);

			var hasChangesFired = false;
			shipmentHeader.HasChangesChanged += (sender, e) =>
			{
				hasChangesFired = true;
			};

			collection.RemoveAndDelete(sales);
			AssertEquals(0, collection.Count);
			AssertEquals(true, collection.HasChanges);
			AssertEquals(true, hasChangesFired);
		}

		#endregion

		#region Estimate Value Changed Event

		public void TestEstimatedValueChangedEvent()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(salesProduct);
			var collection = salesHeader.EntitySalesCollectionProductView;
			var estimatedValueChangedEventFired = false;
			collection.EstimatedValueChanged += (sender, e) =>
				{
					estimatedValueChangedEventFired = true;
				};

			var sales = collection.AddNew();
			AssertEquals("Event should fire after adding an element", true, estimatedValueChangedEventFired);

			estimatedValueChangedEventFired = false;
			sales.OW_IsCustomRevenue = true;
			sales.OW_AnnualRevenue = 100;
			AssertEquals("Event should fire after changing element revenue", true, estimatedValueChangedEventFired);

			estimatedValueChangedEventFired = false;
			collection.Remove(sales);
			AssertEquals("Event should fire after removing an element", true, estimatedValueChangedEventFired);

			estimatedValueChangedEventFired = false;
			sales.OW_AnnualRevenue = 200;
			AssertEquals("Event should no longer fire after changing element revenue once remove from collection", false, estimatedValueChangedEventFired);
		}

		public void TestEstimatedValueChangedEvent_IsNotInvokedHeapsOfTimesDuringLoad()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(salesProduct);
			var collection = salesHeader.EntitySalesCollectionProductView;
			for (var i = 0; i < 10; i++)
			{
				collection.AddNew();
			}

			var totalEstimatedValueChangedEventFired = 0;
			collection.EstimatedValueChanged += (sender, e) =>
			{
				totalEstimatedValueChangedEventFired++;
			};

			org.SalesCollection.Load();

			AssertEquals(0, totalEstimatedValueChangedEventFired);
		}

		#endregion

		#region Sales Matching Property Changed Event

		public void TestSalesMatchingPropertyChangedEvent()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var salesCollection = salesHeader.EntitySalesCollectionProductView;
			var sales1 = salesCollection.AddNew();
			sales1.OW_IsTraded = false;
			sales1.OW_MP_Product = product.PK;
			var sales2 = salesCollection.AddNew();
			sales2.OW_IsTraded = false;
			sales2.OW_MP_Product = product.PK;

			var entitySales1 = salesCollection.Cast<EntitySalesWrapper>().Single(x => x.PK == sales1.PK);
			var entitySales2 = salesCollection.Cast<EntitySalesWrapper>().Single(x => x.PK == sales2.PK);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { entitySales1, entitySales2 }, salesCollection);

			var salesMatchingPropertyChangedEventFiredSenders = new List<EntitySalesWrapper>();
			salesCollection.SalesMatchingPropertyChanged += (sender, e) =>
				{
					salesMatchingPropertyChangedEventFiredSenders.Add((EntitySalesWrapper)sender);
				};

			sales1.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "AU", RefCountrySchema.Constants.Prefix).PK;
			AssertArrayEqualsByElements(
				new[] { entitySales1 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			sales2.OW_DestinationID = ViewLocationHelper.GetLocationFromString(Factory, "AUSYD", RefUNLOCOSchema.Constants.Prefix).PK;
			AssertArrayEqualsByElements(
				new[] { entitySales1, entitySales2 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			sales1.OW_OriginID = ZGuid.Empty;
			AssertArrayEqualsByElements(
				new[] { entitySales1, entitySales2, entitySales1 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			var entitySales3 = salesCollection.AddNew();
			entitySales3.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "US", RefCountrySchema.Constants.Prefix).PK;
			AssertArrayEqualsByElements(
				new[] { entitySales1, entitySales2, entitySales1, entitySales3 },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());

			var uncommittedEntitySales = ((EntitySalesWrapper)((IBindingList)salesCollection).AddNew());
			uncommittedEntitySales.OW_OriginID = ViewLocationHelper.GetLocationFromString(Factory, "US", RefCountrySchema.Constants.Prefix).PK;
			AssertArrayEqualsByElements(
				new[] { entitySales1, entitySales2, entitySales1, entitySales3, uncommittedEntitySales },
				salesMatchingPropertyChangedEventFiredSenders.ToArray());
		}

		#endregion

		#region Overrides

		protected override EntitySalesWrapperCollectionProductView GetCollectionToTest()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(salesProduct);
			return salesHeader.EntitySalesCollectionProductView;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EntitySalesWrapper>();
		}

		#endregion
	}
}
