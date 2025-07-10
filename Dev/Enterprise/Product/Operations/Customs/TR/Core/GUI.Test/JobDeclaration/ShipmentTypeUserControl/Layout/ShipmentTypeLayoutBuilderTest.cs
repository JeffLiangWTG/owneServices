using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayoutBuilder<JobDeclaration>))]
	class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder<JobDeclaration>, JobDeclaration, Customs.GUI.ShipmentTypeControlBag>
	{
		protected override ShipmentTypeLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		protected override int ExpectedMaxColumns => 1;

		public void TestSpecificCircumstanceDropEditVisibility()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("SpecificCircumstanceDropEdit Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Export, true, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("SpecificCircumstanceDropEdit Visible, if messagetype " + Common.Shared.SharedJobMessageTypeList.Codes.Import, false, Layout.IsVisible(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, declaration));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;

		public PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayouts().Layout);
		PanelLayout layout;
	}
}
