using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class TaskTrialAssignerTestCase : TestCaseWithFactory
	{
		public void TestTryAssignResourceToTask_NoRestrictions()
		{
			SetupRegistry(setupSamRestrictions: false, setupCBCDifRestrictions: false, setupCBFDifRestrictions: false, setupCNTDifRestrictions: false);
			SetupResourceAndTasks();
			Factory.Save();

			var assigner = new TaskTrialAssigner();
			assigner.AssignResourceToTask(resource.GS_Code, taskINV);

			Assert("taskINV should be assigned to the resource trially", !assigner.GetAssignedStaffCode(taskINV).IsEmpty);
			Assert("taskCDU should not be assigned as there are no SAM restrictions", assigner.GetAssignedStaffCode(taskCDU).IsEmpty);
			Assert("taskCDF should not be assigned as there are no SAM restrictions", assigner.GetAssignedStaffCode(taskCDF).IsEmpty);
			Assert("taskCBC should not be assigned", assigner.GetAssignedStaffCode(taskCBC).IsEmpty);
			Assert("taskCBF should not be assigned", assigner.GetAssignedStaffCode(taskCBF).IsEmpty);
			Assert("taskCNT should not be assigned", assigner.GetAssignedStaffCode(taskCNT).IsEmpty);

			Assert("The assignment should be allowed as there are no DIR restrictions", assigner.AreAssignmentsAllowedByDIFRestrictions);
		}

		public void TestTryAssignResourceToTask_SAM_Restrictions()
		{
			SetupRegistry(setupSamRestrictions: true, setupCBCDifRestrictions: false, setupCBFDifRestrictions: false, setupCNTDifRestrictions: false);
			SetupResourceAndTasks();
			Factory.Save();

			var assigner = new TaskTrialAssigner();
			assigner.AssignResourceToTask(resource.GS_Code, taskINV);

			Assert("taskINV should be assigned to the resource trially", !assigner.GetAssignedStaffCode(taskINV).IsEmpty);
			Assert("taskCDU should be assigned due to SAM restrictions", !assigner.GetAssignedStaffCode(taskCDU).IsEmpty);
			Assert("taskCDF should be assigned due to SAM restrictions", !assigner.GetAssignedStaffCode(taskCDF).IsEmpty);
			Assert("taskCBC should not be assigned", assigner.GetAssignedStaffCode(taskCBC).IsEmpty);
			Assert("taskCBF should not be assigned", assigner.GetAssignedStaffCode(taskCBF).IsEmpty);
			Assert("taskCNT should not be assigned", assigner.GetAssignedStaffCode(taskCNT).IsEmpty);

			Assert("The assignment should be allowed as there are no DIR restrictions", assigner.AreAssignmentsAllowedByDIFRestrictions);
		}

		public void TestTryAssignResourceToTask_DIF_ERR_Restrictions()
		{
			SetupRegistry(setupSamRestrictions: false, setupCBCDifRestrictions: true, setupCBFDifRestrictions: false, setupCNTDifRestrictions: false);
			SetupResourceAndTasks();
			taskCBC.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			Factory.Save();

			var assigner = new TaskTrialAssigner();
			Assert("Precondition", !assigner.GetAssignedStaffCode(taskCBC).IsEmpty);

			assigner.AssignResourceToTask(resource.GS_Code, taskINV);

			Assert("taskINV should be assigned to the resource trially", !assigner.GetAssignedStaffCode(taskINV).IsEmpty);
			Assert("taskCDU should not be assigned as there are no SAM restrictions", assigner.GetAssignedStaffCode(taskCDU).IsEmpty);
			Assert("taskCDF should not be assigned as there are no SAM restrictions", assigner.GetAssignedStaffCode(taskCDF).IsEmpty);
			Assert("taskCBF should not be assigned", assigner.GetAssignedStaffCode(taskCBF).IsEmpty);
			Assert("taskCNT should not be assigned", assigner.GetAssignedStaffCode(taskCNT).IsEmpty);

			Assert("The assignment should not be allowed due to direct DIR restrictions between INV and CBC", !assigner.AreAssignmentsAllowedByDIFRestrictions);
		}

		public void TestTryAssignResourceToTask_DIF_WRN_Restrictions()
		{
			SetupRegistry(setupSamRestrictions: false, setupCBCDifRestrictions: false, setupCBFDifRestrictions: true, setupCNTDifRestrictions: false);
			SetupResourceAndTasks();
			taskCBF.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			Factory.Save();

			var assigner = new TaskTrialAssigner();
			Assert("Precondition", !assigner.GetAssignedStaffCode(taskCBF).IsEmpty);

			assigner.AssignResourceToTask(resource.GS_Code, taskINV);

			Assert("taskINV should be assigned to the resource trially", !assigner.GetAssignedStaffCode(taskINV).IsEmpty);
			Assert("taskCDU should not be assigned as there are no SAM restrictions", assigner.GetAssignedStaffCode(taskCDU).IsEmpty);
			Assert("taskCDF should not be assigned as there are no SAM restrictions", assigner.GetAssignedStaffCode(taskCDF).IsEmpty);
			Assert("taskCBC should not be assigned", assigner.GetAssignedStaffCode(taskCBC).IsEmpty);
			Assert("taskCNT should not be assigned", assigner.GetAssignedStaffCode(taskCNT).IsEmpty);

			Assert("The assignment should not be allowed due to direct DIR restrictions between INV and CBF", !assigner.AreAssignmentsAllowedByDIFRestrictions);
		}

		public void TestTryAssignResourceToTask_SAM_And_DIF_Restrictions()
		{
			SetupRegistry(setupSamRestrictions: true, setupCBCDifRestrictions: false, setupCBFDifRestrictions: false, setupCNTDifRestrictions: true);
			SetupResourceAndTasks();
			taskCNT.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			Factory.Save();

			var assigner = new TaskTrialAssigner();
			Assert("Precondition", !assigner.GetAssignedStaffCode(taskCNT).IsEmpty);

			assigner.AssignResourceToTask(resource.GS_Code, taskINV);

			Assert("taskINV should be assigned to the resource trially", !assigner.GetAssignedStaffCode(taskINV).IsEmpty);
			Assert("taskCDU should be assigned due to SAM restrictions", !assigner.GetAssignedStaffCode(taskCDU).IsEmpty);
			Assert("taskCDF should be assigned due to SAM restrictions", !assigner.GetAssignedStaffCode(taskCDF).IsEmpty);
			Assert("taskCBC should not be assigned", assigner.GetAssignedStaffCode(taskCBC).IsEmpty);
			Assert("taskCBF should not be assigned", assigner.GetAssignedStaffCode(taskCBF).IsEmpty);

			Assert("The assignment should not be allowed due to DIR restrictions between CNT and CDF", !assigner.AreAssignmentsAllowedByDIFRestrictions);
		}

		public void TestTryAssignResourceToTask_ShouldNotApplyRestrictionsToNonSpecifiedJobTypes()
		{
			var salesJob = Factory.NewWithValidTestData<SalesEnquiry>();
			var salesWorkflow = ProcessJobHeader.GetForParent(salesJob, Factory).ProcessHeaders.AddNew();

			var salesTaskINV = salesWorkflow.Parent.WorkflowItems.Tasks.AddNew();
			salesTaskINV.P9_FH_ProcessHeader = salesWorkflow.PK;
			salesTaskINV.P9_Type = "INV";
			salesTaskINV.P9_GS_NKAssignedStaffMember = ZString.Empty;

			var salesTaskINV2 = salesWorkflow.Parent.WorkflowItems.Tasks.AddNew();
			salesTaskINV2.P9_FH_ProcessHeader = salesWorkflow.PK;
			salesTaskINV2.P9_Type = "INV";
			salesTaskINV2.P9_GS_NKAssignedStaffMember = ZString.Empty;

			var salesTaskCDU = salesWorkflow.Parent.WorkflowItems.Tasks.AddNew();
			salesTaskCDU.P9_FH_ProcessHeader = salesWorkflow.PK;
			salesTaskCDU.P9_Type = "CDU";
			salesTaskCDU.P9_GS_NKAssignedStaffMember = ZString.Empty;

			SetupRegistry(setupSamRestrictions: true, setupCBCDifRestrictions: false, setupCBFDifRestrictions: false, setupCNTDifRestrictions: true);
			SetupResourceAndTasks();
			taskCNT.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			Factory.Save();

			var assigner = new TaskTrialAssigner();
			Assert("Precondition", !assigner.GetAssignedStaffCode(taskCNT).IsEmpty);

			assigner.AssignResourceToTask(resource.GS_Code, taskINV);

			AssertEquals("taskINV should be assigned to the resource trially", false, assigner.GetAssignedStaffCode(taskINV).IsEmpty);
			AssertEquals("taskCDU should be assigned due to SAM restrictions", false, assigner.GetAssignedStaffCode(taskCDU).IsEmpty);
			AssertEquals("salesTaskINV should not be assigned since the SAM restrictions are only set up for ORG workflow types", true, assigner.GetAssignedStaffCode(salesTaskINV).IsEmpty);
			AssertEquals("salesTaskINV2 should not be assigned since the SAM restrictions are only set up for ORG workflow types", true, assigner.GetAssignedStaffCode(salesTaskINV2).IsEmpty);
			AssertEquals("salesTaskCDU should not be assigned since the SAM restrictions are only set up for ORG workflow types", true, assigner.GetAssignedStaffCode(salesTaskCDU).IsEmpty);

			assigner.AssignResourceToTask(resource.GS_Code, salesTaskINV);

			AssertEquals("salesTaskINV should be assigned to the resource trially", false, assigner.GetAssignedStaffCode(salesTaskINV).IsEmpty);
			AssertEquals("salesTaskINV2 should not be assigned since the SAM restrictions are only set up for ORG workflow types", true, assigner.GetAssignedStaffCode(salesTaskINV2).IsEmpty);
			AssertEquals("salesTaskCDU should not be assigned since the SAM restrictions are only set up for ORG workflow types", true, assigner.GetAssignedStaffCode(salesTaskCDU).IsEmpty);
		}

		public void TestTryAssignResourceToTask_SAM_ShouldNotReopenLinkedTaskTypes()
		{
			SetupRegistry(setupSamRestrictions: true, setupCBCDifRestrictions: false, setupCBFDifRestrictions: false, setupCNTDifRestrictions: false);
			SetupResourceAndTasks();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			taskINV.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskINV.P9_GS_NKAssignedStaffMember = resource.GS_Code;
			taskCDU.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskCDU.P9_GS_NKAssignedStaffMember = ZString.Empty;
			taskCDF.P9_GS_NKAssignedStaffMember = ZString.Empty;
			taskCBC.P9_GS_NKAssignedStaffMember = ZString.Empty;
			taskCBF.P9_GS_NKAssignedStaffMember = ZString.Empty;
			taskCNT.P9_GS_NKAssignedStaffMember = ZString.Empty;

			Factory.SuspendValidation();    // force empty staff on
			Factory.Save();                 // closed task change into
			Factory.ResumeValidation();     // the DB

			AssertEquals("TaskCDU should be closed with an empty assigned staff member", ZString.Empty, taskCDU.P9_GS_NKAssignedStaffMember);

			var assigner = new TaskTrialAssigner();
			assigner.AssignResourceToTask(resource2.GS_Code, taskCDF);

			AssertEquals("taskINV should be assigned to the resource trially", resource.GS_Code, assigner.GetAssignedStaffCode(taskINV));
			AssertEquals("taskCDU should still be unassigned", ZString.Empty, assigner.GetAssignedStaffCode(taskCDU));
			AssertEquals("taskCDF should be assigned due to SAM restrictions", resource2.GS_Code, assigner.GetAssignedStaffCode(taskCDF));
			AssertEquals("taskCBC should not be assigned", ZString.Empty, assigner.GetAssignedStaffCode(taskCBC));
			AssertEquals("taskCBF should not be assigned", ZString.Empty, assigner.GetAssignedStaffCode(taskCBF));
			AssertEquals("taskCNT should not be assigned", ZString.Empty, assigner.GetAssignedStaffCode(taskCNT));
		}

		void SetupRegistry(bool setupSamRestrictions, bool setupCBCDifRestrictions, bool setupCBFDifRestrictions, bool setupCNTDifRestrictions)
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry(WorkflowManagementMode);

			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType4 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType5 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType6 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CDF";
			workflowTaskType4.Code = "CBC";
			workflowTaskType5.Code = "CBF";
			workflowTaskType6.Code = "CNT";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			//SAME type restriction - INV, CDU, CDF
			if (setupSamRestrictions)
			{
				var restriction = collection.AddNew();
				restriction.Active = true;
				restriction.WorkflowType = "ORG";
				restriction.TaskType = "INV";
				restriction.NotificationType = NotificationTypeList.Codes.Error;
				restriction.Scope = RestrictionScope;
				restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

				var samTaskType1 = restriction.TaskTypesCollection.AddNew();
				var samTaskType2 = restriction.TaskTypesCollection.AddNew();

				samTaskType1.Code = "CDU";
				samTaskType2.Code = "CDF";
			}

			//DIF type restriction - CBC
			if (setupCBCDifRestrictions)
			{
				var restriction = collection.AddNew();
				restriction.Active = true;
				restriction.WorkflowType = "ORG";
				restriction.TaskType = "CBC";
				restriction.NotificationType = NotificationTypeList.Codes.Error;
				restriction.Scope = RestrictionScope;
				restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;

				var difTaskType1 = restriction.TaskTypesCollection.AddNew();
				var difTaskType2 = restriction.TaskTypesCollection.AddNew();
				var difTaskType3 = restriction.TaskTypesCollection.AddNew();

				difTaskType1.Code = "INV";
				difTaskType2.Code = "CDU";
				difTaskType3.Code = "CDF";
			}

			//DIF type restriction - CBF
			if (setupCBFDifRestrictions)
			{
				var restriction = collection.AddNew();
				restriction.Active = true;
				restriction.WorkflowType = "ORG";
				restriction.TaskType = "CBF";
				restriction.NotificationType = NotificationTypeList.Codes.Warning; //to test what happens when it's warning, not error
				restriction.Scope = RestrictionScope;
				restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;

				var difTaskType1 = restriction.TaskTypesCollection.AddNew();
				var difTaskType2 = restriction.TaskTypesCollection.AddNew();
				var difTaskType3 = restriction.TaskTypesCollection.AddNew();

				difTaskType1.Code = "INV";
				difTaskType2.Code = "CDU";
				difTaskType3.Code = "CDF";
			}

			//DIF type restriction - CNT
			if (setupCNTDifRestrictions)
			{
				var restriction = collection.AddNew();
				restriction.Active = true;
				restriction.WorkflowType = "ORG";
				restriction.TaskType = "CNT";
				restriction.NotificationType = NotificationTypeList.Codes.Error;
				restriction.Scope = RestrictionScope;
				restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;

				var difTaskType1 = restriction.TaskTypesCollection.AddNew();

				difTaskType1.Code = "CDF";
			}

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		protected abstract string WorkflowManagementMode { get; }

		protected abstract ZString RestrictionScope { get; }

		ProcessTask taskINV;
		ProcessTask taskCDU;
		ProcessTask taskCDF;
		ProcessTask taskCBC;
		ProcessTask taskCBF;
		ProcessTask taskCNT;

		GlbStaff resource;

		void SetupResourceAndTasks()
		{
			resource = Factory.NewWithValidTestData<GlbStaff>();
			var job = Factory.NewWithValidTestData<OrgHeader>();

			taskINV = job.WorkflowItems.Tasks.AddNew();
			taskINV.P9_Type = "INV";
			taskINV.P9_GS_NKAssignedStaffMember = ZString.Empty;

			taskCDU = job.WorkflowItems.Tasks.AddNew();
			taskCDU.P9_Type = "CDU";
			taskCDU.P9_GS_NKAssignedStaffMember = ZString.Empty;

			taskCDF = job.WorkflowItems.Tasks.AddNew();
			taskCDF.P9_Type = "CDF";
			taskCDF.P9_GS_NKAssignedStaffMember = ZString.Empty;

			taskCBC = job.WorkflowItems.Tasks.AddNew();
			taskCBC.P9_Type = "CBC";
			taskCBC.P9_GS_NKAssignedStaffMember = ZString.Empty;

			taskCBF = job.WorkflowItems.Tasks.AddNew();
			taskCBF.P9_Type = "CBF";
			taskCBF.P9_GS_NKAssignedStaffMember = ZString.Empty;

			taskCNT = job.WorkflowItems.Tasks.AddNew();
			taskCNT.P9_Type = "CNT";
			taskCNT.P9_GS_NKAssignedStaffMember = ZString.Empty;

			if (WorkflowManagementMode != WorkflowManagementModes.Codes.BasicWorkflow)
			{
				var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
				taskINV.P9_FH_ProcessHeader = workflow.PK;
				taskCDU.P9_FH_ProcessHeader = workflow.PK;
				taskCDF.P9_FH_ProcessHeader = workflow.PK;
				taskCBC.P9_FH_ProcessHeader = workflow.PK;
				taskCBF.P9_FH_ProcessHeader = workflow.PK;
				taskCNT.P9_FH_ProcessHeader = workflow.PK;
			}
		}
	}
}
