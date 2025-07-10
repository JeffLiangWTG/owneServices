using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public abstract class BaseConfigurationMessageProcessor<TRequest, TResponse> : ICFGUniversalCustomsMessageProcessor
		where TRequest : ICFGMessageRequest
		where TResponse : ICFGMessageResponse
	{
		public void ProcessMessage(EDIMessage message, ILoggingInformation logger)
		{
			var outgoingInterchange = UCMPHelper.GetOutgoingEdiInterchange(message.Interchange);
			if (outgoingInterchange == null)
			{
				logger.LogError($"No outgoing Interchange for CFG message {message.EM_MessageNum}");
				message.EM_Status = EDIMessage.Status.Discarded;
			}
			else
			{
				var provider = GetProvider(outgoingInterchange);

				if (IsValidMessageCore(outgoingInterchange, provider.GetRequestMessage(outgoingInterchange), message))
				{
					message.EM_Status = EDIMessage.Status.Received;
					ProcessMessageCore(message, provider.ParseResponseMessage(message), logger);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;
				}
			}
		}

		IRequestResponseProvider<TRequest, TResponse> GetProvider(EDIInterchange outgoingInterchange)
		{
			var configurationType = outgoingInterchange.EI_To == XtCredentialConstants.CustomsCredentialChange
					? XtCredentialConstants.CustomsCredentialChange : Constants.Configuration.XHRecipient;
			var factory = new CFGRequestResponseProviderFactory();
			return factory.GetProvider<TRequest, TResponse>(configurationType);
		}

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, ILoggingInformation logger)
		{
			var outgoingInterchange = UCMPHelper.GetOutgoingEdiInterchange(message.Interchange);
			if (outgoingInterchange == null)
			{
				logger.LogError($"No outgoing Interchange for CFG message {message.EM_MessageNum}");
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, ResString.GetMultilingualString("36cb5a99-f3ea-430b-9cb2-a74454907809", "No outgoing Interchange for CFG message {0}", message.EM_MessageNum));
			}

			var provider = GetProvider(outgoingInterchange);
			var linkedObjectFromRequest = provider.GetLinkedObjectFromRequest(outgoingInterchange);

			var linkedBusinessObjectMetaData = GetLinkedBusinessObjectMetaDataCore(message, linkedObjectFromRequest, logger);
			return linkedBusinessObjectMetaData ?? new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, ZString.Empty);
		}

		protected abstract bool IsValidMessageCore(EDIInterchange outgoingInterchange, TRequest requestMessage, EDIMessage message);
		protected abstract void ProcessMessageCore(EDIMessage message, TResponse responseMessage, ILoggingInformation logger);
		protected abstract LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(EDIMessage message, object linkedObject, ILoggingInformation logger);
	}
}
