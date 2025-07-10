using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusDV1DetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDV1_ContractNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var dv1Detail1 = declaration.DV1Details.AddNew();

			CombineAssertions(() =>
			{
				dv1Detail1.DV1_ContractNumber = "11111";
				dv1Detail1.Validation.ValidateAll();
				AssertNoMessageErrorContaining(dv1Detail1.DV1_ContractNumberInfo, "Only one D.V.1 Details is allowed.");

				var dv1Detail2 = declaration.DV1Details.AddNew();
				dv1Detail2.DV1_ContractNumber = "22222";
				AssertHasMessageErrorContaining(dv1Detail2.DV1_ContractNumberInfo, "Only one D.V.1 Details is allowed.");
			});
		}
	}
}
