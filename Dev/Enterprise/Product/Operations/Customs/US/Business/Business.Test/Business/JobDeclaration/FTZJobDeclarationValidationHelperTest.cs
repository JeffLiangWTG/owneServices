using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FTZJobDeclarationValidationHelperTest : TestCaseWithFactory
	{
		public void TestHelper()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			AssertEquals(!declaration.IsODZ_AdmissionType, FTZJobDeclarationValidationHelper.IsNotODZAdmissionType(declaration));
			AssertEquals(declaration.IsFTZAdmissionValidationMode, FTZJobDeclarationValidationHelper.IsFTZAdmissionValidationMode(declaration));
			AssertEquals(declaration.IsFTZPTTValidationMode, FTZJobDeclarationValidationHelper.IsFTZPTTValidationMode(declaration));
			AssertEquals(declaration.IsFTZPTTValidationMode ? ValidationConstants.FTZ.DataRequired : ValidationConstants.FTZ.DataRequiredWhenPTTIncluded, FTZJobDeclarationValidationHelper.GetErrorMessage(declaration));
		}
	}
}
