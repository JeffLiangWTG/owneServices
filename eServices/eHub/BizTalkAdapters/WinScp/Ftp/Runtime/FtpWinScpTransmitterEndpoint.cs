using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp
{
	public class FtpWinScpTransmitterEndpoint : WinScpTransmitterEndpoint
	{
		public FtpWinScpTransmitterEndpoint(AsyncTransmitter asyncTransmitter) : this(asyncTransmitter, LoggerFactory.Instance, FtpWinScpClient.Factory.Instance) { }
		public FtpWinScpTransmitterEndpoint(AsyncTransmitter asyncTransmitter, ILoggerFactory loggerFactory, IWinScpClientFactory winScpClientFactory) : base(asyncTransmitter, loggerFactory, winScpClientFactory, typeof(FtpWinScpTransmitConfiguration)) { }
	}
}
