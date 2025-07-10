using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountWaveWorkflowDescriptor))]
	public class WhsCycleCountWaveWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsCycleCountWaveWorkflowDescriptor>
	{
		#region TestID

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.WhsCycleCountWaveWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Warehouse Cycle Count Wave", WorkflowDescriptor.Description);
		}
		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals("No sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestSupportsWorkflowTemplates

		public override void TestSupportsWorkflowTemplates() => AssertEquals(expected: false, WorkflowDescriptor.SupportsWorkflowTemplates);

		#endregion

		#region SupportsUniversalTemplates

		public void TestSupportsUniversalTemplates() => AssertEquals(expected: false, WorkflowDescriptor.SupportsUniversalTemplates);

		#endregion

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsCycleCountWave>() };
		}
	}
}
