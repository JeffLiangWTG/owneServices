using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonDeclarationDetailsLayoutBuilder<BaseJobDeclaration>))]
	sealed class CommonDeclarationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommonDeclarationDetailsLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, CommonDeclarationDetailsControlBag>
	{
		protected override CommonDeclarationDetailsLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting()
		{
			return new CommonDeclarationDetailsLayoutBuilder<BaseJobDeclaration>();
		}

		protected override int ExpectedMaxColumns => 1;
	}
}
