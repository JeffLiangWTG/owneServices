using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ShipmentDetailsLayoutBuilder<BaseJobDeclaration>))]
	sealed class ShipmentDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentDetailsLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, ShipmentDetailsControlBag>
	{
		protected override int ExpectedMaxColumns => 1;

		protected override ShipmentDetailsLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting() => new ShipmentDetailsLayoutBuilder<BaseJobDeclaration>();
	}
}
