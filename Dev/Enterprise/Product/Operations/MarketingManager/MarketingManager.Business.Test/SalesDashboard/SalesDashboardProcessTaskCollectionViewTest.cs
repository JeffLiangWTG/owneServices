using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesDashboardProcessTaskCollectionView))]
	sealed class SalesDashboardProcessTaskCollectionViewTest : ProcessTaskCollectionViewTest<SalesDashboardProcessTaskCollectionView>
	{
		public override void TestIsThisPartOfTheCollection()
		{
			var task = Collection.AddNew();
			task.IsMilestone = false;
			task.IsException = false;

			var milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.IsException = false;

			var exception = Collection.AddNew();
			exception.IsMilestone = false;
			exception.IsException = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", task, Collection[0]);

			AssertTaskTypeIsThisPartOfTheCollection(task, ProcessTaskStatusCodeList.Codes.Assigned, expectedIsPartOfCollection: true);
			AssertTaskTypeIsThisPartOfTheCollection(task, ProcessTaskStatusCodeList.Codes.Working, expectedIsPartOfCollection: true);
			AssertTaskTypeIsThisPartOfTheCollection(task, ProcessTaskStatusCodeList.Codes.Suspended, expectedIsPartOfCollection: true);
			AssertTaskTypeIsThisPartOfTheCollection(task, ProcessTaskStatusCodeList.Codes.Closed, expectedIsPartOfCollection: true);
			AssertTaskTypeIsThisPartOfTheCollection(task, ProcessTaskStatusCodeList.Codes.Open, expectedIsPartOfCollection: false);
			AssertTaskTypeIsThisPartOfTheCollection(task, ProcessTaskStatusCodeList.Codes.Cancelled, expectedIsPartOfCollection: false);
		}

		void AssertTaskTypeIsThisPartOfTheCollection(ProcessTask task, ZString taskType, bool expectedIsPartOfCollection)
		{
			task.P9_Status = taskType;
			Collection.Rebuild();
			if (expectedIsPartOfCollection)
			{
				AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
				AssertEquals("The item that returns true on IsTypeMatch should be included", task, Collection[0]);
			}
			else
			{
				AssertEquals("The item that returns false on IsTypeMatch should not be included", 0, Collection.Count);
			}
		}

		public override void TestSetDefaultsForNewChild_LoadingTasksFromTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = true;
			template.P0_ProcessType = ((IWorkflowProviderCore)Dummy).WorkflowType;
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			AddTask(template, "", "", ZGuid.Empty, ZGuid.Empty); // First task to be initialized later
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "U1", ZGuid.Empty, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "", group.PK, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, capability.PK);

			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, ZGuid.Empty,
				ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, ZGuid.Empty);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Assigned, "U1", ZGuid.Empty, ZGuid.Empty,
				ProcessTaskStatusCodeList.Codes.Assigned, "U1", ZGuid.Empty, ZGuid.Empty);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Assigned, "", group.PK, ZGuid.Empty,
				ProcessTaskStatusCodeList.Codes.Assigned, "", group.PK, ZGuid.Empty);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, capability.PK,
				ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, capability.PK);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Assigned, "U1", group.PK, capability.PK,
				ProcessTaskStatusCodeList.Codes.Assigned, "U1", group.PK, capability.PK);

			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Closed, "U1", group.PK, capability.PK,
				ProcessTaskStatusCodeList.Codes.Closed, "U1", group.PK, capability.PK);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Description = "Match";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			return task;
		}

		new SalesDashboardProcessTaskCollectionView Collection
		{
			get { return (SalesDashboardProcessTaskCollectionView)base.Collection; }
		}

		protected override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new SalesDashboardProcessTaskCollectionView(collection);
		}

		#endregion
	}
}
