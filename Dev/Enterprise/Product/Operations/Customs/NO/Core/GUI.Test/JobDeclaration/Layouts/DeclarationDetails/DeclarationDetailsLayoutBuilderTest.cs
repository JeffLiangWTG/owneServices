using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(DeclarationDetailsLayoutBuilder))]
sealed class DeclarationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DeclarationDetailsLayoutBuilder, JobDeclaration, CommonDeclarationDetailsControlBag>
{
	protected override DeclarationDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		return new DeclarationDetailsLayoutBuilder();
	}

	protected override int ExpectedMaxColumns => 2;
}
