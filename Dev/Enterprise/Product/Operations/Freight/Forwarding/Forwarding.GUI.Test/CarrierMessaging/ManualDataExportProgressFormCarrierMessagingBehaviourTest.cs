using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ManualDataExportProgressFormCarrierMessagingBehaviour))]
	class ManualDataExportProgressFormCarrierMessagingBehaviourTest : ManualDataExportProgressFormBehaviourTest<ManualDataExportProgressFormCarrierMessagingBehaviour>
	{
		public void TestTryUpdateEmptyMasterBillNumWithForwardAirReferenceNumberRange()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "TSTCAR";
				carrier.OH_IsShippingLine = true;
				carrier.CustomsCodes.AddNew("HID", "FWA", "US");

				var communicationsMode = carrier.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "PANDORA";

				var stmNums = carrier.OrgFountains.AddNew();
				stmNums.SN_Type = "FWA";
				stmNums.SN_Prefix = string.Empty;
				stmNums.SN_MinimumValue = 10;
				stmNums.SN_MaximumValue = 10;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_MasterBillNum = string.Empty;
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_AWBServiceLevel = "PUC";
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				Factory.Save();

				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						string.Empty,
						consol,
						new CarrierMessagingValidation(consol));

					behaviour.Apply(progressForm);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageSent.Code);
					var events = consol.Logs.Find(eventFilter);

					AssertEquals("should have added 1 Message Sent event", 1, events.Length);
					AssertEquals("Should update to 10 from the FWA number range", "00000010", consol.JK_MasterBillNum);
				}

				consol.JK_MasterBillNum = string.Empty;
				Factory.Save();

				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						string.Empty,
						consol,
						new CarrierMessagingValidation(consol));

					behaviour.Apply(progressForm);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageSent.Code);
					var events = consol.Logs.Find(eventFilter);

					var errorMessage = @"Forward Air bill number cannot be allocated as no numbers are left in range.
Please close this form and contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges";

					AssertEquals("No more events added", 1, events.Length);
					AssertEquals("Should keep empty as system can not get an available number from the FWA number range", ZString.Empty, consol.JK_MasterBillNum);
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public override void TestApply()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var haulageOrg = Factory.New<OrgHeader>();
				haulageOrg.OH_Code = "BUMPER";

				OrgCusCode carrierCode = haulageOrg.CustomsCodes.AddNew();
				carrierCode = haulageOrg.CustomsCodes.AddNew();
				carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
				carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

				var communicationsMode = haulageOrg.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "PANDORA";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_AWBServiceLevel = "PUC";
				consol.JK_OA_ShippingLineAddress = haulageOrg.MainAddress.PK;
				consol.JK_RL_NKLoadPort = "USCHI";

				Factory.Save();

				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						string.Empty,
						consol,
						new CarrierMessagingValidation(consol));

					behaviour.Apply(progressForm);

					AssertEquals("progressForm.TitleLabel.Text", "Send Forward Air Booking Request", progressForm.TitleLabel.Text);
					AssertEquals("progressForm.SendButton.Enabled", true, progressForm.SendButton.Enabled);
					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					AssertMultilineASCIIEquals("notifications",
						@"Processing Consol C00001000
Universal Shipment queued for sending to Organization [BUMPER].
Delivery succeeded.
", progressForm.NotificationsTextBox.Text);

					AssertEquals("progressForm.SendButton.Enabled", false, progressForm.SendButton.Enabled);
					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);

					var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);
					StmALog[] events = consol.Logs.Find(eventFilter);

					AssertEquals("should have added 1 Message Sent event", 1, events.Length);
					AssertEquals("added event should have a parameter that its name is MST and value is Carrier Booking Request",
						"Carrier Booking Request", events[0].Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType]);
					AssertEquals("added event should have a parameter that its name is DEP and value is Forward Air", "Forward Air",
						events[0].Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department]);

					var message = Factory.LoadTop1<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK));
					AssertNotNull("Linked EDI Message", message);
					AssertEquals("Should be version 1.1", "1.1", message.Content.LastAttribute.Value);
				}
			}
		}

		public void TestValidation_PreventSendingUnsavedConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;

			using (var progressForm = new DummyManualDataExportProgressForm())
			{
				var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
					"Send Booking Request to Carrier",
					consol,
					new CarrierMessagingValidation(consol));

				behaviour.Apply(progressForm);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(progressForm);

				AssertMultilineASCIIEquals("notifications",
@"Consol must be saved before sending message to a carrier.
", progressForm.NotificationsTextBox.Text);

				AssertEquals("progressForm.SendButton.Enabled", false, progressForm.SendButton.Enabled);
				AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);
			}
		}

		public void TestValidation_CarrierMustBeForwardAir()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "MAS0909090";

			var haulageOrg = Factory.New<OrgHeader>();
			haulageOrg.OH_Code = "BUMPER";

			var carrierCode = haulageOrg.CustomsCodes.AddNew();
			carrierCode = haulageOrg.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ERouter;

			consol.JK_OA_ShippingLineAddress = haulageOrg.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						"Send Forward Air Booking Request",
						consol,
						new ForwardAirCarrierMessagingValidation(consol));

					behaviour.Apply(progressForm);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					AssertMultilineASCIIEquals("notifications",
					@"The carrier is not registered as Forward Air.", progressForm.NotificationsTextBox.Text);

					AssertEquals("progressForm.SendButton.Enabled", false, progressForm.SendButton.Enabled);
					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);
				}
			}
		}

		public void TestServiceLevels_WarningSendingMessage()
		{
			var haulageOrg = Factory.New<OrgHeader>();
			haulageOrg.OH_Code = "BUMPER";

			var carrierCode = haulageOrg.CustomsCodes.AddNew();
			carrierCode = haulageOrg.CustomsCodes.AddNew();
			carrierCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			carrierCode.OK_CustomsRegNo = ApplicationCodeList.Codes.ForwardAir;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_MasterBillNum = "MAS099990";

			consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.JK_OA_ShippingLineAddress = haulageOrg.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					NotificationBufferTestClass notificationBuffer = new NotificationBufferTestClass();
					var messageForm = new ManualDataExportProgressFormCarrierMessagingBehaviour(
							"Send Forward Air Booking Request",
							consol,
							new ForwardAirCarrierMessagingValidation(consol));

					var validation = CarrierMessagingValidationFactory.GetValidation(consol);
					validation.Validate(notificationBuffer);

					messageForm.Apply(progressForm);
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);
					progressForm.SendButton.PerformClick();

					AssertEquals(@"The carrier service level does not correspond to Forward Air delivery methods and therefore no special door pickup or delivery instructions will be send to Forward Air. Do you want to proceed?

If you need to request pickup or delivery service, please configure your Forward Air Organization
(Organization -> Carrier -> Service Level) with the following service levels: PUC-Pickup, PUD-Delivery, PAD-Pickup and Delivery.", notificationBuffer.LastQueryUserMessage);
					notificationBuffer.LastQueryUserMessage = "";
					notificationBuffer.Clear();

					AssertEquals("progressForm.SendButton.Enabled", false, progressForm.SendButton.Enabled);
					AssertEquals("progressForm.CloseButton.Enabled", true, progressForm.CloseButton.Enabled);
				}
			}
		}

		public void TestSendForwardAirBookingRequestConcurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var haulageOrg = Factory.New<OrgHeader>();
				haulageOrg.OH_Code = "BUMPER";
				haulageOrg.CustomsCodes.AddNew("HID", "FWA", "US");

				var communicationsMode = haulageOrg.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "PANDORA";

				var fountain = haulageOrg.OrgFountains.AddNew();
				fountain.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
				fountain.SN_MinimumValue = 1111111;
				fountain.SN_MaximumValue = 2222222;
				fountain.SN_Value = 1111111;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_AWBServiceLevel = "PUC";
				consol.JK_OA_ShippingLineAddress = haulageOrg.MainAddress.PK;
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.Transports[0].JW_ETA = ZDateTime.Now;
				consol.JK_OA_SendingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				consol.JK_OA_ReceivingForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
				consol.Shipments.AddNew();

				Factory.Save();

				var factory1 = new BusinessObjectFactory();
				factory1.RefreshEnabled = false;

				var consolInFactory1 = factory1.Load<ForwardingConsol>(consol.PK);

				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;

				var consolInFactory2 = factory2.Load<ForwardingConsol>(consol.PK);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						string.Empty,
						consolInFactory1,
						new ForwardAirCarrierMessagingValidation(consolInFactory1));

					behaviour.Apply(progressForm);

					AssertEquals("Send Forward Air Booking Request", progressForm.TitleLabel.Text);
					AssertEquals(true, progressForm.SendButton.Enabled);
					AssertEquals(true, progressForm.CloseButton.Enabled);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					AssertMultilineASCIIEquals("Successful Notification",
						@"Processing Consol C00001000 (Master Bill='01111111')
Universal Shipment queued for sending to Organization [BUMPER].
Delivery succeeded.
", progressForm.NotificationsTextBox.Text);

					AssertEquals(false, progressForm.SendButton.Enabled);
					AssertEquals(true, progressForm.CloseButton.Enabled);

					var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);
					var events = consolInFactory1.Logs.Find(eventFilter);

					AssertEquals(1, events.Length);
					AssertEquals("Added an event with parameter name 'MST' and value 'Carrier Booking Request'",
						"Carrier Booking Request", events[0].Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType]);
					AssertEquals("Added an event with parameter name 'DEP' and value 'Forward Air'",
						"Forward Air", events[0].Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department]);

					AssertEquals("01111111", consolInFactory1.JK_MasterBillNum);

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (var progressForm = new DummyManualDataExportProgressForm())
				{
					var behaviour = new ManualDataExportProgressFormCarrierMessagingBehaviour(
						string.Empty,
						consolInFactory2,
						new ForwardAirCarrierMessagingValidation(consolInFactory2));

					behaviour.Apply(progressForm);

					AssertEquals("Send Forward Air Booking Request", progressForm.TitleLabel.Text);
					AssertEquals(true, progressForm.SendButton.Enabled);
					AssertEquals(true, progressForm.CloseButton.Enabled);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ShowDialogWithoutDispose(progressForm);

					progressForm.SendButton.PerformClick();

					AssertEquals(string.Empty, progressForm.NotificationsTextBox.Text);
					AssertEquals(false, progressForm.SendButton.Enabled);
					AssertEquals(true, progressForm.CloseButton.Enabled);

					var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);
					var events = consolInFactory2.Logs.Find(eventFilter);

					AssertEquals("No more events added", 1, events.Length);

					AssertEquals(@"While you have been working with this form attempting to send an electronic Booking Request message, another user has made changes which cannot be merged.

Your electronic message was not sent.

Please close and re-open the Consol form to continue.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#region Implementation

		protected override ManualDataExportProgressFormCarrierMessagingBehaviour GetNewBehaviour()
		{
			var consol = Factory.New<ForwardingConsol>();

			return new ManualDataExportProgressFormCarrierMessagingBehaviour(
				"Send Booking Request to Carrier",
				consol,
				new CarrierMessagingValidation(consol));
		}

		#endregion
	}
}
