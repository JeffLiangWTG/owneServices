using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGEDIMessageTypeDecider : TypeDecider, Integration.Customs.SG.ISGEDIMessageTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if (row[EDIMessageSchema.Constants.EM_ApplicationCode].ToString() == ApplicationCodeList.Codes.SGCustomsTradenetXML)
			{
				return typeof(SGXmlEDIMessage);
			}

			var result = typeof(SGEDIMessage);

			string messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString();
			string messageDirection = row[EDIMessageSchema.Constants.EM_ReceiveTransmit].ToString();

			if (messageDirection == EDIMessage.Direction.Transmit)
			{
				result = messageType == MessageTypeCodeList.Codes.COO ? typeof(COODECEDIMessage) : typeof(CUSDECEDIMessage);
			}
			else
			{
				switch (messageType)
				{
					case Aperak09bMessageProcessor.MessageType:
						return typeof(APERAKEDIMessage);
					case Cuspmt09bMessageProcessor.MessageType:
						return typeof(CUSPMTEDIMessage);
					case Cusres09bMessageProcessor.MessageType:
						return typeof(CUSRESEDIMessage);
					case Debadv09bMessageProcessor.MessageType:
						return typeof(DEBADVEDIMessage);
					case Tcodec09bMessageProcessor.MessageType:
						return typeof(COODCIEDIMessage);
				}
			}

			return result;
		}

		public override Type GetTypeForNew()
		{
			return typeof(SGEDIMessage);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(SGEDIMessage);
		}
	}
}
