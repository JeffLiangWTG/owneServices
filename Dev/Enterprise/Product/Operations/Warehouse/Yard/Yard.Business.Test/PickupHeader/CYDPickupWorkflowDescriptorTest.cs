using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupWorkflowDescriptor))]
	public class CYDPickupWorkflowDescriptorTest : WorkflowDescriptorTestCase<CYDPickupWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Container Yard Pick up", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CYDPickupWorkflowDescriptorCode, WorkflowDescriptor.Code);
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
			AssertEquals("Client", WorkflowDescriptor.ClientName);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(CYDPickup), WorkflowDescriptor.WorkflowProviderType);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();

			var pickup = Factory.NewWithValidTestData<CYDPickup>();
			pickup.YPL_YTU_PickupTransportationUnit = transportationUnit.PK;

			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState.YUS_YPL_Pickup = pickup.PK;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			pickup.LinkedYardUnit.CurrentYard.WW_OA_WarehouseAddress = orgAddress.PK;
			var bookingPartyMode = orgAddress.Header.EDICommunicationsModes.AddNew();
			bookingPartyMode.EK_Module = WorkflowDescriptors.CYDPickupWorkflowDescriptorCode;
			bookingPartyMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			bookingPartyMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode.EK_Destination = "@notificationemail.cargowise.com";

			return new IWorkflowProvider[] { transportationUnit };
		}

		protected override BusinessObject SetupLineTriggerIfRequired(IWorkflowProvider parent, ProcessTask trigger)
		{
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.CYDPickup;
			return ((CYDTransportationUnit)parent).Pickups[0];
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.GateManagement;
	}
}
