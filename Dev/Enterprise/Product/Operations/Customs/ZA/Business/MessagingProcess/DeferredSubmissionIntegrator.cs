namespace Enterprise.Customs.ZA.Business.MessagingProcess
{
	public class DeferredSubmissionIntegrator
	{
		public DeferredSubmissionIntegrator(JobDeclarationMessageSendingObjectParent decWrapper)
		{
			this.decWrapper = decWrapper;
		}
		readonly JobDeclarationMessageSendingObjectParent decWrapper;

		public bool ShouldShowDialog()
		{
			var result = false;

			if (decWrapper.HasDutiableSendingObject)
			{
				var helper = new DeferredSubmissionHelper(decWrapper.ParentDeclaration);
				helper.RefreshData();

				if (helper.CanAlterMessageSubmitDateOrPaymentDetails)
				{
					Submission = new DeferredSubmission(helper);
					result = true;
				}
			}

			return result;
		}

		public void UpdateDeferment()
		{
			if (Submission != null)
			{
				Submission.UpdateDeclaration();
				decWrapper.UpdateFromDeferredSubmission(Submission);
			}
		}

		public DeferredSubmission Submission { get; private set; }
	}
}
