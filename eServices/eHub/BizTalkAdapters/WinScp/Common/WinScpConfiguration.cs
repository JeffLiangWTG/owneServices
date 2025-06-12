using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public abstract class WinScpConfiguration : IWinScpConfiguration
	{
		[XmlElement("uri")]
		public string Uri { get; set; }
		public string Server { get; set; }
		public int? Port { get; set; }
		public bool PortSpecified => Port.HasValue;
		public string UserName { get; set; }
		public string Password { get; set; }
		public int Timeout { get; set; }
		public TransferrerLogLevel LogLevel { get; set; }
		public string LogDir { get; set; }
		public string StructuredLogDir { get; set; }
		public int? LogMaxSize { get; set; }
		public bool LogMaxSizeSpecified => LogMaxSize.HasValue;
		public int? LogMaxCount { get; set; }
		public bool LogMaxCountSpecified => LogMaxSize.HasValue;
		public TransferrerLogFormat LogFormat { get; set; }

		protected WinScpConfiguration() { }

		protected WinScpConfiguration(XmlDocument configXml)
		{
			Server = ConfigProperties.IfExistsExtract(configXml, "/Config/Server", null);
			Port = ConfigProperties.IfExistsExtractInt(configXml, "/Config/Port", -1) is int port && port >= 0 ? port : (int?)null;
			UserName = ConfigProperties.IfExistsExtract(configXml, "/Config/UserName", null);
			Password = ConfigProperties.IfExistsExtract(configXml, "/Config/Password", null);
			Timeout = ConfigProperties.IfExistsExtractInt(configXml, "/Config/Timeout", 90000);
			LogLevel = (TransferrerLogLevel)Enum.Parse(typeof(TransferrerLogLevel), ConfigProperties.IfExistsExtract(configXml, "/Config/LogLevel", TransferrerLogLevel.Off.ToString()));
			LogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/LogDir", null);
			StructuredLogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/StructuredLogDir", null);
			LogMaxSize = ConfigProperties.IfExistsExtractInt(configXml, "/Config/LogMaxSize", -1) is int logMaxSize && logMaxSize >= 0 ? logMaxSize : (int?)null;
			LogMaxCount = ConfigProperties.IfExistsExtractInt(configXml, "/Config/LogMaxCount", -1) is int logMaxCount && logMaxCount >= 0 ? logMaxCount : (int?)null;
			LogFormat = (TransferrerLogFormat)Enum.Parse(typeof(TransferrerLogFormat), ConfigProperties.IfExistsExtract(configXml, "/Config/LogFormat", TransferrerLogFormat.Flat.ToString()));
		}

		public override string ToString()
		{
			var serializer = new XmlSerializer(GetType());
			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add("", "");
			using (var stringWriter = new StringWriter())
			using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { OmitXmlDeclaration = true }))
			{
				serializer.Serialize(xmlWriter, this, namespaces);
				xmlWriter.Flush();
				return stringWriter.ToString();
			}
		}
	}
}
