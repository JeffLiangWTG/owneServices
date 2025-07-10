using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DeclarationDuplicateValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			string messageError = MandatoryValidation.YouHaveNotEntered + " a Type.";
			declarationDuplicate.Validation.ValidateCY_Code();
			AssertHasMessageError(declarationDuplicate.CY_CodeInfo, messageError);
			declarationDuplicate.CY_Code = "1";
			AssertHasMessageErrorContaining(declarationDuplicate.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			declarationDuplicate.CY_Code = "2";
			AssertNoMessageError(declarationDuplicate.CY_CodeInfo, messageError);
			var declarationDuplicate2 = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate2.CY_Code = "2";
			AssertHasMessageErrorContaining(declarationDuplicate2.CY_CodeInfo, ValidationConstants.EntryInstruction.DeclarationDuplicateDuplicated);
		}

		public void TestCheckCY_Data()
		{
			string errorMessage = "This number is invalid.";
			declarationDuplicate.Copy = -1;
			AssertHasError(declarationDuplicate.CopyInfo, errorMessage);
			declarationDuplicate.Copy = 1;
			AssertNoError(declarationDuplicate.CopyInfo, errorMessage);
			declarationDuplicate.Copy = 0;
			AssertHasError(declarationDuplicate.CopyInfo, errorMessage);
			declarationDuplicate.Copy = 99;
			AssertNoError(declarationDuplicate.CopyInfo, errorMessage);
			declarationDuplicate.Copy = 100;
			AssertHasError(declarationDuplicate.CopyInfo, errorMessage);
		}

		protected override void SetUp()
		{
			entryInstruction = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew();
			declarationDuplicate = entryInstruction.DeclarationDuplicates.AddNew();
			base.SetUp();
		}

		DeclarationDuplicate declarationDuplicate;
		CusEntryInstruction entryInstruction;
	}
}
