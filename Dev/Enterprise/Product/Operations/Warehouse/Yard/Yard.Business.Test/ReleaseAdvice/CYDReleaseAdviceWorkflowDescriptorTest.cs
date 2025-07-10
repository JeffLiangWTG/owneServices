using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceWorkflowDescriptor))]
	public class CYDReleaseAdviceWorkflowDescriptorTest : WorkflowDescriptorTestCase<CYDReleaseAdviceWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Container Yard Release Advice", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.CYDReleaseAdviceWorkflowDescriptorCode, WorkflowDescriptor.Code);
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
			var releaseAdvice = Factory.NewWithValidTestData<CYDReleaseAdvice>();
			return new IWorkflowProvider[] { releaseAdvice };
		}
	}
}

