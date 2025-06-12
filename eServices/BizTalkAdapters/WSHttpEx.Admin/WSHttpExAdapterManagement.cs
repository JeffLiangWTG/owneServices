using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx.Admin
{
	public class WSHttpExAdapterManagement : AdapterManagementBase, IAdapterConfig, IStaticAdapterConfig, IAdapterConfigValidation
	{
		public string GetConfigSchema(ConfigType type)
		{
			switch (type)
			{
				case ConfigType.TransmitLocation:
					return GetSchemaFromResource("CargoWise.eHub.BizTalkAdapters.WSHttpEx.Admin.WSHttpExTransmitLocation.xsd");

				default:
					return null;
			}
		}

		public string[] GetServiceDescription(string[] wsdls)
		{
			return null;
		}

		public string GetServiceOrganization(IPropertyBag endPointConfiguration,
												string NodeIdentifier)
		{
			return null;
		}

		public Result GetSchema(string xsdLocation,
								string xsdNamespace,
								out string xsdSchema)
		{
			xsdSchema = null;
			return Result.Continue;
		}

		public string ValidateConfiguration(ConfigType configType,
			string xmlInstance)
		{
			string validXml = String.Empty;

			switch (configType)
			{
				case ConfigType.TransmitHandler:
					validXml = xmlInstance;
					break;

				case ConfigType.TransmitLocation:
					validXml = ValidateTransmitLocation(xmlInstance);
					break;
			}

			return validXml;
		}

		string ValidateTransmitLocation(string xmlInstance)
		{
			var document = new XmlDocument();
			document.LoadXml(xmlInstance);

			var builder = new StringBuilder();

			var destinationUrl = document.SelectSingleNode("Config/destinationUrl");

			if (destinationUrl == null || destinationUrl.InnerText == String.Empty)
				throw new ApplicationException("Transport properties validation failed.  Value for required adapter property \"Destination Url\" is not specified.");

			if (!destinationUrl.InnerText.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !destinationUrl.InnerText.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
				throw new ApplicationException("The Url must start with HTTP:// or HTTPS://");

			var uri = document.SelectSingleNode("Config/uri");
			if (null == uri)
			{
				uri = document.CreateElement("uri");
				document.DocumentElement.AppendChild(uri);
			}
			uri.InnerText = destinationUrl.InnerText;

			var inboundBodyPathExpression = document.SelectSingleNode("Config/inboundBodyPathExpression");
			if (inboundBodyPathExpression != null && inboundBodyPathExpression.InnerText != String.Empty)
			{
				var inboundBodyLocation = document.SelectSingleNode("Config/inboundBodyLocation");
				if (inboundBodyLocation == null || inboundBodyLocation.InnerText != "2")
				{
					throw new ApplicationException("'Inbound Body Path Expression' can only be specified if 'BodyPath' has been selected as the value for the 'Inbound Body Message Source'.");
				}
			}

			return document.OuterXml;
		}
	}
}
