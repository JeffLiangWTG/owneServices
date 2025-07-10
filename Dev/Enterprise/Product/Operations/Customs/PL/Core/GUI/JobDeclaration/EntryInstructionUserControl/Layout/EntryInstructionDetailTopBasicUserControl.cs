using System;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class EntryInstructionDetailTopBasicUserControl : ZUserControl
{
	public EntryInstructionDetailTopBasicUserControl()
	{
		InitializeComponent();
	}

	public CusEntryInstruction EntryInstruction => CurrentDataItem as CusEntryInstruction;

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		var entryInstruction = EntryInstruction;
		if (entryInstruction != null)
		{
			declarationValueChangedAnnouncer = (entryInstruction.JobDeclaration as Customs.Business.IInvoicesProvider)?.GetValueChangedAnnouncer();
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += declarationValueChangedAnnouncer_OnValueChanged;
			}

			declarationValueChangedAnnouncer_OnValueChanged(this, null);
		}
	}
	Customs.Business.IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

	protected override void OnCurrentDataItemChanging(EventArgs e)
	{
		base.OnCurrentDataItemChanging(e);
		if (declarationValueChangedAnnouncer != null)
		{
			declarationValueChangedAnnouncer.OnValueChanged -= declarationValueChangedAnnouncer_OnValueChanged;
			declarationValueChangedAnnouncer.Dispose();
		}
		var entryInstruction = EntryInstruction;
	}

	void declarationValueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
	{
		var isExport = false;
		var exportManifestCheckBoxVisible = false;

		if (EntryInstruction is CusEntryInstruction entryInstruction
			&& entryInstruction.JobDeclaration is JobDeclaration declaration)
		{
			isExport = declaration.IsExport;
			exportManifestCheckBoxVisible = entryInstruction.IsAESTransitionPeriod()
				? declaration.JE_CustomsOffice == declaration.JE_OfficeOfEntryExit
				: (bool)declaration.IsExportGoodsLocatedAtOfficeOfExit;
		}

		EADPrintOutDropEdit.Visible = isExport;
		PostExportTransitCheckBox.Visible = isExport;
		OfficeOfExitArrivalTimeLimitDateEdit.Visible = isExport;
		ExportManifestCheckBox.Visible = exportManifestCheckBoxVisible;
	}
}
