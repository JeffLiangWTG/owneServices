using System;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp
{
	public class FtpWinScpTransmitter : Transmitter
	{
		public FtpWinScpTransmitter() : base(
			"FTPWinSCPV2 Transmitter",
			"1.0",
			"Custom FTP adapter using WinSCP",
			"FTPWinSCPV2",
			new Guid("9DBF934C-67E9-41F9-8E56-1BB5A0F496A1"),
			FtpWinScpAdapterManagement.FtpWinScpPropertyNamespace,
			typeof(FtpWinScpTransmitterEndpoint),
			20)
		{
		}
	}
}
