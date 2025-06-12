using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.FtpEx
{
	public class FtpExTransmitterEndpoint : TransferrerTransmitterEndpoint
	{
		public FtpExTransmitterEndpoint(AsyncTransmitter asyncTransmitter) : this(asyncTransmitter, new FtpTransferrerFactory()) { }
		public FtpExTransmitterEndpoint(AsyncTransmitter asyncTransmitter, ITransferrerFactory factory) : base(asyncTransmitter, factory) { }
	}
}
