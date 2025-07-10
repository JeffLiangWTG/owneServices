using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ConsolPortMessagingManager))]
	sealed class ConsolPortMessagingManagerTest : PortMessagingManagerTest
	{
		public void TestValidateContainers_MoreThanOneFCLShipmentsAttachedToContainer()
		{
			var expectedError = "CON00: A container is not permitted to have multiple FCL shipments attached.";

			var consol = CreateValidConsol();

			var shipment = consol.Shipments[0];
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var portMessaging = new ConsolPortMessagingManager(consol);
			AssertNotEquals("There is only one FCL Shipment attached", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			var newShipment = consol.Shipments.AddNew();
			newShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			newShipment.JS_TransportMode = Constants.TransportModes.Sea;
			newShipment.JS_SZB = "777";
			newShipment.OuterPackLines.AddNew();

			AssertEquals("There are more than one FCL Shipments attached", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestValidateContainers_OneFCLShipmentOnGRPOrLCLContainer()
		{
			var expectedError = "CON00: LCL or GRP container modes are not permitted to have FCL shipments attached.";

			var consol = CreateValidConsol();

			var shipment = consol.Shipments[0];
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var portMessaging = new ConsolPortMessagingManager(consol);

			AssertNotEquals("Should not return this message as the Container's Container Mode is not GRP or LCL",
				expectedError,
				portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			var container = consol.Containers[0];
			container.JC_ContainerMode = Constants.ContainerModes.LCL;

			AssertEquals("Should return this message as the Container's Container Mode is LCL and there is one attached packline which master shipment's pack mode is FCL",
				expectedError,
				portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			container.JC_ContainerMode = Constants.ContainerModes.Groupage;

			AssertEquals("Should return this message as the Container's Container Mode is GRP and there is one attached packline which master shipment's pack mode is FCL",
				expectedError,
				portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			AssertNotEquals("Should not return this message as there is none attached packlines which master shipment's pack mode is FCL",
				expectedError,
				portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestValidateContainers_NoPackLines()
		{
			var expectedError = "CON00: No packlines were found. Please allocate packlines to this container.";

			var consol = CreateValidConsol();
			var portMessaging = new ConsolPortMessagingManager(consol);

			AssertNotEquals("There is one packline", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			consol.Shipments[0].OuterPackLines.RemoveAndDeleteAll();
			AssertEquals("No packlines", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		ForwardingConsol CreateValidConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON00";
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			var sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_SZB = "666";

			shipment.OuterPackLines.AddNew();

			return consol;
		}

		public void TestCheckSenderCode()
		{
			var expectedError = "Cannot send message while the Sending Forwarder's Dakosy Participant Code is missing. Enter the Dakosy Participant Code on Organization -> Config -> Registration Numbers / Codes.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;

			var portMessaging = new ConsolPortMessagingManager(consol);
			AssertEquals("Sending Forwarder has no SenderCode", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			AssertNotEquals("Sending Forwarder has SenderCode", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestCheckUNDGDataItems()
		{
			const string expectedError = "Multiple Dangerous Goods are recorded against a pack line on these shipments S00001011, S00001012, please enter Package Count and Package Type for each DG recorded via Shipment > Packing > Pack line > Right click menu > Dangerous Goods";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.JK_TransportMode = "SEA";

			var sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001011";
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_SZB = "1111111";

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var undg1 = packLine1.UNDGs.AddNew();
			undg1.DI_PackageCount = 2;
			undg1.DI_F3_NKPackType = "BAG";

			var undg2 = packLine1.UNDGs.AddNew();
			undg2.DI_PackageCount = 0;
			undg2.DI_F3_NKPackType = string.Empty;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001012";
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_SZB = "2222222";

			var packLine2 = shipment2.OuterPackLines.AddNew();
			var undg3 = packLine2.UNDGs.AddNew();
			undg3.DI_PackageCount = 2;
			undg3.DI_F3_NKPackType = "BAG";

			var undg4 = packLine2.UNDGs.AddNew();
			undg4.DI_PackageCount = 0;
			undg4.DI_F3_NKPackType = string.Empty;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00001013";
			shipment3.JS_TransportMode = "SEA";
			shipment3.JS_SZB = "3333333";

			var packLine3 = shipment3.OuterPackLines.AddNew();
			var undg5 = packLine3.UNDGs.AddNew();
			undg5.DI_PackageCount = 2;
			undg5.DI_F3_NKPackType = "BAG";

			var portMessaging = new ConsolPortMessagingManager(consol);
			AssertEquals("Pack count and type are empty on undg2 and undg4.", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			undg2.DI_PackageCount = 5;
			undg2.DI_F3_NKPackType = "BAG";

			undg4.DI_PackageCount = 15;
			undg4.DI_F3_NKPackType = "BAG";

			AssertNotEquals("Pack count and type are not empty on these UNDG items.", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestCheckUNDGItems_SingleUNDG()
		{
			const string expectedError = "Multiple Dangerous Goods are recorded against a pack line on these shipments S00001011, S00001012, please enter Package Count and Package Type for each DG recorded via Shipment > Packing > Pack line > Right click menu > Dangerous Goods";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";

			var sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001011";
			shipment1.JS_TransportMode = "SEA";
			shipment1.JS_SZB = "1111111";

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var undg1 = packLine1.UNDGs.AddNew();
			undg1.DI_PackageCount = 2;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001012";
			shipment2.JS_TransportMode = "SEA";
			shipment2.JS_SZB = "2222222";

			var packLine2 = shipment2.OuterPackLines.AddNew();
			var undg2 = packLine2.UNDGs.AddNew();
			undg2.DI_PackageCount = 2;

			var portMessaging = new ConsolPortMessagingManager(consol);
			AssertNotEquals("Single dangerous goods item should not prevent sending message.", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestHasDG()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			var undg = packLine.UNDGs.AddNew();

			var manager = new ConsolPortMessagingManager(consol);
			Assert(!manager.HasDG);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "DG";
			subs.DG_Variant = "1";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.LinkDefault(subs);
			Assert(manager.HasDG);
		}

		public void TestIsExport()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var manager = new ConsolPortMessagingManager(consol);
			Assert(!manager.IsExport);

			consol.JK_RL_NKLoadPort = "DEHAM";
			Assert(manager.IsExport);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			Assert(!manager.IsExport);

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			Assert(!manager.IsExport);

			transport.JW_RL_NKLoadPort = "DEHAM";
			Assert(manager.IsExport);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			Assert(!manager.IsExport);
		}

		public void TestIsImport()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var manager = new ConsolPortMessagingManager(consol);
			Assert(!manager.IsImport);

			consol.JK_RL_NKDischargePort = "DEHAM";
			Assert(manager.IsImport);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			Assert(!manager.IsImport);

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "GBLON";
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			Assert(!manager.IsImport);

			transport.JW_RL_NKDiscPort = "DEHAM";
			Assert(manager.IsImport);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			Assert(!manager.IsImport);
		}

		public void TestSZBProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_SZB = "FOO";
			consol.JK_SZBInformation = "BAR";
			consol.JK_SZBIssueDate = new ZDateTime(2013, 10, 31);

			var manager = new ConsolPortMessagingManager(consol);
			AssertEquals("FOO", manager.SZBNumber);
			AssertEquals("BAR", manager.SZBInformation);
			AssertEquals(new ZDateTime(2013, 10, 31), manager.SZBIssueDate);
		}

		public void TestShouldShowPortMessagingForDakosy()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manager = new ConsolPortMessagingManager(consol);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, manager.ShouldShowPortMessagingForDakosy);

			consol.JK_RL_NKLoadPort = "DEFRA";
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			AssertEquals(true, manager.ShouldShowPortMessagingForDakosy);

			consol.JK_RL_NKDischargePort = "DEFRA";
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, manager.ShouldShowPortMessagingForDakosy);

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			transport.JW_RL_NKDiscPort = "DEHAM";
			AssertEquals(true, manager.ShouldShowPortMessagingForDakosy);
		}

		public void TestCheckPortMessagingAvailability()
		{
			string message = "Port Order messages to DAKOSY can only be sent for Consols linked to a Sea transport leg to or from Hamburg (DEHAM).";
			AssertEquals("Pre-condition: security setting should be on by default", true, Env.Security.PortMessaging.IsAllowed);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var manager = new ConsolPortMessagingManager(consol);
			consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals(message, manager.CheckPortMessagingAvailability());

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, manager.CheckPortMessagingAvailability());

			consol.JK_RL_NKLoadPort = "DEFRA";
			AssertEquals(message, manager.CheckPortMessagingAvailability());

			consol.JK_RL_NKDischargePort = "DEHAM";
			AssertEquals(ZString.Empty, manager.CheckPortMessagingAvailability());

			consol.JK_RL_NKDischargePort = "GBLON";
			consol.Transports.AddNew().JW_RL_NKLoadPort = "NLAMS";
			AssertEquals(message, manager.CheckPortMessagingAvailability());

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals(ZString.Empty, manager.CheckPortMessagingAvailability());

			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "DEHAM";
			AssertEquals(ZString.Empty, manager.CheckPortMessagingAvailability());

			Env.Security.PortMessaging.IsAllowed = false;
			Factory.Save();

			AssertEquals("Expected security warning as user does not have permission to view or edit port messages", Env.Security.PortMessaging.ErrorMessageForNotAllowed, manager.CheckPortMessagingAvailability());

			consol.JK_TransportMode = Constants.TransportModes.Air;

			AssertEquals("Expected to still display security warning, regardless of the consol being invalid", Env.Security.PortMessaging.ErrorMessageForNotAllowed, manager.CheckPortMessagingAvailability());
		}

		public void TestIsValidConsolForDakosyPortMessaging()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(consol));

			consol.JK_RL_NKLoadPort = "DEFRA";
			AssertEquals(false, ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(consol));

			consol.JK_RL_NKDischargePort = "DEHAM";
			AssertEquals(true, ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(consol));

			consol.JK_RL_NKDischargePort = "GBLON";
			consol.Transports.AddNew().JW_RL_NKLoadPort = "NLAMS";
			AssertEquals(false, ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(consol));

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(consol));

			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "DEHAM";
			AssertEquals(true, ConsolPortMessagingManager.IsValidConsolForDakosyPortMessaging(consol));
		}

		public void TestPreSendDataValidation_SZBNumberOnSeaShipments()
		{
			const string errorMessage = "Not all sea shipments on this Consol have received their release SZB number. Send the Port Order on the Shipments to receive release number(s) from Dakosy.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			var manager = new ConsolPortMessagingManager(consol);
			AssertEquals("Expected an error as consol has no shipment", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_SZB = "2803649";
			shipment1.JS_UniqueConsignRef = "S00001400";
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var subShipment1 = consol.Shipments.AddNew();
			subShipment1.JS_SZB = ZString.Empty;
			subShipment1.JS_UniqueConsignRef = "S00001401";
			subShipment1.JS_TransportMode = Constants.TransportModes.Sea;
			subShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment1.JS_JS_ColoadMasterShipment = shipment1.PK;

			AssertNotEquals("Expected no error as consol has a SZB number on an attached sea shipment", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipment1.JS_SZB = ZString.Empty;
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertNotEquals("Expected no error as consol has a SZB number on an attached sea shipment", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("Expected to have an error as added shipment does not have an SZB number", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipment1.JS_SZB = "999999";
			AssertNotEquals("Expected no error as consol has a SZB number on an attached sea shipment", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			AssertNotEquals("Expected no error as there already is a valid shipment for port messaging on the consol", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			consol.Shipments.Remove(shipment1);
			AssertEquals("Expected to have an error as shipment valid for port messaging has been removed", errorMessage, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_AUSEntryType()
		{
			const string expectedError = "Shipment S1234: All Pack Lines must have Harmonized Code entered for Entry Type 'AUS' (Emergency Concept)";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var sendingForwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S1234";
			shipment.JS_SZB = "123456";
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			PackLinePortMessaging.LoadOrCreate(packLine1);
			PackLinePortMessaging.LoadOrCreate(packLine2);

			var shipmentManager = new ShipmentPortMessagingManager(shipment);
			shipmentManager.PortMessaging.JSM_MovementReferenceNumber = "123";
			shipmentManager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			var manager = new ConsolPortMessagingManager(consol);

			packLine1.JL_HarmonisedCode = "BLAH";
			packLine2.JL_HarmonisedCode = ZString.Empty;
			AssertEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			packLine2.JL_HarmonisedCode = "BLAH2";
			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipmentManager.PortMessaging.JSM_EntryType = ZString.Empty;
			shipmentManager.PackLines[0].PortMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			shipmentManager.PackLines[1].PortMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;

			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			packLine2.JL_HarmonisedCode = ZString.Empty;
			AssertEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_EntryTypeIsMandatory()
		{
			const string expectedError = "At least one shipment must have port messaging entry type be entered before you can send Port Order.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_SZB = "123456";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_Description = "AAA";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_Description = "BBB";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_SZB = "123456";

			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_Description = "CCC";

			Factory.Save();

			ShipmentPortMessaging.LoadOrCreate(shipment);
			PackLinePortMessaging.LoadOrCreate(packLine1);
			PackLinePortMessaging.LoadOrCreate(packLine2);
			ShipmentPortMessaging.LoadOrCreate(shipment2);
			PackLinePortMessaging.LoadOrCreate(packLine3);

			var shipmentManager = new ShipmentPortMessagingManager(shipment);
			var manager = new ConsolPortMessagingManager(consol);

			AssertEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderInbound));

			shipmentManager.PackLines[0].PortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			AssertEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipmentManager.PackLines[1].PortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipmentManager.PackLines[0].PortMessaging.JLM_EntryType = ZString.Empty;
			shipmentManager.PackLines[1].PortMessaging.JLM_EntryType = ZString.Empty;

			var shipmentManager2 = new ShipmentPortMessagingManager(shipment2);
			shipmentManager2.PortMessaging.JSM_MovementReferenceNumber = "123";
			shipmentManager2.PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestDataChangeNotifiesOnShipmentsCountChanged()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manager = new ConsolPortMessagingManager(consol);
			int dataChangedInvoked = 0;
			manager.DataChanged += (s, e) => dataChangedInvoked++;

			var shipment = consol.Shipments.AddNew();
			AssertEquals(1, dataChangedInvoked);

			consol.Shipments.Remove(shipment);
			AssertEquals(2, dataChangedInvoked);
		}

		#region Implementation

		protected override PortMessagingManager GetNewManager()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new ConsolPortMessagingManager(consol);
		}

		protected override PortMessagingManager GetNewManagerFromPopulatedBusinessObject(ForwardingConsol consol, ForwardingShipment shipment)
		{
			return new ConsolPortMessagingManager(consol);
		}

		#endregion
	}
}
