using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public class TRManifestAutoReceiveResponseMessageProvider : IMessageSender
	{
		readonly Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader;

		public TRManifestAutoReceiveResponseMessageProvider(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader manifestHeader)
		{
			this.manifestHeader = manifestHeader;
		}

		public BusinessObject Parent => (BusinessObject)manifestHeader;

		public IBusinessObjectCollection Messages => manifestHeader.Messages;

		public ZString JobReference => TRMessageConstants.ReferencePrefix + manifestHeader.AMA_JobReference;
	}
}
