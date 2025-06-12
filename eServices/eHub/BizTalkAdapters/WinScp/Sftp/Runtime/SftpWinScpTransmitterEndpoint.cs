using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp
{
	public class SftpWinScpTransmitterEndpoint : WinScpTransmitterEndpoint
	{
		public SftpWinScpTransmitterEndpoint(AsyncTransmitter asyncTransmitter) : this(asyncTransmitter, LoggerFactory.Instance, SftpWinScpClient.Factory.Instance) { }

		internal SftpWinScpTransmitterEndpoint(AsyncTransmitter asyncTransmitter, ILoggerFactory loggerFactory, IWinScpClientFactory winScpClientFactory)
			: base(asyncTransmitter, loggerFactory, winScpClientFactory, typeof(SftpWinScpTransmitConfiguration)) { }
	}
}
