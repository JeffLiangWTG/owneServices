using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ProcessTaskCollectionViewTest<T> : ProcessTaskBaseCollectionViewTest<T> where T : ProcessTaskCollectionView
	{
		public override void TestIsThisPartOfTheCollection()
		{
			ProcessTask task = Collection.AddNew();
			task.IsMilestone = false;
			task.IsException = false;

			ProcessTask milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.IsException = false;

			ProcessTask exception = Collection.AddNew();
			exception.IsMilestone = false;
			exception.IsException = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", task, Collection[0]);
		}

		public void TestSetCollectionRelationships()
		{
			ProcessTask task = Dummy.WorkflowItems.Tasks.AddNew();
			AssertEquals("IsTask", true, task.IsTask);
			AssertEquals("P9_ParentID NOT attached to job", Dummy.PK, task.P9_ParentID);
		}

		public new void TestAllowSort()
		{
			var collectionView = new TestProcessTaskCollectionView(new ProcessTaskCollection(Factory));
			AssertEquals("Tasks may be sorted", true, collectionView.AllowSort);
		}

		public void TestSetDefaultsForNewChild_AssignUserToFirstTask()
		{
			AssertEquals("Collection should be empty at this point", 0, Collection.Count);
			ProcessTask task = Collection.AddNew();
			AssertEquals("If first task is entered manually it must be set to current user", GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);

			ProcessTask task2 = Collection.AddNew();
			AssertEquals("If second task is entered manually it must be set to current user", GlbStaff.CurrentUser.GS_Code, task2.P9_GS_NKAssignedStaffMember);
		}

		public void TestTasksViewFilterInvokesTasksViewFilterAddedEventWhenSetToNonNullValue()
		{
			var tasksViewFilterHasBeenAdded = false;

			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var parent = Factory.New<OrgOpportunity>();
				var collection = new ProcessTaskCollection(parent);
				collection.Tasks.TasksViewFilterAdded += SetTasksViewFilterHasBeenAddedToTrue;

				AssertEquals("Precondition: TasksViewFilterAdded event should not have triggered yet", false, tasksViewFilterHasBeenAdded);

				new ProcessTaskCollectionViewFilter(collection);

				AssertEquals("TasksViewFilterAdded event should have been triggered", true, tasksViewFilterHasBeenAdded);
			}

			void SetTasksViewFilterHasBeenAddedToTrue(object sender, EventArgs e)
			{
				tasksViewFilterHasBeenAdded = true;
			}
		}

		#region TestSetDefaultsForNewChild_LoadingTasksFromTemplate

		public virtual void TestSetDefaultsForNewChild_LoadingTasksFromTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = true;
			template.P0_ProcessType = ((IWorkflowProviderCore)Dummy).WorkflowType;
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			AddTask(template, "", "", ZGuid.Empty, ZGuid.Empty); // First task to be initialized later
			AddTask(template, ProcessTaskStatusCodeList.Codes.Open, "", ZGuid.Empty, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Open, "U1", ZGuid.Empty, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Open, "", group.PK, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Open, "", ZGuid.Empty, capability.PK);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "U1", ZGuid.Empty, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "", group.PK, ZGuid.Empty);
			AddTask(template, ProcessTaskStatusCodeList.Codes.Assigned, "", ZGuid.Empty, capability.PK);

			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Open, "", ZGuid.Empty, ZGuid.Empty,
				ProcessTaskStatusCodeList.Codes.Assigned, GlbStaff.CurrentUser.GS_Code, ZGuid.Empty, ZGuid.Empty);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Open, "U1", ZGuid.Empty, ZGuid.Empty,
				ProcessTaskStatusCodeList.Codes.Assigned, GlbStaff.CurrentUser.GS_Code, ZGuid.Empty, ZGuid.Empty);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Open, "", group.PK, ZGuid.Empty,
				ProcessTaskStatusCodeList.Codes.Assigned, GlbStaff.CurrentUser.GS_Code, group.PK, ZGuid.Empty);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Open, "", ZGuid.Empty, capability.PK,
				ProcessTaskStatusCodeList.Codes.Assigned, GlbStaff.CurrentUser.GS_Code, ZGuid.Empty, capability.PK);
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Open, "U1", group.PK, capability.PK,
				ProcessTaskStatusCodeList.Codes.Assigned, GlbStaff.CurrentUser.GS_Code, group.PK, capability.PK);

			// subsequent application of template should not reassign the first task to the current user
			Collection[0].P9_GS_NKAssignedStaffMember = "U1";
			Collection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(template,
				ProcessTaskStatusCodeList.Codes.Open, "U1", group.PK, capability.PK,
				ProcessTaskStatusCodeList.Codes.Open, "U1", group.PK, capability.PK, false);

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

		public void TestElementCanBeAdded()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "Cumpnee";

			IWorkflowProvider shipment = (IWorkflowProvider)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var task = shipment.WorkflowItems.Tasks.AddNew();
			task.P9_GC = company.PK;
			task.P9_ShareTasksForAllCompanies = false;

			Factory.Save();

			template.WorkflowItems.Tasks.Add(task);
			AssertEquals(0, template.WorkflowItems.Tasks.Count);
		}

		protected void AddTask(ProcessTaskTemplate template, ZString status, ZString staff, ZGuid group, ZGuid capability)
		{
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Status = status;
			task.P9_GS_NKAssignedStaffMember = staff;
			task.P9_GG_AssignedGroup = group;
			task.P9_G4_RequiredCapability = capability;
		}

		protected void AssertSetDefaultsForNewChild_LoadingTasksFromTemplate(ProcessTaskTemplate template,
			ZString initialStatus, ZString initialStaff, ZGuid initialGroup, ZGuid initialCapability,
			ZString expectedStatus, ZString expectedStaff, ZGuid expectedGroup, ZGuid expectedCapability, bool reset = true)
		{
			template.WorkflowItems.Tasks[0].P9_GS_NKAssignedStaffMember = initialStaff;
			template.WorkflowItems.Tasks[0].P9_GG_AssignedGroup = initialGroup;
			template.WorkflowItems.Tasks[0].P9_G4_RequiredCapability = initialCapability;
			template.WorkflowItems.Tasks[0].P9_Status = initialStatus; // Should be initialized last
			template.WorkflowItems.Tasks[0].P9_CompletedTimeUtc = ZDateTime.Now;

			if (reset)
			{
				Collection.RemoveAndDeleteAll();
			}

			Factory.Save();

			Collection.CreateItemsFromTemplate();

			AssertEquals(template.WorkflowItems.Tasks.Count, Collection.Count);

			AssertEquals(expectedStatus, Collection[0].P9_Status);
			AssertEquals(expectedStaff, Collection[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(expectedGroup, Collection[0].P9_GG_AssignedGroup);
			AssertEquals(expectedCapability, Collection[0].P9_G4_RequiredCapability);

			for (int i = 1; i < Collection.Count; i++)
			{
				AssertEquals(template.WorkflowItems.Tasks[i].P9_Status, Collection[i].P9_Status);
				AssertEquals(template.WorkflowItems.Tasks[i].P9_GS_NKAssignedStaffMember, Collection[i].P9_GS_NKAssignedStaffMember);
				AssertEquals(template.WorkflowItems.Tasks[i].P9_GG_AssignedGroup, Collection[i].P9_GG_AssignedGroup);
				AssertEquals(template.WorkflowItems.Tasks[i].P9_G4_RequiredCapability, Collection[i].P9_G4_RequiredCapability);
			}
		}

		#endregion

		#region Rebuild

		public void TestRebuild()
		{
			ProcessTask task = Collection.AddNew();

			TestRebuild(task, "", "", "", Guid.Empty, "", "", "", Guid.Empty, true);
			TestRebuild(task, "", "", "", Guid.Empty, "A", "", "", Guid.Empty, true);
			TestRebuild(task, "", "", "", Guid.Empty, "", "B", "", Guid.Empty, true);
			TestRebuild(task, "", "", "", Guid.Empty, "A", "B", "", Guid.Empty, true);

			TestRebuild(task, "A", "", "", Guid.Empty, "A", "", "", Guid.Empty, true);
			TestRebuild(task, "A", "", "", Guid.Empty, "A", "B", "", Guid.Empty, true);
			TestRebuild(task, "A", "", "", Guid.Empty, "B", "", "", Guid.Empty, false);
			TestRebuild(task, "A", "", "", Guid.Empty, "B", "A", "", Guid.Empty, false);
			TestRebuild(task, "A", "", "", Guid.Empty, "", "", "", Guid.Empty, false);

			TestRebuild(task, "", "B", "", Guid.Empty, "", "B", "", Guid.Empty, true);
			TestRebuild(task, "", "B", "", Guid.Empty, "A", "B", "", Guid.Empty, true);
			TestRebuild(task, "", "B", "", Guid.Empty, "", "", "", Guid.Empty, false);
			TestRebuild(task, "", "B", "", Guid.Empty, "B", "", "", Guid.Empty, false);
			TestRebuild(task, "", "B", "", Guid.Empty, "", "", "", Guid.Empty, false);

			TestRebuild(task, "A", "B", "", Guid.Empty, "A", "B", "", Guid.Empty, true);
			TestRebuild(task, "A", "B", "", Guid.Empty, "A", "D", "", Guid.Empty, false);
			TestRebuild(task, "A", "B", "", Guid.Empty, "C", "B", "", Guid.Empty, false);
			TestRebuild(task, "A", "B", "", Guid.Empty, "", "", "", Guid.Empty, false);
			TestRebuild(task, "A", "B", "", Guid.Empty, "A", "", "", Guid.Empty, false);
			TestRebuild(task, "A", "B", "", Guid.Empty, "", "B", "", Guid.Empty, false);

			TestRebuild(task, "A", "B", "ZA", Guid.Empty, "A", "B", "ZA", Guid.Empty, true);
			TestRebuild(task, "A", "B", "", Guid.Empty, "A", "B", "ZA", Guid.Empty, true);
			TestRebuild(task, "A", "B", "ZA", Guid.Empty, "A", "B", "RA", Guid.Empty, false);
			TestRebuild(task, "A", "B", "LA", Guid.Empty, "A", "B", "", Guid.Empty, false);

			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid guid3 = Guid.NewGuid();

			TestRebuild(task, "A", "B", "ZA", guid1, "A", "B", "ZA", guid1, true);
			TestRebuild(task, "A", "B", "ZA", Guid.Empty, "A", "B", "ZA", guid1, true);
			TestRebuild(task, "A", "B", "ZA", guid1, "A", "B", "ZA", guid2, false);
			TestRebuild(task, "A", "B", "LA", guid3, "A", "B", "", Guid.Empty, false);
		}

		void TestRebuild(ProcessTask task,
			string p9_StatusFilter, string p9_TypeFilter, string staffCodeFilter, Guid groupPKFilter,
			string p9_Status, string p9_Type, string staffCode, Guid groupPK,
			bool isContain)
		{
			task.P9_Status = p9_Status;
			task.P9_Type = p9_Type;
			task.P9_GS_NKAssignedStaffMember = staffCode;
			task.P9_GG_AssignedGroup = groupPK;

			Collection.Rebuild(p9_StatusFilter, p9_TypeFilter, staffCodeFilter, groupPKFilter);
			AssertEquals("Contains(Task)", isContain, Collection.Contains(task));
		}

		#endregion

		#region Test Classes

		class TestProcessTaskCollectionView : ProcessTaskCollectionView
		{
			public TestProcessTaskCollectionView(ProcessTaskCollection collection)
				: base(collection)
			{
			}

			public new bool AllowSort
			{
				get { return base.AllowSort; }
			}
		}

		#endregion

		#region Implementation

		new ProcessTaskCollectionView Collection
		{
			get { return (ProcessTaskCollectionView)base.Collection; }
		}

		protected override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new ProcessTaskCollectionView(collection);
		}

		#endregion
	}
}
