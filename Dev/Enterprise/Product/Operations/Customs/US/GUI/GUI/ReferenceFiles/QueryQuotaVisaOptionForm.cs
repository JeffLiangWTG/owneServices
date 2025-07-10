using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class QueryQuotaVisaOptionForm : ZChildForm
	{
		public QueryQuotaVisaOptionForm()
		{
		}

		public QueryQuotaVisaOptionForm(QueryQuotaVisaOption option)
			: base(option)
		{
			VisaTextBox.Visible = Option != null && !Option.IsACE;
		}

		QueryQuotaVisaOption Option
		{
			get { return (QueryQuotaVisaOption)BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Option != null && Option.IsACE ? "Query for Quota Options" : "Query for Quota/Visa Options"; }
		}

		internal void SendButton_Click(object sender, EventArgs e)
		{
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				try
				{
					Option.SendQueryWithoutSaving();
					Option.Factory.Save();
				}
				catch (Exception e1) when (!e1.IsCriticalException())
				{
					HandleSaveException(e1);
				}
				Close();
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				if (Option.HasNotifications() && Globals.Message.Show(NotificationsWarning, (Option != null && Option.IsACE ? "Query Quota" : "Query Quota/Visa"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
				{
					result = ContinueWithSave.No;
				}
			}

			return result;
		}

		internal const string NotificationsWarning = "There are notifications. Are you sure you wish to continue?";

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
