namespace Enterprise.Customs.Business
{
	public abstract class DISMessageManagerBase
	{
		protected DISMessageManagerBase(IDISDocumentBase disDocument)
		{
			this.disDocument = disDocument;
		}

		protected readonly IDISDocumentBase disDocument;

		public void SendSubmission()
		{
			SendSubmissionCore();
		}

		protected abstract void SendSubmissionCore();

		public void SendWithdrawl()
		{
			SendWithdrawlCore();
		}

		protected abstract void SendWithdrawlCore();
	}
}
