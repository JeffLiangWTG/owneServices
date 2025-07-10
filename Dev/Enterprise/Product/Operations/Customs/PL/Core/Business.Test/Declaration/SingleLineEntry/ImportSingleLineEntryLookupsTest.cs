using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportSingleLineEntryLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCountryOfOrigins()
	{
		var declaration = Factory.New<JobDeclaration>();
		var countryOfOriginsList = new ImportSingleLineEntry(declaration).Lookups.CountryOfOrigins;
		AssertNotEquals(0, countryOfOriginsList.Count);
	}

	public void TestCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var singleLineEntry = new ImportSingleLineEntry(declaration);
		AssertType<ImportPreviousDocumentCodeList>(singleLineEntry.Lookups.PreviousDocumentCodes);
	}
}
