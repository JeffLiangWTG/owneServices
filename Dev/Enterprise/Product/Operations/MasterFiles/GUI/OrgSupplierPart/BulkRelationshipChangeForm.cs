using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BulkRelationshipChangeForm : ZChildForm
	{
		public BulkRelationshipChangeForm(OrgSupplierBulkRelationshipChanger businessObject) : base(businessObject)
		{
			this.zLabel1.Text = Res.GetString("BulkRelationshipChangeForm|InstructionsLabel", "This form allows you to do a 'Bulk' change of the Organization Relationships on all products selected by the Grid Filter. All Relationships specified in the 'From Details' will be changed to the 'To Details'. If you do not enter any 'From Details' then the Organization and Relationship specified in the 'To Details' will be added to all nominated products. If you do not enter a 'To Details' Organization, but do enter a 'To Details' relationship then the relationship specified in 'From Details' will be changed to the new relationship.");
		}

		#region Events

		void ContinueBtn_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors())
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
			}
		}

		#endregion
	}
}
