using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipmentInvoicingSupporter))]
	sealed class CommonShipmentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public override void TestCustomsEntryNumberType()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.CustomsEntryNumberType = "TF";
			var supporter = new CommonShipmentInvoicingSupporter(shipment);

			AssertEquals("CustomsEntryNumberType", "TF", supporter.CustomsEntryNumberType);
		}

		public override void TestCommunityTransitStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_CommunityTransitStatus = "TF";
			var supporter = new CommonShipmentInvoicingSupporter(shipment);

			AssertEquals("CommunityTransitStatus", "TF", supporter.CommunityTransitStatus);
		}

		public void TestConstantSecurityProperties()
		{
			AssertEquals("AuditSecurity", null, Supporter.AuditSecurity);
			AssertEquals("EditSecurityCheckpoint", Env.Security.None, Supporter.EditSecurityCheckpoint);
			AssertEquals("EditSecurityLock", false, Supporter.EditSecurityLock);
			AssertEquals("EditSecurityMessage", ZString.Empty, Supporter.EditSecurityMessage);
		}

		public void TestJobInvoicingSecurityImplementation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("Precondition", JobInvoicingConsumerTypes.CFSShipment, supporter.ConsumerType);
			AssertEquals("JobInvoicingSecurity", Env.Security.CFSShipmentJobInvoicing, supporter.JobInvoicingSecurity);

			shipment.JS_IsForwardRegistered = true;
			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment, supporter.ConsumerType);
			AssertEquals("JobInvoicingSecurity", Env.Security.MaintainShipmentJobInvoicing, supporter.JobInvoicingSecurity);
		}

		public void TestConsumerType()
		{
			Action<bool, bool, bool, JobInvoicingConsumerType> assertConsumerType = (isBooking, isCFSRegistered, isForwardRegistered, expectedType) =>
				{
					var shipment = Factory.New<CommonShipment>();
					shipment.JS_IsBooking = isBooking;
					shipment.JS_IsCFSRegistered = isCFSRegistered;
					shipment.JS_IsForwardRegistered = isForwardRegistered;

					var supporter = new CommonShipmentInvoicingSupporter(shipment);
					AssertEquals("ConsumerType", expectedType, supporter.ConsumerType);
				};

			assertConsumerType(false, true, false, JobInvoicingConsumerTypes.CFSShipment);
			assertConsumerType(true, true, false, JobInvoicingConsumerTypes.QuotedBooking);
			assertConsumerType(false, true, true, JobInvoicingConsumerTypes.Shipment);
			assertConsumerType(false, false, false, JobInvoicingConsumerTypes.Shipment);
			assertConsumerType(false, false, true, JobInvoicingConsumerTypes.Shipment);
			assertConsumerType(true, true, true, JobInvoicingConsumerTypes.Shipment);
			assertConsumerType(true, false, false, JobInvoicingConsumerTypes.QuotedBooking);
		}

		public void TestConsolType()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ConsolType", (ZString)Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, supporter.ConsolType);

			shipment.Consols.AddNew().JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("ConsolType", Constants.AgentType.Agent, supporter.ConsolType);
		}

		public void TestConsolNumber()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ConsolNumber", ZString.Empty, supporter.ConsolNumber);

			string expectedNumber = "SomeNumber";
			shipment.Consols.AddNew().JK_UniqueConsignRef = expectedNumber;
			AssertEquals("ConsolNumber", expectedNumber, supporter.ConsolNumber);
		}

		public void TestConsolExchangeRate()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			VoyageExRate usdVoyageRate = voyage.ExRates.AddNew();
			usdVoyageRate.E8_RX_NKExCurrency = "USD";
			usdVoyageRate.E8_VoyageExchangeRate = 1.024m;

			VoyageExRate nzdVoyageRate = voyage.ExRates.AddNew();
			nzdVoyageRate.E8_RX_NKExCurrency = "NZD";
			nzdVoyageRate.E8_VoyageExchangeRate = 2.048m;

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

			AssertEquals("Precondition", "USD", consol.FreightCostsCurrency.RX_Code);
			AssertEquals("Precondition", 1.024m, consol.FreightCostsExchangeRate);
			AssertEquals("Precondition", 2.048m, consol.GetExchangeRateFromFreightCostsOrSchedule("NZD"));

			CommonShipment shipment = consol.Shipments.AddNew();
			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);

			AssertEquals("USD", supporter.ConsolRateCurrency.RX_Code);
			AssertEquals(1.024m, supporter.ConsolExchangeRate);
			AssertEquals(2.048m, supporter.GetConsolExchangeRate("NZD"));
			AssertEquals(0m, supporter.GetConsolExchangeRate("XXX"));
		}

		public void TestModeProperties()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("TransportMode", Constants.TransportModes.Air, supporter.TransportMode);
			AssertEquals("ContainerMode", Constants.ContainerModes.Loose, supporter.ContainerMode);
		}

		public void TestLocationProperties()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);

			AssertNull("Precondition", shipment.Origin);
			AssertNull("Origin", supporter.Origin);

			AssertNull("Precondition", shipment.Destination);
			AssertNull("Destination", supporter.Destination);

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				shipment.JS_RL_NKOrigin = "";
				shipment.JS_RL_NKDestination = "USSFO";
				AssertNull("Precondition", shipment.Origin);
				AssertEquals("Origin from consol", "AUSYD", supporter.Origin.RL_Code);
			}

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "";
			AssertNull("Precondition", shipment.Destination);
			AssertEquals("Destination from consol", "USLAX", supporter.Destination.RL_Code);

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("Origin from shipment", "AUMEL", supporter.Origin.RL_Code);
			AssertEquals("Destination from shipment", "NZAKL", supporter.Destination.RL_Code);
		}

		public void TestOrganizationProperties()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			OrgHeader consignor = Factory.New<OrgHeader>();
			OrgHeader pickUpAgent = Factory.New<OrgHeader>();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = pickUpAgent.PK;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("Consignee", consignee, supporter.Consignee);
			AssertEquals("Consignor", consignor, supporter.Consignor);
			AssertEquals("PickUpAgent", pickUpAgent, supporter.PickUpAgent);
			AssertNull("ReceivingAgent", supporter.ReceivingAgent);
			AssertNull("SendingAgent", supporter.SendingAgent);

			OrgHeader deliveryAgent = Factory.New<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			AssertEquals("ReceivingAgent", deliveryAgent, supporter.ReceivingAgent);
			AssertNull("SendingAgent", supporter.SendingAgent);
			AssertEquals("DeliveryAgent", deliveryAgent, supporter.DeliveryAgent);

			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertNull("ReceivingAgent", supporter.ReceivingAgent);
			AssertNull("SendingAgent", supporter.SendingAgent);

			CommonConsol consol = shipment.Consols.AddNew();
			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			AssertEquals("ReceivingAgent from consol", receivingForwarder, supporter.ReceivingAgent);
			AssertEquals("SendingAgent from consol", sendingForwarder, supporter.SendingAgent);
		}

		public void TestMeasureProperties()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualVolume = 0.1;
			shipment.JS_ActualWeight = 100;
			shipment.JS_ActualChargeable = 110;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ActualChargeable", 110m, supporter.ActualChargeable);
			AssertEquals("ActualChargeableUnit", "KG", supporter.ActualChargeableUnit);
			AssertEquals("ActualVolume", 0.1m, supporter.ActualVolume);
			AssertEquals("ActualVolumeUnit", "M3", supporter.ActualVolumeUnit);
			AssertEquals("ActualWeight", 100m, supporter.ActualWeight);
			AssertEquals("ActualWeightUnit", "KG", supporter.ActualWeightUnit);
		}

		public void TestDateTimeProperties()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2011, 06, 01);
			shipment.JS_E_ARV = new ZDateTime(2011, 07, 01);
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 08, 01);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 09, 01);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2011, 10, 01);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2011, 11, 01);

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ETD", new ZDateTime(2011, 06, 01), supporter.ETD);
			AssertEquals("ATD", new ZDateTime(2011, 06, 01), supporter.ATD);
			AssertEquals("ETA", new ZDateTime(2011, 07, 01), supporter.ETA);
			AssertEquals("ATA", new ZDateTime(2011, 07, 01), supporter.ATA);
			AssertEquals("ESP", new ZDateTime(2011, 08, 01), supporter.ESP);
			AssertEquals("ESD", new ZDateTime(2011, 09, 01), supporter.ESD);
			AssertEquals("ActualPickupDate", new ZDateTime(2011, 10, 01), supporter.ActualPickupDate);
			AssertEquals("ActualDeliveryDate", new ZDateTime(2011, 11, 01), supporter.ActualDeliveryDate);

			CommonConsol consol = shipment.Consols.AddNew();
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "NZAKL";
			consol.Transports.MostInterestingTransport.JW_ATD = new ZDateTime(2011, 06, 10);
			consol.Transports.MostInterestingTransport.JW_ATA = new ZDateTime(2011, 07, 10);

			AssertEquals("ETD", new ZDateTime(2011, 06, 01), supporter.ETD);
			AssertEquals("ATD", new ZDateTime(2011, 06, 01), supporter.ATD);
			AssertEquals("ETA", new ZDateTime(2011, 07, 01), supporter.ETA);
			AssertEquals("ATA", new ZDateTime(2011, 07, 01), supporter.ATA);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals("ETD", new ZDateTime(2011, 06, 01), supporter.ETD);
			AssertEquals("ATD from consol", new ZDateTime(2011, 06, 10), supporter.ATD);
			AssertEquals("ETA", new ZDateTime(2011, 07, 01), supporter.ETA);
			AssertEquals("ATA from consol", new ZDateTime(2011, 07, 10), supporter.ATA);
		}

		public void TestDateTimeProperties_PickupAndDeliveryDate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2011, 08, 01);
			shipment.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2011, 09, 01);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2011, 10, 01);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2011, 11, 01);

			var supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ESP", new ZDateTime(2011, 08, 01), supporter.ESP);
			AssertEquals("ESD", new ZDateTime(2011, 09, 01), supporter.ESD);
			AssertEquals("ActualPickupDate", new ZDateTime(2011, 10, 01), supporter.ActualPickupDate);
			AssertEquals("ActualDeliveryDate", new ZDateTime(2011, 11, 01), supporter.ActualDeliveryDate);
		}

		public void TestActualAndEstimatedArrivalAtLoadPort()
		{
			var today = ZDateTime.Today;
			var shipment_ETD = today.AddDays(10);
			var shipment_ETA = today.AddDays(11);
			var sailing1_E_ATL = today.AddDays(20);
			var sailing1_A_ATL = today.AddDays(21);
			var sailing2_E_ATL = today.AddDays(30);
			var sailing2_A_ATL = today.AddDays(31);

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage1.JV_VoyageFlight = "12";
			voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First().RV_FK;

			var origin1 = voyage1.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			var sailing1 = voyage1.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			AssertNotNull("Sailing should exist", sailing1);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "APPLE IVORY";

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage2.JV_VoyageFlight = "34";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;

			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			var sailing2 = voyage2.Sailings.GetSailingFromLoadAndDischarge("AUMEL", "USLAX");
			AssertNotNull("Sailing should exist", sailing2);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = shipment_ETD;
			shipment.JS_E_ARV = shipment_ETA;

			var supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ATL: Shipment with NO consol - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Shipment with NO consol - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			var consol1 = shipment.Consols.AddNew();
			var departureTransport1 = consol1.Transports.DepartureTransport;

			departureTransport1.JW_IsLinked = false;
			AssertEquals("DepartureTransport is NOT Linked", false, departureTransport1.JW_IsLinked);
			AssertEquals("DepartureTransport has NO Sailing", null, departureTransport1.Sailing);
			AssertEquals("ATL: Departure Transport is NOT Linked - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Departure Transport is NOT Linked - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			departureTransport1.JW_IsLinked = true;
			departureTransport1.JW_JX = sailing1.PK;
			AssertEquals("DepartureTransport is Linked", true, departureTransport1.JW_IsLinked);
			AssertEquals("DepartureTransport has Sailing", sailing1, departureTransport1.Sailing);
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, departureTransport1.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: Empty", ZDateTime.Empty, departureTransport1.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			origin1.JA_E_ARV = sailing1_E_ATL;
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, departureTransport1.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing1_E_ATL, departureTransport1.Sailing.Origin.JA_E_ARV);
			AssertEquals("Sailing matching shipment's origin found. ATL: A_ATL is Empty - Sailing E_ATL", sailing1_E_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching shipment's origin found. Estimated ATL: A_ATL is Empty - Sailing E_ATL", sailing1_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			origin1.JA_A_ARV = sailing1_A_ATL;
			AssertEquals("Sailing A_ATL: sailing_A_ATL", sailing1_A_ATL, departureTransport1.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing1_E_ATL, departureTransport1.Sailing.Origin.JA_E_ARV);
			AssertEquals("Sailing matching shipment's origin found. ATL: Sailing A_ATL", sailing1_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching shipment's origin found. Estimated ATL: Sailing E_ATL", sailing1_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals("Sailing matching consol's first load port found. ATL: Sailing A_ATL", sailing1_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching consol's first load port found. Estimated ATL: Sailing A_ATL", sailing1_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			consol1.Transports.AddNew();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			AssertEquals("Sailing matching consol's transport load port found. ATL: Sailing A_ATL", sailing1_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching consol's transport load port found. Estimated ATL: Sailing E_ATL", sailing1_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			var consol2 = shipment.Consols.AddNew();
			var departureTransport2 = consol2.Transports.DepartureTransport;

			departureTransport2.JW_IsLinked = true;
			departureTransport2.JW_JX = sailing2.PK;
			origin2.JA_E_ARV = sailing2_E_ATL;
			origin2.JA_A_ARV = sailing2_A_ATL;

			AssertEquals("Sailing A_ATL: sailing_A_ATL", sailing2_A_ATL, departureTransport2.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing2_E_ATL, departureTransport2.Sailing.Origin.JA_E_ARV);
			AssertEquals("Sailing matching shipment's origin found. ATL: Sailing A_ATL", sailing2_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching shipment's origin found. Estimated ATL: Sailing A_ATL", sailing2_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			shipment.JS_RL_NKOrigin = "AUBNE";
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			AssertEquals("Sailing matching consol's first load port found. ATL: Sailing A_ATL", sailing2_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching consol's first load port found. Estimated ATL: Sailing A_ATL", sailing2_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			consol2.Transports.AddNew();
			consol2.JK_RL_NKLoadPort = "AUBNE";
			AssertEquals("Sailing matching consol's transport load port found. ATL: Sailing A_ATL", sailing2_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Sailing matching consol's transport load port found. Estimated ATL: Sailing A_ATL", sailing2_E_ATL, supporter.EstimatedArrivalAtLoadPort);
		}

		public void TestSendingReceivingAgentUsesFirstInternationalConsol()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			CommonConsol localConsol1 = Factory.New<CommonConsol>();
			CommonConsol localConsol2 = Factory.New<CommonConsol>();
			OrgHeader sendingAgent1 = Factory.New<OrgHeader>();
			OrgHeader sendingAgent2 = Factory.New<OrgHeader>();
			OrgHeader receivingAgent1 = Factory.New<OrgHeader>();
			OrgHeader receivingAgent2 = Factory.New<OrgHeader>();

			CommonConsol internationalConsol = Factory.New<CommonConsol>();

			localConsol1.JK_RL_NKLoadPort = "AU";
			localConsol1.JK_RL_NKDischargePort = "AU";
			localConsol1.JK_OA_SendingForwarderAddress = sendingAgent1.MainAddress.PK;
			localConsol1.JK_OA_ReceivingForwarderAddress = receivingAgent1.MainAddress.PK;

			localConsol2.JK_RL_NKLoadPort = "AU";
			localConsol2.JK_RL_NKDischargePort = "AU";
			localConsol2.JK_OA_SendingForwarderAddress = sendingAgent2.MainAddress.PK;
			localConsol2.JK_OA_ReceivingForwarderAddress = receivingAgent2.MainAddress.PK;

			internationalConsol.JK_RL_NKLoadPort = "AU";
			internationalConsol.JK_RL_NKDischargePort = "US";

			shipment.Consols.Add(localConsol1);
			shipment.Consols.Add(localConsol2);

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals(sendingAgent1, supporter.SendingAgent);
			AssertEquals(receivingAgent1, supporter.ReceivingAgent);

			internationalConsol.JK_OA_SendingForwarderAddress = sendingAgent2.MainAddress.PK;
			internationalConsol.JK_OA_ReceivingForwarderAddress = receivingAgent2.MainAddress.PK;

			shipment.Consols.Add(internationalConsol);

			AssertEquals(sendingAgent2, supporter.SendingAgent);
			AssertEquals(receivingAgent2, supporter.ReceivingAgent);
		}

		public void TestIsImport()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";
			Assert("IsImport should be true", consol.IsImport());

			CommonShipment shipment = Factory.New<CommonShipment>();
			AssertNotEquals("Precondition", consol.IsImport(), shipment.IsImport());

			consol.Shipments.Add(shipment);

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("IsImport should be true", consol.IsImport(), supporter.IsImport);
		}

		public void TestActualChargeable()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 200m;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals(200m, supporter.ActualChargeable);
			AssertEquals(Constants.Weight.Kilograms, supporter.ActualChargeableUnit);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			AssertEquals(0.2m, supporter.ActualChargeable);
			AssertEquals(Constants.Volume.CubicMetres, supporter.ActualChargeableUnit);
		}

		public void TestActualChargeableUnit()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals(Constants.Weight.Kilograms, supporter.ActualChargeableUnit);
		}

		public void TestActualLoadingMeters()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_LoadingMeters = 13m;

			AssertEquals("Precondition", true, shipment.IsRoadLoadingMetersEnabled);

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals(13m, supporter.ActualLoadingMeters);

			shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_LoadingMeters = 13m;

			supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("Precondition", false, shipment.IsRoadLoadingMetersEnabled);
			AssertEquals(0m, supporter.ActualLoadingMeters);
		}

		public void TestInvoicingSupporterIncoTerm()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_INCO = Constants.IncoTerms.ExWorks;

			var invoicingSupporter = new CommonShipmentInvoicingSupporter(shipment);
			var paymentTerm = shipment.RatingAdapter.PaymentTerm;
			AssertEquals(Constants.IncoTerms.ExWorks, paymentTerm.GetPaymentTermInfo(CostSell.Revenue).Value);
			AssertEquals(Constants.IncoTerms.ExWorks, invoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue).Value);

			shipment.IsDomesticFreight = true;
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals("PPD", invoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue).Value);

			shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectCOD;
			AssertEquals("FCD", invoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue).Value);
		}

		public void TestShipmentNumberOfColoadMaster()
		{
			CommonShipment normalShipment = Factory.New<CommonShipment>();
			CommonShipment coloadMaster = Factory.New<CommonShipment>();
			CommonShipment subHouseBillShipment = Factory.New<CommonShipment>();

			normalShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			coloadMaster.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			subHouseBillShipment.JS_JS_ColoadMasterShipment = coloadMaster.PK;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(normalShipment);
			AssertEquals("Normal CommonShipment should display empty string", ZString.Empty, supporter.ShipmentNumberOfColoadMaster);

			supporter = new CommonShipmentInvoicingSupporter(coloadMaster);
			AssertEquals("Co-load master should display empty string", ZString.Empty, supporter.ShipmentNumberOfColoadMaster);

			supporter = new CommonShipmentInvoicingSupporter(subHouseBillShipment);
			AssertEquals("Sub house bill CommonShipment should display the number of its Co-load Master", coloadMaster.JS_UniqueConsignRef, supporter.ShipmentNumberOfColoadMaster);
		}

		public void TestMasterBillNumber()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.Consols.Add(consol);

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = ZString.Empty;
			transport.JW_RL_NKDiscPort = ZString.Empty;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("MasterBillNumber", consol.JK_MasterBillNum, supporter.MasterBillNumber);
		}

		public void TestHouseBillNumber()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("HouseBillNumber", shipment.JS_HouseBill, supporter.HouseBillNumber);
		}

		public void TestCreateAccountingJob()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;

			var supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("Precondition", JobInvoicingConsumerTypes.CFSShipment, supporter.ConsumerType);
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			shipment.JS_IsCFSRegistered = false;
			shipment.JS_IsForwardRegistered = true;
			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment, supporter.ConsumerType);
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = "SGSIN";
			AssertEquals("CreateAccountingJobOnSavingOfOperationsJob", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "SGSIN";

			Assert(shipment.IsCrossTrade());

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsDebtor = true;
			shipment.ConsigneePK = consignee.PK;
			AssertEquals("Consignee is a debtor for cross trade and Consignee's delivery bill fall back to Consignee", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);
			consignee.OH_IsDebtor = false;
			AssertEquals("Consignee is NOT a debtor for cross trade and Consignee's delivery bill fall back to Consignee", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			var relatedParty = Factory.New<OrgHeader>();
			relatedParty.OH_Code = "AAA";
			consignee.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			relatedParty.OH_IsDebtor = true;
			AssertEquals("Consignee's delivery bill is a debtor for cross trade but Consignee is NOT", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);
			relatedParty.OH_IsDebtor = false;
			AssertEquals("Consignee and Consignee's delivery bill are both NOT a debtor for cross trade", false, supporter.CreateAccountingJobOnSavingOfOperationsJob);

			consignee.OH_IsDebtor = true;
			AssertEquals("Consignee is a debtor for cross trade but Consignee's delivery bill is NOT", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob_AttachedToImportConsol_ReturnTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<CommonConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUBNE";

				var consignee = Factory.New<OrgHeader>();
				consignee.OH_IsDebtor = false;

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_IsDebtor = false;

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_IsCFSRegistered = false;
				shipment.JS_IsForwardRegistered = true;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				shipment.Consols.Add(consol);

				var supporter = new CommonShipmentInvoicingSupporter(shipment);
				AssertEquals("Shipment is attached to an Import Consol and regardless of parties being flagged as A/R or not, Job should be created automatically", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);
			}
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob_AttachedToExportConsol_ReturnTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<CommonConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";

				var consignee = Factory.New<OrgHeader>();
				consignee.OH_IsDebtor = false;

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_IsDebtor = false;

				var shipment = Factory.New<CommonShipment>();
				shipment.JS_IsCFSRegistered = false;
				shipment.JS_IsForwardRegistered = true;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;
				shipment.Consols.Add(consol);

				var supporter = new CommonShipmentInvoicingSupporter(shipment);
				AssertEquals("Shipment is attached to an Export Consol and regardless of parties being flagged as A/R or not, Job should be created automatically", true, supporter.CreateAccountingJobOnSavingOfOperationsJob);
			}
		}

		public void TestVoyageVesselOrFlightDate()
		{
			var shipment = Factory.New<CommonShipment>();
			var consolSEA = Factory.New<CommonConsol>();
			consolSEA.JK_TransportMode = Constants.TransportModes.Sea;
			consolSEA.Transports[0].JW_VoyageFlight = "TEST54321";
			consolSEA.Transports[0].JW_Vessel = "ENTERPRISE";

			var consolAIR = Factory.New<CommonConsol>();
			consolAIR.JK_TransportMode = Constants.TransportModes.Air;
			consolAIR.Transports[0].JW_VoyageFlight = "TEST12345";

			shipment.Consols.Add(consolAIR);
			shipment.Consols.Add(consolSEA);

			supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("TEST12345, TEST54321/ENTERPRISE", supporter.VoyageVesselOrFlightDate);

			shipment.JS_E_DEP = ZDateTime.BrettsBirthday;
			AssertEquals("TEST12345/18-SEP-71, TEST54321/ENTERPRISE", supporter.VoyageVesselOrFlightDate);
		}

		public void TestBroker()
		{
			OrgHeader importBroker = Factory.New<OrgHeader>();
			OrgHeader exportBroker = Factory.New<OrgHeader>();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OH_ImportBroker = importBroker.PK;
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			AssertEquals("IsImport should be true", true, shipment.IsImport());
			AssertEquals("IsExport should be false", false, shipment.IsExport());

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("Broker should be ImportBroker", importBroker, supporter.Broker);
			AssertEquals("ImportBroker", importBroker, supporter.ImportBroker);
			AssertEquals("ExportBroker", exportBroker, supporter.ExportBroker);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";

			AssertEquals("IsImport should be false", false, shipment.IsImport());
			AssertEquals("IsExport should be true", true, shipment.IsExport());
			AssertEquals("Broker should be ExportBroker", exportBroker, supporter.Broker);
			AssertEquals("ImportBroker", importBroker, supporter.ImportBroker);
			AssertEquals("ExportBroker", exportBroker, supporter.ExportBroker);

			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "USCHI";

			AssertEquals("IsImport should be false", false, shipment.IsImport());
			AssertEquals("IsExport should be false", false, shipment.IsExport());
			AssertEquals("Broker should be null", null, supporter.Broker);
			AssertEquals("ImportBroker", importBroker, supporter.ImportBroker);
			AssertEquals("ExportBroker", exportBroker, supporter.ExportBroker);
		}

		public void TestDefaultChargeGroup()
		{
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, Supporter.DefaultChargeGroup);
		}

		public void TestContainerCount()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
			consol.Containers.AddNew().JC_ContainerCount = 4;
			consol.Containers.AddNew().JC_ContainerCount = 8;

			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_TEU = 10;
			consol.Containers[0].JC_RC = refContainer.PK;

			shipment.OuterPackLines.AddNew().SetContainer(consol.Containers[0].PK);
			shipment.OuterPackLines.AddNew().SetContainer(consol.Containers[1].PK);

			AssertEquals("Precondition", 2, shipment.Containers.Count());
			AssertEquals("Precondition", 12, shipment.JS_Calc_ContainerCount);
			AssertEquals("Precondition", new ZDecimal(4 * 10 + 8), shipment.ContainerTEUCount);

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("ContainerCount", 12, supporter.ContainerCount);
			AssertEquals("TEUCount", new ZDecimal(4 * 10 + 8), supporter.TEUCount);
		}

		public override void TestOuterPackTotal()
		{
			var shipment = Factory.New<CommonShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var innerPackLine = shipment.InnerPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 5;
			innerPackLine.JL_PackageCount = 3;

			var invoicingSupporter = new CommonShipmentInvoicingSupporter(shipment);

			AssertEquals("Should count the packages from the outer packs only", 15, invoicingSupporter.OuterPackTotal);
		}

		public void TestGetOrganisationByBranchDefaultingRule()
		{
			var orgHeaderForArrivalCTO = Factory.New<OrgHeader>();
			var orgHeaderForDepartureCTO = Factory.New<OrgHeader>();
			var orgHeaderForConsolArrivalLocalTransport = Factory.New<OrgHeader>();
			var orgHeaderForConsolDepartureLocalTransport = Factory.New<OrgHeader>();
			var orgHeaderForReceivingAgent = Factory.New<OrgHeader>();
			var orgHeaderForSendingAgent = Factory.New<OrgHeader>();
			var orgHeaderForShipmentDeliveryLocalTransportCompany = Factory.New<OrgHeader>();
			var orgHeaderForShipmentPickupLocalTransportCompany = Factory.New<OrgHeader>();
			var orgHeaderForShipmentExportBroker = Factory.New<OrgHeader>();
			var orgHeaderForShipmentImportBroker = Factory.New<OrgHeader>();
			var orgHeaderForShipmentDeliveryAgent = Factory.New<OrgHeader>();
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_OA_ArrivalCTOAddress = orgHeaderForArrivalCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = orgHeaderForDepartureCTO.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgHeaderForReceivingAgent.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgHeaderForSendingAgent.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = orgHeaderForConsolArrivalLocalTransport.MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = orgHeaderForConsolDepartureLocalTransport.MainAddress.PK;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgHeaderForShipmentDeliveryLocalTransportCompany.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgHeaderForShipmentPickupLocalTransportCompany.MainAddress.PK;
			shipment.JS_OH_ExportBroker = orgHeaderForShipmentExportBroker.PK;
			shipment.JS_OH_ImportBroker = orgHeaderForShipmentImportBroker.PK;
			shipment.JS_OH_DeliveryAgent = orgHeaderForShipmentDeliveryAgent.PK;

			CommonShipmentInvoicingSupporter supporter = new CommonShipmentInvoicingSupporter(shipment);
			AssertEquals("Empty rule", null, supporter.GetOrganisationByBranchDefaultingRule(""));
			AssertEquals("ArrivalCTO rule, but shipment don't have consol", null, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO));

			consol.Shipments.Add(shipment);

			AssertEquals("ArrivalCTO", orgHeaderForArrivalCTO, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO));
			AssertEquals("DepartureCTO", orgHeaderForDepartureCTO, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.DepartureCTO));
			AssertEquals("ConsolArrivalLocalTransport", orgHeaderForConsolArrivalLocalTransport, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ConsolArrivalLocalTransport));
			AssertEquals("ConsolDepartureLocalTransport", orgHeaderForConsolDepartureLocalTransport, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ConsolDepartureLocalTransport));
			AssertEquals("SendingAgent", orgHeaderForSendingAgent, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.SendingAgent));
			AssertEquals("ShipmentDeliveryLocalTransportCompany", orgHeaderForShipmentDeliveryLocalTransportCompany, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ShipmentDeliveryLocalTransportCompany));
			AssertEquals("ShipmentPickupLocalTransportCompany", orgHeaderForShipmentPickupLocalTransportCompany, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ShipmentPickupLocalTransportCompany));
			AssertEquals("ShipmentExportBroker", orgHeaderForShipmentExportBroker, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ShipmentExportBroker));
			AssertEquals("ShipmentImportBroker", orgHeaderForShipmentImportBroker, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker));
			AssertEquals("ReceivingAgent", orgHeaderForReceivingAgent, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ReceivingAgent));
			AssertEquals("ShipmentDeliveryAgent", orgHeaderForShipmentDeliveryAgent, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.DeliveryAgentWithReceivingAgentFallback));
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertEquals("ReceivingAgent", orgHeaderForReceivingAgent, supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.DeliveryAgentWithReceivingAgentFallback));
		}

		[ExpectNoExceptions]
		public void TestGetOrganisationByBranchDefaultingRule_AvoidNRE()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = Factory.New<CommonShipment>();
			var supporter = new CommonShipmentInvoicingSupporter(shipment);
			consol.Shipments.Add(shipment);

			AssertNull("No NRE", supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO));
			AssertNull("No NRE", supporter.GetOrganisationByBranchDefaultingRule(Constants.ChargeCodeBranchDefaultingRule.DepartureCTO));
		}

		#region Implementation

		CommonShipmentInvoicingSupporter Supporter
		{
			get { return supporter ?? (supporter = GetNewBusinessObject().InvoicingSupporter as CommonShipmentInvoicingSupporter); }
		}

		CommonShipmentInvoicingSupporter supporter;

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CommonShipment>();
		}

		#endregion
	}
}
