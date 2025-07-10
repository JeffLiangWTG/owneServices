using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class OfficeCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOfficeCodeInvalid()
	{
		var message = "The code you have selected is not in the list.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var office = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation);

		CombineAssertions(() =>
		{
			AssertNoMessageError("Valid", office.CY_CodeInfo, message);
			var office2 = declaration.CustomsOffices.AddNew("ABC");
			AssertHasMessageError("Invalid", office2.CY_CodeInfo, message);
		});
	}

	public void TestCheckOnlyOneOfficeOfThisTypeExists()
	{
		var message = "Only one office of type PRE is allowed";
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			var office = declaration.CustomsOffices.AddNew("PRE");
			AssertNoError("Valid", office.CY_CodeInfo, message);

			var office2 = declaration.CustomsOffices.AddNew("PRE");
			office.Validation.ValidateAll();
			AssertHasError("PRE 1", office.CY_CodeInfo, message);
			AssertHasError("PRE 2", office2.CY_CodeInfo, message);
		});
	}
}
