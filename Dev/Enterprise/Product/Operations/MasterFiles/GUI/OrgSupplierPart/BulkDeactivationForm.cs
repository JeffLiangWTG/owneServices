using CargoWise.ComponentModel;

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BulkDeactivationForm : ZChildForm
	{
		public BulkDeactivationForm(OrgSupplierBulkDeactivator businessObject)
			: base(businessObject)
		{
		}

		protected void ContinueBtn_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors())
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
			}
		}
	}
}
