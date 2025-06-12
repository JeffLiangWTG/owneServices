using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.BizTalkAdapters.Transferrer.UI;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Ftp.Admin
{
	[XmlRoot("Config")]
	public class FtpWinScpReceiveConfiguration : WinScpReceiveConfiguration, IFtpWinScpConfiguration
	{
		public string Mode { get; set; }
		public string FtpsMode { get; set; }
		public bool ValidateServerCert { get; set; }
		public string ClientCertPath { get; set; }
		public string HostCertificateThumbprint { get; set; }

		public FtpWinScpReceiveConfiguration() {}

		public FtpWinScpReceiveConfiguration(XmlDocument configXml)
			: this(configXml, null) { }

		public FtpWinScpReceiveConfiguration(XmlDocument configXml, IAdapterManagementUI adapterManagementUI)
			: base(configXml, FtpWinScpAdapterManagement.FtpWinScpScheme, adapterManagementUI)
		{
			FtpWinScpAdapterManagement.ExtractFtpConfiguration(configXml, this);
		}
	}
}
