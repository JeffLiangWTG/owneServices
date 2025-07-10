using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrderInvoiceImporter))]
	sealed class OrderInvoiceImporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultInvoiceNumberAndDate()
		{
			var currentDate = ZDateTime.BrettsBirthday;

			var newOrder = Factory.New<Order>();
			newOrder.BuyerPK = Org.PK;
			newOrder.JD_OrderNumber = "1";
			newOrder.JD_JE = declaration.PK;
			Factory.Save();

			var importer = new OrderInvoiceImporter(declaration);

			Assert("Precondition", importer.OrdersToImport.FindByPK(newOrder.PK) != null);
			var order = importer.OrdersToImport[0];
			AssertEquals("Initial order number should be empty.", ZString.Empty, order.JD_InvoiceNumber);
			AssertEquals("Initial order date should be empty", ZDateTime.Empty, order.JD_InvoiceDate);

			importer.InvoiceNumber = "INVOICE";
			importer.InvoiceDate = currentDate;

			importer.DefaultInvoiceNumberAndDate(order);
			AssertEquals("Order Number should have been set to 'INVOICE'", "INVOICE", order.JD_InvoiceNumber);
			AssertEquals("Order date should have been set", currentDate, order.JD_InvoiceDate);
		}

		public void TestQuantityType()
		{
			OrderInvoiceImporter importer = new OrderInvoiceImporter(declaration);
			AssertEquals(ComInvReconciliationQuantityType.OrderQuantity, importer.QuantityType);

			importer.IsInvoicedQuantityImport = false;
			AssertEquals(ComInvReconciliationQuantityType.OrderQuantity, importer.QuantityType);
			AssertEquals(true, importer.IsOrderedQuantityImport);
			AssertEquals(false, importer.IsReceivedQuantityImport);

			importer.IsInvoicedQuantityImport = true;
			AssertEquals(ComInvReconciliationQuantityType.InvoiceQuantity, importer.QuantityType);
			AssertEquals(false, importer.IsOrderedQuantityImport);
			AssertEquals(false, importer.IsReceivedQuantityImport);

			importer.IsReceivedQuantityImport = false;
			AssertEquals(ComInvReconciliationQuantityType.InvoiceQuantity, importer.QuantityType);
			AssertEquals(false, importer.IsOrderedQuantityImport);
			AssertEquals(true, importer.IsInvoicedQuantityImport);

			importer.IsReceivedQuantityImport = true;
			AssertEquals(ComInvReconciliationQuantityType.ReceivedQuantity, importer.QuantityType);
			AssertEquals(false, importer.IsOrderedQuantityImport);
			AssertEquals(false, importer.IsInvoicedQuantityImport);

			importer.IsOrderedQuantityImport = false;
			AssertEquals(ComInvReconciliationQuantityType.ReceivedQuantity, importer.QuantityType);
			AssertEquals(true, importer.IsReceivedQuantityImport);
			AssertEquals(false, importer.IsInvoicedQuantityImport);

			importer.IsOrderedQuantityImport = true;
			AssertEquals(ComInvReconciliationQuantityType.OrderQuantity, importer.QuantityType);
			AssertEquals(false, importer.IsReceivedQuantityImport);
			AssertEquals(false, importer.IsInvoicedQuantityImport);
		}

		public void TestOrdersToImport()
		{
			CommonShipment ship = CommonShipment.New(Factory);
			Order order = Factory.New<Order>();
			order.BuyerPK = Org.PK;
			order.JD_OrderNumber = "1";
			order.JD_JS = ship.PK;
			ship.ConsigneePK = Org.PK;
			declaration.JE_JS = ship.PK;
			Factory.Save();

			OrderInvoiceImporter importer1 = new OrderInvoiceImporter(declaration);

			AssertEquals("PreCondition: Order JD_JE should be Empty", ZGuid.Empty, order.JD_JE);
			AssertEquals("Orders To Import should have count of 1", 1, importer1.OrdersToImport.Count);

			order.JD_JS = ZGuid.Empty;
			order.JD_JE = declaration.PK;
			declaration.JE_JS = ZGuid.Empty;
			Factory.Save();

			OrderInvoiceImporter importer2 = new OrderInvoiceImporter(declaration);

			AssertEquals("Orders To Import should have count of 1", 1, importer2.OrdersToImport.Count);
		}

		public void TestOrdersToImportForCoload()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = "AGT";

			var shipment = Factory.New<ForwardingShipment>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = shipment.PK;
			shipment.Consols.Add(consol);

			var declaration = Factory.New<BaseJobDeclaration>();
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.JE_JS = shipment.PK;

			var subShipment = shipment.CoLoadShipments.AddNew();
			order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = subShipment.PK;

			subShipment = shipment.CoLoadShipments.AddNew();
			order = Factory.NewWithValidTestData<Order>();
			order.JD_JS = subShipment.PK;
			Factory.Save();

			var orderImporter = new OrderInvoiceImporter(declaration);

			AssertEquals("orders from both coload shipments", 3, orderImporter.OrdersToImport.Count);
		}

		public void TestValidateOrders()
		{
			Order order = Factory.New<Order>();
			order.BuyerPK = Org.PK;
			order.JD_OrderNumber = "1";
			order.JD_JE = declaration.PK;
			Factory.Save();

			OrderInvoiceImporter importer = new OrderInvoiceImporter(declaration);
			order = importer.OrdersToImport[0];

			AssertEquals(false, order.HasRowErrors);
			AssertEquals(false, order.HasRowWarnings);

			importer.ValidateOrders();
			AssertEquals(true, order.HasRowErrors);
			string expectedMessage = "This Order has no Order Lines. Data will not be imported";
			AssertEquals("Should contain Error '" + expectedMessage + "'", true, order.RowErrors.Contains(expectedMessage));
			AssertEquals(false, order.HasRowWarnings);

			OrderLine line = order.OrderLines.AddNew();
			importer.ValidateOrders();
			AssertEquals(false, order.HasRowErrors);
			AssertEquals(false, order.HasRowWarnings);

			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_JO = line.PK;
			importer.ValidateOrders();
			AssertEquals(false, order.HasRowErrors);
			AssertEquals(true, order.HasRowWarnings);
			expectedMessage = "Some order lines on this Order have previously been imported. These will override the existing invoice line";
			AssertEquals("Should contain Warning '" + expectedMessage + "'", true, order.RowWarnings.Contains(expectedMessage));
		}

		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			return new OrderInvoiceImporter(dec);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
		}

		OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
				}
				return fOrg;
			}
		}
		OrgHeader fOrg;
		BaseJobDeclaration declaration;

		#endregion
	}
}
