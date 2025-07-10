using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrderLine))]
	class ImportedOrderLineTest : ImportedObjectTest
	{
		#region Relationships

		public void TestDeliveries()
		{
			AmendedOrderLine.Deliveries.DeleteAll();
			OrderLineDelivery delivery1 = AmendedOrderLine.Deliveries.AddNew();
			OrderLineDelivery delivery2 = AmendedOrderLine.Deliveries.AddNew();
			delivery1.J4_Allocated = 1;
			delivery2.J4_Allocated = 2;
			AssertEquals("Delivery line 1", 1m, AmendedImportedOrderLine.Deliveries[0].J4_Allocated.Value);
			AssertEquals("Delivery line 2", 2m, AmendedImportedOrderLine.Deliveries[1].J4_Allocated.Value);
		}

		#endregion

		#region Properties

		public void TestIsProductOnFile()
		{
			AmendedOrderLine.JO_Partno = "NotOnFile";
			AssertEquals("Not on file", false, AmendedImportedOrderLine.IsProductOnFile);

			OrgSupplierPart product = Factory.New<OrgSupplierPart>();
			product.RelatedOrganisations.AddOrganisationIfNotExist(AmendedOrder.BuyerPK, OrgPartRelation.RelationshipTypes.Owner);
			product.OP_PartNum = "PartNumber";

			AmendedOrderLine.JO_Partno = product.OP_PartNum;
			AssertEquals("Product on file", true, AmendedImportedOrderLine.IsProductOnFile);
		}

		public void TestIsUNDGSubstanceValid()
		{
			if (AmendedOrderLine.UNDGs.Count == 0)
			{
				AmendedOrderLine.UNDGs.AddNew();
			}

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1234";
			subs.DG_Variant = "v";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			AmendedOrderLine.UNDGs[0].LinkDefault(subs);
			AssertEquals("Dg Substance code is in database", true, AmendedImportedOrderLine.IsUNDGSubstanceValid);

			AmendedOrderLine.UNDGs[0].UNDGSubstancePivotCollection.RemoveAllFromRelationship();
			AssertEquals("Empty Dg Substance code", true, AmendedImportedOrderLine.IsUNDGSubstanceValid);
		}

		#endregion

		#region Amended Properties

		public void TestJO_LineNoAndSplitAndSubLine()
		{
			AmendedOrderLine.JO_LineNo = 1;
			AssertEquals("1", AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.DisplayValue);

			AmendedOrderLine.JO_LineNo = 1;
			AmendedOrderLine.JO_SubLineNo = 2;
			AssertEquals("1 sub 2", AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.DisplayValue);

			AmendedOrderLine.JO_LineNo = 1;
			AmendedOrderLine.JO_SubLineNo = 2;
			AmendedOrderLine.JO_LineSplitNumber = 3;
			AssertEquals("1.3 sub 2", AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.DisplayValue);
		}

		public void TestJO_LineNoAndSplitAndSubLine_State()
		{
			AssertEquals(ImportedPropertyState.Unchanged, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);

			AmendedOrderLine.JO_LineNo = 2;
			AssertEquals(ImportedPropertyState.Modified, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);
			AmendedOrderLine.JO_LineNo = 1;
			AssertEquals(ImportedPropertyState.Unchanged, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);

			AmendedOrderLine.JO_SubLineNo = 2;
			AssertEquals(ImportedPropertyState.Modified, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);
			AmendedOrderLine.JO_SubLineNo = 1;
			AssertEquals(ImportedPropertyState.Unchanged, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);

			AmendedOrderLine.JO_LineSplitNumber = 2;
			AssertEquals(ImportedPropertyState.Modified, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);
			AmendedOrderLine.JO_LineSplitNumber = 0;
			AssertEquals(ImportedPropertyState.Unchanged, AmendedImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);

			ImportedOrderLine newImportedOrderLine = new ImportedOrderLine(AmendedImportedOrder, Factory.NewWithValidTestData<OrderLine>());
			AssertEquals(ImportedPropertyState.New, newImportedOrderLine.JO_LineNoAndSplitAndSubLine.State);
		}

		#endregion

		#region Amended Properties

		public void TestAmendedPropertiesToShowAlways()
		{
			AssertEquals("JO_LineNoAndSplitAndSubLine 1st always", AmendedOrderLine.JO_LineNoAndSplitAndSubLineInfo.Name, AmendedImportedOrderLine.AmendedProperty1.Name);
			AssertEquals("JO_Partno 2nd always", AmendedOrderLine.JO_PartnoInfo.Name, AmendedImportedOrderLine.AmendedProperty2.Name);
			AssertEquals("JO_Description 3rd always", AmendedOrderLine.JO_DescriptionInfo.Name, AmendedImportedOrderLine.AmendedProperty3.Name);
		}

		public void TestAmendedProperties()
		{
			int i = 4;
			TestAmendedProperty(AmendedOrderLine.JO_InnerPacksInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_OuterPacksInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_QuantityInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_QtyInvoicedInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_QtyReceivedInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_F3_NKPackTypeInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_ItemPriceInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_LinePriceInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_CommercialInvoiceNoInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_RN_NKCountryOfOriginInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_LineStatusInfo, i++);
			TestAmendedProperty(AmendedOrderLine.JO_LineDropDateInfo, i++);
		}

		public void TestAmendedProperties_WithMultipleOrderLines()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			OrderLine orderLine1 = order.OrderLines.AddNew();
			OrderLine orderLine2 = order.OrderLines.AddNew();
			Factory.Save();

			orderLine1.JO_InnerPacks = 1;
			orderLine1.JO_OuterPacks = 2;
			orderLine2.JO_Quantity = 3;

			ImportedOrder importedOrder = new ImportedOrder(order);
			ImportedOrderLine importedOrderLine1 = new ImportedOrderLine(importedOrder, orderLine1);
			ImportedOrderLine importedOrderLine2 = new ImportedOrderLine(importedOrder, orderLine2);

			AssertEquals("Always show JO_LineNo", orderLine1.JO_LineNoAndSplitAndSubLineInfo.Name, importedOrderLine1.AmendedProperty1.Name);
			AssertEquals("Always show JO_Partno", JobOrderLineSchema.JO_Partno.Name, importedOrderLine1.AmendedProperty2.Name);
			AssertEquals("Always show JO_Description", JobOrderLineSchema.JO_Description.Name, importedOrderLine1.AmendedProperty3.Name);
			AssertEquals("OrderLine1 has modified JO_InnerPacks", JobOrderLineSchema.JO_InnerPacks.Name, importedOrderLine1.AmendedProperty4.Name);
			AssertEquals("OrderLine1 has modified JO_OuterPacks", JobOrderLineSchema.JO_OuterPacks.Name, importedOrderLine1.AmendedProperty5.Name);
			AssertEquals("OrderLine2 has modified JO_Quantity, and should show column for OrderLine1", JobOrderLineSchema.JO_Quantity.Name, importedOrderLine1.AmendedProperty6.Name);
			AssertEquals("No more amended properties for any of the order lines", null, importedOrderLine1.AmendedProperty7);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportedOrderLine(AmendedImportedOrder, AmendedOrderLine);
		}

		ImportedOrder AmendedImportedOrder
		{
			get { return new ImportedOrder(AmendedOrder); }
		}

		ImportedOrderLine AmendedImportedOrderLine
		{
			get { return new ImportedOrderLine(AmendedImportedOrder, AmendedOrderLine); }
		}

		Order AmendedOrder
		{
			get { return OrdersWithChanges.AmendedOrder; }
		}

		OrderLine AmendedOrderLine
		{
			get { return OrdersWithChanges.AmendedOrderLine; }
		}

		OrdersWithChangesForTest OrdersWithChanges
		{
			get { return ordersWithChanges ?? (ordersWithChanges = new OrdersWithChangesForTest(Factory)); }
		}
		OrdersWithChangesForTest ordersWithChanges;

		#endregion
	}
}
