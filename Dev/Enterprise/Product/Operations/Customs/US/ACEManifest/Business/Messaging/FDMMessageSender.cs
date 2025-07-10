using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ACEManifest.Business.Messaging
{
	public class FDMMessageSender
	{
		public FDMMessageSender(AdditionalMessageInformation additionalMessageInformation)
		{
			this.additionalMessageInformation = additionalMessageInformation;
			this.header = additionalMessageInformation.Header;
		}

		readonly AsycudaManifestHeader header;
		readonly AdditionalMessageInformation additionalMessageInformation;

		public void SendMessage()
		{
			var builder = new AIMMessageBuilder(new DepartureMessageHeader(header, additionalMessageInformation), header.Factory);
			var message = builder.Build();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USAMA;
			message.EM_ApplicationReference = header.AMA_MasterBill;
			message.EM_LinkedObject = header;
		}
	}
}
