using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class AmendmentReasonForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AmendmentReasonForm()
		{
			InitializeComponent();
		}

		public AmendmentReasonForm(AmendmentWithdrawalReason amendmentWithdrawalReason) : base(amendmentWithdrawalReason)
		{
			this.amendmentWithdrawalReason = amendmentWithdrawalReason;
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("f81248c0-6c85-4e46-a6b2-aa32a0b8766d", "Please enter a reason"); }
		}

		readonly AmendmentWithdrawalReason amendmentWithdrawalReason;

		internal void OKButton_Click(object sender, EventArgs e)
		{
			amendmentWithdrawalReason.RunPreSaveValidation();
			if (amendmentWithdrawalReason.HasErrors)
			{
				amendmentWithdrawalReason.IsCancelled = true;
				Globals.Message.ShowWarning(Res.GetString("19a44181-4c1a-498a-ae21-66c8be68b280", "Please enter a reason for the amendment or withdrawal."), "");
			}
			else
			{
				amendmentWithdrawalReason.IsCancelled = false;
				this.DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		internal void CancelButton_Click(object sender, EventArgs e)
		{
			amendmentWithdrawalReason.IsCancelled = true;
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
