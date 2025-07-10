using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonOrganisationsLayoutBuilder<BaseJobDeclaration>))]
	sealed class CommonOrganisationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommonOrganisationsLayoutBuilder<BaseJobDeclaration>, BaseJobDeclaration, CommonOrganisationsControlBag>
	{
		public void TestBondedWarehouseDocAddressControlVisibility()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.BondedWarehouseEditable).Returns(true);

			var layout = ((IPanelLayoutProvider)new CommonOrganisationsLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("Bonded Warehouse Editable", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, declarationMock.Object));
				declarationMock.Setup(m => m.BondedWarehouseEditable).Returns(false);
				AssertEquals("Bonded Warehouse ReadOnly", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, declarationMock.Object));
			});
		}

		protected override int ExpectedMaxColumns => 1;

		protected override CommonOrganisationsLayoutBuilder<BaseJobDeclaration> GetColumnLayoutBuilderForTesting() => new CommonOrganisationsLayoutBuilder<BaseJobDeclaration>();

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
