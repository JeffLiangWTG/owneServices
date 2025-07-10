using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public abstract class UniversalCustomsApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		protected UniversalCustomsApplicationTypeMessageProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsMessageProcessor customsMessageProcessor)
			: base(logger)
		{
			this.applicationCode = Argument.NotNullOrEmpty(applicationCode, nameof(applicationCode));
			this.customsMessageProcessor = Argument.NotNull(customsMessageProcessor, nameof(customsMessageProcessor));
		}
		protected readonly string applicationCode;
		protected readonly IUniversalCustomsMessageProcessor customsMessageProcessor;

		protected sealed override string ApplicationCodeCore => applicationCode;

		protected sealed override ZQuery MessageFilterCore
		{
			get
			{
				var result = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
				result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				result.AddToFilter(EDIMessageSchema.EM_Status, StatusesToInclude);
				return result;
			}
		}

		protected abstract ZString[] StatusesToInclude { get; }

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => customsMessageProcessor.ShouldMessageBeProcessedInASeparateFactory(message);
	}
}
