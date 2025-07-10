using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrganisationViewStmNumsEditorForm : ViewStmNumsEditorForm
	{
		public OrganisationViewStmNumsEditorForm(OrganisationViewStmNums stmNums)
			: base(stmNums)
		{
			InitializeComponent();
			StmNums.SN_TypeInfo.ValueChanged += SN_TypeInfo_ValueChanged;
			ZoneIDPrefixTextBox.Visible = false;
			ClientPrefexTextBox.Visible = false;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateControlsVisibility();
		}

		void SN_TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControlsVisibility();
		}

		void UpdateControlsVisibility()
		{
			ZoneIDPrefixTextBox.Visible = StmNums.IsFTZNumberType;
			ClientPrefexTextBox.Visible = StmNums.IsFTZNonWarehouseType;
			PrefixTextBox.Visible = !StmNums.IsFTZWarehouseType && !StmNums.IsFTZNonWarehouseType;
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			StmNums.SN_TypeInfo.ValueChanged -= SN_TypeInfo_ValueChanged;
			base.OnClosing(e);
		}

		OrganisationViewStmNums StmNums
		{
			get { return (OrganisationViewStmNums)BusinessEntity; }
		}
	}
}
