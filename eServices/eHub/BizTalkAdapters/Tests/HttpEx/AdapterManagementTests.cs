using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.HttpEx.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.HttpEx
{
	public class AdapterManagementTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_AdapterManagement_GetConfigSchema()
		{
			var getXml = new Func<string, string>(n =>
			{
				using (var res = typeof(HttpExAdapterManagement).Assembly.GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.HttpEx.Admin." + n))
				{
					Assert.IsNotNull(res);
					using (var rdr = new StreamReader(res))
						return rdr.ReadToEnd();
				}
			});

			var httpExAdapterManagement = new HttpExAdapterManagement();
			Assert.AreEqual(string.Empty, httpExAdapterManagement.GetConfigSchema(ConfigType.ReceiveHandler));
			Assert.AreEqual(string.Empty, httpExAdapterManagement.GetConfigSchema(ConfigType.ReceiveLocation));
			Assert.AreEqual(getXml("HttpExTransmitHandler.xsd"), httpExAdapterManagement.GetConfigSchema(ConfigType.TransmitHandler));
			Assert.AreEqual(getXml("HttpExTransmitLocation.xsd"), httpExAdapterManagement.GetConfigSchema(ConfigType.TransmitLocation));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_AdapterManagement_ValidateTransmitHandler()
		{
			var adapterManagement = new HttpExAdapterManagement();

			var config = new XElement("Config",
				new XElement("InterfacesLogDir", @"C:\INTERFACES_LOGDIR"),
				new XElement("StructuredLogDir", @"C:\INTERFACES_LOGDIR"),
				new XElement("TerminateWaitLimit", "60000"),
				new XElement("LogDir", @"\\SERVER\LOGDIR"),
				new XElement("LogMaxSize", "5"),
				new XElement("LogMaxCount", "10"),
				new XElement("LogLevel", "Debug")
			);

			var expected = config.ToString(SaveOptions.DisableFormatting);
			var actual = adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString());

			Assert.AreEqual(expected, actual);

			config.Element("InterfacesLogDir").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/InterfacesLogDir not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/InterfacesLogDir not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("InterfacesLogDir", "INTERFACES_LOGDIR"));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Interfaces Log Directory is not a valid path."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("InterfacesLogDir").Value = @"C:\INTERFACES_LOGDIR";

			config.Element("StructuredLogDir").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/StructuredLogDir not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/StructuredLogDir not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("StructuredLogDir", "INTERFACES_LOGDIR"));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Structured Interfaces Log Directory is not a valid path."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("StructuredLogDir").Value = @"C:\INTERFACES_LOGDIR";

			config.Element("TerminateWaitLimit").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/TerminateWaitLimit not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("TerminateWaitLimit", ""));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Input string was not in a correct format."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("TerminateWaitLimit").Value = "-1";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Terminate Wait Limit cannot be negative."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("TerminateWaitLimit").Value = "0";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString());
			config.Element("TerminateWaitLimit").Value = "60000";

			config.Element("LogDir").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogDir not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("LogDir", "LOGDIR"));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Directory is not a valid path."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogDir").Value = @"\\SERVER\LOGDIR";

			config.Element("LogMaxSize").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogMaxSize not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("LogMaxSize", ""));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Input string was not in a correct format."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogMaxSize").Value = "-1";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max File Size (MB) must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogMaxSize").Value = "0";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max File Size (MB) must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogMaxSize").Value = "5";

			config.Element("LogMaxCount").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogMaxCount not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("LogMaxCount", ""));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Input string was not in a correct format."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogMaxCount").Value = "-1";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max Rollover Count must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogMaxCount").Value = "0";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max Rollover Count must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogMaxCount").Value = "10";

			config.Element("LogLevel").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogLevel not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Add(new XElement("LogLevel", "Error"));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Level not a valid value."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString()));
			config.Element("LogLevel").Value = "Info";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString());
			config.Element("LogLevel").Value = "Debug";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString());
			config.Element("LogLevel").Value = "Trace";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitHandler, config.ToString());
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_AdapterManagement_ValidateTransmitLocation()
		{
			var adapterManagement = new HttpExAdapterManagement();

			var config = new XElement("Config",
				new XElement("UniqueName", "UNIQUENAME"),
				new XElement("DestinationUrl", "http://destination"),
				new XElement("Timeout", "90000"),
				new XElement("Method", "POST"),
				new XElement("ContentType", "application/xml; charset=UTF-8"),
				new XElement("CustomHeaders", "X-CustomHeader: CUSTOM_VALUE\r\nX-AnotherHeader: ANOTHER_VALUE"),
				new XElement("CustomHeaderNamesWithoutValidation", "UserName-Agent, X-CustomHeader"),
				new XElement("SuppressMessageBodyForHttpVerbs", "GET, DELETE"),
				new XElement("Success", "200-299"),
				new XElement("Terminate", "400-499,500-599"),
				new XElement("LogMaxSize", "5"),
				new XElement("LogMaxCount", "10"),
				new XElement("LogLevel", "Debug"),
				new XElement("LogFormat", "Flat"),
				new XElement("Certificate", "YQ=="),
				new XElement("CertificatePassphrase", "CertificatePassphrase")
			);

			var expected = new XElement(config);
			expected.AddFirst(new XElement("uri", "httpex://destination:80/#UNIQUENAME"));
			var actual = adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			Assert.AreEqual(expected.ToString(SaveOptions.DisableFormatting), actual);

			config.Element("DestinationUrl").Value = string.Empty;
			expected.Element("DestinationUrl").Value = string.Empty;
			expected.Element("uri").Value = "httpex://uniquename/";

			actual = adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			Assert.AreEqual(expected.ToString(SaveOptions.DisableFormatting), actual);

			config.Element("DestinationUrl").Value = "https://destination";
			config.Element("UniqueName").Value = string.Empty;
			config.Element("SuppressMessageBodyForHttpVerbs").Remove();
			expected.Element("DestinationUrl").Value = "https://destination";
			expected.Element("UniqueName").Value = string.Empty;
			expected.Element("uri").Value = "httpsex://destination:443/";

			actual = adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			Assert.AreEqual(expected.ToString(SaveOptions.DisableFormatting), actual);

			config.Element("DestinationUrl").Value = string.Empty;
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Destination URL or Unique Name must be specified."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("DestinationUrl").Value = "http://destination";

			config.Element("DestinationUrl").Value = "ftp://destination";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Address scheme 'ftp' is not valid."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("DestinationUrl").Value = "http://destination";

			config.Element("Timeout").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/Timeout not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Add(new XElement("Timeout", ""));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Input string was not in a correct format."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Timeout").Value = "-1";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Timeout must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Timeout").Value = "0";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Timeout must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Timeout").Value = "5";

			config.Element("Method").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/Method not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Add(new XElement("Method", string.Empty));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("HTTP Method is required."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Method").Value = "GET";

			config.Element("ContentType").Value = "/";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Error parsing Content Type value. The format of value '/' is invalid."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("ContentType").Value = "application/xml; charset=UTF-8";

			config.Element("CustomHeaders").Value = "X-CustomHeader:Invalid";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("HTTP header format is invalid."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("CustomHeaders").Value = "X-CustomHeader: CUSTOM_VALUE";

			config.Element("Success").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/Success not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Add(new XElement("Success", string.Empty));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Status range format is incorrect."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Success").Value = "0";
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Status range format is incorrect."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Success").Value = "200,400-";
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Status range format is incorrect."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Success").Value = "400";
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Status ranges overlap."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Success").Value = "400-499,404";
			config.Element("Terminate").Value = string.Empty;
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Status ranges overlap."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Success").Value = "401-400";
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Status range format is incorrect."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("Success").Value = "200-299";

			config.Element("LogMaxSize").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogMaxSize not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Add(new XElement("LogMaxSize", ""));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Input string was not in a correct format."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogMaxSize").Value = "-1";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max File Size (MB) must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogMaxSize").Value = "0";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max File Size (MB) must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogMaxSize").Value = "5";

			config.Element("LogMaxCount").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogMaxCount not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Add(new XElement("LogMaxCount", ""));
			Assert.Throws(Is.TypeOf<FormatException>().And.Message.EqualTo("Input string was not in a correct format."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogMaxCount").Value = "-1";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max Rollover Count must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogMaxCount").Value = "0";
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Max Rollover Count must be positive."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogMaxCount").Value = "10";

			config.Element("LogLevel").Remove();
			Assert.Throws(Is.TypeOf<NoSuchProperty>().And.Message.EqualTo("Property /Config/LogLevel not found on adapter configuration XML."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Add(new XElement("LogLevel", "Error"));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Level not a valid value."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogLevel").Value = "Info";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			config.Element("LogLevel").Value = "Debug";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			config.Element("LogLevel").Value = "Trace";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());

			config.Element("LogFormat").Remove();
			config.Add(new XElement("LogFormat", "Error"));
			Assert.Throws(Is.TypeOf<AdapterException>().And.Message.EqualTo("Log Format not a valid value."), () =>
				adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString()));
			config.Element("LogFormat").Value = "Flat";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			config.Element("LogFormat").Value = "Structured";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
			config.Element("LogFormat").Value = "Both";
			adapterManagement.ValidateConfiguration(ConfigType.TransmitLocation, config.ToString());
		}
	}
}
