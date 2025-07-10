using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ProcessWorkflowExceptionCauseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWEC_CodeValidation()
		{
			var type1 = Factory.New<ProcessWorkflowExceptionType>();
			var cause1 = Factory.New<ProcessWorkflowExceptionCause>();
			cause1.WEC_WET_Type = type1.PK;
			var cause2 = Factory.New<ProcessWorkflowExceptionCause>();
			cause2.WEC_WET_Type = type1.PK;

			cause1.WEC_Code = " ";
			AssertHasError(cause1.WEC_CodeInfo, "Please enter a value.");

			cause1.WEC_Code = "CA1";
			cause2.WEC_Code = "CA1";
			AssertHasError(cause2.WEC_CodeInfo, "The Code has been duplicated and must be unique.");

			cause2.WEC_Code = "CA2";
			AssertNoErrors(cause2.WEC_CodeInfo);

			var type2 = Factory.New<ProcessWorkflowExceptionType>();
			var cause3 = Factory.New<ProcessWorkflowExceptionCause>();
			cause3.WEC_WET_Type = type2.PK;
			cause3.WEC_Code = "CA1";
			AssertNoErrors(cause3.WEC_CodeInfo);
		}

		public void TestWEC_DescriptionValidation()
		{
			var cause = Factory.New<ProcessWorkflowExceptionCause>();
			cause.WEC_Description = "   ";
			AssertHasError(cause.WEC_DescriptionInfo, "Please enter a value.");

			cause.WEC_Description = "Lala la";
			AssertNoErrors(cause.WEC_DescriptionInfo);
		}
	}
}
