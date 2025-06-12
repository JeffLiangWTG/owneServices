using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public abstract class WinScpTransmitterEndpoint : TransmitterEndpoint
	{
		private const string ContextConfigurationPropertyNamespace = "http://cargowise.com/ehub/biztalkadapters/transferrer-properties";

		protected WinScpTransmitterEndpoint(AsyncTransmitter asyncTransmitter, ILoggerFactory loggerFactory, IWinScpClientFactory winScpClientFactory, Type configurationType)
			: base(asyncTransmitter, loggerFactory)
		{
			this.winScpClientFactory = winScpClientFactory;
			this.configurationType = configurationType;
		}

		protected override IBaseMessage TransmitMessage(IBaseMessage message, XmlDocument configXml, ILog log, string activityId, CancellationToken cancelToken)
		{
			log.Log(activityId, LogLevel.Debug, "Start TransmitMessageAsync");
			ApplyConfigDynamicOverrides(message, configNamespace, configXml);

			var useContext = ConfigProperties.IfExistsExtractBool(configXml, "/Config/UseContextConfiguration", false);
			if (useContext)
			{
				ApplyConfigDynamicOverrides(message, ContextConfigurationPropertyNamespace, configXml);
			}

			log.Log(activityId, LogLevel.Trace, "Configuration: {0}", configXml.OuterXml);
			var config = (WinScpTransmitConfiguration)Activator.CreateInstance(configurationType, configXml);
			var portName = (string)message.Context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");

			bool enteredSemaphore = false;
			IWinScpClient winScpClient = null;
			try
			{
				var fileNames = ReplaceFileNameMacros(new[] { config.Location.FileName, config.TemporaryFileName, config.FlagFile }, message);
				config.Location.FileName = fileNames[0];
				if (!string.IsNullOrWhiteSpace(config.TemporaryFileName))
					config.TemporaryFileName = fileNames[1];
				if (!string.IsNullOrWhiteSpace(config.FlagFile))
					config.FlagFile = fileNames[2];
				log.Log(activityId, LogLevel.Info, "Transmitting to: {0}", config.Location);
				HandlerCancelToken.ThrowIfCancellationRequested();

				if (config.ConnectionLimit > 0)
				{
					connectionLimiter ??= InitialiseConnectionLimiter(config.ConnectionLimit);
					log.Log(activityId, LogLevel.Debug, "Acquiring connection semaphore. Available slots = {0}/{1}",
						connectionLimiter.CurrentCount, config.ConnectionLimit);
					using var startCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(HandlerCancelToken);
					startCancelTokenSource.CancelAfter(config.Timeout);
					connectionLimiter.Wait(startCancelTokenSource.Token);
					enteredSemaphore = true;
				}

				log.Log(activityId, LogLevel.Trace, "Creating client for location: {0}", config.Location);
				winScpClient = winScpClientFactory.CreateClient(config.Location, config, log, activityId);
				HandlerCancelToken.ThrowIfCancellationRequested();

				log.Log(activityId, LogLevel.Debug, "Opening client for location: {0}", config.Location);
				winScpClient.Open();
				HandlerCancelToken.ThrowIfCancellationRequested();

				if (string.IsNullOrWhiteSpace(config.TemporaryFolder) && string.IsNullOrWhiteSpace(config.TemporaryFileName))
				{
					log.Log(activityId, LogLevel.Debug, "Putting message to: {0}", config.Location);
					winScpClient.PutFile(message.BodyPart.GetOriginalDataStream(), config.Location);
					log.Log(activityId, LogLevel.Info, "Successfully put message to: {0}", config.Location);
				}
				else
				{
					var tempLocn = config.Location.Clone();
					if (!string.IsNullOrWhiteSpace(config.TemporaryFolder))
						tempLocn.Folder = config.TemporaryFolder;
					if (!string.IsNullOrWhiteSpace(config.TemporaryFileName))
						tempLocn.FileName = config.TemporaryFileName;
					log.Log(activityId, LogLevel.Debug, "Putting message to temporary location: {0}", tempLocn);
					winScpClient.PutFile(message.BodyPart.GetOriginalDataStream(), tempLocn);
					log.Log(activityId, LogLevel.Debug, "Moving from temporary location '{0}' to: {1}", tempLocn.GetPath(), config.Location);
					winScpClient.MoveFile(tempLocn, config.Location);
					log.Log(activityId, LogLevel.Info, "Successfully put message to: {0}", config.Location);
				}
				if (!string.IsNullOrWhiteSpace(config.FlagFile))
				{
					var flagLocn = config.Location.Clone();
					flagLocn.FileName = config.FlagFile;
					log.Log(activityId, LogLevel.Debug, "Putting flag file to: {0}", flagLocn);
					using (var flagStream = new MemoryStream())
						winScpClient.PutFile(flagStream, flagLocn);
					log.Log(activityId, LogLevel.Info, "Successfully put flag file to: {0}", flagLocn);
				}

				log.Log(activityId, LogLevel.Debug, "Closing client for location: {0}", config.Location);
				winScpClient.Close();

				return null;
			}
			finally
			{
				winScpClient?.Dispose();
				if (enteredSemaphore)
				{
					connectionLimiter.Release();
					log.Log(activityId, LogLevel.Debug, "Released connection semaphore. Available slots = {0}/{1}",
						connectionLimiter.CurrentCount, config.ConnectionLimit);
				}
				log.Log(activityId, LogLevel.Debug, "Finish TransmitMessageAsync");
			}
		}

		private SemaphoreSlim InitialiseConnectionLimiter(int connectionLimit)
		{
			lock (connectionLimiterLock)
			{
				return connectionLimiter ??= new SemaphoreSlim(connectionLimit, connectionLimit);
			}
		}

		private readonly IWinScpClientFactory winScpClientFactory;
		private readonly Type configurationType;
		private SemaphoreSlim connectionLimiter;
		private readonly object connectionLimiterLock = new object();
	}
}
