using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class StowPlanOutgoingMessageProcessor : CBPOutgoingMessageProcessor
	{
		public StowPlanOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool IsBranchFilter => false;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new StowPlanInterchangeProvider(readyMessages);
		}

		protected override ZQuery MessageFilter
		{
			get { return fMessageFilter ?? (fMessageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.StowPlan)); }
		}
		ZQuery fMessageFilter;
	}
}
