using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ImportDeclarationSubTypes))]
sealed class ImportDeclarationSubTypesTest : TestCaseWithFactory
{
	public void TestAllCodesAndDescriptions() => CombineAssertions(() =>
		AssertCodeDescriptionPairList(new ImportDeclarationSubTypes(),
			("N", "Normal declaration"),
			("P", "Preliminary declaration"),
			("C", "Collective simplified clearance")));
}
