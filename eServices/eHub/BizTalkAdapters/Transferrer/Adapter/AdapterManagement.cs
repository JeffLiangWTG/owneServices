using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public abstract class AdapterManagement :
		AdapterManagementBase,
		IAdapterConfig,
		IAdapterConfigValidation
	{
		public Result GetSchema(string uri, string namespaceName, out string fileLocation)
		{
			fileLocation = string.Empty;
			return Result.Continue;
		}

		public virtual string GetConfigSchema(ConfigType configType)
		{
			switch (configType)
			{
				case ConfigType.TransmitHandler:
				case ConfigType.ReceiveHandler:
					return GetResolvedSchemaFromAssemblyPath("HandlerConfiguration.xsd");
				default:
					return string.Empty;
			}
		}

		public string ValidateConfiguration(ConfigType configType, string configuration)
		{
			XmlDocument configXml = new XmlDocument();
			configXml.LoadXml(configuration);

			switch (configType)
			{
				case ConfigType.TransmitHandler:
					return ValidateTransmitHandler(configXml);
				case ConfigType.TransmitLocation:
					return ValidateTransmitLocation(configXml);
				case ConfigType.ReceiveHandler:
					return ValidateReceiveHandler(configXml);
				case ConfigType.ReceiveLocation:
					return ValidateReceiveLocation(configXml);
				default:
					return string.Empty;
			}
		}

		protected virtual string GetResolvedSchemaFromResource(string name)
		{
			var assemblyPath = GetAssemblyPath();
			var document = new XmlDocument();
			document.LoadXml(GetSchemaFromResource(name));
			return GetResolvedSchema(document, assemblyPath);
		}

		private string GetResolvedSchemaFromAssemblyPath(string name)
		{
			var assemblyPath = GetAssemblyPath();
			var document = new XmlDocument();
			document.Load(Path.Combine(assemblyPath, name));
			return GetResolvedSchema(document, assemblyPath);
		}

		private string GetResolvedSchema(XmlDocument document, string assemblyPath)
		{
			ResolveSchemaIncludes(document, assemblyPath);
			AddPathToEditorAssembly(document, assemblyPath);
			return MakeString(document);
		}

		private string GetAssemblyPath() => $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{Path.DirectorySeparatorChar}";

		private void ResolveSchemaIncludes(XmlDocument document, string assemblyPath)
		{
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
			nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

			foreach (XmlNode includeNode in document.DocumentElement.SelectNodes("xs:include", nsmgr))
			{
				var schemaLocation = includeNode.Attributes["schemaLocation"];
				var schemaFile = new FileInfo(Path.Combine(assemblyPath, Path.GetFileName(schemaLocation.Value)));
				if (schemaFile.Exists)
				{
					document.DocumentElement.RemoveChild(includeNode);
					var includeDocument = new XmlDocument();
					includeDocument.Load(schemaFile.FullName);
					foreach (var node in includeDocument.DocumentElement.ChildNodes.Cast<XmlNode>().Where(n => n?.LocalName != "import"))
					{
						document.DocumentElement.AppendChild(document.ImportNode(node, true));
					}
				}
			}
		}

		protected virtual string ValidateTransmitHandler(XmlDocument configXml) => ValidateHandlerConfig(configXml);

		protected virtual string ValidateReceiveHandler(XmlDocument configXml) => ValidateHandlerConfig(configXml);

		protected virtual string ValidateTransmitLocation(XmlDocument configXml) => configXml.OuterXml;

		protected virtual string ValidateReceiveLocation(XmlDocument configXml) => configXml.OuterXml;

		protected string ValidateHandlerConfig(XmlDocument configXml)
		{
			string intLogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/InterfacesLogDir", DefaultHandlerInterfacesLogDir);
			if (!Uri.TryCreate(intLogDir, UriKind.Absolute, out var uriParse) || uriParse.Scheme != "file")
				throw new AdapterException("Interfaces Log Directory is not a valid path.");

			string structuredLogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/StructuredLogDir", DefaultHandlerStructuredLogDir);
			if (!Uri.TryCreate(structuredLogDir, UriKind.Absolute, out uriParse) || uriParse.Scheme != "file")
				throw new AdapterException("Structured Interfaces Log Directory is not a valid path.");

			int terminateWaitLimit = ConfigProperties.IfExistsExtractInt(configXml, "/Config/TerminateWaitLimit", DefaultHandlerTerminateWaitLimit);
			if (terminateWaitLimit < 0)
				throw new AdapterException("Terminate Wait Limit cannot be negative.");

			if (!Enum.TryParse<TransferrerLogLevel>(ConfigProperties.IfExistsExtract(configXml, "/Config/LogLevel", DefaultHandlerLogLevel.ToString()), out _))
				throw new AdapterException("Log Level not a valid value.");

			string logDir = ConfigProperties.IfExistsExtract(configXml, "/Config/LogDir", DefaultHandlerLogDir);
			if (!Uri.TryCreate(logDir, UriKind.Absolute, out uriParse) || uriParse.Scheme != "file")
				throw new AdapterException("Log Directory is not a valid path.");

			int logMaxSize = ConfigProperties.IfExistsExtractInt(configXml, "/Config/LogMaxSize", DefaultHandlerLogMaxSize);
			if (logMaxSize <= 0)
				throw new AdapterException("Log Max File Size (MB) must be positive.");

			int logMaxCount = ConfigProperties.IfExistsExtractInt(configXml, "/Config/LogMaxCount", DefaultHandlerLogMaxCount);
			if (logMaxCount <= 0)
				throw new AdapterException("Log Max Rollover Count must be positive.");

			return configXml.OuterXml;
		}

		public static XmlDocument DefaultHandlerConfigDom()
		{
			var configDom = new XmlDocument();
			configDom.LoadXml(new XElement("Config",
				new XElement("InterfacesLogDir", DefaultHandlerInterfacesLogDir),
				new XElement("StructuredLogDir", DefaultHandlerStructuredLogDir),
				new XElement("TerminateWaitLimit", DefaultHandlerTerminateWaitLimit),
				new XElement("LogDir", DefaultHandlerLogDir),
				new XElement("LogMaxSize", DefaultHandlerLogMaxSize),
				new XElement("LogMaxCount", DefaultHandlerLogMaxCount)
				).ToString());
			return configDom;
		}

		public static XmlDocument EmptyConfigDom()
		{
			var configDom = new XmlDocument();
			configDom.LoadXml(new XElement("Config").ToString());
			return configDom;
		}

		public const string DefaultHandlerInterfacesLogDir = @"C:\Logs\BizTalk\Interfaces";
		public const string DefaultHandlerStructuredLogDir = @"C:\Logs\Structured\Interfaces";
		public const int DefaultHandlerTerminateWaitLimit = 60000;
		public const string DefaultHandlerLogDir = @"C:\Logs\BizTalk\Handlers";
		public const int DefaultHandlerLogMaxSize = 2;
		public const int DefaultHandlerLogMaxCount = 10;
		public const TransferrerLogLevel DefaultHandlerLogLevel = TransferrerLogLevel.Info;
	}
}
