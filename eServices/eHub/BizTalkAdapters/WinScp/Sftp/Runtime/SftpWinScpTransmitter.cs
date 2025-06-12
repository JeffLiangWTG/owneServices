using System;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp
{
	public class SftpWinScpTransmitter : Transmitter
	{
		public SftpWinScpTransmitter()
			: base(
				"SFTPWinSCP Transmitter",
				"1.0",
				"Custom SFTPWinSCP adapter",
				"SFTPWinSCP",
				new Guid("4EE87825-6933-42A3-856A-74BE1FCC78CB"),
				SftpWinScpAdapterManagement.SftpWinScpPropertyNamespace,
				typeof(SftpWinScpTransmitterEndpoint),
				1)
		{
		}
	}
}
