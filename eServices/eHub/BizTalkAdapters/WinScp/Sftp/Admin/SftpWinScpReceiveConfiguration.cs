using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.BizTalkAdapters.Transferrer.UI;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin
{
	[XmlRoot("Config")]
	public class SftpWinScpReceiveConfiguration : WinScpReceiveConfiguration, ISftpWinScpConfiguration
	{
		public string HostKeyFingerprint { get; set; }
		public string PrivateKey { get; set; }
		public string PrivateKeyPassphrase { get; set; }
		public bool UseRealPath { get; set; } = true;

		public SftpWinScpReceiveConfiguration() { }

		public SftpWinScpReceiveConfiguration(XmlDocument configXml)
			: this(configXml, null) { }

		public SftpWinScpReceiveConfiguration(XmlDocument configXml, IAdapterManagementUI adapterManagementUI)
			: base(configXml, SftpWinScpAdapterManagement.SftpWinScpScheme, adapterManagementUI)
		{
			SftpWinScpAdapterManagement.ExtractSftpConfiguration(configXml, this);
		}
	}
}
