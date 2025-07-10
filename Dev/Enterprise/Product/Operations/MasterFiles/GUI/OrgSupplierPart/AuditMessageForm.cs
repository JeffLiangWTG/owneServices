using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AuditMessageForm : ZChildForm
	{
		public override string FormVerb => string.Empty;

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("ae8e6b3b-0249-455b-bdfc-30a045346579", "Set audit message");
				return string.IsNullOrEmpty(base.FormCaption) ? caption : $"{base.FormCaption} {caption}";
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			WriteToLogButton.DialogResult = System.Windows.Forms.DialogResult.OK;

			CancelButton = CloseButton;
			AcceptButton = WriteToLogButton;

			ActiveControl = ReferenceTextBox;
		}

		public string ReferenceText
		{
			get => ReferenceTextBox.Text;
			set => ReferenceTextBox.Text = value;
		}
	}
}
