using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ExportDeclarationSubTypes))]
sealed class ExportDeclarationSubTypesTest : TestCaseWithFactory
{
	public void TestAllCodesAndDescriptions() => CombineAssertions(() =>
		AssertCodeDescriptionPairList(new ExportDeclarationSubTypes(),
			("N", "Normal declaration"),
			("P", "Preliminary declaration")));
}
