using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesTeamForm : ZTemplateForm
	{
		public SalesTeamForm()
		{
			InitializeComponent();
		}

		public SalesTeamForm(SalesTeam businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupCommissionTabPage();
		}

		#endregion

		#region Properties

		SalesTeam SalesTeam
		{
			get { return (SalesTeam)BusinessEntity; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		#endregion

		#region Commissions

		void SetupCommissionTabPage()
		{
			CommissionTabPage.SetupSecurity(Env.Security.SalesTeamsEditCommission.IsAllowed ? Env.Security.SalesTeamsEditCommission : Env.Security.SalesTeamsViewCommission);
			if (!Env.Security.SalesTeamsEditCommission.IsAllowed)
			{
				CommissionTabPage.SetReadOnlyIncludingChildren();
				foreach (Control control in CommissionTabPage.Controls)
				{
					control.Enabled = false;
				}
			}
		}

		#endregion

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && SalesTeam.ShouldDetachSalesReps)
			{
				string warningText = Res.GetString("d9590741-859c-4bec-8047-7e73b10666f9", "Deactivating this Sales Team will also detach any Sales Representatives who are assigned to this team. Do you wish to continue?");
				DialogResult messageResult = Globals.Message.Show(warningText, Res.GetString("1f9c9c03-23f1-490a-9138-a76e66b8d9a2", "Deactivate Sales Team"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				result = messageResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			return result;
		}
	}
}
