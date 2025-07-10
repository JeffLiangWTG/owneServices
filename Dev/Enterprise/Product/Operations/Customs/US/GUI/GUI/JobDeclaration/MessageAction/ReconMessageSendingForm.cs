using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ReconMessageSendingForm : ZChildForm
	{
		public ReconMessageSendingForm()
		{
			InitializeComponent();
		}

		void ChangePaymentPanelVisibility()
		{
			PaymentPanel.Visible = action.IsActionForReplaceOrAdd;
		}

		public ReconMessageSendingForm(ACEReconMessageSendingAction action)
			: base(action)
		{
			this.action = action;
			InitializeComponent();
			ChangePaymentPanelVisibility();
		}
		readonly ACEReconMessageSendingAction action;

		public override string FormCaption
		{
			get { return "Send '" + action.ActionCode + "' message"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			action.RunPreSaveValidation();

			if (action.HasNotifications(CargoWise.EntityFramework.NotificationType.MessageError))
			{
				if (Globals.Message.Show(ThereIsANotification, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
			else
			{
				this.DialogResult = DialogResult.OK;
				Close();
			}
		}
		public const string ThereIsANotification = "There is a notification. Are you sure you wish to continue?";

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
