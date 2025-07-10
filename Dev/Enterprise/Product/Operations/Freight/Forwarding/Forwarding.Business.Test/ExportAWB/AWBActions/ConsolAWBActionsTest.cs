using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.eTail.Integration.HVLVConstants;
using AgentType = Enterprise.Core.Constants.AgentType;
using Constants = Enterprise.Core.Constants;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ConsolAWBActions))]
	sealed class ConsolAWBActionsTest : AWBActionsTest
	{
		const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
			"<ConsolAWBActionsSettings><PrintBarcodeLabel>Y</PrintBarcodeLabel><FiveInchLabel>Y</FiveInchLabel>" +
			"<PrintOptionalInformation>N</PrintOptionalInformation><LabelPrinter>0c762aec-2819-4b36-ac7a-289932636da5</LabelPrinter><LabelUseEPrint>N</LabelUseEPrint>" +
			"<PrintNeutralAWBsAsLaser>Y</PrintNeutralAWBsAsLaser><MAWBPrinter>00000000-0000-0000-0000-000000000000</MAWBPrinter><MAWBUseEPrint>N</MAWBUseEPrint>" +
			"<PrintHAWBBarcodeLabels>N</PrintHAWBBarcodeLabels><HAWBLabelPrinter>a418bbbb-d379-44d6-a4db-6b2b2ab81afa</HAWBLabelPrinter><HAWBLabelUseEPrint>N</HAWBLabelUseEPrint>" +
			"<AWBPackagesLabel>N</AWBPackagesLabel>" +
			"</ConsolAWBActionsSettings>";
		const string ErrorCSDNotApplicableForUS = "CSD is not applicable for United States.";

		public override void TestLoadSettings()
		{
			CreatePrinter(new Guid("0c762aec-2819-4b36-ac7a-289932636da5"));

			Env.Registry.SetFilterCriteria("ConsolAWBActionsSettings", xml);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SouthAfrica))
			{
				var awbActions = new ConsolAWBActions(Factory.New<ForwardingConsol>(), Business.AWB.AWBActions.ActionsModeType.LabelsOnly);

				AssertEquals(true, awbActions.PrintBarcodeLabel);
				AssertEquals(true, awbActions.FiveInchLabel);
				AssertEquals(false, awbActions.PrintOptionalInformation);
				AssertEquals("existing printer guid", new ZGuid("0c762aec-2819-4b36-ac7a-289932636da5"), awbActions.LabelPrinter);
				AssertEquals(true, awbActions.PrintNeutralAWBsAsLaser);
				AssertEquals("empty guid", ZGuid.Empty, awbActions.MAWBPrinter);
				AssertEquals(false, awbActions.PrintHAWBBarcodeLabels);
				AssertEquals("non existing printer guid defaults to empty", ZGuid.Empty, awbActions.HAWBLabelPrinter);
				AssertEquals(false, awbActions.AWBPackagesLabel);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var awbActions = new ConsolAWBActions(Factory.New<ForwardingConsol>(), Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
				AssertEquals(false, awbActions.PrintConsignmentSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var awbActions = new ConsolAWBActions(Factory.New<ForwardingConsol>(), Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
				AssertEquals(false, awbActions.PrintConsignmentSecurityDeclaration);
			}
		}

		public override void TestSaveSettings()
		{
			CreatePrinter(new Guid("0c762aec-2819-4b36-ac7a-289932636da5"));
			CreatePrinter(new Guid("a418bbbb-d379-44d6-a4db-6b2b2ab81afa"));

			var awbActions = new ConsolAWBActions(Factory.New<ForwardingConsol>(), Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
			awbActions.PrintBarcodeLabel = true;
			awbActions.FiveInchLabel = true;
			awbActions.PrintOptionalInformation = false;
			awbActions.LabelPrinter = new ZGuid("0c762aec-2819-4b36-ac7a-289932636da5");
			awbActions.PrintNeutralAWBsAsLaser = true;
			awbActions.MAWBPrinter = ZGuid.Empty;
			awbActions.PrintHAWBBarcodeLabels = false;
			awbActions.HAWBLabelPrinter = new ZGuid("a418bbbb-d379-44d6-a4db-6b2b2ab81afa");
			awbActions.AWBPackagesLabel = false;

			awbActions.SaveSettings();

			AssertEquals(xml, Env.Registry.GetFilterCriteria("ConsolAWBActionsSettings"));
		}

		public void TestAutoSwitchFWBAndFHL()
		{
			AssertEquals("Precondition: registry CargoImpSentMessageVersions", CargoIMPSentMessageVersionsList.Codes.AutoSwitch, ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.Value);
			AssertEquals("Precondition: Consol.Transports.Count", 1, Consol.Transports.Count);
			var firstLeg = Consol.Transports[0];
			firstLeg.JW_ETD = new ZDateTime(2010, 12, 28);
			var mawbHeader = Consol.AWBHeader;
			mawbHeader.EH_AWBIssueDate = new ZDateTime(2010, 12, 20);
			AssertEquals("Precondition: Booking1stFlightDate", new ZDateTime(2010, 12, 28), mawbHeader.Booking1stFlightDate);

			AWBActions.DoSendFWB();
			var message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			AssertContains("message.EM_MessageText", "FWB/10\r\n", message.EM_MessageText);
			message.DeleteFromTest();

			AWBActions.DoSendFHL();
			message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			AssertContains("message.EM_MessageText", "FHL/2\r\n", message.EM_MessageText);
			message.DeleteFromTest();

			Consol.JK_OverrideWaybillDefaults = true;
			mawbHeader.EH_Booking1stFlightDate = "29";
			AssertEquals("Precondition: Booking1stFlightDate", new ZDateTime(2010, 12, 29), mawbHeader.Booking1stFlightDate);

			AWBActions.DoSendFWB();
			message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			AssertContains("message.EM_MessageText", "FWB/16\r\n", message.EM_MessageText);
			message.DeleteFromTest();

			AWBActions.DoSendFHL();
			message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			AssertContains("message.EM_MessageText", "FHL/4\r\n", message.EM_MessageText);
			message.DeleteFromTest();
		}

		public void TestCanGenerateFWB16()
		{
			ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPSentMessageVersionsList.Codes.FWBv16_FHLv4);
			AWBActions.DoSendFWB();

			CIMEDIMessage message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			CombineAssertions(delegate
			{
				AssertEquals("Precondition: message.EM_MessageType", CargoIMP.MessageTypes.FWB, message.EM_MessageType);
				AssertEquals("Precondition: message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("Precondition: message.EM_ApplicationReference", "08112345678", message.EM_ApplicationReference);

				AssertContains("message.EM_MessageText", "FWB/16\r\n", message.EM_MessageText);
			});
		}

		public void TestCanGenerateFWB10()
		{
			ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPSentMessageVersionsList.Codes.FWBv10_FHLv2);
			AWBActions.DoSendFWB();

			CIMEDIMessage message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			CombineAssertions(delegate
			{
				AssertEquals("Precondition: message.EM_MessageType", CargoIMP.MessageTypes.FWB, message.EM_MessageType);
				AssertEquals("Precondition: message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("Precondition: message.EM_ApplicationReference", "08112345678", message.EM_ApplicationReference);

				AssertContains("message.EM_MessageText", "FWB/10\r\n", message.EM_MessageText);
			});
		}

		public void TestCanGenerateFHL4()
		{
			ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPSentMessageVersionsList.Codes.FWBv16_FHLv4);
			AWBActions.DoSendFHL();

			CIMEDIMessage message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			CombineAssertions(delegate
			{
				AssertEquals("Precondition: message.EM_MessageType", CargoIMP.MessageTypes.FHL, message.EM_MessageType);
				AssertEquals("Precondition: message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("Precondition: message.EM_ApplicationReference", "SHIPMENT", message.EM_ApplicationReference);

				AssertContains("message.EM_MessageText", "FHL/4\r\n", message.EM_MessageText);
			});
		}

		public void TestCanGenerateFHL2()
		{
			ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPSentMessageVersionsList.Codes.FWBv10_FHLv2);
			AWBActions.DoSendFHL();

			CIMEDIMessage message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNotNull("Precondition: EDIMessage on Consol", message);
			CombineAssertions(delegate
			{
				AssertEquals("Precondition: message.EM_MessageType", CargoIMP.MessageTypes.FHL, message.EM_MessageType);
				AssertEquals("Precondition: message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("Precondition: message.EM_ApplicationReference", "SHIPMENT", message.EM_ApplicationReference);

				AssertContains("message.EM_MessageText", "FHL/2\r\n", message.EM_MessageText);
			});
		}

		public void TestDoNotSetHVI_SecurityFillingFirstUsageTimeUtc_FHL()
		{
			var shipment = Consol.Shipments[0];
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "BOOKS";

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "COMICS";

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;

			Factory.Save();
			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBActions.DoSendFHL();
			}

			var reloadedItem = Factory.Load<IHVLVItem>(item1.PK);

			AssertEquals(ZDateTime.Empty, reloadedItem.HVI_SecurityFilingFirstUsageTimeUtc);
		}

		public void TestInterchangeHeaderBlank()
		{
			AWBActions.DoSendFWB();
			AWBActions.DoSendFHL();
			AssertEquals("Consol.Messages.Count", 2, Consol.CIMEDIMessages.Count);
			EDIInterchange interchange1 = Factory.Load<EDIInterchange>(Consol.CIMEDIMessages[0].EM_EI);
			EDIInterchange interchange2 = Factory.Load<EDIInterchange>(Consol.CIMEDIMessages[1].EM_EI);
			AssertEquals("Header Text empty as dependent on transmission method. Set in sender process", "", interchange1.EI_HeaderText);
			AssertEquals("Header Text empty as dependent on transmission method. Set in sender process", "", interchange2.EI_HeaderText);
		}

		#region Test DoSendFHL

		[TestDate(2023, 2, 22, 10, 11, 48)]
		public void TestDoSendFHLs()
		{
			try
			{
				ZDateTime now = ZDateTime.Now;

				AWBActions.ShowMessageOnGUI += AWBActions_ShowMessageOnGUI;
				lastGUIMessageTitle = null;
				Consol.JK_MasterBillIssueDate = now.AddDays(2);
				AWBActions.DoSendFHL();
				AssertEquals(now.AddDays(2), Consol.JK_MasterBillIssueDate);

				CIMEDIMessage message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
				AssertNotNull(message);
				AssertEquals(CargoIMP.MessageTypes.FHL, message.EM_MessageType);
				AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("SHIPMENT", message.EM_ApplicationReference);

				CIMEDIInterchange interchange = Factory.LoadTop1<CIMEDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, message.EM_EI));
				AssertNotNull(interchange);
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals((registrationKey.EnterpriseCode.Length == 0 ? (string)GlbCompany.CurrentCompany.GC_Code : registrationKey.EnterpriseCode) + registrationKey.ServerCode, interchange.EI_From);
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("EDI CCN", interchange.EI_To);
				AssertNull("GUI Message", lastGUIMessageTitle);

				Consol.JK_MasterBillIssueDate = ZDateTime.Empty;
				AWBActions.DoSendFHL();
				AssertEquals(now, Consol.JK_MasterBillIssueDate);
			}
			finally
			{
				AWBActions.ShowMessageOnGUI -= AWBActions_ShowMessageOnGUI;
			}
		}

		public void TestDoSendFHLs_ApplicationReferenceSetWhenShipmentHasUniqueConsignRef()
		{
			var shipment = Consol.Shipments[0];
			shipment.JS_UniqueConsignRef = "S00001000";

			AWBActions.DoSendFHL();

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			var message = messages.Single();
			AssertEquals("EM_ApplicationReference is taken from JS_UniqueConsignRef rather than JS_HouseBill", "S00001000", message.EM_ApplicationReference);
		}

		public void TestDoSendFHLs_WhenShipmentIsHVLAndRegistryEnabled_SendFHLForConsignments()
		{
			var shipment = Consol.Shipments[0];
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "BOOKS";

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "COMICS";

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;

			Factory.Save();

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBActions.DoSendFHL();
			}

			var messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkTable, HVLVConsignmentSchema.Constants.TableName));

			AssertEquals("Should be two messages for consignments", 2, messages.Length);
			AssertContainsExactElementsInAnyOrder("message details come from HVLVConsignments", messages.Cast<EDIMessage>().Select(m => m.EM_ApplicationReference), new[] { consignment1.HVC_ConsignmentId, consignment2.HVC_ConsignmentId });
			AssertContainsExactElementsInAnyOrder("message is linked to HVLVConsignments", messages.Cast<EDIMessage>().Select(m => m.EM_LinkUniqueID), new[] { consignment1.PK, consignment2.PK });
		}

		[TestDate(2021, 2, 2, 2, 2, 2)]
		public void TestDoSendFHLs_WhenShipmentIsHVL_WillNotRepopulateHVI_SecurityFilingFirstUsageTimeUtc()
		{
			var shipment = Consol.Shipments[0];
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "BOOKS";

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;
			consignment2.HVC_WaybillNumber = "bruh";

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			((BusinessObject)item1)[HVLVItemSchema.HVI_SecurityFilingFirstUsageTimeUtc.Name] = new ZDateTime(2019, 5, 3);
			((BusinessObject)item2)[HVLVItemSchema.HVI_SecurityFilingFirstUsageTimeUtc.Name] = new ZDateTime(2019, 5, 3);

			Factory.Save();

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBActions.DoSendFHL();
			}

			AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 2, 2), ((BusinessObject)item1)[HVLVItemSchema.HVI_SecurityFilingFirstUsageTimeUtc.Name]);
			AssertNotEquals(new ZDateTime(2021, 2, 2, 2, 2, 2), ((BusinessObject)item2)[HVLVItemSchema.HVI_SecurityFilingFirstUsageTimeUtc.Name]);
		}

		public void TestDoSendFHLs_WhenShipmentIsHVL_SetsHVI_LastUsageCode()
		{
			var shipment = Consol.Shipments[0];
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var shipment2 = otherFactory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment1 = otherFactory.New<IHVLVConsignment>();
			consignment1.HVC_WaybillNumber = "Waybill1";
			((BusinessObject)consignment1)[HVLVConsignmentSchema.HVC_HCH_Header.Name] = shipment.HVLVConsignmentHeader.PK;

			var item1 = otherFactory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = otherFactory.New<IHVLVConsignment>();
			consignment2.HVC_WaybillNumber = "Waybill2";
			((BusinessObject)consignment2)[HVLVConsignmentSchema.HVC_HCH_Header.Name] = shipment.HVLVConsignmentHeader.PK;

			var item2 = otherFactory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var item3 = otherFactory.New<IHVLVItem>();
			item3.HVI_HVC_Consignment = consignment2.PK;
			item3.HVI_JS_LoadedOnShipment = shipment2.PK;

			item1.HVI_LastUsageCode = "ZZZ";
			item2.HVI_LastUsageCode = "ZZZ";
			item3.HVI_LastUsageCode = "ZZZ";

			otherFactory.Save();

			AssertContainsExactElementsInAnyOrder("Precondition: consignment1 and 2 both have items on shipment", new[] { "Waybill1", "Waybill2" }, shipment.HVLVConsignments.Select(c => c.HVC_WaybillNumber));

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBActions.DoSendFHL();
			}

			AssertEquals("item1 usage code set", ExpectedFHLUsageCode, item1.HVI_LastUsageCode);
			AssertEquals("item2 usage code set", ExpectedFHLUsageCode, item2.HVI_LastUsageCode);
			AssertEquals("item3 usage code set, because item2 (from same consignment) is on shipment", ExpectedFHLUsageCode, item3.HVI_LastUsageCode);
		}

		public void TestHVLVUsageCodeAndCategory()
		{
			AssertEquals("UsageCode", "FHL", UsageCodes.FHLAirlineMessaging);
			AssertEquals("UsageCategory", UsageCategories.SecurityFiling, UsageCategories.LookupByUsageCode[ExpectedFHLUsageCode]);
		}

		const string ExpectedFHLUsageCode = "FHL";

		#region Test DoSendFHL For Master Shipments

		public void TestDoSendFHLsForASMMaster()
		{
			ForwardingShipment shipment1 = Consol.Shipments[0];
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingShipment master = Consol.Shipments.AddNew();
			master.JS_ShipmentType = "ASM";

			shipment1.JS_HouseBill = "shipment1";
			shipment2.JS_HouseBill = "shipment2";
			master.JS_HouseBill = "master";

			shipment1.JS_JS_ColoadMasterShipment = master.PK;
			shipment2.JS_JS_ColoadMasterShipment = master.PK;

			AWBActions.DoSendFHL();

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			AssertEquals("Should only be one message", 1, messages.Length);
			Assert(messages[0].EM_FormattedMessageText.Contains("HBS/MASTER/"));
		}

		public void TestDoSendFHLsForBCNMaster()
		{
			ForwardingShipment shipment1 = Consol.Shipments[0];
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			ForwardingShipment master = Consol.Shipments.AddNew();
			master.JS_ShipmentType = "BCN";

			shipment1.JS_HouseBill = "shipment1";
			shipment2.JS_HouseBill = "shipment2";
			master.JS_HouseBill = "lead";

			shipment1.JS_JS_ColoadMasterShipment = master.PK;
			shipment2.JS_JS_ColoadMasterShipment = master.PK;

			AWBActions.DoSendFHL();

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			AssertEquals("Should be 3 messages", 3, messages.Length);

			bool foundShipment1 = false;
			bool foundShipment2 = false;
			bool foundLead = false;

			foreach (CIMEDIMessage message in messages)
			{
				if (message.EM_FormattedMessageText.Contains("HBS/SHIPMENT1/"))
				{
					foundShipment1 = true;
				}
				else if (message.EM_FormattedMessageText.Contains("HBS/SHIPMENT2/"))
				{
					foundShipment2 = true;
				}
				else if (message.EM_FormattedMessageText.Contains("HBS/LEAD/"))
				{
					foundLead = true;
				}
			}

			AssertEquals("Shipment1", true, foundShipment1);
			AssertEquals("Shipment2", true, foundShipment2);
			AssertEquals("Lead", true, foundLead);
		}

		public void TestDoSendFHLsForCLDMaster()
		{
			ForwardingShipment masterShipment = Consol.Shipments[0];
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master";

			ForwardingShipment shipment1 = Consol.Shipments.AddNew();
			ForwardingShipment shipment2 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "shipment1";
			shipment2.JS_HouseBill = "shipment2";
			shipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			shipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			AWBActions.DoSendFHL();

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			AssertEquals("Expected a message", 1, messages.Length);
			var message = messages[0].EM_FormattedMessageText;

			AssertContains("Expected to find ONLY the master shipment", "HBS/MASTER/", message);
			AssertNotContains("Expected not to have included sub shipments", "HBS/SHIPMENT1/", message);
			AssertNotContains("Expected not to have included sub shipments", "HBS/SHIPMENT2/", message);
		}

		public void TestDoSendFHLForHVMMaster_WhenRegistryDisabled_ShouldNotSendFHLForHVLShipment()
		{
			var hvmShipment = Consol.Shipments[0];
			hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			hvmShipment.JS_HouseBill = "Master";

			var hvlShipment1 = Consol.Shipments.AddNew();
			var hvlShipment2 = Consol.Shipments.AddNew();
			hvlShipment1.JS_HouseBill = "shipment1";
			hvlShipment2.JS_HouseBill = "shipment2";
			hvlShipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment1.JS_JS_ColoadMasterShipment = hvmShipment.PK;
			hvlShipment2.JS_JS_ColoadMasterShipment = hvmShipment.PK;

			Factory.Save();

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				AWBActions.DoSendFHL();
			}

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			AssertEquals("Expected only 1 message", 1, messages.Length);
			var message = messages[0].EM_FormattedMessageText;

			AssertContains("Expected to find ONLY the master shipment", "HBS/MASTER/", message);
			AssertNotContains("Expected not to have included sub shipments", "HBS/SHIPMENT1/", message);
			AssertNotContains("Expected not to have included sub shipments", "HBS/SHIPMENT2/", message);
		}

		public void TestDoSendFHLForHVMMaster_WhenRegistryEnabled_ShouldSendFHLForHVMShipmentAndHVLVConsignments()
		{
			var hvmShipment = Consol.Shipments[0];
			hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			hvmShipment.JS_HouseBill = "Master";

			var hvlShipment = Consol.Shipments.AddNew();
			hvlShipment.JS_HouseBill = "Shipment";
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = hvlShipment.PK;
			consignment1.HVC_WaybillNumber = "BOOKS";

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = hvlShipment.PK;
			consignment2.HVC_WaybillNumber = "COMICS";

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;

			Factory.Save();

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBActions.DoSendFHL();
			}

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("Should be three messages, 1 for HVM shipment and 2 for consignments", 3, messages.Length);
				AssertContainsExactElementsInAnyOrder("message details come from HVLVConsignments", messages.Cast<EDIMessage>().Select(m => m.EM_ApplicationReference), new[] { hvmShipment.JS_UniqueConsignRef, consignment1.HVC_ConsignmentId, consignment2.HVC_ConsignmentId });
				AssertContainsExactElementsInAnyOrder("message is linked to HVLVConsignments", messages.Cast<EDIMessage>().Select(m => m.EM_LinkUniqueID), new[] { Consol.PK, consignment1.PK, consignment2.PK });
			});
		}

		public void TestDoSendFHLForHVLShipment_WhenNoHVMShipmentAndRegistryIsDisabled()
		{
			var hvlShipment = Consol.Shipments[0];
			hvlShipment.JS_HouseBill = "Shipment";
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			Factory.Save();

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				AWBActions.DoSendFHL();
			}

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			AssertEquals("Expected only 1 message", 1, messages.Length);
			var message = messages[0].EM_FormattedMessageText;

			AssertContains("Expected to find the HVL shipment", "HBS/SHIPMENT/", message);
		}

		public void TestDoSendFHLsForVariousMasters()
		{
			var blindMaster = Consol.Shipments[0];
			blindMaster.JS_ShipmentType = Core.Constants.ShipmentTypes.BlindCoLoadMaster;
			blindMaster.JS_HouseBill = "Zatoichi";
			var blindSubShipment = Consol.Shipments.AddNew();
			blindSubShipment.JS_HouseBill = "BlindSub";
			blindSubShipment.JS_JS_ColoadMasterShipment = blindMaster.PK;

			var standardShipment = Consol.Shipments.AddNew();
			standardShipment.JS_HouseBill = "Standard";

			var buyersLeadShipment = Consol.Shipments.AddNew();
			buyersLeadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			buyersLeadShipment.JS_HouseBill = "BuyersLead";
			var buyersSubShipment = Consol.Shipments.AddNew();
			buyersSubShipment.JS_HouseBill = "BuyersSub";
			buyersSubShipment.JS_JS_ColoadMasterShipment = buyersLeadShipment.PK;

			AWBActions.DoSendFHL();

			CIMEDIMessage[] messages = Factory.Load<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));

			bool foundBlindMaster = false;
			bool foundBlindSub = false;
			bool foundStandard = false;
			bool foundBuyersLead = false;
			bool foundBuyersSub = false;

			AssertNotNull("Expected to have created messages", messages);
			AssertEquals("Expected find 4 shipment messages", 4, messages.Length);

			foreach (CIMEDIMessage message in messages)
			{
				if (message.EM_FormattedMessageText.Contains("HBS/ZATOICHI/"))
				{
					foundBlindMaster = true;
				}
				else if (message.EM_FormattedMessageText.Contains("HBS/BLINDSUB/"))
				{
					foundBlindSub = true;
				}
				else if (message.EM_FormattedMessageText.Contains("HBS/STANDARD/"))
				{
					foundStandard = true;
				}
				else if (message.EM_FormattedMessageText.Contains("HBS/BUYERSLEAD/"))
				{
					foundBuyersLead = true;
				}
				else if (message.EM_FormattedMessageText.Contains("HBS/BUYERSSUB/"))
				{
					foundBuyersSub = true;
				}
			}

			AssertEquals("Should not have found blind master shipment in any message", false, foundBlindMaster);
			AssertEquals("Expected to have found the blind sub shipment", true, foundBlindSub);
			AssertEquals("Expected to have found the standard shipment", true, foundStandard);
			AssertEquals("Expected to have found the buyer's consolation lead shipment", true, foundBuyersLead);
			AssertEquals("Expected to have found the buyer's subshipment", true, foundBuyersSub);
		}

		#endregion

		public void TestDoSendFHLsWithoutLicence()
		{
			try
			{
				using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.CCN))
				{
					AWBActions.ShowMessageOnGUI += AWBActions_ShowMessageOnGUI;
					Env.Licence.EzycargoInterface.AllowUsageForTest = false;
					lastGUIMessageTitle = null;
					AWBActions.DoSendFHL();
					AssertEquals("FHL Message", lastGUIMessageTitle);
					AssertContains("Cargo 2000 Phase 1", lastGUIMessageText);
				}
			}
			finally
			{
				AWBActions.ShowMessageOnGUI -= AWBActions_ShowMessageOnGUI;
				Env.Licence.EzycargoInterface.AllowUsageForTest = null;
			}

			var message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNull(message);
		}

		public void TestDoSendFHL_SavesAWB()
		{
			AssertEquals(false, Consol.AWBHeader.IsInDatabase);
			Factory.Save();
			AssertEquals("Still not in database because not overridden", false, Consol.AWBHeader.IsInDatabase);

			AWBActions.DoSendFHL();
			AssertEquals("In database after sending FHL", true, Consol.AWBHeader.IsInDatabase);
		}

		#endregion

		[TestDate(2023, 2, 22, 10, 11, 48)]
		public void TestDoSendFWB()
		{
			try
			{
				ZDateTime now = ZDateTime.Now;

				AWBActions.ShowMessageOnGUI += AWBActions_ShowMessageOnGUI;
				lastGUIMessageTitle = null;
				Consol.JK_MasterBillIssueDate = now.AddDays(2);
				AWBActions.DoSendFWB();
				AssertEquals(now.AddDays(2), Consol.JK_MasterBillIssueDate);

				CIMEDIMessage message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
				AssertNotNull(message);
				AssertEquals(CargoIMP.MessageTypes.FWB, message.EM_MessageType);
				AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("08112345678", message.EM_ApplicationReference);

				CIMEDIInterchange interchange = Factory.LoadTop1<CIMEDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, message.EM_EI));
				AssertNotNull(interchange);
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				AssertEquals((registrationKey.EnterpriseCode.Length == 0 ? (string)GlbCompany.CurrentCompany.GC_Code : registrationKey.EnterpriseCode) + registrationKey.ServerCode, interchange.EI_From);
				AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
				AssertEquals("EDI CCN", interchange.EI_To);
				AssertNull("GUI Message", lastGUIMessageTitle);

				Consol.JK_MasterBillIssueDate = ZDateTime.Empty;
				AWBActions.DoSendFWB();
				AssertEquals(now, Consol.JK_MasterBillIssueDate);
			}
			finally
			{
				AWBActions.ShowMessageOnGUI -= AWBActions_ShowMessageOnGUI;
			}
		}

		public void TestDoSendFWBsWithoutLicence()
		{
			try
			{
				using (ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.CCN))
				{
					AWBActions.ShowMessageOnGUI += AWBActions_ShowMessageOnGUI;
					Env.Licence.EzycargoInterface.AllowUsageForTest = false;
					lastGUIMessageTitle = null;
					AWBActions.DoSendFWB();
					AssertEquals("FWB Message", lastGUIMessageTitle);
					AssertContains("Cargo 2000 Phase 1", lastGUIMessageText);
				}
			}
			finally
			{
				AWBActions.ShowMessageOnGUI -= AWBActions_ShowMessageOnGUI;
				Env.Licence.EzycargoInterface.AllowUsageForTest = null;
			}

			var message = Factory.LoadTop1<CIMEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Consol.PK));
			AssertNull(message);
		}

		public void TestDoSendFWBClientCodeSuffix()
		{
			AWBActions.DoSendFWB();

			var query = new ZDBOnlyQuery(typeof(EDIInterchange));
			var sub = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
			sub.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, Consol.PK);
			query.AddSubQuery(sub, JoinCondition.And);

			var interchanges = Factory.Load<EDIInterchange>(query);

			AssertEquals(1, interchanges.Length);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			AssertEquals(registrationKey.EnterpriseCode + registrationKey.ServerCode, interchanges[0].EI_From);

			ForwardingConfigurationRegistry.Instance.CargoIMPClientCodeSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");

			try
			{
				AWBActions.DoSendFWB();

				interchanges = Factory.Load<EDIInterchange>(query);

				AssertEquals(2, interchanges.Length);
				AssertEquals(registrationKey.EnterpriseCode + "ABC", interchanges[1].EI_From);
			}
			finally
			{
				ForwardingConfigurationRegistry.Instance.CargoIMPClientCodeSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrationKey.ServerCode);
			}
		}

		public void TestDoSendFWB_SavesAWB()
		{
			AssertEquals(false, Consol.AWBHeader.IsInDatabase);
			Factory.Save();
			AssertEquals("Still not in database because not overridden", false, Consol.AWBHeader.IsInDatabase);

			AWBActions.DoSendFWB();
			AssertEquals("In database after sending FWB", true, Consol.AWBHeader.IsInDatabase);
		}

		public void TestPrintConsignmentSecurityDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var awbHeader = GetAWBHeaderWithoutSecurityDeclarationNotifications();
				awbHeader.EH_RN_NKAgentApprovalCountryCode = "JM";
				AWBActions.SendFWB = true;
				AWBActions.PrintConsignmentSecurityDeclaration = true;

				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);

				awbHeader.EH_AgentApprovalNumber = ZString.Empty;
				AssertHasMessageErrors(awbHeader.EH_AgentApprovalNumberInfo);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);

				awbHeader.EH_AgentApprovalNumber = "BLAH123";
				AssertNoMessageErrors(awbHeader.EH_AgentApprovalNumberInfo);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);

				var line = awbHeader.CargoSecurityScreeningMethods[0];
				line.EAS_ScreeningMethod = "UNK";
				AssertHasMessageErrors(line.EAS_ScreeningMethodInfo);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertHasError(AWBActions.PrintConsignmentSecurityDeclarationInfo, "The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".");

				line.EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;
				AssertNoMessageErrors(line.EAS_ScreeningMethodInfo);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);
			}
		}

		public void TestPrintConsignmentSecurityDeclarationValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				awbActions.PrintConsignmentSecurityDeclaration = true;
				AssertHasError("Should be invalid for US companies", awbActions.PrintConsignmentSecurityDeclarationInfo, ErrorCSDNotApplicableForUS);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				awbActions.PrintConsignmentSecurityDeclaration = true;
				AssertNoError("Should be valid for non-US companies", awbActions.PrintConsignmentSecurityDeclarationInfo, ErrorCSDNotApplicableForUS);
			}
		}

		public void TestDoPrintConsignmentSecurityDeclaration()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			Consol.JK_IsNeutralMaster = ZBool.True;
			AWBActions.MAWBPrinter = printer.PK;
			AWBActions.PrintConsignmentSecurityDeclaration = ZBool.True;

			AWBActions.DoPrintConsignmentSecurityDeclaration();

			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);

			AssertEquals("1 print job is created for a Consignment Security Declaration", 1, printJobs.Length);
		}

		public void TestDoPrintConsignmentSecurityDeclaration_EPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var ePrintEmailAddress = "email@domain.com";

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				AWBActions.PrintConsignmentSecurityDeclaration = ZBool.True;
				AWBActions.MAWBUseEPrint = ZBool.True;
				AWBActions.DoPrintConsignmentSecurityDeclaration();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

				AssertEquals("1 print job is created for a Consignment Security Declaration", 1, printJobs.Length);

				AssertEquals(nameof(PrintType.EML), printJobs[0].SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(ZGuid.Empty, printJobs[0].SP_SQ);
				AssertEquals(ePrintEmailAddress, printJobs[0].SP_Destination);
			}
		}

		public void TestPrintConsignmentSecurityDeclarationDefaulting()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertNotNull("Precondition: AWBHeader", consol.AWBHeader);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SouthAfrica))
			{
				consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
				var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				Factory.Save();

				var supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is allowed to be included by this country", true, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				AssertEquals("Security Declaration is not defaulted", false, awbActions.PrintConsignmentSecurityDeclaration);
				awbActions.SaveSettings();

				awbActions.PrintConsignmentSecurityDeclaration = true;
				consol.AWBHeader.EH_FinalizationDate = new DateTime(2001, 1, 1);
				awbActions.SaveSettings();

				awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				AssertEquals("Security Declaration value is NOT restored", false, awbActions.PrintConsignmentSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
				var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				Factory.Save();

				var supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is allowed to be included by this country", true, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				AssertEquals("Security Declaration is not defaulted", false, awbActions.PrintConsignmentSecurityDeclaration);
				awbActions.SaveSettings();

				awbActions.PrintConsignmentSecurityDeclaration = true;
				consol.AWBHeader.EH_FinalizationDate = new DateTime(2001, 1, 1);
				awbActions.SaveSettings();

				awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				AssertEquals("Security Declaration value is NOT restored", false, awbActions.PrintConsignmentSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
				var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				Factory.Save();

				var supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is allowed to be included by this country", false, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				AssertEquals("Security Declaration is not defaulted", false, awbActions.PrintConsignmentSecurityDeclaration);
				awbActions.SaveSettings();

				awbActions.PrintConsignmentSecurityDeclaration = true;
				consol.AWBHeader.EH_FinalizationDate = new DateTime(2001, 1, 1);
				awbActions.SaveSettings();

				awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				AssertEquals("Security Declaration value is NOT restored", false, awbActions.PrintConsignmentSecurityDeclaration);
			}
		}

		[TestDate(2023, 7, 17, 2, 3, 00)]
		public void TestPrintFinalMasterUpdateSecurityStatusIssueDate()
		{
			ZDateTime originalTime = ZDateTime.Now;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			Factory.Save();

			consol.JK_OverrideWaybillDefaults = false;
			consol.JK_OverrideSecurityDeclarationDefaults = false;

			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals("Precondition", ZDateTime.Empty, consol.AWBHeader.EH_FinalizationDate);
			AssertEquals("Precondition", ZDateTime.Empty, consol.AWBHeader.EH_SecurityStatusIssueDate);

			awbActions.PerformAllActions();
			consol.UpdateAWBPrinted();
			var consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consolInNewFactory.PopulateAWB();
			AssertEquals(originalTime, consolInNewFactory.JK_MasterBillIssueDate);
			AssertEquals(originalTime, consolInNewFactory.AWBHeader.EH_AWBIssueDate);
			AssertEquals(originalTime, consolInNewFactory.AWBHeader.EH_FinalizationDate);
			AssertEquals(originalTime, consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			consol.IsCSDValuesOverriddenProperty = true;
			consol.AWBHeader.EH_SecurityStatusIssueDate = originalTime;
			TestDateAttribute.AddMinutes(1);
			var now = originalTime.AddMinutes(1);
			awbActions.PrintConsignmentSecurityDeclaration = true;
			awbActions.PerformAllActions();
			consol.UpdateAWBPrinted();
			consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals(originalTime, consolInNewFactory.JK_MasterBillIssueDate);
			AssertEquals(originalTime, consolInNewFactory.AWBHeader.EH_AWBIssueDate);
			AssertEquals("17-Jul-23 02:04:00", now, consolInNewFactory.AWBHeader.EH_FinalizationDate);
			AssertEquals("17-Jul-23 02:03:00", originalTime, consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			consol.IsCSDValuesOverriddenProperty = false;
			awbActions.PerformAllActions();
			consol.UpdateAWBPrinted();
			consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals(originalTime, consolInNewFactory.JK_MasterBillIssueDate);
			AssertEquals(originalTime, consolInNewFactory.AWBHeader.EH_AWBIssueDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_FinalizationDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			SetConsolDateValues(new ZDateTime(2023, 1, 1));
			Factory.Save();
			awbActions.PerformAllActions();
			consol.UpdateAWBPrinted();
			consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.AWBHeader.EH_AWBIssueDate);
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.JK_MasterBillIssueDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_FinalizationDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			consolInNewFactory.PopulateAWB();
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.AWBHeader.EH_AWBIssueDate);
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.JK_MasterBillIssueDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_FinalizationDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			SetConsolDateValues(new ZDateTime(2023, 1, 1));
			awbActions.PrintConsignmentSecurityDeclaration = false;
			awbActions.PerformAllActions();
			consol.UpdateAWBPrinted();
			consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			consolInNewFactory.PopulateAWB();
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.JK_MasterBillIssueDate);
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.AWBHeader.EH_AWBIssueDate);
			AssertEquals(now, consolInNewFactory.AWBHeader.EH_FinalizationDate);
			AssertEquals(new ZDateTime(2023, 1, 1), consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			void SetConsolDateValues(ZDateTime theDate)
			{
				consol.JK_MasterBillIssueDate = theDate;
				consol.AWBHeader.EH_FinalizationDate = theDate;
				consol.AWBHeader.EH_AWBIssueDate = theDate;
				consol.AWBHeader.EH_SecurityStatusIssueDate = theDate;
			}
		}

		[TestDate(2023, 7, 17, 2, 3, 00)]
		public void TestDoSendFWBUpdateSecurityStatusIssueDate()
		{
			ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CargoIMPSentMessageVersionsList.Codes.FWBv16_FHLv4);
			var now = ZDateTime.Now;

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.AWBHeader.EH_SecurityStatusIssueDate = new ZDateTime(2023, 1, 1);
			Consol.JK_OverrideWaybillDefaults = false;
			Consol.JK_OverrideSecurityDeclarationDefaults = false;
			Factory.Save();

			AWBActions.SendFWB = true;
			AWBActions.IncludeSecurityDeclaration = true;
			AWBActions.PrintConsignmentSecurityDeclaration = false;

			AWBActions.DoSendFWB();

			var consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(Consol.PK);
			AssertEquals("Precondition: EH_SecurityStatusIssueDate should be the current date", now, consolInNewFactory.AWBHeader.EH_SecurityStatusIssueDate);

			var message = Consol.CIMEDIMessages.GetLatestTransmittedMessage();
			AssertContains("///SD/17JUL230203", message.EM_MessageText);
		}

		public void TestSwitchToSendAirlineMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			Factory.Save();

			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
			awbActions.SendFWB = true;

			var mockManager = new mockAirlineMsgManager();
			using (ObjectFactory.Substitute<IAirlineMessagingManager>(mockManager))
			{
				using (FreightDataRegistry.Instance.MessagingViaPelicanServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					mockManager.IsPelicanCalled = false;
					awbActions.PerformAllActions();
					AssertEquals("Pelican API should be called", true, mockManager.IsPelicanCalled);
				}

				using (FreightDataRegistry.Instance.MessagingViaPelicanServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					mockManager.IsPelicanCalled = false;
					awbActions.PerformAllActions();
					AssertEquals("Pelican API should not be called", false, mockManager.IsPelicanCalled);
				}
			}
		}

		public void TestSendAirlineMessageShouldReceiveAddInfo()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			Factory.Save();

			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
			awbActions.SendFWB = true;
			var mockManager = new mockAirlineMsgManager();

			using (ObjectFactory.Substitute<IAirlineMessagingManager>(mockManager))
			{
				using (FreightDataRegistry.Instance.MessagingViaPelicanServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Short codes to iterate all combination of TestCase(bool, bool, bool, bool)
					var bools = new[] { false, true };
					foreach (var combination in bools.SelectMany(a => bools.SelectMany(b => bools.SelectMany(c => bools, (c, d) => new { a, b, c, d }))))
					{
						TestCase(mockManager, combination.a, combination.b, combination.c, combination.d);
					}
				}
			}

			void TestCase(mockAirlineMsgManager mockManager, bool sendFHL, bool includeECSD, bool sendFWBOrFHLToAirlineBasedOnMAWBPrefix, bool sendFWBNatureAndQuantityOfGoodsType)
			{
				using (ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, sendFWBOrFHLToAirlineBasedOnMAWBPrefix))
				using (FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, sendFWBNatureAndQuantityOfGoodsType))
				{
					awbActions.SendFHL = sendFHL;
					awbActions.IncludeSecurityDeclaration = includeECSD;
					awbActions.PerformAllActions();
					AssertEquals("SendFWBOrFHLToAirlineBasedOnMAWBPrefix should be correct", sendFWBOrFHLToAirlineBasedOnMAWBPrefix.ToString(),
						mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "SendFWBOrFHLToAirlineBasedOnMAWBPrefix").Value);
					AssertEquals("SendFWBNatureAndQuantityOfGoodsType should be correct", sendFWBNatureAndQuantityOfGoodsType.ToString(),
						mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "SendFWBNatureAndQuantityOfGoodsType").Value);
					AssertEquals("SendFHL should be correct", sendFHL.ToString(),
						mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "SendFHL").Value);
					AssertEquals("IncludeECSD should be correct", includeECSD.ToString(),
						mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "IncludeECSD").Value);
				}
			}
		}

		public void TestSendAirlineMessageIncludeImpVersion()
		{
			new List<string> { "001", "002", "003" }.ForEach(x =>
			{
				var refAirline = RefAirline.LoadFromAirlinePrefix(Factory, x);
				if (refAirline == null)
				{
					refAirline = Factory.NewWithValidTestData<RefAirline>();
					refAirline.RM_AirlinePrefix = x;
					refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = x;
				}
			});

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			Factory.Save();

			var mockManager = new mockAirlineMsgManager();
			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
			awbActions.SendFWB = true;
			var airImpVersionConfig = new CargoImpVersionConfiguration();
			airImpVersionConfig.AirlineImpVersionMappings.Add(new AirlineImpVersion { AirlinePrefix = "001", ImpVersion = "V17" });
			airImpVersionConfig.AirlineImpVersionMappings.Add(new AirlineImpVersion { AirlinePrefix = "002", ImpVersion = "V16" });

			using (ObjectFactory.Substitute<IAirlineMessagingManager>(mockManager))
			using (FreightDataRegistry.Instance.MessagingViaPelicanServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.AirlineMessagingCargoImpVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, airImpVersionConfig))
			{
				consol.JK_MasterBillNum = "00188888888";
				awbActions.PerformAllActions();
				AssertEquals("001 should use V17 as per setting", "V17",
					mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "CargoIMPVersion").Value);

				consol.JK_MasterBillNum = "00288888888";
				awbActions.PerformAllActions();
				AssertEquals("002 should use V16 as per setting", "V16",
					mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "CargoIMPVersion").Value);

				consol.JK_MasterBillNum = "00388888888";
				awbActions.PerformAllActions();
				AssertEquals("003 should use V16 as the fall back value", "V16",
					mockManager.receivedAdditionalInfoCollection.First(x => x.Key == "CargoIMPVersion").Value);
			}
		}

		public void TestSendAirlineMessageSuccessAndFailure()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			Factory.Save();

			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
			awbActions.SendFWB = true;

			var mockManager = new mockAirlineMsgManager();
			using (FreightDataRegistry.Instance.MessagingViaPelicanServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute<IAirlineMessagingManager>(mockManager))
			{
				var ret = true;
				mockManager.SetupMockResponse(true, null);
				ret = awbActions.PerformAllActions();
				AssertEquals(true, ret);

				mockManager.SetupMockResponse(false, "Input XML is invalid.");
				ret = awbActions.PerformAllActions();
				AssertEquals(false, ret);
				AssertEquals("Airline Messaging Engine has returned the following validation error/rejection during data processing. Please review and resubmit your airline messages for this consol as the data has not been sent to the Airline.\r\n\r\nInput XML is invalid.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class mockAirlineMsgManager : IAirlineMessagingManager
		{
			public bool IsPelicanCalled;
			public KeyValuePair<string, string>[] receivedAdditionalInfoCollection;

			bool responseReturnValue = true;
			string failureReason;

			public void SetupMockResponse(bool responseReturnValue, string failureReason)
			{
				this.failureReason = failureReason;
				this.responseReturnValue = responseReturnValue;				
			}

			public bool Send<T>(T documentObject, KeyValuePair<string, string>[] additionalInfoCollection, out string failureReason)
			{
				IsPelicanCalled = true;
				failureReason = this.failureReason;
				receivedAdditionalInfoCollection = additionalInfoCollection;
				return responseReturnValue;
			}
		}

		public void TestDoPrintNeutralMasterAirWaybill()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			Consol.JK_IsNeutralMaster = ZBool.True;
			AWBActions.PrintMasterAirWaybill = ZBool.True;
			AWBActions.MAWBPrinter = printer.PK;
			AWBActions.LaserAWB = ZBool.False;
			AWBActions.CarrierAWB = ZBool.False;
			AWBActions.NeutralAWB = ZBool.True;

			AWBActions.DoPrintMasterAirWaybill();

			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);

			AssertEquals("2 separate print jobs are created for a Neutral AWB", 2, printJobs.Length);
			AssertEquals("IsPrintingFinalMAWB on AWB Header of Consol set to true", ZBool.True, Consol.AWBHeader.IsPrintingFinalNeutralMAWB);
			foreach (var job in printJobs)
			{
				AssertEquals(nameof(PrintType.PRN), job.SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, job.SP_EmailAttachmentFormat);
				AssertEquals(printer.PK, job.SP_SQ);
				AssertEquals(string.Empty, job.SP_FaxDestination);
			}
		}

		public void TestDoPrintNeutralMasterAirWaybill_EPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var ePrintEmailAddress = "email@domain.com";

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				Consol.JK_IsNeutralMaster = ZBool.True;
				AWBActions.PrintMasterAirWaybill = ZBool.True;
				AWBActions.MAWBUseEPrint = ZBool.True;
				AWBActions.LaserAWB = ZBool.False;
				AWBActions.CarrierAWB = ZBool.False;
				AWBActions.NeutralAWB = ZBool.True;

				AWBActions.DoPrintMasterAirWaybill();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

				AssertEquals("1 print job is created for a Neutral AWB", 1, printJobs.Length);
				AssertEquals("IsPrintingFinalMAWB on AWB Header of Consol set to true", ZBool.True, Consol.AWBHeader.IsPrintingFinalNeutralMAWB);

				AssertEquals(nameof(PrintType.EML), printJobs[0].SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(ZGuid.Empty, printJobs[0].SP_SQ);
				AssertEquals(ePrintEmailAddress, printJobs[0].SP_Destination);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
					AssertEquals("2 sheets are created for a Neutral AWB", 2, excelInterface.WorkSheets.Count(sheet => !sheet.IsHidden));
				}
			}
		}

		public void TestDoPrintCarrierMasterAirWaybill()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			Consol.JK_IsNeutralMaster = ZBool.False;
			AWBActions.PrintMasterAirWaybill = ZBool.True;
			AWBActions.MAWBPrinter = printer.PK;
			AWBActions.LaserAWB = ZBool.False;
			AWBActions.CarrierAWB = ZBool.True;
			AWBActions.NeutralAWB = ZBool.False;

			AWBActions.DoPrintMasterAirWaybill();

			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);

			AssertEquals("2 separate print jobs are created for a Carrier AWB", 2, printJobs.Length);
			AssertEquals("IsPrintingFinalMAWB on AWB Header of Consol should be false", ZBool.False, Consol.AWBHeader.IsPrintingFinalNeutralMAWB);

			foreach (var job in printJobs)
			{
				AssertEquals(nameof(PrintType.PRN), job.SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, job.SP_EmailAttachmentFormat);
				AssertEquals(printer.PK, job.SP_SQ);
				AssertEquals(string.Empty, job.SP_FaxDestination);
			}
		}

		public void TestDoPrintCarrierMasterAirWaybill_EPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var ePrintEmailAddress = "email@domain.com";

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				Consol.JK_IsNeutralMaster = ZBool.False;
				AWBActions.PrintMasterAirWaybill = ZBool.True;
				AWBActions.MAWBUseEPrint = ZBool.True;
				AWBActions.LaserAWB = ZBool.False;
				AWBActions.CarrierAWB = ZBool.True;
				AWBActions.NeutralAWB = ZBool.False;

				AWBActions.DoPrintMasterAirWaybill();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

				AssertEquals("1 print job is created for a Carrier AWB.", 1, printJobs.Length);
				AssertEquals("IsPrintingFinalMAWB on AWB Header of Consol should be false", ZBool.False, Consol.AWBHeader.IsPrintingFinalNeutralMAWB);

				AssertEquals(nameof(PrintType.EML), printJobs[0].SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(ZGuid.Empty, printJobs[0].SP_SQ);
				AssertEquals(ePrintEmailAddress, printJobs[0].SP_Destination);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
					AssertEquals("2 sheet are created for a Carrier AWB", 2, excelInterface.WorkSheets.Count(sheet => !sheet.IsHidden));
				}
			}
		}

		public void TestDoPrintLaserMasterAirWaybill()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			Consol.JK_IsNeutralMaster = ZBool.True;
			AWBActions.PrintMasterAirWaybill = ZBool.True;
			AWBActions.MAWBPrinter = printer.PK;
			AWBActions.LaserAWB = ZBool.True;
			AWBActions.CarrierAWB = ZBool.False;
			AWBActions.NeutralAWB = ZBool.False;

			AWBActions.DoPrintMasterAirWaybill();

			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);

			AssertEquals("9 separate print jobs are created for a Laser AWB - 8 AWBs + 1 Cover Page", 9, printJobs.Length);
			AssertEquals("IsPrintingFinalMAWB on AWB Header of Consol set to true", ZBool.True, Consol.AWBHeader.IsPrintingFinalNeutralMAWB);

			foreach (var job in printJobs)
			{
				AssertEquals(nameof(PrintType.PRN), job.SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, job.SP_EmailAttachmentFormat);
				AssertEquals(printer.PK, job.SP_SQ);
				AssertEquals(string.Empty, job.SP_FaxDestination);
			}
		}

		#region TestFindFirstApplicableDocumentCommand

		public void TestFindFirstApplicableDocumentCommand()
		{
			CreateTestDocument("\"<JK_AgentType>\" == \"OTH\"");
			CreateTestDocument("\"<JK_AgentType>\" == \"CLD\"");
			CreateTestDocument("\"<JK_AgentType>\" == \"AGT\"");
			Factory.Save();

			Consol.JK_AgentType = AgentType.Agent;
			var awbActions = new AWBActionsTestClassWithMenuPath(Consol, Business.AWB.AWBActions.ActionsModeType.All, "AWB2");
			var command = awbActions.CreateDocumentCommand("Test Doc");

			CombineAssertions(() =>
			{
				AssertNotNull(command);
				AssertEquals("IsApplicable should be true.", ZBool.True, command.IsApplicable);
				AssertEquals("Filter list should be \"<JK_AgentType>\" == \"AGT\".", "\"<JK_AgentType>\" == \"AGT\"", command.SU_FilterList);
				AssertNotNull(command.Parent);
			});

			Consol.JK_AgentType = AgentType.CoLoad;
			awbActions = new AWBActionsTestClassWithMenuPath(Consol, Business.AWB.AWBActions.ActionsModeType.All, "AWB2");
			command = awbActions.CreateDocumentCommand("Test Doc");

			CombineAssertions(() =>
			{
				AssertNotNull(command);
				AssertEquals("IsApplicable should be true.", ZBool.True, command.IsApplicable);
				AssertEquals("Filter list should be \"<JK_AgentType>\" == \"CLD\".", "\"<JK_AgentType>\" == \"CLD\"", command.SU_FilterList);
				AssertNotNull(command.Parent);
			});
		}

		public void TestFindFirstApplicableDocumentCommand_BulkConsols()
		{
			CreateTestDocument("\"<JK_AgentType>\" == \"OTH\"");
			CreateTestDocument("\"<JK_AgentType>\" == \"CLD\"");
			CreateTestDocument("\"<JK_AgentType>\" == \"AGT\"");
			Factory.Save();

			Consol.JK_AgentType = AgentType.Agent;
			var awbActions = new AWBActionsTestClassWithMenuPath(Consol, Business.AWB.AWBActions.ActionsModeType.All, "AWB2");
			var command = awbActions.CreateDocumentCommand("Test Doc");

			CombineAssertions(() =>
			{
				AssertNotNull(command);
				AssertNotNull(command.Parent);
				AssertEquals(Consol, command.Parent);
			});

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = AgentType.Agent;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_IsNeutralMaster = true;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_MasterBillNum = "08112345679";

			var awbActions2 = new AWBActionsTestClassWithMenuPath(consol2, Business.AWB.AWBActions.ActionsModeType.All, "AWB2");
			var command2 = awbActions2.CreateDocumentCommand("Test Doc");

			CombineAssertions(() =>
			{
				AssertNotNull(command2);
				AssertNotNull(command2.Parent);
				AssertEquals(consol2, command2.Parent);
			});
		}

		void CreateTestDocument(string filterList)
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_MenuType = "DOC";
			menuItem.SU_MenuPath = "AWB";
			menuItem.SU_FilterList = filterList;
		}

		#endregion

		public void TestDoPrintLaserMasterAirWaybill_EPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var ePrintEmailAddress = "email@domain.com";

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				Consol.JK_IsNeutralMaster = ZBool.True;
				AWBActions.PrintMasterAirWaybill = ZBool.True;
				AWBActions.MAWBUseEPrint = ZBool.True;
				AWBActions.LaserAWB = ZBool.True;
				AWBActions.CarrierAWB = ZBool.False;
				AWBActions.NeutralAWB = ZBool.False;

				AWBActions.DoPrintMasterAirWaybill();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());

				AssertEquals("1 print job is created for a Laser AWB", 1, printJobs.Length);
				AssertEquals("IsPrintingFinalMAWB on AWB Header of Consol set to true", ZBool.True, Consol.AWBHeader.IsPrintingFinalNeutralMAWB);

				AssertEquals(nameof(PrintType.EML), printJobs[0].SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(ZGuid.Empty, printJobs[0].SP_SQ);
				AssertEquals(ePrintEmailAddress, printJobs[0].SP_Destination);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
					AssertEquals("9 sheets are created for a Laser AWB - 8 AWBs + 1 Cover Page", 9, excelInterface.WorkSheets.Count(sheet => !sheet.IsHidden));
				}
			}
		}

		public void TestPrintMasterAirWayBillGroup()
		{
			AWBActions.PrintMasterAirWaybill = ZBool.False;
			Assertion.Assert(AWBActions.NeutralAWBInfo.ReadOnly);
			Assertion.Assert(AWBActions.LaserAWBInfo.ReadOnly);
			Assertion.Assert(AWBActions.MAWBPrinterInfo.ReadOnly);
			Assertion.Assert(!AWBActions.PrintMasterAirWaybillInfo.HasWarnings());
			AWBActions.PrintMasterAirWaybill = ZBool.True;
			Assertion.Assert(!AWBActions.NeutralAWBInfo.ReadOnly);
			Assertion.Assert(!AWBActions.LaserAWBInfo.ReadOnly);
			Assertion.Assert(!AWBActions.MAWBPrinterInfo.ReadOnly);
			Assertion.Assert(!AWBActions.PrintMasterAirWaybillInfo.HasWarnings());
			AWBActions.DatePrinted = "13 Jul 2004";
			AWBActions.PrintMasterAirWaybill = ZBool.True;
			Assertion.Assert(AWBActions.PrintMasterAirWaybillInfo.HasWarnings());
		}

		public void TestPrePrintValidation()
		{
			ForwardingShipment shipment = Consol.Shipments[0];
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.Consignor.OH_IsDebtor = true;
			shipment.Consignor.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			shipment.Consignor.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			StmMenuItem neutralMAWBMenuItem = MenuItemLoader("Neutral MAWB", "AWB");
			StmMenuItem hawbBarcodeLabelsMenuItem = MenuItemLoader("HAWB Barcode Label 5 Inch", "AWB");
			StmMenuItem awbBarcodeLabelMenuItem = MenuItemLoader("AWB Barcode Label", "AWB");

			neutralMAWBMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			hawbBarcodeLabelsMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			awbBarcodeLabelMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);

			AWBActions.NeutralAWB = true;
			AWBActions.LaserAWB = false;

			AWBActions.PrintBarcodeLabel = true;
			AWBActions.PrintHAWBBarcodeLabels = true;
			AWBActions.PrintMasterAirWaybill = true;

			AssertNoWarnings("PrintBarcodeLabelInfo should have no errors", AWBActions.PrintBarcodeLabelInfo);
			AssertNoWarnings("PrintHAWBBarcodeLabelsInfo should have no errors", AWBActions.PrintHAWBBarcodeLabelsInfo);
			AssertNoWarnings("PrintMasterAirWaybillInfo should have no errors", AWBActions.PrintMasterAirWaybillInfo);

			AWBActions.PrintBarcodeLabel = false;
			AWBActions.PrintHAWBBarcodeLabels = false;
			AWBActions.PrintMasterAirWaybill = false;

			neutralMAWBMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			hawbBarcodeLabelsMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			awbBarcodeLabelMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);

			AWBActions.PrintBarcodeLabel = true;
			AWBActions.PrintHAWBBarcodeLabels = true;
			AWBActions.PrintMasterAirWaybill = true;

			AssertHasWarningContaining(AWBActions.PrintBarcodeLabelInfo, "on Credit Hold");
			AssertHasWarningContaining(AWBActions.PrintHAWBBarcodeLabelsInfo, "on Credit Hold");
			AssertHasWarningContaining(AWBActions.PrintMasterAirWaybillInfo, "on Credit Hold");
		}

		public void TestPrePrintValidation_DoNotIncludeCargoSecurityValidation()
		{
			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "008";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "12345678";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_MasterBillNum = "00812345678";

			var awb = consol.AWBHeader;
			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 7;
			shipment.JS_HouseBill = "shipment";
			shipment.JS_InspectionTypeCode = "UNK";

			Factory.Save();

			var awbHeader = consol.AWBHeader;
			awbHeader.EH_ShipperName = "FRED";
			awbHeader.EH_ShipperAddress = "HERE";
			awbHeader.EH_ShipperPlace = "THERE";
			awbHeader.EH_ShipperCountryCode = "NZ";
			awbHeader.EH_ConsigneeName = "JOHN";
			awbHeader.EH_ConsigneeAddress = "THOMPSON";
			awbHeader.EH_ConsigneePlace = "CONTENDER";
			awbHeader.EH_ConsigneeCountryCode = "AU";
			awbHeader.EH_By1st = "QF";
			awbHeader.EH_AirportOfDestinationCode = "AUS";
			awbHeader.EH_ChargesCode = "PP";
			awbHeader.EH_AgentApprovalNumber = ZString.Empty;

			awbHeader.AWBRateLines[0].ER_GrossWeight = 32;

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "123456-7";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "NOT ALL HERE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "TIMBUKTU";

			var line = awbHeader.ExportAWBSecurityStatusLines.AddNew();
			line.EAS_ScreeningMethod = "UNK";

			AssertHasMessageError("prerequite: CSD part of header has a message error",
				awbHeader.EH_AgentApprovalNumberInfo,
				"A Security Declaration cannot be issued for this consignment because the Sending Agent does not match the login (issuing) company.");

			AssertHasMessageError("prerequite: CSD line has a message error",
				line.EAS_ScreeningMethodInfo,
				"Shipment/s  require a screening status to be recorded for all packages, in line with Air Cargo Piece Level Security Screening legislation, unless the shipment is APP.");

			var actions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);

			actions.ValidateConsolExportAWBHeader();

			AssertNoErrors("MessageErrors on CSD line should not prevent sending FWB", actions.SendFWBInfo);
		}

		public void TestPrePrintValidation_AllowToSendFWBsWithErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				AssertEquals("Precondition: Registry.AllowUsersToSendFWBsWithErrors", false, ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.Value);

				var shipment = Consol.Shipments.AddNew();
				shipment.JS_InspectionTypeCode = "UNK";

				var awbHeader = Consol.AWBHeader;
				awbHeader.Populate();
				awbHeader.EH_ShipperName = "FRED";
				awbHeader.EH_ShipperAddress = ZString.Empty;
				awbHeader.EH_ShipperPlace = "THERE";
				awbHeader.EH_ShipperCountryCode = "NZ";
				awbHeader.EH_ConsigneeName = "JOHN";
				awbHeader.EH_ConsigneeAddress = "THOMPSON";
				awbHeader.EH_ConsigneePlace = "CONTENDER";
				awbHeader.EH_ConsigneeCountryCode = "AU";
				awbHeader.EH_By1st = "QF";
				awbHeader.EH_AirportOfDestinationCode = "AUS";
				awbHeader.EH_ChargesCode = "PP";
				awbHeader.EH_AgentApprovalNumber = ZString.Empty;

				awbHeader.AWBRateLines[0].ER_GrossWeight = 32;

				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "123456-7";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "NOT ALL HERE";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "TIMBUKTU";

				var line = awbHeader.ExportAWBSecurityStatusLines.AddNew();
				line.EAS_ScreeningMethod = "UNK";

				AssertHasMessageError("Header has message errors",
					awbHeader.EH_AgentApprovalNumberInfo,
					"Enter the identifier of the Regulated Agent issuing the security status.");

				var actions = new ConsolAWBActions(Consol, Business.AWB.AWBActions.ActionsModeType.All);

				actions.ValidateConsolExportAWBHeader();

				AssertHasError(actions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);

				ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				Factory.Save();
				actions.SendFWBInfo.ClearAllNotifications();
				awbHeader.AWBRateLine1.ER_WeightInLBsOrKGs = "";
				actions.ValidateConsolExportAWBHeader();

				AssertNoErrors("Now we should be able to send FWB with errors", actions.SendFWBInfo);
				AssertHasWarning("Warning should be added", actions.SendFWBInfo, ConsolAWBActions.WarningFWBContainsMessageErrors);
			}
		}

		public void TestPrePrintValidation_UnknownPackLinesFromMRACountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var expectedWarning = "This Consol is destined for United States, and as part of TSA's National Security Program (NSCP), MRAs are currently in place between United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.";
				var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);

				using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
				{
					serviceManagerQuerier.Setup(m => m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender)).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

					using (ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetTemporaryValue(Guid.Empty,
						Guid.Empty, Guid.Empty, true))
					{
						Consol.JK_RL_NKLoadPort = "DEFRA";
						Consol.JK_RL_NKDischargePort = "USCHI";

						var shipment = Consol.Shipments[0];
						shipment.AviationSecurity.ResetSupplyChainSecurityConfigurationForTesting();
						shipment.JS_RL_NKOrigin = "DEFRA";
						shipment.JS_RL_NKDestination = "USCHI";
						shipment.JS_InspectionTypeCode = "MAI";

						var pack1 = shipment.OuterPackLines.AddNew();
						var pack2 = shipment.OuterPackLines.AddNew();

						pack1.JL_InspectionTypeCode = "MAI";
						pack2.JL_InspectionTypeCode = "UNK";

						var actions = new ConsolAWBActions(Consol, Business.AWB.AWBActions.ActionsModeType.All);
						actions.ValidateSendFWB();
						AssertHasWarning(actions.SendFWBInfo, expectedWarning);

						pack2.JL_InspectionTypeCode = "MAI";
						pack1.JL_InspectionTypeCode = "";
						shipment.JS_InspectionTypeCode = "APP";

						actions.ValidateSendFWB();
						AssertNoWarning("Blank inspection type is valid for approved shipment", actions.SendFWBInfo, expectedWarning);

						pack1.JL_InspectionTypeCode = "UNK";
						actions.ValidateSendFWB();
						AssertHasWarning(actions.SendFWBInfo, expectedWarning);
					}
				}

				serviceManagerQuerier.Verify(
					m => m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender),
					Times.AtLeastOnce);
			}
		}

		public void TestPrePrintValidation_UnknownPackLines_AUExport()
		{
			var expectedWarning = "In line with Air Cargo Piece Level Security Screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.";
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier.Setup(m => m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender)).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.SetTemporaryValue(Guid.Empty,
					Guid.Empty, Guid.Empty, true))
				{
					Consol.JK_RL_NKDischargePort = "CNSHA";

					var shipment = Consol.Shipments[0];
					shipment.AviationSecurity.ResetSupplyChainSecurityConfigurationForTesting();
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "CNSHA";
					shipment.JS_InspectionTypeCode = "MAI";

					var pack1 = shipment.OuterPackLines.AddNew();
					var pack2 = shipment.OuterPackLines.AddNew();

					pack1.JL_InspectionTypeCode = "MAI";
					pack2.JL_InspectionTypeCode = "UNK";

					var actions = new ConsolAWBActions(Consol, Business.AWB.AWBActions.ActionsModeType.All);
					actions.ValidateSendFWB();
					AssertHasWarning(actions.SendFWBInfo, expectedWarning);

					pack2.JL_InspectionTypeCode = "MAI";
					pack1.JL_InspectionTypeCode = "";
					shipment.JS_InspectionTypeCode = "APP";

					actions.ValidateSendFWB();
					AssertNoWarning("Blank inspection type is valid for approved shipment", actions.SendFWBInfo, expectedWarning);

					pack1.JL_InspectionTypeCode = "UNK";
					actions.ValidateSendFWB();
					AssertHasWarning(actions.SendFWBInfo, expectedWarning);
				}

				serviceManagerQuerier.Verify(
					m => m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender),
					Times.AtLeastOnce());
			}
		}

		public void TestGetDocumentNamesToPrint()
		{
			AWBActions.PrintConsignmentSecurityDeclaration = false;
			AWBActions.PrintBarcodeLabel = AWBActions.PrintHAWBBarcodeLabels = AWBActions.PrintMasterAirWaybill = false;

			AssertEquals(0, AWBActions.DocumentNamesToPrint.Length);

			AWBActions.PrintBarcodeLabel = true;
			AssertContainsExactElementsInAnyOrder(new string[] { "AWB Barcode Label" }, AWBActions.DocumentNamesToPrint);

			AWBActions.PrintHAWBBarcodeLabels = true;
			AssertContainsExactElementsInAnyOrder(new string[] { "AWB Barcode Label", "HAWB Barcode Label 5 Inch" }, AWBActions.DocumentNamesToPrint);

			AWBActions.PrintMasterAirWaybill = true;
			AWBActions.CarrierAWB = true;
			AWBActions.LaserAWB = AWBActions.NeutralAWB = false;
			AWBActions.PrintConsignmentSecurityDeclaration = false;
			AssertContainsExactElementsInAnyOrder(new string[] { "AWB Barcode Label", "HAWB Barcode Label 5 Inch", "Carrier MAWB" }, AWBActions.DocumentNamesToPrint);

			AWBActions.CarrierAWB = false;
			AWBActions.LaserAWB = true;
			AssertContainsExactElementsInAnyOrder(new string[] { "AWB Barcode Label", "HAWB Barcode Label 5 Inch", "Laser MAWB" }, AWBActions.DocumentNamesToPrint);

			AWBActions.LaserAWB = false;
			AWBActions.NeutralAWB = true;
			AWBActions.PrintConsignmentSecurityDeclaration = true;
			AssertContainsExactElementsInAnyOrder(new string[] { "AWB Barcode Label", "HAWB Barcode Label 5 Inch", "Neutral MAWB", "Consignment Security Declaration" }, AWBActions.DocumentNamesToPrint);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Consol", AWBActions.HumanReadableName);
		}

		public void TestUseMenuPathToGetDocument()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				AWBActionsTestClassWithMenuPath awbActions = new AWBActionsTestClassWithMenuPath(Consol, Business.AWB.AWBActions.ActionsModeType.All, "AWB");
				DocumentCommand command = awbActions.CreateDocumentCommand("Laser MAWB");

				AssertEquals("AWB", command.SU_MenuPath);
				AssertEquals("Laser MAWB", command.SU_MenuName);

				awbActions = new AWBActionsTestClassWithMenuPath(Consol, Business.AWB.AWBActions.ActionsModeType.All, "AWB/Italy");
				command = awbActions.CreateDocumentCommand("Laser MAWB");

				AssertEquals("AWB/Italy", command.SU_MenuPath);
				AssertEquals("Laser MAWB", command.SU_MenuName);
			}
		}

		#region TestMAWBPrinter

		public void TestMAWBPrinter()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.PrintMasterAirWaybill = true;
			AWBActions.MAWBPrinter = ZGuid.Empty;
			Assert(AWBActions.MAWBPrinterInfo.HasErrors());

			AWBActions.MAWBPrinter = printer.PK;
			Assert(!AWBActions.MAWBPrinterInfo.HasErrors());

			AWBActions.MAWBPrinter = ZGuid.Empty;
			Assert(AWBActions.MAWBPrinterInfo.HasErrors());

			AWBActions.MAWBUseEPrint = true;
			Assert(!AWBActions.MAWBPrinterInfo.HasErrors());
		}

		public void TestMAWBPrinter_Readonlyness()
		{
			AWBActions.PrintMasterAirWaybill = false;
			AWBActions.MAWBUseEPrint = false;
			Assert(AWBActions.MAWBPrinterInfo.ReadOnly);

			AWBActions.PrintMasterAirWaybill = true;
			Assert(!AWBActions.MAWBPrinterInfo.ReadOnly);

			AWBActions.MAWBUseEPrint = true;
			Assert(AWBActions.MAWBPrinterInfo.ReadOnly);
		}

		public void TestMAWBUseEPrint()
		{
			AWBActions.PrintMasterAirWaybill = true;

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AWBActions.MAWBPrinter = ZGuid.NewZGuid();
				Assert(!AWBActions.MAWBPrinter.IsEmpty);

				AWBActions.MAWBUseEPrint = false;
				Assert(!AWBActions.MAWBPrinter.IsEmpty);
				AssertNoErrors(AWBActions.MAWBUseEPrintInfo);

				AWBActions.MAWBUseEPrint = true;
				Assert(AWBActions.MAWBPrinter.IsEmpty);
				AssertHasErrors(AWBActions.MAWBUseEPrintInfo);
			}

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@domain.com"))
			{
				AWBActions.MAWBPrinter = ZGuid.NewZGuid();
				Assert(!AWBActions.MAWBPrinter.IsEmpty);

				AWBActions.MAWBUseEPrint = false;
				Assert(!AWBActions.MAWBPrinter.IsEmpty);
				AssertNoErrors(AWBActions.MAWBUseEPrintInfo);

				AWBActions.MAWBUseEPrint = true;
				Assert(AWBActions.MAWBPrinter.IsEmpty);
				AssertNoErrors(AWBActions.MAWBUseEPrintInfo);
			}
		}

		public void TestMAWBUseEPrint_Readonlyness()
		{
			AWBActions.PrintMasterAirWaybill = false;
			Assert(AWBActions.MAWBUseEPrintInfo.ReadOnly);

			AWBActions.PrintMasterAirWaybill = true;
			Assert(!AWBActions.MAWBUseEPrintInfo.ReadOnly);
		}

		#endregion

		#region TestHAWBLabelPrinter

		public void TestHAWBLabelPrinter()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.PrintHAWBBarcodeLabels = true;
			AWBActions.HAWBLabelPrinter = ZGuid.Empty;
			Assert(AWBActions.HAWBLabelPrinterInfo.HasErrors());

			AWBActions.HAWBLabelPrinter = printer.PK;
			Assert(!AWBActions.HAWBLabelPrinterInfo.HasErrors());

			AWBActions.HAWBLabelPrinter = ZGuid.Empty;
			Assert(AWBActions.HAWBLabelPrinterInfo.HasErrors());

			AWBActions.HAWBLabelUseEPrint = true;
			Assert(!AWBActions.HAWBLabelPrinterInfo.HasErrors());
		}

		public void TestHAWBLabelPrinter_Readonlyness()
		{
			AWBActions.PrintHAWBBarcodeLabels = false;
			AWBActions.HAWBLabelUseEPrint = false;
			Assert(AWBActions.HAWBLabelPrinterInfo.ReadOnly);

			AWBActions.PrintHAWBBarcodeLabels = true;
			Assert(!AWBActions.HAWBLabelPrinterInfo.ReadOnly);

			AWBActions.HAWBLabelUseEPrint = true;
			Assert(AWBActions.HAWBLabelPrinterInfo.ReadOnly);
		}

		public void TestHAWBLabelUseEPrint()
		{
			AWBActions.PrintHAWBBarcodeLabels = true;

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AWBActions.HAWBLabelPrinter = ZGuid.NewZGuid();
				Assert(!AWBActions.HAWBLabelPrinter.IsEmpty);

				AWBActions.HAWBLabelUseEPrint = false;
				Assert(!AWBActions.HAWBLabelPrinter.IsEmpty);
				AssertNoErrors(AWBActions.HAWBLabelUseEPrintInfo);

				AWBActions.HAWBLabelUseEPrint = true;
				Assert(AWBActions.HAWBLabelPrinter.IsEmpty);
				AssertHasErrors(AWBActions.HAWBLabelUseEPrintInfo);
			}

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@domain.com"))
			{
				AWBActions.HAWBLabelPrinter = ZGuid.NewZGuid();
				Assert(!AWBActions.HAWBLabelPrinter.IsEmpty);

				AWBActions.HAWBLabelUseEPrint = false;
				Assert(!AWBActions.HAWBLabelPrinter.IsEmpty);
				AssertNoErrors(AWBActions.HAWBLabelUseEPrintInfo);

				AWBActions.HAWBLabelUseEPrint = true;
				Assert(AWBActions.HAWBLabelPrinter.IsEmpty);
				AssertNoErrors(AWBActions.HAWBLabelUseEPrintInfo);
			}
		}

		public void TestHAWBLabelUseEPrint_Readonlyness()
		{
			AWBActions.PrintHAWBBarcodeLabels = false;
			Assert(AWBActions.HAWBLabelUseEPrintInfo.ReadOnly);

			AWBActions.PrintHAWBBarcodeLabels = true;
			Assert(!AWBActions.HAWBLabelUseEPrintInfo.ReadOnly);
		}

		#endregion

		public void TestConsolOuterPacksCount()
		{
			AssertEquals("7", AWBActions.ConsolOuterPacksCount);
		}

		public void TestSendFWB_CargoImpServiceTask()
		{
			var developerStaff = Factory.NewWithValidTestData<GlbStaff>();
			developerStaff.GS_Code = "DEV";
			developerStaff.GS_LoginName = "developer";
			developerStaff.GS_IsDeveloper = true;

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "USR";
			user.GS_LoginName = "TestUser";

			Factory.Save();

			using (Env.SetTemporaryUserContext(developerStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Developer user", true, Env.CurrentUser.IsDeveloper);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.ServiceTaskIsInactive, false, true);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb, false, true);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily, true, false);
			}

			using (Env.SetTemporaryUserContext("CWSupport", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Support user", true, Env.CurrentUser.IsSupportUser);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.ServiceTaskIsInactive, false, true);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb, false, true);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily, true, false);
			}
			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Not developer user", false, Env.CurrentUser.IsDeveloper);
				AssertEquals("Not support user", false, Env.CurrentUser.IsSupportUser);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.ServiceTaskIsInactive, false, true);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb, false, true);
				AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily, true, false);
			}
		}

		void AssertTestSendFWB_CargoImpServiceTask(ServiceTaskStatus serviceTaskStatus, ZBool expectedSendFWB, ZBool expectedSendFWBHasError)
		{
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier.Setup(m => m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender)).Returns(serviceTaskStatus);

				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				ConsolAWBActions awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
				AssertEquals(expectedSendFWB, awbActions.SendFWB);

				awbActions.SendFWB = !expectedSendFWB;
				if (expectedSendFWBHasError)
				{
					if (Env.CurrentUser.IsSupportUser || Env.CurrentUser.IsDeveloper)
					{
						AssertHasWarning(awbActions.SendFWBInfo, ConsolAWBActions.ErrorCargoImpMessageServiceTaskIsNotActive);
						AssertNoError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorCargoImpMessageServiceTaskIsNotActive);
					}
					else
					{
						AssertNoWarning(awbActions.SendFWBInfo, ConsolAWBActions.ErrorCargoImpMessageServiceTaskIsNotActive);
						AssertHasError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorCargoImpMessageServiceTaskIsNotActive);
					}
				}
				else
				{
					AssertNoWarning(awbActions.SendFWBInfo, ConsolAWBActions.ErrorCargoImpMessageServiceTaskIsNotActive);
					AssertNoError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorCargoImpMessageServiceTaskIsNotActive);
				}

				serviceManagerQuerier.Verify(m =>
					m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender), Times.AtLeastOnce());
			}
		}

		public void TestSendFWB()
		{
			SendFWBBase();
		}

		public void TestJapanIncludedInFWBCountries_LoadingPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			var awbHeader = consol.AWBHeader;
			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

			awbActions.DateLastSent = "";
			awbActions.SendFWB = ZBool.False;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "JPAAE";
			shipment.JS_RL_NKDestination = "BR6MO";
			awbActions.RunPreSaveValidation();
			AssertHasWarning("Check that when Japan is added as a loading port, the warning appears", awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);

			shipment.JS_RL_NKOrigin = "AUSYD";
			awbActions.RunPreSaveValidation();
			AssertNoWarning("Check that when Japan is not added as the loading port (or any other FWB country), the warning doesn't appear", awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);
		}

		public void TestJapanIncludedInFWBCountries_DestinationPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			var awbHeader = consol.AWBHeader;
			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

			awbActions.DateLastSent = "";
			awbActions.SendFWB = ZBool.False;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "BR6MO";
			shipment.JS_RL_NKDestination = "JPAAE";
			awbActions.RunPreSaveValidation();
			AssertHasWarning("Check that when Japan is added as a destination port, the warning appears", awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);

			shipment.JS_RL_NKDestination = "AUSYD";
			awbActions.RunPreSaveValidation();
			AssertNoWarning("Check that when Japan is not added as the destination port (or any other FWB country), the warning doesn't appear", awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);
		}

		public void TestJapanIncludedInFWBCountries_ConnectingPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			var awbHeader = consol.AWBHeader;
			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

			awbActions.DateLastSent = "";
			awbActions.SendFWB = ZBool.False;
			consol.JK_RL_NKDischargePort = "BR6MO";
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "JPAAE";
			awbActions.RunPreSaveValidation();
			AssertHasWarning("Check that when Japan is added as a connecting port, the warning appears", awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);

			transport.JW_RL_NKDiscPort = "AUMEL";
			awbActions.RunPreSaveValidation();
			AssertNoWarning("Check that when Japan is not added as a connecting port (or any other FWB country), the warning doesn't appear", awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);
		}

		public void TestFWBFHLWarning_ACAS()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_AgentType = "DRT";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_HouseBill = "S00001111";
			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);

			awbActions.ValidateACASForFWB();
			AssertHasWarning(awbActions.SendFWBInfo, "The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFWBInfo, "The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFWBInfo, "The Customer Account Shipping Frequency/Volume could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFWBInfo, "The Customer Account Establishment Date could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFWBInfo, "The Customer Account Billing Type could not be determined for ACAS reporting requirements and will not be sent to the airline.");

			consol.JK_AgentType = "AGT";
			awbActions.SendFHL = true;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_HouseBill = "S00002222";

			awbActions.ValidateACASForFHL();

			AssertHasWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Customer Account Shipping Frequency/Volume could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Customer Account Establishment Date could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertHasWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Customer Account Billing Type could not be determined for ACAS reporting requirements and will not be sent to the airline.");

			var proxyOrgCusCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			proxyOrgCusCode.OK_RN_NKCodeCountry = "US";
			proxyOrgCusCode.OK_CodeType = "CCA";
			proxyOrgCusCode.SecuredCustomsRegNo = "USCCAREGNO";
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_SystemCreateTimeUtc = DateTime.Now;
			controllingCustomer.OH_IsCreditor = true;
			shipment1.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			shipment2.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			shipment1.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment2.JS_INCO = Constants.IncoTerms.FreeOnBoard;

			awbActions.SendFHLInfo.ClearAllNotifications();
			awbActions.ValidateACASForFHL();

			AssertNoWarnings(awbActions.SendFHLInfo);

			var refAirline = Factory.NewWithValidTestData<RefAirline>();
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "020";
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.CompanyData.OB_APAirlineAccountNumber = "Air Acc Num";
			carrierOrg.MiscServ.OM_RM_Airline = refAirline.PK;
			consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			consol.JK_RL_NKDischargePort = "USLAX";
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_SystemCreateTimeUtc = DateTime.Now;
			sendingForwarder.OH_IsCreditor = true;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			awbActions.SendFWBInfo.ClearAllNotifications();
			awbActions.ValidateACASForFWB();

			AssertNoWarning(awbActions.SendFWBInfo, "The Customer Account Holder and Customer Account Name could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertNoWarning(awbActions.SendFWBInfo, "The Customer Account Number and/or Customer Account Issuer could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertNoWarning(awbActions.SendFWBInfo, "The Customer Account Shipping Frequency/Volume could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertNoWarning(awbActions.SendFWBInfo, "The Customer Account Establishment Date could not be determined for ACAS reporting requirements and will not be sent to the airline.");
			AssertNoWarning(awbActions.SendFWBInfo, "The Customer Account Billing Type could not be determined for ACAS reporting requirements and will not be sent to the airline.");
		}

		public void TestFWBFHLWarning_ACAS_BioData()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.AWBHeader.EH_FinalizationDate = ZDateTime.Empty;
			consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Empty;
			consol.JK_RL_NKDischargePort = "USCHI";
			consol.JK_AgentType = "DRT";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_OuterPacks = 1;
			shipment1.JS_HouseBill = "S00001111";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_SystemCreateTimeUtc = DateTime.Now;
			controllingCustomer.OH_Category = "NAT";
			shipment1.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);

			awbActions.ValidateACASForFWB();
			AssertHasWarning(awbActions.SendFWBInfo, "The Biographic Data (e.g. passport number or drivers license of a Natural Person/Individual Organization type) could not be determined for ACAS reporting requirements and will not be sent to the airline.");

			consol.JK_AgentType = "AGT";
			awbActions.SendFHL = true;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_OuterPacks = 1;
			shipment2.JS_HouseBill = "S00002222";
			shipment2.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			awbActions.ValidateACASForFHL();

			AssertHasWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Biographic Data (e.g. passport number or drivers license of a Natural Person/Individual Organization type) could not be determined for ACAS reporting requirements and will not be sent to the airline.");

			var cusCode = controllingCustomer.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.SecuredCustomsRegNo = "UsPassport123";

			awbActions.SendFHLInfo.ClearAllNotifications();
			awbActions.ValidateACASForFHL();

			AssertNoWarning(awbActions.SendFHLInfo, "S00001111, S00002222 - The Biographic Data (e.g. passport number or drivers license of a Natural Person/Individual Organization type) could not be determined for ACAS reporting requirements and will not be sent to the airline.");

			awbActions.SendFWBInfo.ClearAllNotifications();
			awbActions.ValidateACASForFWB();
			AssertNoWarning(awbActions.SendFWBInfo, "The Biographic Data (e.g. passport number or drivers license of a Natural Person/Individual Organization type) could not be determined for ACAS reporting requirements and will not be sent to the airline.");
		}

		void SendFWBBase()
		{
			var serviceManagerQuerier = new Mock<IServiceManagerQuerier>(MockBehavior.Strict);
			using (ObjectFactory.Substitute(serviceManagerQuerier.Object))
			{
				serviceManagerQuerier.Setup(m => m.CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender)).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
				{
					Factory.Save();

					var mawb = Factory.New<JobMawb>();
					mawb.JM_Airline3DigitPrefix = "008";
					mawb.JM_GB = GlbBranch.CurrentBranch.PK;
					mawb.JM_MAWB = "12345678";
					mawb.JM_ServiceLevel = "STD";

					Factory.Save();

					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					consol.JK_IsNeutralMaster = true;
					consol.JK_RL_NKLoadPort = "JMKIN";

					consol.JK_MasterBillNum = "00812345678";

					var shipment = consol.Shipments.AddNew();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_OuterPacks = 7;
					shipment.JS_HouseBill = "shipment";
					shipment.JS_InspectionTypeCode = "APP";

					var awbHeader = consol.AWBHeader;
					var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

					awbHeader.EH_ShipperName = "FRED";
					awbHeader.EH_ShipperAddress = "HERE";
					awbHeader.EH_ShipperPlace = "THERE";
					awbHeader.EH_ShipperCountryCode = "NZ";
					awbHeader.EH_ConsigneeName = "JOHN";
					awbHeader.EH_ConsigneeAddress = "THOMPSON";
					awbHeader.EH_ConsigneePlace = "CONTENDER";
					awbHeader.EH_ConsigneeCountryCode = "AU";
					awbHeader.EH_By1st = "QF";
					awbHeader.EH_AirportOfDestinationCode = "AUS";
					awbHeader.EH_ChargesCode = "PP";
					awbHeader.AWBRateLines[0].ER_GrossWeight = 32;
					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "123456-7";
					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "NOT ALL HERE";
					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "TIMBUKTU";
					consol.JK_PrepaidCollect = "PP";
					GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";

					awbHeader.EH_AgentApprovalNumber = "1234";
					awbHeader.EH_AgentApprovalExpiryDate = ZDateTime.Today.AddDays(100);

					awbHeader.ExportAWBSecurityStatusLines.RemoveAndDeleteAll();

					var line = awbHeader.ExportAWBSecurityStatusLines.AddNew();
					line.EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;

					var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();
					securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

					awbHeader.RunPreSaveValidation();
					AssertNoMessageErrors(awbHeader);

					consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
					consol.JK_MasterBillNum = "00812345678";
					awbActions.SendFWB = ZBool.True;
					AssertHasError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentForColoadConsols);
					AssertNoError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);
					AssertHasError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutBranchHomePort);
					AssertNoWarnings(awbActions.SendFWBInfo);

					consol.JK_AgentType = Core.Constants.AgentType.Direct;
					consol.JK_MasterBillNum = "00812345678";
					awbActions.DateLastSent = "13 Jul 4003";
					GlbBranch.CurrentBranch.GB_RL_NKHomePort = "JMKIN";
					GlbBranch.CurrentBranch.HomePort.RL_IATA = "";
					awbActions.SendFWB = ZBool.True;
					AssertHasWarning(awbActions.SendFWBInfo, ConsolAWBActions.WarningCargoImpMessagesHaveAlreadyBeenSent);
					AssertNoWarning(awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);
					AssertHasError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutBranchHomePortIATACode);

					consol.Shipments[0].JS_RL_NKDestination = "USLAX";
					awbActions.DateLastSent = "";
					awbActions.SendFWB = ZBool.False;
					GlbBranch.CurrentBranch.HomePort.RL_IATA = "KIN";
					AssertHasWarning(awbActions.SendFWBInfo, awbActions.WarningFHLRegulationsMessage);
					AssertNoWarning(awbActions.SendFWBInfo, ConsolAWBActions.WarningCargoImpMessagesHaveAlreadyBeenSent);
					AssertNoErrors(awbActions.SendFWBInfo);

					consol.Shipments[0].JS_RL_NKDestination = "DEHAM";
					awbHeader.EH_AlsoNotifyName = "TELL ME MORE";
					awbHeader.EH_AlsoNotifyAddress = "";
					AssertHasMessageErrors(awbHeader.EH_AlsoNotifyAddressInfo);
					awbActions.SendFWB = ZBool.True;
					AssertNoWarnings(awbActions.SendFWBInfo);
					AssertNoError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentForColoadConsols);
					AssertHasError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);
					AssertNoError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutBranchHomePort);
					AssertNoError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutBranchHomePortIATACode);
				}
			}
		}

		public void TestSendFWBThroughHUB()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			SendFWBBase();
		}

		public void TestSendFWBCreatesMessageSentEvent()
		{
			using (WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBActions.SendFWB = ZBool.True;
				Assert("Consol should not yet have Message Sent Event", !Consol.Logs.HasLogWith(c => c.SL_SE_NKEvent == "MSN"));

				AWBActions.PerformAllActions();

				AssertEquals("Consol should now have Message Sent Event with correct parameters",
					"|DEP=Airline|MST=FWB|RFN=08112345678",
					Consol.Logs.Find(c => c.SL_SE_NKEvent == "MSN").Single().SL_Reference);
			}
		}

		public void TestSendFHL()
		{
			SendFHLBase();

			AWBActions.SendFWB = ZBool.False;
			AWBActions.SendFHL = ZBool.True;
			AssertHasError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithoutFWB);
		}

		void SendFHLBase()
		{
			ExportAWBHeader awbHeader = Consol.Shipments[0].AWBHeader;
			Consol.Shipments[0].JS_OverrideWaybillDefaults = true;

			awbHeader.AWBRateLines[0].ER_GrossWeight = 32;
			awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			awbHeader.EH_AirportOfDestinationCode = "AUS";
			awbHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			awbHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;

			awbHeader.EH_ShipperCountryCode = Core.Constants.CountryCodes.Australia;
			awbHeader.EH_ShipperName = "Bob";
			awbHeader.EH_ShipperAddress = "44 Macho Macho Man Lane";
			awbHeader.EH_ShipperPlace = "Placeville";
			awbHeader.EH_ConsigneeCountryCode = Core.Constants.CountryCodes.Kazakhstan;
			awbHeader.EH_ConsigneeName = "Kazakhstan is best";
			awbHeader.EH_ConsigneeAddress = "sorry to stop the song";
			awbHeader.EH_ConsigneePlace = "by little girls";

			awbHeader.RunPreSaveValidation();
			AssertNoMessageErrors(awbHeader);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			Consol.JK_MasterBillNum = "08112345678";
			AWBActions.SendFWB = ZBool.True;
			AWBActions.SendFHL = ZBool.True;
			AssertHasError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentForDirectOrColoadConsols);
			AssertNoWarnings(AWBActions.SendFHLInfo);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			Consol.JK_MasterBillNum = "08112345678";
			AWBActions.SendFHL = ZBool.True;
			AssertHasError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentForDirectOrColoadConsols);
			AssertNoWarnings(AWBActions.SendFHLInfo);

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBActions.DateLastSent = "13 Jul 4003";
			AWBActions.SendFHL = ZBool.True;
			AssertNoWarnings(AWBActions.SendFHLInfo);
			AssertNoErrors(AWBActions.SendFHLInfo);

			Consol.Shipments[0].JS_RL_NKDestination = "USLAX";
			AWBActions.DateLastSent = "";
			AWBActions.SendFHL = ZBool.False;
			AssertHasWarning(AWBActions.SendFHLInfo, AWBActions.WarningFHLRegulationsMessage);
			AssertNoErrors(AWBActions.SendFHLInfo);
		}

		public void TestSendFHLWithNoCargoImLicenceThrowHUB()
		{
			Env.Licence.EzycargoInterface.AllowUsageForTest = false;
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			SendFHLBase();
			Env.Licence.EzycargoInterface.AllowUsageForTest = null;
		}

		public void TestSendFHLCreatesMessageSentEvent()
		{
			using (WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBActions.SendFWB = ZBool.True;
				AWBActions.SendFHL = ZBool.True;
				Assert("Shipment should not yet have Message Sent Event", !Consol.Shipments[0].Logs.HasLogWith(c => c.SL_SE_NKEvent == "MSN"));

				AWBActions.PerformAllActions();

				AssertEquals("Consol should now have Message Sent Event with correct parameters",
					"|DEP=Airline|MST=FHL|RFN=SHIPMENT",
					Consol.Shipments[0].Logs.Find(c => c.SL_SE_NKEvent == "MSN").Single().SL_Reference);
			}
		}

		public void TestSetAdditionalDefaults()
		{
			Env.Registry.Freight.AirWaybill.SelectFHLByDefault = true;
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);

			Assert(AWBActions.LaserAWB);
			Assert(AWBActions.PrintBarcodeLabel);
			AssertEquals(true, AWBActions.ParentPackagesLabel);
			AssertEquals(false, AWBActions.AWBPackagesLabel);
			Assert(AWBActions.PrintOptionalInformation);
			AssertEquals("", AWBActions.DatePrinted);
			Assert(AWBActions.PrintMasterAirWaybill);
			AssertEquals("", AWBActions.DateLastSent);
			Assert(AWBActions.SendFWB);
			Assert(AWBActions.SendFHL);
			Assert(AWBActions.FiveInchLabel);
			Assert(!AWBActions.SixInchLabel);
			AssertEquals(7, AWBActions.TotalPacks);
			AssertEquals(7, AWBActions.LabelRangeTo);
			AssertEquals(1, AWBActions.AWBLabelCopies);
			Assert(!AWBActions.PrintHAWBBarcodeLabels);
			AssertEquals(1, AWBActions.HAWBLabelCopies);

			AWBActions.LaserAWB = false;
			AWBActions.NeutralAWB = true;
			AWBActions.PrintBarcodeLabel = false;
			AWBActions.MAWBPrinter = ZGuid.NewZGuid();
			AWBActions.LabelPrinter = ZGuid.NewZGuid();
			AWBActions.SixInchLabel = true;
			AWBActions.FiveInchLabel = false;
			AWBActions.PrintOptionalInformation = false;
			AWBActions.AWBLabelCopies = 5;
			AWBActions.PrintHAWBBarcodeLabels = true;
			AWBActions.HAWBLabelPrinter = ZGuid.NewZGuid();
			AWBActions.HAWBLabelCopies = 6;

			AWBActions.SaveSettings();

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);

			Assert(AWBActions.NeutralAWB);
			Assert(!AWBActions.LaserAWB);
			Assert(!AWBActions.PrintBarcodeLabel);
			Assert(!AWBActions.MAWBPrinter.IsValid);
			Assert(!AWBActions.LabelPrinter.IsValid);
			Assert(!AWBActions.FiveInchLabel);
			Assert(AWBActions.SixInchLabel);
			Assert(!AWBActions.PrintOptionalInformation);
			AssertEquals(1, AWBActions.AWBLabelCopies);
			Assert(AWBActions.PrintHAWBBarcodeLabels);
			Assert(!AWBActions.HAWBLabelPrinter.IsValid);
			AssertEquals(1, AWBActions.HAWBLabelCopies);

			StmPrintQueue queue = Factory.New<StmPrintQueue>();
			AWBActions.MAWBPrinter = queue.PK;
			AWBActions.LabelPrinter = queue.PK;
			AWBActions.HAWBLabelPrinter = queue.PK;

			AWBActions.SaveSettings();

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			Assert(AWBActions.MAWBPrinter.IsValid);
			Assert(AWBActions.LabelPrinter.IsValid);
			Assert(AWBActions.HAWBLabelPrinter.IsValid);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
			Assert(AWBActions.PrintBarcodeLabel);
			Assert(!AWBActions.PrintMasterAirWaybill);
			Assert(!AWBActions.SendFWB);
			Assert(!AWBActions.SendFHL);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);
			Assert(!AWBActions.PrintHAWBBarcodeLabels);

			Consol.JK_IsNeutralMaster = false;
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);

			Assert(!AWBActions.LaserAWB);
			Assert(!AWBActions.NeutralAWB);
			Assert(AWBActions.CarrierAWB);

			AWBActions.SaveSettings();

			Consol.JK_IsNeutralMaster = true;
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);

			Assert(!AWBActions.LaserAWB);
			Assert(AWBActions.NeutralAWB);
			Assert(!AWBActions.CarrierAWB);

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.None);
			Assert(!AWBActions.PrintBarcodeLabel);
			Assert(!AWBActions.PrintMasterAirWaybill);
			Assert(!AWBActions.SendFWB);
			Assert(!AWBActions.SendFHL);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);
			Assert(AWBActions.PrintHAWBBarcodeLabels);
		}

		public void TestSendFHLDefaults()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals(false, AWBActions.SendFHL);

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(false, AWBActions.SendFHL);

			Consol.JK_RL_NKLoadPort = "USLAX";
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(true, AWBActions.SendFHL);

			Consol.JK_RL_NKLoadPort = "CAVAN";
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(true, AWBActions.SendFHL);

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
			AssertEquals(false, AWBActions.SendFHL);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(false, AWBActions.SendFHL);
		}

		public void TestSendFHLDefaults_Old()
		{
			Env.Registry.Freight.AirWaybill.SelectFHLByDefault = true;

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals(true, AWBActions.SendFHL);

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(true, AWBActions.SendFHL);

			Consol.JK_RL_NKLoadPort = "USLAX";
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(true, AWBActions.SendFHL);

			Consol.JK_RL_NKLoadPort = "CAVAN";
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(true, AWBActions.SendFHL);

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
			AssertEquals(false, AWBActions.SendFHL);
			AssertEquals(1, AWBActions.LabelRangeFrom);
			AssertEquals(7, AWBActions.LabelRangeTo);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(false, AWBActions.SendFHL);
		}

		public void TestValidateShipmentsToSendRepopulateAWB()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_GoodsDescription = "Old Desc";

			Factory.Save();

			var action = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);

			action.ValidateShipmentsToSend();
			AssertEquals("The goods description should be equal shipment goods description.", "Old Desc", shipment.AWBHeader.EH_ManifestDescriptionOfGoods);

			shipment.JS_GoodsDescription = "New Desc";
			Factory.Save();

			action.ValidateShipmentsToSend();
			AssertEquals("The goods description should be equal shipment goods description.", "New Desc", shipment.AWBHeader.EH_ManifestDescriptionOfGoods);
		}

		public void TestValidateShipmentsToSendExcludeHVLShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			Factory.Save();

			var action = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);

			using (HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				action.ValidateShipmentsToSend();
			}
			AssertEquals("Should exclude HVL shipments for validation", false, shipment.HasErrors);

			shipment.RunPreSaveValidation();
			AssertEquals("post condition: shipment does have errors if validated", true, shipment.HasErrors);
		}

		public void TestValidateNeutralAWB()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_IsNeutralMaster = true;
			AWBActions.NeutralAWB = false;
			Assert(!AWBActions.NeutralAWBInfo.HasErrors());

			Consol.JK_IsNeutralMaster = false;
			AWBActions.NeutralAWB = true;
			Assert(AWBActions.NeutralAWBInfo.HasErrors());

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBActions.ValidateNeutralAWB();
			Assert(AWBActions.NeutralAWBInfo.HasErrors());

			Consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			AWBActions.ValidateNeutralAWB();
			Assert(AWBActions.NeutralAWBInfo.HasErrors());

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBActions.ValidateNeutralAWB();
			Assert(!AWBActions.NeutralAWBInfo.HasErrors());
		}

		public void TestValidateCarrierAWB()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_IsNeutralMaster = true;
			AWBActions.CarrierAWB = false;
			Assertion.Assert(!AWBActions.CarrierAWBInfo.HasErrors());

			AWBActions.CarrierAWB = true;
			Assertion.Assert(AWBActions.CarrierAWBInfo.HasErrors());

			Consol.JK_IsNeutralMaster = false;
			AWBActions.CarrierAWB = true;
			Assertion.Assert(!AWBActions.CarrierAWBInfo.HasErrors());

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBActions.CarrierAWB = true;
			Assertion.Assert(AWBActions.CarrierAWBInfo.HasErrors());
		}

		public void TestValidateLaserAWB()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_IsNeutralMaster = true;
			AWBActions.LaserAWB = false;
			Assert(!AWBActions.LaserAWBInfo.HasErrors());

			Consol.JK_IsNeutralMaster = false;
			AWBActions.LaserAWB = true;
			Assert(AWBActions.LaserAWBInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBActions.ValidateLaserAWB();
			Assert(AWBActions.LaserAWBInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.AWBCoload;
			AWBActions.ValidateLaserAWB();
			Assert(AWBActions.LaserAWBInfo.HasErrors());

			Consol.JK_AgentType = Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AWBActions.ValidateLaserAWB();
			Assert(!AWBActions.LaserAWBInfo.HasErrors());
		}

		public void TestCantValidateSendFWBWithSuspendValidation()
		{
			GlbCompany.CurrentCompany.SetCountry("JM");

			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "008";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "12345678";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_RL_NKLoadPort = "JMKIN";
			consol.JK_MasterBillNum = "00812345678";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 7;
			shipment.JS_HouseBill = "shipment";
			shipment.JS_InspectionTypeCode = "APP";

			var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

			Factory.Save();

			ExportAWBHeader awbHeader = consol.AWBHeader;
			awbHeader.EH_ShipperName = "FRED";
			awbHeader.EH_ShipperAddress = "HERE";
			awbHeader.EH_ShipperPlace = "THERE";
			awbHeader.EH_ShipperCountryCode = "NZ";
			awbHeader.EH_ConsigneeName = "JOHN";
			awbHeader.EH_ConsigneeAddress = "THOMPSON";
			awbHeader.EH_ConsigneePlace = "CONTENDER";
			awbHeader.EH_ConsigneeCountryCode = "AU";
			awbHeader.EH_By1st = "H9";
			awbHeader.EH_AirportOfDestinationCode = "AUS";
			awbHeader.EH_ChargesCode = "PP";
			awbHeader.AWBRateLines[0].ER_GrossWeight = 32;
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "123456-7";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "NOT ALL HERE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "TIMBUKTU";
			consol.JK_PrepaidCollect = "PP";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";

			awbHeader.EH_AgentApprovalNumber = "1234";
			awbHeader.EH_AgentApprovalExpiryDate = ZDateTime.Today.AddDays(100);

			awbHeader.ExportAWBSecurityStatusLines.RemoveAndDeleteAll();

			var line = awbHeader.ExportAWBSecurityStatusLines.AddNew();
			line.EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;

			var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

			awbHeader.RunPreSaveValidation();
			AssertNoMessageErrors(awbHeader);

			using (awbHeader.GetValidationSuspender())
			{
				awbHeader.EH_ConsigneeCountryCode = "";
				awbActions.SendFWB = ZBool.True;

				AssertHasError(awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);
			}
		}

		public void TestCantValidateSendFHLWithSuspendValidation()
		{
			Consol.Shipments.SuspendValidation();

			AWBActions.SendFWB = true;
			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();

			AssertHasError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithMessageErrors);
		}

		public void TestCantSendFWBWithoutMasterBill()
		{
			Consol.JK_MasterBillNum = "";
			AWBActions.SendFWB = true;
			AssertHasError(AWBActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutMAWBNumber);

			Consol.JK_MasterBillNum = "081123";
			AWBActions.SendFWB = true;
			AssertHasError(AWBActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutMAWBNumber);

			Consol.JK_MasterBillNum = "08112345678";
			AWBActions.SendFWB = true;
			AssertNoError(AWBActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithoutMAWBNumber);
		}

		public void TestCantSendFHLWithoutMasterBill()
		{
			Consol.JK_MasterBillNum = "";
			AWBActions.SendFHL = true;
			AssertHasError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithoutMAWBNumber);

			Consol.JK_MasterBillNum = "081123";
			AWBActions.SendFHL = true;
			AssertHasError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithoutMAWBNumber);

			Consol.JK_MasterBillNum = "08112345678";
			AWBActions.SendFHL = true;
			AssertNoError(AWBActions.SendFHLInfo, ConsolAWBActions.ErrorFHLCannotBeSentWithoutMAWBNumber);
		}

		public void TestCantSendFHLWithoutHouseBill()
		{
			var shipment = Consol.Shipments[0];
			shipment.JS_UniqueConsignRef = "S11111111";
			shipment.JS_HouseBill = "";

			SetUpShipmentAWBHeader(shipment);
			Factory.Save();

			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();
			AssertHasError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S11111111");

			shipment.JS_HouseBill = "0123456789";
			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();
			AssertNoError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S11111111");
		}

		public void TestCantSendFHLWithoutHouseBillForMaster()
		{
			var shipment1 = Consol.Shipments[0];
			var shipment2 = Consol.Shipments.AddNew();
			var master = Consol.Shipments.AddNew();
			master.JS_ShipmentType = "ASM";

			shipment1.JS_UniqueConsignRef = "S11111111";
			shipment2.JS_UniqueConsignRef = "S22222222";
			master.JS_UniqueConsignRef = "S00000000";

			shipment1.JS_HouseBill = "";
			shipment2.JS_HouseBill = "shipment2";
			master.JS_HouseBill = "master";

			shipment1.JS_JS_ColoadMasterShipment = master.PK;
			shipment2.JS_JS_ColoadMasterShipment = master.PK;

			SetUpShipmentAWBHeader(master);
			SetUpShipmentAWBHeader(shipment1);
			SetUpShipmentAWBHeader(shipment2);

			Factory.Save();
			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();
			AssertNoError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S11111111");

			master.JS_HouseBill = "";
			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();
			AssertHasError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S00000000");
			AssertNoError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S11111111");

			master.JS_ShipmentType = "BCN";
			master.JS_HouseBill = "master";
			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();
			AssertHasError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S11111111");

			master.JS_HouseBill = "";
			AWBActions.SendFHL = true;

			AWBActions.RunPreSaveValidation();
			AssertHasError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S11111111");
			AssertHasError(AWBActions.SendFHLInfo, "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: S00000000");
		}

		public void TestIncludeSecurityDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var awbHeader = GetAWBHeaderWithoutSecurityDeclarationNotifications();
				awbHeader.EH_RN_NKAgentApprovalCountryCode = "JM";
				AWBActions.SendFWB = true;
				AWBActions.IncludeSecurityDeclaration = true;

				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);

				awbHeader.EH_AgentApprovalNumber = ZString.Empty;
				AssertHasMessageErrors(awbHeader.EH_AgentApprovalNumberInfo);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertHasError(AWBActions.IncludeSecurityDeclarationInfo, "eCSD cannot be included while there are Message Errors on the Security Declaration.");

				awbHeader.EH_AgentApprovalNumber = "BLAH123";
				AssertNoMessageErrors(awbHeader.EH_AgentApprovalNumberInfo);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);

				var line = awbHeader.CargoSecurityScreeningMethods[0];
				line.EAS_ScreeningMethod = "UNK";
				AssertHasMessageErrors(line.EAS_ScreeningMethodInfo);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertHasError(AWBActions.IncludeSecurityDeclarationInfo, "The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".");

				line.EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;
				AssertNoMessageErrors(line.EAS_ScreeningMethodInfo);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);
			}
		}

		public void TestIncludeSecurityDeclaration_EH_SecurityStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var awbHeader = GetAWBHeaderWithoutSecurityDeclarationNotifications();
				awbHeader.EH_RN_NKAgentApprovalCountryCode = "JM";
				AWBActions.SendFWB = true;
				AWBActions.IncludeSecurityDeclaration = true;

				awbHeader.AWBSpecialHandlingItems.RemoveAndDeleteAll();
				var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertHasError(AWBActions.IncludeSecurityDeclarationInfo, "The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".");

				AWBActions.IncludeSecurityDeclaration = false;
				AWBActions.ValidateIncludeSecurityDeclaration();
				AssertNoErrors(AWBActions.IncludeSecurityDeclarationInfo);
			}
		}

		public void TestPrintConsignmentSecurityDeclaration_EH_SecurityStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var awbHeader = GetAWBHeaderWithoutSecurityDeclarationNotifications();
				awbHeader.EH_RN_NKAgentApprovalCountryCode = "JM";
				AWBActions.SendFWB = true;
				AWBActions.PrintConsignmentSecurityDeclaration = true;

				awbHeader.AWBSpecialHandlingItems.RemoveAndDeleteAll();
				var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);

				securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
				AssertEquals("Precondition", securityStatus.EP_SpecialHandling, awbHeader.EH_SecurityStatus);
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertHasError(AWBActions.PrintConsignmentSecurityDeclarationInfo, "The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".");

				AWBActions.PrintConsignmentSecurityDeclaration = false;
				AWBActions.ValidatePrintConsignmentSecurityDeclaration();
				AssertNoErrors(AWBActions.PrintConsignmentSecurityDeclarationInfo);
			}
		}

		public void TestIncludeSecurityDeclaration_SG()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var awbHeader = GetAWBHeaderWithoutSecurityDeclarationNotifications();
				awbHeader.EH_RN_NKAgentApprovalCountryCode = "SG";

				AWBActions.SendFWB = true;
				AWBActions.IncludeSecurityDeclaration = true;

				AssertHasError(AWBActions.IncludeSecurityDeclarationInfo, "eCSD is not applicable for Singapore.");
			}
		}

		public void TestIncludeSecurityDeclaration_SendFWBIsTrue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);
			SupplyChainSecurityConfiguration supplyChainConfiguration;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.SouthAfrica))
			{
				supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is allowed to be included by this country", true, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				awbActions.SendFWB = true;
				AssertEquals("Security Declaration is included when eCSD is not to be sent by default", true, awbActions.IncludeSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is NOT allowed to be included by this country", false, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				awbActions.SendFWB = true;
				AssertEquals("Security Declaration is NOT included by default for US", false, awbActions.IncludeSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is NOT allowed to be included by this country", false, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				awbActions.SendFWB = true;
				AssertEquals("Security Declaration is NOT included when eCSD is not allowed", false, awbActions.IncludeSecurityDeclaration);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Japan))
			{
				supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Precondition: eCSD is allowed to be included by this country", true, supplyChainConfiguration.AllowIncludeECSD);
				AssertEquals("Precondition: SCS Module is enabled for this company", true, supplyChainConfiguration.IsEnabled);
				awbActions.SendFWB = true;
				AssertEquals("Security Declaration is included when eCSD is to be sent by default", true, awbActions.IncludeSecurityDeclaration);
			}
		}

		public void TestIncludeSecurityDeclaration_SendFWBIsFalse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.All);

				awbActions.SendFWB = true;
				awbActions.IncludeSecurityDeclaration = true;
				AssertEquals("Precodition: Security Declaration is currently included", true, awbActions.IncludeSecurityDeclaration);

				awbActions.SendFWB = false;
				AssertEquals("Security Declaration is NOT included (no matter what) when SendFWB is unticked", false, awbActions.IncludeSecurityDeclaration);
			}
		}

		ExportAWBHeader GetAWBHeaderWithoutSecurityDeclarationNotifications()
		{
			var awbHeader = Consol.AWBHeader;
			awbHeader.EH_AgentApprovalNumber = "BLAH123";
			awbHeader.CargoSecurityKnownShippers[0].EAS_ApprovalNumber = "456";

			var securityStatus = awbHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;

			awbHeader.CargoSecurityScreeningMethods.AddNew().EAS_ScreeningMethod = ScreeningMethods.Codes.VisualCheck;

			return awbHeader;
		}

		[TestDate(2010, 02, 05)]
		public void TestDatePrinted()
		{
			Consol.Logs.CreateRecreateOrUpdateEventLog(Events.DocumentSent, EstimateActual.Actual, ZDateTimeOffset.Now.AddDays(-1), "Some Ref");
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";

			Consol.JK_IsNeutralMaster = false;
			Consol.MasterBillMAWB = "";

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_MAWB = "00010006";

			Factory.Save();

			Consol.JK_AWBServiceLevel = "STD";
			Consol.JK_RL_NKLoadPort = mawb.JM_RL_NKPortOfLoading;
			Consol.Transports.MostInterestingTransport.JW_VoyageFlight = "QF030";
			Consol.MasterBillAirlinePrefix = "081";
			Consol.JK_IsNeutralMaster = ZBool.True;

			Factory.Save();

			AssertEquals(mawb.JM_MAWB, Consol.MasterBillMAWB);
			AssertEquals("", AWBActions.DatePrinted);

			AWBActions.PrintMasterAirWaybill = ZBool.True;
			AWBActions.MAWBPrinter = printer.PK;
			AWBActions.LaserAWB = ZBool.False;
			AWBActions.CarrierAWB = ZBool.False;
			AWBActions.NeutralAWB = ZBool.True;

			AWBActions.DoPrintMasterAirWaybill();

			Factory.Save();

			Consol.UpdateAWBPrinted();

			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);
			AssertEquals(ZDateTime.Now.ToLongTimeString(), AWBActions.DatePrinted);
		}

		public void TestValidatePrintHAWBBarcodeLabels()
		{
			AWBActions.PrintHAWBBarcodeLabels = true;

			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AWBActions.ValidatePrintHAWBBarcodeLabels();
			AssertNoErrors("No errors expected for non-direct consol", AWBActions.PrintHAWBBarcodeLabelsInfo);

			Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBActions.ValidatePrintHAWBBarcodeLabels();
			AssertHasError("Error for direct consol when print HAWB option selected", AWBActions.PrintHAWBBarcodeLabelsInfo, "HAWB Barcode Labels cannot be printed for Direct Consols.");

			AWBActions.PrintHAWBBarcodeLabels = false;
			AWBActions.ValidatePrintHAWBBarcodeLabels();
			AssertNoErrors("No error when print HAWB option not selected", AWBActions.PrintHAWBBarcodeLabelsInfo);
		}

		public void TestDoPrintHAWBBarcodeLabels()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.HAWBLabelPrinter = printer.PK;
			AWBActions.PrintHAWBBarcodeLabels = ZBool.True;

			AWBActions.PrintHAWBBarcodeLabel();
			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK);
			var printJobs = Factory.Load<StmPrintJob>(filter);
			AssertEquals(1, printJobs.Length);
			AssertEquals(nameof(PrintType.PRN), printJobs[0].SP_JobType);
			AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
			AssertEquals(printer.PK, printJobs[0].SP_SQ);
			AssertEquals(string.Empty, printJobs[0].SP_FaxDestination);
		}

		public void TestDoPrintHAWBBarcodeLabels_EPrint()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var ePrintEmailAddress = "email@domain.com";

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				AWBActions.PrintHAWBBarcodeLabels = true;
				AWBActions.HAWBLabelUseEPrint = true;

				AWBActions.PrintHAWBBarcodeLabel();
				var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
				AssertEquals(1, printJobs.Length);
				AssertEquals(nameof(PrintType.EML), printJobs[0].SP_JobType);
				AssertEquals(OrgConstants.AttachmentType.TIF, printJobs[0].SP_EmailAttachmentFormat);
				AssertEquals(ZGuid.Empty, printJobs[0].SP_SQ);
				AssertEquals(ePrintEmailAddress, printJobs[0].SP_Destination);
			}
		}

		public void TestPrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabelDoNotAffectConsol()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "123123123";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_HouseBill = "12345678";
			shipment.JS_OuterPacks = 5;

			FreightDataRegistry.Instance.PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ConsolAWBActions awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
			awbActions.LabelPrinter = printer.PK;
			awbActions.PrintBarcodeLabel = ZBool.True;
			awbActions.PrintAWBBarcodeLabel();

			AssertEquals(5, consol.AWBHeader.LabelTotalPacks);

			FreightDataRegistry.Instance.PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			awbActions = new ConsolAWBActions(consol, Business.AWB.AWBActions.ActionsModeType.LabelsOnly);
			awbActions.LabelPrinter = printer.PK;
			awbActions.PrintBarcodeLabel = ZBool.True;
			awbActions.PrintAWBBarcodeLabel();

			AssertEquals(5, consol.AWBHeader.LabelTotalPacks);
		}

		public override void TestDocumentSettings()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			AWBActions.LabelPrinter = printer.PK;

			AWBActions.FiveInchLabel = false;
			AWBActions.LabelRangeFrom = 3;
			AWBActions.LabelRangeTo = 7;
			AWBActions.TotalPacks = 11;
			AWBActions.PrintOptionalInformation = true;

			AWBActions.PrintAWBBarcodeLabel();

			AssertEquals("6 Inch", AWB.DocumentSize);
			AssertEquals(3, AWB.LabelStartRange);
			AssertEquals(7, AWB.LabelEndRange);
			AssertEquals(11, AWB.LabelTotalPacks);
			AssertEquals(3, AWB.MAWBLabelStartRange);
			AssertEquals(11, AWB.MAWBLabelTotalPacks);
			AssertEquals(true, AWB.PrintOptionalInformation);
			AssertEquals(true, AWB.DocumentSettingsPopulated);
		}

		public void TestValidateAWBtoFindErrorMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				JobMawb mawb = Factory.New<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "008";
				mawb.JM_GB = GlbBranch.CurrentBranch.PK;
				mawb.JM_MAWB = "12345678";
				mawb.JM_ServiceLevel = "STD";

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_IsNeutralMaster = true;
				consol.JK_RL_NKLoadPort = "JMKIN";

				consol.JK_MasterBillNum = "00812345678";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_OuterPacks = 7;
				shipment.JS_HouseBill = "shipment";
				shipment.JS_InspectionTypeCode = "APP";

				var awbHeader = consol.AWBHeader;
				awbHeader.EH_ShipperName = "FRED";
				awbHeader.EH_ShipperAddress = "HERE";
				awbHeader.EH_ShipperPlace = "THERE";
				awbHeader.EH_ShipperCountryCode = "NZ";
				awbHeader.EH_ConsigneeName = "JOHN";
				awbHeader.EH_ConsigneeAddress = "THOMPSON";
				awbHeader.EH_ConsigneePlace = "CONTENDER";
				awbHeader.EH_ConsigneeCountryCode = "AU";
				awbHeader.EH_By1st = "QF";
				awbHeader.EH_AirportOfDestinationCode = "AUS";
				awbHeader.EH_ChargesCode = "PP";
				awbHeader.AWBRateLines[0].ER_GrossWeight = 32;
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "123456-7";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "NOT ALL HERE";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "TIMBUKTU";

				var awbActions = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);

				consol.JK_AgentType = Core.Constants.AgentType.Direct;
				consol.JK_MasterBillNum = "00812345678";
				consol.IsAWBValuesOverriddenProperty = true;

				awbActions.SendFWB = ZBool.True;
				AssertNoError("The FWB message creator didn't show validation error because ConsigneeCountryCode is specified", awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);
				awbActions.SendFWB = ZBool.False;

				awbHeader.EH_ConsigneeCountryCode = "";
				awbActions.SendFWB = ZBool.True;
				AssertHasError("The FWB message creator show validation error because ConsigneeCountryCode is empty", awbActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);
				awbActions.SendFWB = ZBool.False;

				Factory.Save();

				var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				var loadedAWBActions = new AWBActionsTestClass(loadedConsol, Business.AWB.AWBActions.ActionsModeType.All);

				loadedAWBActions.SendFWB = ZBool.True;
				AssertHasError("The FWB message creator show validation error because ConsigneeCountryCode is empty", loadedAWBActions.SendFWBInfo, ConsolAWBActions.ErrorFWBCannotBeSentWithMessageErrors);

				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AYSYD";

				string portCode;
				RefCountry[] countries = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_EconomicGrouping, "EUN"));
				foreach (RefCountry item in countries)
				{
					portCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, item.Code)).Code;
					if (portCode != null)
					{
						consol.JK_RL_NKDischargePort = portCode;
						AWBActionsTestClass aWBActions2 = new AWBActionsTestClass(consol, Business.AWB.AWBActions.ActionsModeType.All);
						AssertEquals("Discharge port is in FHL country and Send FHL should select.", aWBActions2.SendFHL, ZBool.True);
					}
				}
			}
		}

		public void TestAWBHeader_NotValidForCourier()
		{
			Consol.JK_AgentType = Core.Constants.AgentType.Courier;
			Consol.JK_MasterBillNum = "01234567890";
			AssertNull("Precondition: AWBHeader should be null", Consol.AWBHeader);

			AWBActions.ValidateSendFWB();
			AssertHasError("There should be an error advising users that FWB message cannot be sent", AWBActions.SendFWBInfo, "FWB Message cannot be sent as current consol does not have a valid AWB.");
		}

		public void TestGetDocumentDataState_NonExistantMenuItem()
		{
			AWBActions.PrintMasterAirWaybill = ZBool.True;
			AWBActions.CarrierAWB = ZBool.True;

			string expectedWarning = "Expected document (Fake Menu/Carrier MAWB) does not exist. Default document (AWB/Carrier MAWB) will be used instead.";

			AWBActions.OverrideMenuPath = "Fake Menu";
			AWBActions.ValidatePrintMasterAirWaybill();
			AssertHasWarning(AWBActions.PrintMasterAirWaybillInfo, expectedWarning);

			AWBActions.OverrideMenuPath = ZString.Empty;
			AWBActions.ValidatePrintMasterAirWaybill();
			AssertNoWarning(AWBActions.PrintMasterAirWaybillInfo, expectedWarning);
		}

		public void TestCheckServiceProviderIsHUB()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB);
			Assert("Should return yes", AWBActions.GetServiceProviderIsHUB);
		}

		#region DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave

		public void TestDeferFiringTemplateApplicationDuringConsolAWBActionsSave()
		{
			AssertDeferTemplateApplicationDuringConsolAWBActionsSave(true);
			AssertDeferTemplateApplicationDuringConsolAWBActionsSave(false);

			void AssertDeferTemplateApplicationDuringConsolAWBActionsSave(bool deferred)
			{
				using (WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, deferred))
				{
					AWBActions.Consol.Logs.CancelAll();
					AssertEquals("No Template Applied Event", 0, AWBActions.Consol.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode && !l.SL_IsCancelled).Count());

					AWBActions.DoSendFWB();
					AssertEquals("Template should only be applied when !deferred", !deferred, AWBActions.Consol.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode && !l.SL_IsCancelled).Any());
				}
			}
		}

		public void TestFiringWorkflowDuringConsolAWBActionsSave()
		{
			using (WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBActions.Consol.Logs.CancelAll();
				AssertEquals("No current logs", 0, AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled).Count());

				AWBActions.DoSendFWB();

				Assert("Logs exist", AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled).Any());

				var deferredLogs = AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled && l.SL_FireWorkflow);
				AssertEquals("There should be no logs where SL_FireWorkflow is deferred", 0, deferredLogs.Count());

				var messageSentLog = AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled && !l.SL_FireWorkflow && l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertNotNull("MessageSent event should be found", messageSentLog);
			}
		}

		public void TestFiringWorkflowDuringConsolAWBActionsSave_Defer()
		{
			using (WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBActions.Consol.Logs.CancelAll();
				AssertEquals("No current logs", 0, AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled).Count());

				AWBActions.DoSendFWB();

				AssertEquals("Logs exist", true, AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled).Any());

				var notDeferredLogs = AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled && !l.SL_FireWorkflow);
				AssertEquals("All logs should be deferred", 0, notDeferredLogs.Count());

				var messageSentLog = AWBActions.Consol.Logs.Find(l => !l.SL_IsCancelled && l.SL_FireWorkflow && l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertNotNull("MessageSent event should be found", messageSentLog);
			}
		}

		#endregion

		#region Implementation

		void AWBActions_ShowMessageOnGUI(object sender, ShowMessageOnGUIEventArgs e)
		{
			lastGUIMessageTitle = e.Title;
			lastGUIMessageText = e.Message;
		}

		string lastGUIMessageTitle;
		string lastGUIMessageText;

		protected override void SetUp()
		{
			base.SetUp();

			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "12345678";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_RL_NKLoadPort = "AUSYD";

			Consol.JK_MasterBillNum = "08112345678";

			var shipment = Consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 7;
			shipment.JS_HouseBill = "shipment";
			shipment.JS_InspectionTypeCode = "APP";

			AWB = Consol.AWBHeader;
			AWBActions = new AWBActionsTestClass(Consol, Business.AWB.AWBActions.ActionsModeType.All);

			AWB.ResetSupplyChainSecurityConfigurationForTesting();
			Consol.ResetSupplyChainSecurityConfigurationForTesting();
		}

		void SetUpShipmentAWBHeader(ForwardingShipment shipment)
		{
			shipment.IsAWBValuesOverriddenProperty = true;

			ExportAWBHeader awbHeader = shipment.AWBHeader;
			awbHeader.EH_ShipperName = "FRED";
			awbHeader.EH_ShipperAddress = "HERE";
			awbHeader.EH_ShipperPlace = "THERE";
			awbHeader.EH_ShipperCountryCode = "NZ";
			awbHeader.EH_ConsigneeName = "JOHN";
			awbHeader.EH_ConsigneeAddress = "THOMPSON";
			awbHeader.EH_ConsigneePlace = "CONTENDER";
			awbHeader.EH_ConsigneeCountryCode = "AU";
			awbHeader.EH_By1st = "QF";
			awbHeader.EH_AirportOfDestinationCode = "AUS";
			awbHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			awbHeader.EH_OtherPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			awbHeader.EH_Currency = "AUD";
			awbHeader.EH_AWBOriginCode = "SYD";
			awbHeader.AWBRateLines[0].ER_GrossWeight = 32;
			awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = "K";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "123456-7";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "NOT ALL HERE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "TIMBUKTU";
		}

		ForwardingConsol Consol;
		new AWBActionsTestClass AWBActions
		{
			get { return (AWBActionsTestClass)fAWBActions; }
			set { fAWBActions = value; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return AWBActions;
		}

		StmMenuItem MenuItemLoader(string documentName, string menuPath)
		{
			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, documentName);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath);
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, Consol.DocumentSupporter.BusinessContext);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuType, Enterprise.Core.Constants.StmMenuItemTypes.Documents);

			return Factory.LoadTop1<StmMenuItem>(filter);
		}

		class AWBActionsTestClass : ConsolAWBActions
		{
			public AWBActionsTestClass(ForwardingConsol consol, ActionsModeType actionsMode)
				: base(consol, actionsMode)
			{
			}

			public new void DoSendFWB()
			{
				base.DoSendFWB();
			}

			public new void DoSendFHL()
			{
				base.DoSendFHL();
			}

			protected override bool CargoIMPMessageSenderServiceTaskIsInActiveOrTurnOff => false;

			public new void DoPrintMasterAirWaybill()
			{
				base.DoPrintMasterAirWaybill();
			}

			public new void DoPrintConsignmentSecurityDeclaration()
			{
				base.DoPrintConsignmentSecurityDeclaration();
			}

			public string[] DocumentNamesToPrint
			{
				get { return GetDocumentNamesToPrint(); }
			}

			public bool GetServiceProviderIsHUB
			{
				get { return base.ServiceProviderIsHUB; }
			}
		}

		class AWBActionsTestClassWithMenuPath : ConsolAWBActions
		{
			public AWBActionsTestClassWithMenuPath(ForwardingConsol consol, ActionsModeType actionsMode, string menuPath)
				: base(consol, actionsMode, menuPath)
			{
			}

			public new DocumentCommand CreateDocumentCommand(string documentName)
			{
				return base.CreateDocumentCommand(documentName);
			}
		}

		#endregion
	}
}
