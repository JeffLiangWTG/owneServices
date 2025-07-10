using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovementRelatedInfo))]
	internal class ContainerMovementRelatedInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadContainers()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "018N";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S18181818";
			shipment.JS_JX = voyage.Sailings[0].PK;
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_ContainerNum = "TEST4100013";
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementDate = ZDateTime.Now.ToSmallDateTime();
			movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement.E9_JV = voyage.PK;
			Factory.Save();
			var containers = movement.RelatedInfo.LoadContainers();
			AssertEquals("containers.Length", 1, containers.Length);
			AssertEquals("containers[0].JC_ContainerNum", "TEST4100013", containers[0].JC_ContainerNum);
			Assert("Don't re-use returned arrays.", !object.ReferenceEquals(movement.RelatedInfo.LoadContainers(), containers));
		}

		public void TestNoShipments()
		{
			ContainerMovement movement = Stock.Movements.AddNew();
			Factory.Save();
			ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
			AssertEquals("BillsOfLading", "", info.BillsOfLading);
			AssertEquals("ShipmentNumbers", "", info.ShipmentNumbers);
			AssertEquals("BookingNumbers", "", info.BookingNumbers);
			AssertEquals("LoadPort", "", info.LoadPort);
			AssertEquals("DischargePort", "", info.DischargePort);
			AssertEquals("Consignor", "", info.Consignor);
			AssertEquals("Consignee", "", info.Consignee);
			AssertEquals("Local Client", "", info.LocalClient);
			AssertEquals("FallBack Local Client", "", info.FallBackLocalClient);
			AssertEquals("Principal", "", info.Principal);
			AssertEquals("FallBack Principal", "", info.FallBackPrincipal);
			AssertEquals("AvailabilityDate", ZDateTime.Empty, info.AvailabilityDate);
			AssertEquals("ReturnBy", ZDateTime.Empty, info.ReturnByDate);
		}

		public void TestClientPrincipalOverrides()
		{
			ZDateTime now = ZDateTime.Now;
			OrgHeader localClient1 = Factory.NewWithValidTestData<OrgHeader>();
			localClient1.OH_Code = "Client1";
			OrgHeader localClient2 = Factory.NewWithValidTestData<OrgHeader>();
			localClient2.OH_Code = "Client2";
			OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
			principal1.OH_Code = "Principal1";
			OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
			principal2.OH_Code = "Principal2";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations[0].JB_AvailabilityDate = now.AddDays(-7);
			voyage.GenerateSailings();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.JS_OH_DeliveryAgent = principal1.PK;
			JobHeader header = new JobHeader.Loader(shipment).TryLoadOrCreate();
			header.JH_OA_LocalChargesAddr = localClient1.MainAddress.PK;
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = Stock.R6_ContainerNum;
			container.JC_EmptyReturnedBy = now.AddDays(-1);
			ContainerMovement movement = Stock.Movements.AddNew();
			movement.E9_JV = voyage.PK;
			movement.E9_OH_Principal = principal2.PK;
			movement.E9_OH_ResponsibleParty = localClient2.PK;
			Factory.Save();
			ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
			AssertEquals("Local Client", "Client2", info.LocalClient);
			AssertEquals("FallBack Local Client", "Client1", info.FallBackLocalClient);
			AssertEquals("Principal", "Principal2", info.Principal);
			AssertEquals("FallBack Principal", "Principal1", info.FallBackPrincipal);
		}

		public void Test1Shipment()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_Code = "Consignor";
				OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_Code = "Consignee";
				OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_Code = "Client";
				OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = "Principal";
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
				voyage.Destinations[0].JB_AvailabilityDate = now.AddDays(-12);
				voyage.GenerateSailings();
				AgencyShipment shipment = Factory.New<AgencyShipment>();
				shipment.JS_UniqueConsignRef = "V00000100";
				shipment.JS_HouseBill = "BNESYD01000";
				shipment.JS_CFSReference = "BOOKINGNUM";
				shipment.JS_JX = voyage.Sailings[0].PK;
				shipment.JS_OH_DeliveryAgent = principal.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
				JobHeader header = new JobHeader.Loader(shipment).TryLoadOrCreate();
				header.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				AgencyShipmentContainer container = shipment.RealContainers.AddNew();
				container.JC_ContainerNum = Stock.R6_ContainerNum;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				ContainerMovement movement = Stock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				Factory.Save();
				ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
				AssertEquals("BillsOfLading", "BNESYD01000", info.BillsOfLading);
				AssertEquals("ShipmentNumbers", "V00000100", info.ShipmentNumbers);
				AssertEquals("BookingNumbers", "BOOKINGNUM", info.BookingNumbers);
				AssertEquals("LoadPort", "AUBNE", info.LoadPort);
				AssertEquals("DischargePort", "AUSYD", info.DischargePort);
				AssertEquals("Consignor", "Consignor", info.Consignor);
				AssertEquals("Consignee", "Consignee", info.Consignee);
				AssertEquals("Local Client", "Client", info.LocalClient);
				AssertEquals("FallBack Local Client", "Client", info.FallBackLocalClient);
				AssertEquals("Principal", "Principal", info.Principal);
				AssertEquals("FallBack Principal", "Principal", info.FallBackPrincipal);
				AssertEquals("AvailabilityDate", now.AddDays(-12), info.AvailabilityDate);
				AssertEquals("ReturnBy", now.AddDays(-3), info.ReturnByDate);
				var roadVoyage = Factory.New<JobVoyage>();
				roadVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
				roadVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				roadVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUADL";
				roadVoyage.Destinations[0].JB_AvailabilityDate = now.AddDays(-10);
				roadVoyage.GenerateSailings();
				var roadLeg = shipment.TransportsIncludingRelated.AddNew();
				roadLeg.JW_RL_NKLoadPort = "AUSYD";
				roadLeg.JW_RL_NKDiscPort = "AUADL";
				roadLeg.JW_IsLinked = true;
				roadLeg.JW_JX = roadVoyage.Sailings[0].PK;
				Factory.Save();
				AssertEquals("AvailabilityDate", now.AddDays(-10), info.AvailabilityDate);
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				Factory.Save();
				AssertEquals("ReturnBy", now.AddDays(-1), info.ReturnByDate);
			}
		}

		public void Test2Shipments()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			consignor1.OH_Code = "Consignor1";
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_Code = "Consignee1";
			OrgHeader localClient1 = Factory.NewWithValidTestData<OrgHeader>();
			localClient1.OH_Code = "Client1";
			OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
			principal1.OH_Code = "Principal1";
			OrgHeader localClient2 = Factory.NewWithValidTestData<OrgHeader>();
			localClient2.OH_Code = "Client2";
			OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
			principal2.OH_Code = "Principal2";
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "GBLON";
			origin1.JA_E_DEP = now.AddDays(-11);
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			origin2.JA_E_DEP = now.AddDays(-10);
			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			destination1.JB_E_ARV = now.AddDays(-9);
			VoyageOrigin origin3 = voyage.Origins.AddNew();
			origin3.JA_RL_NKPortOfLoading = "AUBNE";
			origin3.JA_E_DEP = now.AddDays(-8);
			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_E_ARV = now.AddDays(-7);
			VoyageOrigin origin4 = voyage.Origins.AddNew();
			origin4.JA_RL_NKPortOfLoading = "AUSYD";
			origin4.JA_E_DEP = now.AddDays(-6);
			VoyageDestination destination3 = voyage.Destinations.AddNew();
			destination3.JB_RL_NKPortOfDischarge = "NZAKL";
			destination3.JB_E_ARV = now.AddDays(-5);
			voyage.GenerateSailings();
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_UniqueConsignRef = "V00000100";
			shipment1.JS_HouseBill = "BNESYD01000";
			shipment1.JS_CFSReference = "BOOKINGNUM1";
			shipment1.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("SGSIN", "AUBNE").PK;
			shipment1.JS_OH_DeliveryAgent = principal1.PK;
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor1.MainAddress.PK;
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee1.MainAddress.PK;
			JobHeader header1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			header1.JH_OA_LocalChargesAddr = localClient1.MainAddress.PK;
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_UniqueConsignRef = "V00000101";
			shipment2.JS_HouseBill = "BNESYD01001";
			shipment2.JS_CFSReference = "BOOKINGNUM2";
			shipment2.JS_OH_DeliveryAgent = principal2.PK;
			shipment2.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment2.ConsignorDocumentaryAddress.E2_CompanyName = "Consignor2";
			shipment2.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment2.ConsigneeDocumentaryAddress.E2_CompanyName = "Consignee2";
			Transport transport = shipment2.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "AUSYD").PK;
			JobHeader header2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			header2.JH_OA_LocalChargesAddr = localClient2.MainAddress.PK;
			header2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();
			container1.JC_ContainerNum = Stock.R6_ContainerNum;
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();
			container2.JC_ContainerNum = Stock.R6_ContainerNum;
			ContainerMovement movement = Stock.Movements.AddNew();
			movement.E9_JV = voyage.PK;
			Factory.Save();
			ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
			AssertEquals("BillsOfLading", "BNESYD01000, BNESYD01001", info.BillsOfLading);
			AssertEquals("ShipmentNumbers", "V00000100, V00000101", info.ShipmentNumbers);
			AssertEquals("BookingNumbers", "BOOKINGNUM1, BOOKINGNUM2", info.BookingNumbers);
			var loadedContainers = info.LoadContainers();

			AssertCorrectPort();

			void AssertCorrectPort()
			{
				AssertEquals(shipment1.Transports.Count, 1);
				AssertEquals(shipment2.Transports.Count, 1);
				var shipment1Sailling = shipment1.Transports[0].Sailing;
				var shipment2Sailling = shipment2.Transports[0].Sailing;
				AssertLessThan(shipment1Sailling.Origin.JA_E_DEP, shipment2Sailling.Origin.JA_E_DEP);
				AssertGreaterThan(shipment2Sailling.Destination.JB_E_ARV, shipment1Sailling.Destination.JB_E_ARV);
				AssertEquals("LoadPort", shipment1Sailling.Origin.JA_RL_NKPortOfLoading, info.LoadPort);
				AssertEquals("DischargePort", shipment2Sailling.Destination.JB_RL_NKPortOfDischarge, info.DischargePort);
			}

			AssertEquals("Consignor", "Consignor1, Consignor2", info.Consignor);
			AssertEquals("Consignee", "Consignee1, Consignee2", info.Consignee);
			AssertEquals("Local Client", "Client1, Client2", info.LocalClient);
			AssertEquals("FallBack Local Client", "Client1, Client2", info.FallBackLocalClient);
			AssertEquals("Principal", "Principal1, Principal2", info.Principal);
			AssertEquals("FallBack Principal", "Principal1, Principal2", info.FallBackPrincipal);
			AssertEquals("AvailabilityDate", ZDateTime.Empty, info.AvailabilityDate);
			AssertEquals("ReturnBy", ZDateTime.Empty, info.ReturnByDate);
		}

		public void TestExcess()
		{
			ZDateTime now = ZDateTime.Now.ToSmallDateTime();
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUHBT";
			voyage.Destinations.GetDestinationFromDischarge("AUSYD").JB_AvailabilityDate = now.AddDays(-7);
			voyage.Destinations.GetDestinationFromDischarge("AUHBT").JB_AvailabilityDate = now.AddDays(-8);
			voyage.GenerateSailings();
			AgencyShipment shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_UniqueConsignRef = "V0000000100";
			shipment1.JS_HouseBill = "BNESYD01000";
			shipment1.JS_CFSReference = "BOOKINGNUM1";
			shipment1.JS_RL_NKOrigin = "AUBNE";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "AUSYD").PK;
			AgencyShipment shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_UniqueConsignRef = "V0000000101";
			shipment2.JS_RL_NKOrigin = "AUCNS";
			shipment2.JS_RL_NKDestination = "AUMEL";
			shipment2.JS_HouseBill = "BNESYD01001";
			shipment2.JS_CFSReference = "BOOKINGNUM2";
			shipment2.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "AUSYD").PK;
			AgencyShipment shipment3 = Factory.New<AgencyShipment>();
			shipment3.JS_UniqueConsignRef = "V0000000102";
			shipment3.JS_RL_NKOrigin = "AUTMB";
			shipment3.JS_RL_NKDestination = "AUHBT";
			shipment3.JS_HouseBill = "BNESYD01002";
			shipment3.JS_CFSReference = "BOOKINGNUM3";
			shipment3.JS_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "AUHBT").PK;
			AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();
			container1.JC_ContainerNum = Stock.R6_ContainerNum;
			container1.JC_EmptyReturnedBy = now.AddDays(-1);
			AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();
			container2.JC_ContainerNum = Stock.R6_ContainerNum;
			container2.JC_EmptyReturnedBy = ZDateTime.Empty;
			AgencyShipmentContainer container3 = shipment3.RealContainers.AddNew();
			container3.JC_ContainerNum = Stock.R6_ContainerNum;
			container3.JC_EmptyReturnedBy = now.AddDays(-2);
			ContainerMovement movement = Stock.Movements.AddNew();
			movement.E9_JV = voyage.PK;
			Factory.Save();
			ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
			AssertEquals("BillsOfLading", "BNESYD01000, BNESYD01001, ...", info.BillsOfLading);
			AssertEquals("ShipmentNumbers", "V0000000100, V0000000101, ...", info.ShipmentNumbers);
			AssertEquals("BookingNumbers", "BOOKINGNUM1, BOOKINGNUM2, ...", info.BookingNumbers);
			AssertEquals("AvailabilityDate", now.AddDays(-8), info.AvailabilityDate);
			AssertEquals("ReturnBy", now.AddDays(-2), info.ReturnByDate);
		}

		public void TestImportDetentionFreeDays()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
				client1.OH_Code = "Client1";
				OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
				client2.OH_Code = "Client2";
				OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
				principal1.OH_Code = "Carrier1";
				OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
				principal2.OH_Code = "Carrier2";
				OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
				depot.OH_Code = "depot";
				depot.OH_RL_NKClosestPort = "AUCNS";
				OrgContainerDetention detentionFree11 = principal1.CarrierContainerPenalties.AddNew();
				detentionFree11.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree11.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detentionFree11.PD_OH_Client = client1.PK;
				detentionFree11.PD_FreeDays = 11;
				detentionFree11.PD_ContainerType = "20F";
				detentionFree11.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree12 = principal1.CarrierContainerPenalties.AddNew();
				detentionFree12.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree12.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detentionFree12.PD_OH_Client = client2.PK;
				detentionFree12.PD_FreeDays = 12;
				detentionFree12.PD_ContainerType = "20F";
				detentionFree12.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree21 = principal2.CarrierContainerPenalties.AddNew();
				detentionFree21.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree21.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detentionFree21.PD_OH_Client = client1.PK;
				detentionFree21.PD_FreeDays = 21;
				detentionFree21.PD_ContainerType = "20F";
				detentionFree21.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree22 = principal2.CarrierContainerPenalties.AddNew();
				detentionFree22.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree22.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detentionFree22.PD_OH_Client = client2.PK;
				detentionFree22.PD_FreeDays = 22;
				detentionFree22.PD_ContainerType = "20F";
				detentionFree22.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree22b = principal2.CarrierContainerPenalties.AddNew();
				detentionFree22b.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree22b.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detentionFree22b.PD_OH_Client = client2.PK;
				detentionFree22b.PD_FreeDays = 23;
				detentionFree22b.PD_ContainerType = "20F";
				detentionFree22b.PD_DetentionPortOrCountry = "AUCNS";
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.GenerateSailings();
				AgencyShipment shipment1 = Factory.New<AgencyShipment>();
				shipment1.JS_JX = voyage.Sailings[0].PK;
				shipment1.ConsigneeDocumentaryAddress.OrganisationPK = client1.PK;
				shipment1.JS_OH_DeliveryAgent = principal1.PK;
				shipment1.JS_RL_NKOrigin = "GBLON";
				shipment1.JS_RL_NKDestination = "AUBNE";
				AgencyShipment shipment2 = Factory.New<AgencyShipment>();
				shipment2.JS_JX = voyage.Sailings[0].PK;
				shipment2.ConsigneeDocumentaryAddress.OrganisationPK = client2.PK;
				shipment2.JS_OH_DeliveryAgent = principal2.PK;
				shipment2.JS_RL_NKOrigin = "GBLON";
				shipment2.JS_RL_NKDestination = "AUBNE";
				AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();
				container1.JC_ContainerNum = Stock.R6_ContainerNum;
				container1.JC_RC = Stock.R6_RC;
				container1.Container.RC_StorageClass = "20F";
				AgencyShipmentContainer container2 = shipment1.RealContainers.AddNew();
				container2.JC_ContainerNum = Stock.R6_ContainerNum;
				container2.JC_RC = Stock.R6_RC;
				container2.Container.RC_StorageClass = "20F";
				ContainerMovement movement = Stock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				movement.E9_OA_Depot = depot.MainAddress.PK;
				ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
				Factory.Save();
				AssertEquals("no overrides", (short)11, info.ImportDetentionFreeDays);
				movement.E9_OH_Principal = principal2.PK;
				AssertEquals("override principal", (short)21, info.ImportDetentionFreeDays);
				movement.E9_OH_ResponsibleParty = client2.PK;
				AssertEquals("override client and principal", (short)23, info.ImportDetentionFreeDays);
				movement.E9_OH_Principal = ZGuid.Empty;
				AssertEquals("override client", (short)12, info.ImportDetentionFreeDays);
				movement.E9_OH_Principal = principal2.PK;
				movement.E9_JV = ZGuid.Empty;
				AssertEquals("no voyage", (short)23, info.ImportDetentionFreeDays);
			}
		}

		public void TestExportDetentionFreeDays()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
				client1.OH_Code = "Client1";
				OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
				client2.OH_Code = "Client2";
				OrgHeader principal1 = Factory.NewWithValidTestData<OrgHeader>();
				principal1.OH_Code = "Carrier1";
				OrgHeader principal2 = Factory.NewWithValidTestData<OrgHeader>();
				principal2.OH_Code = "Carrier2";
				OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
				depot.OH_Code = "depot";
				depot.OH_RL_NKClosestPort = "AUCNS";
				OrgContainerDetention detentionFree11 = principal1.CarrierContainerPenalties.AddNew();
				detentionFree11.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree11.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree11.PD_OH_Client = client1.PK;
				detentionFree11.PD_FreeDays = 11;
				detentionFree11.PD_ContainerType = "20F";
				detentionFree11.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree12 = principal1.CarrierContainerPenalties.AddNew();
				detentionFree12.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree12.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree12.PD_OH_Client = client2.PK;
				detentionFree12.PD_FreeDays = 12;
				detentionFree12.PD_ContainerType = "20F";
				detentionFree12.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree21 = principal2.CarrierContainerPenalties.AddNew();
				detentionFree21.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree21.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree21.PD_OH_Client = client1.PK;
				detentionFree21.PD_FreeDays = 21;
				detentionFree21.PD_ContainerType = "20F";
				detentionFree21.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree22 = principal2.CarrierContainerPenalties.AddNew();
				detentionFree22.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree22.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree22.PD_OH_Client = client2.PK;
				detentionFree22.PD_FreeDays = 22;
				detentionFree22.PD_ContainerType = "20F";
				detentionFree22.PD_DetentionPortOrCountry = "AU";
				OrgContainerDetention detentionFree22b = principal2.CarrierContainerPenalties.AddNew();
				detentionFree22b.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detentionFree22b.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detentionFree22b.PD_OH_Client = client2.PK;
				detentionFree22b.PD_FreeDays = 23;
				detentionFree22b.PD_ContainerType = "20F";
				detentionFree22b.PD_DetentionPortOrCountry = "AUCNS";
				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
				voyage.GenerateSailings();
				AgencyShipment shipment1 = Factory.New<AgencyShipment>();
				shipment1.JS_JX = voyage.Sailings[0].PK;
				shipment1.ConsignorDocumentaryAddress.OrganisationPK = client1.PK;
				shipment1.JS_OH_DeliveryAgent = principal1.PK;
				shipment1.JS_RL_NKOrigin = "AUBNE";
				shipment1.JS_RL_NKDestination = "AUWEL";
				AgencyShipment shipment2 = Factory.New<AgencyShipment>();
				shipment2.JS_JX = voyage.Sailings[0].PK;
				shipment2.ConsignorDocumentaryAddress.OrganisationPK = client2.PK;
				shipment2.JS_OH_DeliveryAgent = principal2.PK;
				shipment2.JS_RL_NKOrigin = "AUBNE";
				shipment2.JS_RL_NKDestination = "AUWEL";
				AgencyShipmentContainer container1 = shipment1.RealContainers.AddNew();
				container1.JC_ContainerNum = Stock.R6_ContainerNum;
				container1.JC_RC = Stock.R6_RC;
				container1.Container.RC_StorageClass = "20F";
				AgencyShipmentContainer container2 = shipment2.RealContainers.AddNew();
				container2.JC_ContainerNum = Stock.R6_ContainerNum;
				container2.JC_RC = Stock.R6_RC;
				container2.Container.RC_StorageClass = "20F";
				ContainerMovement movement = Stock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				movement.E9_OA_Depot = depot.MainAddress.PK;
				ContainerMovementRelatedInfo info = new ContainerMovementRelatedInfo(movement);
				Factory.Save();
				AssertEquals("no overrides", (short)11, info.ExportDetentionFreeDays);
				movement.E9_OH_Principal = principal2.PK;
				AssertEquals("override principal", (short)21, info.ExportDetentionFreeDays);
				movement.E9_OH_ResponsibleParty = client2.PK;
				AssertEquals("override client and principal", (short)23, info.ExportDetentionFreeDays);
				movement.E9_OH_Principal = ZGuid.Empty;
				AssertEquals("override client", (short)12, info.ExportDetentionFreeDays);
				movement.E9_OH_Principal = principal2.PK;
				movement.E9_JV = ZGuid.Empty;
				AssertEquals("no voyage", (short)23, info.ExportDetentionFreeDays);
			}
		}

		[UseDummyDetentionStrategy]
		public void TestDetentionFreeDays()
		{
			DummyDetentionStrategy.Instance.GetDetentionFreeDaysOverride = delegate
			{
				return 5;
			};
			ContainerMovement movement = Stock.Movements.AddNew();
			AssertEquals((short)5, movement.RelatedInfo.DetentionFreeDays);
		}

		[UseDummyDetentionStrategy]
		public void TestStartOfDetentionFreePeriod()
		{
			ZDateTime now = ZDateTime.Now;
			DummyDetentionStrategy.Instance.GetStartOfDetentionFreePeriodOverride = delegate
			{
				return now.AddDays(-1);
			};
			ContainerMovement movement = Stock.Movements.AddNew();
			AssertEquals(now.AddDays(-1), movement.RelatedInfo.StartOfDetentionFreePeriod);
		}

		[UseDummyDetentionStrategy]
		public void TestStartOfDetentionPeriod()
		{
			ZDateTime now = ZDateTime.Now;
			DummyDetentionStrategy.Instance.GetStartOfDetentionPeriodOverride = delegate
			{
				return now.AddDays(-1);
			};
			ContainerMovement movement = Stock.Movements.AddNew();
			AssertEquals(now.AddDays(-1), movement.RelatedInfo.StartOfDetentionPeriod);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContainerMovementRelatedInfo(Stock.Movements.AddNew());
		}

		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "TEST4100013";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		#endregion
	}
}
