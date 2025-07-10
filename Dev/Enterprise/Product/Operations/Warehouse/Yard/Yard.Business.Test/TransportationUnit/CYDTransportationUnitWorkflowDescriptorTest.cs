using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitWorkflowDescriptor))]
	public class CYDTransportationUnitWorkflowDescriptorTest : WorkflowDescriptorTestCase<CYDTransportationUnitWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Container Yard Transportation Unit", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CYDTransportationUnitWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			Assert(!WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert(WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert(!WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestClientLabelName()
		{
			AssertEquals("Transport Provider", WorkflowDescriptor.ClientName);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transportationUnit.Yard.WW_OA_WarehouseAddress = orgAddress.PK;
			var bookingPartyMode = orgAddress.Header.EDICommunicationsModes.AddNew();
			bookingPartyMode.EK_Module = WorkflowDescriptors.CYDTransportationUnitWorkflowDescriptorCode;
			bookingPartyMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			bookingPartyMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode.EK_Destination = "@notificationemail.cargowise.com";

			return new IWorkflowProvider[] { transportationUnit };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.GateManagement;
	}
}
