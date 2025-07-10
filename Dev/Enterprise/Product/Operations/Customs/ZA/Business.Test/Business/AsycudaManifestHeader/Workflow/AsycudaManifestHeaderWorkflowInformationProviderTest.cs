using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderWorkflowInformationProviderTest : WorkflowProviderTest<AsycudaManifestHeader, ProcessTaskCollection<ZAsycudaManifestHeaderProcessTask, AsycudaManifestHeader>>
	{
		public void TestEverything()
		{
			IWorkflowProvider asycuda = Factory.New<AsycudaManifestHeader>();
			AssertType(typeof(AsycudaManifestHeaderWorkflowInformationProvider), asycuda.GetWorkflowInformationProvider());
			AssertType(typeof(ProcessTaskCollection<ZAsycudaManifestHeaderProcessTask, AsycudaManifestHeader>), asycuda.WorkflowItems);
			AssertType(typeof(ColumnValueRanker), asycuda.GetTemplateSelectionCriteria());
		}

		public new void TestTaskCanLoadParent()
		{
			var parent = BusinessObject;
			var task = parent.WorkflowItems.Tasks.AddNew();
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(parent.WorkflowType);
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a workflow descriptor with code " + parent.WorkflowType + " present in WorkflowDescriptors.Instance. This is done in the WorkflowDescriptors element of WorkflowDescriptorsConfiguration.xml.", descriptor);
				Assert("Parent and provider type should be the same", descriptor.WorkflowProviderType.IsAssignableFrom(ParentProxyType ?? task.Parent.GetType()));
				AssertNotEquals("Parent type never ought to be this.", typeof(BusinessObject), task.Parent.GetType());
				AssertEquals("But maybe we can load the task anyway", GetParent(parent), task.Parent);
			});
			OnFinishedRunningTestThatLoadsWorkflowDescriptor();
		}

		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.AsycudaManifestHeaderWorkflowDescriptorCode;
			}
		}

		public override void TestProcessTasksCreatedOnSave() //ProcessTask Created?
		{
			var asycuda = GetNewBusinessObject(Factory);
			if (asycuda.SupportsWorkflow)
			{
				base.TestProcessTasksCreatedOnSave();
			}
			else
			{
				Assert(true);
			}
		}
	}
}
