using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvoiceReconciliator))]
	public sealed class ComInvoiceReconciliatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReconciliationOfMassAndVolume()
		{
			Declaration = Factory.New<BaseJobDeclaration>();
			SelectedOrders = new Order[1];
			SelectedOrders[0] = Helper.CreateOrder(Factory, "ORDER1", 1, new ZDateTime(2005, 6, 29), "INVOICE1", new ZDateTime(2005, 6, 30));
			SelectedOrders[0].OrderLines[0].JO_Quantity = 1000;
			SelectedOrders[0].OrderLines[0].JO_QtyInvoiced = 100;
			SelectedOrders[0].OrderLines[0].JO_ActualWeight = 6900m;
			SelectedOrders[0].OrderLines[0].JO_ActualVolume = 7000m;
			SelectedOrders[0].OrderLines[0].JO_UnitOfWeight = "KG";
			SelectedOrders[0].OrderLines[0].JO_UnitOfVolume = "M3";
			Factory.Save();

			var reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ImportInvoices();

			var invoiceLine = Declaration.InvoiceLines[0];
			AssertEquals(6900m, invoiceLine.JI_Weight);
			AssertEquals(7000m, invoiceLine.JI_Volume);
			AssertEquals("KG", invoiceLine.JI_WeightUQ);
			AssertEquals("M3", invoiceLine.JI_VolumeUQ);
		}

		public void TestReconciliationOfMassAndVolumePartial()
		{
			Declaration = Factory.New<BaseJobDeclaration>();
			SelectedOrders = new Order[1];
			SelectedOrders[0] = Helper.CreateOrder(Factory, "ORDER1", 1, new ZDateTime(2005, 6, 29), "INVOICE1", new ZDateTime(2005, 6, 30));
			SelectedOrders[0].OrderLines[0].JO_Quantity = 1000;
			SelectedOrders[0].OrderLines[0].JO_QtyInvoiced = 100;
			SelectedOrders[0].OrderLines[0].JO_ActualWeight = 6900m;
			SelectedOrders[0].OrderLines[0].JO_ActualVolume = 7000m;
			SelectedOrders[0].OrderLines[0].JO_UnitOfWeight = "KG";
			SelectedOrders[0].OrderLines[0].JO_UnitOfVolume = "M3";
			Factory.Save();

			var reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ComInvHeaders[0].InvoiceLines[0].JO_Recon_Quantity = 10;  // edit the value on screen
			reconciliator.ImportInvoices();
			var invoiceLine = Declaration.InvoiceLines[0];
			AssertEquals(69m, invoiceLine.JI_Weight);
			AssertEquals(70m, invoiceLine.JI_Volume);
		}

		#region TestComInvoiceReconciliatorIsUsingDifferentFactory

		public void TestInvoiceHeaderAndLineAmounts()
		{
			Declaration = Factory.New<BaseJobDeclaration>();
			Declaration.JE_OH_Importer = Helper.Buyer.PK;
			SelectedOrders = new Order[1];
			SelectedOrders[0] = Helper.CreateOrder(Factory, "ORDER1", 1, new ZDateTime(2005, 6, 29), "INVOICE1", new ZDateTime(2005, 6, 30));
			SelectedOrders[0].OrderLines[0].JO_Quantity = 1;
			SelectedOrders[0].OrderLines[0].JO_LinePrice = 10;
			var orderLine2 = new TestHelper().CreateOrderLine(Factory);
			orderLine2.JO_Quantity = 1;
			orderLine2.JO_LinePrice = 20m;
			SelectedOrders[0].OrderLines.Add(orderLine2);
			orderLine2.JO_LineNo = 2;
			Factory.Save();

			var comInvoiceReconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			comInvoiceReconciliator.ImportInvoices();

			var invoiceHeader = Declaration.Invoices[0];
			AssertEquals(10m, invoiceHeader.InvoiceLines[0].JI_LinePrice);
			AssertEquals(20m, invoiceHeader.InvoiceLines[1].JI_LinePrice);
			AssertEquals("Invoice header total should be the sum of its lines' amounts (not double that)", 30m, invoiceHeader.JZ_InvoiceAmount);
		}

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestComInvoiceReconciliatorIsUsingDifferentFactory()
		{
			CreateDummyDeclarationAndOrders(Factory);
			AssertPreConditionDummyDeclarationAndOrders(Factory);
			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			AssertEquals("ComInvHeaders Count", 4, reconciliator.ComInvHeaders.Count);

			ComInvOrderReconciliation comInvOrder = Factory.New<ComInvOrderReconciliation>();
			comInvOrder.SupplierPK = SelectedOrders[0].SupplierPK;
			comInvOrder.JD_InvoiceNumber = SelectedOrders[0].JD_InvoiceNumber;
			comInvOrder.JD_InvoiceDate = SelectedOrders[0].JD_InvoiceDate;
			comInvOrder.JD_RX_NKOrderCurrency = SelectedOrders[0].JD_RX_NKOrderCurrency;
			comInvOrder.JD_IncoTerm = SelectedOrders[0].JD_IncoTerm;

			AssertReconciliatorHasOrderHavingDifferentFactory(reconciliator, comInvOrder, SelectedOrders[0]);

			comInvOrder.JD_InvoiceNumber = SelectedOrders[1].JD_InvoiceNumber;
			AssertReconciliatorHasOrderHavingDifferentFactory(reconciliator, comInvOrder, SelectedOrders[1]);

			comInvOrder.JD_InvoiceNumber = SelectedOrders[2].JD_InvoiceNumber;
			AssertReconciliatorHasOrderHavingDifferentFactory(reconciliator, comInvOrder, SelectedOrders[2]);

			comInvOrder.JD_InvoiceNumber = "ORDER4-1";
			comInvOrder.JD_InvoiceDate = ZDateTime.Now;
			AssertReconciliatorHasOrderHavingDifferentFactory(reconciliator, comInvOrder, SelectedOrders[3]);

			comInvOrder.JD_InvoiceNumber = SelectedOrders[4].JD_InvoiceNumber;
			AssertReconciliatorHasOrderHavingDifferentFactory(reconciliator, comInvOrder, SelectedOrders[4]);
		}

		void AssertReconciliatorHasOrderHavingDifferentFactory(ComInvoiceReconciliator reconciliator, ComInvOrderReconciliation comInvOrder, Order chkOrder)
		{
			ComInvHeaderReconciliation header = reconciliator.ComInvHeaders[comInvOrder];
			AssertNotNull(header);
			AssertOrdersHaveDifferentFactory((Order)((IBusinessObjectCollection)header.ComInvOrders).FindByPK(chkOrder.PK), chkOrder);
		}

		void AssertOrdersHaveDifferentFactory(Order orderA, Order orderB)
		{
			AssertNotNull(orderA);
			AssertNotNull(orderB);
			AssertEquals("Should have different Factory", false, orderA.Factory == orderB.Factory);
		}

		#endregion

		public void TestImportInvoicesSavesChangesDoneOnOrders()
		{
			ZDateTime currentDate = new ZDateTime(2005, 11, 9);
			CreateDummyDeclarationAndOrders(Factory);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Order selectedOrder1 = SelectedOrders[0];
			selectedOrder1.JD_InvoiceNumber = "INVORD1";
			selectedOrder1.JD_InvoiceDate = currentDate;

			Order order1 = newFactory.Load<Order>(selectedOrder1.PK);
			Assert("Order hasn't been updated yet", order1.JD_InvoiceNumber != "INVORD1");

			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ImportInvoices();

			order1 = newFactory.Load<Order>(selectedOrder1.PK);
			AssertEquals("Order should have been saved", "INVORD1", order1.JD_InvoiceNumber);
			AssertEquals("Order should have been saved", currentDate, order1.JD_InvoiceDate);
		}

		public void TestInvoiceNumberAndDateGetUpdatedFromSelectedOrderInConstructor()
		{
			ZDateTime currentDate = new ZDateTime(2005, 11, 9);
			CreateDummyDeclarationAndOrders(Factory);
			Order selectedOrder1 = SelectedOrders[0];
			Assert("Precondition", selectedOrder1.JD_InvoiceNumber != "INVORD1");
			selectedOrder1.JD_InvoiceNumber = "INVORD1";
			selectedOrder1.JD_InvoiceDate = currentDate;

			Order selectedOrder2 = SelectedOrders[1];
			Assert("Precondition", selectedOrder1.JD_InvoiceNumber != "INVORD2");
			selectedOrder2.JD_InvoiceNumber = "INVORD2";
			selectedOrder2.JD_InvoiceDate = currentDate.AddDays(1);

			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			ComInvHeaderReconciliationCollection comInvHeaders = reconciliator.ComInvHeaders;
			Assert("Precondition: ComInvHeaders", comInvHeaders.Count > 1);
			Assert("Precondition: ComInvOrders", comInvHeaders[0].ComInvOrders.Count > 0);
			Order order1 = comInvHeaders[0].ComInvOrders[0];
			AssertEquals("Order invoice number should have been set to the new value", "INVORD1", order1.JD_InvoiceNumber);
			AssertEquals("Order invoice date should have been set to the new value", currentDate, order1.JD_InvoiceDate);

			Order order2 = comInvHeaders[1].ComInvOrders[0];
			AssertEquals("Order invoice number should have been set to the new value", "INVORD2", order2.JD_InvoiceNumber);
			AssertEquals("Order invoice date should have been set to the new value", currentDate.AddDays(1), order2.JD_InvoiceDate);
		}

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestComInvHeaders()
		{
			CreateDummyDeclarationAndOrders(Factory);
			AssertPreConditionDummyDeclarationAndOrders(Factory);
			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			AssertNotNull(reconciliator.ComInvHeaders);
			AssertEquals("ComInvHeaders Count", 4, reconciliator.ComInvHeaders.Count);

			ComInvOrderReconciliation comInvOrder = Factory.New<ComInvOrderReconciliation>();
			comInvOrder.SupplierPK = SelectedOrders[0].SupplierPK;
			comInvOrder.JD_InvoiceNumber = SelectedOrders[0].JD_InvoiceNumber;
			comInvOrder.JD_InvoiceDate = SelectedOrders[0].JD_InvoiceDate;
			comInvOrder.JD_RX_NKOrderCurrency = SelectedOrders[0].JD_RX_NKOrderCurrency;
			comInvOrder.JD_IncoTerm = SelectedOrders[0].JD_IncoTerm;

			ComInvHeaderReconciliation header1 = reconciliator.ComInvHeaders[comInvOrder];
			AssertNotNull(header1);
			AssertNotNull(header1.ComInvOrders);
			AssertEquals(2, header1.ComInvOrders.Count);
			AssertNotNull(((IBusinessObjectCollection)header1.ComInvOrders).FindByPK(SelectedOrders[0].PK));
			AssertNotNull(((IBusinessObjectCollection)header1.ComInvOrders).FindByPK(SelectedOrders[1].PK));

			comInvOrder.JD_InvoiceNumber = SelectedOrders[2].JD_InvoiceNumber;
			ComInvHeaderReconciliation header2 = reconciliator.ComInvHeaders[comInvOrder];
			AssertNotNull(header2);
			AssertNotNull(header2.ComInvOrders);
			AssertEquals(1, header2.ComInvOrders.Count);
			AssertEquals(SelectedOrders[2].PK, header2.ComInvOrders[0].PK);

			comInvOrder.JD_InvoiceNumber = "ORDER4-1";
			comInvOrder.JD_InvoiceDate = ZDateTime.Now;
			ComInvHeaderReconciliation header3 = reconciliator.ComInvHeaders[comInvOrder];
			AssertNotNull(header3);
			AssertNotNull(header3.ComInvOrders);
			AssertEquals(1, header3.ComInvOrders.Count);
			AssertEquals(SelectedOrders[3].PK, header3.ComInvOrders[0].PK);

			comInvOrder.JD_InvoiceNumber = SelectedOrders[4].JD_InvoiceNumber;
			ComInvHeaderReconciliation header4 = reconciliator.ComInvHeaders[comInvOrder];
			AssertNotNull(header4);
			AssertNotNull(header4.ComInvOrders);
			AssertEquals(1, header4.ComInvOrders.Count);
			AssertEquals(SelectedOrders[4].PK, header4.ComInvOrders[0].PK);
		}

		#region Test ImportInvoices

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestImportInvoices_OrderQuantity()
		{
			AssertImportInvoicesFromOrderHavingQuantityType(ComInvReconciliationQuantityType.OrderQuantity, Factory);
		}

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestImportInvoices_InvoiceQuantity()
		{
			AssertImportInvoicesFromOrderHavingQuantityType(ComInvReconciliationQuantityType.InvoiceQuantity, Factory);
		}

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestImportInvoices_ReceivedQuantity()
		{
			AssertImportInvoicesFromOrderHavingQuantityType(ComInvReconciliationQuantityType.ReceivedQuantity, Factory);
		}

		void AssertImportInvoicesFromOrderHavingQuantityType(ComInvReconciliationQuantityType quantityType, BusinessObjectFactory factory)
		{
			CreateDummyDeclarationAndOrders(factory);
			AssertPreConditionDummyDeclarationAndOrders(factory);
			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, quantityType);
			reconciliator.ImportInvoices();
			AssertDeclarationHavingInvHeadersCreatedFromOrders(quantityType);
		}

		static public void AssertPreConditionDummyDeclarationAndOrders(BusinessObjectFactory factory)
		{
			AssertNotNull("PreCondition: Declaration", Declaration);
			AssertEquals("PreCondition: Declaration has 2 Invoices", 2, Declaration.Invoices.Count);

			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)((IBusinessObjectCollection)Declaration.Invoices).FindByPK(invoiceHeader1.PK);
			AssertNotNull("PreCondition: Declaration has Invoice 1", invoiceHeader);
			AssertEquals("PreCondition: Declaration's Invoice 1 has 2 lines", 2, invoiceHeader.JobComInvoiceLines.Count);

			BaseJobComInvoiceLine invoiceLine = (BaseJobComInvoiceLine)((IBusinessObjectCollection)invoiceHeader.JobComInvoiceLines).FindByPK(invoiceHeader1Line1.PK);
			AssertNotNull("PreCondition: Declaration's Invoice 1 has line 1", invoiceLine);
			AssertEquals("PreCondition: Declaration's Invoice 1 line 1 is linked to an orderline", SelectedOrders[0].OrderLines[0].PK, invoiceLine.JI_JO);
			AssertInvoiceLineHasOrderLineValue(invoiceLine, SelectedOrders[0].OrderLines[0]);

			invoiceLine = (BaseJobComInvoiceLine)((IBusinessObjectCollection)invoiceHeader.JobComInvoiceLines).FindByPK(invoiceHeader1Line2.PK);
			AssertNotNull("PreCondition: Declaration's Invoice 1 has line 2", invoiceLine);
			AssertEquals("PreCondition: Declaration's Invoice 1 line 2 is not linked to an orderline", ZGuid.Empty, invoiceLine.JI_JO);
			AssertInvoiceLineHasOrderLineValue(invoiceLine, SelectedOrders[1].OrderLines[0]);

			invoiceHeader = (BaseJobComInvoiceHeader)((IBusinessObjectCollection)Declaration.Invoices).FindByPK(invoiceHeader2.PK);
			AssertNotNull("PreCondition: Declaration has Invoice 2", invoiceHeader);
			AssertEquals("PreCondition: Declaration's Invoice 2 has 1 line", 1, invoiceHeader.JobComInvoiceLines.Count);

			invoiceLine = invoiceHeader.JobComInvoiceLines[0];
			AssertEquals("PreCondition: Declaration's Invoice 2 line 1", invoiceHeader2Line1.PK, invoiceLine.PK);
			AssertEquals("PreCondition: Declaration's Invoice 2 line 1 is linked to an orderline", SelectedOrders[2].OrderLines[0].PK, invoiceLine.JI_JO);
			AssertInvoiceLineHasOrderLineValue(invoiceLine, SelectedOrders[2].OrderLines[0]);

			AssertEquals("PreCondition: Order4 is not assigned to an invoice", ZString.Empty, SelectedOrders[3].JD_InvoiceNumber);

			invoiceHeader = (BaseJobComInvoiceHeader)((IBusinessObjectCollection)Declaration.Invoices).FindByPK(invoiceHeaderUnlink.PK);
			AssertNull("PreCondition: Declaration does not has " + invoiceHeaderUnlink.HumanReadableName, invoiceHeader);
			AssertEquals("PreCondition: " + invoiceHeaderUnlink.HumanReadableName + " has 1 line", 1, invoiceHeaderUnlink.JobComInvoiceLines.Count);

			invoiceLine = invoiceHeaderUnlink.JobComInvoiceLines[0];
			AssertEquals("PreCondition: " + invoiceHeaderUnlink.HumanReadableName + " line 1", invoiceHeaderUnlinkLine1.PK, invoiceLine.PK);
			AssertEquals("PreCondition: " + invoiceHeaderUnlink.HumanReadableName + " line 1 is not linked to an orderline", ZGuid.Empty, invoiceLine.JI_JO);
			AssertInvoiceLineDoesNotHaveOrderLineValue(invoiceLine, SelectedOrders[4].OrderLines[0]);
		}

		void AssertDeclarationHavingInvHeadersCreatedFromOrders(ComInvReconciliationQuantityType quantityType)
		{
			AssertEquals(5, Declaration.Invoices.Count);
			AssertInvoiceHeader1(quantityType);
			AssertInvoiceHeader2();
			AssertNewInvHeaderFromOrderThatHadInvNo(quantityType, SelectedOrders[2]);
			AssertNewInvHeaderFromOrderThatHadNoInvNo(quantityType);
			AssertUnlinkInvHeaderWasNotUpdatedButNewInvHeaderWasCreated(quantityType);
		}

		void AssertInvoiceHeader1(ComInvReconciliationQuantityType quantityType)
		{
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)((IBusinessObjectCollection)Declaration.Invoices).FindByPK(invoiceHeader1.PK);
			AssertNotNull(invoiceHeader);
			AssertEquals("Invoice 1 should have 3 lines", 3, invoiceHeader.JobComInvoiceLines.Count);
			AssertInvoiceLineHavingSameDataAsOrderLine((BaseJobComInvoiceLine)((IBusinessObjectCollection)invoiceHeader.JobComInvoiceLines).FindByPK(invoiceHeader1Line1.PK), SelectedOrders[0].OrderLines[0], quantityType);
			AssertInvoiceLineIsNotLinkedToOrderLine((BaseJobComInvoiceLine)((IBusinessObjectCollection)invoiceHeader.JobComInvoiceLines).FindByPK(invoiceHeader1Line2.PK));

			ZQuery newInvoiceLineFilter = new ZQuery(JobComInvoiceLineSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeader1Line1.PK);
			newInvoiceLineFilter.AddToFilter(JobComInvoiceLineSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeader1Line2.PK);
			BaseJobComInvoiceLine[] newInvoiceLines = (BaseJobComInvoiceLine[])invoiceHeader.JobComInvoiceLines.Find(newInvoiceLineFilter);
			AssertEquals(1, newInvoiceLines.Length);
			AssertInvoiceLineHavingSameDataAsOrderLine(newInvoiceLines[0], SelectedOrders[1].OrderLines[0], quantityType);
		}

		void AssertInvoiceHeader2()
		{
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)((IBusinessObjectCollection)Declaration.Invoices).FindByPK(invoiceHeader2.PK);
			AssertNotNull(invoiceHeader);
			AssertEquals("Invoice 2 should have 1 line", 1, invoiceHeader.JobComInvoiceLines.Count);
			AssertInvoiceLineIsNotLinkedToOrderLine(invoiceHeader.JobComInvoiceLines[0]);
		}

		void AssertNewInvHeaderFromOrderThatHadInvNo(ComInvReconciliationQuantityType quantityType, Order chkOrder)
		{
			ZQuery newInvoiceFilter = new ZQuery(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeader1.PK);
			newInvoiceFilter.AddToFilter(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeader2.PK);
			newInvoiceFilter.AddToFilter(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeaderUnlink.PK);
			newInvoiceFilter.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, chkOrder.JD_InvoiceNumber);

			List<BaseJobComInvoiceHeader> invoiceHeaders = new List<BaseJobComInvoiceHeader>(Declaration.Invoices.Find(newInvoiceFilter));
			AssertEquals("New Invoice", 1, invoiceHeaders.Count);
			BaseJobComInvoiceHeader invoiceHeader = invoiceHeaders[0];
			AssertEquals(chkOrder.SupplierPK, invoiceHeader.JZ_OH_Supplier);
			AssertEquals(chkOrder.JD_IncoTerm, invoiceHeader.JZ_IncoTerm);
			AssertEquals(chkOrder.JD_RX_NKOrderCurrency, invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals(chkOrder.JD_InvoiceDate, invoiceHeader.JZ_InvoiceDate);

			AssertEquals("New Invoice should have 1 line", 1, invoiceHeader.JobComInvoiceLines.Count);
			AssertInvoiceLineHavingSameDataAsOrderLine(invoiceHeader.JobComInvoiceLines[0], chkOrder.OrderLines[0], quantityType);
		}

		void AssertNewInvHeaderFromOrderThatHadNoInvNo(ComInvReconciliationQuantityType quantityType)
		{
			ZQuery newInvoiceFilter = new ZQuery(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeader1.PK);
			newInvoiceFilter.AddToFilter(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeader2.PK);
			newInvoiceFilter.AddToFilter(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoiceHeaderUnlink.PK);
			newInvoiceFilter.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, "ORDER4-1");

			List<BaseJobComInvoiceHeader> invoiceHeaders = new List<BaseJobComInvoiceHeader>(Declaration.Invoices.Find(newInvoiceFilter));
			AssertEquals("New Invoice", 1, invoiceHeaders.Count);
			BaseJobComInvoiceHeader invoiceHeader = invoiceHeaders[0];
			AssertEquals(SelectedOrders[3].SupplierPK, invoiceHeader.JZ_OH_Supplier);
			AssertEquals(SelectedOrders[3].JD_IncoTerm, invoiceHeader.JZ_IncoTerm);
			AssertEquals(SelectedOrders[3].JD_RX_NKOrderCurrency, invoiceHeader.JZ_RX_NKInvoice_Currency);
			AssertEquals(ZDateTime.Now, invoiceHeader.JZ_InvoiceDate);

			AssertEquals("New Invoice should have 1 line", 1, invoiceHeader.JobComInvoiceLines.Count);
			AssertInvoiceLineHavingSameDataAsOrderLine(invoiceHeader.JobComInvoiceLines[0], SelectedOrders[3].OrderLines[0], quantityType);
		}

		void AssertUnlinkInvHeaderWasNotUpdatedButNewInvHeaderWasCreated(ComInvReconciliationQuantityType quantityType)
		{
			BaseJobComInvoiceHeader invoiceHeader = (BaseJobComInvoiceHeader)((IBusinessObjectCollection)Declaration.Invoices).FindByPK(invoiceHeaderUnlink.PK);
			AssertNull("Declaration does not has " + invoiceHeaderUnlink.HumanReadableName, invoiceHeader);
			AssertEquals(invoiceHeaderUnlink.HumanReadableName + " has 1 line", 1, invoiceHeaderUnlink.JobComInvoiceLines.Count);
			BaseJobComInvoiceLine invoiceLine = invoiceHeaderUnlink.JobComInvoiceLines[0];
			AssertEquals(invoiceHeaderUnlink.HumanReadableName + " line 1", invoiceHeaderUnlinkLine1.PK, invoiceLine.PK);
			AssertEquals(invoiceHeaderUnlink.HumanReadableName + " line 1 is not linked to an orderline", ZGuid.Empty, invoiceLine.JI_JO);
			AssertInvoiceLineDoesNotHaveOrderLineValue(invoiceLine, SelectedOrders[4].OrderLines[0]);

			AssertNewInvHeaderFromOrderThatHadInvNo(quantityType, SelectedOrders[4]);
		}

		void AssertInvoiceLineHavingSameDataAsOrderLine(BaseJobComInvoiceLine invoiceLine, OrderLine line, ComInvReconciliationQuantityType quantityType)
		{
			AssertNotNull("InvoiceLine", invoiceLine);
			AssertNotNull("OrderLine", line);
			AssertInvoiceLineHavingSameReconcileDataAsOrderLine(invoiceLine, line, quantityType);

			AssertEquals("Invoice line UQ should equal the UQ from the OrderLine", line.JO_F3_NKPackType, invoiceLine.JI_InvoiceUQ);
			AssertEquals("Invoice line Description should equal the Description from the OrderLine", line.JO_Description, invoiceLine.JI_Description);
			AssertEquals("Invoice line PartNo should equal the Description from the OrderLine", line.JO_Partno, invoiceLine.JI_PartNo);
			AssertEquals("Invoice Line Custom Attrib 1 should equal the Custom Attrib from the OrderLine", line.JO_CustomAttrib1, invoiceLine.JI_CustomAttrib1);
			AssertEquals("Invoice Line Custom Attrib 2 should equal the Custom Attrib from the OrderLine", line.JO_CustomAttrib2, invoiceLine.JI_CustomAttrib2);
			AssertEquals("Invoice Line Custom Attrib 3 should equal the Custom Attrib from the OrderLine", line.JO_CustomAttrib3, invoiceLine.JI_CustomAttrib3);
			AssertEquals("Invoice Line Custom Attrib 4 should equal the Custom Attrib from the OrderLine", line.JO_CustomAttrib4, invoiceLine.JI_CustomAttrib4);
			AssertEquals("Invoice Line Custom Attrib 5 should equal the Custom Attrib from the OrderLine", line.JO_CustomAttrib5, invoiceLine.JI_CustomAttrib5);
			AssertEquals("Invoice Line Custom Attrib 6 should equal the Custom Attrib from the OrderLine", line.JO_CustomAttrib6, invoiceLine.JI_CustomAttrib6);
			AssertEquals("Invoice Line Custom TextBlob1 should equal the Custom TextBlob1 from the OrderLine", line.JO_CustomTextBlob1, invoiceLine.JI_CustomTextBlob1);
			AssertEquals("Invoice Line Part Attrib 1 should equal the Part Attrib from the OrderLine", line.JO_PartAttrib1, invoiceLine.JI_PartAttrib1);
			AssertEquals("Invoice Line Part Attrib 2 should equal the Part Attrib from the OrderLine", line.JO_PartAttrib2, invoiceLine.JI_PartAttrib2);
			AssertEquals("Invoice Line Part Attrib 3 should equal the Part Attrib from the OrderLine", line.JO_PartAttrib3, invoiceLine.JI_PartAttrib3);
			AssertEquals("Invoice Line Serial Number should equal the Part Attrib from the OrderLine", line.JO_SerialNumber, invoiceLine.JI_SerialNumber);
			AssertEquals("Invoice Line Custom Date 1 should equal the Custom Date from the OrderLine", line.JO_CustomDate1, invoiceLine.JI_CustomDate1);
			AssertEquals("Invoice Line Custom Date 2 should equal the Custom Date from the OrderLine", line.JO_CustomDate2, invoiceLine.JI_CustomDate2);
			AssertEquals("Invoice Line Custom Date 3 should equal the Custom Date from the OrderLine", line.JO_CustomDate3, invoiceLine.JI_CustomDate3);
			AssertEquals("Invoice Line Custom Date 4 should equal the Custom Date from the OrderLine", line.JO_CustomDate4, invoiceLine.JI_CustomDate4);
			AssertEquals("Invoice Line Custom Date 5 should equal the Custom Date from the OrderLine", line.JO_CustomDate5, invoiceLine.JI_CustomDate5);
			AssertEquals("Invoice Line Custom Decimal 1 should equal the Custom Decimal from the OrderLine", line.JO_CustomDecimal1, invoiceLine.JI_CustomDecimal1);
			AssertEquals("Invoice Line Custom Decimal 2 should equal the Custom Decimal from the OrderLine", line.JO_CustomDecimal2, invoiceLine.JI_CustomDecimal2);
			AssertEquals("Invoice Line Custom Decimal 3 should equal the Custom Decimal from the OrderLine", line.JO_CustomDecimal3, invoiceLine.JI_CustomDecimal3);
			AssertEquals("Invoice Line Custom Decimal 4 should equal the Custom Decimal from the OrderLine", line.JO_CustomDecimal4, invoiceLine.JI_CustomDecimal4);
			AssertEquals("Invoice Line Custom Decimal 5 should equal the Custom Decimal from the OrderLine", line.JO_CustomDecimal5, invoiceLine.JI_CustomDecimal5);
			AssertEquals("Invoice Line Custom Flag 1 should equal the Custom Flag from the OrderLine", line.JO_CustomFlag1, invoiceLine.JI_CustomFlag1);
			AssertEquals("Invoice Line Custom Flag 2 should equal the Custom Flag from the OrderLine", line.JO_CustomFlag2, invoiceLine.JI_CustomFlag2);
			AssertEquals("Invoice Line Custom Flag 3 should equal the Custom Flag from the OrderLine", line.JO_CustomFlag3, invoiceLine.JI_CustomFlag3);
			AssertEquals("Invoice Line Custom Flag 4 should equal the Custom Flag from the OrderLine", line.JO_CustomFlag4, invoiceLine.JI_CustomFlag4);
			AssertEquals("Invoice Line Custom Flag 5 should equal the Custom Flag from the OrderLine", line.JO_CustomFlag5, invoiceLine.JI_CustomFlag5);
			AssertEquals("Inovice line should have link to Order Line", line.PK, invoiceLine.JI_JO);
		}

		void AssertInvoiceLineHavingSameReconcileDataAsOrderLine(BaseJobComInvoiceLine invoiceLine, OrderLine line, ComInvReconciliationQuantityType quantityType)
		{
			ZDecimal recon_Quantity = 0m;

			ZDecimal recon_ItemPrice = line.JO_ItemPrice;
			ZString quantityMessage = "Quantity";

			switch (quantityType)
			{
				case ComInvReconciliationQuantityType.InvoiceQuantity:
					recon_Quantity = line.JO_QtyInvoiced;
					quantityMessage = "Invoiced " + quantityMessage;
					break;
				case ComInvReconciliationQuantityType.ReceivedQuantity:
					recon_Quantity = line.JO_QtyReceived;
					quantityMessage = "Received " + quantityMessage;
					break;
				default:
					recon_Quantity = line.JO_Quantity;
					break;
			}

			ZDecimal recon_LinePrice = recon_Quantity * recon_ItemPrice;

			AssertEquals("Invoice Line Quantity should equal the Line " + quantityMessage + " from the OrderLine", recon_Quantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("Invoice Line Unit Price should equal the Line Item Price from the OrderLine", recon_ItemPrice, invoiceLine.UnitPrice);
			AssertEquals("Invoice Line Line Price should equal the " + quantityMessage + " times Item Price from the OrderLine", recon_LinePrice, invoiceLine.JI_LinePrice);
		}

		void AssertInvoiceLineIsNotLinkedToOrderLine(BaseJobComInvoiceLine invoiceLine)
		{
			AssertNotNull(invoiceLine);
			AssertEquals(ZGuid.Empty, invoiceLine.JI_JO);
		}

		static public void AssertInvoiceLineHasOrderLineValue(BaseJobComInvoiceLine invoiceLine, OrderLine line)
		{
			AssertEquals(line.JO_Quantity, invoiceLine.JI_InvoiceQuantity);
			AssertEquals(line.JO_ItemPrice, invoiceLine.UnitPrice);
		}

		static public void AssertInvoiceLineDoesNotHaveOrderLineValue(BaseJobComInvoiceLine invoiceLine, OrderLine line)
		{
			AssertEquals("InvoiceLine and OrderLine Quantity should not be the same", false, line.JO_Quantity == invoiceLine.JI_InvoiceQuantity);
			AssertEquals("InvoiceLine and OrderLine ItemPrice should not be the same", false, line.JO_ItemPrice == invoiceLine.UnitPrice);
		}

		[ExpectNoExceptions()]
		public void TestImportInvoices_SaveOK()
		{
			CreateDummyDeclarationAndOrders(Factory);
			AssertPreConditionDummyDeclarationAndOrders(Factory);
			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.InvoiceQuantity);
			reconciliator.ImportInvoices();
			Declaration.Factory.Save();
		}

		#endregion

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestImportInvoices_CopyInvoiceLinesPersistentValuesToDeclarationFactory()
		{
			CreateDummyDeclarationAndOrders(Factory);
			AssertPreConditionDummyDeclarationAndOrders(Factory);
			OrderLine originalLine = SelectedOrders[0].OrderLines[0];
			AssertEquals("PreCondition: OriginalLine Quantity", 10m, originalLine.JO_Quantity);
			AssertEquals("PreCondition: OriginalLine Line Price", 150m, originalLine.JO_LinePrice);

			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(Declaration, SelectedOrders, ComInvReconciliationQuantityType.OrderQuantity);
			ComInvOrderReconciliation comInvOrder = Factory.New<ComInvOrderReconciliation>();
			comInvOrder.SupplierPK = Helper.Supplier.PK;
			comInvOrder.JD_InvoiceNumber = "INVOICE1";
			comInvOrder.JD_InvoiceDate = new ZDateTime(2005, 6, 30);
			comInvOrder.JD_RX_NKOrderCurrency = Helper.USDCurrency.RX_Code;
			comInvOrder.JD_IncoTerm = Constants.IncoTerms.FreeOnBoard;

			ComInvHeaderReconciliation header = reconciliator.ComInvHeaders[comInvOrder];
			AssertNotNull("PreCondition: Header should not be null", header);
			OrderLine reconciliatorLine = (OrderLine)((IBusinessObjectCollection)header.InvoiceLines).FindByPK(originalLine.PK);
			AssertNotNull("PreCondition: Reconcilliator should have OriginalLine", reconciliatorLine);
			AssertEquals("PreCondition: PK", originalLine.PK, reconciliatorLine.PK);
			AssertEquals("PreCondition: Quantity", originalLine.JO_Quantity, reconciliatorLine.JO_Quantity);
			AssertEquals("PreCondition: Line Price", originalLine.JO_LinePrice, reconciliatorLine.JO_LinePrice);

			reconciliatorLine.JO_Quantity = 5m;
			AssertEquals("Quantity not equal", false, originalLine.JO_Quantity == reconciliatorLine.JO_Quantity);
			AssertEquals("Line Price not equal", false, originalLine.JO_LinePrice == reconciliatorLine.JO_LinePrice);

			reconciliator.ImportInvoices();
			AssertEquals("Quantity should have been updated", reconciliatorLine.JO_Quantity, originalLine.JO_Quantity);
			AssertEquals("Quantity should have been updated", 5m, originalLine.JO_Quantity);
			AssertEquals("Line Price should have been updated", reconciliatorLine.JO_LinePrice, originalLine.JO_LinePrice);
			AssertEquals(true, originalLine.HasChanges);
		}

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestImportInvoiceLinesReusesExistingEmptyInvoiceHeader()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Helper.Buyer.PK;
			declaration.JE_OH_Supplier = Helper.Supplier.PK;

			declaration.Invoices.AddNew();

			Order order = Factory.NewWithValidTestData<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.FillWithValidTestData();

			order.JD_InvoiceNumber = "INVORD1";
			order.JD_InvoiceDate = ZDateTime.Today;

			Factory.Save();

			AssertEquals("Precondition", 1, declaration.Invoices.Count);

			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(declaration, new Order[] { order }, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ImportInvoices();

			AssertEquals(1, declaration.Invoices.Count);
		}

		[TestDate(2005, 6, 30, 0, 0, 0)]
		public void TestImportInvoiceLinesDoesntUseExistingNonEmptyInvoiceHeaders()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Helper.Buyer.PK;
			declaration.JE_OH_Supplier = Helper.Supplier.PK;

			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "DONTUSEME";

			Order order = Factory.NewWithValidTestData<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.FillWithValidTestData();

			order.JD_InvoiceNumber = "INVORD1";
			order.JD_InvoiceDate = ZDateTime.Today;

			Factory.Save();

			ComInvoiceReconciliator reconciliator = new ComInvoiceReconciliator(declaration, new Order[] { order }, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ImportInvoices();

			AssertEquals(2, declaration.Invoices.Count);
		}

		public void TestImportInvoiceLinesDoesntTriggerWorkflowPropertyChangedActions()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Helper.Buyer.PK;
			declaration.JE_OH_Supplier = Helper.Supplier.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "DONTUSEME";

			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.FillWithValidTestData();
			orderLine.JO_Quantity = 4.0m;

			order.JD_InvoiceNumber = "INVORD1";
			order.JD_InvoiceDate = ZDateTime.Today;

			Factory.Save();

			var rule = Factory.New<IProcessFieldChangeRule>();
			rule.PFR_ProcessType = "ORL";
			rule.PFR_GroupName = "Group1";
			rule.PFR_SE_NKEvent = "Z35";

			var field = rule.Fields.AddNew();
			field.PFL_FieldName = "JO_Quantity";
			Factory.Save();

			orderLine.JO_Quantity = 6.0m;

			var reconciliator = new ComInvoiceReconciliator(declaration, new Order[] { order }, ComInvReconciliationQuantityType.OrderQuantity);
			reconciliator.ImportInvoices();

			AssertEquals(2, declaration.Invoices.Count);
			AssertEquals(false, orderLine.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, "Z35"));
		}

		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComInvoiceReconciliator(Factory.New<BaseJobDeclaration>(), new Order[] { Factory.New<Order>() }, ComInvReconciliationQuantityType.OrderQuantity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new TestHelper();
		}

		public static void CreateDummyDeclarationAndOrders(BusinessObjectFactory factory)
		{
			CreateDummySelectedOrders(factory);
			CreateDummyDeclaration(factory);
			Declaration.Factory.Save();
		}

		public static void CreateDummySelectedOrders(BusinessObjectFactory factory)
		{
			SelectedOrders = new Order[5];
			SelectedOrders[0] = Helper.CreateOrder(factory, "ORDER1", 1, new ZDateTime(2005, 6, 29), "INVOICE1", new ZDateTime(2005, 6, 30));
			SelectedOrders[1] = Helper.CreateOrder(factory, "ORDER2", 1, new ZDateTime(2005, 6, 28), "INVOICE1", new ZDateTime(2005, 6, 30));
			SelectedOrders[2] = Helper.CreateOrder(factory, "ORDER3", 1, new ZDateTime(2005, 6, 27), "INVOICE2", new ZDateTime(2005, 6, 30));
			SelectedOrders[3] = Helper.CreateOrder(factory, "ORDER4", 1, new ZDateTime(2005, 6, 27), ZString.Empty, ZDateTime.Empty);
			SelectedOrders[4] = Helper.CreateOrder(factory, "ORDER5", 1, new ZDateTime(2005, 6, 26), "UNLINKINV", new ZDateTime(2005, 6, 30));
		}

		static void CreateDummyDeclaration(BusinessObjectFactory factory)
		{
			Declaration = factory.New<BaseJobDeclaration>();
			Declaration.JE_OH_Importer = Helper.Buyer.PK;

			invoiceHeader1 = Declaration.Invoices.AddNew();
			PopulateInvoiceHeaderWithOrderData(invoiceHeader1, SelectedOrders[0]);

			invoiceHeader1Line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			PopulateInvoiceLineWithOrderLineData(invoiceHeader1Line1, SelectedOrders[0].OrderLines[0], (short)1);

			invoiceHeader1Line2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			PopulateInvoiceLineWithOrderLineData(invoiceHeader1Line2, SelectedOrders[1].OrderLines[0], (short)2);
			invoiceHeader1Line2.JI_JO = ZGuid.Empty;

			invoiceHeader2 = Declaration.Invoices.AddNew();
			PopulateInvoiceHeaderWithOrderData(invoiceHeader2, SelectedOrders[2]);
			invoiceHeader2.JZ_InvoiceDate = new ZDateTime(2005, 6, 29);

			invoiceHeader2Line1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			PopulateInvoiceLineWithOrderLineData(invoiceHeader2Line1, SelectedOrders[2].OrderLines[0], (short)1);

			invoiceHeaderUnlink = factory.New<BaseJobComInvoiceHeader>();
			invoiceHeaderUnlink.JZ_OH_Buyer = Helper.Buyer.PK;
			PopulateInvoiceHeaderWithOrderData(invoiceHeaderUnlink, SelectedOrders[4]);

			invoiceHeaderUnlinkLine1 = invoiceHeaderUnlink.JobComInvoiceLines.AddNew();
			invoiceHeaderUnlinkLine1.JI_LineNo = (short)1;
			invoiceHeaderUnlinkLine1.JI_InvoiceQuantity = SelectedOrders[4].OrderLines[0].JO_Quantity + 10m;
			invoiceHeaderUnlinkLine1.UnitPrice = SelectedOrders[4].OrderLines[0].JO_ItemPrice + 2m;
		}

		static void PopulateInvoiceHeaderWithOrderData(BaseJobComInvoiceHeader invoiceHeader, Order orderSource)
		{
			invoiceHeader.JZ_OH_Supplier = orderSource.SupplierPK;
			invoiceHeader.JZ_InvoiceNumber = orderSource.JD_InvoiceNumber;
			invoiceHeader.JZ_IncoTerm = orderSource.JD_IncoTerm;
			invoiceHeader.JZ_RX_NKInvoice_Currency = orderSource.JD_RX_NKOrderCurrency;
			invoiceHeader.JZ_InvoiceDate = orderSource.JD_InvoiceDate;
		}

		static void PopulateInvoiceLineWithOrderLineData(BaseJobComInvoiceLine invoiceLine, OrderLine line, ZShort lineNo)
		{
			invoiceLine.JI_JO = line.PK;
			invoiceLine.JI_LineNo = lineNo;
			invoiceLine.JI_InvoiceQuantity = line.JO_Quantity;
			invoiceLine.UnitPrice = line.JO_ItemPrice;
		}

		public static TestHelper Helper;
		public static Order[] SelectedOrders;
		public static BaseJobDeclaration Declaration;

		static BaseJobComInvoiceHeader invoiceHeader1;
		static BaseJobComInvoiceLine invoiceHeader1Line1;
		static BaseJobComInvoiceLine invoiceHeader1Line2;

		static BaseJobComInvoiceHeader invoiceHeader2;
		static BaseJobComInvoiceLine invoiceHeader2Line1;

		static BaseJobComInvoiceHeader invoiceHeaderUnlink;
		static BaseJobComInvoiceLine invoiceHeaderUnlinkLine1;

		#endregion
	}
}
