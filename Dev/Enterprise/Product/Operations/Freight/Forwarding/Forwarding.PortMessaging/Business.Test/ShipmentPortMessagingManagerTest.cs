using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ShipmentPortMessagingManager))]
	sealed class ShipmentPortMessagingManagerTest : PortMessagingManagerTest
	{
		public void TestCheckUNDGDataItemsAreValid()
		{
			const string expectedError = "Multiple Dangerous Goods are recorded against a pack line on a shipment S00001010, please enter Package Count and Package Type for each DG recorded via Shipment > Packing > Pack line > Right click menu > Dangerous Goods";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.JK_TransportMode = "SEA";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONA";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001010";
			shipment.JS_TransportMode = "SEA";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container1);

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_PackageCount = 2;
			undg1.DI_F3_NKPackType = "BAG";

			var undg2 = packLine.UNDGs.AddNew();
			undg2.DI_PackageCount = 0;
			undg2.DI_F3_NKPackType = string.Empty;

			var portMessaging = new ShipmentPortMessagingManager(shipment);
			portMessaging.PortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertEquals("Pack count and type are empty on undg2.", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			undg2.DI_PackageCount = 5;
			undg2.DI_F3_NKPackType = "BAG";
			AssertNotEquals("Pack count and type are not empty on these UNDG items.", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestCheckUNDGDataItemsAreValid_SingleUNDG()
		{
			const string expectedError = "Multiple Dangerous Goods are recorded against a pack line on a shipment S00001234, please enter Package Count and Package Type for each DG recorded via Shipment > Packing > Pack line > Right click menu > Dangerous Goods";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(consol, container);

			var undg = packline.UNDGs.AddNew();
			undg.DI_PackageCount = 5;

			var portMessaging = new ShipmentPortMessagingManager(shipment);
			portMessaging.PortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			AssertNotEquals("Single dangerous goods item should not prevent sending message.", expectedError, portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestHasDG()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var undg = packLine.UNDGs.AddNew();

			var manager = new ShipmentPortMessagingManager(shipment);
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
			var shipment = Factory.New<ForwardingShipment>();
			var manager = new ShipmentPortMessagingManager(shipment);
			Assert(!manager.IsExport);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			Assert(manager.IsExport);
		}

		public void TestIsImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var manager = new ShipmentPortMessagingManager(shipment);
			Assert(!manager.IsImport);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "DEHAM";
			Assert(manager.IsImport);
		}

		public void TestSZBProperties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_SZB = "FOO";
			shipment.JS_SZBInformation = "BAR";
			shipment.JS_SZBIssueDate = new ZDateTime(2013, 10, 31);

			var manager = new ShipmentPortMessagingManager(shipment);
			AssertEquals("FOO", manager.SZBNumber);
			AssertEquals("BAR", manager.SZBInformation);
			AssertEquals(new ZDateTime(2013, 10, 31), manager.SZBIssueDate);
		}

		public void TestShouldShowPortMessagingForDakosy()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var manager = new ShipmentPortMessagingManager(shipment);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(false, manager.ShouldShowPortMessagingForDakosy);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
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
			ZString errorMessage = "Port Order to DAKOSY can only be sent from a sea Shipment linked to Consol with a sea leg loading or discharging in Hamburg.";
			AssertEquals("Pre-condition: security setting should be on by default", true, Env.Security.PortMessaging.IsAllowed);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var shipment = consol.Shipments.AddNew();
			var manager = new ShipmentPortMessagingManager(shipment);

			AssertEquals("Expected no error message as shipment is sea and has sea consol departing or arriving to DEHAM", ZString.Empty, manager.CheckPortMessagingAvailability());

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Expected an error message as shipment is not sea", errorMessage, manager.CheckPortMessagingAvailability());

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "DEHAM";
			AssertEquals("Expected no error message as shipment is sea and has sea consol departing or arriving to DEHAM", ZString.Empty, manager.CheckPortMessagingAvailability());

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Expected an error message as has not sea consol departing or arriving to DEHAM", errorMessage, manager.CheckPortMessagingAvailability());

			Env.Security.PortMessaging.IsAllowed = false;
			Factory.Save();

			AssertEquals("Expected security warning as user does not have permission to view or edit port messages", Env.Security.PortMessaging.ErrorMessageForNotAllowed, manager.CheckPortMessagingAvailability());

			consol.JK_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Expected to still display security warning, regardless of the consol and shipment being valid", Env.Security.PortMessaging.ErrorMessageForNotAllowed, manager.CheckPortMessagingAvailability());
		}

		public void TestCheckPortMessagingAvailability_SendFromASMShipment()
		{
			const string message = "Port Order to DAKOSY can't be sent from an Assembly master shipment.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var manager = new ShipmentPortMessagingManager(shipment);

			AssertEquals(message, manager.CheckPortMessagingAvailability());

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(string.Empty, manager.CheckPortMessagingAvailability());
		}

		public void TestCheckPortMessagingAvailability_SendFromCLDSubShipment()
		{
			const string message = "Port Order to DAKOSY can't be sent from a Co-Load sub-shipment.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var master = consol.Shipments.AddNew();
			master.JS_TransportMode = Constants.TransportModes.Sea;
			master.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var shipment = master.CoLoadShipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var manager = new ShipmentPortMessagingManager(shipment);

			AssertEquals(message, manager.CheckPortMessagingAvailability());

			master.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertEquals(string.Empty, manager.CheckPortMessagingAvailability());
		}

		public void TestIsEntryTypeBooleans()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var portMessagingData = new ShipmentPortMessagingData(consol, shipment);
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			Assert(portMessagingData.IsAUSEntryType());

			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			Assert(portMessagingData.IsMITEntryType());

			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			Assert(portMessagingData.IsSACEntryType());
		}

		public void TestPreSendDataValidation_ShouldNotAddErrorMessage_WhenConsolidationIsNotCoLoadAndCarrierBookingReferenceIsNotEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_BookingReference = "B2134";
			consol.JK_CoLoadBookingReference = ZString.Empty;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}
		}

		public void TestPreSendDataValidation_ShouldAddErrorMessage_WhenConsolidationIsNotCoLoadAndCarrierBookingReferenceIsEmpty()
		{
			const string message = "Carrier Booking Reference is required for LCL shipments when sending the Port Order with HDS. Consol -> Carrier Booking Reference.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_BookingReference = ZString.Empty;
			consol.JK_CoLoadBookingReference = "B1234";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}

			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS);
			}
			finally
			{
				AssertHasNotifications(manager.Data.BookingReferenceInfo);
				AssertHasMessageError(manager.Data.BookingReferenceInfo, message);
			}
		}

		public void TestPreSendDataValidation_ShouldAddErrorMessage_WhenConsolidationIsCoLoadAndCoLoadBookingReferenceIsEmpty()
		{
			const string message = "Co-Load Booking Reference is required for LCL shipments when sending the Port Order with HDS and Consol type is Co-Load. Consol -> Co-Load Booking Reference.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_BookingReference = "B2134";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;

			consol.JK_CoLoadBookingReference = "CONSOL-BKG-123";
			shipment.BKGNumber = ZString.Empty;
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}

			consol.JK_CoLoadBookingReference = ZString.Empty;
			shipment.BKGNumber = "SHIP-BKG-123";
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}

			consol.JK_CoLoadBookingReference = ZString.Empty;
			shipment.BKGNumber = ZString.Empty;
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}

			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS);
			}
			finally
			{
				AssertHasNotifications(manager.Data.BookingReferenceInfo);
				AssertHasMessageError(manager.Data.BookingReferenceInfo, message);
			}
		}

		public void TestPreSendDataValidation_ShouldNotAddErrorMessage_WhenConsolidationIsCoLoadAndCoLoadBookingReferenceIsNotEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadBookingReference = "B1234";
			consol.JK_BookingReference = ZString.Empty;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}

			try
			{
				manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS);
			}
			finally
			{
				AssertNoNotifications(manager.Data.BookingReferenceInfo);
			}
		}

		public void TestPreSendDataValidation_MRNIsMandatory()
		{
			const string message = "MRN number must be entered before you can send Port Order.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.GatePass));
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PortMessaging.JSM_Annex30AType = Annex30ATypeList.Codes.AlreadyCompleted;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PortMessaging.JSM_MovementReferenceNumber = "123";
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_MRNIsNotRequiredForAUSType()
		{
			const string message = "MRN number must be entered before you can send Port Order.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;

			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_EntryTypeIsMandatory()
		{
			const string message = "Entry Type must be entered on either shipment or all pack lines before you can send Port Order.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			PackLinePortMessaging.LoadOrCreate(packLine1);
			PackLinePortMessaging.LoadOrCreate(packLine2);

			var manager = new ShipmentPortMessagingManager(shipment);

			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PackLines[0].PortMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PackLines[1].PortMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PackLines[0].PortMessaging.JLM_EntryType = ZString.Empty;
			manager.PackLines[1].PortMessaging.JLM_EntryType = ZString.Empty;
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_MRNCompleteForCancellationOnExit()
		{
			const string message = "At least one MRN must be marked as complete before you can send Cancellation on Exit.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_SZB = "123456";

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			PackLinePortMessaging.LoadOrCreate(packLine1);
			PackLinePortMessaging.LoadOrCreate(packLine2);

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			manager.PortMessaging.JSM_MovementReferenceNumber = "15DE12345678910126";

			manager.PortMessaging.JSM_MovementReferenceNumberComplete = false;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit));

			manager.PortMessaging.JSM_MovementReferenceNumberComplete = true;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit));

			manager.PortMessaging.JSM_MovementReferenceNumber = ZString.Empty;
			manager.PortMessaging.JSM_MovementReferenceNumberComplete = false;

			manager.PackLines[0].PortMessaging.JLM_MovementReferenceNumber = "15DE12345678910126";
			manager.PackLines[0].PortMessaging.JLM_MovementReferenceNumberComplete = false;
			manager.PackLines[1].PortMessaging.JLM_MovementReferenceNumber = "15DE12345678910126";
			manager.PackLines[1].PortMessaging.JLM_MovementReferenceNumberComplete = false;

			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit));

			manager.PackLines[0].PortMessaging.JLM_MovementReferenceNumberComplete = true;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit));
		}

		public void TestPreSendDataValidation_MRNCompleteForForwardingCancellation()
		{
			const string message = "All MRNs being sent must be marked as complete before you can send Forwarding Cancellation.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_SZB = "123456";

			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			PackLinePortMessaging.LoadOrCreate(packLine1);
			PackLinePortMessaging.LoadOrCreate(packLine2);

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			manager.PortMessaging.JSM_MovementReferenceNumber = "15DE12345678910126";
			manager.PortMessaging.JSM_ForwardingCustomsOfficeCode = "SCAMBURG";

			manager.PortMessaging.JSM_MovementReferenceNumberComplete = false;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation));

			manager.PortMessaging.JSM_MovementReferenceNumberComplete = true;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation));

			manager.PortMessaging.JSM_MovementReferenceNumber = ZString.Empty;
			manager.PortMessaging.JSM_MovementReferenceNumberComplete = false;

			manager.PackLines[0].PortMessaging.JLM_MovementReferenceNumber = "15DE12345678910126";
			manager.PackLines[0].PortMessaging.JLM_MovementReferenceNumberComplete = false;
			manager.PackLines[1].PortMessaging.JLM_MovementReferenceNumber = "15DE12345678910126";
			manager.PackLines[1].PortMessaging.JLM_MovementReferenceNumberComplete = false;

			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation));

			manager.PackLines[0].PortMessaging.JLM_MovementReferenceNumberComplete = true;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation));

			manager.PackLines[1].PortMessaging.JLM_MovementReferenceNumberComplete = true;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation));
		}

		public void TestPreSendDataValidation_AUSEntryType()
		{
			const string expectedError = "All Pack Lines must have Harmonized Code entered for Entry Type 'AUS' (Emergency Concept)";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_SZB = "123456";
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			PackLinePortMessaging.LoadOrCreate(packLine1);
			PackLinePortMessaging.LoadOrCreate(packLine2);

			var manager = new ShipmentPortMessagingManager(shipment);
			manager.PortMessaging.JSM_MovementReferenceNumber = "123";
			manager.PortMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;

			packLine1.JL_HarmonisedCode = "BLAH";
			packLine2.JL_HarmonisedCode = ZString.Empty;
			AssertEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			packLine2.JL_HarmonisedCode = "BLAH2";
			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			manager.PortMessaging.JSM_EntryType = ZString.Empty;
			manager.PackLines[0].PortMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;
			manager.PackLines[1].PortMessaging.JLM_EntryType = EntryTypeList.Codes.EmergencyConcept;

			AssertNotEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
			packLine2.JL_HarmonisedCode = ZString.Empty;
			AssertEquals(expectedError, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPortMessagingPackLinesContainerLink()
		{
			var otherConsol = Factory.New<ForwardingConsol>();
			otherConsol.JK_TransportMode = Constants.TransportModes.Sea;
			otherConsol.JK_RL_NKLoadPort = "DEFRA";
			otherConsol.Containers.AddNew();

			var shipment = otherConsol.Shipments.AddNew();

			var dakosyConsol = Factory.New<ForwardingConsol>();
			dakosyConsol.JK_TransportMode = Constants.TransportModes.Sea;
			dakosyConsol.JK_RL_NKLoadPort = "DEHAM";
			var dakosyContainer = dakosyConsol.Containers.AddNew();

			dakosyConsol.Shipments.Add(shipment);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(dakosyContainer.PK);
			PackLinePortMessaging.LoadOrCreate(packLine);
			var manager = new ShipmentPortMessagingManager(shipment);

			AssertEquals("Expected pack line to be linked to the container on the consol valid for Dakosy port messaging.", dakosyContainer.PK, manager.PackLines[0].JL_JC);
		}

		#region Implementation

		protected override PortMessagingManager GetNewManager()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new ShipmentPortMessagingManager(shipment);
		}

		protected override PortMessagingManager GetNewManagerFromPopulatedBusinessObject(ForwardingConsol consol, ForwardingShipment shipment)
		{
			return new ShipmentPortMessagingManager(shipment);
		}

		#endregion
	}
}
