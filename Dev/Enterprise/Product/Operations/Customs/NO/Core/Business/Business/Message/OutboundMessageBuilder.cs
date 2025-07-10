using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business
{
	public sealed class OutboundMessageBuilder : IOutboundMessageBuilder
	{
		public OutboundMessageBuilder(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly BusinessObjectFactory factory;

		OutboundEDIMessage IOutboundMessageBuilder.Create(IMessageInformationProvider informationProvider)
			=> CreateCore(informationProvider);

		OutboundEDIMessage CreateCore(IMessageInformationProvider informationProvider)
		{
			Argument.NotNull(informationProvider, nameof(informationProvider));
			Argument.NotNull(informationProvider.MessageNumberStrategy, nameof(IMessageInformationProvider.MessageNumberStrategy));

			var messageText = informationProvider.MessageText;

			if (messageText.IsEmpty)
			{
				return null;
			}

			var outboundMessage = factory.New<OutboundEDIMessage>();
			outboundMessage.EM_ApplicationCode = informationProvider.ApplicationCode;
			outboundMessage.EM_MessageType = informationProvider.MessageType;
			outboundMessage.EM_MessageSubType = informationProvider.MessageSubType;
			outboundMessage.EM_Status = EDIMessage.Status.Queued;
			outboundMessage.EM_MessageText = messageText;
			outboundMessage.EM_LinkedObject = informationProvider.Parent;
			outboundMessage.EM_ApplicationReference = informationProvider.ApplicationReference;
			outboundMessage.MessageNumberStrategy = informationProvider.MessageNumberStrategy;
			outboundMessage.EM_GP = ZGuid.Empty;
			return outboundMessage;
		}
	}
}
