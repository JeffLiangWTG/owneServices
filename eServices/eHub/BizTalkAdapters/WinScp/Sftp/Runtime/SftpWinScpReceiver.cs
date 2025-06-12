using System;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp
{
	public class SftpWinScpReceiver : Receiver
	{
		public SftpWinScpReceiver()
			: base(
				"SFTPWinSCP Receiver",
				"1.0",
				"Custom SFTPWinSCP adapter",
				"SFTPWinSCP",
				new Guid("594C4FE8-6770-4B53-8ABA-9E00D3DD93F0"),
				SftpWinScpAdapterManagement.SftpWinScpPropertyNamespace,
				typeof(SftpWinScpReceiverEndpoint))
		{
		}
	}
}
