using System;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class DeferredSubmissionForm : ZChildForm
	{
		public DeferredSubmissionForm(DeferredSubmission businessObject) : base(businessObject)
		{
			//InitializeComponent() is called from ZForm.  Do not call again here.

			deferredSubmission = businessObject;
			ArrivalDateEdit.ReadOnly = true;
		}

		readonly DeferredSubmission deferredSubmission;

		public override string FormHeading => Res.GetString("5EFE0246-CDB3-4A40-ADE6-4CBAA9AE21FE", "Defer Submission Options");

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ActiveControl = SubmitButton;
		}

		void SubmitButton_Click(object sender, EventArgs e)
		{
			deferredSubmission.Validation.ValidateAll();
			if (deferredSubmission.SubmissionDateInfo.HasNotifications()
				|| deferredSubmission.PaymentMethodInfo.HasNotifications()
				|| deferredSubmission.DeferredAccountInfo.HasNotifications())
			{
				Globals.Message.ShowError(Res.GetString("90800A71-4AB0-4282-B43C-59CB9A1BE755", "There are errors that need to be corrected before this Declaration can be Submitted."));
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}
	}
}
