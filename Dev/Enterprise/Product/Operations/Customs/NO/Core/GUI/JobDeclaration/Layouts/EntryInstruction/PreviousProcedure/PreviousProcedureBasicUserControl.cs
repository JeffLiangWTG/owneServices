using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NO.GUI;

partial class PreviousProcedureBasicUserControl : ZUserControl
{
	public PreviousProcedureBasicUserControl()
	{
		InitializeComponent();
	}

	void ImportFromTemporaryStorageRegisterButton_Click(object sender, EventArgs e)
	{
		using var module = ZModuleFactory.Instance.Create(ModuleIDs.Customs.ImportFromTemporaryStorageRegister) as ZFilterGridModule;
		var parentForm = ParentForm;
		module.SetFormsModalTo(parentForm);

		using var temporaryStorageRegisterPopup = module.ShowPopup() as ImportFromTemporaryStorageRegisterModuleForm;
		temporaryStorageRegisterPopup.PreviousDocuments = PreviousDocuments;
		_ = ZFormModaliser.ShowDialogWithoutDispose(temporaryStorageRegisterPopup, parentForm);
	}

	PreviousDocumentMaster PreviousDocumentMaster => DataSource as PreviousDocumentMaster;

	CusSupportingInfoCollection<CusSupportingInfo> PreviousDocuments => (CusSupportingInfoCollection<CusSupportingInfo>)PreviousDocumentMaster?.PreviousDocuments;
}
