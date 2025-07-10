using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ContainerLoadListLineWorkflowDescriptor))]
	sealed class ContainerLoadListLineWorkflowDescriptorTest : WorkflowDescriptorTestCase<ContainerLoadListLineWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Container Load List Line", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub types", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var line = Factory.NewWithValidTestData<ContainerLoadListLine>();
			line.CLL_LoadMode = CommonContainerLoadListLoadModeList.Codes.CY;
			return [line];
		}

		public void TestValidationToolSettings()
		{
			AssertType<ContainerLoadListLineValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}
	}
}
