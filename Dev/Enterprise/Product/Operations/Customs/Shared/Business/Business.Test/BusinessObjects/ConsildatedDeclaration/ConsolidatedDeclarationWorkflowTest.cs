using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ProcessTaskCollection<ConsolidatedDeclarationProcessTask, ConsolidatedDeclaration>))]
	sealed class ConsolidatedDeclarationProcessTaskCollectionTest : ProcessTaskCollectionTest<ProcessTaskCollection<ConsolidatedDeclarationProcessTask, ConsolidatedDeclaration>>
	{
		protected override ProcessTaskCollection<ConsolidatedDeclarationProcessTask, ConsolidatedDeclaration> GetCollectionToTestCore()
		{
			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			var workFlowProvider = (IWorkflowProvider)consolidatedDeclaration;
			return (ProcessTaskCollection<ConsolidatedDeclarationProcessTask, ConsolidatedDeclaration>)workFlowProvider.WorkflowItems;
		}
	}

	[TestedType(typeof(ConsolidatedDeclarationProcessTask))]
	sealed class ConsolidatedDeclarationProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var consolidatedDeclaration = Factory.New<ConsolidatedDeclaration>();
			var workFlowProvider = (IWorkflowProvider)consolidatedDeclaration;
			return workFlowProvider.WorkflowItems.AddNew();
		}
	}

	[TestedType(typeof(ConsolidatedDeclaration))]
	sealed class ConsolidatedDeclarationWorkflowProviderTest : WorkflowProviderTest<ConsolidatedDeclaration, ProcessTaskCollection<ConsolidatedDeclarationProcessTask, ConsolidatedDeclaration>>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ConsolidatedDeclarationWorkflowDescriptorCode;
	}

	[TestedType(typeof(ConsolidatedDeclarationWorkflowDescriptor))]
	sealed class ConsolidatedDeclarationWorkflowDescriptorTest : WorkflowDescriptorTestCase<ConsolidatedDeclarationWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.ConsolidatedDeclarationWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Consolidated Entry", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			Assert(true);
		}

		public override void TestRequiresClient()
		{
			Assert(true);
		}

		public override void TestRequiresBranch()
		{
			Assert(true);
		}

		public override void TestRequiresDepartment()
		{
			Assert(true);
		}

		public override void TestSupportsEventTracking()
		{
			Assert(WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestValidationToolSettings()
		{
			AssertType<ConsolidatedDeclarationValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { Factory.New<ConsolidatedDeclaration>() };
	}
}
