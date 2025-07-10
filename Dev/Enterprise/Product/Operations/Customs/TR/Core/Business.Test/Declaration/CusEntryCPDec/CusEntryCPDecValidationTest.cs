using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusEntryCPDecValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckON_QuestionType()
		{
			CPDec.ON_QuestionType = "";
			CPDec.Validation.ValidateON_QuestionType();
			AssertNoMessageErrors(CPDec.ON_QuestionTypeInfo);

			CPDec.ON_AnswerCode = "YES";
			CPDec.Validation.ValidateON_QuestionType();
			AssertHasMessageErrorContaining(CPDec.ON_QuestionTypeInfo, MandatoryValidation.YouHaveNotEntered);

			CPDec.ON_QuestionType = "Q";
			CPDec.Validation.ValidateON_QuestionType();
			AssertNoMessageErrors(CPDec.ON_QuestionTypeInfo);

			ValidationTestHelper.AssertInvalidCodeMessageError(CPDec.ON_QuestionTypeInfo, "XXX", "W");
		}

		public void TestCheckON_CPDecNum()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(CPDec.ON_CPDecNumInfo);
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(CPDec.ON_CPDecNumInfo);
		}

		public void TestCheckON_AnswerCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(CPDec.ON_AnswerCodeInfo, "XXX", "Y");

			CPDec.ON_AnswerCode = "";
			CPDec.Validation.ValidateON_AnswerCode();
			AssertNoWarnings(CPDec.ON_AnswerCodeInfo);

			CPDec.ON_QuestionType = "Q";
			CPDec.Validation.ValidateON_AnswerCode();
			AssertHasWarningContaining(CPDec.ON_AnswerCodeInfo, MandatoryValidation.YouHaveNotEntered);

			CPDec.ON_AnswerCode = "NO";
			CPDec.Validation.ValidateON_AnswerCode();
			AssertNoWarnings(CPDec.ON_AnswerCodeInfo);
		}

		CusEntryCPDec CPDec => cpDec ?? (cpDec = Factory.New<CusEntryCPDec>());
		CusEntryCPDec cpDec;
	}
}
