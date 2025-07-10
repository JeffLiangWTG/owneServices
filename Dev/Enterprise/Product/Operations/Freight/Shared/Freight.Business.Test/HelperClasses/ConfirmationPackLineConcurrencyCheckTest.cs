using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using ConfirmTypes = Enterprise.Core.Constants.PickupDeliveryConfirmTypes;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConfirmationPackLineConcurrencyCheckTest : TestCaseWithFactory
	{
		public void TestConfirmationPackLineConcurrencyCheck_NoGUI()
		{
			var isUserInteractive = Globals.IsUserInteractive;
			Assert("Precondition: everything is going well", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			try
			{
				Globals.IsUserInteractive = false;
				RunConcurrencyCheck();
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
				ErrorReporter.Clear();
			}
		}

		public void TestConfirmationPackLineConcurrencyCheck_StateShipment()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			RunConcurrencyCheck();
		}

		public void TestConfirmationPackLineConcurrencyCheck_StateConsol()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			RunConcurrencyCheck();
		}

		public void TestConfirmationPackLineConcurrencyCheck_StateOrder()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Order);
			RunConcurrencyCheck();
		}

		public void TestConfirmationPackLineConcurrencyCheck_StateUnknown()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Unknown);
			RunConcurrencyCheck();
		}

		public void TestConfirmationPackLineConcurrencyCheck_StateTallyContainer()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.TallyContainer);
			RunConcurrencyCheck();
		}

		public void TestConfirmationPackLineConcurrencyCheck_StateNotSet()
		{
			RunConcurrencyCheck();
		}

		void RunConcurrencyCheck()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.FillWithValidTestData();
			packLine.JL_PackageCount = 20;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryType = ConfirmTypes.DestinationDelivery;

			Factory.Save();

			AssertEquals("Should be registered.", true, ConfirmationPackLineConcurrencyCheck.IsRegistered(Factory));

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;

			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);
			var newPackLine1 = shipmentInNewFactory.OuterPackLines.AddNew();
			newPackLine1.JL_PackageCount = 15;
			var newPackLine2 = shipmentInNewFactory.OuterPackLines.AddNew();
			newPackLine2.JL_PackageCount = 16;

			newFactory.Save();

			var newConfirm1 = shipment.DeliveryConfirms.AddNew();
			newConfirm1.EU_PickupDeliveryType = ConfirmTypes.DestinationDelivery;
			newConfirm1.EU_DriversName = "DS2";
			Assert(newConfirm1.IsSavedByFactory);

			var newConfirm2 = shipment.DeliveryConfirms.AddNew();
			newConfirm2.EU_PickupDeliveryType = ConfirmTypes.DestinationDelivery;
			newConfirm2.EU_DriversName = "DS2";
			Assert(newConfirm2.IsSavedByFactory);

			var exception = AssertExceptionThrown<ZCannotSaveException>(Factory.Save);
			AssertEquals("Confirmation And Packing Line concurrency error, you must reopen this form again before saving", exception.Heading);
			AssertContains("Another user has made changes on the Packing tab that conflicts with your own changes. You must reopen this form again before saving.", exception.Message);
			Assert("Exception should be marked as retriable", exception.ShouldReprocess);

			if (!Globals.IsUserInteractive)
			{
				AssertContains($"Confirm from current factory: {newConfirm1.PK}|{newConfirm1.IsInDatabase}|{newConfirm1.IsDeleted}|{newConfirm1.EU_JS}", exception.Message);
				AssertContains("Constructor StackTrace start:", exception.Message);
				AssertContains($@"PackLine from current factory: {packLine.PK}|True|False|{packLine.JL_PackageCount}
PackLines:
 - '{packLine.PK}', Packs: '{packLine.JL_PackageCount}'
 - '{newPackLine1.PK}', Packs: '{newPackLine1.JL_PackageCount}'
 - '{newPackLine2.PK}', Packs: '{newPackLine2.JL_PackageCount}'",
 exception.Message);
				AssertEquals("StackTrace should only appear once to minimise performance impact", 1, ContainsCount(exception.Message, "Constructor StackTrace start:"));
			}
		}

		int ContainsCount(string withinText, string substring)
		{
			int count = 0;
			int index = 0;

			while ((index = withinText.IndexOf(substring, index)) != -1)
			{
				count++;
				index += substring.Length;
			}

			return count;
		}

		public void TestConfirmationPackLineConcurrencyCheck_NoConcurrencyErrors()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.FillWithValidTestData();
			packLine.JL_PackageCount = 20;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryType = ConfirmTypes.DestinationDelivery;

			AssertEquals("Should be registered.", true, ConfirmationPackLineConcurrencyCheck.IsRegistered(Factory));
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestContainerGetOrCreateConfirm_NoConcurrencyErrorsAndNotSavedToDB()
		{
			Factory.RefreshEnabled = false;
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var container = packLine.Containers.AddNew();
			consol.Containers.Add(container);
			consol.Shipments.Add(shipment);
			Factory.Save();

			CreateDeliveryConfirmAndNewPackLineInAnotherFactory(shipment);

			var confirm = container.DestinationConfirm;
			AssertEquals("Should be registered.", true, ConfirmationPackLineConcurrencyCheck.IsRegistered(Factory));
			Assert("Empty confirm will not cause a database write. So it can't have a concurrency error.", !confirm.IsSavedByFactory);
			AssertEquals(1, confirm.Shipments.Count);

			AssertNoExceptionThrown(Factory.Save);
			Assert(!confirm.IsInDatabase);
		}

		public void TestEmptyConfirm_NoConcurrencyErrorsAndNotSavedToDB()
		{
			Factory.RefreshEnabled = false;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			Factory.Save();

			CreateDeliveryConfirmAndNewPackLineInAnotherFactory(shipment);

			var confirm = shipment.DeliveryConfirms.AddNew();
			AssertEquals("Should be registered.", true, ConfirmationPackLineConcurrencyCheck.IsRegistered(Factory));
			Assert("Empty confirm will not cause a database write. So it can't have a concurrency error.", !confirm.IsSavedByFactory);
			AssertEquals(1, confirm.Shipments.Count);

			AssertNoExceptionThrown(Factory.Save);
			Assert(!confirm.IsInDatabase);
		}

		void CreateDeliveryConfirmAndNewPackLineInAnotherFactory(CommonShipment shipment)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadedShipment = factory.Load<CommonShipment>(shipment.PK);
			reloadedShipment.OuterPackLines.AddNew();
			var confirm = reloadedShipment.DeliveryConfirms.AddNew();
			confirm.EU_DriversName = "DS2";
			factory.Save();
			Assert(confirm.IsInDatabase);
		}
	}
}
