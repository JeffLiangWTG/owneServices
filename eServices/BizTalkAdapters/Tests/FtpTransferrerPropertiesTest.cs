using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
    [TestClass()]
    public class FtpTransferrerPropertiesTest
    {
        [TestMethod()]
		public void FtpTransferrerProperties_ReadLocationConfigurationDefaultTest()
        {
            var target = new FtpTransferrerProperties();
            var config = new XElement("Config");
            var configDom = new XmlDocument();
            configDom.LoadXml(config.ToString());

            target.ReadLocationConfiguration(configDom);

            Assert.AreEqual("Passive", target.Mode);
            Assert.AreEqual(false, target.UseNLST);
        }

        [TestMethod()]
		public void FtpTransferrerProperties_ReadLocationConfiguration_UpdateTransferrerSettingsTest()
        {
			var target = new FtpTransferrerProperties();
            var config = new XElement("Config",
                new XElement("Mode", "Active"),
				new XElement("UseNLST", "True"),
				new XElement("MoveWorkingDirectory", "True"),
				new XElement("FtpsMode", "Explicit"),
				new XElement("ValidateServerCert", "False"),
				new XElement("ThumbprintClientCert", "THUMBPRINTCLIENTCERT"),
				new XElement("DataEncryption", "False")
			);
            var configDom = new XmlDocument();
            configDom.LoadXml(config.ToString());

            target.ReadLocationConfiguration(configDom);

            Assert.AreEqual("Active", target.Mode);
            Assert.AreEqual(true, target.UseNLST);
            Assert.AreEqual(true, target.MoveWorkingDirectory);
            Assert.AreEqual("Explicit", target.FtpsMode);
            Assert.AreEqual(false, target.ValidateServerCert);
            Assert.AreEqual("THUMBPRINTCLIENTCERT", target.ThumbprintClientCert);
            Assert.AreEqual(false, target.DataEncryption);
        }
    }
}
