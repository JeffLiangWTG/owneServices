using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonPickupDeliveryConfirm))]
	sealed class CommonPickupDeliveryConfirmTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShipmentsIsNullable()
		{
			var pickupConfirm = Factory.New<CommonPickupDeliveryConfirm>();
			pickupConfirm.EU_JS = ZGuid.Empty;

			AssertEquals(0, pickupConfirm.Shipments.Count);
			AssertNull(pickupConfirm.FirstShipment);

			pickupConfirm.EU_JS = ZGuid.NewZGuid();
			AssertEquals(0, pickupConfirm.Shipments.Count);
			AssertNull(pickupConfirm.FirstShipment);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00010001";

			pickupConfirm.EU_JS = shipment.PK;
			AssertEquals(1, pickupConfirm.Shipments.Count);
			AssertNotNull(pickupConfirm.FirstShipment);
			AssertEquals("S00010001", pickupConfirm.FirstShipment.JS_UniqueConsignRef);
		}

		#region TestInvalidPacklineIsNotLoose

		public void TestInvalidPacklineIsNotLoose()
		{
			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var pickupConfirm = shipment.PickupConfirms.AddNew();
			Assert(pickupConfirm.IsLoose);

			pickupConfirm.Divots[0].J8_JL = ZGuid.NewZGuid();
			Assert(!pickupConfirm.IsLoose);
		}

		#endregion

		#region TestEmptyPickupDeliveryTypeDoesntCauseExceptions

		public void TestEmptyPickupDeliveryTypeDoesntCauseExceptions()
		{
			CommonPickupDeliveryConfirm pickupConfirm = Factory.New<CommonPickupDeliveryConfirm>();
			pickupConfirm.EU_PickupDeliveryType = ZString.Empty;
			AssertEquals(null, pickupConfirm.DeliverTo);
			AssertEquals(null, pickupConfirm.PickupFrom);
			AssertEquals(null, pickupConfirm.ContainerYard);
			JobDocAddress dummy = pickupConfirm.ConfirmAddress;
		}

		#endregion

		#region TestSavingDocument

		[ExpectNoExceptions()]
		public void TestSavingDocument()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();

			var pickupConfirm = shipment.PickupConfirms.AddNew();
			pickupConfirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 15);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonPickupDeliveryConfirm confirmInNewFactory = newFactory.Load<CommonPickupDeliveryConfirm>(pickupConfirm.PK);
			confirmInNewFactory.Logs.AddNew(Events.DocumentDelivered, "hey hey");
			newFactory.Save();
		}

		#endregion

		#region TestWebReadOnlyCalculation

		public void TestWebReadOnlyCalculation()
		{
			bool isWebInitialSetting = Globals.IsWeb;
			Globals.IsWeb = true;

			try
			{
				CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
				Assert(!confirm.DivotsAreReadOnlyOnWeb);

				AssertPropertiesAreNotReadOnly(confirm);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, false, true);

				confirm.EU_TransportCoName = "Test Company";
				Assert(!confirm.DivotsAreReadOnlyOnWeb);

				AssertPropertiesAreNotReadOnly(confirm);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, false, true);

				confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
				Assert(!confirm.DivotsAreReadOnlyOnWeb);

				AssertPropertiesAreNotReadOnly(confirm);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, false, true);

				Factory.Save();
				Assert(confirm.DivotsAreReadOnlyOnWeb);

				AssertPropertiesAreNotReadOnly(confirm);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, true, true);

				confirm.EU_PickupDeliveryTime = ZDateTime.Now;
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, false, true);

				confirm.EU_GoodsSignForBy = "someone";

				Assert(confirm.DivotsAreReadOnlyOnWeb);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, true, true);

				confirm.EU_GoodsSignForBy = ZString.Empty;
				confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Empty;
				Assert(!confirm.DivotsAreReadOnlyOnWeb);

				AssertPropertiesAreNotReadOnly(confirm);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, false, true);

				confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
				confirm.EU_TransportCoName = string.Empty;
				Assert(!confirm.DivotsAreReadOnlyOnWeb);

				AssertPropertiesAreNotReadOnly(confirm);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, true, true);
				AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_DropModeInfo, false, true);
				AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, false, true);
			}
			finally
			{
				Globals.IsWeb = isWebInitialSetting;
			}
		}

		void AssertPropertiesAreNotReadOnly(CommonPickupDeliveryConfirm confirm)
		{
			AssertPropertyReadOnly(confirm.EU_PickupDeliveryTimeInfo, false, false);
			AssertPropertyReadOnly(confirm.EU_GoodsSignForByInfo, false, false);
			AssertPropertyReadOnly(confirm.EU_RequestedPickupDeliveryTimeInfo, false, false);
			AssertPropertyReadOnly(confirm.EU_DropModeInfo, false, false);
			AssertPropertyReadOnly(confirm.EU_PickupDeliveryInstructionInfo, false, false);
		}

		void AssertPropertyReadOnly(ZPropertyInfo property, bool expectedReadOnly, bool isWeb)
		{
			bool isWebInitialSetting = Globals.IsWeb;
			Globals.IsWeb = isWeb;
			AssertEquals("Testing " + property.HumanReadableName + "ReadOnly when Globals.IsWeb == " + isWeb, expectedReadOnly, property.ReadOnly);
			Globals.IsWeb = isWebInitialSetting;
		}

		#endregion

		#region DefaultDropMode

		public void TestDefaultDropMode()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.OuterPackLines.AddNew();
			shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "111";
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "222";

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			AssertEquals("111", pickupConfirm.EU_DropMode);

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			AssertEquals("222", deliveryConfirm.EU_DropMode);
		}

		#endregion

		#region TestSetCartageCompletedIfRequired

		public void TestSetPickupCartageCompletedIfRequired_LCL()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 20;
			packLine3.JL_PackageCount = 30;

			CommonPickupDeliveryConfirm pickupConfirm1 = shipment.PickupConfirms.AddNew();
			CommonConfirmDivot divot11 = pickupConfirm1.GetDivot(packLine1);
			CommonConfirmDivot divot12 = pickupConfirm1.GetDivot(packLine2);
			CommonConfirmDivot divot13 = pickupConfirm1.GetDivot(packLine3);
			divot11.J8_PackagesDelivered = 5;
			divot12.J8_PackagesDelivered = 10;
			divot13.J8_PackagesDelivered = 15;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);

			pickupConfirm1.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);

			CommonPickupDeliveryConfirm pickupConfirm2 = shipment.PickupConfirms.AddNew();
			CommonConfirmDivot divot21 = pickupConfirm2.GetDivot(packLine1);
			CommonConfirmDivot divot22 = pickupConfirm2.GetDivot(packLine2);
			CommonConfirmDivot divot23 = pickupConfirm2.GetDivot(packLine3);
			divot21.J8_PackagesDelivered = 5;
			divot22.J8_PackagesDelivered = 10;
			divot23.J8_PackagesDelivered = 15;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_PickupRequiredBy.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_EstimatedPickup.IsEmpty);

			ZDateTime now = ZDateTime.Now;

			shipment.DocsAndCartage.JP_PickupCartageCompleted = now.AddDays(2);
			shipment.DocsAndCartage.JP_PickupRequiredBy = now.AddDays(2);
			shipment.DocsAndCartage.JP_EstimatedPickup = now.AddDays(2);

			//complete all confirms
			pickupConfirm1.EU_PickupDeliveryTime = now;
			pickupConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			pickupConfirm2.EU_PickupDeliveryTime = now;
			pickupConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(3);
			Factory.Save();

			AssertEquals(now.AddDays(2), shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(now.AddDays(2), shipment.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(now.AddDays(2), shipment.DocsAndCartage.JP_EstimatedPickup);

			now = ZDateTime.Now.AddMinutes(2);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Empty;

			//complete all confirms
			pickupConfirm1.EU_PickupDeliveryTime = now;
			pickupConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			pickupConfirm2.EU_PickupDeliveryTime = now;
			pickupConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(3);
			Factory.Save();

			AssertEquals(now, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(now.AddHours(2), shipment.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(now.AddHours(3), shipment.DocsAndCartage.JP_EstimatedPickup);
		}

		public void TestSetPickupCartageCompletedIfRequired_FCL()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			packLine2.SetContainer(consol, container2);
			packLine3.SetContainer(consol, container3);

			CommonPickupDeliveryConfirm pickupConfirm1 = container1.OriginConfirm;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);

			pickupConfirm1.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals(pickupConfirm1.EU_PickupDeliveryTime, pickupConfirm1.Container.JC_DepartureCartageComplete);

			CommonPickupDeliveryConfirm pickupConfirm2 = container2.OriginConfirm;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);

			pickupConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			AssertEquals(pickupConfirm1.EU_PickupDeliveryTime, pickupConfirm1.Container.JC_DepartureCartageComplete);

			CommonPickupDeliveryConfirm pickupConfirm3 = container3.OriginConfirm;
			Assert(shipment.DocsAndCartage.JP_PickupCartageCompleted.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_PickupRequiredBy.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_EstimatedPickup.IsEmpty);

			ZDateTime now = ZDateTime.Now;

			pickupConfirm1.EU_PickupDeliveryTime = now;
			pickupConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			pickupConfirm2.EU_PickupDeliveryTime = now;
			pickupConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			pickupConfirm3.EU_PickupDeliveryTime = now;
			pickupConfirm3.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			pickupConfirm3.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			AssertEquals(pickupConfirm3.EU_PlannedPickupDeliveryTime, pickupConfirm3.Container.JC_DepartureEstimatedPickup);
			Factory.Save();
			AssertEquals(now, shipment.DocsAndCartage.JP_PickupCartageCompleted);
			AssertEquals(now.AddHours(2), shipment.DocsAndCartage.JP_PickupRequiredBy);
			AssertEquals(now.AddHours(3), shipment.DocsAndCartage.JP_EstimatedPickup);
		}

		public void TestSetDeliveryCartageCompletedIfRequired_LCL()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 20;
			packLine3.JL_PackageCount = 30;

			CommonPickupDeliveryConfirm deliveryConfirm1 = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot11 = deliveryConfirm1.GetDivot(packLine1);
			CommonConfirmDivot divot12 = deliveryConfirm1.GetDivot(packLine2);
			CommonConfirmDivot divot13 = deliveryConfirm1.GetDivot(packLine3);
			divot11.J8_PackagesDelivered = 5;
			divot12.J8_PackagesDelivered = 10;
			divot13.J8_PackagesDelivered = 15;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			deliveryConfirm1.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			CommonPickupDeliveryConfirm deliveryConfirm2 = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot21 = deliveryConfirm2.GetDivot(packLine1);
			CommonConfirmDivot divot22 = deliveryConfirm2.GetDivot(packLine2);
			CommonConfirmDivot divot23 = deliveryConfirm2.GetDivot(packLine3);
			divot21.J8_PackagesDelivered = 5;
			divot22.J8_PackagesDelivered = 10;
			divot23.J8_PackagesDelivered = 15;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_DeliveryRequiredBy.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			ZDateTime now = ZDateTime.Now;

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = now.AddDays(2);
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = now.AddDays(2);
			shipment.DocsAndCartage.JP_EstimatedDelivery = now.AddDays(2);

			//complete all comfirns
			deliveryConfirm1.EU_PickupDeliveryTime = now;
			deliveryConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			deliveryConfirm2.EU_PickupDeliveryTime = now;
			deliveryConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(3);
			Factory.Save();
			AssertEquals(now.AddDays(2), shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(now.AddDays(2), shipment.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(now.AddDays(2), shipment.DocsAndCartage.JP_EstimatedDelivery);

			now = ZDateTime.Now.AddMinutes(2);

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_DeliveryRequiredBy = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Empty;

			//complete all comfirns
			deliveryConfirm1.EU_PickupDeliveryTime = now;
			deliveryConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			deliveryConfirm2.EU_PickupDeliveryTime = now;
			deliveryConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(3);
			Factory.Save();
			AssertEquals(now, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(now.AddHours(2), shipment.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(now.AddHours(3), shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestSetDeliveryCartageCompletedIfRequired_FCL()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			packLine2.SetContainer(consol, container2);
			packLine3.SetContainer(consol, container3);

			CommonPickupDeliveryConfirm deliveryConfirm1 = container1.DestinationConfirm;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			deliveryConfirm1.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			AssertEquals(deliveryConfirm1.EU_PickupDeliveryTime, deliveryConfirm1.Container.JC_ArrivalCartageComplete);
			CommonPickupDeliveryConfirm deliveryConfirm2 = container2.DestinationConfirm;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			deliveryConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			CommonPickupDeliveryConfirm deliveryConfirm3 = container3.DestinationConfirm;
			Assert(shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_DeliveryRequiredBy.IsEmpty);
			Assert(shipment.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			ZDateTime now = ZDateTime.Now;

			deliveryConfirm1.EU_PickupDeliveryTime = now;
			deliveryConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			deliveryConfirm2.EU_PickupDeliveryTime = now;
			deliveryConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(3);

			deliveryConfirm3.EU_PickupDeliveryTime = now;
			deliveryConfirm3.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm3.EU_PlannedPickupDeliveryTime = now.AddHours(3);
			AssertEquals(deliveryConfirm1.EU_PickupDeliveryTime, deliveryConfirm1.Container.JC_ArrivalCartageComplete);
			Factory.Save();
			AssertEquals(now, shipment.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(now.AddHours(2), shipment.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(now.AddHours(3), shipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestSetDeliveryCartageCompletedIfRequired_BCN()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			PackLine packLine11 = shipment1.OuterPackLines.AddNew();
			PackLine packLine12 = shipment1.OuterPackLines.AddNew();
			PackLine packLine13 = shipment1.OuterPackLines.AddNew();
			packLine11.SetContainer(consol, container1);
			packLine12.SetContainer(consol, container2);
			packLine13.SetContainer(consol, container3);

			CommonShipment shipment2 = consol.Shipments.AddNew();
			PackLine packLine21 = shipment2.OuterPackLines.AddNew();
			PackLine packLine22 = shipment2.OuterPackLines.AddNew();
			PackLine packLine23 = shipment2.OuterPackLines.AddNew();
			packLine21.SetContainer(consol, container1);
			packLine22.SetContainer(consol, container2);
			packLine23.SetContainer(consol, container3);

			CommonPickupDeliveryConfirm deliveryConfirm1 = container1.DestinationConfirm;
			Assert(shipment1.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			deliveryConfirm1.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment1.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			CommonPickupDeliveryConfirm deliveryConfirm2 = container2.DestinationConfirm;
			Assert(shipment1.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);

			deliveryConfirm2.EU_PickupDeliveryTime = ZDateTime.Now;
			Assert(shipment1.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			AssertEquals(deliveryConfirm2.EU_PickupDeliveryTime, deliveryConfirm2.Container.JC_ArrivalCartageComplete);

			CommonPickupDeliveryConfirm deliveryConfirm3 = container3.DestinationConfirm;
			Assert(shipment1.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty);
			Assert(shipment1.DocsAndCartage.JP_DeliveryRequiredBy.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_DeliveryRequiredBy.IsEmpty);
			Assert(shipment1.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);
			Assert(shipment2.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);

			ZDateTime now = ZDateTime.Now;
			//complete all confirms
			deliveryConfirm1.EU_PickupDeliveryTime = now;
			deliveryConfirm1.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm1.EU_PlannedPickupDeliveryTime = now.AddHours(5);

			deliveryConfirm2.EU_PickupDeliveryTime = now;
			deliveryConfirm2.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm2.EU_PlannedPickupDeliveryTime = now.AddHours(5);

			deliveryConfirm3.EU_PickupDeliveryTime = now;
			deliveryConfirm3.EU_RequestedPickupDeliveryTime = now.AddHours(2);
			deliveryConfirm3.EU_PlannedPickupDeliveryTime = now.AddHours(5);
			AssertEquals(deliveryConfirm3.EU_PickupDeliveryTime, deliveryConfirm3.Container.JC_ArrivalCartageComplete);
			Factory.Save();
			AssertEquals(now, shipment1.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(now, shipment2.DocsAndCartage.JP_DeliveryCartageCompleted);
			AssertEquals(now.AddHours(2), shipment1.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(now.AddHours(2), shipment2.DocsAndCartage.JP_DeliveryRequiredBy);
			AssertEquals(now.AddHours(5), shipment1.DocsAndCartage.JP_EstimatedDelivery);
			AssertEquals(now.AddHours(5), shipment2.DocsAndCartage.JP_EstimatedDelivery);
		}

		#endregion

		#region Test UniqueID

		public void TestUniqueID()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();

			confirm.UniqueID = "A";
			AssertEquals("A", confirm.UniqueID);
			AssertEquals((ZShort)1, confirm.EU_GatePassCount);

			confirm.UniqueID = "C";
			AssertEquals("C", confirm.UniqueID);
			AssertEquals((ZShort)3, confirm.EU_GatePassCount);

			confirm.UniqueID = "AC";
			AssertEquals("AC", confirm.UniqueID);
			AssertEquals((ZShort)29, confirm.EU_GatePassCount);

			confirm.UniqueID = "BD";
			AssertEquals("BD", confirm.UniqueID);
			AssertEquals((ZShort)56, confirm.EU_GatePassCount);

			confirm.UniqueID = "DZ";
			AssertEquals("DZ", confirm.UniqueID);
			AssertEquals((ZShort)130, confirm.EU_GatePassCount);

			confirm.EU_GatePassCount = byte.MaxValue;
			AssertEquals("IU", confirm.UniqueID);

			confirm.UniqueID = "ZZ";
			AssertEquals("ZZ", confirm.UniqueID);
			AssertEquals((ZShort)702, confirm.EU_GatePassCount);

			confirm.UniqueID = "ZZZ";
			AssertEquals("ZZZ", confirm.UniqueID);
			AssertEquals((ZShort)18278, confirm.EU_GatePassCount);
		}

		public void TestUniqueID_Saving()
		{
			var confirm1 = Factory.New<CommonPickupDeliveryConfirm>();
			confirm1.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 26);
			Factory.Save();
			AssertEquals("", confirm1.UniqueID);

			var shipment = Factory.New<CommonShipment>();
			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 5;
			var confirm2 = shipment.DeliveryConfirms.AddNew();
			confirm2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 27);

			Factory.Save();
			AssertEquals("A", confirm2.UniqueID);

			var confirm3 = shipment.DeliveryConfirms.AddNew();
			confirm3.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 27);

			Factory.Save();
			AssertEquals("A", confirm2.UniqueID);
			AssertEquals("B", confirm3.UniqueID);

			var confirm4 = shipment.PickupConfirms.AddNew();
			confirm4.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 28);

			Factory.Save();
			AssertEquals("A", confirm2.UniqueID);
			AssertEquals("B", confirm3.UniqueID);
			AssertEquals("C", confirm4.UniqueID);

			var confirm5 = shipment.OriginCFSArrivals.AddNew();
			confirm5.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 29);

			Factory.Save();
			AssertEquals("A", confirm2.UniqueID);
			AssertEquals("B", confirm3.UniqueID);
			AssertEquals("C", confirm4.UniqueID);
			AssertEquals("D", confirm5.UniqueID);

			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var confirm6 = container1.OriginConfirm;
			confirm6.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 29);

			Factory.Save();
			AssertEquals("A", confirm6.UniqueID);

			var confirm7 = container2.DestinationConfirm;
			confirm7.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 30);
			Factory.Save();
			AssertEquals("A", confirm6.UniqueID);
			AssertEquals("B", confirm7.UniqueID);

			var confirm8 = container2.DestinationCFSDeparture;
			confirm8.EU_PickupDeliveryTime = new ZDateTime(2016, 1, 1);
			Factory.Save();
			AssertEquals("A", confirm6.UniqueID);
			AssertEquals("B", confirm7.UniqueID);
			AssertEquals("C", confirm8.UniqueID);
		}

		#endregion

		#region TestFullGatePass

		public void TestFullGatePass()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S01000010";
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_TranshipToOtherCFS = true;

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			shipment.OuterPackLines.AddNew();

			var delivery1 = shipment.DestinationCFSDepartures.AddNew();
			delivery1.SetUniqueID();
			AssertEquals((ZShort)1, delivery1.EU_GatePassCount);
			AssertEquals("Gate Pass Number", "S01000010/A", delivery1.FullGatePass);

			var delivery2 = shipment.DestinationCFSDepartures.AddNew();
			delivery2.EU_GatePassCount = 128;
			AssertEquals("Gate Pass Number", "S01000010/DX", delivery2.FullGatePass);

			var delivery3 = shipment.DestinationCFSDepartures.AddNew();
			delivery3.EU_GatePassCount = 256;
			AssertEquals("Gate Pass Number", "S01000010/IV", delivery3.FullGatePass);

			var delivery4 = shipment.DestinationCFSDepartures.AddNew();
			delivery4.UniqueID = "ZZ";
			AssertEquals((ZShort)702, delivery4.EU_GatePassCount);
			AssertEquals("Gate Pass Number", "S01000010/ZZ", delivery4.FullGatePass);
		}

		#endregion

		#region Shipment

		public void TestFirstShipment()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			AssertEquals(shipment, confirm.FirstShipment);

			CommonShipment shipment1 = Factory.New<CommonShipment>();
			confirm.FirstShipment = shipment1;
			AssertEquals("Shipment should be unchanged because assigned shipment is not related", shipment, confirm.FirstShipment);
		}

		public void TestDocumentaryAddresses()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			AssertEquals(shipment.ConsigneeDocumentaryAddress, confirm.ConsigneeDocAddress);
			AssertEquals(shipment.ConsignorDocumentaryAddress, confirm.ConsignorDocAddress);
		}

		#endregion

		#region Test ConfirmAddress

		public void TestConfirmAddress()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine line = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();

			AssertEquals(shipment, confirm.Shipments[0]);
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;

			JobDocAddress shipmentAddress = shipment.DocAddresses.FindOrCreateWithRequirement(shipment.ConsignorPickupDeliveryAddressRequirement);
			shipmentAddress.E2_Contact = "contact1";

			AssertEquals(true, confirm.ConfirmAddress.IsTheSameDocAddressAndContactAs(shipmentAddress));

			Factory.Save();

			Assert(confirm.ConfirmAddress.IsInDatabase);

			confirm.ConfirmAddressOverride = true;
			confirm.ConfirmAddress.E2_Contact = "contact2";

			AssertEquals(false, confirm.ConfirmAddress.IsTheSameDocAddressAndContactAs(shipmentAddress));
		}

		public void TestConfirmAddress_NoParent()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;
			AssertNull(confirm.DeliverTo);

			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;
			AssertNull(confirm.PickupFrom);
		}

		#endregion

		#region ExistingConfirmAddressFallbackToParent

		public void TestConfirmAddressForExport_Pickup()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";
			header.MainAddress.OA_City = "Sydney";

			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			shipment.ConsignorPickupAddress.E2_OA_Address = header.MainAddress.PK;

			var confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;

			var pickupAddress = confirm.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			AssertNull(pickupAddress);

			AssertNotNull(confirm.ExistingConfirmAddressFallbackToParent);
			AssertEquals("Sydney", confirm.ExistingConfirmAddressFallbackToParent.City);
			Assert(confirm.IsConfirmAddressSameAsParentAddress);

			var newDocAddress = confirm.DocAddresses.AddNew();
			newDocAddress.DocAddressType = DocAddressType.ConsignorPickupDeliveryAddress;
			newDocAddress.City = "Melbourne";

			pickupAddress = confirm.DocAddresses.FindByDocAddressType(DocAddressType.ConsignorPickupDeliveryAddress);
			AssertNotNull(pickupAddress);

			AssertEquals(newDocAddress, confirm.ExistingConfirmAddressFallbackToParent);
			Assert(!confirm.IsConfirmAddressSameAsParentAddress);
		}

		public void TestConfirmAddressForExport_Delivery()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "CRMORG";
			header.MainAddress.OA_City = "Sydney";

			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = header.MainAddress.PK;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;

			var deliveryAddress = confirm.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			AssertNull(deliveryAddress);

			AssertNotNull(confirm.ExistingConfirmAddressFallbackToParent);
			AssertEquals("Sydney", confirm.ExistingConfirmAddressFallbackToParent.City);
			Assert(confirm.IsConfirmAddressSameAsParentAddress);

			var newDocAddress = confirm.DocAddresses.AddNew();
			newDocAddress.DocAddressType = DocAddressType.ConsigneePickupDeliveryAddress;
			newDocAddress.City = "Melbourne";

			deliveryAddress = confirm.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			AssertNotNull(deliveryAddress);

			AssertEquals(newDocAddress, confirm.ExistingConfirmAddressFallbackToParent);
			Assert(!confirm.IsConfirmAddressSameAsParentAddress);
		}

		#endregion

		#region TotalDelivered

		public void TestTotalDelivered()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HouseBill = "FindMe";
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 100;
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Pounds;
			packLine1.JL_ActualVolume = 200m;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 100;
			packLine2.JL_ActualWeight = 100m;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine2.JL_ActualVolume = 200m;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			CommonPickupDeliveryConfirm pod = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot1 = pod.GetDivot(packLine1);
			CommonConfirmDivot divot2 = pod.GetDivot(packLine2);
			AssertNotNull("Packline1 should have a divot", divot1);
			AssertNotNull("Packline2 should have a divot", divot2);

			AssertEquals("Default - Packs Delivered", 200, pod.TotalDeliveredPackages);
			AssertEquals("Default - Delivery Weight in Shipment Units (pounds)", 320.462m, Utilities.Round(pod.TotalDeliveredWeight, 3));
			AssertEquals("Default - Delivery Volume in Shipment Units (cf)", 7262.933m, Utilities.Round(pod.TotalDeliveredVolume, 3));
			AssertEquals("Default - Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
			AssertEquals("Default - Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
			AssertEquals("Default - Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
			AssertEquals("Default - Divot 2 - Packs Delivered", 100, divot2.J8_PackagesDelivered);
			AssertEquals("Default - Divot 2 - Delivery Weight", 100m, divot2.J8_DeliveryWeight);
			AssertEquals("Default - Divot 2 - Delivery Volume", 200m, divot2.J8_DeliveryVolume);

			pod.TotalDeliveredPackages = 210;
			AssertEquals("Packs Delivered", 210, pod.TotalDeliveredPackages);
			AssertEquals("Delivery Weight in Shipment Units (pounds)", 342.508m, Utilities.Round(pod.TotalDeliveredWeight, 3));
			AssertEquals("Delivery Volume in Shipment Units (cf)", 7969.227m, Utilities.Round(pod.TotalDeliveredVolume, 3));
			AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
			AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
			AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
			AssertEquals("Divot 2 - Packs Delivered", 110, divot2.J8_PackagesDelivered);
			AssertEquals("Divot 2 - Delivery Weight", 110m, divot2.J8_DeliveryWeight);
			AssertEquals("Divot 2 - Delivery Volume", 220m, divot2.J8_DeliveryVolume);

			pod.TotalDeliveredWeight = 350m;
			AssertEquals("Packs Delivered", 210, pod.TotalDeliveredPackages);
			AssertEquals("Delivery Weight in Shipment Units (pounds)", 350m, Utilities.Round(pod.TotalDeliveredWeight, 3));
			AssertEquals("Delivery Volume in Shipment Units (cf)", 7969.227m, Utilities.Round(pod.TotalDeliveredVolume, 3));
			AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
			AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
			AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
			AssertEquals("Divot 2 - Packs Delivered", 110, divot2.J8_PackagesDelivered);
			AssertEquals("Divot 2 - Delivery Weight", 113.398m, Utilities.Round(divot2.J8_DeliveryWeight, 3));
			AssertEquals("Divot 2 - Delivery Volume", 220m, divot2.J8_DeliveryVolume);

			pod.TotalDeliveredVolume = 8000m;
			AssertEquals("Packs Delivered", 210, pod.TotalDeliveredPackages);
			AssertEquals("Delivery Weight in Shipment Units (pounds)", 350m, Utilities.Round(pod.TotalDeliveredWeight, 3));
			AssertEquals("Delivery Volume in Shipment Units (cf)", 7999.986m, Utilities.Round(pod.TotalDeliveredVolume, 3));
			AssertEquals("Divot 1 - Packs Delivered", 100, divot1.J8_PackagesDelivered);
			AssertEquals("Divot 1 - Delivery Weight", 100m, divot1.J8_DeliveryWeight);
			AssertEquals("Divot 1 - Delivery Volume", 200m, divot1.J8_DeliveryVolume);
			AssertEquals("Divot 2 - Packs Delivered", 110, divot2.J8_PackagesDelivered);
			AssertEquals("Divot 2 - Delivery Weight", 113.398m, Utilities.Round(divot2.J8_DeliveryWeight, 3));
			AssertEquals("Divot 2 - Delivery Volume", 220.871m, Utilities.Round(divot2.J8_DeliveryVolume, 3));
		}

		#region DatesKind_Unspecified

		public void TestDates_DateTimeKind_Unspecified()
		{
			var shipment = Factory.New<CommonShipment>();
			var confirm = shipment.DeliveryConfirms.AddNew();

			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, confirm.EU_PlannedPickupDeliveryTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, confirm.EU_RequestedPickupDeliveryTime.Kind);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, confirm.EU_PickupDeliveryTime.Kind);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestPackagesDeliveredWithMinusValue()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 20;

			var deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			var divot11 = deliveryConfirm.GetDivot(packLine1);
			var divot12 = deliveryConfirm.GetDivot(packLine2);
			divot11.J8_PackagesDelivered = 5;
			divot12.J8_PackagesDelivered = 25;

			deliveryConfirm = shipment.DeliveryConfirms.AddNew();
		}

		#endregion

		#region IRelatedJobNumber Members

		public void TestIRelatedJobNumberMembers()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "blah";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			PackLine packLine = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();
			CommonConfirmDivot divot = confirm.GetDivot(packLine);
			string[] jobNumbers = ((IRelatedJobNumber)confirm).JobNumber;
			AssertEquals("Number of elements in JobNumber", 1, jobNumbers.Length);
			AssertEquals("JobNumber must be equal Shipment.JS_UniqueConsignRef", shipment.JS_UniqueConsignRef, jobNumbers[0]);
		}

		#endregion

		#region ICreditControlledDocumentDelivery

		public void TestICreditControlledDocumentDelivery()
		{
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			ZQuery consigneeFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
			consigneeFilter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignor.PK);
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(consigneeFilter);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_UniqueConsignRef = "blah";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.PickupConfirms.AddNew();
			CommonConfirmDivot divot = confirm.GetDivot(packLine);

			ICreditControlledDocumentDelivery creditControlled = confirm;

			AssertEquals(2, creditControlled.OrganisationsForCreditChecks.Length);
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignee));
			Assert(creditControlled.OrganisationsForCreditChecks.Contains(consignor));

			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsignorPK = ZGuid.Empty;

			AssertEquals(0, creditControlled.OrganisationsForCreditChecks.Length);

			AssertContains("Consignee or Consignor", creditControlled.DescriptionOfOrganisationBeingCheckedForCredit);
		}

		#endregion

		#region TestShouldBeGatePassed

		public void TestShouldBeGatePassed()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			Assert(!pickupConfirm.IsGatePass);

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			Assert(!deliveryConfirm.IsGatePass);

			CommonPickupDeliveryConfirm destinationDepartureConfirm = shipment.DestinationCFSDepartures.AddNew();
			Assert(!destinationDepartureConfirm.IsGatePass);

			shipment.JS_IsCFSRegistered = true;
			Assert(!destinationDepartureConfirm.IsGatePass);

			shipment.JS_TranshipToOtherCFS = true;
			Assert(destinationDepartureConfirm.IsGatePass);
			Assert(!deliveryConfirm.IsGatePass);
			Assert(!container.DestinationCFSDeparture.IsGatePass);
		}

		#endregion

		#region TestDistanceCalculation

		public void TestConfirmIsIDistanceConsumer()
		{
			AssertEquals(true, Factory.New<CommonPickupDeliveryConfirm>() is IDistanceCalculationConsumer);
		}

		public void TestIDistanceCalculationConsumer_Checkpoint()
		{
			AssertEquals(Env.Security.RoadDistanceCalculationServiceForwarding, ((IDistanceCalculationConsumer)Factory.New<CommonPickupDeliveryConfirm>()).Checkpoint);
		}

		public void TestSetCalculatedDistance()
		{
			OrgAddress packAddress = Factory.New<OrgAddress>();
			packAddress.OA_City = "Pack";
			packAddress.OA_Address1 = "Address";

			OrgAddress unpackAddress = Factory.New<OrgAddress>();
			unpackAddress.OA_City = "Unpack";
			unpackAddress.OA_Address1 = "Address";

			OrgAddress consignorPickupAddress = Factory.New<OrgAddress>();
			consignorPickupAddress.OA_City = "Consignor";
			consignorPickupAddress.OA_Address1 = "Address";

			OrgAddress consigneeDeliveryAddress = Factory.New<OrgAddress>();
			consigneeDeliveryAddress.OA_City = "Consignee";
			consigneeDeliveryAddress.OA_Address1 = "Address";

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_PackDepotAddress = packAddress.PK;
			consol.JK_OA_UnpackDepotAddress = unpackAddress.PK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.PK;

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			pickupConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;

			NotificationBuffer notifications = new NotificationBuffer();
			FreightDistanceCalculator calculator = new FreightDistanceCalculator(pickupConfirm, notifications);
			pickupConfirm.EU_DistanceUnit = Constants.Length.Miles;
			calculator.SetCalculatedDistance();
			AssertEquals("PickupConfirm distance - from PickUpFrom to ConfirmAddress", new ZDecimal("PackAddressAustraliaConsignorAddressAustralia".Length), pickupConfirm.EU_Distance);
			AssertEquals(Constants.Length.Miles, pickupConfirm.EU_DistanceUnit);

			pickupConfirm.EU_DistanceUnit = Constants.Length.Kilometres;
			calculator.SetCalculatedDistance();
			AssertEquals("PickupConfirm distance - from ConfirmAddress to DeliverTo", new ZDecimal("ConsignorAddressAustraliaPackAddressAustralia".Length), pickupConfirm.EU_Distance);
			AssertEquals(Constants.Length.Kilometres, pickupConfirm.EU_DistanceUnit);

			deliveryConfirm.EU_DistanceUnit = Constants.Length.Kilometres;
			calculator = new FreightDistanceCalculator(deliveryConfirm, notifications);
			calculator.SetCalculatedDistance();
			AssertEquals("DeliveryConfirm distance from PickUpFrom to ConfirmAddress", new ZDecimal("ConsigneeAddressAustraliaUnpackAddressAustralia".Length), deliveryConfirm.EU_Distance);
			AssertEquals(Constants.Length.Kilometres, deliveryConfirm.EU_DistanceUnit);
		}

		public void TestSetupDistanceCalculationConfiguration()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(consignor, "CNS", "10", "AAA");

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(consignee, "CNE", "20", "BBB");

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			pickupConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.OriginPickup;

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;

			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)pickupConfirm).DistanceCalculationConfig, consignor);
			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)deliveryConfirm).DistanceCalculationConfig, consignee);

			OrgHeader anotherConsignor = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(anotherConsignor, "ABC", "30", "CCC");

			OrgHeader anotherConsignee = Factory.NewWithValidTestData<OrgHeader>();
			SetupClientDistanceConfig(anotherConsignee, "XYZ", "40", "DDD");

			pickupConfirm.ConfirmAddressOverride = true;
			pickupConfirm.ConfirmAddress.E2_OA_Address = anotherConsignor.MainAddress.PK;
			deliveryConfirm.ConfirmAddressOverride = true;
			deliveryConfirm.ConfirmAddress.E2_OA_Address = anotherConsignee.MainAddress.PK;

			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)pickupConfirm).DistanceCalculationConfig, anotherConsignor);
			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)deliveryConfirm).DistanceCalculationConfig, anotherConsignee);

			pickupConfirm.ConfirmAddress.E2_AddressOverride = true;
			deliveryConfirm.ConfirmAddress.E2_AddressOverride = true;

			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)pickupConfirm).DistanceCalculationConfig, consignor);
			AssertDistanceConfigIsFromClient(((IDistanceCalculationConsumer)deliveryConfirm).DistanceCalculationConfig, consignee);
		}

		void SetupClientDistanceConfig(OrgHeader client, string provider, string version, string method)
		{
			client.MiscServ.OM_CMDistanceCalculationProvider = provider;
			client.MiscServ.OM_CMDistanceCalculationVersion = version;
			client.MiscServ.OM_CMDistanceCalculationMethod = method;
		}

		void AssertDistanceConfigIsFromClient(DistanceCalculationConfiguration config, OrgHeader client)
		{
			AssertEquals("Provider", client.MiscServ.OM_CMDistanceCalculationProvider, config.ProviderCode);
			AssertEquals("Version", client.MiscServ.OM_CMDistanceCalculationVersion, config.ProviderVersion);
			AssertEquals("Method", client.MiscServ.OM_CMDistanceCalculationMethod, config.CalculationMethod);
		}

		public void TestPostcodeDistance()
		{
			OrgAddress packAddress = Factory.New<OrgAddress>();
			packAddress.OA_City = "Pack";
			packAddress.OA_PostCode = "2000";
			packAddress.OA_Address1 = "Address";
			packAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress unpackAddress = Factory.New<OrgAddress>();
			unpackAddress.OA_City = "Unpack";
			unpackAddress.OA_PostCode = "2010";
			unpackAddress.OA_Address1 = "Address";
			unpackAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress consignorPickupAddress = Factory.New<OrgAddress>();
			consignorPickupAddress.OA_City = "Consignor";
			consignorPickupAddress.OA_PostCode = "2020";
			consignorPickupAddress.OA_Address1 = "Address";
			consignorPickupAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			OrgAddress consigneeDeliveryAddress = Factory.New<OrgAddress>();
			consigneeDeliveryAddress.OA_City = "Consignee";
			consigneeDeliveryAddress.OA_PostCode = "2030";
			consigneeDeliveryAddress.OA_Address1 = "Address";
			consigneeDeliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_OA_PackDepotAddress = packAddress.PK;
			consol.JK_OA_UnpackDepotAddress = unpackAddress.PK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.PK;

			CommonPickupDeliveryConfirm pickupConfirm = shipment.PickupConfirms.AddNew();
			AssertEquals(10.763m, pickupConfirm.PostcodeDistance);

			pickupConfirm.EU_DistanceUnit = Enterprise.Core.Constants.Length.Miles;
			AssertEquals(6.688m, pickupConfirm.PostcodeDistance);

			CommonPickupDeliveryConfirm deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			AssertEquals(5.424m, deliveryConfirm.PostcodeDistance);

			deliveryConfirm.EU_DistanceUnit = Enterprise.Core.Constants.Length.Miles;
			AssertEquals(3.370m, deliveryConfirm.PostcodeDistance);
		}

		#endregion

		#region EU_OA_TransportProvider

		public void TestEU_OA_TransportProvider()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			Assert(confirm.EU_OA_TransportProvider.IsEmpty);
			Assert(!confirm.EU_OA_TransportProviderInfo.ReadOnly);

			OrgHeader transportProvider = Factory.New<OrgHeader>();
			transportProvider.MainAddress.OA_City = "confirm";
			transportProvider.MainAddress.OA_Address1 = "confirm1";

			confirm.EU_OA_TransportProvider = transportProvider.MainAddress.PK;
			AssertEquals(transportProvider.MainAddress.PK, confirm.EU_OA_TransportProvider);
			Assert(!confirm.EU_OA_TransportProviderInfo.ReadOnly);

			CommonConsolidatedTransportBooking booking = Factory.New<CommonConsolidatedTransportBooking>();
			booking.Confirms.Add(confirm);
			Assert(confirm.EU_OA_TransportProvider.IsEmpty);
			Assert(confirm.EU_OA_TransportProviderInfo.ReadOnly);

			OrgHeader bookingTransportProvider = Factory.New<OrgHeader>();
			bookingTransportProvider.MainAddress.OA_City = "booking";
			bookingTransportProvider.MainAddress.OA_Address1 = "booking1";
			booking.D1_OA_TransportCo = bookingTransportProvider.MainAddress.PK;
			AssertEquals(bookingTransportProvider.MainAddress.PK, confirm.EU_OA_TransportProvider);
			Assert(confirm.EU_OA_TransportProviderInfo.ReadOnly);

			booking.Confirms.Relationship.RemoveFromRelationship(confirm);
			Assert(confirm.EU_OA_TransportProvider.IsEmpty);
			Assert(!confirm.EU_OA_TransportProviderInfo.ReadOnly);
		}

		#endregion

		#region IsSavedByFactory

		public void TestIsSavedByFactory()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.DeliveryConfirms.AddNew();

			Assert(!confirm.IsSavedByFactory);

			confirm.EU_DropMode = "ABC";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_DropMode = "";
			confirm.EU_RequestedPickupDeliveryTime = new ZDateTime(2017, 2, 16);
			Assert(confirm.IsSavedByFactory);

			confirm.EU_RequestedPickupDeliveryTime = ZDateTime.Empty;
			confirm.EU_PlannedPickupDeliveryTime = new ZDateTime(2017, 2, 16);
			Assert(confirm.IsSavedByFactory);

			confirm.EU_PlannedPickupDeliveryTime = ZDateTime.Empty;
			confirm.EU_PickupDeliveryTime = new ZDateTime(2017, 2, 16);
			Assert(confirm.IsSavedByFactory);

			confirm.EU_PickupDeliveryTime = ZDateTime.Empty;
			confirm.EU_GoodsSignForBy = "Miss Green";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_GoodsSignForBy = "";
			confirm.EU_TransportCoName = "James";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_TransportCoName = "";
			confirm.EU_DriversLicence = "CNNJG000156444";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_DriversLicence = "";
			confirm.EU_DriversName = "Dong";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_DriversName = "";
			confirm.EU_VehicleRegistration = "SEE";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_VehicleRegistration = "";
			confirm.EU_PickupDeliveryInstruction = "TODO";
			Assert(confirm.IsSavedByFactory);

			confirm.EU_PickupDeliveryInstruction = "";
			confirm.EU_Distance = 500.00m;
			Assert(confirm.IsSavedByFactory);

			var transportProvider = Factory.New<OrgHeader>();

			confirm.EU_Distance = 0m;
			confirm.EU_OA_TransportProvider = transportProvider.PK;
			Assert(confirm.IsSavedByFactory);

			confirm.EU_OA_TransportProvider = ZGuid.Empty;
			confirm.Divots[0].J8_PackagesDelivered = 15;
			Assert(confirm.IsSavedByFactory);

			confirm.Divots[0].J8_PackagesDelivered = 0;
			confirm.Divots[0].J8_DeliveryWeight = 20.15m;
			Assert(confirm.IsSavedByFactory);

			confirm.Divots[0].J8_DeliveryWeight = 0m;
			confirm.Divots[0].J8_DeliveryVolume = 16.25m;
			Assert(confirm.IsSavedByFactory);
		}

		#endregion

		#region TestDebugLog

		//WI00743739 - Remove logging causing performance issues
//		public void TestDebugLog()
//		{
//			var shipment = Factory.New<CommonShipment>();
//			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
//			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

//			var pack1 = shipment.OuterPackLines.AddNew();
//			var pickupConfirm = shipment.PickupConfirms.AddNew();
//			pickupConfirm.EU_PickupDeliveryTime = ZDateTime.Today;

//			var pack2 = shipment.OuterPackLines.AddNew();

//			AssertContains(FormattableString.Invariant($@"CreateDivot - Divot created: Divot PK: '{pickupConfirm.Divots[0].PK}',
//J8_JL: '{pack1.PK}',
//J8_EU_PickupDeliverConfirm: '{pickupConfirm.PK}',
//J8_PackagesDelivered: '0',
//J8_DeliveryWeight: '0',
//J8_DeliveryVolume: '0',
//IsInDatabase: '{pickupConfirm.Divots[0].IsInDatabase}',
//J8_SystemCreateTimeUtc: '',
//J8_SystemCreateUser: ''
//PackLine: IsInDatabase: 'False', JL_SystemCreateTimeUtc: '', JL_SystemCreateUser: '', JL_SystemLastEditTimeUtc: '', JL_SystemLastEditUser: ''"), pickupConfirm.DebugLog.ToString());
//			AssertContains(FormattableString.Invariant($@"OnAddIntoRelationshipCore - Skipping creating divot for Confirm: '{pickupConfirm.PK}', pickupDeliveryType: '', pickupDelivery.EU_JS: '{shipment.PK}', shipment: null
//Adding PackLine '{pack2.PK}' to existing Confirm: '{pickupConfirm.PK}'") +
//FormattableString.Invariant($@"

//CreateDivot - Divot created: Divot PK: '{pickupConfirm.Divots[1].PK}',
//J8_JL: '{pack2.PK}',
//J8_EU_PickupDeliverConfirm: '{pickupConfirm.PK}',
//J8_PackagesDelivered: '0',
//J8_DeliveryWeight: '0',
//J8_DeliveryVolume: '0',
//IsInDatabase: '{pickupConfirm.Divots[0].IsInDatabase}'"),
//pickupConfirm.DebugLog.ToString());
//		}

		#endregion

		#region TestDebugLogMessage

		//WI00743739 - Remove logging causing performance issues
//		[TestDate(2021, 1, 1)]
//		public void TestDebugLogMessage()
//		{
//			var shipment = Factory.New<CommonShipment>();
//			var pack1 = shipment.OuterPackLines.AddNew();
//			var pickupConfirm = shipment.PickupConfirms.AddNew();

//			AssertContains(FormattableString.Invariant($@"OnAddIntoRelationshipCore - Packline: '{pack1.PK}'. Confirm is already in Database: '{pickupConfirm.IsInDatabase}'
//CreateDivot - Divot created: Divot PK: '{pickupConfirm.Divots[0].PK}',
//J8_JL: '{pickupConfirm.Divots[0].J8_JL}',
//J8_EU_PickupDeliverConfirm: '{pickupConfirm.PK}',
//J8_PackagesDelivered: '0',
//J8_DeliveryWeight: '0',
//J8_DeliveryVolume: '0',
//IsInDatabase: '{pickupConfirm.Divots[0].IsInDatabase}',
//J8_SystemCreateTimeUtc: '',
//J8_SystemCreateUser: ''
//PackLine: IsInDatabase: 'False', JL_SystemCreateTimeUtc: '', JL_SystemCreateUser: '', JL_SystemLastEditTimeUtc: '', JL_SystemLastEditUser: ''
//Confirm: IsInDatabase: 'False', EU_PickupDeliveryType: 'PCU', EU_JS: '{shipment.PK}', EU_D1: '{ZGuid.Empty}', EU_JC: '{ZGuid.Empty}'
//EU_SystemCreateTimeUtc: '', EU_SystemCreateUser: '', EU_SystemLastEditTimeUtc: '', EU_SystemLastEditUser: ''
//allocatePackages: 'True'"),
//CommonPickupDeliveryConfirmCollection.CommonPickupDeliveryConfirmRelationship.debugLogMessage);
//		}

//		[TestDate(2021, 1, 1)]
//		public void TestInitPivotsErrorReport()
//		{
//			var shipment = Factory.New<CommonShipment>();
//			var packline = shipment.OuterPackLines.AddNew();
//			var pickupConfirm = shipment.PickupConfirms.AddNew();
//			pickupConfirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 15);

//			Factory.Save();
//			Assert("Precondition: everything is going well", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
//			AssertEquals("Precondition: there is one divot", 1, pickupConfirm.Divots.Count());

//			var factory2 = new BusinessObjectFactory();
//			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
//			var divot2 = factory2.New<CommonConfirmDivot>();
//			divot2.J8_JL = packline.PK;
//			divot2.J8_EU_PickupDeliverConfirm = pickupConfirm.PK;
//			shipment2.PickupConfirms.AddNew();

//			AssertContains("the pivot that could not be added has debugging data", $@"Pivot Debug information:
//Duplicate divots (2):
//Divot PK: '{divot2.PK}',
//J8_JL: '{packline.PK}',
//J8_EU_PickupDeliverConfirm: '{pickupConfirm.PK}',
//J8_PackagesDelivered: '0',
//J8_DeliveryWeight: '0',
//J8_DeliveryVolume: '0',
//IsInDatabase: 'False',
//J8_SystemCreateTimeUtc: '',
//J8_SystemCreateUser: ''
//PackLine: IsInDatabase: 'True', JL_SystemCreateTimeUtc: '01-Jan-21 00:00:00', JL_SystemCreateUser: 'E', JL_SystemLastEditTimeUtc: '01-Jan-21 00:00:00', JL_SystemLastEditUser: 'E'
//Divot PK: '{pickupConfirm.Divots.ElementAt(0).PK}',
//J8_JL: '{packline.PK}',
//J8_EU_PickupDeliverConfirm: '{pickupConfirm.PK}',
//J8_PackagesDelivered: '0',
//J8_DeliveryWeight: '0.000',
//J8_DeliveryVolume: '0.000',
//IsInDatabase: 'True',
//J8_SystemCreateTimeUtc: '01-Jan-21 00:00:00',
//J8_SystemCreateUser: 'E'
//PackLine: IsInDatabase: 'True', JL_SystemCreateTimeUtc: '01-Jan-21 00:00:00', JL_SystemCreateUser: 'E', JL_SystemLastEditTimeUtc: '01-Jan-21 00:00:00', JL_SystemLastEditUser: 'E'

//Confirms of duplicate divots (1):
//IsInDatabase: 'True', EU_PickupDeliveryType: 'PCU', EU_JS: '{shipment.PK}', EU_D1: '{ZGuid.Empty}', EU_JC: '{ZGuid.Empty}'
//EU_SystemCreateTimeUtc: '01-Jan-21 00:00:00', EU_SystemCreateUser: 'E', EU_SystemLastEditTimeUtc: '01-Jan-21 00:00:00', EU_SystemLastEditUser: 'E'", ErrorReporter.LastMessageReported);
//			ErrorReporter.Clear();
//		}

		public void TestCreateDivot_NoDuplicatedCreation()
		{
			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var pickupConfirm = shipment.PickupConfirms.AddNew();
			pickupConfirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 15);

			Factory.Save();
			Assert("Precondition: everything is going well.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals("Precondition: there is one divot.", 1, pickupConfirm.Divots.Count);

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			var packline2 = (PackLine)shipment.OuterPackLines.FirstOrDefault();
			var pickupConfirm2 = shipment2.PickupConfirms.FirstOrDefault();

			AssertEquals("Precondition: same pack line exists in another factory.", packline.PK, packline2.PK);

			pickupConfirm2.CreateDivot(packline2);
			Assert("Everything is still going well (no debugging messages created).", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 10;

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_DropMode = "ABC";

			return confirm;
		}

		#endregion
	}
}
