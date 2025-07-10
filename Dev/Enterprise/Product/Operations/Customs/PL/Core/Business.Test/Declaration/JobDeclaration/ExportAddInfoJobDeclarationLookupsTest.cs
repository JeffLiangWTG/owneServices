using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportAddInfoJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSpecificCircumstanceIndicatorList()
	{
		var lookup = Factory.New<JobDeclaration>().AddInfoLookups;
		var specificCircumstanceIndicatorList = lookup.SpecificCircumstanceIndicatorList;

		CombineAssertions(() =>
		{
			AssertEquals("SpecificCircumstanceIndicatorList codes", "A20", specificCircumstanceIndicatorList.CodesAsString);
			AssertSame("Cached", specificCircumstanceIndicatorList, lookup.SpecificCircumstanceIndicatorList);
		});
	}
}
