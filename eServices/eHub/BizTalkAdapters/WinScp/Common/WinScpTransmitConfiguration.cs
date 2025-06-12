using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	public class WinScpTransmitConfiguration : WinScpConfiguration
	{
		[XmlIgnore]
		public WinScpLocation Location { get; }
		public string Folder { get; set; }
		public string TargetFileName { get; set; }
		public string FlagFile { get; set; }
		public string TemporaryFolder { get; set; }
		public string TemporaryFileName { get; set; }
		public int ConnectionLimit { get; set; }
		public bool UseContextConfiguration { get; set; }

		public WinScpTransmitConfiguration() { }

		public WinScpTransmitConfiguration(XmlDocument configXml) : this(configXml, "winscp") { }

		public WinScpTransmitConfiguration(XmlDocument configXml, string scheme) : base(configXml)
		{
			Folder = ConfigProperties.IfExistsExtract(configXml, "/Config/Folder", null);
			TargetFileName = ConfigProperties.IfExistsExtract(configXml, "/Config/TargetFileName", null);
			FlagFile = ConfigProperties.IfExistsExtract(configXml, "/Config/FlagFile", null);
			TemporaryFolder = ConfigProperties.IfExistsExtract(configXml, "/Config/TemporaryFolder", null);
			TemporaryFileName = ConfigProperties.IfExistsExtract(configXml, "/Config/TemporaryFileName", null);
			ConnectionLimit = ConfigProperties.IfExistsExtractInt(configXml, "/Config/ConnectionLimit", 1);
			UseContextConfiguration = ConfigProperties.IfExistsExtractBool(configXml, "/Config/UseContextConfiguration", false);

			Location = new WinScpLocation
			{
				Scheme = scheme,
				UserName = UserName,
				Password = Password,
				Server = Server,
				Port = Port,
				Folder = Folder,
				FileName = TargetFileName
			};
			Uri = UseContextConfiguration ? Location.GetContextUri() : Location.GetUri();
		}
	}
}
