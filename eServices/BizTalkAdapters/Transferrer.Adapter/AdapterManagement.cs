using System;
using System.Xml;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public abstract class AdapterManagement :
		AdapterManagementBase,
		IAdapterConfig,
		IAdapterConfigValidation
	{
		protected abstract string Scheme();

		public abstract string GetConfigSchema(ConfigType configType);

		public Result GetSchema(string uri, string namespaceName, out string fileLocation)
		{
			fileLocation = string.Empty;
			return Result.Continue;
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
					return String.Empty;
			}
		}

		protected virtual string ValidateTransmitHandler(XmlDocument configXml)
		{
			return configXml.OuterXml;
		}

		protected virtual string ValidateReceiveHandler(XmlDocument configXml)
		{
			return configXml.OuterXml;
		}

		protected virtual string ValidateTransmitLocation(XmlDocument configXml)
		{
			return configXml.OuterXml;
		}

		protected virtual string ValidateReceiveLocation(XmlDocument configXml)
		{
			return configXml.OuterXml;
		}

		protected void SetNamedUri(XmlDocument configXml)
		{
			string name = ConfigProperties.IfExistsExtract(configXml, "/Config/Name", null);
			if (name != null)
			{
				var uriValue = new Uri(Scheme() + "://" + Uri.EscapeUriString(name));
				AddOrUpdate(configXml, "uri", uriValue.ToString());
			}
		}

		protected static void AddOrUpdate(XmlDocument configXml, string name, string value)
		{
			var configNode = configXml.SelectSingleNode("/Config");
			var itemNode = configNode.SelectSingleNode(name);
			if (itemNode == null)
			{
				itemNode = configXml.CreateElement(name);
				configNode.AppendChild(itemNode);
			}
			itemNode.InnerText = value;
		}
	}
}
