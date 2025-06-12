using System;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Common.Logging.Simple;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class LoggingPropertiesTest : TestBase
	{
		[TestMethod()]
		public void LoggingProperties_Defaults()
		{
			var target = new LoggingProperties();
			var testPortName = "TestPortName";
			var configDom = new XmlDocument();
			var configXml = new XElement("Config");
			configDom.LoadXml(configXml.ToString());

			target.ReadLocationConfiguration(configDom, testPortName);

			Assert.AreEqual(testPortName, target.PortName);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual(@"C:\Logs\BizTalk\Interfaces\{prefix,3}", target.LogFolder);
			Assert.AreEqual(2, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
		}

		[TestMethod()]
		public void LoggingProperties_NotDefault()
		{
			var target = new LoggingProperties();
			var testPortName = "TestPortName";
			var configDom = new XmlDocument();
			var configXml = new XElement("Config",
								new XElement("Logging", "False"),
								new XElement("LogDir", "LOGDIR"),
								new XElement("LogMaxSize", "5"),
								new XElement("LogMaxCount", "10")
								);
			configDom.LoadXml(configXml.ToString());

			target.ReadLocationConfiguration(configDom, testPortName);

			Assert.AreEqual(testPortName, target.PortName);
			Assert.IsFalse(target.LoggingEnabled);
			Assert.AreEqual("LOGDIR", target.LogFolder);
			Assert.AreEqual(5, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
		}
	}
}
