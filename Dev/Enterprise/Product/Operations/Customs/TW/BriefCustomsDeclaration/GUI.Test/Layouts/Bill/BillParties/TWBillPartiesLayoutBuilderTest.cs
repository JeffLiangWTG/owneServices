using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillPartiesLayoutBuilder<AsycudaBill>))]
	sealed class TWBillPartiesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TWBillPartiesLayoutBuilder<AsycudaBill>, AsycudaBill, TWBillPartiesControlBag>
	{
		protected override TWBillPartiesLayoutBuilder<AsycudaBill> GetColumnLayoutBuilderForTesting() => new TWBillPartiesLayoutBuilder<AsycudaBill>();

		protected override int ExpectedMaxColumns => 2;
	}
}
