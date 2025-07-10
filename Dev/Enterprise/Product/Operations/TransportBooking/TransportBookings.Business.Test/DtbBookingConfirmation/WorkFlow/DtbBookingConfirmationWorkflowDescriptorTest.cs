using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingConfirmationWorkflowDescriptor))]
	public class DtbBookingConfirmationWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbBookingConfirmationWorkflowDescriptor>
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
					MessageRecipientPartyType.NotifyParty;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var transportCo = CreateOrgHeaderAndSetupEDICommunications();
			var consignee1 = CreateOrgHeaderAndSetupEDICommunications();
			var consignor1 = CreateOrgHeaderAndSetupEDICommunications();
			var bookingParty1 = CreateOrgHeaderAndSetupEDICommunications();
			var billingParty1 = CreateOrgHeaderAndSetupEDICommunications();
			var notifyParty1 = CreateOrgHeaderAndSetupEDICommunications();

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

			var instruction1 = booking.Instructions.AddNew();
			instruction1.OrganisationType = OrganisationTypesList.Codes.CNE;
			instruction1.Address.E2_OA_Address = consignee1.MainAddress.PK;

			var instruction2 = booking.Instructions.AddNew();
			instruction2.OrganisationType = OrganisationTypesList.Codes.CNR;
			instruction2.Address.E2_OA_Address = consignor1.MainAddress.PK;

			return new[] { instruction1.Confirmations.AddNew() };
		}

		OrgHeader CreateOrgHeaderAndSetupEDICommunications()
		{
			var org = Factory.New<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Transport Booking Confirmation", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
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

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public void TestParentSupportsWorkflowTriggerActionUniversalShipmentXML()
		{
			var confirmationParent = GetParentsWithConfiguredOrganisationPartiesForTest().Single() as DtbBookingConfirmation;
			var actionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes(null, confirmationParent);

			var result = WorkflowDescriptor.ParentSupportsWorkflowTriggerActionUniversalShipmentXML(confirmationParent);

			AssertEquals("Should support creating XUS for parent", true, result);
			AssertEquals("The action types should contain XUS", true, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));

			var nonWorkflowProviderParent = Factory.New<DtbBookingInstructionPkgDivot>() as IBusiness;
			AssertEquals("Preconditon: Should not be a workflow provider", false, nonWorkflowProviderParent is IWorkflowProvider);
			actionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes(null, nonWorkflowProviderParent);

			result = WorkflowDescriptor.ParentSupportsWorkflowTriggerActionUniversalShipmentXML(nonWorkflowProviderParent);

			AssertEquals("Should support not creating XUS for parent that is not a workflow provider", false, result);
			AssertEquals("The action types should not contain XUS", false, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
		}

		public void TestGetWorkflowTriggerActionCoreForXUS()
		{
			var confirmation = GetParentsWithConfiguredOrganisationPartiesForTest().Single();
			var processTask = confirmation.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "Start Work";
			processTask.ReferenceCode = "REF";
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			var triggerLogBO = processTask.Logs.AddNew();
			using (triggerLogBO.LockForUpdatingKeyFieldsForTesting())
			{
				triggerLogBO.SL_SE_NKEvent = Events.WorkflowTriggerEventCode;
			}

			var resultType = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, processTask)).GetType();

			AssertEquals("GetWorkflowTriggerAction() should return processor of type DtbBookingWorkflowProcessor when the trigger type is XUS", typeof(DtbBookingWorkflowProcessor), resultType);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

			triggerLogBO = processTask.Logs.AddNew();
			using (triggerLogBO.LockForUpdatingKeyFieldsForTesting())
			{
				triggerLogBO.SL_SE_NKEvent = Events.WorkflowTriggerEventCode;
			}

			resultType = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, processTask)).GetType();

			AssertNotEquals("GetWorkflowTriggerAction() should not return processor of type DtbBookingWorkflowProcessor when the trigger type is not XUS", typeof(DtbBookingWorkflowProcessor), resultType);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
