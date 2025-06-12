using System;
using System.Xml;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class LoggerConfiguration : ILoggerConfiguration
	{
		public TransferrerLogLevel LogLevel { get; set; } = TransferrerLogLevel.Info;
		public string LogDir { get; set; }
		public string StructuredLogDir { get; set; }
		public int? LogMaxSize { get; set; } = DefaultLogMaxSize;
		public int? LogMaxCount { get; set; } = DefaultLogMaxCount;
		public TransferrerLogFormat LogFormat { get; set; } = TransferrerLogFormat.Flat;

		public const int DefaultLogMaxSize = 2;
		public const int DefaultLogMaxCount = 10;

		public LoggerConfiguration(XmlDocument configXml)
		{
			ExtractLoggerConfiguration(configXml, this);
		}

		internal static void ExtractLoggerConfiguration(XmlDocument configXml, ILoggerConfiguration config)
		{
			config.LogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/LogDir", null);
			config.StructuredLogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/StructuredLogDir", null);
			config.LogLevel = (TransferrerLogLevel)Enum.Parse(typeof(TransferrerLogLevel), ConfigProperties.IfExistsExtract(configXml, "/Config/LogLevel", "Off"));
			config.LogMaxSize = ConfigProperties.IfExistsExtractInt(configXml, "/Config/LogMaxSize", DefaultLogMaxSize);
			config.LogMaxCount = ConfigProperties.IfExistsExtractInt(configXml, "/Config/LogMaxCount", DefaultLogMaxCount);
			config.LogFormat = (TransferrerLogFormat)Enum.Parse(typeof(TransferrerLogFormat),
				ConfigProperties.IfExistsExtract(configXml, "/Config/LogFormat", "Flat"));
		}
	}
}
