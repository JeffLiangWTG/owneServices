using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingInstructionWorkflowDescriptor))]
	public class DtbBookingInstructionWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbBookingInstructionWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.TransportCo |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.Consignee;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();

			var transportCo = CreateOrgHeaderAndSetupEDICommunications();
			booking.Address.E2_OA_Address = transportCo.MainAddress.PK;

			// Setup Consignor and Consignee in SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery
			return new IWorkflowProvider[] { instruction };
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);

			var instruction = (DtbBookingInstruction)workflowProvider;

			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				instruction.OrganisationType = OrganisationTypesList.Codes.CNR;
				instruction.Address.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
			else if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				instruction.OrganisationType = OrganisationTypesList.Codes.CNE;
				instruction.Address.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
		}

		OrgHeader CreateOrgHeaderAndSetupEDICommunications()
		{
			var org = Factory.New<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Transport Booking Instruction", WorkflowDescriptor.Description);
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

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
