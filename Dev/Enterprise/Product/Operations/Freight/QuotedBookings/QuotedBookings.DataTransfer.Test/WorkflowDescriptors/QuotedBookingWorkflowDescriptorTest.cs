using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingWorkflowDescriptor))]
	public class QuotedBookingWorkflowDescriptorTest : WorkflowDescriptorTestCase<QuotedBookingWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Quoted Booking", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("QBK", WorkflowDescriptor.Code);
		}

		public override void TestWorkflowProviderType()
		{
			IWorkflowProvider provider = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);
			Assert(WorkflowDescriptor.WorkflowProviderType.IsInstanceOfType(provider));
			AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);
		}

		protected override Type WorkflowProviderTypeForNonPersistentBusinessObjects => typeof(ViewQuotedBooking);

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsWorkflowTriggerActionXML()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTriggerActionXML);
		}

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		public override void TestSubTypes()
		{
			ProcessTemplateSubType[] subTypeInformation = WorkflowDescriptor.SubTypeInformation;

			AssertEquals(3, subTypeInformation.Length);
			AssertEquals("Transport Mode", subTypeInformation[0].Description);
			AssertEquals(false, subTypeInformation[0].IsListRequired);
			AssertEquals("Type", subTypeInformation[1].Description);
			AssertEquals(true, subTypeInformation[1].IsListRequired);
			AssertEquals("Direction", subTypeInformation[2].Description);
			AssertEquals(false, subTypeInformation[2].IsListRequired);

			CodeDescriptionPairList expectedTransportTypes = new CodeDescriptionPairList();
			expectedTransportTypes.AddPair("", "All");
			expectedTransportTypes.AddPair(Core.Constants.TransportModes.Air, "Air Freight");
			expectedTransportTypes.AddPair(Core.Constants.TransportModes.Sea, "Sea Freight");
			expectedTransportTypes.AddPair(Core.Constants.TransportModes.Road, "Road Freight");
			expectedTransportTypes.AddPair(Core.Constants.TransportModes.Rail, "Rail Freight");
			expectedTransportTypes.AddPair(Core.Constants.TransportModes.Courier, "Courier");
			expectedTransportTypes.AddPair(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModeDescriptions.FCL);
			expectedTransportTypes.AddPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
			AssertContainsExactElementsInAnyOrder(expectedTransportTypes, subTypeInformation[0].List);

			CodeDescriptionPairList expectedBookingTypes = new CodeDescriptionPairList();
			expectedBookingTypes.AddPair(QuotedBooking.BookingWithQuoteCode, "Booking with Quote");
			expectedBookingTypes.AddPair(QuotedBooking.QuickBookingCode, "Quick Booking");
			expectedBookingTypes.AddPair(QuotedBooking.SpotQuoteCode, "One Off Quote");
			AssertContainsExactElementsInAnyOrder(expectedBookingTypes, subTypeInformation[1].List);
		}

		public void TestFormCustomisationSettingsProvider()
		{
			QuotedBookingFormCustomisationSettingsProvider formCustomisationSettingsProvider = WorkflowDescriptor.FormCustomisationSettings as QuotedBookingFormCustomisationSettingsProvider;

			AssertNotNull(formCustomisationSettingsProvider);
			AssertEquals(WorkflowDescriptor, formCustomisationSettingsProvider.ParentWorkflowDescriptor);
		}

		public void TestDocumentBusinessContext()
		{
			var descriptor = new QuotedBookingWorkflowDescriptor();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking,
				BusinessContext.Quotation
			},
			descriptor.DocumentBusinessContext);

			var provider = (ISpecificDocumentBusinessContextProvider)descriptor;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking,
				BusinessContext.Quotation
			},
			provider.GetDocumentBusinessContext(null));

			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking,
				BusinessContext.Quotation
			},
			provider.GetDocumentBusinessContext(processTaskTemplate));

			processTaskTemplate.P0_SubType2 = QuotedBooking.QuickBookingCode;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking
			},
			provider.GetDocumentBusinessContext(processTaskTemplate));

			processTaskTemplate.P0_SubType2 = QuotedBooking.BookingWithQuoteCode;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking
			},
			provider.GetDocumentBusinessContext(processTaskTemplate));

			processTaskTemplate.P0_SubType2 = QuotedBooking.SpotQuoteCode;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.Quotation
			},
			provider.GetDocumentBusinessContext(processTaskTemplate));

			var quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking
			},
			provider.GetDocumentBusinessContext(quotedBooking));

			quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.QuickBooking, Factory);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.QuotedBooking
			},
			provider.GetDocumentBusinessContext(quotedBooking));

			quotedBooking = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				BusinessContext.Quotation
			},
			provider.GetDocumentBusinessContext(quotedBooking));
		}

		public void TestGetWorkflowTriggerAction()
		{
			var quotedBooking = QuotedBookingWithConfiguredOrganisationParties;
			var processTask = quotedBooking.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendXML", processor is XmlMessageDeliver);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendXMLSimplified", processor is XmlMessageDeliver);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor (default) for NotificationEmail", processor is WorkflowTriggerNotification);
		}

		public void TestIsValidOneOffQuoteAction_ValidActions()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var processTask = quotedBooking.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			var errorMessage = "Not available for Spot Quotes.";
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals("SendUniversalShipmentXML", false, action.PQ_TriggerTypeInfo.HasError(errorMessage));

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			AssertEquals("SendUniversalEventXMLWithEDoc", false, action.PQ_TriggerTypeInfo.HasError(errorMessage));

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag; // other actions
			AssertEquals("Other actions", false, action.PQ_TriggerTypeInfo.HasError(errorMessage));
		}

		public void TestIsActionInvalidForOneOffQuote_InvalidActions()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			var processTask = quotedBooking.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			var errorMessage = "Not available for Spot Quotes.";

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML; // StandardXML
			Assert("Standard XML types", action.PQ_TriggerTypeInfo.HasError(errorMessage));

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified; // Simplified XML
			Assert("Simplified XML types", action.PQ_TriggerTypeInfo.HasError(errorMessage));

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance; // Debtor Balance
			Assert("Debtor Balance XML types", action.PQ_TriggerTypeInfo.HasError(errorMessage));
		}

		public void TestOneOffQuote_ClientNotificationEmailFallback_GetRecipients_ReturnsContactEmailAddress()
		{
			TestClientNotificationEmailFallback(true);
		}

		public void TestOneOffQuote_ClientNotificationEmailFallback_GetRecipients_ReturnsToOrgEmailAddress()
		{
			TestClientNotificationEmailFallback(false);
		}

		void TestClientNotificationEmailFallback(bool shouldJobHaveClientContact)
		{
			var partyType = MessageRecipientPartyTypeList.Codes.Client;
			var contactEmail = "ConTestOneOffQuote_ClientNotificationEmailFallback_TriggerTask_FallbackToOrgEmailAddress()tactEmail@wisetechglobal.com";
			var orgEmail = "OrgEmail@wisetechglobal.com";

			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			ratingHeader.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;
			ratingHeader.TH_QuoteDate = ZDate.Today;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "ABC";
			client.OH_FullName = "ABC Enterprises";
			client.OH_RL_NKClosestPort = "AUBNE";

			var orgAddress = client.Addresses.AddNew();
			orgAddress.OA_Email = orgEmail;
			orgAddress.OA_Address1 = "100 adelaide st";

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressType = AutoDocAddressTypes.Codes.QuotationClientAddress;
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			jobDocAddress.E2_Contact = "WiseTech";
			jobDocAddress.E2_ParentID = ratingHeader.PK;
			jobDocAddress.E2_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var quotedBooking = QuotedBookingWithConfiguredOrganisationParties;
			quotedBooking.Job.JH_OA_LocalChargesAddr = orgAddress.PK;
			quotedBooking.Job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			quotedBooking.ClientDocAddress.E2_OA_Address = orgAddress.PK;
			if (shouldJobHaveClientContact)
			{
				var clientContact = client.Contacts.AddNew();
				clientContact.OC_Email = contactEmail;
				quotedBooking.ClientDocAddress.ContactPK = clientContact.PK;
			}

			Factory.Save();
			var quotedBookingWorkflowDescriptor = new QuotedBookingWorkflowDescriptor();
			var recipients = quotedBookingWorkflowDescriptor.GetMessageRecipientParty(quotedBooking, partyType);
			if (shouldJobHaveClientContact)
			{
				AssertNotNull("Fallback contact email", recipients.SingleOrDefault(r => r.FallbackEmail == contactEmail));
			}
			else
			{
				AssertNotNull("Fallback orgnisation email", recipients.SingleOrDefault(r => r.FallbackEmail == orgEmail));
			}
		}

		public void TestOneOffQuote_ClientNotificationEmailFallback_TriggerTask_FallbackToOrgMainEmailAddress()
		{
			TestRegistryCommunicationFallback(true);
		}

		public void TestOneOffQuote_ClientNotificationEmailFallback_TriggerTask_FallbackToNoResult()
		{
			TestRegistryCommunicationFallback(false);
		}

		void TestRegistryCommunicationFallback(bool registrySetting)
		{
			if (registrySetting)
			{
				WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
			else
			{
				WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
			var orgMainEmail = "OrgMainEmail@wisetechglobal.com";
			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			ratingHeader.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;
			ratingHeader.TH_QuoteDate = (ZDate)System.DateTime.Now;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "ABC";
			client.OH_FullName = "ABC Enterprises";
			client.OH_RL_NKClosestPort = "AUBNE";
			var orgAddress = client.MainAddress;
			orgAddress.OA_Email = orgMainEmail;
			orgAddress.OA_Address1 = "100 adelaide st";

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressType = AutoDocAddressTypes.Codes.QuotationClientAddress;
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			jobDocAddress.E2_Contact = "WiseTech";
			jobDocAddress.E2_ParentID = ratingHeader.PK;
			jobDocAddress.E2_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var quotedBooking = QuotedBookingWithConfiguredOrganisationParties;
			quotedBooking.Job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			quotedBooking.ClientDocAddress.E2_OA_Address = orgAddress.PK;
			quotedBooking.Job.JH_OA_LocalChargesAddr = orgAddress.PK;

			var communicationsMode = client.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = quotedBooking.Job.JH_ParentTableCode;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode.EK_Destination = orgMainEmail;

			var processTriggerTask = quotedBooking.WorkflowItems.Triggers.AddNew();
			processTriggerTask.P9_TaskID = "T0000100";
			processTriggerTask.P9_Description = "triggerTest";
			processTriggerTask.ReferenceCode = "REF";
			processTriggerTask.P9_ParentID = quotedBooking.PK;
			processTriggerTask.P9_Type = "TRG";
			processTriggerTask.P9_Status = "OPN";
			processTriggerTask.TriggerConditions.TriggerEventCode = Events.Authorised.Code;

			var action = processTriggerTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_TriggerParty = "CLI";
			action.PQ_P9 = processTriggerTask.PK;

			var log = new QueuedLogForTesting(Factory);
			Factory.Save();

			var workFlowDescriptor = new QuotedBookingWorkflowDescriptor();
			var notificationAction = (WorkflowTriggerNotification)workFlowDescriptor.GetWorkflowTriggerAction(action, log);

			if (registrySetting)
			{
				AssertNotNull("Fallback orgnisation main email", notificationAction.Modes.CommunicationModes.SingleOrDefault(c => c.EK_Destination == orgMainEmail));
			}
			else
			{
				AssertEquals("Fallback orgnisation main email failed", 0, notificationAction.Modes.CommunicationModes.Count);
			}
		}

		public void TestGetWorkflowTriggerAction_ConvertToShipment()
		{
			var quotedBooking = QuotedBookingWithConfiguredOrganisationParties;
			var processTask = quotedBooking.WorkflowItems.Triggers.AddNew();

			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ConvertToShipment;

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for ConvertToShipment", processor is ConvertToShipmentProcessor);
		}

		public void TestSupportsConvertToShipment()
		{
			Assert("Should support convert to shipment", WorkflowDescriptor.SupportsConvertToShipment);
		}

		public void TestSupportsBufferManagement()
		{
			Assert(WorkflowDescriptor.SupportsBufferManagement);
		}

		public void TestControllerID_ShouldReturnQuotedBookingController()
		{
			AssertEquals(ControllerIDs.QuotedBookings, WorkflowDescriptor.ControllerID);
		}

		public void TestAddToMessageRecipientPartyList_JobNotRegisterEditableChildObject()
		{
			var quotedBooking = QuotedBookingWithConfiguredOrganisationParties;
			var job = quotedBooking.Job;
			var processTask = quotedBooking.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			quotedBooking.UnRegisterEditableChildObject(job);
			AssertEquals("Job is not registered editable child object.", false, quotedBooking.IsRegisteredEditableChildObject(job));

			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Client;
			AssertEquals("Job is not registered editable child object.", false, quotedBooking.IsRegisteredEditableChildObject(job));
		}

		#region CO2e

		protected override ProcessTaskTemplate CreateTaskTemplateForCO2eTests()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.QuotedBooking.Code;

			return template;
		}

		protected override ICO2eCalculationSupporter CreateWithValidDataForCO2eTests()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = DataTransfer.Universal.Test.CO2eTestHelper.CreateQuotedBooking(Factory, Core.Constants.RateMode.LSE, ZString.Empty, consignee, consignee, consignor, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.LoadPort = "AUSYD";
			quotedBooking.DischargePort = "USLAX";
			quotedBooking.Weight = 1000m;

			return quotedBooking;
		}

		protected override ICO2eCalculationSupporter CreateWithInvalidDataForCO2eTests()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var quotedBooking = DataTransfer.Universal.Test.CO2eTestHelper.CreateQuotedBooking(Factory, Core.Constants.RateMode.LSE, ZString.Empty, consignee, consignee, consignor, null, "AUSYD", "USLAX", 10m, 1m, QuotedBookingState.QuoteOnly);
			quotedBooking.Weight = 0;

			return quotedBooking;
		}

		#endregion

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			QuotedBooking provider = QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, Factory);

			provider.ControllingCustomerDocumentaryAddress.OrganisationPK = ControllingCustomerOrg.PK;
			provider.ControllingAgentDocumentaryAddress.OrganisationPK = ControllingAgentOrg.PK;

			JobHeader.Loader jobLoader = new JobHeader.Loader(provider);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;

			return new[] { provider };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.Client |
					MessageRecipientPartyType.ControllingAgent |
					MessageRecipientPartyType.ControllingCustomer;
			}
		}

		QuotedBooking QuotedBookingWithConfiguredOrganisationParties
		{
			get { return quotedBooking ?? (quotedBooking = (QuotedBooking)GetParentsWithConfiguredOrganisationPartiesForTest()[0]); }
		}
		QuotedBooking quotedBooking;
	}
}
