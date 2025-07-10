using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowItemCollectionViewTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMissingCompaniesDoesntPuke()
		{
			WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var dummy = Factory.New<DummyWithWorkflowWithWorkflowProvider>();
			var collection = new WorkflowTriggerCollectionView(dummy.WorkflowItems);
			collection.CreateItemsFromTemplate();
		}

		[ExpectNoExceptions]
		public void TestCollectionWorkflows()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			var system = (BMSystem)helper.CreateSystem(Factory, "");
			var collection = new WorkflowTriggerCollectionView(new ProcessTaskCollection(Factory));
			AssertNull((collection as IWorkflowProvider).Workflows);
		}

		class DummyWithWorkflowWithWorkflowProvider : DummyWithWorkflow, IWorkflowProvider
		{
			public DummyWithWorkflowWithWorkflowProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
			{
				return new WorkflowInformationProvider(companies: null);
			}
		}

		public void TestReportErrorOnRebuildDuringBinding()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Tasks.AddNew();
			using (dummy.WorkflowItems.Tasks.Binding())
			{
				dummy.WorkflowItems.Tasks.Rebuild();
			}

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}
	}
}
