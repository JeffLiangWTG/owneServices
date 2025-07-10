using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public partial class NZImportTCOBulkChangeForm : ZForm
	{
		public NZImportTCOBulkChangeForm(NZTariffBulkChange businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButton, CloseButton);
			DisableNewAction();
		}

		public new NZTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as NZTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return Enterprise.Customs.NZ.GUI.Res.GetString("414180F7-58A1-48DC-AE5C-E63C15CAED17", "Import Concession Bulk Change"); }
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				result = BusinessEntity.TCOAdditionalContinueWithSave();
			}
			return result;
		}
	}
}
