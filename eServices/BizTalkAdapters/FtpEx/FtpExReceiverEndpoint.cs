using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.FtpEx
{
	public class FtpExReceiverEndpoint : TransferrerReceiverEndpoint
	{
		public FtpExReceiverEndpoint() : this(new FtpTransferrerFactory(), new SyncReceiveSubmitBatchFactory(), new TransferrerMessageFactory(), new TransferrerProperties.ReceiveFactory()) { }
		public FtpExReceiverEndpoint(FtpTransferrerFactory transferrerfactory, ISyncReceiveSubmitBatchFactory batchFactory, ITransferrerMessageFactory transferrerMessageFactory, TransferrerProperties.IReceiveFactory receivePropertiesFactory) :
			base(transferrerfactory, batchFactory, transferrerMessageFactory, receivePropertiesFactory) { }
	}
}
