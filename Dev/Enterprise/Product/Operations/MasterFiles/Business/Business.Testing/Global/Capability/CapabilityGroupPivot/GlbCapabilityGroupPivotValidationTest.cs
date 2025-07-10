using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCapabilityGroupPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCorrectlyEnteredReleaseGroupPivot_WhenAutoAssignmentIsEnabled()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot.GGC_AllowTaskAutoAssignment = true;
			pivot.GGC_GG_Group = group.PK;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertNoErrors(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		public void TestCheckCorrectlyEnteredReleaseGroupPivot_WhenAutoAssignmentIsDisabled()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot.GGC_AllowTaskAutoAssignment = false;
			pivot.GGC_GG_Group = group.PK;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertNoErrors(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		[TestDate(2021, 8, 4, 8, 45, 0)]
		public void TestCheckNoValidationError_WhenGroupIsNotEnteredAndAutoAssignTasksAgeIsDefault_AndAutoAssignmentIsDisabled()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AllowTaskAutoAssignment = false;

			AssertEquals("Prerequisite - default task age", ZDateTime.DefaultDurationEpoch, pivot.GGC_AutoAssignTasksAge);

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertNoErrors(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		[TestDate(2021, 8, 4, 8, 45, 0)]
		public void TestCheckNoValidationError_WhenAutoAssignTasksAgeIsDefault_AndAutoAssignmentIsDisabled()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AllowTaskAutoAssignment = false;
			pivot.GGC_GG_Group = group.PK;

			AssertEquals("Prerequisite - default task age", ZDateTime.DefaultDurationEpoch, pivot.GGC_AutoAssignTasksAge);

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertNoErrors(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		public void TestCheckWarning_WhenAutoAssignTaskAgeIsZero()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot.GGC_AllowTaskAutoAssignment = true;
			pivot.GGC_GG_Group = group.PK;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertHasWarning(pivot.GGC_AutoAssignTasksAgeInfo, "000:00 indicates task will be assigned immediately after release.");
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		public void TestCheckNoWarning_WhenAllowAutoAssignTasksIsFalse()
		{
			// Arrange
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_GG_Group = group.PK;
			pivot.GGC_AllowTaskAutoAssignment = true;
			pivot.GGC_AutoAssignTasksAge = ZDateTime.Invalid;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertHasError(pivot.GGC_AutoAssignTasksAgeInfo, "Enter a valid Auto Assign Tasks Age. Correct format should be 000:00.");

			// Act
			pivot.GGC_AllowTaskAutoAssignment = false;
			validation.ValidateAll();

			// Assert
			AssertNoWarnings(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		public void TestCheckValidationError_WhenAutoAssignTasksAgeIsEmpty()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AllowTaskAutoAssignment = true;
			pivot.GGC_GG_Group = group.PK;
			pivot.GGC_AutoAssignTasksAge = ZDateTime.Empty;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertHasError(pivot.GGC_AutoAssignTasksAgeInfo, "Please enter an Auto Assign Tasks Age.");
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		public void TestCheckValidationError_WhenAutoAssignTasksAgeIsInvalid()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = ZDateTime.Invalid;
			pivot.GGC_AllowTaskAutoAssignment = true;
			pivot.GGC_GG_Group = group.PK;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertHasError(pivot.GGC_AutoAssignTasksAgeInfo, "Enter a valid Auto Assign Tasks Age. Correct format should be 000:00.");
			AssertNoErrors(pivot.GGC_GG_GroupInfo);
		}

		public void TestCheckValidationError_WhenGroupIsEmpty()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot.GGC_AllowTaskAutoAssignment = true;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertNoErrors(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertHasError(pivot.GGC_GG_GroupInfo, "Please enter a Group Code.");
		}

		public void TestCheckValidationError_WhenGroupIsInvalid()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot.GGC_GG_Group = ZGuid.Invalid;
			pivot.GGC_AllowTaskAutoAssignment = true;

			var validation = new GlbCapabilityGroupPivotValidation(pivot);
			validation.ValidateAll();
			AssertNoErrors(pivot.GGC_AutoAssignTasksAgeInfo);
			AssertHasError(pivot.GGC_GG_GroupInfo, "Enter a valid Group.");
		}

		public void TestCheckValidationError_WhenGroupIsUsedMoreThanOnce()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot1 = capability.ReleaseGroupPivots.AddNew();
			pivot1.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot1.GGC_AllowTaskAutoAssignment = true;
			pivot1.GGC_GG_Group = group.PK;

			var pivot2 = capability.ReleaseGroupPivots.AddNew();
			pivot2.GGC_AutoAssignTasksAge = new ZDateTime(ZDateTime.Today.Year - 3, 1, 1);
			pivot2.GGC_AllowTaskAutoAssignment = true;
			pivot2.GGC_GG_Group = group.PK;

			var validation1 = new GlbCapabilityGroupPivotValidation(pivot1);
			validation1.ValidateAll();
			AssertNoErrors(pivot1.GGC_AutoAssignTasksAgeInfo);
			AssertHasError(pivot1.GGC_GG_GroupInfo, "The Group Code has been duplicated and must be unique.");

			var validation2 = new GlbCapabilityGroupPivotValidation(pivot1);
			validation2.ValidateAll();
			AssertNoErrors(pivot2.GGC_AutoAssignTasksAgeInfo);
			AssertHasError(pivot2.GGC_GG_GroupInfo, "The Group Code has been duplicated and must be unique.");
		}
	}
}
