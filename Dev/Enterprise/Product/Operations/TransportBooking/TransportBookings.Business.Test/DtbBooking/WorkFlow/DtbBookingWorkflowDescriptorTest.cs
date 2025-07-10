using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingWorkflowDescriptor))]
	public class DtbBookingWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbBookingWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.TransportCo |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.BookingParty |
					MessageRecipientPartyType.BillToParty |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.NotifyParty |
					MessageRecipientPartyType.CarrierBookingAgent;
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes => new CodeDescriptionPair[] { new CodeDescriptionPair("CTJ", WorkflowTriggerActionTypeConstants.Descriptions.CreateTransportJob)  };

		public void TestSupportedMessageRecipientPartiesForSpecificActionCreateTransportJob()
		{
			TestSupportedMessageRecipientPartiesForSpecificActionCore(WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, MessageRecipientPartyType.TransportJobRegistry);
		}

		void TestSupportedMessageRecipientPartiesForSpecificActionCore(string code, MessageRecipientPartyType expectedRecipients)
		{
			var result = WorkflowDescriptor.SupportedMessageRecipientPartiesForSpecificAction(code);
			AssertEquals($"Incorrect Recipients for {code}", result, expectedRecipients);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var transportCo = GetNewConfiguredOrgHeader();
			var consignee1 = GetNewConfiguredOrgHeader();
			var consignor1 = GetNewConfiguredOrgHeader();
			var bookingParty1 = GetNewConfiguredOrgHeader();
			var billingParty1 = GetNewConfiguredOrgHeader();
			var notifyParty1 = GetNewConfiguredOrgHeader();
			var carrierBookingAgent1 = GetNewConfiguredOrgHeader();

			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			var consolidateBooking = Helper.CreateConsolidation();
			var booking = consolidateBooking.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;
			booking.Address.E2_OA_Address = transportCo.MainAddress.PK;

			booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ClientRequestedBillingParty).E2_OA_Address = billingParty1.MainAddress.PK;
			booking.ConsolidationSingleJob.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).E2_OA_Address = bookingParty1.MainAddress.PK;
			booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty).E2_OA_Address = notifyParty1.MainAddress.PK;
			booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CarrierBookingAgent).E2_OA_Address = carrierBookingAgent1.MainAddress.PK;

			var instruction1 = booking.Instructions.AddNew();
			instruction1.OrganisationType = OrganisationTypesList.Codes.CNE;
			instruction1.Address.E2_OA_Address = consignee1.MainAddress.PK;

			var instruction2 = booking.Instructions.AddNew();
			instruction2.OrganisationType = OrganisationTypesList.Codes.CNR;
			instruction2.Address.E2_OA_Address = consignor1.MainAddress.PK;

			return new IWorkflowProvider[] { booking };
		}

		OrgHeader GetNewConfiguredOrgHeader()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;
			mode.EK_FileFormat = "NTF";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";

			var org = Factory.New<OrgHeader>();
			org.EDICommunicationsModes.Add(mode);

			return org;
		}

		public void TestAddToMessageRecipientPartyList()
		{
			var transportCo = Factory.New<OrgHeader>();
			var consignee1 = Factory.New<OrgHeader>();
			var consignee2 = Factory.New<OrgHeader>();
			var consignor1 = Factory.New<OrgHeader>();
			var consignor2 = Factory.New<OrgHeader>();
			var bookingParty = Factory.New<OrgHeader>();
			var sendingParty = Factory.New<OrgHeader>();
			var billingParty = Factory.New<OrgHeader>();
			var notifyParty = Factory.New<OrgHeader>();
			var carrierBookingAgent = Factory.New<OrgHeader>();

			var consolidateBooking = Helper.CreateConsolidation();
			var booking = consolidateBooking.Bookings.AddNew();

			var instruction1 = booking.Instructions.AddNew();
			instruction1.OrganisationType = OrganisationTypesList.Codes.CNE;
			instruction1.Address.E2_OA_Address = consignee1.MainAddress.PK;

			var instruction2 = booking.Instructions.AddNew();
			instruction2.OrganisationType = OrganisationTypesList.Codes.CNE;
			instruction2.Address.E2_OA_Address = consignee2.MainAddress.PK;

			var instruction3 = booking.Instructions.AddNew();
			instruction3.OrganisationType = OrganisationTypesList.Codes.CNR;
			instruction3.Address.E2_OA_Address = consignor1.MainAddress.PK;

			var instruction4 = booking.Instructions.AddNew();
			instruction4.OrganisationType = OrganisationTypesList.Codes.CNR;
			instruction4.Address.E2_OA_Address = consignor2.MainAddress.PK;

			booking.Address.E2_OA_Address = transportCo.MainAddress.PK;
			booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ClientRequestedBillingParty).E2_OA_Address = billingParty.MainAddress.PK;
			booking.ConsolidationSingleJob.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).E2_OA_Address = bookingParty.MainAddress.PK;
			booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty).E2_OA_Address = notifyParty.MainAddress.PK;
			booking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.CarrierBookingAgent).E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.Consignor).Select(recipient => recipient.Party);
			AssertEquals(2, orgs.Count());
			AssertCollectionContains(consignor1, orgs);
			AssertCollectionContains(consignor2, orgs);

			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.Consignee).Select(recipient => recipient.Party);
			AssertEquals(2, orgs.Count());
			AssertCollectionContains(consignee1, orgs);
			AssertCollectionContains(consignee2, orgs);

			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.TransportCo).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(transportCo, orgs);

			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.BillToParty).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(billingParty, orgs);

			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.BookingParty).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(bookingParty, orgs);

			consolidateBooking.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).E2_OA_Address = sendingParty.MainAddress.PK;
			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.BookingParty).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(sendingParty, orgs);

			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.NotifyParty).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(notifyParty, orgs);

			orgs = new DtbBookingWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(booking, MessageRecipientPartyTypeList.Codes.CarrierBookingAgent).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(carrierBookingAgent, orgs);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.DtbBookingWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Transport Booking", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(3, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertEquals("Job Direction", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);
			AssertEquals("Booking Template", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[2].List);

			var transportModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals(3, transportModeList.Count);
			AssertEquals(Constants.TransportModes.Road, transportModeList[0].Code);
			AssertEquals("Road", transportModeList[0].Description);
			AssertEquals(Constants.TransportModes.Rail, transportModeList[1].Code);
			AssertEquals("Rail", transportModeList[1].Description);
			AssertEquals(Constants.TransportModes.InlandWaterwayTransport, transportModeList[2].Code);
			AssertEquals("Inland Waterway", transportModeList[2].Description);

			var directionList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals(2, directionList.Count);
			AssertEquals(nameof(DtbBookingDirection.PIC), directionList[0].Code);
			AssertEquals("Pickup", directionList[0].Description);
			AssertEquals(nameof(DtbBookingDirection.DLV), directionList[1].Code);
			AssertEquals("Delivery", directionList[1].Description);

			var expectedTemplateList = new CodeDescriptionPairList();
			BindToLists.BookingTemplates.ForEach(x => expectedTemplateList.Add(x));
			var templateList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[2].List;
			AssertEquals(expectedTemplateList.Count, templateList.Count);
			AssertEquals(expectedTemplateList.CodesAsString, templateList.CodesAsString);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.DtbBooking, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestSupportedTriggerLineTypes()
		{
			AssertContainsExactElementsInAnyOrder("Types", new[] { TriggerLineTypes.Codes.DtbBookingConfirmation }, ((IWorkflowParentWithLines)WorkflowDescriptor).SupportedTriggerLineTypes);
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var processTask = parentBO.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			var workFlowDescriptor = new DtbBookingWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertType("Should return a DtbBookingWorkflowProcessor", typeof(DtbBookingWorkflowProcessor), result);
			var dtbBookingWorkflowProcessor = result as DtbBookingWorkflowProcessor;
			AssertEquals("Should have a factory", parentBO.Factory, dtbBookingWorkflowProcessor.Factory);
			AssertNotNull("Should have a wrapped processor", dtbBookingWorkflowProcessor.WrappedProcessor);
		}

		public void TestGetWorkflowTriggerActionTypesIncludeGHG_WhenEnableGreenhouseGasEmissionRegistryIsEnabled()
		{
			// Arrange
			var template = CreateTaskTemplateForCO2eTests();
			var trigger = template.WorkflowItems.Triggers.AddNew();

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				// Act
				var triggerActionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);

				// Assert
				Assert("Should contain GHG action when GHG registry is enabled", triggerActionTypes.ContainsCode("GHG"));
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				// Act
				var triggerActionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);

				// Assert
				Assert("Should not contain GHG action when GHG registry is disabled", !triggerActionTypes.ContainsCode("GHG"));
			}
		}

		protected override ProcessTaskTemplate CreateTaskTemplateForCO2eTests()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.TransportBooking.Code;

			return template;
		}

		protected override ICO2eCalculationSupporter CreateWithValidDataForCO2eTests()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 1, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var picAddress = Helper.CreateOrgAddress(Factory, Forwarder, "PIC");
			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, picAddress);
			Helper.CreatePackageDivot(pic, package, 1);

			var dlvAddress = Helper.CreateOrgAddress(Factory, OrgProxyOrg, "DLV");
			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, dlvAddress);
			Helper.CreatePackageDivot(dlv, package, 1);

			return booking;
		}

		protected override ICO2eCalculationSupporter CreateWithInvalidDataForCO2eTests()
		{
			return Helper.CreateBooking();
		}

		protected override void SetupAndRunCO2eTest(bool greenhouseGasEnableValue, Action action)
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(greenhouseGasEnableValue))
			{
				action();
			}
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		BindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new BindToLists(Factory)); }
		}

		BindToLists bindToLists;

		class DtbBookingWorkflowDescriptorExposed : DtbBookingWorkflowDescriptor
		{
			public MessageRecipientPartyCollection GetMessageRecipientPartyExposed(BusinessObject workflowProvider, ZString partyType)
			{
				return GetMessageRecipientParty(workflowProvider, partyType);
			}
		}
	}
}
