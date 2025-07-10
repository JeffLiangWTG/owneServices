using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineConfirmationConcurrencyCheckTest : TestCaseWithFactory
	{
		public void TestPackLineConfirmationConcurrencyCheck_StateShipment()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			RunConcurrencyCheck();
		}

		public void TestPackLineConfirmationConcurrencyCheck_StateConsol()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			RunConcurrencyCheck();
		}

		public void TestPackLineConfirmationConcurrencyCheck_StateOrder()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);
			RunConcurrencyCheck();
		}

		public void TestPackLineConfirmationConcurrencyCheck_StateUnknown()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Unknown);
			RunConcurrencyCheck();
		}

		public void TestPackLineConfirmationConcurrencyCheck_StateTallyContainer()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.TallyContainer);
			RunConcurrencyCheck();
		}

		public void TestPackLineConfirmationConcurrencyCheck_StateNotSet()
		{
			RunConcurrencyCheck();
		}

		void RunConcurrencyCheck()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.FillWithValidTestData();
			packLine.JL_PackageCount = 20;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			confirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 24);

			Factory.Save();

			AssertEquals("Should be registered.", true, PackLineConfirmationConcurrencyCheck.IsRegistered(Factory));

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;

			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);

			var newConfirm = shipmentInNewFactory.DeliveryConfirms.AddNew();
			newConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			newConfirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);

			newFactory.Save();

			var newPackLine = shipment.OuterPackLines.AddNew();
			newPackLine.JL_PackageCount = 15;

			var exception = AssertExceptionThrown<ZCannotSaveException>(Factory.Save);
			AssertEquals("Packing Line And Confirmation concurrency error, you must reopen this form again before saving", exception.Heading);
			AssertEquals("Another user has made changes on the Delivery tab - Confirmations that conflicts with your own changes. You must reopen this form again before saving.", exception.Message);
			Assert("Exception should be marked as retriable", exception.ShouldReprocess);
		}

		public void TestConcurrencyCheckWithNoExistingConfirmations()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.FillWithValidTestData();
			packLine.JL_PackageCount = 20;

			Factory.Save();

			AssertEquals("Should be registered.", true, PackLineConfirmationConcurrencyCheck.IsRegistered(Factory));

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;

			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);

			var newConfirm = shipmentInNewFactory.DeliveryConfirms.AddNew();
			newConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			newConfirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);

			newFactory.Save();

			var newPackLine = shipment.OuterPackLines.AddNew();
			newPackLine.JL_PackageCount = 15;

			var exception = AssertExceptionThrown<ZCannotSaveException>(Factory.Save);
			AssertEquals("Packing Line And Confirmation concurrency error, you must reopen this form again before saving", exception.Heading);
			AssertEquals("Another user has made changes on the Delivery tab - Confirmations that conflicts with your own changes. You must reopen this form again before saving.", exception.Message);
			Assert("Exception should be marked as retriable", exception.ShouldReprocess);
		}

		public void TestPackLineConfirmationConcurrencyCheck_NoConcurrencyErrors()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.FillWithValidTestData();
			packLine.JL_PackageCount = 20;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			confirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 24);

			AssertEquals("Should be registered.", true, PackLineConfirmationConcurrencyCheck.IsRegistered(Factory));
			AssertNoExceptionThrown(Factory.Save);
		}
	}
}
