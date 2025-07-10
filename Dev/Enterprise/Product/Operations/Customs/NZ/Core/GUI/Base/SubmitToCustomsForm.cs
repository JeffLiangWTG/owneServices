using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Base
{
	public partial class SubmitToCustomsForm : ZChildForm
	{
		public SubmitToCustomsForm()
		{
		}

		public SubmitToCustomsForm(MessageManagerForClearance messageManager)
			: base(messageManager)
		{
		}

		protected MessageManagerForClearance MessageManager
		{
			get { return (MessageManagerForClearance)DataSource; }
		}

		public virtual void OkButton_Click(object sender, EventArgs e)
		{
			ZString errorsOnForm = MessageManager.GetErrorsForEnteredValues();
			if (errorsOnForm.IsEmpty)
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowError(errorsOnForm, MessageManager.HumanReadableOperationType);
			}
		}

		void CancelBtn_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			if (MessageManager != null)
			{
				SetFormDefaultsFromMessageManager();
			}
		}

		void SetFormDefaultsFromMessageManager()
		{
			Text = MessageManager.HumanReadableOperationType;
			MessageTypeLabel.Text = MessageManager.MessageTypeAndStatus;
		}

		void SelectDefaultRemarksButton_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new SelectDefaultRemarksForm(MessageManager));
		}
	}
}
