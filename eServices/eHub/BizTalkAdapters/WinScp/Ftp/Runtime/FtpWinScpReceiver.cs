using System;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp
{
	public class FtpWinScpReceiver : Receiver
	{
		public FtpWinScpReceiver() : base(
			"FTPWinSCPV2 Adapter",
			"1.0",
			"Custom FTP adapter using WinSCP",
			"FTPWinSCPV2",
			new Guid("98017773-D5B4-46A7-9572-BB5CC96E22A6"),
			FtpWinScpAdapterManagement.FtpWinScpPropertyNamespace,
			typeof(FtpWinScpReceiverEndpoint))
		{
		}
	}
}
