using System;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Messaging.Business;
using InboundInterchangeProcessor = Enterprise.Customs.US.Messaging.Business.InboundInterchangeProcessor;

namespace Enterprise.Customs.US.eManifest.Messaging.Interchange
{
	public class InterchangeProcessor : InboundInterchangeProcessor
	{
		#region Overrides of InboundInterchangeProcessor

		protected override string[] ApplicationCodes
		{
			get { return new string[] { CBPEDIInterchange.ApplicationCodes.USeManifest }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new MessageCreator();
		}

		protected override void HandleProcessingException(Exception ex, EDIInterchange interchange, ZGuid interchangePK)
		{
			var processingException = ex as MessageProcessingException;
			if (processingException != null)
			{
				BaseInterchangeRetriever.HandleMessageProcessingException(processingException);
			}
			base.HandleProcessingException(ex, interchange, interchangePK);
		}
		#endregion

		#region MessageCreator

		class MessageCreator : IInboundMessageCreator
		{
			#region Implementation of IInboundMessageCreator

			public void CreateMessagesForInterchange(EDIInterchange interchange)
			{
				interchange.PopulateInterchangeFromString(
					interchange.EI_BodyText,
					EDIInterchange.ApplicationCodes.USeManifest,
					true,
					true);
				interchange.EI_FromInfo.Value = interchange.EI_FromInfo.OriginalValue;
				interchange.EI_ToInfo.Value = interchange.EI_ToInfo.OriginalValue;
				interchange.EI_InterchangeNumInfo.Value = interchange.EI_InterchangeNumInfo.OriginalValue;
				interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.USeManifest;
				interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived(throwExceptionIfInDatabase: false);
			}

			#endregion
		}

		#endregion
	}
}
