using CargoWise.eHub.BizTalkAdapters.WinScp.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin
{
	public interface ISftpWinScpConfiguration : IWinScpConfiguration
	{
		string HostKeyFingerprint { get; set; }
		string PrivateKey { get; set; }
		string PrivateKeyPassphrase { get; set; }
		bool UseRealPath { get; set; }
	}
}
