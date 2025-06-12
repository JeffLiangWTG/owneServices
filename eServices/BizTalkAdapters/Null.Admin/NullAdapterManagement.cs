using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.BizTalk.Component.Interop;

namespace CargoWise.eHub.BizTalkAdapters.Null.Admin
{
	public class NullAdapterManagement : 
		IAdapterConfig,
		IStaticAdapterConfig,
		IAdapterConfigValidation
	{
		#region IAdapterConfig

		public string GetConfigSchema(ConfigType configType)
		{
			if (configType == ConfigType.TransmitLocation)
			{
				using (var xsd = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.Null.Admin.NullTransmitLocation.xsd"))
				using (var rdr = new StreamReader(xsd))
					return rdr.ReadToEnd();
			}
			return null;
		}

		public Result GetSchema(string uri, string namespaceName, out string fileLocation)
		{
			fileLocation = null;
			return Result.Continue;
		}

		#endregion

		#region IStaticAdapterConfig

		public string[] GetServiceDescription(string[] wsdlReferences)
		{
			return null;
		}

		public string GetServiceOrganization(IPropertyBag endpointConfiguration, string nodeIdentifier)
		{
			return null;
		} 

		#endregion

		#region IAdapterConfigValidation

		public string ValidateConfiguration(ConfigType configType, string configuration)
		{
			if (configType != ConfigType.TransmitLocation)
				return configuration;

			var xdoc = XDocument.Parse(configuration);
			var config = xdoc.Element("Config");

			var name = config.Element("Name");
			if (name == null || String.IsNullOrWhiteSpace(name.Value))
				throw new InvalidOperationException("Name cannot be blank");

			var uriValue = new Uri("null://" + Uri.EscapeUriString(name.Value));

			var uri = config.Element("uri");
			if (uri == null)
				config.Add(uri = new XElement("uri"));
			uri.Value = uriValue.ToString();

			return xdoc.ToString(SaveOptions.DisableFormatting);
		}

		#endregion
	}
}
