using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ImportJobDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestCountryList()
		{
			ImportJobDeclaration importDec = new ImportJobDeclaration(Factory);
			AssertNotNull("Country List", importDec.Lookups.CountryList);
		}

		public void TestDeclarationList()
		{
			ImportJobDeclaration importDec = new ImportJobDeclaration(Factory);
			AssertNotNull("Declaration List", importDec.Lookups.DeclarationList);
		}
	}
}
