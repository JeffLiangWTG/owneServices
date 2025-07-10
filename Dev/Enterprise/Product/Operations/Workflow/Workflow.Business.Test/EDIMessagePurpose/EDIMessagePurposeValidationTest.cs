using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class EDIMessagePurposeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEMP_Code_NoDuplicates_CaseInsensitive()
		{
			var purpose1 = Factory.New<EDIMessagePurpose>();
			var purpose2 = Factory.New<EDIMessagePurpose>();
			purpose1.EMP_Code = "AAA";
			purpose2.EMP_Code = "aaa";
			AssertHasError(purpose2.EMP_CodeInfo, EDIMessagePurposeValidation.GetNonUniqueCodeError(purpose2.EMP_CodeInfo));
		}

		public void TestEMP_Code_NoEmpty()
		{
			var purpose = Factory.New<EDIMessagePurpose>();
			purpose.EMP_Code = "";
			AssertHasError(purpose.EMP_CodeInfo, MandatoryValidation.MustBeEnteredMessage(purpose.EMP_CodeInfo.HumanReadableName));
		}

		public void TestEMP_Description_NoEmpty()
		{
			var purpose = Factory.New<EDIMessagePurpose>();
			purpose.EMP_Description = "";
			AssertHasError(purpose.EMP_DescriptionInfo, MandatoryValidation.MustBeEnteredMessage(purpose.EMP_DescriptionInfo.HumanReadableName));
		}

		public void TestEMP_Description_AllowDuplicates()
		{
			var purpose1 = Factory.New<EDIMessagePurpose>();
			var purpose2 = Factory.New<EDIMessagePurpose>();
			purpose1.EMP_Code = "AAA";
			purpose2.EMP_Code = "bbb";

			purpose1.EMP_Description = "Dupe";
			purpose2.EMP_Description = "Dupe";

			AssertNoExceptionThrown(() => Factory.Save());
		}
	}
}
