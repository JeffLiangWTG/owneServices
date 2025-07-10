using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeLayoutBuilder))]
	class ETradeLayoutBuilderTest : ASYCUDA.GUI.Testing.ManifestLayoutBuilderAbstractTest<ETradeLayoutBuilder, Business.AsycudaManifestHeader>
	{
		protected override int ExpectedMaxColumns => 3;

		protected override ETradeLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			var builder = new ETradeLayoutBuilder();
			builder.AddControlBag(ETradeControlBag.Instance);
			return builder;
		}
	}
}
