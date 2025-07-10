using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExpireCommissionAgreementForm : ZChildForm
	{
		public ExpireCommissionAgreementForm(ExpireCommissionAgreementAction action)
			: base(action)
		{
			InitializeComponent();
		}

		new ExpireCommissionAgreementAction CurrentDataItem
		{
			get { return (ExpireCommissionAgreementAction)base.CurrentDataItem; }
		}

		#region Buttons

		void OkButton_Click(object sender, System.EventArgs e)
		{
			CurrentDataItem.Validation.ValidateAll();
			if (CurrentDataItem.HasErrors)
			{
				Globals.Message.Show(CurrentDataItem.Notifications.ToMessageListString(), CannotExecuteCaptionMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			CurrentDataItem.Execute();
			Close();
		}

		#endregion

		#region Form Captions

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Messages

		public string CannotExecuteCaptionMessage
		{
			get { return Res.GetString("73331912-7fcf-43b5-9903-1bc4e7f6880e", "Unable to continue"); }
		}

		#endregion
	}
}
