using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ProcessWorkflowExceptionResolutionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWER_CodeValidation()
		{
			var type1 = Factory.New<ProcessWorkflowExceptionType>();
			var res1 = Factory.New<ProcessWorkflowExceptionResolution>();
			res1.WER_WET_Type = type1.PK;
			var res2 = Factory.New<ProcessWorkflowExceptionResolution>();
			res2.WER_WET_Type = type1.PK;

			res1.WER_Code = " ";
			AssertHasError(res1.WER_CodeInfo, "Please enter a value.");

			res1.WER_Code = "RE1";
			res2.WER_Code = "RE1";
			AssertHasError(res2.WER_CodeInfo, "The Code has been duplicated and must be unique.");

			res2.WER_Code = "RE2";
			AssertNoErrors(res2.WER_CodeInfo);

			var type2 = Factory.New<ProcessWorkflowExceptionType>();
			var res3 = Factory.New<ProcessWorkflowExceptionResolution>();
			res3.WER_WET_Type = type2.PK;
			res3.WER_Code = "RE1";
			AssertNoErrors(res3.WER_CodeInfo);
		}

		public void TestWEC_DescriptionValidation()
		{
			var res = Factory.New<ProcessWorkflowExceptionResolution>();
			res.WER_Description = "   ";
			AssertHasError(res.WER_DescriptionInfo, "Please enter a value.");

			res.WER_Description = "Lala la";
			AssertNoErrors(res.WER_DescriptionInfo);
		}
	}
}
