using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControl))]
sealed class EntryInstructionDetailsBasicUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new EntryInstructionDetailsBasicUserControl();
		AssertEquals("EntryInstructionDetailsBasicUserControl test data source", typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
	}

	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new EntryInstructionDetailsBasicUserControl();
		_ = control.AssertContainsControl<GoodsNumberAndPositionUserControl>("GoodsNumberUserControl", x => x.WithBindTo("."));
		_ = control.AssertContainsControl<ZDropEdit>("SelectedDeclTypeDropEdit", x => x.WithBindTo("EntryHeader.CH_ReCalcDeclType"));
		_ = control.AssertContainsControl<ZDropEdit>("OriginalDeclarationDropEdit", x => x.WithBindTo("EntryHeader.CH_ReCalcOrigDecl"));
		_ = control.AssertContainsControl<ZDropEdit>("CaseCodeDropEdit", x => x.WithBindTo("EntryHeader.CH_ReCalcCaseCode"));
		_ = control.AssertContainsControl<ZCalcEdit>("PackageCountCalcEdit", x => x
			.WithBindTo(nameof(CusEntryInstruction.CEI_PackageCount))
			.WithCaption("Total No Of Units")
			.WithFullDescription("State the Total No Of Units for this entry.")
			.WithCharacterCasing(CharacterCasing.Normal)
			);
		_ = control.AssertContainsControl<ZTextBox>("ReasonTextBox", x => x
			.WithMultiline()
			.WithBindTo("EntryHeader.CH_ReCalcReason")
			);
		_ = control.AssertContainsControl<ZTextBox>("CustomsReplyMessageTextBox", x => x
			.WithMultiline()
			.WithReadOnly()
			.WithBindTo("EntryHeader.CH_ReCalcReplyMessage")
			);
		_ = control.AssertContainsControl<ZDateEdit>("RequestProcessingDateDateEdit", x => x
			.WithBindTo(nameof(CusEntryInstruction.CEI_DateForDuty)));
	});
}
