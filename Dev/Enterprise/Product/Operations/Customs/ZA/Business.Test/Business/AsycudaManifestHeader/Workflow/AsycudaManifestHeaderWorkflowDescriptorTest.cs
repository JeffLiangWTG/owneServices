using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderWorkflowDescriptor))]
	sealed class AsycudaManifestHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<AsycudaManifestHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Outturn & Gate In/Out", WorkflowDescriptor.Description);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new[] { Factory.New<AsycudaManifestHeader>() };
		}

		public override void TestRequiresBranch()
		{
			Assert(true);
		}

		public override void TestRequiresClient()
		{
			Assert(true);
		}

		public override void TestRequiresPorts()
		{
			Assert(true);
		}

		public override void TestSubTypes()
		{
			Assert(true);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsTasks);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsScreenLayout);
		}
	}
}
