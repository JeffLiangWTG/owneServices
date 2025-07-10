using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonHeaderDetailsLayoutBuilder<CusReconDeclaration>))]
	sealed class CommonHeaderDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommonHeaderDetailsLayoutBuilder<CusReconDeclaration>, CusReconDeclaration, CommonHeaderDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override CommonHeaderDetailsLayoutBuilder<CusReconDeclaration> GetColumnLayoutBuilderForTesting() => new CommonHeaderDetailsLayoutBuilder<CusReconDeclaration>();
	}
}
