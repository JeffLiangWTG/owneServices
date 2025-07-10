using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ContainerPenaltyCalculateHandlerForShipmentTest : TestCaseWithFactory
	{
		public void TestHandleContainerDateChanging_NotShipperOwned()
		{
			AssertHandleContainerDateChanging(false);
		}

		public void TestHandleContainerDateChanging_ShipperOwned()
		{
			AssertHandleContainerDateChanging(true);
		}

		public void AssertHandleContainerDateChanging(bool containerIsShipperOwned)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OrgFountains.DeleteAll();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			var penalty1 = consignee.ConsigneeContainerPenalties.AddNew();
			penalty1.PD_FreeDays = 1;
			penalty1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			penalty1.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			consignee.ConsigneeCTOStorages.AddNew().PD_FreeDays = 2;
			consignee.ConsigneeCTOStorages[0].PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;

			var penalty2 = consignor.ConsignorContainerPenalties.AddNew();
			penalty2.PD_FreeDays = 3;
			penalty2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			penalty2.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			consignor.ConsignorCTOStorages.AddNew().PD_FreeDays = 4;
			consignor.ConsignorCTOStorages[0].PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;

			var importCTO = Factory.NewWithValidTestData<OrgHeader>();
			importCTO.ServiceImportCTOStorages.AddNew().PD_FreeDays = 5;
			importCTO.ServiceImportCTOStorages[0].PD_OH_Client = consignee.PK;

			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();
			exportCTO.ServiceExportCTOStorages.AddNew().PD_FreeDays = 6;
			exportCTO.ServiceExportCTOStorages[0].PD_OH_Client = consignor.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var container = consol.Containers.AddNew();
			container.JC_IsShipperOwned = containerIsShipperOwned;
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			shipment.OuterPackLines.AddNew();

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
			AssertArrivalDetentionPenalty(1);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
			AssertEquals((ZByte)2, container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertArrivalDetentionPenalty(1);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
			AssertArrivalDetentionPenalty(1);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);
			AssertEquals((ZByte)2, container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertArrivalDetentionPenalty(1);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate);
			AssertEquals((ZByte)2, container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
			AssertEquals((ZByte)4, container.PickupPenalties.FindOrCreateDepartureCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)6, container.PickupPenalties.FindOrCreateDepartureCTOStoragePenalty(false).FreeTimeAsDays);
			AssertDepartureDetentionPenalty(3);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
			AssertDepartureDetentionPenalty(3);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
			AssertEquals((ZByte)4, container.PickupPenalties.FindOrCreateDepartureCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)6, container.PickupPenalties.FindOrCreateDepartureCTOStoragePenalty(false).FreeTimeAsDays);
			AssertDepartureDetentionPenalty(3);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
			AssertEquals((ZByte)2, container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertArrivalDetentionPenalty(1);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
			AssertNull(container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false));
			AssertNull(container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false));

			void AssertArrivalDetentionPenalty(int expectedFreeDaysWhenNotShipper)
			{
				if (container.JC_IsShipperOwned)
				{
					AssertNull(container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false));
					AssertEquals(0, shipment.DeliveryPenalties.Find(p => p.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention).Count());
				}
				else
				{
					AssertEquals((ZByte)expectedFreeDaysWhenNotShipper, container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);
					AssertNotNull(shipment.DeliveryPenalties.FindByPK(container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).PK));
				}
			}

			void AssertDepartureDetentionPenalty(int expectedFreeDaysWhenNotShipper)
			{
				if (container.JC_IsShipperOwned)
				{
					AssertNull(container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false));
					AssertEquals(0, shipment.PickupPenalties.Find(p => p.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention).Count());
				}
				else
				{
					AssertEquals((ZByte)expectedFreeDaysWhenNotShipper, container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);
					AssertNotNull(shipment.PickupPenalties.FindByPK(container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).PK));
				}
			}
		}

		public void TestHandleContainerDateChanging_MDD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OrgFountains.DeleteAll();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			var importMDDConsignee = consignee.ConsigneeContainerPenalties.AddNew();
			importMDDConsignee.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDDConsignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDDConsignee.PD_FreeDays = 7;

			var exportMDDConsignee = consignee.ConsigneeContainerPenalties.AddNew();
			exportMDDConsignee.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportMDDConsignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			exportMDDConsignee.PD_FreeDays = 8;

			var importMDDConsignor = consignor.ConsignorContainerPenalties.AddNew();
			importMDDConsignor.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDDConsignor.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDDConsignor.PD_FreeDays = 9;

			var exportMDDConsignor = consignor.ConsignorContainerPenalties.AddNew();
			exportMDDConsignor.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportMDDConsignor.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			exportMDDConsignor.PD_FreeDays = 10;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			shipment.OuterPackLines.AddNew();

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
			AssertEquals((ZByte)7, container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
			AssertEquals((ZByte)7, container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
			AssertEquals((ZByte)7, container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);
			AssertEquals((ZByte)7, container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate);
			AssertEquals((ZByte)7, container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
			AssertEquals((ZByte)10, container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
			AssertEquals((ZByte)10, container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
			AssertEquals((ZByte)10, container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false).FreeTimeAsDays);

			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
			AssertEquals((ZByte)7, container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
			AssertNull(container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false));
		}

		public void TestHandleContainerDeallocation_RefreshesShipmentPenalties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_FCLUnloadFromVessel = ZDateTime.Today;
			container.JC_FCLOnBoardVessel = ZDateTime.Today;
			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();
			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container);
			var deliveryPenalty1 = container.DeliveryPenalties.AddNew();
			deliveryPenalty1.CPY_JS_Shipment = shipment1.PK;
			var pickupPenalty1 = container.PickupPenalties.AddNew();
			pickupPenalty1.CPY_JS_Shipment = shipment1.PK;
			handler.HandleContainerAllocation(packLine1);
			AssertNotNull(shipment1.DeliveryPenalties.FindByPK(deliveryPenalty1.PK));
			AssertNotNull(shipment1.PickupPenalties.FindByPK(pickupPenalty1.PK));

			var shipment2 = consol.Shipments.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container);
			var deliveryPenalty2 = container.DeliveryPenalties.AddNew();
			deliveryPenalty2.CPY_JS_Shipment = shipment2.PK;
			var pickupPenalty2 = container.PickupPenalties.AddNew();
			pickupPenalty2.CPY_JS_Shipment = shipment2.PK;
			handler.HandleContainerAllocation(packLine2);
			AssertNotNull(shipment1.DeliveryPenalties.FindByPK(deliveryPenalty1.PK));
			AssertNotNull(shipment1.PickupPenalties.FindByPK(pickupPenalty1.PK));
			AssertNotNull(shipment2.DeliveryPenalties.FindByPK(deliveryPenalty2.PK));
			AssertNotNull(shipment2.PickupPenalties.FindByPK(pickupPenalty2.PK));

			packLine1.JL_JC = ZGuid.Empty;
			AssertNull(shipment1.DeliveryPenalties.FindByPK(deliveryPenalty1.PK));
			AssertNull(shipment1.PickupPenalties.FindByPK(pickupPenalty1.PK));
			AssertNotNull(shipment2.DeliveryPenalties.FindByPK(deliveryPenalty2.PK));
			AssertNotNull(shipment2.PickupPenalties.FindByPK(pickupPenalty2.PK));

			packLine2.JL_JC = ZGuid.Empty;
			AssertNull(shipment1.DeliveryPenalties.FindByPK(deliveryPenalty1.PK));
			AssertNull(shipment1.PickupPenalties.FindByPK(pickupPenalty1.PK));
			AssertNull(shipment2.DeliveryPenalties.FindByPK(deliveryPenalty2.PK));
			AssertNull(shipment2.PickupPenalties.FindByPK(pickupPenalty2.PK));
			AssertNull(container.DeliveryPenalties.FindByPK(deliveryPenalty1.PK));
			AssertNull(container.PickupPenalties.FindByPK(pickupPenalty1.PK));
			AssertNull(container.DeliveryPenalties.FindByPK(deliveryPenalty2.PK));
			AssertNull(container.PickupPenalties.FindByPK(pickupPenalty2.PK));
		}

		public void TestHandleContainerDeallocation_WithMultipleContainers_DoesNotDeletePreviousPenalties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			var container1 = consol.Containers.AddNew();
			packLine1.SetContainer(consol, container1);
			var penalty = shipment.DeliveryPenalties.AddNew();
			penalty.CPY_JC_Container = container1.PK;
			AssertNotNull(shipment.DeliveryPenalties.FindByPK(penalty.PK));
			AssertNotNull(container1.DeliveryPenalties.FindByPK(penalty.PK));

			var packLine2 = shipment.OuterPackLines.AddNew();
			var container2 = consol.Containers.AddNew();
			packLine2.SetContainer(consol, container2);
			AssertNotNull(shipment.DeliveryPenalties.FindByPK(penalty.PK));
			AssertNotNull(container1.DeliveryPenalties.FindByPK(penalty.PK));
		}

		public void TestHandleContainerDeallocation_NoExceptionWhenPackLineIsDeleted()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_FCLUnloadFromVessel = ZDateTime.Today;
			container.JC_FCLOnBoardVessel = ZDateTime.Today;
			container.DeliveryPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();

			var shipment1 = consol.Shipments.AddNew();
			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container);

			var deliveryPenalty1 = container.DeliveryPenalties.AddNew();
			deliveryPenalty1.CPY_JC_Container = container.PK;
			deliveryPenalty1.CPY_PenaltyType = "DET";

			var shipment2 = consol.Shipments.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container);

			var deliveryPenalty2 = container.DeliveryPenalties.AddNew();
			deliveryPenalty2.CPY_JC_Container = container.PK;
			deliveryPenalty2.CPY_PenaltyType = "DET";

			Factory.Save();

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			packLine2.Delete();

			AssertNoExceptionThrown(() => handler.HandleContainerDeallocation(packLine2));
		}

		public void TestCarrierAndCTOPenaltiesWithMultipleShipment()
		{
			// Arrange
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "DEBRE";
			consol.JK_RL_NKDischargePort = "USMIA";
			consol.JK_RL_NKLastForeignPort = "USMIA";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			void AddCarrierContainerPenalty(string ptype, byte days, string lftype)
			{
				var penalty = carrier.CarrierContainerPenalties.AddNew();
				penalty.PD_Direction = "EXP";
				penalty.PD_PenaltyType = ptype;
				penalty.PD_DetentionPortOrCountry = "DEBRE";
				penalty.PD_FreeDays = days;
				penalty.PD_LastFreeDayType = lftype;
			}

			AddCarrierContainerPenalty("DET", 8, "WGI");
			AddCarrierContainerPenalty(Core.Constants.ContainerDetentionPenaltyType.STO, 9, "FCL");

			var leg1 = consol.Transports.AddNew();
			leg1.JW_IsLinked = true;
			leg1.JW_LegOrder = 1;
			leg1.JW_TransportMode = "SEA";
			leg1.JW_RL_NKLoadPort = "DEBRE";
			leg1.JW_RL_NKDiscPort = "PAPTY";
			leg1.CarrierPK = carrier.PK;
			var leg2 = consol.Transports.AddNew();
			leg2.JW_IsLinked = false;
			leg2.JW_LegOrder = 2;
			leg2.JW_TransportMode = "SEA";
			leg2.JW_RL_NKLoadPort = "PAPTY";
			leg2.JW_RL_NKDiscPort = "USMIA";
			leg2.CarrierPK = carrier.PK;

			var container = consol.Containers.AddNew();

			CommonShipment AddShipment(string type)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "DEBRE";
				shipment.JS_RL_NKDestination = "USMIA";
				shipment.JS_PackingMode = "FCL";
				shipment.JS_ShipmentType = type;
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 10;
				packline.JL_F3_NKPackType = "PKG";
				packline.JL_ActualWeight = 800;
				packline.JL_ActualWeightUQ = "LB";
				packline.JL_ActualVolume = 18;
				packline.JL_ActualVolumeUQ = "M3";
				packline.JL_JC = container.PK;
				return shipment;
			}

			var masterShipment = AddShipment("ASM");
			var subShipment1 = AddShipment("STD");
			var subShipment2 = AddShipment("STD");
			subShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			subShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();
			var exportCTOStorage = exportCTO.ServiceExportCTOStorages.AddNew();
			exportCTOStorage.PD_FreeDays = 5;
			exportCTOStorage.PD_DetentionPortOrCountry = "DEBRE";
			exportCTOStorage.PD_FreeDayType = "1FC";
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;

			// Act
			container.JC_ContainerYardEmptyPickupGateOut = new ZDateTime(2023, 3, 17);
			container.JC_FCLWharfGateIn = new ZDateTime(2023, 3, 27);
			container.JC_FCLOnBoardVessel = new ZDateTime(2023, 4, 5);

			// Assert
			void AssertPenalty(CommonShipment shipment, string penaltyType, string creditorType, byte freeTime)
			{
				var penalty = shipment.PickupPenalties.FirstOrDefault(x => x.CPY_PenaltyType == penaltyType && x.CPY_CreditorType == creditorType);
				AssertNotNull(penalty);
				AssertEquals(freeTime, penalty.FreeTimeAsDays);
			}

			void AssertPenalties(CommonShipment shipment)
			{
				AssertEquals(3, shipment.PickupPenalties.Count);
				AssertPenalty(shipment, ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, 8);
				AssertPenalty(shipment, ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.Carrier, 9);
				AssertPenalty(shipment, ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO, 5);
			}

			AssertPenalties(subShipment1);
			AssertPenalties(subShipment2);
		}

		public void TestShipmentDetentionDuration()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OrgFountains.DeleteAll();

			var importDetention = carrier.CarrierContainerPenalties.AddNew();
			importDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			importDetention.PD_FreeDays = 5;
			importDetention.PD_FreeDayType = "CTD";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;
			var penalty1 = consignee.ConsigneeContainerPenalties.AddNew();
			penalty1.PD_FreeDays = 3;
			penalty1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			penalty1.PD_FreeDayType = "CTD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var container = consol.Containers.AddNew();

			container.JC_FCLAvailable = new ZDateTime(2021, 11, 16);
			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2021, 11, 16);
			container.JC_FCLWharfGateOut = new ZDateTime(2021, 11, 22);
			container.JC_EmptyReturnedBy = container.JC_FCLAvailable.AddDays(importDetention.PD_FreeDays - 1);
			container.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2021, 11, 29);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			var consolPenalty = container.ImportPenalties.FirstOrDefault(x => x.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention);
			AssertEquals((ZByte)5, consolPenalty.FreeTimeAsDays);
			AssertEquals((ZByte)9, consolPenalty.DurationAsDays);
			var shipmentPenalty = shipment.DeliveryPenalties.Cast<ShipmentContainerPenalty>().FirstOrDefault(x => x.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention);
			AssertEquals((ZByte)3, shipmentPenalty.FreeTimeAsDays);
			AssertEquals((ZByte)11, shipmentPenalty.DurationAsDays);
		}

		public void TestShipmentStorageDuration_IMP_CTO_STO_Override_OnlyAppliesToConsol()
		{
			var ctoAddressOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoAddressOrg.OH_Code = "TSTCAR";
			ctoAddressOrg.OH_IsShippingLine = true;
			ctoAddressOrg.CustomsCodes.AddNew("HID", "FWA", CountryCodes.UnitedStates);
			ctoAddressOrg.OrgFountains.DeleteAll();

			var importCTOStorage = ctoAddressOrg.ServiceImportCTOStorages.AddNew();
			importCTOStorage.PD_Direction = ContainerDetentionDirection.Import;
			importCTOStorage.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
			importCTOStorage.PD_FreeDays = 8;
			importCTOStorage.PD_DetentionPortOrCountry = "DEHAM";
			importCTOStorage.PD_FreeDayType = ContainerDetentionFreeDayType.VesselArrival;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;
			var consigneeCTOStorage = consignee.ConsigneeCTOStorages.AddNew();
			consigneeCTOStorage.PD_FreeDays = 6;
			consigneeCTOStorage.PD_DetentionPortOrCountry = "DEHAM";
			consigneeCTOStorage.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
			consigneeCTOStorage.PD_FreeDayType = ContainerDetentionFreeDayType.VesselArrival;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.Transports[0].JW_ETD = new ZDateTime(2024, 08, 02);
			consol.Transports[0].JW_ATD = new ZDateTime(2024, 08, 03);
			consol.Transports[0].JW_ETA = new ZDateTime(2024, 08, 16);
			consol.Transports[0].JW_ATA = new ZDateTime(2024, 08, 17);
			consol.Transports[0].JW_TerminalAvailabilityDate = new ZDateTime(2024, 08, 17);
			consol.Transports[0].JW_TerminalStorageDate = new ZDateTime(2024, 08, 17);
			consol.JK_OA_ArrivalCTOAddress = ctoAddressOrg.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "DEHAM";
			var container = consol.Containers.AddNew();

			container.JC_FCLWharfGateOut = new ZDateTime(2024, 08, 30);
			container.JC_EmptyReturnedBy = new ZDateTime(2024, 08, 26);
			container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2024, 08, 25);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			AssertEquals(new ZDateTime(2024, 08, 17), consol.Transports[0].JW_TerminalAvailabilityDate);
			AssertEquals(new ZDateTime(2024, 08, 17), consol.Transports[0].JW_TerminalStorageDate);
			var consolPenalty = container.ImportPenalties.Single(x => x.CPY_PenaltyType == "STO" && x.CPY_CreditorType == "CTO");
			AssertEquals((ZByte)8, consolPenalty.FreeTimeAsDays);
			AssertEquals((ZByte)6, consolPenalty.DurationAsDays);
			var shipmentPenalty = shipment.DeliveryPenalties.Cast<ShipmentContainerPenalty>().Single(x => x.CPY_PenaltyType == "STO" && x.CPY_CreditorType == "CTO");
			AssertEquals((ZByte)6, shipmentPenalty.FreeTimeAsDays);
			AssertEquals((ZByte)8, shipmentPenalty.DurationAsDays);
		}

		public void TestHandleContainerAdded()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OrgFountains.DeleteAll();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			var importDetention = consignee.CarrierContainerPenalties.AddNew();
			importDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			importDetention.PD_FreeDays = 1;

			consignee.ConsigneeCTOStorages.AddNew().PD_FreeDays = 2;
			consignee.ConsigneeCTOStorages[0].PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;

			var exportDetention = consignor.CarrierContainerPenalties.AddNew();
			exportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			exportDetention.PD_FreeDays = 3;

			consignor.ConsignorCTOStorages.AddNew().PD_FreeDays = 4;
			consignor.ConsignorCTOStorages[0].PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;

			var importCTO = Factory.NewWithValidTestData<OrgHeader>();
			importCTO.ServiceImportCTOStorages.AddNew().PD_FreeDays = 5;
			importCTO.ServiceImportCTOStorages[0].PD_OH_Client = consignee.PK;

			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();
			exportCTO.ServiceExportCTOStorages.AddNew().PD_FreeDays = 6;
			exportCTO.ServiceExportCTOStorages[0].PD_OH_Client = consignor.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var container1 = consol.Containers.AddNew();
			var handler1 = new ContainerPenaltyCalculateHandlerForShipment(container1);
			shipment.OuterPackLines.AddNew();
			handler1.HandleContainerAdded();
			AssertNull(container1.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false));
			AssertNull(container1.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false));

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			var handler2 = new ContainerPenaltyCalculateHandlerForShipment(container2);
			shipment.OuterPackLines.AddNew();
			handler2.HandleContainerAdded();
			AssertEquals((ZByte)2, container2.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container2.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
		}

		public void TestDefaultingWithDuplicatePenaltyType()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OrgFountains.DeleteAll();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			var importDetention = consignee.ConsigneeContainerPenalties.AddNew();
			importDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			importDetention.PD_FreeDays = 1;

			var exportDetention = consignor.ConsignorContainerPenalties.AddNew();
			exportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			exportDetention.PD_FreeDays = 3;

			var importMDD = consignee.ConsigneeContainerPenalties.AddNew();
			importMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDD.PD_FreeDays = 7;

			var exportMDD = consignor.ConsignorContainerPenalties.AddNew();
			exportMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			exportMDD.PD_FreeDays = 10;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			shipment.OuterPackLines.AddNew();

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			{
				container.DeliveryPenalties.DeleteAll();
				container.PickupPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
				AssertNull(container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				var deliveryDetentionPenalty = container.DeliveryPenalties.AddNew();
				deliveryDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				deliveryDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				deliveryDetentionPenalty.FreeTimeAsDays = 20;
				AssertNull(container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false));
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
				AssertNull(container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
				AssertNull(container.DeliveryPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)20, container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
			{
				container.DeliveryPenalties.DeleteAll();
				container.PickupPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
				AssertNull(container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);

				var deliveryDetentionPenalty = container.PickupPenalties.AddNew();
				deliveryDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				deliveryDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				deliveryDetentionPenalty.FreeTimeAsDays = 20;
				AssertNull(container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false));
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
				AssertNull(container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
				AssertNull(container.PickupPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)20, container.PickupPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
		}

		#region StopHandlingContainerDateChanging

		public void TestStopHandlingContainerDateChanging_IsImportingData()
		{
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			(container as ISupportDataImporting).IsImportingData = true;

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			Assert(handler.StopHandlingContainerDateChanging());

			handler = new ContainerPenaltyCalculateHandlerForShipment(null);
			Assert(handler.StopHandlingContainerDateChanging());
		}

		public void TestStopHandlingContainerDateChanging_ContainerParent()
		{
			var container = Factory.NewWithValidTestData<ForwardingContainer>();

			Assert(!container.SupportsContainerPenalties);
			Assert(container.ContainerParent == null);

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			Assert(handler.StopHandlingContainerDateChanging());
		}

		public void TestStopHandlingContainerDateChanging_SuspendedContainerPenalties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			using (container.SuspendContainerPenalties())
			{
				Assert(container.SuspendedContainerPenalties);

				var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
				Assert(handler.StopHandlingContainerDateChanging());
			}
		}

		public void TestStopHandlingContainerDateChanging_IsNotAllocated()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			Assert(!container.PackLines.Any());

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			Assert(handler.StopHandlingContainerDateChanging());
		}

		public void TestStopHandlingContainerDateChanging_TransportMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			shipment.OuterPackLines.AddNew();

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			Assert(handler.StopHandlingContainerDateChanging());
		}

		public void TestStopHandlingContainerDateChanging_False_ConsolIsNull()
		{
			var container = Factory.New<ForwardingContainer>();
			var declaration = Factory.New<IBaseJobDeclaration>();

			var cusContainer = (BusinessObject)Factory.New<Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = container.PK;

			var packLine = Factory.New<PackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_JS = Factory.New<CommonShipment>().PK;
			container.PackLines.Add(packLine);

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			AssertEquals(false, handler.StopHandlingContainerDateChanging());
		}

		public void TestStopHandlingContainerDateChanging_False_ConsolIsNotNull()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			shipment.OuterPackLines.AddNew();

			var handler = new ContainerPenaltyCalculateHandlerForShipment(container);
			AssertEquals(false, handler.StopHandlingContainerDateChanging());
		}

		#endregion

		#region Defaulting MDD Penalties

		public void TestDefaulting_ClientContractMDDAndConsigneeSTOPenalties()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 4 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "TSTCAR";
				carrier.OH_IsShippingLine = true;
				carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
				carrier.OrgFountains.DeleteAll();

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = "CONSIGNEE";
				consignee.MainAddress.OA_Address1 = "Consignee Address";
				consignee.OH_IsConsignee = true;

				var ratingContract1 = CreateNewContract("CLAG00000001", consignee.PK, "CLI");
				AddNewContractPenalty(Core.Constants.ContainerDetentionPenaltyType.MDD, "IMP", 12, ZString.Empty, ZString.Empty, ZString.Empty, ratingContract1.PK, carrier.PK);

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.MainAddress.OA_Address1 = "Consignor Address";
				consignor.OH_IsConsignor = true;

				var importSTOConsignee = consignee.ConsigneeContainerPenalties.AddNew();
				importSTOConsignee.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				importSTOConsignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
				importSTOConsignee.PD_FreeDays = 9;

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_E_DEP = new ZDateTime(2022, 10, 1);
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				var jobHeader = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
				jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				shipment.Job.JH_ClientContractNumber = "CLAG00000001";

				var container = consol.Containers.AddNew();
				container.JC_FCLAvailable = new ZDateTime(2022, 10, 1);
				shipment.OuterPackLines.AddNew();

				container.ImportPenalties.DeleteAll();
				shipment.DeliveryPenalties.DeleteAll();
				container.JC_FCLUnloadFromVessel = new ZDateTime(2022, 10, 12);
				CombineAssertions(() =>
				{
					var ctoSTOPenalty = shipment.DeliveryPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO && p.CPY_CreditorType == "CTO");
					AssertNotNull(ctoSTOPenalty);
					AssertEquals((ZByte)3, ctoSTOPenalty.FreeTimeAsDays);

					var carMDDPenalty = shipment.DeliveryPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.MDD && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNotNull(carMDDPenalty);
					AssertEquals((ZByte)12, carMDDPenalty.FreeTimeAsDays);
				});
			}
		}

		public void TestDefaulting_CarrierContractWithClientMDDAndConsigneeSTOPenalties()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 4 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "TSTCAR";
				carrier.OH_IsShippingLine = true;
				carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
				carrier.OrgFountains.DeleteAll();

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = "CONSIGNEE";
				consignee.MainAddress.OA_Address1 = "Consignee Address";
				consignee.OH_IsConsignee = true;

				var ratingContract1 = CreateNewContract("CN001", carrier.PK, "PRO");
				AddNewContractPenalty(Core.Constants.ContainerDetentionPenaltyType.MDD, "IMP", 15, ZString.Empty, ZString.Empty, ZString.Empty, ratingContract1.PK, consignee.PK);

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.MainAddress.OA_Address1 = "Consignor Address";
				consignor.OH_IsConsignor = true;

				var importSTOConsignee = consignee.ConsigneeContainerPenalties.AddNew();
				importSTOConsignee.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				importSTOConsignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
				importSTOConsignee.PD_FreeDays = 9;
				importSTOConsignee.PD_OH_Carrier = carrier.PK;

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_CarrierContractNumber = "CN001";
				consol.Transports.MostInterestingTransport.JW_ATD = new ZDateTime(2022, 9, 1);

				var shipment = consol.Shipments.AddNew();
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;

				var container = consol.Containers.AddNew();
				container.JC_FCLAvailable = new ZDateTime(2022, 10, 1);
				shipment.OuterPackLines.AddNew();

				Factory.Save();

				container.ImportPenalties.DeleteAll();
				shipment.DeliveryPenalties.DeleteAll();
				container.JC_FCLUnloadFromVessel = new ZDateTime(2022, 10, 12);
				CombineAssertions(() =>
				{
					AssertEquals((ZByte)3, container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
					AssertNull(container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false));
					AssertEquals((ZByte)15, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

					var carSTOPenalty = shipment.DeliveryPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNull(carSTOPenalty);

					var ctoSTOPenalty = shipment.DeliveryPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO && p.CPY_CreditorType == "CTO");
					AssertNotNull(ctoSTOPenalty);
					AssertEquals((ZByte)3, ctoSTOPenalty.FreeTimeAsDays);

					var carMDDPenalty = shipment.DeliveryPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.MDD && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNotNull(carMDDPenalty);
					AssertEquals((ZByte)15, carMDDPenalty.FreeTimeAsDays);
				});
			}
		}

		public void TestDefaulting_ClientContractMDDWithCarrierSTOPenalties()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 4 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "TSTCAR";
				carrier.OH_IsShippingLine = true;
				carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
				carrier.OrgFountains.DeleteAll();
				var carrierSTO = carrier.CarrierContainerPenalties.AddNew();
				carrierSTO.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				carrierSTO.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
				carrierSTO.PD_FreeDays = 9;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = "CONSIGNEE";
				consignee.MainAddress.OA_Address1 = "Consignee Address";
				consignee.OH_IsConsignee = true;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.MainAddress.OA_Address1 = "Consignor Address";
				consignor.OH_IsConsignor = true;

				var clientContract = CreateNewContract("CLAG00000001", consignor.PK, "CLI");
				AddNewContractPenalty(Core.Constants.ContainerDetentionPenaltyType.MDD, "EXP", 12, ZString.Empty, ZString.Empty, ZString.Empty, clientContract.PK, carrier.PK);

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_E_DEP = new ZDateTime(2022, 10, 1);
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				var jobHeader = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
				jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				shipment.Job.JH_ClientContractNumber = "CLAG00000001";

				var container = consol.Containers.AddNew();
				shipment.OuterPackLines.AddNew();

				container.ExportPenalties.DeleteAll();
				shipment.PickupPenalties.DeleteAll();
				container.JC_FCLOnBoardVessel = new ZDateTime(2022, 10, 12);
				CombineAssertions(() =>
				{
					var carMDDPenalty = shipment.PickupPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.MDD && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNotNull(carMDDPenalty);
					AssertEquals((ZByte)12, carMDDPenalty.FreeTimeAsDays);

					var carSTOPenalty = shipment.PickupPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNull(carSTOPenalty);
				});
			}
		}

		public void TestDefaulting_ClientContractSTOWithCarrierMDDPenalties()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 4 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "TSTCAR";
				carrier.OH_IsShippingLine = true;
				carrier.CustomsCodes.AddNew("HID", "FWA", Core.Constants.CountryCodes.UnitedStates);
				carrier.OrgFountains.DeleteAll();
				var carrierSTO = carrier.CarrierContainerPenalties.AddNew();
				carrierSTO.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				carrierSTO.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
				carrierSTO.PD_FreeDays = 9;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = "CONSIGNEE";
				consignee.MainAddress.OA_Address1 = "Consignee Address";
				consignee.OH_IsConsignee = true;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR";
				consignor.MainAddress.OA_Address1 = "Consignor Address";
				consignor.OH_IsConsignor = true;

				var clientContract = CreateNewContract("CLAG00000001", consignor.PK, "CLI");
				AddNewContractPenalty(Core.Constants.ContainerDetentionPenaltyType.STO, "EXP", 12, ZString.Empty, ZString.Empty, ZString.Empty, clientContract.PK, carrier.PK);

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_E_DEP = new ZDateTime(2022, 10, 1);
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				var jobHeader = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
				jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				shipment.Job.JH_ClientContractNumber = "CLAG00000001";

				var container = consol.Containers.AddNew();
				shipment.OuterPackLines.AddNew();

				container.ExportPenalties.DeleteAll();
				shipment.PickupPenalties.DeleteAll();
				container.JC_FCLOnBoardVessel = new ZDateTime(2022, 10, 12);
				CombineAssertions(() =>
				{
					var carSTOPenalty = shipment.PickupPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.STO && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNotNull(carSTOPenalty);
					AssertEquals((ZByte)12, carSTOPenalty.FreeTimeAsDays);

					var carMDDPenalty = shipment.PickupPenalties.FirstOrDefault(p => p.CPY_PenaltyType == Core.Constants.ContainerDetentionPenaltyType.MDD && p.CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier);
					AssertNull(carMDDPenalty);
				});
			}
		}

		IRatingContract CreateNewContract(string number, ZGuid orgPK, ZString contractType)
		{
			var contract = Factory.New<IRatingContract>();
			contract.RCT_OH = orgPK;
			contract.RCT_StartDate = new ZDate(2022, 7, 1);
			contract.RCT_ContractNumber = number;
			contract.RCT_EndDate = ZDate.Empty;
			contract.RCT_ContractType = contractType;
			contract.RCT_IsActive = true;
			contract.RCT_TransportMode = "SEA";
			contract.RCT_GS_NKContractOwner = "USR";

			return contract;
		}

		IRatingContractContainerDetention AddNewContractPenalty(ZString penaltyType = default, ZString direction = default, ZByte freeDays = default, ZString origin = default, ZString location = default, ZString containerClass = default, ZGuid? contractPK = null, ZGuid? clientPK = null)
		{
			var detention = Factory.New<IRatingContractContainerDetention>();
			detention.RCD_RCT = contractPK ?? ZGuid.Empty;
			detention.RCD_PenaltyType = penaltyType;
			detention.RCD_Direction = direction;
			detention.RCD_OriginPortOrCountry = origin;
			detention.RCD_DetentionPortOrCountry = location;
			detention.RCD_ContainerType = containerClass;
			detention.RCD_OH_Client = clientPK ?? ZGuid.Empty;
			detention.RCD_FreeDays = freeDays;
			detention.RCD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.FCLUnload;

			return detention;
		}

		#endregion
	}
}
