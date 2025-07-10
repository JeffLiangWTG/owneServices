using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsEDIMessagePacker(ApplicationCodeList.Codes.TWCustoms, typeof(Enterprise.Customs.TW.Business.TWCMessagePacker))]

namespace Enterprise.Customs.TW.Business;

public class TWCMessagePacker : IUniversalCustomsEDIMessagePacker
{
	bool IUniversalCustomsEDIMessagePacker.AllowEmptyMessageBody => false;

	ZString IUniversalCustomsEDIMessagePacker.Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		try
		{
			PopulateInterchange((TWMessage)message, interchange);
			logger.Log($"Message({message.EM_MessageNum}) packer finished successfully.");
			return ZString.Empty;
		}
		catch (Exception ex)
		{
			logger.LogError($"Message({message.EM_MessageNum}) packer failed:{ex.Message}");
			return ex.Message;
		}
	}

	void PopulateInterchange(TWMessage message, EDIInterchange interchange)
	{
		EDIMessagePackerUtils.PopulateInterchange(
			interchange,
			message.EM_ApplicationCode,
			message.IsLicensingMessageDeliveryNotification ? MessageTypeList.Codes.NXM : message.EM_MessageType,
			GlbCompany.CurrentCompany.LicenceKeyIdentifier,
			message.EM_IsTestMessage ? TWCInterchange.TWCustomsForTest : MessageConstants.TWCustoms,
			message.EM_GB,
			message.EM_GP,
			ZGuid.NewZGuid(),
			status: EDIInterchange.Status.eHubQueued,
			receiveTransmit: message.EM_ReceiveTransmit,
			transportType: EDIInterchangeTransportTypeList.Codes.eHub);

		interchange.EI_HeaderText = TWMessageHelper.Serialize(TWMessageHelper.CreateTWMessageInfoFromTWMessage(message), true);
		interchange.EI_BodyText = message.EM_MessageText;
		interchange.ContainedMessages.Add(message);
	}
}
