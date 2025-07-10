using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(PreviousReferenceTypes))]
sealed class PreviousReferenceTypesTest : TestCaseWithFactory
{
	public void TestCodeDescriptionsPairs()
	{
		var previousReferenceTypeList = new PreviousReferenceTypes();
		CombineAssertions(() =>
		{
			AssertEquals("Count", 1, previousReferenceTypeList.Count);
			AssertEquals("Code 'MAN'", "Manifest", previousReferenceTypeList.GetDescriptionFromCode("MAN"));
		});
	}
}
