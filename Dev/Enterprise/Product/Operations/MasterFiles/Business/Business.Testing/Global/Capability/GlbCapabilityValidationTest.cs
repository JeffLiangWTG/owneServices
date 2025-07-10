using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCapabilityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckG4_TaskAssignTaskAgeEmpty()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";
			var validation = new GlbCapabilityValidation(capability);
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_AutoAssignTasksAge = ZDateTime.Empty;
			validation.ValidateAll();
			AssertHasWarning(capability.G4_AutoAssignTasksAgeInfo, "No auto assign task age is set, release group auto assign task age will be applied if available.");
		}

		public void TestCheckG4_TaskAssignTaskAgeZero()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";
			var validation = new GlbCapabilityValidation(capability);
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_AutoAssignTasksAge = new ZDateTime(2015, 1, 1, 0, 0, 0);
			validation.ValidateAll();
			AssertHasWarning(capability.G4_AutoAssignTasksAgeInfo, "000:00 indicates task will be assigned immediately after release.");
		}

		public void TestCheckG4_TaskAssignTaskAgeInvalidFormat()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";
			var validation = new GlbCapabilityValidation(capability);
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_AutoAssignTasksAge = ZDateTime.Invalid;
			validation.ValidateAll();
			AssertHasError(capability.G4_AutoAssignTasksAgeInfo, "Enter a valid Auto Assign Tasks Age. Correct format should be 000:00.");
		}

		public void TestCheckG4_Code_ShouldBeUnique()
		{
			Factory.New<GlbCapability>().G4_Code = "ABC";
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "ABC";
			var validation = new GlbCapabilityValidation(capability);
			validation.ValidateAll();
			AssertHasError(capability.G4_CodeInfo, "The Code has been duplicated and must be unique.");
		}

		public void TestCheckG4_AutoAssignTasksAge()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_AutoAssignTasksAge = new ZDateTime("2013-01-01 00:00:00");
			var validation = new GlbCapabilityValidation(capability);
			validation.ValidateAll();
			AssertNoErrors(capability.G4_CodeInfo);
		}
	}
}
