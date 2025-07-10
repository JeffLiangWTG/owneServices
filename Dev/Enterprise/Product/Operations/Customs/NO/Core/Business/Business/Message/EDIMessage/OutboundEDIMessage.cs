using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business
{
	public class OutboundEDIMessage : EDIMessage
	{
		public OutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}
	}
}


