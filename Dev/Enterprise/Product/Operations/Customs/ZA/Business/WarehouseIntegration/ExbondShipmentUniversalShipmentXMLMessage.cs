using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ZA.Business.WarehouseIntegration
{
	class ExbondShipmentUniversalShipmentXMLMessage : EDIMessage
	{
		public ExbondShipmentUniversalShipmentXMLMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			this.factory = factory;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.ZACustomsEDIFACTNumberFountain("M", ApplicationCodeList.Codes.UniversalDataMessaging).GetNextFormatted(factory);
		}

		public override void OnSaving()
		{
			PopulateMessageNumber();
			base.OnSaving();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			EM_MessageType = EDIMessageTypeList.Codes.XDC;
			EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		}

		readonly BusinessObjectFactory factory;
	}
}
