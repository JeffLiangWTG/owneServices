using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeAutoReceiveResponseMessageProvider : IMessageSender
	{
		readonly Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader header;

		public ETradeAutoReceiveResponseMessageProvider(Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader header)
		{
			this.header = header;
		}

		public BusinessObject Parent => (BusinessObject)header;

		public IBusinessObjectCollection Messages => header.Messages;

		public ZString JobReference => header.AMA_JobReference;
	}
}
