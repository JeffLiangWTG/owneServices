using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMOutgoingMessageProcessor : Enterprise.Messaging.Business.MessageProcessor.OutgoingMessageProcessor
	{
		public AIMOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool IsBranchFilter => false;

		protected override ZQuery MessageFilter
		{
			get
			{
				if (messageFilter == null)
				{
					messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USAMA);
					messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					messageFilter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
					messageFilter.AddToFilter(EDIMessageSchema.EM_IsActive, "Y");
				}
				return messageFilter;
			}
		}

		ZQuery messageFilter;

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new AIMInterchangeProvider(readyMessages);
		}
	}
}
