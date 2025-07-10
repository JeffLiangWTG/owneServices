using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public sealed class ForwardingDocsAndCartageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJP_OrderItemsAsString()
		{
			var cartage = (ForwardingDocsAndCartage)ForwardingDocsAndCartage.New(Factory.New<ForwardingShipment>());

			cartage.JP_OrderItemsAsString = "123, 456";
			AssertEquals("Shouldn't have any errors", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			cartage.JP_OrderItemsAsString = "123,3~3";
			AssertEquals("Shouldn't have any errors", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			cartage.JP_OrderItemsAsString = "12~3, 456";
			AssertEquals("Shouldn't have any errors", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			cartage.JP_OrderItemsAsString = "123, 656";
			AssertEquals("Shouldn't have any errors", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			cartage.JP_OrderItemsAsString = "123,3`3";
			AssertEquals("Should have 1 error", 1, cartage.JP_OrderItemsAsStringInfo.GetErrors().Count());

			cartage.JP_OrderItemsAsString = "1`23,33";
			AssertEquals("Should have 1 error", 1, cartage.JP_OrderItemsAsStringInfo.GetErrors().Count());

			cartage.JP_OrderItemsAsString = "123456789012345678901234578900";
			AssertEquals("Errors as too long", true, cartage.JP_OrderItemsAsStringInfo.HasErrors());
		}

		public void TestValidateJP_OrderItemsAsString_RequiresOrderTrackLink()
		{
			var parent = new MockJobDocsAndCartageParentForTesting(Factory);
			var cartage = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent);

			parent.RequiresOrderTrackLink = false;
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("No order references and requires them", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			parent.RequiresOrderTrackLink = true;
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("No order references and requires them", true, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			parent.RequiresOrderTrackLink = true;
			cartage.JP_OrderItemsAsString = "xxx";
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("Order references exist but still no link to order tracking", true, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			parent.RequiresOrderTrackLink = true;
			parent.AttachedOrders.AddNew();
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("Order tracking orders attached and order tracking required", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());
		}

		public void TestValidateJP_OrderItemsAsString_RequiresOrderNumbersOnDocs()
		{
			var parent = new MockJobDocsAndCartageParentForTesting(Factory);
			var cartage = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent);

			parent.RequiresOrderNumbersOnDocs = false;
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("No order references and requires them", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			parent.RequiresOrderNumbersOnDocs = true;
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("No order references and requires them", true, cartage.JP_OrderItemsAsStringInfo.HasErrors());

			parent.RequiresOrderNumbersOnDocs = true;
			cartage.JP_OrderItemsAsString = "xxx";
			cartage.Validation.ValidateJP_OrderItemsAsString();
			AssertEquals("Order references exist and requires them", false, cartage.JP_OrderItemsAsStringInfo.HasErrors());
		}

		public void TestValidateJP_OrderItemsAsString_GenericOrdersShouldBeChecked()
		{
			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			var cartage = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			parent.ConsigneePK = consignee.PK;
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;
			parent.GenericOrders.Add(Factory.NewWithValidTestData(ObjectFactory.GetType<Warehouse.Integration.IWhsOrder>()));

			AssertEquals("Precondition - Warehouse order should not be attached to attached orders.", 0, parent.AttachedOrders.Count);
			AssertEquals("Precondition - Warehosue order should have been attached to generic orders.", 1, parent.GenericOrders.Count);

			AssertNoError(cartage.JP_OrderItemsAsStringInfo, "You cannot save without attaching any orders.");

			parent.GenericOrders.RemoveAndDeleteAll();
			AssertHasError(cartage.JP_OrderItemsAsStringInfo, "You cannot save without attaching any orders.");
		}

		public void TestValidateJP_OrderItemsAsString_RequiresOrderNumbersOnDocsShouldCheckGenericOrders()
		{
			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			var cartage = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			parent.ConsigneePK = consignee.PK;
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = false;
			parent.GenericOrders.Add(Factory.NewWithValidTestData(ObjectFactory.GetType<Warehouse.Integration.IWhsOrder>()));

			AssertNoError(cartage.JP_OrderItemsAsStringInfo, "You cannot save without specifying any orders or order references.");

			parent.GenericOrders.RemoveAndDeleteAll();
			AssertHasError(cartage.JP_OrderItemsAsStringInfo, "You cannot save without specifying any orders or order references.");
		}

		public void TestValidateJP_OrderItemsAsString_DuplicateOrderNumbers()
		{
			var expectedError = "There are duplicate Order Refs. You must either delete the duplicate number from the Order Refs field or unlink the linked Order before saving.";

			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			var cartage = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			parent.ConsigneePK = consignee.PK;
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = false;

			cartage.JP_OrderItemsAsString = "1001, 1001";

			AssertHasError(cartage.JP_OrderItemsAsStringInfo, expectedError);

			cartage.JP_OrderItemsAsString = "ORD001, WHS001";

			var whsOrder = (Warehouse.Integration.IWhsOrder)Factory.NewWithValidTestData(ObjectFactory.GetType<Warehouse.Integration.IWhsOrder>());
			whsOrder.WD_ExternalReference = "WHS001";
			parent.GenericOrders.Add((BusinessObject)whsOrder);

			AssertHasError(cartage.JP_OrderItemsAsStringInfo, expectedError);

			cartage.JP_OrderItemsAsString = "ORD001";

			AssertNoError(cartage.JP_OrderItemsAsStringInfo, expectedError);

			var order = Factory.New<Order>();
			order.JD_OrderNumber = "ORD001";
			parent.GenericOrders.Add(order);

			AssertHasError(cartage.JP_OrderItemsAsStringInfo, expectedError);
		}

		public void TestValidationNoExceptionWhenMiscServDeleted()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = consignee.Contacts.AddNew();

			consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			consignee.MiscServ.Delete();
			Factory.Save();

			var parent = Factory.NewWithValidTestData<ForwardingShipment>();
			var cartage = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent);

			AssertNoExceptionThrown(() =>
			{
				parent.ConsigneePK = consignee.PK;
			});

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = consignor.Contacts.AddNew();

			consignor.MiscServ.OM_EXJobRequireOrderTrackLink = true;
			consignor.MiscServ.OM_EXExporterRequiresOrderNumbersOnDocs = true;
			consignor.MiscServ.Delete();
			Factory.Save();

			var parent2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var cartage2 = (ForwardingDocsAndCartage)JobDocsAndCartage.New(parent2);

			AssertNoExceptionThrown(() =>
			{
				parent2.ConsignorPK = consignor.PK;
			});
		}

		public void TestCheckJP_EstimatedDeliveryDateHasAWarning()
		{
			var currentTime = ZDateTime.Now;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_E_ARV = new ZDateTime(currentTime.Year, currentTime.Month, currentTime.Day);
			shipment.DocsAndCartage.JP_EstimatedDelivery = shipment.JS_E_ARV.AddHours(-1);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.Transports[0].JW_VoyageFlight = "QF512";
			consol.Transports[0].JW_ETA = shipment.JS_E_ARV;
			shipment.Consols.Add(consol);

			Assert("There should be no error when attaching to consol", !shipment.DocsAndCartage.JP_EstimatedDeliveryInfo.HasErrors());
			Assert("There should be warnings when attaching to consol", shipment.DocsAndCartage.JP_EstimatedDeliveryInfo.HasWarnings());
		}

		#region Implementation

		public class MockJobDocsAndCartageParentForTesting : MockJobDocsAndCartageParent, IAttachOrders
		{
			public MockJobDocsAndCartageParentForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			Type IDocsAndCartageParent.DocsAndCartageType
			{
				get { return typeof(ForwardingDocsAndCartage); }
			}

			public OrderCollection AttachedOrders
			{
				get { return attachedOrders ?? (attachedOrders = new OrderCollection(Factory)); }
			}
			OrderCollection attachedOrders;

			public OrderCollection PossibleOrdersForAttachment_List
			{
				get { return null; }
			}

			public void SetDefaultsOnOrder(Order newOrder)
			{
			}

			public void OnOrderAttached(Order attachedOrder)
			{
			}
		}

		#endregion
	}
}
