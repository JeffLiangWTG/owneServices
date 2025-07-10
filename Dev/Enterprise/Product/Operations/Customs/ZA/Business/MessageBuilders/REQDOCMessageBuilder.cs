using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D99B.Messages.REQDOC;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	class REQDOCMessageBuilder : EDIFACTMessageBuilder<IEDIMessageCollectionProvider, REQDOCMessage, REQDOCEDIMessage>
	{
		public REQDOCMessageBuilder(BusinessObjectFactory factory, IEDIMessageCollectionProvider messageCollectionProvider, IREQDOCMessageDataProvider source, MessageSubTypes messagesubType)
			: base(messageCollectionProvider, messagesubType, new ZACharacterSetNoCasing())
		{
			this.rEQDOCMessageDataProvider = source;
			this.factory = factory;
		}

		protected override void PopulateEdifactMessage()
		{
			REQDOCMessageTextBuilder.PopulateREQDOCMessage(edifactMessage, rEQDOCMessageDataProvider);
		}

		protected override REQDOCEDIMessage PopulateMessagesReturningResult()
		{
			REQDOCEDIMessage messageToSend = null;
			if (data != null)
			{
				messageToSend = base.PopulateMessagesReturningResult();
			}
			else
			{
				PopulateEdifactMessage();
				messageToSend = factory.New<REQDOCEDIMessage>();
				messageToSend.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
				messageToSend.EM_MessageSubType = GetMessageSubType();
				messageToSend.EM_MessageText = edifactMessage.ToString(characterSet);
				messageToSend.EM_MessageInterpretation = interpretation.ToHtml();
			}
			messageToSend.EM_GB = rEQDOCMessageDataProvider.Branch.PK;
			return messageToSend;
		}

		readonly IREQDOCMessageDataProvider rEQDOCMessageDataProvider;
		readonly BusinessObjectFactory factory;
	}
}
