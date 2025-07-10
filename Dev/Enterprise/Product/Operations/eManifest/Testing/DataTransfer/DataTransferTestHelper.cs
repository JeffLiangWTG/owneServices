using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.eManifest.DataTransfer.Testing
{
	class DataTransferTestHelper
	{
		internal static IEDIMessage GetQueuedUniversalDataMessage(UniversalObjectFactory uFactory, string messageText, string subType, bool save, bool createInterchange = false)
		{
			var message = GetQueuedUniversalDataMessage(uFactory.BOFactory, messageText, subType);

			if (createInterchange)
			{
				CreateInterchange(uFactory.BOFactory, message);
			}

			if (save)
			{
				uFactory.SaveForTesting();
			}
			return message;
		}

		internal static IEDIMessage GetQueuedUniversalDataMessage(BusinessObjectFactory factory, string messageText, string subType)
		{
			var message = factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = subType;
			message.EM_MessageText = messageText;

			return message;
		}

		static void CreateInterchange(BusinessObjectFactory factory, IEDIMessage message)
		{
			var interchange = factory.New<IEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = "A";
			interchange.EI_To = "B";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			var interchangeHeaderFooter =
					@"<UniversalInterchange>
						<Header>
							<SenderID>A</SenderID>
							<RecipientID>B</RecipientID>
						</Header>
						<Body>
							{0}
						</Body>
					</UniversalInterchange>";
			var interchangeText = string.Format(interchangeHeaderFooter, message.EM_MessageText);
			interchange.EI_BodyText = interchangeText;
			message.EM_EI = interchange.PK;

			return;
		}
	}
}
