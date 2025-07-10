using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class ConsolCollectionTest : BaseFreightTest
	{
		public void TestRemove()
		{
			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", Constants.ShipmentTypes.StandardHouse, Factory);

			shipment.Consols.Add(consol);
			FreightTestHelper.AssertConsolCollection(shipment.Consols, consol);
			FreightTestHelper.AssertShipmentCollection(consol.Shipments, shipment);

			shipment.Consols.Remove(consol);
			FreightTestHelper.AssertConsolCollection(shipment.Consols);
			FreightTestHelper.AssertShipmentCollection(consol.Shipments);
		}

		[ExpectNoExceptions]
		public void TestRemoveAndDeleteAll()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
			Factory.Save();

			shipment.Consols.RemoveAndDeleteAll();
		}

		public void TestSetShipmentCFSFlagWhenAddingToACFSConsol()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonConsol newConsol = newFactory.New<CommonConsol>();
			newConsol.JK_IsCFS = true;
			newFactory.Save();

			// setting JK_IsCFS will hit the Shipments collection so we have to load in a different collection.
			CommonConsol consol = Factory.Load<CommonConsol>(newConsol.PK);
			CommonShipment shipment = Factory.New<CommonShipment>();

			AssertEquals("precondition: Consol should be CFS", true, consol.JK_IsCFS);
			AssertEquals("precondition: CommonShipment should not be CFS yet", false, shipment.JS_IsCFSRegistered);

			shipment.Consols.Add(consol);
			AssertEquals("CommonShipment should now be CFS Registered", true, shipment.JS_IsCFSRegistered);
		}

		public void TestSetDefaultsForNewChild()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			CommonConsol consol = shipment.Consols.AddNew();

			AssertEquals(shipment.JS_TransportMode, consol.JK_TransportMode);
			AssertEquals(shipment.JS_RL_NKOrigin, consol.JK_JX_JA_RL_NKPortOfLoading);
			AssertEquals(shipment.JS_RL_NKDestination, consol.JK_JX_JB_RL_NKPortOfDischarge);

			AssertEquals(1, consol.Transports.Count);
			AssertEquals(shipment.JS_TransportMode, consol.Transports[0].JW_TransportMode);
			AssertEquals(Constants.TransportPlanningType.MainVessel, consol.Transports[0].JW_TransportType);
			AssertEquals(shipment.JS_RL_NKOrigin, consol.Transports[0].JW_RL_NKLoadPort);
			AssertEquals(shipment.JS_RL_NKDestination, consol.Transports[0].JW_RL_NKDiscPort);
		}

		public void TestSetDefaultsForNewChildCarrier()
		{
			var org = Factory.New<OrgHeader>();
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_BookedShippingLineAddress = address.PK;
			CommonConsol consol = shipment.Consols.AddNew();

			AssertEquals(consol.JK_OA_ShippingLineAddress, address.PK);
			AssertEquals(shipment.JS_OA_BookedShippingLineAddress, address.PK);
		}

		public void TestSetDefaultsForNewChild_LCLShipments()
		{
			AssertConsolCreatedWithConsolMode("Consol mode should be the same as shipment packing mode for sea LCL", Constants.ContainerModes.LCL, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			AssertConsolCreatedWithConsolMode("Consol mode should be FCL if shipment packing mode for sea is Other", Constants.ContainerModes.Other, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertConsolCreatedWithConsolMode("Consol mode should be the same as shipment packing mode when not LCL or Other for sea transport", Constants.ContainerModes.LTL, Constants.TransportModes.Sea, Constants.ContainerModes.LTL);
			AssertConsolCreatedWithConsolMode("Consol mode should be the same as shipment packing mode for non sea transport", Constants.ContainerModes.LTL, Constants.TransportModes.Air, Constants.ContainerModes.LTL);
		}

		public void TestSetDefaultsForNewChild_FromStandaloneShipment_ExportDepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Export = true;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			var shipment = Factory.New<CommonShipment>();
			AssertEquals("Pre-condition: AUBNE", "AUBNE", shipment.JS_RL_NKOrigin);

			shipment.JS_RL_NKOrigin = "INDEL";
			var consol = shipment.Consols.AddNew();

			AssertEquals("Port is set from Shipment's origin, not home port", "INDEL", consol.JK_RL_NKLoadPort);
		}

		public void TestSetDefaultsForNewChild_FromStandaloneShipment_ImportDepartment()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			var shipment = Factory.New<CommonShipment>();
			AssertEquals("Pre-condition: AUBNE", "AUBNE", shipment.JS_RL_NKDestination);

			shipment.JS_RL_NKDestination = "INDEL";
			var consol = shipment.Consols.AddNew();

			AssertEquals("Port is set from shipment's destination, not home port", "INDEL", consol.JK_RL_NKDischargePort);
		}

		public void TestSetDefaultsForAttachedChildCarrier_DoesNotDefaultWhenShippingLineIsAlreadyPopulated()
		{
			var org1 = Factory.New<OrgHeader>();
			var address1 = Factory.New<OrgAddress>();
			address1.OA_OH = org1.PK;

			var org2 = Factory.New<OrgHeader>();
			var address2 = Factory.New<OrgAddress>();
			address2.OA_OH = org2.PK;

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_BookedShippingLineAddress = address1.PK;

			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = address2.PK;
			shipment.Consols.Add(consol);

			AssertEquals(consol.JK_OA_ShippingLineAddress, address2.PK);
			AssertEquals(shipment.JS_OA_BookedShippingLineAddress, address1.PK);
		}

		void AssertConsolCreatedWithConsolMode(string message, ZString shipmentPackingMode, ZString shipmentTransportMode, string expected)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = shipmentTransportMode;
			shipment.JS_PackingMode = shipmentPackingMode;
			var consol = shipment.Consols.AddNew();
			AssertEquals(message, expected, consol.JK_ConsolMode);
		}

		public void TestIndexer()
		{
			ConsolCollection consols = CommonShipment.New(Factory).Consols;

			CommonConsol consol1 = consols.AddNew();
			AssertEquals(consol1, consols[0]);

			CommonConsol consol2 = consols.AddNew();
			AssertEquals(consol2, consols[1]);
		}

		[ExpectNoExceptions]
		public void TestUniqueOriginAndDestinationWithValidTwoValidDestinationsNoOrigin()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			CommonConsol consol2 = shipment.Consols.AddNew();

			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKDiscPort = "AUSYD";

			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKDiscPort = "AUSYD";

			shipment.Consols.CheckUniqueOriginAndDestination();
		}

		public void TestShipmentETADefaultsToConsolETAPlusDeliveryTimeWhenAttached()
		{
			ZDateTime dateOfArrival = new ZDateTime(2005, 4, 14);

			GlbPortDeliveryTime seaDefaultDelay = Factory.New<GlbPortDeliveryTime>();
			seaDefaultDelay.G1_FreightMode = Constants.TransportModes.Sea;
			seaDefaultDelay.G1_RL_NKDischargePort = "AUMEL";
			seaDefaultDelay.G1_RL_NKDestinationPort = "AUBNE";
			seaDefaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;

			CommonShipment testShipment = CommonShipment.New(Factory);
			testShipment.JS_TransportMode = Constants.TransportModes.Sea;
			testShipment.ConsigneePK = LocalConsignee.PK;
			testShipment.JS_RL_NKOrigin = "HKHKG";
			testShipment.JS_RL_NKDestination = "AUBNE";
			testShipment.JS_E_DEP = dateOfArrival.AddDays(-10);
			testShipment.JS_E_ARV = dateOfArrival.AddDays(-4);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Combination;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUMEL";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "123";
			consol.JK_MasterBillNum = "11111";
			transport.JW_Vessel = "vesselname";
			transport.JW_ETD = dateOfArrival.AddDays(-9);
			transport.JW_ETA = dateOfArrival;

			Factory.Save();

			testShipment.Consols.Add(consol);
			AssertEquals("ShipmentETA should be Consol ETA + PortDeliveryTime", dateOfArrival.AddDays(3), testShipment.JS_E_ARV);
		}

		public void TestShipmentRelatedPartiesDefaultToConsolReceivingAndSendingAgents()
		{
			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty2 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty3 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty4 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty5 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty6 = Factory.NewWithValidTestData<OrgHeader>();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "ABC Co.";
			consignee.SetRelatedParty(relatedParty1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignee.SetRelatedParty(relatedParty2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "SDF Co.";
			consignor.SetRelatedParty(relatedParty3, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignor.SetRelatedParty(relatedParty4, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var consol = shipment.Consols.AddNew();

			AssertEquals("Consignee related party agent should default to receiving agent on the consol", consol.JK_OA_ReceivingForwarderAddress, relatedParty1.MainAddress.PK);
			AssertEquals("Consignor related party agent should default to sending agent on the consol", consol.JK_OA_SendingForwarderAddress, relatedParty4.MainAddress.PK);

			relatedParty1.Delete();
			relatedParty4.Delete();
			consignee.SetRelatedParty(relatedParty2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignee.SetRelatedParty(relatedParty3, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignor.SetRelatedParty(relatedParty5, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignor.SetRelatedParty(relatedParty6, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			consol = shipment.Consols.AddNew();

			AssertEquals("Consignee related party agent should default to receiving agent on the consol", consol.JK_OA_ReceivingForwarderAddress, relatedParty3.MainAddress.PK);
			AssertEquals("Consignor related party agent should default to sending agent on the consol", consol.JK_OA_SendingForwarderAddress, relatedParty6.MainAddress.PK);
		}

		public void TestSetReceivingAndSendingAgents_WhenRelatedParties_Empty()
		{
			var relatedParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty2 = Factory.NewWithValidTestData<OrgHeader>();

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;

			OrgAppointedAgentPorts newAppAgent = sendingAgent.AppointedAgentPorts.AddNew();
			newAppAgent.O5_PortOrCountry = "NZAKL";
			newAppAgent.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			newAppAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			newAppAgent.O5_AirAgentStatus = "APP";

			sendingAgent.Factory.Save();

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_IsForwarder = true;

			OrgAppointedAgentPorts newAppAgent1 = receivingAgent.AppointedAgentPorts.AddNew();
			newAppAgent1.O5_PortOrCountry = "AUSYD";
			newAppAgent1.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			newAppAgent1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			newAppAgent1.O5_AirAgentStatus = "APP";

			receivingAgent.Factory.Save();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "ABC Co.";
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "SDF Co.";

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_TransportMode = "AIR";

			var consol = shipment.Consols.AddNew();

			AssertEquals("Port agents should default to receiving agent on the consol", consol.JK_OA_ReceivingForwarderAddress, receivingAgent.MainAddress.PK);
			AssertEquals("Port agents should default to sending agent on the consol", consol.JK_OA_SendingForwarderAddress, sendingAgent.MainAddress.PK);

			consignee.SetRelatedParty(relatedParty1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(relatedParty2, RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);

			consol = shipment.Consols.AddNew();

			AssertEquals("Consignee related party agent should default to receiving agent on the consol", consol.JK_OA_ReceivingForwarderAddress, relatedParty1.MainAddress.PK);
			AssertEquals("Consignor related party agent should default to sending agent on the consol", consol.JK_OA_SendingForwarderAddress, relatedParty2.MainAddress.PK);
		}

		#region Matching Related Parties

		public void TestCheckNonMatchingReceivingAgentWithShipmentRelatedParties()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol1.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol2.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var warningMessage = "message";
				helper
					.SetupSequence(m => m.CheckRelatedReceivingAgents(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(warningMessage)
					.Returns(string.Empty);

				shipment.Consols.Add(consol1);
				AssertHasRowWarningContaining(consol1, warningMessage);
				shipment.Consols.Add(consol2);
				AssertNoRowWarningContaining(consol2, "message");
			}
		}

		public void TestCheckNonMatchingSendingAgentWithShipmentRelatedParties()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol1.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol2.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			Factory.Save();

			var helper = new Mock<IShipmentVsConsolMessageHelper>();

			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var warningMessage = "message";
				helper
					.SetupSequence(m => m.CheckRelatedSendingAgents(It.IsAny<IEnumerable<CommonShipment>>(),
						It.IsAny<IEnumerable<CommonConsol>>()))
					.Returns(warningMessage)
					.Returns(string.Empty);

				shipment.Consols.Add(consol1);
				AssertHasRowWarningContaining(consol1, warningMessage);
				shipment.Consols.Add(consol2);
				AssertNoRowWarningContaining(consol2, "message");
			}
		}

		#endregion

		public void TestShipmentETDeliveryDefaultsToShipmentETAPlusDeliveryTimeWhenAttached()
		{
			ZDateTime dateOfArrival = new ZDateTime(2005, 4, 14);

			GlbPortDeliveryTime seaDefaultDelay = Factory.New<GlbPortDeliveryTime>();
			seaDefaultDelay.G1_FreightMode = Constants.TransportModes.Sea;
			seaDefaultDelay.G1_RL_NKDischargePort = "AUMEL";
			seaDefaultDelay.G1_RL_NKDestinationPort = "AUBNE";
			seaDefaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;
			seaDefaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 2;

			CommonShipment testShipment = CommonShipment.New(Factory);
			testShipment.JS_TransportMode = Constants.TransportModes.Sea;
			testShipment.ConsigneePK = LocalConsignee.PK;
			testShipment.JS_RL_NKOrigin = "HKHKG";
			testShipment.JS_RL_NKDestination = "AUBNE";
			testShipment.JS_E_DEP = dateOfArrival.AddDays(-10);
			testShipment.JS_E_ARV = dateOfArrival.AddDays(-4);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Combination;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUMEL";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "123";
			consol.JK_MasterBillNum = "11111";
			transport.JW_Vessel = "vesselname";
			transport.JW_ETD = dateOfArrival.AddDays(-9);
			transport.JW_ETA = dateOfArrival;

			Factory.Save();

			testShipment.Consols.Add(consol);
			AssertEquals("CommonShipment Est. Delivery should be CommonShipment ETA + PortDeliveryTime", testShipment.JS_E_ARV.AddDays(2), testShipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		public void TestUniqueOriginAndDestination()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			string origin1 = HomePort;
			string origin2 = AlternateHomePort;
			string destination1 = OverseasPort;
			string destination2 = OverseasPort2;

			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = destination1;
			consol1.JK_RL_NKLoadPort = origin1;

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKDischargePort = destination2;
			consol2.JK_RL_NKLoadPort = origin2;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("No duplicates: HasErrors", false, shipment.Consols.HasErrors());

			CommonConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_RL_NKDischargePort = destination1;
			consol3.JK_RL_NKLoadPort = origin1;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("Add duplicate: HasErrors.", true, shipment.Consols.HasErrors());

			shipment.Consols.Remove(consol3);
			AssertEquals("Remove duplicate: HasErrors", false, shipment.Consols.HasErrors());
		}

		public void TestUniqueOriginAndDestinationIgnoreSameLoadDiscPort()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			string origin1 = HomePort;
			string origin2 = AlternateHomePort;
			string destination1 = OverseasPort;
			string destination2 = OverseasPort2;

			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = destination1;
			consol1.JK_RL_NKLoadPort = origin1;

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKDischargePort = destination2;
			consol2.JK_RL_NKLoadPort = origin2;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("No duplicates: HasErrors", false, shipment.Consols.HasErrors());

			CommonConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = Constants.TransportModes.Road;
			consol3.JK_ConsolMode = Constants.ContainerModes.FTL;
			consol3.JK_RL_NKDischargePort = origin2;
			consol3.JK_RL_NKLoadPort = origin2;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("No duplicates: HasErrors.", false, shipment.Consols.HasErrors());
		}

		public void TestUniqueOriginAndDestinationIgnoreRailOrRoadTransportMode()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = OverseasPort;
			consol1.JK_RL_NKLoadPort = HomePort;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Constants.TransportModes.Road;
			consol2.JK_ConsolMode = Constants.ContainerModes.FTL;
			consol2.JK_RL_NKDischargePort = HomePort;
			consol2.JK_RL_NKLoadPort = AlternateHomePort;
			consol2.IsDomesticFreight = true;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("No duplicates: HasErrors", false, shipment.Consols.HasErrors());

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = Constants.TransportModes.Road;
			consol3.JK_ConsolMode = Constants.ContainerModes.FTL;
			consol3.JK_RL_NKDischargePort = HomePort;
			consol3.JK_RL_NKLoadPort = AlternateHomePort;
			consol3.IsDomesticFreight = true;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("No duplicates: HasErrors.", false, shipment.Consols.HasErrors());

			consol3.JK_TransportMode = Constants.TransportModes.Rail;
			consol3.JK_ConsolMode = Constants.ContainerModes.Bulk;

			shipment.Consols.CheckUniqueOriginAndDestination();
			AssertEquals("No duplicates: HasErrors.", false, shipment.Consols.HasErrors());
		}

		public void TestUniqueOriginAndDestination_ClearsErrorsOnConsolLoadOrDischargePortChanged()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "";
			shipment.JS_RL_NKDestination = "";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			string origin1 = HomePort;
			string origin2 = AlternateHomePort;
			string destination1 = OverseasPort;
			string destination2 = OverseasPort2;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = origin1;
			consol1.JK_RL_NKDischargePort = destination1;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = origin1;
			consol2.JK_RL_NKDischargePort = destination1;

			AssertEquals("Precondition - shipment.Consols: HasErrors", true, shipment.Consols.HasErrors());
			AssertEquals("Precondition - consol1: HasErrors", true, consol1.HasErrors);
			AssertEquals("Precondition - consol2: HasErrors", true, consol2.HasErrors);

			consol2.JK_RL_NKLoadPort = origin2;
			consol2.JK_RL_NKDischargePort = destination2;

			AssertEquals("After Consol Change - shipment.Consols: No Errors", false, shipment.Consols.HasErrors());
			AssertEquals("After Consol Change - consol1: No Errors", false, consol1.HasErrors);
			AssertEquals("After Consol Change - consol2: No Errors", false, consol2.HasErrors);
		}

		public void TestAddingBizOFireOnConsolChangedReplacementEventHandler()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			TestClassInterestedInConsolChangeEvent testClass = new TestClassInterestedInConsolChangeEvent(shipment);

			CommonConsol consol1 = Factory.New<CommonConsol>();
			shipment.Consols.Add(consol1);
			AssertEquals("TestClass gets notifed the event", 1, testClass.EventFiredCount);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			shipment.Consols.Add(consol2);
			AssertEquals("TestClass gets notifed the event", 2, testClass.EventFiredCount);
		}

		protected class TestClassInterestedInConsolChangeEvent
		{
			public TestClassInterestedInConsolChangeEvent(CommonShipment shipment)
			{
				shipment.Consols.CountChanged += new CollectionCountChangedEventHandler(Consols_CountChanged);
			}

			public int EventFiredCount;
			void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
			{
				EventFiredCount++;
			}
		}

		public void TestAddDefaultOuterPack()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertEquals("No default packline on new shipment.", 0, shipment.OuterPackLines.Count);

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport1 = consol1.Transports[0];
			transport1.JW_JX = new SailingsForTestClasses(Factory).SydLaxSailing.PK;

			shipment.Consols.Add(consol1);
			AssertEquals("Add consol with no containers.", 0, shipment.OuterPackLines.Count);

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = new SailingsForTestClasses(Factory).MelSydSailing.PK;

			CommonContainer container1 = consol2.Containers.AddNew();
			shipment.Consols.Add(consol2);
			shipment.OuterPackLines.CurrentConsol = consol2;
			shipment.JS_OuterPacks = 4;
			AssertEquals("Add consol with container.", 1, shipment.OuterPackLines.Count);
			AssertEquals("Default packline should be allocated to container.", container1, shipment.OuterPackLines[0].GetContainer(consol2));
		}

		public void TestConsolGetsAddedIntoShipmentConsolsCollection()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("CommonShipment count", 0, consol.Shipments.Count);

			CommonShipment shipment = consol.Shipments.AddNew();
			AssertEquals("CommonShipment count", 1, consol.Shipments.Count);

			AssertEquals("Consol in Shipment.Consols", consol, shipment.Consols[0]);
			AssertEquals("Shipment in Consol.Shipments", shipment, consol.Shipments[0]);
		}

		public void TestDetachingConsolWithContainer()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			CommonConsol consol1 = shipment.Consols.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT-ONE";

			PackLine outerPack1 = shipment.OuterPackLines.AddNew();
			outerPack1.CurrentConsol = consol1;
			outerPack1.SetContainer(consol1, container1);
			Factory.Save();
			AssertEquals("PackContainerPivot should be added.", 1, outerPack1.Containers.Count);

			shipment.Consols.Remove(consol1);
			AssertEquals("PackLine should not be removed when Consol is detached.", 1, shipment.OuterPackLines.Count);
		}

		public void TestDetachingConsolWithContainersWithPackLines()
		{
			//Create a Consol with a Container
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";
			//Create a Shipmnet with an Outer Packline and attach the Consol
			CommonShipment shipment1 = CommonShipment.New(Factory);
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_ActualWeight = 100;
			shipment1.Consols.Add(consol);
			//Create another CommonShipment and attach to same Consol
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_ActualWeight = 200;

			AssertEquals("Container should be allocated to Shipment1", 1, shipment1.OuterPackLines[0].Containers.Count);
			AssertEquals("Container should be allocated to Shipment2", 1, shipment2.OuterPackLines[0].Containers.Count);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			shipment1.Consols.Remove(consol);
			Factory.Save();
			AssertEquals("Shipment1 Packline should be removed from Container", 0, shipment1.OuterPackLines[0].Containers.Count);
			AssertEquals("Shipment2 Packline should not be removed from Container", 1, shipment2.OuterPackLines[0].Containers.Count);
		}

		public void TestDetachingConsolWithContainersWithPackLines_NoCorrespondingJobConShipLinkException()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234001";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT1234002";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_OuterPacks = 1;
			shipment.JS_ActualWeight = 100;
			factory2.Save();

			var newShipment = Factory.Load<CommonShipment>(shipment.PK);
			consol.Shipments.Add(newShipment);
			Factory.Save();

			shipment.OuterPackLines[0].JL_JC = container1.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 2000;
			packLine2.JL_ActualVolume = 2;
			shipment.OuterPackLines[1].JL_JC = container2.PK;

			consol.Shipments.Remove(newShipment);
			Factory.Save();

			factory2.Save();
			Assert("NoCorrespondingJobConShipLink exception is not risen during detaching shipment from consol.", ErrorReporter.TotalErrorCount == 0);
		}

		public void TestAdjustShipmentDatesOnAdded()
		{
			ZDateTime today = ZDateTime.Today;
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort3;
			transport.JW_ETD = today;
			transport.JW_ETA = today.AddDays(7);

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort3;
			shipment.JS_E_DEP = today.AddDays(1);
			shipment.JS_E_ARV = today.AddDays(5);
			shipment.Consols.Add(consol);

			AssertEquals("ETD should be adjusted to match consol", today, shipment.JS_E_DEP);
			AssertEquals("ETA should be adjusted to match consol.", today.AddDays(7), shipment.JS_E_ARV);
		}

		public void TestConsolLoadOrDischargePortChanged()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.Consols.ConsolLoadOrDischargePortChanged += new EventHandler(OnConsols_ConsolLoadOrDischargePortChanged);

			AssertEquals("Load or discharge port hasn't changed initially", false, fLoadOrDischargePortChanged);

			CommonConsol consol1 = shipment.Consols.AddNew();
			CommonConsol consol2 = shipment.Consols.AddNew();

			AssertEquals("Load or discharge port still hasn't changed on an adds", false, fLoadOrDischargePortChanged);

			fLoadOrDischargePortChanged = false;
			consol1.JK_RL_NKLoadPort = "LOAD";
			AssertEquals("Load port changed", true, fLoadOrDischargePortChanged);

			fLoadOrDischargePortChanged = false;
			consol2.JK_RL_NKDischargePort = "DISC";
			AssertEquals("Discharge port changed", true, fLoadOrDischargePortChanged);

			fLoadOrDischargePortChanged = false;
			shipment.Consols.Remove(consol1);
			consol1.JK_RL_NKLoadPort = "LOAD2";
			consol1.JK_RL_NKDischargePort = "DISC2";
			AssertEquals("Load port changed on removed consol, no event should be raised", false, fLoadOrDischargePortChanged);

			fLoadOrDischargePortChanged = false;
			consol2.JK_RL_NKLoadPort = "LOAD4";
			AssertEquals("Load port changed on consol that is still there, event should be raised", true, fLoadOrDischargePortChanged);
		}

		bool fLoadOrDischargePortChanged;
		void OnConsols_ConsolLoadOrDischargePortChanged(object sender, EventArgs e)
		{
			fLoadOrDischargePortChanged = true;
		}

		public void TestEarliestAndLatestConsol()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentsReference = "CONSOL1";
			Transport transport1 = consol1.Transports[0];
			transport1.JW_ETD = new ZDateTime(2014, 1, 3);
			transport1.JW_ETA = new ZDateTime(2014, 1, 3);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentsReference = "CONSOL2";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_ETD = new ZDateTime(2014, 1, 2);
			transport2.JW_ETA = new ZDateTime(2014, 1, 2);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_AgentsReference = "CONSOL3";
			Transport transport3 = consol3.Transports[0];
			transport3.JW_ETD = new ZDateTime(2014, 1, 2);
			transport3.JW_ETA = new ZDateTime(2014, 1, 3);

			var shipment = Factory.New<CommonShipment>();
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			AssertEquals("Earliest consol", "CONSOL2", shipment.Consols.GetEarliestConsol().JK_AgentsReference);
			AssertEquals("Latest consol", "CONSOL1", shipment.Consols.GetLatestConsol().JK_AgentsReference);
		}

		public void TestLatestAndEarliestConsol()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_AgentsReference = "CONSOL1";
			Transport transport1 = consol1.Transports[0];
			transport1.JW_ETD = new ZDateTime(2014, 1, 2);
			transport1.JW_ETA = new ZDateTime(2014, 1, 3);

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_AgentsReference = "CONSOL2";
			Transport transport2 = consol2.Transports[0];
			transport2.JW_ETD = new ZDateTime(2014, 1, 3);
			transport2.JW_ETA = new ZDateTime(2014, 1, 4);

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_AgentsReference = "CONSOL3";
			Transport transport3 = consol3.Transports[0];
			transport3.JW_ETD = new ZDateTime(2014, 1, 1);
			transport3.JW_ETA = new ZDateTime(2014, 1, 1);

			var shipment = Factory.New<CommonShipment>();
			shipment.Consols.Add(consol1);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			AssertEquals("Earliest consol", "CONSOL3", shipment.Consols.GetEarliestConsol().JK_AgentsReference);
			AssertEquals("Latest consol", "CONSOL2", shipment.Consols.GetLatestConsol().JK_AgentsReference);
		}
	}
}
