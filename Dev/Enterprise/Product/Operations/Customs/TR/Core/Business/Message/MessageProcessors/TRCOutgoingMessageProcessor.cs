using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business
{
	public class TRCOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public TRCOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new TRInterchangeProvider(Logger, readyMessages);
		}

		protected override ZQuery MessageFilter
		{
			get { return messageFilter ?? (messageFilter = GetMessageFilterQuery()); }
		}
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.TRCustoms);
			result.OrderBy = EDIMessage.Schema.EM_MessageNum;
			return result;
		}
	}
}
