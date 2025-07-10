using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayoutBuilder<BaseJobDeclaration>))]
	sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, TransportDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override TransportDetailsLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting() => new TransportDetailsLayoutBuilder<BaseJobDeclaration>();
	}
}
