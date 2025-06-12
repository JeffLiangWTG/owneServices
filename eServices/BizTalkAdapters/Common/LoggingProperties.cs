using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class LoggingProperties : ConfigProperties
	{
		static ConcurrentDictionary<string, string> savedConfigurations = new ConcurrentDictionary<string, string>();

		public bool LoggingEnabled { get; private set; }
		public string LogFolder { get; private set; }
		public int LogMaxSize { get; private set; }
		public int LogMaxCount { get; private set; }
		public string LogLevel { get; private set; }
		public virtual ILog Logger { get; internal set; }
		public XmlDocument ConfigDom { get; private set; }
		public string PortName { get; private set; }

		public LoggingProperties()
		{
			UpdateLogger();
		}

		public virtual void ReadConfiguration(IBaseMessage message, XmlDocument configDOM, string portName)
		{
			ReadLocationConfiguration(configDOM, portName);
		}

		public virtual void ReadLocationConfiguration(XmlDocument configDOM, string portName, CancellationToken cancelToken = default)
		{
			this.PortName = portName;
			this.ConfigDom = configDOM;
			this.LoggingEnabled = IfExistsExtractBool(configDOM, "Config/Logging", true);
			this.LogFolder = IfExistsExtract(configDOM, "Config/LogDir", @"C:\Logs\BizTalk\Interfaces\{prefix,3}");
			this.LogMaxSize = IfExistsExtractInt(configDOM, "Config/LogMaxSize", 2);
			this.LogMaxCount = IfExistsExtractInt(configDOM, "Config/LogMaxCount", 10);
			this.LogLevel = IfExistsExtract(configDOM, "Config/LogLevel", "Info");

			UpdateLogger();
		}

		void UpdateLogger()
		{
			if (this.LoggingEnabled)
			{
                // Create a unique and meaningful filename
                string logName = "";
                if (!String.IsNullOrWhiteSpace(this.PortName))
                {
                    logName += this.PortName + ".";
                }
                if (Process.GetCurrentProcess().ProcessName.StartsWith("BTSNTSvc")) // is running in a host instance
                {
                    // The host instance name, listed as the handler in the send/receive port properties dialog.
                    var hostInstance = Environment.GetCommandLineArgs()[4];
                    logName += hostInstance + ".";
                }
                logName += this.GetType().Name;

				string savedConfig = null;
				string currentConfig = ConfigDom != null ? ConfigDom.InnerXml : null;
				if (savedConfigurations.TryGetValue(logName, out savedConfig) && savedConfig == currentConfig)
				{
					try
					{
						this.Logger = LogManager.GetLogger(logName);
					}
					catch (Exception)
					{
						this.Logger = new NoOpLogger();
					}
				}
				else
				{
					savedConfigurations[logName] = ConfigDom.InnerXml;
					this.Logger = TransferrerHelpers.CreatePortLogger(logName, this.LogFolder, this.LogLevel, this.LogMaxSize, this.LogMaxCount);
				}
			}
			else
			{
				this.Logger = new NoOpLogger();
			}
		}
	}
}
