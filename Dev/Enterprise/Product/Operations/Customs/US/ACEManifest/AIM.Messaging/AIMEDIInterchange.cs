using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMEDIInterchange : EDIInterchange, Integration.Customs.US.IAIMEDIInterchange
	{
		public AIMEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodeList.Codes.USAMA;
		}

		protected override bool ShouldSendViaEHubCore => true;

		public AIMEDIMessage CreateMessageFromInterchange()
		{
			var ediMessage = (AIMEDIMessage)ContainedMessages.AddNew(typeof(AIMEDIMessage));
			ediMessage.EM_MessageType = EDIMessageTypeList.Codes.FHL;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_GB = EI_GB;
			ediMessage.EM_MessageText = EI_BodyText;
			ediMessage.EM_MessageNum = "00001";

			var messageType = ediMessage.EM_MessageText.Left(3);
			ediMessage.EM_MessageSubType = !messageType.IsEmpty ? messageType.ToString() : EDIMessageTypeList.Codes.FHL;

			return ediMessage;
		}
	}
}
