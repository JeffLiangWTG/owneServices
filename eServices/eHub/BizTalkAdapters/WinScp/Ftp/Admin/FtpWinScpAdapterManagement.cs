using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.Transferrer.UI;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin
{
	public class FtpWinScpAdapterManagement : AdapterManagement
	{
		public const string FtpWinScpScheme = "ftpwinscpv2";
		public const string FtpWinScpPropertyNamespace = "http://cargowise.com/ehub/biztalkadapters/ftpwinscpv2-properties";

		public override string GetConfigSchema(ConfigType configType)
		{
			switch (configType)
			{
				case ConfigType.TransmitHandler:
				case ConfigType.ReceiveHandler:
					return base.GetConfigSchema(configType);
				case ConfigType.TransmitLocation:
					return GetResolvedSchemaFromResource("CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin.FtpWinScpTransmitLocation.xsd");
				case ConfigType.ReceiveLocation:
					return GetResolvedSchemaFromResource("CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin.FtpWinScpReceiveLocation.xsd");
				default:
					return string.Empty;
			}
		}

		protected override string ValidateReceiveLocation(XmlDocument configXml) => new FtpWinScpReceiveConfiguration(configXml, AdapterManagementUI.Instance).ToString();

		protected override string ValidateTransmitLocation(XmlDocument configXml) => new FtpWinScpTransmitConfiguration(configXml).ToString();

		internal static void ExtractFtpConfiguration(XmlDocument configXml, IFtpWinScpConfiguration config)
		{
			config.Mode = ConfigProperties.IfExistsExtract(configXml, "/Config/Mode", null) is string mode && mode.Length > 0
				? mode : null;
			config.FtpsMode = ConfigProperties.IfExistsExtract(configXml, "/Config/FtpsMode", null) is string ftpsMode && ftpsMode.Length > 0
				? ftpsMode : null;
			config.ValidateServerCert = ConfigProperties.IfExistsExtractBool(configXml, "/Config/ValidateServerCert", false);
			config.ClientCertPath = ConfigProperties.IfExistsExtract(configXml, "/Config/ClientCertPath", null) is string clientCertPath && clientCertPath.Length > 0
				? clientCertPath : string.Empty;
			config.HostCertificateThumbprint = ConfigProperties.IfExistsExtract(configXml, "/Config/HostCertificateThumbprint", null) is string hostCertificateThumbprint && hostCertificateThumbprint.Length > 0
				? hostCertificateThumbprint : string.Empty;
		}
	}
}
