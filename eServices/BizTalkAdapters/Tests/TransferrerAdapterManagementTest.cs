using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common.UI;
using CargoWise.eHub.BizTalkAdapters.FtpEx.Admin;
using Microsoft.BizTalk.Adapter.Framework;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class TransferrerAdapterManagementTest : TestBase
	{
		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerAdapterManagement_ValidateConfigurationTest_Valid()
		{
			var target = new FtpExAdapterManagement();
			XElement configXml;
			string expected, actual;

			configXml = new XElement("Config");
			expected = configXml.ToString();
			actual = target.ValidateConfiguration(ConfigType.TransmitHandler, configXml.ToString());
			Assert.AreEqual(expected, actual);

			configXml = new XElement("Config",
							new XElement("uri", "URI"),
							new XElement("Server", "SERVER"),
							new XElement("Port", "21"),
							new XElement("User", "USER"),
							new XElement("Password", "PASSWORD"),
							new XElement("PollingInterval", "1"),
							new XElement("PollingUnit", "minute"),
							new XElement("Folder", "FOLDER"),
							new XElement("FileMask", "FILEMASK"),
							new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD-{f}-{n}-{x}"),
							new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD-{f}-{n}-{x}"),
							new XElement("EmptyFileOption", "Discard"),
							new XElement("OrderBy", "None")
							);
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://USER@SERVER:21/FOLDER/FILEMASK";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			configXml = new XElement("Config",
							new XElement("uri", "URI"),
							new XElement("Server", "SERVER"),
							new XElement("Port", "21"),
							new XElement("User", "USER"),
							new XElement("Password", "PASSWORD"),
							new XElement("PollingInterval", "1"),
							new XElement("PollingUnit", "minute"),
							new XElement("Folder", "FOLDER"),
							new XElement("FileMask", "")
							);
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://USER@SERVER:21/FOLDER";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			configXml = new XElement("Config",
							new XElement("Server", "SERVER"),
							new XElement("Port", "21"),
							new XElement("User", "USER"),
							new XElement("Password", "PASSWORD"),
							new XElement("Folder", "FOLDER"),
							new XElement("TargetFileName", "TARGETFILENAME")
							);
			actual = target.ValidateConfiguration(ConfigType.TransmitLocation, configXml.ToString());
			configXml.Add(new XElement("uri", "ftpex://USER@SERVER:21/FOLDER/TARGETFILENAME"));
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			configXml = new XElement("Config",
							new XElement("Server", "SERVER"),
							new XElement("Port", "21"),
							new XElement("User", "USER"),
							new XElement("Password", "PASSWORD"),
							new XElement("TargetFileName", "%DestinationPartyQualifier%")
							);
			actual = target.ValidateConfiguration(ConfigType.TransmitLocation, configXml.ToString());
			configXml.Add(new XElement("uri", "ftpex://USER@SERVER:21/%DestinationPartyQualifier%"));
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			configXml = new XElement("Config",
				new XElement("Server", "SERVER"),
				new XElement("Port", "21"),
				new XElement("User", "USER"),
				new XElement("Password", "PASSWORD"),
				new XElement("Folder", "FOLDER"),
				new XElement("TargetFileName", "%DestinationPartyQualifier%"),
				new XElement("UseContextConfiguration", "True")
				);
			actual = target.ValidateConfiguration(ConfigType.TransmitLocation, configXml.ToString());
			configXml.Add(new XElement("uri", "ftpex://USER@SERVER:21/FOLDER/%DestinationPartyQualifier%"));
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			configXml = new XElement("Config",
				new XElement("UseContextConfiguration", "True")
				);
			actual = target.ValidateConfiguration(ConfigType.TransmitLocation, configXml.ToString());
			configXml.Add(new XElement("uri", "ftpex://[ContextUser]@[ContextServer]:[ContextPort]/[ContextFolder]/[ContextTargetFileName]"));
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			expected = string.Empty;
			actual = target.ValidateConfiguration((ConfigType)4, null);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(AdapterException))]
		public void TransferrerAdapterManagement_ValidateConfigurationTest_Invalid()
		{
			var target = new FtpExAdapterManagement();
			string config = "<Config />";
			target.ValidateConfiguration(ConfigType.ReceiveLocation, config);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(AdapterException))]
		public void TransferrerAdapterManagement_ValidateConfigurationTest_InvalidIntParse()
		{
			var target = new FtpExAdapterManagement();
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "PORT"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("PollingInterval", "POLLINGINTERVAL")
								);
			target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(AdapterException))]
		public void TransferrerAdapterManagement_ValidateConfigurationTest_InvalidIntNegative()
		{
			var target = new FtpExAdapterManagement();
			var configXml = new XElement("Config",
					new XElement("Server", "SERVER"),
					new XElement("Port", "-1"),
					new XElement("User", "USER"),
					new XElement("Password", "PASSWORD"),
					new XElement("PollingInterval", "-1"),
					new XElement("PollingUnit", "Seconds")
					);
			target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerAdapterManagement_GetSchemaTest()
		{
			var target = new FtpExAdapterManagement();
			string fileLocation = null;
			string fileLocationExpected = String.Empty;
			Result expected = Result.Continue;
			Result actual;
			actual = target.GetSchema(String.Empty, String.Empty, out fileLocation);
			Assert.AreEqual(fileLocationExpected, fileLocation);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerAdapterManagement_ValidateReceiveLocation_FromMultipleLocations()
		{
			var target = MockRepository.GeneratePartialMock<FtpExAdapterManagement>();
			var mockPasswordPrompt = MockRepository.GenerateMock<PasswordPrompt>();
			mockPasswordPrompt.Stub(x => x.ShowDialog()).Return(DialogResult.OK);
			mockPasswordPrompt.Stub(x => x.Password).Return("password");
			target.Stub(x => x.GetPasswordPrompt(Arg<string>.Is.Anything)).Return(mockPasswordPrompt);

			XElement configXml;
			string expected, actual;

			configXml = new XElement("Config",
							new XElement("uri", "URI"),
							new XElement("Server"),
							new XElement("Port", "21"),
							new XElement("User"),
							new XElement("Password"),
							new XElement("PollingInterval", "1"),
							new XElement("PollingUnit", "minute"),
							new XElement("Folder"),
							new XElement("FileMask"),
							new XElement("MultipleLocations", "ftpex://USER:PASSWORD@SERVER:21/"),
							new XElement("MultipleLocationsCredentials"),
							new XElement("EmptyFileOption", "Discard"),
							new XElement("OrderBy", "None")
						);

			// Create credentials from URI password
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://USER@SERVER:21";
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER@SERVER:21/";
			configXml.Descendants().Single(x => x.Name == "MultipleLocationsCredentials").Value = "USER:PASSWORD@SERVER:21";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			// Change URI and credentials with password from prompt
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER2@SERVER:2100/FOLDER2";
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://USER2@SERVER:2100/FOLDER2";
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER2@SERVER:2100/FOLDER2";
			configXml.Descendants().Single(x => x.Name == "MultipleLocationsCredentials").Value = "USER2:password@SERVER:2100";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			// Change password from URI password
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER2:PASSword@SERVER:2100/FOLDER2";
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER2@SERVER:2100/FOLDER2";
			configXml.Descendants().Single(x => x.Name == "MultipleLocationsCredentials").Value = "USER2:PASSword@SERVER:2100";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			// Add a second location that reuses existing saved credentials
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER2@SERVER:2100/FOLDER2" + Environment.NewLine
																					 + "ftpex://USER2@SERVER:2100/FOLDER3/*.xml";
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://USER2@SERVER:2100/[MULTIPLE]";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);

			// Add a new location with new credentials with password from prompt
			configXml.Descendants().Single(x => x.Name == "MultipleLocations").Value = "ftpex://USER2@SERVER:2100/FOLDER2" + Environment.NewLine
																					 + "ftpex://USER3@SERVER3:3333/../*.xml";
			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://[MULTIPLE]@[SERVER:2100+SERVER3:3333]/[MULTIPLE]";
			configXml.Descendants().Single(x => x.Name == "MultipleLocationsCredentials").Value = "USER2:PASSword@SERVER:2100" + Environment.NewLine
																								+ "USER3:password@SERVER3:3333";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerAdapterManagement_ValidateReceiveLocation_FromClientRegistration()
		{
			var connection = MockRepository.GenerateMock<IDbConnection>();
			var command = MockRepository.GenerateMock<IDbCommand>();
			command.Expect(c => c.Parameters.Add(new SqlParameter("@RT_ID", SqlDbType.NVarChar, 50) { Value = "GLSHK" })).Repeat.Once();

			connection.Expect(x => x.CreateCommand()).Return(command).Repeat.Once();
			connection.Expect(x => x.Open()).Repeat.Once();
			connection.Expect(x => x.CreateCommand()).Return(command).Repeat.Once();

			var reader = MockRepository.GenerateMock<IDataReader>();
			command.Expect(_ => _.ExecuteReader()).Return(reader).Repeat.Once();
			reader.Expect(_ => _.Read()).Return(true).Repeat.Twice();
			reader.Expect(_ => _["CX_Attr1"].ToString()).Return("ftpex://priorbwihost@paftp.glshk.com:4021/in/*").Repeat.Once();
			reader.Expect(x => x["CX_Attr1"].ToString()).Return("ftpex://itnyulhost@xsfs.glshk.com:21/in/*").Repeat.Once();
			reader.Expect(x => x.Read()).Return(false).Repeat.Once();

			var target = MockRepository.GeneratePartialMock<FtpExAdapterManagement>();
			string expected, actual;

			XElement configXml = new XElement("Config",
							new XElement("uri", "URI"),
							new XElement("Server"),
							new XElement("Port", "21"),
							new XElement("User"),
							new XElement("Password"),
							new XElement("PollingInterval", "1"),
							new XElement("PollingUnit", "minute"),
							new XElement("Folder"),
							new XElement("FileMask"),
							new XElement("MultipleLocations"),
							new XElement("MultipleLocationsCredentials"),
							new XElement("EmptyFileOption", "Discard"),
							new XElement("OrderBy", "None"),
							new XElement("ConnectionStringName", "eHubTransactionsContext"),
							new XElement("RegistrationType", "GLSHK")
						);

			actual = target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
			configXml.Descendants().Single(x => x.Name == "uri").Value = "ftpex://GLSHK@eHubTransactionsContext";
			expected = configXml.ToString(SaveOptions.DisableFormatting);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(AdapterException))]
		public void TransferrerAdapterManagement_ValidateReceiveLocation_ThrowsAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<FtpExAdapterManagement>();
			XElement configXml = new XElement("Config",
							new XElement("uri", "URI"),
							new XElement("Server"),
							new XElement("Port", "21"),
							new XElement("User"),
							new XElement("Password"),
							new XElement("PollingInterval", "1"),
							new XElement("PollingUnit", "minute"),
							new XElement("Folder"),
							new XElement("FileMask"),
							new XElement("MultipleLocations"),
							new XElement("MultipleLocationsCredentials"),
							new XElement("EmptyFileOption", "Discard"),
							new XElement("OrderBy", "None"),
							new XElement("ConnectionStringName"),
							new XElement("RegistrationType", "GLSHK")
						);

			target.ValidateConfiguration(ConfigType.ReceiveLocation, configXml.ToString());
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(AdapterException))]
		public void TransferrerAdapterManagement_KeepAliveMustNotBeTrueWhenUseContextConfiguration()
		{
			var target = new FtpExAdapterManagement();
			XElement configXml = new XElement("Config",
							new XElement("uri", "URI"),
							new XElement("Server", "SERVER"),
							new XElement("Port", "21"),
							new XElement("User", "USER"),
							new XElement("Password", "PASSWORD"),
							new XElement("PollingInterval", "1"),
							new XElement("PollingUnit", "minute"),
							new XElement("Folder", "FOLDER"),
							new XElement("FileMask", "FILEMASK"),
							new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD-{f}-{n}-{x}"),
							new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD-{f}-{n}-{x}"),
							new XElement("EmptyFileOption", "Discard"),
							new XElement("OrderBy", "None"),
							new XElement("UseContextConfiguration", "TRUE"),
							new XElement("KeepAlive", "TRUE")
							);

			target.ValidateConfiguration(ConfigType.TransmitLocation, configXml.ToString());
		}
	}
}
