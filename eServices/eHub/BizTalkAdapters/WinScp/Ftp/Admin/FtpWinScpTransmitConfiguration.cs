using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin
{
	[XmlRoot("Config")]
	public class FtpWinScpTransmitConfiguration : WinScpTransmitConfiguration, IFtpWinScpConfiguration
	{
		public string Mode { get; set; }
		public string FtpsMode { get; set; }
		public bool ValidateServerCert { get; set; }
		public string ClientCertPath { get; set; }
		public string HostCertificateThumbprint { get; set; }

		public FtpWinScpTransmitConfiguration() {}

		public FtpWinScpTransmitConfiguration(XmlDocument configXml) : base(configXml, FtpWinScpAdapterManagement.FtpWinScpScheme)
		{
			FtpWinScpAdapterManagement.ExtractFtpConfiguration(configXml, this);
		}
	}
}
