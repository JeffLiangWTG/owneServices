using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin;
using Common.Logging;
using WinSCP;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp
{
	public class SftpWinScpClient : WinScpClient
	{
		public SftpWinScpClient(WinScpLocation location, ISftpWinScpConfiguration config, ILog logger, string activityId)
			: base(location, config, logger, activityId)
		{
			SessionOptions.Protocol = Protocol.Sftp;
			SessionOptions.SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny;
			if (!string.IsNullOrWhiteSpace(config.HostKeyFingerprint))
				SessionOptions.SshHostKeyFingerprint = config.HostKeyFingerprint;
			else
				SessionOptions.SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny;
			if (!string.IsNullOrWhiteSpace(config.PrivateKey))
				SessionOptions.SshPrivateKey = config.PrivateKey;
			if (!string.IsNullOrWhiteSpace(config.PrivateKeyPassphrase))
				SessionOptions.PrivateKeyPassphrase = config.PrivateKeyPassphrase;
			if (!config.UseRealPath)
				SessionOptions.AddRawSettings("SFTPRealPath", "off");
		}

		public class Factory : IWinScpClientFactory
		{
			public static IWinScpClientFactory Instance { get; } = new Factory();

			public IWinScpClient CreateClient(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId)
				=> new SftpWinScpClient(location, (ISftpWinScpConfiguration)config, logger, activityId);

			public Task<IWinScpClient> CreateClientAsync(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId, CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				return Task.FromResult<IWinScpClient>(new SftpWinScpClient(location, (ISftpWinScpConfiguration)config, logger, activityId));
			}
		}
	}
}
