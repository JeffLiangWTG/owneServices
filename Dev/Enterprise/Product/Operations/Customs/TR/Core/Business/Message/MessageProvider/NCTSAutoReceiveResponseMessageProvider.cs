using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSAutoReceiveResponseMessageProvider : IMessageSender
	{
		readonly Integration.Customs.TR.ICusInBondHeader header;

		public NCTSAutoReceiveResponseMessageProvider(Integration.Customs.TR.ICusInBondHeader header)
		{
			this.header = header;
		}

		public BusinessObject Parent => (BusinessObject)header;

		public IBusinessObjectCollection Messages => header.Messages;

		public ZString JobReference => header.BH_JobReference;
	}
}
