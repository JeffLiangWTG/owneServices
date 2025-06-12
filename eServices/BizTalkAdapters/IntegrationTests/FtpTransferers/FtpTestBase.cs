using System;
using System.Threading;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Simple;
using Microsoft.Samples.BizTalk.Adapter.Common;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.IntegrationTests.FtpTransferers
{
	[TestFixture]
	abstract class FtpTestBase
	{
		protected ILog log;
		protected ILoggerFactoryAdapter loggerAdapter;
		protected CargoWise.eHub.BizTalkAdapters.Common.TransferrerProperties.Receive InitReceiveConfiguration(ITransferrer ftpTransferer, string config)
		{
			this.linkedCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.localCancelTokenSource.Token);
			this.linkedCancelToken = this.linkedCancelTokenSource.Token;

			var configDom = new XmlDocument();
			configDom.LoadXml(config);

			var receiveProperties = new TransferrerProperties.Receive(ConfigProperties.IfExistsExtract(configDom, "Config/uri", null));
			receiveProperties.ReadLocationConfiguration(configDom, "ReceiveLocationPort");

			if (!String.IsNullOrWhiteSpace(ConfigProperties.IfExistsExtract(configDom, "Config/MultipleLocations", null))
				&& !String.IsNullOrWhiteSpace(ConfigProperties.IfExistsExtract(configDom, "Config/MultipleLocationsCredentials", null)))
				throw new NotSupportedException("IntegrationTest doesn't support MultipleLocations yet. Please implement it if you need.");

			var location = receiveProperties.SingleLocation;
			ftpTransferer.Server = location.Server;
			ftpTransferer.Port = location.Port;
			ftpTransferer.UserName = location.UserName;
			ftpTransferer.Password = location.Password;
			ftpTransferer.Timeout = receiveProperties.Timeout;
			ftpTransferer.Logger = log;
			ftpTransferer.CancelToken = linkedCancelToken;
			ftpTransferer.ReadLocationConfiguration(configDom);

			return receiveProperties;
		}

		[OneTimeSetUp]
		public void Init()
		{
			NameValueCollection properties = new NameValueCollection();
			properties["level"] = "Trace";
			properties["showLogName"] = "true";
			properties["showDateTime"] = "true";
			properties["dateTimeFormat"] = "yyyy/MM/dd HH:mm:ss:fff";
			loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);
			log = loggerAdapter.GetLogger(this.GetType().Name);
		}

		public CancellationTokenSource localCancelTokenSource = new CancellationTokenSource();
		public CancellationTokenSource linkedCancelTokenSource;
		public CancellationToken linkedCancelToken;
	}
}
