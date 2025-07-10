using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGAttributeZZValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLanguageIsCorrect()
		{
			var attribute = Factory.NewWithValidTestData<UNDGAttributeZZ>();
			attribute.DAZ_Language = "Easdas";
			AssertHasErrors(attribute.DAZ_LanguageInfo);

			attribute.DAZ_Language = "EN";
			AssertNoErrors(attribute.DAZ_LanguageInfo);
		}
	}
}
