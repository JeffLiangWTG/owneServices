using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class ConsolidatedDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheck_CRD_JE_LeadDeclaration()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration.Validation.ValidateCRD_JE_LeadDeclaration();
				AssertHasRowError(consolidatedDeclaration, "At least one declaration must be selected.");
				consolidatedDeclaration.ClearRowNotifications();
				consolidatedDeclaration.CRD_JE_LeadDeclaration = CargoWise.Types.ZGuid.BrettsGuid;
				AssertNoRowError(consolidatedDeclaration, "At least one declaration must be selected.");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = Factory.NewWithValidTestData<ConsolidatedDeclaration>();
		}
		ConsolidatedDeclaration consolidatedDeclaration;
	}
}
