using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ProcessWorkflowExceptionTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWET_CodeValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_Code = " ";
			AssertHasError(type.WET_CodeInfo, "Please enter a value.");

			type.WET_Code = "T1";
			AssertNoErrors(type.WET_CodeInfo);
		}

		public void TestWET_CodeValidation_Uniqueness()
		{
			var type1 = Factory.New<ProcessWorkflowExceptionType>();
			type1.WET_JobType = "";
			type1.WET_Code = "T1";

			var type2 = Factory.New<ProcessWorkflowExceptionType>();
			type2.WET_JobType = "WKI";
			type2.WET_Code = "T1";

			AssertHasError(type2.WET_CodeInfo, "Type Code T1 already exists. Specify a different Type Code");
		}

		public void TestWET_DescriptionValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_Description = "    ";
			AssertHasError(type.WET_DescriptionInfo, "Please enter a Description.");

			type.WET_Description = "type 1";
			AssertNoErrors(type.WET_DescriptionInfo);
		}

		public void TestWET_CategoryValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_Category = "XXX";
			AssertHasError(type.WET_CategoryInfo, "Enter a valid selection.");

			type.WET_Category = " ";
			AssertNoErrors(type.WET_CategoryInfo);
		}

		public void TestWET_JobTypeValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_JobType = "XXX";
			AssertHasError(type.WET_JobTypeInfo, "Enter a valid selection.");

			type.WET_JobType = " ";
			AssertNoErrors(type.WET_JobTypeInfo);
		}

		public void TestWET_CauseRequiredValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_IsCauseRequired = true;
			AssertHasError(type.WET_IsCauseRequiredInfo, "Types with Cause Required must specify at least one active Cause.");
		}

		public void TestWET_DefaultDurationHoursValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_DefaultDurationHours = -10;
			AssertHasError(type.WET_DefaultDurationHoursInfo, "value cannot be negative.");
		}

		public void TestWET_ResolutionRequiredValidation()
		{
			var type = Factory.New<ProcessWorkflowExceptionType>();
			type.WET_IsResolutionRequired = true;
			AssertHasError(type.WET_IsResolutionRequiredInfo, "Types with Resolution Required must specify at least one active Resolution.");
		}

		public void TestWEC_IsActiveValidation()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			type.WET_IsCauseRequired = true;

			var cause1 = Factory.New<ProcessWorkflowExceptionCause>();
			cause1.WEC_WET_Type = type.PK;
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = "c1";
			cause1.WEC_IsActive = false;
			type.Causes.Add(cause1);

			var cause2 = Factory.New<ProcessWorkflowExceptionCause>();
			cause2.WEC_WET_Type = type.PK;
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = "c2";
			cause2.WEC_IsActive = false;
			type.Causes.Add(cause2);

			type.Validation.ValidateAll();
			AssertHasRowError(cause1, "Types with Cause Required must specify at least one active Cause.");
			AssertHasRowError(cause2, "Types with Cause Required must specify at least one active Cause.");

			cause1.WEC_IsDefault = true;
			cause2.WEC_IsActive = true;

			type.Validation.ValidateAll();
			AssertHasRowError(cause1, "The default Cause must always be active.");
			AssertNoRowErrors(cause2);

			cause1.WEC_IsActive = true;

			type.Validation.ValidateAll();
			AssertNoRowErrors(cause1);
			AssertNoRowErrors(cause2);
		}

		public void TestWEC_IsDefaultValidation()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();

			var cause1 = Factory.New<ProcessWorkflowExceptionCause>();
			cause1.WEC_WET_Type = type.PK;
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = "c1";
			cause1.WEC_IsDefault = true;
			type.Causes.Add(cause1);

			var cause2 = Factory.New<ProcessWorkflowExceptionCause>();
			cause2.WEC_WET_Type = type.PK;
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = "c2";
			cause2.WEC_IsDefault = true;
			type.Causes.Add(cause2);

			type.Validation.ValidateAll();
			AssertHasRowError(cause1, "There can be no more than one Default Cause for a given Type.");
			AssertHasRowError(cause2, "There can be no more than one Default Cause for a given Type.");

			cause1.WEC_IsDefault = false;
			cause2.WEC_IsDefault = false;

			type.Validation.ValidateAll();
			AssertNoRowErrors(cause1);
			AssertNoRowErrors(cause2);
		}

		public void TestWEC_IsDefaultValidation_SystemType()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			type.WET_IsCauseRequired = true;
			type.WET_IsSystem = true;

			var cause1 = Factory.New<ProcessWorkflowExceptionCause>();
			cause1.WEC_WET_Type = type.PK;
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = "c1";
			type.Causes.Add(cause1);

			var cause2 = Factory.New<ProcessWorkflowExceptionCause>();
			cause2.WEC_WET_Type = type.PK;
			cause2.WEC_Code = "CA2";
			cause2.WEC_Description = "c2";
			type.Causes.Add(cause2);

			type.Validation.ValidateAll();
			AssertHasRowError(cause1, "For System Types with Cause Required a single default Cause must be specified.");
			AssertHasRowError(cause2, "For System Types with Cause Required a single default Cause must be specified.");

			type.WET_IsSystem = false;

			type.Validation.ValidateAll();
			AssertNoRowErrors(cause1);
			AssertNoRowErrors(cause2);
		}

		public void TestWER_IsActiveValidation()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			type.WET_IsResolutionRequired = true;

			var res1 = Factory.New<ProcessWorkflowExceptionResolution>();
			res1.WER_WET_Type = type.PK;
			res1.WER_Code = "RE1";
			res1.WER_Description = "r1";
			res1.WER_IsActive = false;
			type.Resolutions.Add(res1);

			var res2 = Factory.New<ProcessWorkflowExceptionResolution>();
			res2.WER_WET_Type = type.PK;
			res2.WER_Code = "RE2";
			res2.WER_Description = "r2";
			res2.WER_IsActive = false;
			type.Resolutions.Add(res2);

			type.Validation.ValidateAll();
			AssertHasRowError(res1, "Types with Resolution Required must specify at least one active Resolution.");
			AssertHasRowError(res2, "Types with Resolution Required must specify at least one active Resolution.");

			res1.WER_IsDefault = true;
			res2.WER_IsActive = true;

			type.Validation.ValidateAll();
			AssertHasRowError(res1, "The default Resolution must always be active.");
			AssertNoRowErrors(res2);

			res1.WER_IsActive = true;

			type.Validation.ValidateAll();
			AssertNoRowErrors(res1);
			AssertNoRowErrors(res2);
		}

		public void TestWER_IsDefaultValidation()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();

			var res1 = Factory.New<ProcessWorkflowExceptionResolution>();
			res1.WER_WET_Type = type.PK;
			res1.WER_Code = "RE1";
			res1.WER_Description = "r1";
			res1.WER_IsDefault = true;
			type.Resolutions.Add(res1);

			var res2 = Factory.New<ProcessWorkflowExceptionResolution>();
			res2.WER_WET_Type = type.PK;
			res2.WER_Code = "RE2";
			res2.WER_Description = "r2";
			res2.WER_IsDefault = true;
			type.Resolutions.Add(res2);

			type.Validation.ValidateAll();
			AssertHasRowError(res1, "There can be no more than one Default Resolution for a given Type.");
			AssertHasRowError(res2, "There can be no more than one Default Resolution for a given Type.");

			res1.WER_IsDefault = false;
			res2.WER_IsDefault = false;

			type.Validation.ValidateAll();
			AssertNoRowErrors(res1);
			AssertNoRowErrors(res2);
		}

		public void TestWER_IsDefaultValidation_SystemType()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();
			type.WET_IsResolutionRequired = true;
			type.WET_IsSystem = true;

			var res1 = Factory.New<ProcessWorkflowExceptionResolution>();
			res1.WER_WET_Type = type.PK;
			res1.WER_Code = "RE1";
			res1.WER_Description = "r1";
			type.Resolutions.Add(res1);

			var res2 = Factory.New<ProcessWorkflowExceptionResolution>();
			res2.WER_WET_Type = type.PK;
			res2.WER_Code = "RE2";
			res2.WER_Description = "r2";
			type.Resolutions.Add(res2);

			type.Validation.ValidateAll();
			AssertHasRowError(res1, "For System Types with Resolution Required a single default Resolution must be specified.");
			AssertHasRowError(res2, "For System Types with Resolution Required a single default Resolution must be specified.");

			type.WET_IsSystem = false;

			type.Validation.ValidateAll();
			AssertNoRowErrors(res1);
			AssertNoRowErrors(res2);
		}

		public void TestWEC_CannotDeleteIfUsed()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();

			var cause1 = Factory.New<ProcessWorkflowExceptionCause>();
			cause1.WEC_WET_Type = type.PK;
			cause1.WEC_Code = "CA1";
			cause1.WEC_Description = "c1";
			type.Causes.Add(cause1);

			AssertEquals(true, cause1.CanDelete);

			var exc = Factory.New<ProcessWorkflowException>();
			exc.WEX_WEC_Cause = cause1.PK;

			AssertEquals(false, cause1.CanDelete);
		}

		public void TestWER_CannotDeleteIfUsed()
		{
			var type = Factory.NewWithValidTestData<ProcessWorkflowExceptionType>();

			var res1 = Factory.New<ProcessWorkflowExceptionResolution>();
			res1.WER_WET_Type = type.PK;
			res1.WER_Code = "RE1";
			res1.WER_Description = "r1";
			type.Resolutions.Add(res1);

			AssertEquals(true, res1.CanDelete);

			var exc = Factory.New<ProcessWorkflowException>();
			exc.WEX_WER_Resolution = res1.PK;

			AssertEquals(false, res1.CanDelete);
		}
	}
}
