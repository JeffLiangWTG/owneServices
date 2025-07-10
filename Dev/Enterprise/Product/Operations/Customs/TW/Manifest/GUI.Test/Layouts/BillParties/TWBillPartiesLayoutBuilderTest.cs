using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.GUI.Testing
{
	[TestedType(typeof(TWBillPartiesLayoutBuilder<AsycudaBill>))]
	sealed class TWBillPartiesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TWBillPartiesLayoutBuilder<AsycudaBill>, AsycudaBill, TWBillPartiesControlBag>
	{
		protected override TWBillPartiesLayoutBuilder<AsycudaBill> GetColumnLayoutBuilderForTesting() => new TWBillPartiesLayoutBuilder<AsycudaBill>();

		protected override int ExpectedMaxColumns => 3;
	}
}
