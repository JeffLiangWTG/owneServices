using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaskLineTriggerWorkflowDescriptor))]
	class TaskLineTriggerWorkflowDescriptorTest : WorkflowDescriptorTestCase<TaskLineTriggerWorkflowDescriptor>
	{
		#region Overrides

		public override void TestDescription() => AssertEquals("Task Line Trigger", WorkflowDescriptor.Description);

		public override void TestID() => AssertEquals("TSK", WorkflowDescriptor.Code);

		public override void TestRequiresBranch() => AssertEquals(false, WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresClient() => AssertEquals(false, WorkflowDescriptor.RequiresClient);

		public override void TestRequiresDepartment() => AssertEquals(false, WorkflowDescriptor.RequiresDepartment);

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes() => AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new[] { Factory.NewWithValidTestData<DummyWithWorkflow>() };

		protected override void SetNotificationEmailAddress(BusinessObject job, ProcessTaskNotification notification, string emailAddress) => ((ProcessTask)job).AssignedStaffMember.GS_EmailAddress = emailAddress;

		protected override BusinessObject SetupLineTriggerIfRequired(IWorkflowProvider parent, ProcessTask trigger)
		{
			base.SetupLineTriggerIfRequired(parent, trigger);

			var staff = MasterFilesTestHelper.CreateStaff(Factory, "chunkylover53@aol.com");
			var capability = MasterFilesTestHelper.CreateCapability(Factory, staff);
			var group = MasterFilesTestHelper.CreateGroup(Factory, staff);

			var task = MasterFilesTestHelper.CreateTask(parent, staff.GS_Code, requiredCapability: capability, assignedGroup: group);

			trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
			return task;
		}

		#endregion

		#region Supports

		public void TestNoXUCSupport() => AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalEventCollectionXML);

		public void TestSupportsXUE() => AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalEventXML);

		public void TestNoSupportTemplateApplication() => AssertEquals(false, WorkflowDescriptor.SupportsApplyWorkflowTemplate);

		public override void TestSupportsTasks() => AssertEquals(false, WorkflowDescriptor.SupportsTasks);

		public override void TestSupportsWorkflowTemplates() => AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);

		public override void TestSupportsAssignStaffAndEmail()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsAssignStaffAndEmail(null, null));

			var processTask = Factory.New<ProcessTaskTemplate>();
			processTask.P0_ProcessType = WorkflowDescriptors.GlbStaffHolidayDescriptorCode;
			AssertEquals(true, WorkflowDescriptor.SupportsAssignStaffAndEmail(null, processTask));

			processTask.P0_ProcessType = WorkflowDescriptors.GlbStaffDescriptorCode;
			AssertEquals(false, WorkflowDescriptor.SupportsAssignStaffAndEmail(null, processTask));
		}

		public void TestParentSupportsWorkflowTriggerActionUniversalShipmentXML()
		{
			var parent = Factory.New<IForwardingShipment>() as IWorkflowProvider;
			var parentDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(parent.WorkflowType);
			var actionTypes = parentDescriptor.GetWorkflowTriggerActionTypes();

			var result = WorkflowDescriptor.ParentSupportsWorkflowTriggerActionUniversalShipmentXML(parent as IBusiness);

			AssertEquals(true, result);
			AssertEquals(true, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
		}

		public void TestParentSupportsWorkflowTriggerActionUniversalShipmentXMLForProcessTaskTemplate()
		{
			var shipmentTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			shipmentTemplate.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var result = WorkflowDescriptor.ParentSupportsWorkflowTriggerActionUniversalShipmentXML(shipmentTemplate);

			AssertEquals(true, result);
		}

		#endregion

		#region Type

		public override void TestWorkflowProviderType() => AssertEquals(typeof(ProcessTask), WorkflowDescriptor.WorkflowProviderType);

		#endregion
	}
}
