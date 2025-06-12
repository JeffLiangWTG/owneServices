using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp
{
	public class FtpWinScpReceiverEndpoint : WinScpReceiverEndpoint
	{
		public FtpWinScpReceiverEndpoint() : base(FtpWinScpClient.Factory.Instance, typeof(FtpWinScpReceiveConfiguration)) {}
	}
}
