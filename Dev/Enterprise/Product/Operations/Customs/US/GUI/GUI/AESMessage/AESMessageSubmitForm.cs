using System;
using System.Globalization;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class AESMessageSubmitForm : ZChildForm
	{
		public AESMessageSubmitForm(JobDeclaration declaration)
			: base(new AESDeclaration(declaration))
		{
			this.Text = "AES Messages";
		}

		public AESMessageSubmitForm()
		{
		}

		public new AESDeclaration BusinessEntity
		{
			get { return (AESDeclaration)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SubmitButton_Click(object sender, EventArgs e)
		{
			bool hasOneFlagForSending = false;
			bool isCancelledByUser = false;
			foreach (CusEntryHeader entry in BusinessEntity.Entries)
			{
				if (entry.US_ShouldBeReportToCustoms)
				{
					hasOneFlagForSending = true;
					bool okToSend = true;
					if (entry.IsWaitingForResponse)
					{
						okToSend = (Globals.Message.Show(string.Format(CultureInfo.CurrentCulture, AwaitingCustomsResponseWarningMessage, entry.CH_BGMReference), "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes);
					}
					if (!okToSend)
					{
						entry.US_ShouldBeReportToCustoms = false;
						isCancelledByUser = true;
					}
				}
			}

			if (hasOneFlagForSending && !isCancelledByUser)
			{
				AESMessageManager.SubmitToCustoms(BusinessEntity);
				Close();
			}
			else
			{
				Globals.Message.ShowInformation(isCancelledByUser ? "Submission Cancelled By User" : "No SED has been flagged for submission", "No SED Submitted");
			}
		}
		const string AwaitingCustomsResponseWarningMessage = "This SED '{0}' is currently awaiting a Customs response.\r\nAre you sure you want to resend to Customs?\r\n(Resending an original Electronic Export Information will be rejected as shipment already on file).";
	}
}
