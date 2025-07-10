using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayoutBuilder<BaseJobDeclaration>))]
	sealed class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, ShipmentTypeControlBag>
	{
		protected override ShipmentTypeLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder<BaseJobDeclaration>();

		protected override int ExpectedMaxColumns => 1;

		public void TestContainerModeDropEditVisible()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.ContainerModeVisible).Returns(false);
			var declaration = declarationMock.Object;
			AssertEquals(false, Layout.IsVisible(ShipmentTypeControlBag.Instance.ContainerModeDropEdit, declaration));
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ShipmentTypeLayouts()).Layout);
		PanelLayout layout;
	}
}
