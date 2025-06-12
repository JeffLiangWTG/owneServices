using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.Transferrer.UI;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin
{
	public class SftpWinScpAdapterManagement : AdapterManagement
	{
		public const string SftpWinScpScheme = "sftpwinscp";

		public override string GetConfigSchema(ConfigType configType)
		{
			switch (configType)
			{
				case ConfigType.TransmitHandler:
				case ConfigType.ReceiveHandler:
					return base.GetConfigSchema(configType);
				case ConfigType.TransmitLocation:
					return GetResolvedSchemaFromResource("CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin.SftpWinScpTransmitLocation.xsd");
				case ConfigType.ReceiveLocation:
					return GetResolvedSchemaFromResource("CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin.SftpWinScpReceiveLocation.xsd");
				default:
					return string.Empty;
			}
		}

		protected override string ValidateReceiveHandler(XmlDocument configXml) => ValidateHandlerConfig(configXml);

		protected override string ValidateTransmitHandler(XmlDocument configXml) => ValidateHandlerConfig(configXml);

		protected override string ValidateReceiveLocation(XmlDocument configXml)
			=> new SftpWinScpReceiveConfiguration(configXml, AdapterManagementUI.Instance).ToString();

		protected override string ValidateTransmitLocation(XmlDocument configXml) => new SftpWinScpTransmitConfiguration(configXml).ToString();

		public const string SftpWinScpPropertyNamespace = "http://cargowise.com/ehub/biztalkadapters/sftpwinscp-properties";

		internal static void ExtractSftpConfiguration(XmlDocument configXml, ISftpWinScpConfiguration config)
		{
			config.HostKeyFingerprint = ConfigProperties.IfExistsExtract(configXml, "/Config/HostKeyFingerprint", null) is string hostKeyFingerprint && hostKeyFingerprint.Length > 0 ? hostKeyFingerprint : null;
			config.PrivateKey = ConfigProperties.IfExistsExtract(configXml, "/Config/PrivateKey", null) is string privateKey && privateKey.Length > 0 ? privateKey : null;
			config.PrivateKeyPassphrase = ConfigProperties.IfExistsExtract(configXml, "/Config/PrivateKeyPassphrase", null) is string privateKeyPassphrase && privateKeyPassphrase.Length > 0 ? privateKeyPassphrase : null;
			config.UseRealPath = ConfigProperties.IfExistsExtractBool(configXml, "/Config/UseRealPath", true);
		}
	}
}
