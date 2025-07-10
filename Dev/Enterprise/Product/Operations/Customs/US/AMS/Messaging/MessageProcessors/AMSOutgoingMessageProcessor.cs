using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSOutgoingMessageProcessor : CBPOutgoingMessageProcessor
	{
		public AMSOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
		protected override bool IsBranchFilter => false;
		protected override ZQuery MessageFilter
		{
			get { return messageFilter ?? (messageFilter = AMSEDIMessage.AMSFilter); }
		}
		ZQuery messageFilter;
	}
}
