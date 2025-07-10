using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineDocumentSupporter))]
	sealed class OrderLineDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestControllingCustomerContactOrganisation()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			var controllingCustomer = Factory.New<OrgHeader>();

			order.ControllingCustomerDocAddress.OrganisationPK = controllingCustomer.PK;
			AssertEquals("Controlling Customer from order", controllingCustomer,
				(OrgHeader)orderLine.DocumentSupporter.GetContactOrganisation("", ContactType.ControllingCustomer, DocumentDirection.ARV).OrgHeader);

			order.ControllingCustomerDocAddress.E2_AddressOverride = true;
			AssertNull((OrgHeader)(orderLine as IDocumentSupportable).DocumentSupporter.GetContactOrganisation("", ContactType.ControllingCustomer, DocumentDirection.ARV).OrgHeader);
		}

		public void TestBuyerContactOrganisation()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.MainAddress.OA_Address1 = "Test Address 1";

			order.BuyerPK = buyer.PK;
			AssertEquals("Buyer from order", buyer,
				(OrgHeader)orderLine.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ARV).OrgHeader);
		}

		public void TestSupplierContactOrganisation()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();

			var supplier = Factory.New<OrgHeader>();
			supplier.MainAddress.OA_Address1 = "Test Address 1";

			order.SupplierPK = supplier.PK;
			AssertEquals("Supplier from order", supplier,
				(OrgHeader)orderLine.DocumentSupporter.GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ARV).OrgHeader);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();

			Factory.Save();

			return orderLine;
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get
			{
				return new[]
				{
					Factory.New<OrderLine>()
				};
			}
		}

		protected override bool ShouldSkipWithContextAndMenu(Core.Constants.DataContext context, IStmMenuItem menu)
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();

			var supporter = orderLine.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return base.ExcludeDocumentCommandTest(documentCommand) || new[]
				{
					"Order Pre-Advice",
					"Request for Missing Documents",
					"Order Status",
					"Delay Alert",
					"Order Notification",
					"Order Advice",
					"Shipped on Board Advice",
					"Landed Costing",
					"Amendment To Booking",
					"Shipped on Board Advice",
					"Routing Order"
				}.Any(s => documentCommand.SU_MenuName.Contains(s));
		}

		#endregion
	}
}
