using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMEDIInterchange : EDIInterchange, Integration.Customs.US.IUEMEDIInterchange
	{
		public UEMEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
		}

		protected override bool ShouldSendViaEHubCore => true;

		public UEMEDIMessage CreateMessageFromInterchange()
		{
			var ediMessage = (UEMEDIMessage)ContainedMessages.AddNew(typeof(UEMEDIMessage));
			ediMessage.EM_MessageType = MessageTypeList.Codes.ExportManifestResponse;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_GB = EI_GB;
			ediMessage.EM_MessageText = EI_BodyText;
			ediMessage.EM_MessageNum = "00001";
			return ediMessage;
		}
	}
}
