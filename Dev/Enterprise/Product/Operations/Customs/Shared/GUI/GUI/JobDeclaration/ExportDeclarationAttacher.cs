using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public class ExportDeclarationAttacher : ZRecordAttacher
	{
		public ExportDeclarationAttacher(BaseJobDeclaration jobDeclaration, BusinessObjectCollection findBoxList)
			: base(null, findBoxList, ModuleIDs.Customs.JobDeclaration)
		{
			this.jobDeclaration = jobDeclaration;
		}
		readonly BaseJobDeclaration jobDeclaration;

		protected bool ShouldShow(ZForm formToShowModalTo)
		{
			return !NeedToSave ||
					(ShowConfirmationForSaveBeforeAttach() == DialogResult.Yes &&
					formToShowModalTo.FireSaveButton() == ContinueWithSave.Yes);
		}

		protected bool NeedToSave
		{
			get { return jobDeclaration.HasChanges; }
		}

		DialogResult ShowConfirmationForSaveBeforeAttach()
		{
			string caption = Res.GetString("b39d19ac-56de-4d73-b940-6ce842d2e29b", "Save Confirmation");
			string message = Res.GetString("26af49a4-e1d4-4746-8b80-092ca6d51aa5", "The form must be saved before an Export Declaration can be attached. Do you wish to save the form?");
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
		}

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return jobDeclaration.AttachExportDeclaration(bizO.PK);
		}
	}
}
