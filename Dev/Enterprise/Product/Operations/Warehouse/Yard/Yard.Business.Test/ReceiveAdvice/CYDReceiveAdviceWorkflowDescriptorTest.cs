using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReceiveAdviceWorkflowDescriptor))]
	public class CYDReceiveAdviceWorkflowDescriptorTest : WorkflowDescriptorTestCase<CYDReceiveAdviceWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Container Yard Receive Advice", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CYDReceiveAdviceWorkflowDescriptorCode, WorkflowDescriptor.Code);
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

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var advice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			var jobDocAddress = advice.DocAddresses.AddNew();
			jobDocAddress.OrganisationPK = DeliveryCartageOrg.PK;
			jobDocAddress.E2_AddressType = AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress;
			return new IWorkflowProvider[] { advice };
		}
	}
}
