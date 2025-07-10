using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class StatementSendingActionForm : ZChildForm
	{
		public StatementSendingActionForm()
		{
		}

		public StatementSendingActionForm(StatementDeleteAndSendingActionCollection actions)
			: base(actions)
		{
			this.actions = actions;
			actions.IsCancelled = true;

			if (actions.IsReconciliationAction)
			{
				ZGridColumnInfo column = EntriesGrid.GetColumnStyle(StatementDeleteAndSendingAction.Schema.US_PeriodicStatementMonth);
				if (column != null)
				{
					column.IsUnavailable = true;
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		internal readonly StatementDeleteAndSendingActionCollection actions;

		public override string FormCaption
		{
			get { return "Send Statement Messages"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		internal void OKButton_Click(object sender, EventArgs e)
		{
			if (!actions.HasAtLeastOneToSendMessageFor)
			{
				Globals.Message.ShowInformation(YouHaveNotSelectedAnythingToSendMessagesFor);
			}
			else
			{
				if (actions.HasMessageErrors() && !Env.Security.USCustomsImportStatementSendWithMessageErrors.IsAllowed)
				{
					Env.Security.USCustomsImportStatementSendWithMessageErrors.ShowError();
				}
				else if (!actions.HasNotifications() ||
					Globals.Message.Show("There are notifications. Are you sure you wish to continue?", "Statement Delete/Add", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					actions.IsCancelled = false;
					Close();
				}
			}
		}

		internal void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		public const string YouHaveNotSelectedAnythingToSendMessagesFor = "There is nothing to send a message for";

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
