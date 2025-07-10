using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class PickupDeliveryConfirmationDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateFCLPickupConfirmation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT11111";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;

			var now = ZDateTime.Now;

			var confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_JC = container.PK;
			confirm.EU_PlannedPickupDeliveryTime = now;
			confirm.EU_RequestedPickupDeliveryTime = now.AddDays(1);
			confirm.EU_PickupDeliveryTime = now.AddDays(2);
			confirm.EU_GoodsSignForBy = "Nobody";
			confirm.EU_VehicleRegistration = "00AA00";
			confirm.EU_Distance = 3m;
			confirm.EU_DistanceUnit = "KM";

			var confirmationData = new PickupDeliveryConfirmationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment))).GetDataObject(confirm);

			AssertEquals("PIC", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);
		}

		public void TestPopulateFCLDeliveryConfirmation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT11111";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;

			var now = ZDateTime.Now;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_JC = container.PK;
			confirm.EU_PlannedPickupDeliveryTime = now;
			confirm.EU_RequestedPickupDeliveryTime = now.AddDays(1);
			confirm.EU_PickupDeliveryTime = now.AddDays(2);
			confirm.EU_GoodsSignForBy = "Nobody";
			confirm.EU_VehicleRegistration = "00AA00";
			confirm.EU_Distance = 3m;
			confirm.EU_DistanceUnit = "KM";

			var confirmationData = new PickupDeliveryConfirmationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment))).GetDataObject(confirm);

			AssertEquals("DLV", confirmationData.DateDescription);
			AssertEquals(now, confirmationData.EstimatedDate);
			AssertEquals(now.AddDays(1), confirmationData.RequiredToDate);
			AssertEquals(now.AddDays(2), confirmationData.ActualDate);
			AssertEquals("Nobody", confirmationData.ReceivedBy);
			AssertEquals(3m, confirmationData.Distance);
			AssertEquals("KM", confirmationData.DistanceUnit.Code);
			AssertEquals("Kilometers", confirmationData.DistanceUnit.Description);
		}
	}
}
