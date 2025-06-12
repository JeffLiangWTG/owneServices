using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin;
using Common.Logging;
using WinSCP;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp
{
	public class FtpWinScpClient : WinScpClient
	{
		public FtpWinScpClient(WinScpLocation location, IFtpWinScpConfiguration config, ILog logger, string activityId) : base(location, config, logger, activityId)
		{
			SessionOptions.Protocol = Protocol.Ftp;
			SessionOptions.FtpMode = config.Mode == "Active" ? FtpMode.Active : FtpMode.Passive;
			SessionOptions.FtpSecure = (FtpSecure)Enum.Parse(typeof(FtpSecure), config.FtpsMode);
			SessionOptions.GiveUpSecurityAndAcceptAnyTlsHostCertificate = !config.ValidateServerCert;
			SessionOptions.TlsClientCertificatePath = string.IsNullOrWhiteSpace(config.ClientCertPath) ? null : config.ClientCertPath;
			SessionOptions.TlsHostCertificateFingerprint = string.IsNullOrWhiteSpace(config.HostCertificateThumbprint) ? null : config.HostCertificateThumbprint;
		}

		public class Factory : IWinScpClientFactory
		{
			public static IWinScpClientFactory Instance { get; } = new Factory();

			public IWinScpClient CreateClient(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId)
				=> new FtpWinScpClient(location, (IFtpWinScpConfiguration)config, logger, activityId);

			public Task<IWinScpClient> CreateClientAsync(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId, CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				return Task.FromResult<IWinScpClient>(new FtpWinScpClient(location, (IFtpWinScpConfiguration)config, logger, activityId));
			}
		}
	}
}
