using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConfirmTimesSyncHelperTest : TestCaseWithFactory
	{
		public void TestSetShipmentLooseConfirms()
		{
			TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned);
			TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy);
			TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual);

			TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned);
			TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy);
			TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual);
		}

		void TestLooseConfirms(ConfirmTimesSyncHelper.ConfirmType confirmType, ConfirmTimesSyncHelper.ConfirmDateType confirmDateType)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.OuterPackLines.AddNew();

			var today = ZDateTime.Today;
			var confirms = (confirmType == ConfirmTimesSyncHelper.ConfirmType.Pickup) ? shipment.PickupConfirms : shipment.DeliveryConfirms;
			var confirmDateFieldName = ConfirmTimesSyncHelper.GetConfirmDateFieldName(confirmDateType);
			AssertEquals(0, confirms.Count);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today, today, false);
			AssertEquals(0, confirms.Count);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today, ZDateTime.Invalid, true);
			AssertEquals(0, confirms.Count);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today, ZDateTime.Empty, true);
			AssertEquals(0, confirms.Count);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today, today, true);
			AssertEquals(1, confirms.Count);
			AssertEquals(today, confirms[0][confirmDateFieldName]);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today, today.AddDays(1), true);
			AssertEquals(1, confirms.Count);
			AssertEquals(today.AddDays(1), confirms[0][confirmDateFieldName]);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today.AddDays(1), ZDateTime.Invalid, true);
			AssertEquals(1, confirms.Count);
			AssertEquals(today.AddDays(1), confirms[0][confirmDateFieldName]);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, confirmType, confirmDateType, today.AddDays(1), ZDateTime.Empty, true);
			AssertEquals(1, confirms.Count);
			AssertEquals(ZDateTime.Empty, confirms[0][confirmDateFieldName]);
		}

		public void TestLooseConfirmNeedsToBeCreated()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.DocsAndCartage.GetType().GetField("fSetShipmentDatesSuspensionLevel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shipment.DocsAndCartage, 1);
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AssertEquals(1, shipment.DocsAndCartage.SetShipmentDatesSuspensionLevel);
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.DocsAndCartage.GetType().GetField("fSetShipmentDatesSuspensionLevel", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shipment.DocsAndCartage, 0);
			AssertEquals(false, ConfirmTimesSyncHelper.IsValidShipmentForAddingConfirmations(shipment));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(true, shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(true, shipment.ShowContainerisedConfirms(ConfirmTimesSyncHelper.ConfirmType.Delivery));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedPickup);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Invalid;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Invalid;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Invalid;
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Invalid;
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Invalid;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Invalid, shipment.DocsAndCartage.JP_EstimatedPickup);
			AssertEquals(ZDateTime.Invalid, shipment.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(ZDateTime.Invalid, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(ZDateTime.Invalid, shipment.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals(ZDateTime.Invalid, shipment.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(ZDateTime.Invalid, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));

			var today = ZDateTime.Today;
			shipment.DocsAndCartage.JP_EstimatedPickup = today;
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = today;
			AssertEquals(0, shipment.OuterPackLines.Count);
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(false, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));

			shipment.OuterPackLines.AddNew();
			AssertEquals(true, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(true, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));
		}

		public void TestCreateLooseConfirmIfNeeded()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;

			var today = ZDateTime.Today;
			shipment.DocsAndCartage.JP_PickupRequiredBy = today;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = today;
			shipment.OuterPackLines.AddNew();

			AssertEquals(true, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup));
			AssertEquals(0, shipment.PickupConfirms.Count);

			ConfirmTimesSyncHelper.CreateLooseConfirmIfNeeded(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup);
			AssertEquals(1, shipment.PickupConfirms.Count);
			AssertEquals(today, shipment.PickupConfirms[0].EU_RequestedPickupDeliveryTime);

			AssertEquals(true, ConfirmTimesSyncHelper.LooseConfirmNeedsToBeCreated(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery));
			AssertEquals(0, shipment.DeliveryConfirms.Count);

			ConfirmTimesSyncHelper.CreateLooseConfirmIfNeeded(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery);
			AssertEquals(1, shipment.DeliveryConfirms.Count);
			AssertEquals(today, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
		}

		public void TestContainerCollectionWasModified()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			CommonConsol consol = factory1.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "ASDF4564561";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "DFDF1212127";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 15;

			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = 10;

			packLine1.SetContainer(consol, container1);
			packLine2.SetContainer(consol, container2);
			packLine3.SetContainer(consol, container1);
			packLine4.SetContainer(consol, container2);

			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonShipment shipment1 = factory2.Load<CommonShipment>(shipment.PK);
			container1.Delete();

			var today = ZDateTime.Today;

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment1, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Actual, ZDateTime.Empty, today.AddDays(5), true);
			AssertNoErrors(shipment1.DocsAndCartage.JP_DeliveryCartageCompletedInfo);
		}

		public void TestSetLooseConfirm()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;

			var today = ZDateTime.Today;
			shipment.DocsAndCartage.JP_PickupRequiredBy = today;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = today;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;

			AssertEquals(0, shipment.PickupConfirms.Count);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Planned, ZDateTime.Empty, today.AddDays(1), true);
			AssertEquals(1, shipment.PickupConfirms.Count);
			AssertEquals(today, shipment.PickupConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(today.AddDays(1), shipment.PickupConfirms[0].EU_PlannedPickupDeliveryTime);
			AssertEquals(10, shipment.PickupConfirms[0].Divots[0].J8_PackagesDelivered);

			ConfirmTimesSyncHelper.SetShipmentTimesFromConfirmIfRequired(shipment, shipment.PickupConfirms[0]);
			AssertEquals(today, shipment.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(today.AddDays(1), shipment.DocsAndCartage.JP_EstimatedPickup);

			shipment.PickupConfirms[0].Divots[0].J8_PackagesDelivered = 9;
			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Pickup, ConfirmTimesSyncHelper.ConfirmDateType.Actual, ZDateTime.Empty, today.AddDays(2), true);
			AssertEquals(2, shipment.PickupConfirms.Count);

			AssertEquals(today, shipment.PickupConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(today.AddDays(1), shipment.PickupConfirms[0].EU_PlannedPickupDeliveryTime);
			AssertEquals(today.AddDays(2), shipment.PickupConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(9, shipment.PickupConfirms[0].Divots[0].J8_PackagesDelivered);

			AssertEquals(today, shipment.PickupConfirms[1].EU_RequestedPickupDeliveryTime);
			AssertEquals(today.AddDays(1), shipment.PickupConfirms[1].EU_PlannedPickupDeliveryTime);
			AssertEquals(today.AddDays(2), shipment.PickupConfirms[1].EU_PickupDeliveryTime);
			AssertEquals(1, shipment.PickupConfirms[1].Divots[0].J8_PackagesDelivered);

			AssertEquals(0, shipment.DeliveryConfirms.Count);

			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.RequestedBy, ZDateTime.Empty, today.AddDays(1), true);
			AssertEquals(1, shipment.DeliveryConfirms.Count);
			AssertEquals(today, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(today.AddDays(1), shipment.DeliveryConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(10, shipment.DeliveryConfirms[0].Divots[0].J8_PackagesDelivered);

			ConfirmTimesSyncHelper.SetShipmentTimesFromConfirmIfRequired(shipment, shipment.DeliveryConfirms[0]);
			AssertEquals(today, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(today.AddDays(1), shipment.DocsAndCartage.JP_DeliveryRequiredBy);

			shipment.DeliveryConfirms[0].Divots[0].J8_PackagesDelivered = 8;
			ConfirmTimesSyncHelper.SetConfirmTimes(shipment, ConfirmTimesSyncHelper.ConfirmType.Delivery, ConfirmTimesSyncHelper.ConfirmDateType.Planned, ZDateTime.Empty, today.AddDays(2), true);
			AssertEquals(2, shipment.DeliveryConfirms.Count);

			AssertEquals(today, shipment.DeliveryConfirms[0].EU_PickupDeliveryTime);
			AssertEquals(today.AddDays(1), shipment.DeliveryConfirms[0].EU_RequestedPickupDeliveryTime);
			AssertEquals(today.AddDays(2), shipment.DeliveryConfirms[0].EU_PlannedPickupDeliveryTime);
			AssertEquals(8, shipment.DeliveryConfirms[0].Divots[0].J8_PackagesDelivered);

			AssertEquals(today, shipment.DeliveryConfirms[1].EU_PickupDeliveryTime);
			AssertEquals(today.AddDays(1), shipment.DeliveryConfirms[1].EU_RequestedPickupDeliveryTime);
			AssertEquals(today.AddDays(2), shipment.DeliveryConfirms[1].EU_PlannedPickupDeliveryTime);
			AssertEquals(2, shipment.DeliveryConfirms[1].Divots[0].J8_PackagesDelivered);
		}
	}
}
