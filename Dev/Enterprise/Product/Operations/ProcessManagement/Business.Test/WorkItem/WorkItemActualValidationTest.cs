using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class WorkItemActualValidationTest : WorkItemValidationTest<WorkItem>
	{
		public void TestWorkItemType()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = "XXX";
			AssertHasErrors(workItem.WKI_WorkItemTypeInfo);

			workItem.WKI_WorkItemType = workItem.Lookups.ActiveTypes[0].Code;
			AssertNoErrors(workItem.WKI_WorkItemTypeInfo);

			workItem.WKI_WorkItemType = "";
			AssertNoErrors(workItem.WKI_WorkItemTypeInfo);

			workItem.WKI_WorkItemType = "1ZZ";
			AssertHasErrors(workItem.WKI_WorkItemTypeInfo);

			Factory.Save();
			workItem.Validation.ValidateWKI_WorkItemType();
			AssertNoErrors("inactive codes don't give error", workItem.WKI_WorkItemTypeInfo);
			AssertHasWarnings("inactive codes give warning", workItem.WKI_WorkItemTypeInfo);
		}

		public void TestWorkItemArea()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = workItem.Lookups.ActiveTypes[0].Code;

			workItem.WKI_WorkItemArea = "XXX";
			AssertHasErrors(workItem.WKI_WorkItemAreaInfo);

			workItem.WKI_WorkItemArea = workItem.Lookups.ActiveAreas[0].Code;
			AssertNoErrors(workItem.WKI_WorkItemAreaInfo);

			workItem.WKI_WorkItemArea = "";
			AssertNoErrors(workItem.WKI_WorkItemAreaInfo);
		}

		public void TestActivityType()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = workItem.Lookups.ActiveTypes[0].Code;
			workItem.WKI_WorkItemArea = workItem.Lookups.ActiveAreas[0].Code;

			workItem.WKI_ActivityType = "XXX";
			AssertHasErrors(workItem.WKI_ActivityTypeInfo);

			workItem.WKI_ActivityType = workItem.Lookups.ActiveActivityTypes[0].Code;
			AssertNoErrors(workItem.WKI_ActivityTypeInfo);

			workItem.WKI_ActivityType = "";
			AssertNoErrors(workItem.WKI_ActivityTypeInfo);
		}

		public void TestActivitySubtype()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = workItem.Lookups.ActiveTypes[0].Code;
			workItem.WKI_WorkItemArea = workItem.Lookups.ActiveAreas[0].Code;
			workItem.WKI_ActivityType = workItem.Lookups.ActiveActivityTypes[0].Code;

			workItem.WKI_ActivitySubtype = "XXX";
			AssertHasErrors(workItem.WKI_ActivitySubtypeInfo);

			workItem.WKI_ActivitySubtype = workItem.Lookups.ActiveActivitySubtypes[0].Code;
			AssertNoErrors(workItem.WKI_ActivitySubtypeInfo);

			workItem.WKI_ActivitySubtype = "";
			AssertNoErrors(workItem.WKI_ActivitySubtypeInfo);
		}

		public void TestPriority()
		{
			WorkItem workItem = Factory.New<WorkItem>();
			workItem.WKI_Priority = "LOW";
			AssertNoErrors(workItem.WKI_PriorityInfo);

			workItem.WKI_Priority = "XXX";
			AssertHasErrors(workItem.WKI_PriorityInfo);

			workItem.WKI_Priority = "5ZZ";
			AssertHasErrors(workItem.WKI_PriorityInfo);

			workItem.WKI_Priority = "";
			AssertNoErrors(workItem.WKI_PriorityInfo);
		}

		public void TestSummary()
		{
			var workItem = Factory.New<WorkItem>();
			workItem.Validation.ValidateWKI_Summary();
			AssertHasErrors(workItem.WKI_SummaryInfo);

			workItem.WKI_Summary = "not blank";
			AssertNoNotifications(workItem.WKI_SummaryInfo);
		}

		public void TestStatus()
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes element = collection.AddNew();
			element.Code = "WKI";
			WorkflowTaskType taskType = element.TaskTypes.AddNew();
			taskType.Code = "WRK";
			taskType.Description = (NoResString)"Stuff";
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var workItem = Factory.New<WorkItem>();
			workItem.WKI_Status = "WRK";
			AssertNoNotifications(workItem.WKI_StatusInfo);

			workItem.WKI_Status = "XXX";
			AssertHasErrors(workItem.WKI_StatusInfo);

			workItem.WKI_Status = "";
			AssertNoNotifications(workItem.WKI_StatusInfo);
		}

		public void TestWorkItemTypeIsMandatory()
		{
			ProcessManagementRegistry.Instance.WorkItemTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = "";
			workItem.RunPreSaveValidation();
			AssertNoErrors(workItem.WKI_WorkItemTypeInfo);

			ProcessManagementRegistry.Instance.WorkItemTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workItem.RunPreSaveValidation();
			AssertMandatoryValidationError(workItem.WKI_WorkItemTypeInfo, true);
		}

		public void TestWorkItemAreaIsMandatory()
		{
			ProcessManagementRegistry.Instance.WorkItemAreaMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemArea = "";
			workItem.RunPreSaveValidation();
			AssertNoErrors(workItem.WKI_WorkItemAreaInfo);

			ProcessManagementRegistry.Instance.WorkItemAreaMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workItem.RunPreSaveValidation();
			AssertMandatoryValidationError(workItem.WKI_WorkItemAreaInfo, true);
		}

		public void TestActivityTypeIsMandatory()
		{
			ProcessManagementRegistry.Instance.WorkItemActivityTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_ActivityType = "";
			workItem.RunPreSaveValidation();
			AssertNoErrors(workItem.WKI_ActivityTypeInfo);

			ProcessManagementRegistry.Instance.WorkItemActivityTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workItem.RunPreSaveValidation();
			AssertMandatoryValidationError(workItem.WKI_ActivityTypeInfo, true);
		}

		public void TestActivitySubtypeIsMandatory()
		{
			ProcessManagementRegistry.Instance.WorkItemActivitySubTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_ActivitySubtype = "";
			workItem.RunPreSaveValidation();
			AssertNoErrors(workItem.WKI_ActivitySubtypeInfo);

			ProcessManagementRegistry.Instance.WorkItemActivitySubTypeMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workItem.RunPreSaveValidation();
			AssertMandatoryValidationError(workItem.WKI_ActivitySubtypeInfo, true);
		}

		public void TestPriorityIsMandatory()
		{
			ProcessManagementRegistry.Instance.WorkItemPriorityMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_Priority = "";
			workItem.RunPreSaveValidation();
			AssertNoErrors(workItem.WKI_PriorityInfo);

			ProcessManagementRegistry.Instance.WorkItemPriorityMandatory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			workItem.RunPreSaveValidation();
			AssertMandatoryValidationError(workItem.WKI_PriorityInfo, true);
		}

		public void TestDefectCausedByWorkItemPK()
		{
			var causedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			var causedWorkItemTask = causedWorkItem.WorkflowItems.AddNew();
			causedWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var fixWorkItem = Factory.NewWithValidTestData<WorkItem>();
			fixWorkItem.WKI_ActivitySubtype = "UDF";
			fixWorkItem.RunPreSaveValidation();
			AssertNoErrors(fixWorkItem.DefectCausedByWorkItemPKInfo);

			fixWorkItem.DefectCausedByWorkItemPK = ZGuid.Invalid;
			fixWorkItem.RunPreSaveValidation();
			AssertHasErrors(fixWorkItem.DefectCausedByWorkItemPKInfo);

			fixWorkItem.DefectCausedByWorkItemPK = causedWorkItem.PK;
			fixWorkItem.RunPreSaveValidation();
			AssertHasError(fixWorkItem.DefectCausedByWorkItemPKInfo, "Work Item must have at least one closed task of the type that can introduce a defect.");

			causedWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			fixWorkItem.RunPreSaveValidation();
			AssertHasError(fixWorkItem.DefectCausedByWorkItemPKInfo, "Work Item must have at least one closed task of the type that can introduce a defect.");

			causedWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			// 3 lines below ensure that the caused work item is a valid work item (refer to lines 98-108 in WorkItemActualValidation.cs)
			ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "CH0" });
			causedWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			causedWorkItemTask.P9_Type = "CH0";
			fixWorkItem.RunPreSaveValidation();
			AssertNoErrors(fixWorkItem.DefectCausedByWorkItemPKInfo);
		}

		public void TestWKI_P9_DefectCausedByTask()
		{
			WorkItemProcessTask randomWorkItemTask = Factory.NewWithValidTestData<WorkItemProcessTask>();
			WorkItem causedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			WorkItemProcessTask causedWorkItemTask = causedWorkItem.WorkflowItems.AddNew();
			// 3 lines below ensure that the caused work item is a valid work item (refer to lines 98-108 in WorkItemActualValidation.cs)
			ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "CH0" });
			causedWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			causedWorkItemTask.P9_Type = "CH0";
			Factory.Save();

			WorkItem fixWorkItem = Factory.NewWithValidTestData<WorkItem>();
			fixWorkItem.RunPreSaveValidation();
			AssertNoErrors(fixWorkItem.WKI_P9_DefectCausedByTaskInfo);

			fixWorkItem.DefectCausedByWorkItemPK = causedWorkItem.PK;

			fixWorkItem.WKI_P9_DefectCausedByTask = ZGuid.Empty;
			fixWorkItem.RunPreSaveValidation();
			AssertHasErrors("Should set a task if caused WI is set", fixWorkItem.WKI_P9_DefectCausedByTaskInfo);

			fixWorkItem.WKI_P9_DefectCausedByTask = ZGuid.Invalid;
			fixWorkItem.RunPreSaveValidation();
			AssertHasErrors("Should set a valid task if caused WI is set", fixWorkItem.WKI_P9_DefectCausedByTaskInfo);

			fixWorkItem.WKI_P9_DefectCausedByTask = causedWorkItemTask.PK;
			fixWorkItem.RunPreSaveValidation();
			AssertNoErrors("Setting a task when the caused WI is set is okay", fixWorkItem.WKI_P9_DefectCausedByTaskInfo);

			fixWorkItem.WKI_P9_DefectCausedByTask = randomWorkItemTask.PK;
			fixWorkItem.RunPreSaveValidation();
			AssertHasErrors("Setting a task that is not part of the caused WI is not okay", fixWorkItem.WKI_P9_DefectCausedByTaskInfo);
		}

		public void TestDefectCausedByTask_NoCausedByTaskIfStatusIsCancelled()
		{
			var fixItem = Factory.NewWithValidTestData<WorkItem>();
			var causedItem = Factory.NewWithValidTestData<WorkItem>();
			var causedTask = causedItem.WorkflowItems.AddNew();

			causedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			fixItem.DefectCausedByWorkItemPK = causedItem.PK;
			fixItem.WKI_P9_DefectCausedByTask = causedTask.PK;
			Factory.Save();

			var newfactory = new BusinessObjectFactory();
			var loadFixItem = newfactory.Load<WorkItem>(fixItem.PK);
			loadFixItem.RunPreSaveValidation();

			AssertNoWarnings(loadFixItem.WKI_P9_DefectCausedByTaskInfo);

			causedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			newfactory = new BusinessObjectFactory();
			loadFixItem = newfactory.Load<WorkItem>(fixItem.PK);
			loadFixItem.RunPreSaveValidation();
			AssertHasWarning(loadFixItem.WKI_P9_DefectCausedByTaskInfo, "Non closed task cannot cause a Defect.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);
		}
	}
}
