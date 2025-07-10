using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class TRSPTSAutoReceiveResponseMessageProvider : IMessageSender
	{
		readonly Integration.Customs.TR.ICusInBondSPTSHeader sptsHeader;

		public TRSPTSAutoReceiveResponseMessageProvider(Integration.Customs.TR.ICusInBondSPTSHeader sptsHeader)
		{
			this.sptsHeader = sptsHeader;
		}

		public BusinessObject Parent => (BusinessObject)sptsHeader;

		public IBusinessObjectCollection Messages => sptsHeader.Messages;

		public ZString JobReference => sptsHeader.BH_JobReference;
	}
}
