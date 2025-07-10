using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidationWorkflowDescriptor))]
	public class DtbBookingConsolidationWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbBookingConsolidationWorkflowDescriptor>
	{
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.TransportCo;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var transportCo = Factory.New<OrgHeader>();
			var transportCoMode = transportCo.EDICommunicationsModes.AddNew();
			transportCoMode.EK_Module = WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode;
			transportCoMode.EK_FileFormat = "NTF";
			transportCoMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			transportCoMode.EK_Destination = "@notificationemail.cargowise.com";

			var consolidation = Helper.CreateConsolidation();
			consolidation.Address.E2_OA_Address = transportCo.MainAddress.PK;

			return new IWorkflowProvider[] { consolidation };
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Transport Booking Consolidation", WorkflowDescriptor.Description);
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
