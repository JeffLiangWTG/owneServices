using CargoWise.eHub.BizTalkAdapters.WinScp.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin
{
	public interface IFtpWinScpConfiguration : IWinScpConfiguration
	{
		string Mode { get; set; }
		string FtpsMode { get; set; }
		bool ValidateServerCert { get; set; }
		string ClientCertPath { get; set; }
		string HostCertificateThumbprint { get; set; }
	}
}
