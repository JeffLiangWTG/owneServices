using System;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx.Admin
{
	public class HttpExAdapterManagement : AdapterManagement
	{
		public override string GetConfigSchema(ConfigType configType)
		{
			switch (configType)
			{
				case ConfigType.TransmitHandler:
					return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.HttpEx.Admin.HttpExTransmitHandler.xsd");
				case ConfigType.TransmitLocation:
					return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.HttpEx.Admin.HttpExTransmitLocation.xsd");
				default:
					return string.Empty;
			}
		}

		protected override string ValidateTransmitHandler(XmlDocument configXml)
		{
			string intLogDir = ConfigProperties.Extract(configXml, "/Config/InterfacesLogDir", null);
			if (!Uri.TryCreate(intLogDir, UriKind.Absolute, out var uriParse) || uriParse.Scheme != "file")
				throw new AdapterException("Interfaces Log Directory is not a valid path.");

			string structuredLogDir = ConfigProperties.Extract(configXml, "/Config/StructuredLogDir", null);
			if (!Uri.TryCreate(structuredLogDir, UriKind.Absolute, out uriParse) || uriParse.Scheme != "file")
				throw new AdapterException("Structured Interfaces Log Directory is not a valid path.");

			int terminateWaitLimit = ConfigProperties.ExtractInt(configXml, "/Config/TerminateWaitLimit");
			if (terminateWaitLimit < 0)
				throw new AdapterException("Terminate Wait Limit cannot be negative.");

			string logDir = ConfigProperties.Extract(configXml, "/Config/LogDir", null);
			if (!Uri.TryCreate(logDir, UriKind.Absolute, out uriParse) || uriParse.Scheme != "file")
				throw new AdapterException("Log Directory is not a valid path.");

			int logMaxSize = ConfigProperties.ExtractInt(configXml, "/Config/LogMaxSize");
			if (logMaxSize <= 0)
				throw new AdapterException("Log Max File Size (MB) must be positive.");

			int logMaxCount = ConfigProperties.ExtractInt(configXml, "/Config/LogMaxCount");
			if (logMaxCount <= 0)
				throw new AdapterException("Log Max Rollover Count must be positive.");

			string logLevel = ConfigProperties.Extract(configXml, "/Config/LogLevel", null);
			if (logLevel != "Info" && logLevel != "Debug" && logLevel != "Trace")
				throw new AdapterException("Log Level not a valid value.");

			return base.ValidateTransmitHandler(configXml);
		}

		protected override string ValidateTransmitLocation(System.Xml.XmlDocument configXml)
		{
			var config = HttpExConfiguration.Parse(configXml);
			return config.ToString();
		}
	}
}
