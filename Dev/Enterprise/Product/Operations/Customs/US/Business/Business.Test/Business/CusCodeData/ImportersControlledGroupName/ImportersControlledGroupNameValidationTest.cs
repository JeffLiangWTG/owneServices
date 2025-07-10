using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportersControlledGroupNameValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			var groupName = Factory.New<ImportersControlledGroupName>();
			groupName.Validation.ValidateAll();
			AssertNoNotifications(groupName.CY_CodeInfo);
		}
	}
}
