using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class SendAESTIROperationalActionRunner : USDeclarationOperationalActionRunner
	{
		public SendAESTIROperationalActionRunner(IOperationalActionSectionLog log)
			: base(log)
		{
		}

		protected override OperationalActionBulkMessageSender<JobDeclaration> GetMessageSenderCore(JobDeclaration declaration)
		{
			return new OperationalActionBulkAESTIRMessageSender(declaration);
		}

		protected override bool IsJobEligibleForSending(JobDeclaration declaration)
		{
			return declaration.IsExport;
		}
	}
}
