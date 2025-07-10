using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business.Test
{
	class WorkflowDescriptorsTest : TestCaseWithFactory
	{
		void AssertGeneratedList(CodeDescriptionPairList list, Func<WorkflowDescriptor, bool> predicate)
		{
			foreach (var value in WorkflowDescriptors.Instance.Values)
			{
				AssertEquals(@"
ATTENTION!

If this test fails, simply run ""bin\Workflow.CodeGeneration.exe"", which should regnerate this list correctly.

ATTENTION!", list.ContainsCode(value.Code), predicate(value));
			}
		}

		public void TestWorkflowDescriptorList()
		{
			AssertGeneratedList(new WorkflowDescriptorList(), w => w.SupportsWorkflowTypeFilters);
		}

		public void TestFullWorkflowDescriptorList()
		{
			AssertGeneratedList(new FullWorkflowDescriptorList(), w => true);
		}

		public void TestTemplateWorkflowDescriptorList()
		{
			AssertGeneratedList(new TemplateWorkflowDescriptorList(), w => w.SupportsWorkflowTypeFilters && (w.SupportsWorkflowTemplates || w.SupportsUniversalTemplates));
		}

		public void TestBMSWorkflowDescriptorList()
		{
			AssertGeneratedList(new BMSWorkflowDescriptorList(), w => w.SupportsWorkflowTypeFilters && w.SupportsBufferManagement);
		}

		public void TestEventPublisherWorkflowDescriptorList()
		{
			AssertGeneratedList(new EventPublisherWorkflowDescriptorList(), w => w.GetType().GetInterface("IEventPublisher") != null);
		}

		public void TestWorkflowDescriptorListOrderByCode()
		{
			var dummyDescriptor = DummyWorkflowDescriptor.Instance;
			var codes = new FullWorkflowDescriptorList().GetAllCodes();
			Assert("Contains dummy descriptor.", codes.Contains(dummyDescriptor.Code));
			Assert("Ordered by code", codes.SequenceEqual(codes.OrderBy(code => code)));
		}
	}
}
