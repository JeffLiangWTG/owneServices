using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControlLayoutBuilder))]
sealed class EntryInstructionDetailsBasicUserControlLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionDetailsBasicUserControlLayoutBuilder, CusEntryInstruction, Customs.GUI.EntryInstructionBasicDetailsControlBag>
{
	protected override EntryInstructionDetailsBasicUserControlLayoutBuilder GetColumnLayoutBuilderForTesting() => new EntryInstructionDetailsBasicUserControlLayoutBuilder();

	public void TestEntryInstructionDetailsBasicUserControl_Visibility()
	{
		var cusEntryInstruction = Factory.New<CusEntryInstruction>();
		CombineAssertions(() =>
		{
			cusEntryInstruction.CEI_Style = DeclarationTypeList.Codes.H1;
			AssertEquals(true, Layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.IsHighValueOvrdCheckBox, cusEntryInstruction));
			cusEntryInstruction.CEI_Style = DeclarationTypeList.Codes.B1;
			AssertEquals(false, Layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.IsHighValueOvrdCheckBox, cusEntryInstruction));
			cusEntryInstruction.CEI_Style = DeclarationTypeList.Codes.H4;
			AssertEquals(true, Layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.IsHighValueOvrdCheckBox, cusEntryInstruction));
			cusEntryInstruction.CEI_Style = DeclarationTypeList.Codes.H5;
			AssertEquals(true, Layout.IsVisible(EntryInstructionBasicDetailsControlBag.Instance.IsHighValueOvrdCheckBox, cusEntryInstruction));
		});
	}

	public void TestLocationOfGoodsUserControl_Caption() => CombineAssertions(() =>
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		Layout.TryGetCaptionData(Customs.EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, entryInstruction, out var captionData);
		AssertEquals("GoodLocationDescription Caption", "Location of Goods", captionData["LocationOfGoodsUserControl"].Caption);
		AssertEquals("GoodLocationDescription FullDescription", "[UCC 5/23] Location of Goods", captionData["LocationOfGoodsUserControl"].FullDescription);
	});

	protected override int ExpectedMaxColumns => 3;

	PanelLayout Layout => layout ?? (layout = new EntryInstructionDetailsBasicUserControlLayout().Layout);
	PanelLayout layout;
}
