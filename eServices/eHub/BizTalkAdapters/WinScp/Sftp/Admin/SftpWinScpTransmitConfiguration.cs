using System.Xml;
using System.Xml.Serialization;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Sftp.Admin
{
	[XmlRoot("Config")]
	public class SftpWinScpTransmitConfiguration : WinScpTransmitConfiguration, ISftpWinScpConfiguration
	{
		public string HostKeyFingerprint { get; set; }
		public string PrivateKey { get; set; }
		public string PrivateKeyPassphrase { get; set; }
		public bool UseRealPath { get; set; } = true;

		public SftpWinScpTransmitConfiguration() { }

		public SftpWinScpTransmitConfiguration(XmlDocument configXml) : base(configXml, SftpWinScpAdapterManagement.SftpWinScpScheme)
		{
			SftpWinScpAdapterManagement.ExtractSftpConfiguration(configXml, this);
		}
	}
}
