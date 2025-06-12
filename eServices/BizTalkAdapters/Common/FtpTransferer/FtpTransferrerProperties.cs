using System.Xml;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	class FtpTransferrerProperties : ConfigProperties
	{
		internal string Mode { get; set; }
		internal bool UseNLST { get; set; }
		internal bool MoveWorkingDirectory { get; set; }
		internal string FtpsMode { get; set; }
		internal bool ValidateServerCert { get; set; }
		internal string ThumbprintClientCert { get; set; }
		internal bool DataEncryption { get; set; }

		internal void ReadLocationConfiguration(XmlDocument configDom)
		{
			this.Mode = IfExistsExtract(configDom, "Config/Mode", "Passive");
			this.UseNLST = IfExistsExtractBool(configDom, "Config/UseNLST", false);
			this.MoveWorkingDirectory = IfExistsExtractBool(configDom, "Config/MoveWorkingDirectory", false);
			this.FtpsMode = IfExistsExtract(configDom, "Config/FtpsMode", "None");
			this.ValidateServerCert = IfExistsExtractBool(configDom, "Config/ValidateServerCert", true);
			this.ThumbprintClientCert = IfExistsExtract(configDom, "Config/ThumbprintClientCert", null);
			this.DataEncryption = IfExistsExtractBool(configDom, "Config/DataEncryption", true);
		}
	}
}