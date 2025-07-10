using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

sealed class EntryInstructionDetailsControlBag : ControlBag
{
	EntryInstructionDetailsControlBag()
	{
		GoodsNumberUserControl = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.GoodsNumberUserControl));
		PackageCountCalcEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.PackageCountCalcEdit));
		RelatedDeclarationSeparatorUserControl = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.RelatedDeclarationSeparatorUserControl));
		CustomsReplyMessageTextBox = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.CustomsReplyMessageTextBox));
		ReasonTextBox = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.ReasonTextBox));
		CaseCodeDropEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.CaseCodeDropEdit));
		OriginalDeclarationDropEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.OriginalDeclarationDropEdit));
		SelectedDeclTypeDropEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.SelectedDeclTypeDropEdit));
		RequestProcessingDateDateEdit = RegisterControl(nameof(EntryInstructionDetailsBasicUserControl.RequestProcessingDateDateEdit));
	}

	public static EntryInstructionDetailsControlBag Instance => instance ??= new EntryInstructionDetailsControlBag();

	[ThreadStatic]
	static EntryInstructionDetailsControlBag instance;

	protected override Control CreateTemplate() => new EntryInstructionDetailsBasicUserControl();

	public ControlReference GoodsNumberUserControl { get; }
	public ControlReference PackageCountCalcEdit { get; }
	public ControlReference RelatedDeclarationSeparatorUserControl { get; }
	public ControlReference CustomsReplyMessageTextBox { get; }
	public ControlReference ReasonTextBox { get; }
	public ControlReference CaseCodeDropEdit { get; }
	public ControlReference OriginalDeclarationDropEdit { get; }
	public ControlReference SelectedDeclTypeDropEdit { get; }
	public ControlReference RequestProcessingDateDateEdit { get; }
}
