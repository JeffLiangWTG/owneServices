using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonConfirmDivot))]
	sealed class CommonConfirmDivotDivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine line = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			return confirm.Divots[0];
		}

		[ExpectNoExceptions]
		public void TestGetPackLineThrowsNoExceptionWhenPackLineTypeIsNull()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];
			divot.PackLineType = null;
			Factory.Save();
		}

		public void TestDeletedRowInaccessibleException()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];
			divot.Delete();

			AssertNoExceptionThrown(() => divot.J8_PackagesDelivered = 99);
			ErrorReporter.Clear();
		}

		public void TestDivotsValidationChangesWithPackingMode()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 50;
			packLine.JL_ActualWeight = 5000m;

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];
			Factory.Save();

			divot.J8_DeliveryWeight = 8000m;

			AssertHasWarnings("Expected an error as the delivery weight should not be greater than the pack line weight", divot.J8_DeliveryWeightInfo);
			AssertEquals("Expected shipment have errors for the above reason", true, divot.HasWarnings());

			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			divot.J8_DeliveryWeight = 6000m;
			divot.RunPreSaveValidation();

			AssertEquals("Expected shipment have no errors as the divot and its weight should not be visible", false, divot.HasErrors());

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.DeliveryConfirms.Delete(confirm);
			divot.RunPreSaveValidation();

			AssertEquals("Expected shipment have no error as the invalid delivery confirmation was removed", false, divot.HasErrors());
		}

		public void TestEmptyDivotIsSavedIfItHasNonEmptySiblings()
		{
			var shipment = Factory.New<CommonShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			pack1.JL_PackageCount = 10;
			pack2.JL_PackageCount = 20;

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot1 = confirm.Divots[0];
			var divot2 = confirm.Divots[1];

			divot1.J8_PackagesDelivered = 0;
			divot2.J8_PackagesDelivered = 0;

			Assert("Confirm is empty", confirm.IsEmpty);
			Assert("Divot1 is empty", divot1.IsEmpty);
			Assert("Divot2 is empty", divot2.IsEmpty);

			Assert("Confirm should not be saved", !confirm.IsSavedByFactory);
			Assert("Divot1 should not be saved", !divot1.IsSavedByFactory);
			Assert("Divot2 should not be saved", !divot2.IsSavedByFactory);

			divot1.J8_PackagesDelivered = 5;

			Assert("Confirm is still empty", confirm.IsEmpty);
			Assert("Divot1 is not empty", !divot1.IsEmpty);
			Assert("Divot2 is still empty", divot2.IsEmpty);

			Assert("Confirm should be saved", confirm.IsSavedByFactory);
			Assert("Divot1 should be saved", divot1.IsSavedByFactory);
			Assert("Divot2 should be saved", divot2.IsSavedByFactory);
		}

		public void TestDivotSavedWhenConfirmIsInDatabase()
		{
			var shipment = Factory.New<CommonShipment>();

			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 10;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_DriversName = "CJP";

			var divot1 = confirm.Divots[0];
			divot1.J8_PackagesDelivered = 0;

			Factory.Save();

			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 20;

			var divot2 = confirm.Divots[1];
			confirm.EU_DriversName = "";

			Assert("Confirm is empty", confirm.IsEmpty);
			Assert("Confirm is in database", confirm.IsInDatabase);
			Assert("divot2 is not in database", !divot2.IsInDatabase);
			Assert("divot2 is not deleted", !divot2.IsDeleted);
			Assert("divot2 is saved by factory", divot2.IsSavedByFactory);
		}

		public void TestJ8_EU_PickupDeliveryConfirm_LogsInformationWhenChanged()
		{
			CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage = string.Empty;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			var pickupConfirm = shipment.PickupConfirms.AddNew();
			string getDebugLog() => CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage;

			var divot = Factory.New<CommonConfirmDivot>();
			divot.J8_EU_PickupDeliverConfirm = deliveryConfirm.PK;
			AssertEquals("No logging occurs on first set of property", string.Empty, getDebugLog());

			divot.J8_EU_PickupDeliverConfirm = deliveryConfirm.PK;
			AssertEquals("No logging occurs when property doesn't change value", string.Empty, getDebugLog());

			divot.J8_EU_PickupDeliverConfirm = pickupConfirm.PK;
			AssertStartsWith(
				"Logging information occurs when property value changes",
				$"CommonConfirmDivot: '{divot.PK}' is being relinked from confirm: '{deliveryConfirm.PK}' to '{pickupConfirm.PK}'",
				getDebugLog());
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];
			divot.J8_DeliveryVolume = 10m;

			return confirm;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 10;

			var confirm = shipment.DeliveryConfirms.AddNew();

			var divot = confirm.Divots[0];
			divot.J8_DeliveryVolume = 10m;

			return divot;
		}

		protected override void InstallBizoForTestDbHits(BusinessObject bizo)
		{
			var divot = bizo as CommonConfirmDivot;
			if (divot != null)
			{
				var confirm = divot.Confirm;
				confirm.PackLineType = typeof(PackLine);
			}
		}
	}
}
