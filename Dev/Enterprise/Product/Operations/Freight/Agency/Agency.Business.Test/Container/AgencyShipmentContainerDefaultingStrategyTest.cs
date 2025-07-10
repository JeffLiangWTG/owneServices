using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerDefaultingStrategyTest : TestCaseWithFactory
	{
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestJC_EmptyReturnedByWithLoggedCompany()
		{
			const int detentionFreeDays = 5;
			AssertNotEquals("Precondition: Company-specific detention free days used for this test should not be equal to the system default", detentionFreeDays, FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ValidFreeDays.Value);

			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = detentionFreeDays }))
			{
				var today = ZDateTime.Today;
				var availabilityDate = today.AddDays(-1);
				var sailing1 = CreateSailing("AUSYD", "AUBNE", today.AddDays(-8), ZDateTime.Empty, ZDateTime.Empty);

				var container = Shipment.BookedContainers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
				container.JC_FCLAvailable = today.AddDays(20);
				container.JC_Purpose = ContainerBookedStatus.Codes.Real;

				Shipment.JS_RL_NKOrigin = "AUSYD";
				Shipment.JS_OH_DeliveryAgent = Principal.PK;
				Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				Shipment.JS_JX = sailing1.PK;
				Shipment.JS_IsShipping = true;
				Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

				var transport = Shipment.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = sailing1.PK;
				Factory.Save();

				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				transport.JW_TerminalAvailabilityDate = availabilityDate;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the company-specific detention free days", availabilityDate.AddDays(detentionFreeDays - 1), container.JC_EmptyReturnedBy);
			}
		}

		public void TestSettingContainerSetsDefaultContainerYards()
		{
			OrgAddress address1 = Factory.New<OrgAddress>();
			OrgAddress address2 = Factory.New<OrgAddress>();
			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "Z123";
			refContainer.RC_StorageClass = "20F";
			OrgCarrierAppointedAgentPorts agentPort = Principal.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = address1.PK;
			agentPort.ContainerTypes.AddNew().PT_ContainerStorageClass = "20F";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			CommonContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = refContainer.PK;
			AssertEquals("NO carrier set", ZGuid.Empty, container.JC_OA_DepartureContainerYardAddress);
			AssertEquals("NO carrier set", ZGuid.Empty, container.JC_OA_ArrivalContainerYardAddress);
			shipment.JS_OH_DeliveryAgent = Principal.PK;
			container.JC_RC = ZGuid.Empty;
			container.JC_RC = refContainer.PK;
			AssertEquals(address1.PK, container.JC_OA_DepartureContainerYardAddress);
			AssertEquals("No container yard/park set for discharge port", ZGuid.Empty, container.JC_OA_ArrivalContainerYardAddress);
			OrgCarrierAppointedAgentPorts agentPort2 = Principal.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			agentPort2.O5_PortOrCountry = "USLAX";
			agentPort2.O5_OA_AgentOfficeAddress = address2.PK;
			agentPort2.ContainerTypes.AddNew().PT_ContainerStorageClass = "20F";
			container.JC_RC = ZGuid.Empty;
			container.JC_RC = refContainer.PK;
			AssertEquals(address1.PK, container.JC_OA_DepartureContainerYardAddress);
			AssertEquals(address2.PK, container.JC_OA_ArrivalContainerYardAddress);
		}

		public void TestNullOrgContainerDetention_HasNoException()
		{
			ZDateTime today = ZDateTime.Today;
			AgencyShipmentContainer container = Shipment.BookedContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_FCLAvailable = today.AddDays(20);
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage1.GenerateSailings();
			voyage1.Sailings[0].Destination.JB_AvailabilityDate = today.AddDays(7);
			Transport transport1 = Shipment.Transports.AddNew();
			transport1.JW_IsLinked = true;
			transport1.JW_JX = voyage1.Sailings[0].PK;
			Principal.CarrierContainerPenalties.DeleteAll();
			Importer.ConsigneeContainerPenalties.DeleteAll();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPopulatingJC_EmptyReturnedBy_FromDetentionFreeDays()
		{
			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var now = ZDateTime.Now;
				var sailing0 = CreateSailing("AUSYD", "AUBNE", now.AddDays(-8), now.AddDays(-1), ZDateTime.Empty);
				var sailing1 = CreateSailing("AUSYD", "HKHKG", now, now.AddDays(7), ZDateTime.Empty);
				var sailing2 = CreateSailing("HKHKG", "CNSHA", now.AddDays(8), now.AddDays(14), ZDateTime.Empty);

				var detention = Principal.CarrierContainerPenalties.AddNew();
				detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detention.PD_ContainerType = "20F";
				detention.PD_OriginPortOrCountry = "AU";
				detention.PD_FreeDays = 3;

				var container = Shipment.BookedContainers.AddNew();
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
				container.JC_FCLAvailable = now.AddDays(20);
				container.JC_Purpose = ContainerBookedStatus.Codes.Real;

				Shipment.JS_RL_NKOrigin = "AUSYD";
				Shipment.JS_OH_DeliveryAgent = Principal.PK;
				Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				Shipment.JS_IsShipping = true;
				Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy not populated", ZDateTime.Empty, container.JC_EmptyReturnedBy);
				Shipment.JS_JX = sailing1.PK;
				Transport transport = Shipment.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = sailing0.PK;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", now.AddDays(7 + 3 - 1), container.JC_EmptyReturnedBy);
				transport.JW_JX = sailing2.PK;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", now.AddDays(14 + 3 - 1), container.JC_EmptyReturnedBy);
			}
		}

		public void TestPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysAndDayType_WithMultipleTransports()
		{
			using (AgencyRegistry.Instance.UpdateEmptyReturnByWhenAvailabilityDatesChange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTime.Today;
				ZByte detentionFreeDays = 3;
				var availabilityDate1 = today.AddDays(7);
				var availabilityDate2 = today.AddDays(14);
				var vesselArrivalDate1 = today.AddDays(6);
				var vesselArrivalDate2 = today.AddDays(13);
				var fclAvailableDate = today.AddDays(20);
				var fclGateOutDate = today.AddDays(25);
				var fclUnloadDate = today.AddDays(30);

				var stock = Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = "TTLU5498666";
				stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var voyage1 = Factory.New<JobVoyage>();
				voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
				voyage1.GenerateSailings();
				voyage1.Sailings[0].Destination.JB_AvailabilityDate = availabilityDate1;
				voyage1.Sailings[0].Destination.JB_A_ARV = vesselArrivalDate1;

				var voyage2 = Factory.New<JobVoyage>();
				voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "HKHKG";
				voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNSHA";
				voyage2.GenerateSailings();
				voyage2.Sailings[0].Destination.JB_AvailabilityDate = availabilityDate2;
				voyage2.Sailings[0].Destination.JB_A_ARV = vesselArrivalDate2;

				var container = Shipment.BookedContainers.AddNew();
				container.JC_ContainerNum = stock.R6_ContainerNum;
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
				container.JC_FCLAvailable = fclAvailableDate;
				container.JC_Purpose = ContainerBookedStatus.Codes.Real;

				var detention = Principal.CarrierContainerPenalties.AddNew();
				detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detention.PD_ContainerType = "20F";
				detention.PD_OriginPortOrCountry = "AU";
				detention.PD_FreeDays = detentionFreeDays;

				CreateMovement(stock, fclGateOutDate, ContainerMovementTypes.Codes.WharfGateOut, voyage1.PK, Principal.MainAddress.PK);

				Shipment.JS_RL_NKOrigin = "AUSYD";
				Shipment.JS_OH_DeliveryAgent = Principal.PK;
				Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = Importer.MainAddress.PK;
				Shipment.JS_IsShipping = true;
				Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

				var transport1 = Shipment.Transports.AddNew();
				transport1.JW_IsLinked = true;
				transport1.JW_JX = voyage1.Sailings[0].PK;

				var transport2 = Shipment.Transports.AddNew();
				transport2.JW_IsLinked = true;
				transport2.JW_JX = voyage2.Sailings[0].PK;
				AssertEquals("JC_FCLWharfGateOut is populated", container.JC_FCLWharfGateOut, fclGateOutDate);
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("PD_FreeDayType should default to CTOAvailable", detention.PD_FreeDayType, Core.Constants.ContainerDetentionFreeDayType.CTOAvailable);
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", availabilityDate2.AddDays(detentionFreeDays - 1), container.JC_EmptyReturnedBy);
				detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload;
				AssertEquals("Precondition", ZDateTime.Empty, container.JC_FCLUnloadFromVessel);

				AssertNoExceptionThrown("DayAfterFCLUnload with empty JC_FCLUnloadFromVessel", () => Factory.Save());
				CreateMovement(stock, fclUnloadDate, ContainerMovementTypes.Codes.Discharge, voyage1.PK, Principal.MainAddress.PK);
				AssertEquals("JC_FCLUnloadFromVessel is populated", container.JC_FCLUnloadFromVessel, fclUnloadDate);
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", fclUnloadDate.AddDays(detentionFreeDays), container.JC_EmptyReturnedBy);

				detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.FCLUnload;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", fclUnloadDate.AddDays(detentionFreeDays - 1), container.JC_EmptyReturnedBy);

				detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.CTOGateOut;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", fclGateOutDate.AddDays(detentionFreeDays - 1), container.JC_EmptyReturnedBy);

				detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.VesselArrival;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", vesselArrivalDate2.AddDays(detentionFreeDays - 1), container.JC_EmptyReturnedBy);

				detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("JC_EmptyReturnedBy populated from the detention free days", fclUnloadDate.AddDays(detentionFreeDays), container.JC_EmptyReturnedBy);
			}
		}

		#region Implementation
		AgencyShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<AgencyShipment>();
				}

				return shipment;
			}
		}

		AgencyShipment shipment;
		OrgHeader Principal
		{
			get
			{
				return principal ?? (principal = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader principal;
		OrgHeader Importer
		{
			get
			{
				return importer ?? (importer = Factory.NewWithValidTestData<OrgHeader>());
			}
		}

		OrgHeader importer;
		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd, ZDateTime availabilityDate, ZDateTime vesselArrivalDate)
		{
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			destination.JB_AvailabilityDate = availabilityDate;
			destination.JB_A_ARV = vesselArrivalDate;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		ContainerMovement CreateMovement(RefContainerStock stock, ZDateTime movementDate, ZString movementType, ZGuid voyagePK, ZGuid depotPK)
		{
			ContainerMovement movement1 = stock.Movements.AddNew();
			movement1.E9_JV = voyagePK;
			movement1.E9_MovementType = movementType;
			movement1.E9_MovementDate = movementDate;
			movement1.E9_OA_Depot = depotPK;
			return movement1;
		}
		#endregion
	}
}
