using Microsoft.Extensions.Configuration;

namespace OcmPoc.Transceivers
{
	public abstract class BaseConnectionBuilder<TConfig> : IConnectionBuilder
		where TConfig : ConnectionConfigBase
    {
		public BaseConnectionBuilder(TConfig config)
		{
			Configuration = config;
		}

		public TConfig Configuration { get; }

		public void BindConfiguration(IConfigurationRoot configRoot)
		{
			configRoot.GetSection("Connection").Bind(Configuration);
		}

		public abstract IReceivingConnection BuildReceivingConnection();

		public abstract ISendingConnection BuildSendingConnection();
	}
}
