using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Ftp;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.FtpExPolling
{
	public class FtpExPollingTransmitterEndpoint : PollingEndpoint
	{
		public FtpExPollingTransmitterEndpoint(AsyncTransmitter asyncTransmitter)
			: base(asyncTransmitter, "http://cargowise.com/ehub/biztalkadapters/polling#FtpExPollingRequest", () => new FtpTransferrer()) { }
	}
}
