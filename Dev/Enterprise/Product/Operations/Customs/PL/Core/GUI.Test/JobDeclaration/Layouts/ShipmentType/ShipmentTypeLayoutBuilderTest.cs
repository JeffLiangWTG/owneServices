using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayoutBuilder))]
sealed class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder, JobDeclaration, Customs.GUI.ShipmentTypeControlBag>
{
	protected override ShipmentTypeLayoutBuilder GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder();

	protected override int ExpectedMaxColumns => 1;

	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			TestVisibilityImportExport(true, true, ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit);
		});
	}

	void TestVisibilityImportExport(bool visibileForImport, bool visibleForExport, ControlReference controlReference)
	{
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals($"{controlReference.Name} Visible, for " + Common.Shared.SharedJobMessageTypeList.Codes.Export, visibleForExport, Layout.IsVisible(controlReference, declaration));

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		AssertEquals($"{controlReference.Name} Visible, for " + Common.Shared.SharedJobMessageTypeList.Codes.Import, visibileForImport, Layout.IsVisible(controlReference, declaration));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;

	public PanelLayout Layout => layout ?? (layout = new ShipmentTypeLayout().Layout);
	PanelLayout layout;
}
