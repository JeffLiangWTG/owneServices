using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class OutwardReportPlugInToConsolTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestMenuLabels()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (OutwardReportPlugInToConsol plugin = new OutwardReportPlugInToConsol(consol))
			{
				Menu.MenuItemCollection menuItems = plugin.TopLevelMenu.MenuItems;
				var item1 = menuItems[0];
				AssertEquals("Submit &Outward Report", item1.Text);
				var item2 = menuItems[1];
				AssertEquals("&Cancel Outward Report", item2.Text);
				var item3 = menuItems[3];
				AssertEquals("Submit &Outward Report with comment", item3.Text);
				var item4 = menuItems[4];
				AssertEquals("Submit &Outward Report with attachments", item4.Text);
			}
		}

		public void TestMainMenuText()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (OutwardReportPlugInToConsol plugin = new OutwardReportPlugInToConsol(consol))
			{
				AssertEquals("&Outward Report", plugin.TopLevelMenu.Text);
			}
		}

		public void TestSendOCRWithAttachments()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var consol = Factory.New<ForwardingConsol>();
			string expectedErrors = @"Unable to send Original due to the following errors:

Carrier is not valid. Please enter a valid Carrier for this consol.
ORN can be sent only for Air Or Sea.
Voyage or flight no. is empty for this consol.
Master Bill Number (BOL) is empty for this consol.
Port of loading is not valid.
Port of discharge is not valid.
You have not attached any shipments yet.
Estimated departure date is not valid.
Either Delivery Notification Organization, Delivery Notification Port or both Delivery Notification Party and email should be entered to ensure that the CCA receives delivery advices.
";
			using (OutwardReportPlugInToConsol plugin = new OutwardReportPlugInToConsol(consol))
			{
				Menu.MenuItemCollection menuItems = plugin.TopLevelMenu.MenuItems;
				var sendOCRWithAttachmentsOption = menuItems[4];
				AssertEquals("Submit &Outward Report with attachments", sendOCRWithAttachmentsOption.Text);
				sendOCRWithAttachmentsOption.PerformClick();
				AssertEquals("Unpopulated consol will error", expectedErrors, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[NUnit.Framework.TestDate(2015, 11, 04)]
		public void TestSendOutwardReportForCLDConsol()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "AAL NEWCASTLE", "AAL NEWCASTLE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "APLU13102015";
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "TESTCARRIER";
			consol.Transports[0].JW_VoyageFlight = "131015";
			consol.Transports[0].JW_Vessel = "AAL NEWCASTLE";
			consol.Transports[0].JW_ATD = CargoWise.Types.ZDateTime.Today;
			consol.Transports[0].CarrierPK = carrier.PK;
			consol.SetSystemDefinedValue(OutwardReportManifestStatus.Schema.DeliveryNotificationPartyName, (ZString)"TEST");
			consol.SetSystemDefinedValue(OutwardReportManifestStatus.Schema.DeliveryNotificationPartyEmail, (ZString)"TEST@MAIL.COM");
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "1131015";
			var childShipmentOne = masterShipment.CoLoadShipments.AddNew();
			childShipmentOne.JS_HouseBill = "2131015";
			var childShipmentTwo = masterShipment.CoLoadShipments.AddNew();
			childShipmentTwo.JS_HouseBill = "3131015";
			Factory.Save();
			using (var plugIn = new OutwardReportPlugInToConsol(consol))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Menu.MenuItemCollection menuItems = plugIn.TopLevelMenu.MenuItems;
				var sendOutwardReportOption = menuItems[0];
				sendOutwardReportOption.PerformClick();
				AssertEquals($@"Unable to send Original due to the following errors:

You have not entered a Customs Entry Number for shipment, {childShipmentOne.JS_UniqueConsignRef}
You have not entered a Customs Entry Number for shipment, {childShipmentTwo.JS_UniqueConsignRef}
", UnitTestUserNotification.Instance.LastMessage.Text);
				childShipmentOne.CustomsEntryNumberType = "CUS";
				childShipmentOne.CustomsEntryNumber = "12345";
				childShipmentTwo.CustomsEntryNumberType = "CUS";
				childShipmentTwo.CustomsEntryNumber = "23456";
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				sendOutwardReportOption.PerformClick();
				AssertEquals("Original message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolValidationMessageErrors()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "AA220", "AA220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			consol.SetSystemDefinedValue(OutwardReportManifestStatus.Schema.DeliveryNotificationPartyName, (ZString)"TEST");
			consol.SetSystemDefinedValue(OutwardReportManifestStatus.Schema.DeliveryNotificationPartyEmail, (ZString)"TEST@MAIL.COM");
			var transport = consol.Transports[0];
			transport.JW_Vessel = "AA220";
			transport.JW_VoyageFlight = "AA100";
			transport.CarrierPK = orgHeader.PK;
			transport.JW_ATD = ZDate.Today;
			var shipment = consol.Shipments.AddNew();
			shipment.CustomsEntryNumberType = "CUS";
			shipment.CustomsEntryNumber = "12345";
			shipment.JS_HouseBill = "S123456";
			Factory.Save();
			using (var plugIn = new OutwardReportPlugInToConsol(consol))
			{
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems[0];
				sendMenuItem.PerformClick();
				AssertEquals(@"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Voyage / Flight: Flight number is not in the list of valid Flights supported by NZ Customs.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}

			transport.JW_Vessel = "AA220";
			transport.JW_VoyageFlight = "AA220";
			Factory.Save();
			using (var plugIn = new OutwardReportPlugInToConsol(consol))
			{
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems[0];
				sendMenuItem.PerformClick();
				AssertEquals(@"Original message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestCannotSendLegacyOCR()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var consol = Factory.New<ForwardingConsol>();
			var legacyMessage = Factory.New<EDIMessage>();
			legacyMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			legacyMessage.EM_MessageType = Business.Declaration.NZCMessage.MessageTypes.OutwardReport.MessageType;
			legacyMessage.EM_MessageSubType = "ORG";
			legacyMessage.EM_MessageText = @"UNH+18+CUSCAR:D:03A:UN'BGM+833:::DEPART+C00001027+9'NAD+CS+00009908C:ZZZ:143'NAD+CH++A&S FURNISHING CO LTD'TDT+20++4+++++:::QF108'LOC+5+NZAKL'LOC+8+AU'DTM+136:20130221:102'CNT+2:1'CNI+1+14551950'RFF+HWB:TESTHAWB007'UNT+12+18'";
			consol.Messages.Add(legacyMessage);
			var orn = Factory.New<CusEntryNumber>();
			orn.CE_ParentID = consol.PK;
			orn.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			orn.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			orn.CE_EntryNum = "682732Y";
			string expectedError = @"This OCR was originally sent as a Legacy message, subsequent changes are no longer possible in TSW.";
			using (OutwardReportPlugInToConsol plugin = new OutwardReportPlugInToConsol(consol))
			{
				Menu.MenuItemCollection menuItems = plugin.TopLevelMenu.MenuItems;
				var cancelOCR = menuItems[1];
				cancelOCR.PerformClick();
				AssertEquals("NZ Customs no longer accepts legacy messages.", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Trying to re-send a legacy entry will error.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
		{
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "SEA";
			return new OutwardReportPlugInToConsol(consol);
		}

		public void TestEnabledForExportVessel()
		{
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "SEA";
			using (OutwardReportPlugInToConsol plugIn = new OutwardReportPlugInToConsol(consol))
			{
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
			}
		}

		public void TestDisabledForImportVessel()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";
			using (OutwardReportPlugInToConsol plugIn = new OutwardReportPlugInToConsol(consol))
			{
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
			}
		}

		public void TestManifestStatus()
		{
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "SEA";
			using (OutwardReportPlugInToConsol plugIn = new OutwardReportPlugInToConsol(consol))
			{
				plugIn.OnGUIShown();
				AssertNotNull("BusinessObject is referenced", plugIn.BusinessEntity);
			}
		}

		public void TestEnabledForDomesticPrecarriage()
		{
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "NZWLG";
			consol.JK_RL_NKDischargePort = "FJSUV";
			Transport domesticPreCarriageLeg = consol.Transports[0];
			domesticPreCarriageLeg.JW_RL_NKLoadPort = "NZWLG";
			domesticPreCarriageLeg.JW_RL_NKDiscPort = "NZAKL";
			domesticPreCarriageLeg.IsDomestic = true;
			Transport internationalMainLeg = consol.Transports.AddNew();
			internationalMainLeg.JW_RL_NKLoadPort = "NZAKL";
			internationalMainLeg.JW_RL_NKDiscPort = "FJSUV";
			using (OutwardReportPlugInToConsol plugIn = new OutwardReportPlugInToConsol(consol))
			{
				AssertEquals("PlugIn.Enabled", true, plugIn.Enabled);
			}
		}

		#region Implementation
		ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			TestHelper.SetupMessagingEnvironment();
		}
		#endregion
	}
}
