using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp
{
	public class SftpWinScpReceiverEndpoint : WinScpReceiverEndpoint
	{
		public SftpWinScpReceiverEndpoint() : base(SftpWinScpClient.Factory.Instance, typeof(SftpWinScpReceiveConfiguration)) { }
	}
}
