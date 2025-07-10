using System;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public partial class EntryNumberUserControl : ZUserControl
	{
		public EntryNumberUserControl()
		{
			InitializeComponent();
		}

		public new AsycudaManifestHeader CurrentDataItem => (AsycudaManifestHeader)base.CurrentDataItem;

		void AllocateEntryNumberButton_Click(object sender, EventArgs e)
		{
			if (FindForm() is ZForm mainForm)
			{
				new AllocateNumberButtonClickEventHandler().Allocate(new AsycudaManifestHeaderEntryNumberSupporter(CurrentDataItem), new AllocateEventHandlerArgs
					(false, mainForm.BusinessEntityForHasChanges, () => mainForm.FireSaveButton()), ZString.Empty);
			}
		}

		void ModifyEntryNumberButton_Click(object sender, EventArgs e)
		{
			if (FindForm() is ZForm mainForm)
			{
				new AllocateNumberButtonClickEventHandler().Modify(new AsycudaManifestHeaderEntryNumberSupporter(CurrentDataItem), new AllocateEventHandlerArgs
					(false, mainForm.BusinessEntityForHasChanges, () => mainForm.FireSaveButton()));
			}
		}
	}
}
