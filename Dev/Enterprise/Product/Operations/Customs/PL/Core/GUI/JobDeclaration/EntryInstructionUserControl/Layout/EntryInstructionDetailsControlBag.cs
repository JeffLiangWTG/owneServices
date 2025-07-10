using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public sealed class EntryInstructionDetailsControlBag : ControlBag
{
	EntryInstructionDetailsControlBag()
	{
		ExportManifestCheckBox = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.ExportManifestCheckBox));
		PostExportTransitCheckBox = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.PostExportTransitCheckBox));
		EADPrintOutDropEdit = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.EADPrintOutDropEdit));
		TemporaryLocationDropEdit = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.TemporaryLocationDropEdit));
		TemporaryLocationTextBox = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.TemporaryLocationTextBox));
		OfficeOfExitArrivalTimeLimitDateEdit = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.OfficeOfExitArrivalTimeLimitDateEdit));
		DeclarationDateDateEdit = RegisterControl(nameof(EntryInstructionDetailTopBasicUserControl.DeclarationDateDateEdit));
	}

	public static EntryInstructionDetailsControlBag Instance => instance ?? (instance = new EntryInstructionDetailsControlBag());

	[ThreadStatic]
	static EntryInstructionDetailsControlBag instance;

	protected override Control CreateTemplate() => new EntryInstructionDetailTopBasicUserControl();

	public ControlReference ExportManifestCheckBox { get; }

	public ControlReference PostExportTransitCheckBox { get; }

	public ControlReference EADPrintOutDropEdit { get; }

	public ControlReference TemporaryLocationDropEdit { get; }

	public ControlReference TemporaryLocationTextBox { get; }

	public ControlReference OfficeOfExitArrivalTimeLimitDateEdit { get; }

	public ControlReference DeclarationDateDateEdit { get; }
}
