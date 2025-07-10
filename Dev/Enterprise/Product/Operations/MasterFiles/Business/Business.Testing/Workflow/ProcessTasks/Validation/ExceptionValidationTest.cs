using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ExceptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestP9_GS_NKAssignedStaffMemberIsValidated()
		{
			var exception = Factory.New<ProcessTask>();
			exception.IsException = true;
			AssertEquals("Should use exception validation", typeof(ExceptionValidation), exception.Validation.GetType());

			exception.P9_GS_NKAssignedStaffMember = "XYZ";
			exception.P9_Description = "Test";
			exception.Validation.ValidateAll();

			AssertHasErrorContaining("Should have validation error with invalid staff code", exception.P9_GS_NKAssignedStaffMemberInfo, "Enter a valid Exception Assigned To");

			exception.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			exception.Validation.ValidateAll();

			AssertNoErrors("Should not have validation error with valid staff code", exception.P9_GS_NKAssignedStaffMemberInfo);
		}

		public void TestP9_Description_WithMilestone_Changed()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Departed at port";
			milestone.TriggerConditions.TriggerEventCode = "DEP";
			milestone.P9_SE_NKExceptionEvent = "EXC";

			var exception = dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "Departed at port";
			exception.TriggerConditions.TriggerEventCode = "DEP";
			exception.P9_SE_NKExceptionEvent = "EXC";
			Factory.Save();

			exception.P9_Description = "New Value";
			AssertHasWarningContaining(
				"Has warning message",
				exception.P9_DescriptionInfo,
				"Modifying the description will break the link between the Milestone and the Exception. This may prevent the Exception from being actioned automatically and could lead to incorrect information appearing in Exception reports.");

			exception.P9_Description = "Departed at port";
			AssertNoWarnings("No warning exists", exception.P9_DescriptionInfo);
		}

		public void TestP9_Description_WithNoMilestone_Changed()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Departed at port";
			milestone.TriggerConditions.TriggerEventCode = "DEP";
			milestone.P9_SE_NKExceptionEvent = "EXC";

			var exception = dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "Random description";
			exception.TriggerConditions.TriggerEventCode = "TST";
			exception.P9_SE_NKExceptionEvent = "EXC";
			Factory.Save();

			exception.P9_Description = "New Value";
			AssertNoWarnings("No warning exists", exception.P9_DescriptionInfo);

			exception.P9_Description = "Random description";
			AssertNoWarnings("No warning exists", exception.P9_DescriptionInfo);
		}
	}
}
