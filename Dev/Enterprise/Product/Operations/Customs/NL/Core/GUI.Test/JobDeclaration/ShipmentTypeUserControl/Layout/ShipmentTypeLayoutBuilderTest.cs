using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayoutBuilder))]
sealed class ShipmentTypeLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ShipmentTypeLayoutBuilder, JobDeclaration, Customs.GUI.ShipmentTypeControlBag>
{
	protected override ShipmentTypeLayoutBuilder GetColumnLayoutBuilderForTesting() => new ShipmentTypeLayoutBuilder();

	protected override int ExpectedMaxColumns => 1;

	public void TestSpecificCircumstanceDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			var controlReference = ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = "B1";
			AssertControlVisibility(controlReference, true, "EXP B1");
			entryInstruction.CEI_Style = "B2";
			AssertControlVisibility(controlReference, true, "EXP B2");
			entryInstruction.CEI_Style = "B3";
			AssertControlVisibility(controlReference, false, "EXP B3");
			entryInstruction.CEI_Style = "B4";
			AssertControlVisibility(controlReference, false, "EXP B4");
			entryInstruction.CEI_Style = "C1";
			AssertControlVisibility(controlReference, true, "EXP C1");
			entryInstruction.CEI_Style = "C2";
			AssertControlVisibility(controlReference, true, "EXP C2");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertControlVisibility(controlReference, false, "IMP");
		});
	}

	public void TestBorderTransportMeansDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			var borderTransportMeansDropEdit = ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertControlVisibility(borderTransportMeansDropEdit, true, "EXP");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertControlVisibility(borderTransportMeansDropEdit, false, "IMP");

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertControlVisibility(borderTransportMeansDropEdit, true, "MSC");
		});
	}

	public void TestSecurityDropEditVisibility()
	{
		CombineAssertions(() =>
		{
			var securityDropEdit = ShipmentTypeControlBag.Instance.SecurityDropEdit;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertControlVisibility(securityDropEdit, true, "EXP");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertControlVisibility(securityDropEdit, false, "IMP");

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertControlVisibility(securityDropEdit, false, "MSC");
		});
	}

	void AssertControlVisibility(ControlReference controlReference, bool isVisible, string reference)
	{
		AssertEquals($"{controlReference.Name} Visible, for " + reference, isVisible, Layout.IsVisible(controlReference, declaration));
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
