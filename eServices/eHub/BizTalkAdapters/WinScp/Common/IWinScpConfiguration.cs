using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public interface IWinScpConfiguration : ILoggerConfiguration
	{
		string Uri { get; set; }
		string Server { get; set; }
		int? Port { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		int Timeout { get; set; }

		string ToString();
	}
}
