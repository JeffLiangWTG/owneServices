using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using static CargoWise.eHub.BizTalkAdapters.Common.TransferrerProperties;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class TransferrerPropertiesTest : TestBase
	{
		[TestMethod()]
		public void TransferrerProperties_ReceiveTest_Defaults()
		{
			string uri = "URI://USR@SVR";
			var cancelTokenSource = MockRepository.GenerateMock<CancellationTokenSource>();
			var target = new TransferrerProperties.Receive(uri);
			var configXml = new XElement("Config",
								new XElement("uri", uri),
								new XElement("Server", "SERVER"),
								new XElement("Port", "21"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD")
								);
			var configDom = new XmlDocument();
			configDom.LoadXml(configXml.ToString());

			target.ReadLocationConfiguration(configDom, "TestPortName", cancelTokenSource.Token);

			Assert.AreEqual("SERVER", target.Server);
			Assert.AreEqual(21, target.Port);
			Assert.AreEqual("USER", target.User);
			Assert.AreEqual("PASSWORD", target.Password);
			Assert.AreEqual(90000, target.Timeout);
			Assert.AreEqual(String.Empty, target.Folder);
			Assert.AreEqual("*", target.FileMask);
			Assert.AreEqual("None", target.SortOrder);
			Assert.AreEqual(60000, target.PollingInterval);
			Assert.AreEqual("Ignore", target.EmptyFileOption);
			Assert.AreEqual(String.Empty, target.RenameBeforeDownload);
			Assert.AreEqual(String.Empty, target.RenameAfterDownload);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual(@"C:\Logs\BizTalk\Interfaces\{prefix,3}", target.LogFolder);
			Assert.AreEqual(2, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsNotNull(target.Logger);
		}

		[TestMethod()]
		public void TransferrerProperties_ReceiveTest_NotDefault()
		{
			string uri = "URI://USR@SVR";
			var cancelTokenSource = MockRepository.GenerateMock<CancellationTokenSource>();
			var target = new TransferrerProperties.Receive(uri);
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("FileMask", "FILEMASK"),
								new XElement("Logging", "True"),
								new XElement("LogDir", "LOGDIR"),
								new XElement("LogMaxSize", "5"),
								new XElement("LogMaxCount", "10"),
								new XElement("PollingInterval", "2"),
								new XElement("PollingUnit", "hours"),
								new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD"),
								new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD"),
								new XElement("EmptyFileOption", "EMPTYFILEOPTION"),
								new XElement("SortOrder", "Name")
								);
			var configDOM = new XmlDocument();
			configDOM.LoadXml(configXml.ToString());

			target.ReadLocationConfiguration(configDOM, "TestPortName", cancelTokenSource.Token);

			Assert.AreEqual("SERVER", target.Server);
			Assert.AreEqual(2121, target.Port);
			Assert.AreEqual("USER", target.User);
			Assert.AreEqual("PASSWORD", target.Password);
			Assert.AreEqual(30000, target.Timeout);
			Assert.AreEqual("FOLDER", target.Folder);
			Assert.AreEqual("FILEMASK", target.FileMask);
			Assert.AreEqual("Name", target.SortOrder);
			Assert.AreEqual(7200000, target.PollingInterval);
			Assert.AreEqual("EMPTYFILEOPTION", target.EmptyFileOption);
			Assert.AreEqual("RENAMEBEFOREDOWNLOAD", target.RenameBeforeDownload);
			Assert.AreEqual("RENAMEAFTERDOWNLOAD", target.RenameAfterDownload);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual("LOGDIR", target.LogFolder);
			Assert.AreEqual(5, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsNotNull(target.Logger);
		}

		[TestMethod()]
		public void TransferrerProperties_ReceiveTest_GetMultipleLocationsFromConfig()
		{
			string uri = "URI://USR@SVR";
			var cancelTokenSource = MockRepository.GenerateMock<CancellationTokenSource>();
			var target = new TransferrerProperties.Receive(uri);
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("FileMask", "FILEMASK"),
								new XElement("Logging", "True"),
								new XElement("LogDir", "LOGDIR"),
								new XElement("LogMaxSize", "5"),
								new XElement("LogMaxCount", "10"),
								new XElement("PollingInterval", "2"),
								new XElement("PollingUnit", "hours"),
								new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD"),
								new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD"),
								new XElement("EmptyFileOption", "EMPTYFILEOPTION"),
								new XElement("MultipleLocations", "ftpex://priorbwihost@paftp.glshk.com:4021/in/*\nftpex://itnyulhost@xsfs.glshk.com:21/in/*"),
								new XElement("MultipleLocationsCredentials", "priorbwihost:bbbbbbbb@paftp.glshk.com:4021\nitnyulhost:aaaaaaaa@xsfs.glshk.com:21"),
								new XElement("SortOrder", "Name")
								);
			var configDOM = new XmlDocument();
			configDOM.LoadXml(configXml.ToString());

			target.ReadLocationConfiguration(configDOM, "TestPortName", cancelTokenSource.Token);

			Assert.AreEqual("SERVER", target.Server);
			Assert.AreEqual(2121, target.Port);
			Assert.AreEqual("USER", target.User);
			Assert.AreEqual("PASSWORD", target.Password);
			Assert.AreEqual(30000, target.Timeout);
			Assert.AreEqual("FOLDER", target.Folder);
			Assert.AreEqual("FILEMASK", target.FileMask);
			Assert.AreEqual("Name", target.SortOrder);
			Assert.AreEqual(7200000, target.PollingInterval);
			Assert.AreEqual("EMPTYFILEOPTION", target.EmptyFileOption);
			Assert.AreEqual("RENAMEBEFOREDOWNLOAD", target.RenameBeforeDownload);
			Assert.AreEqual("RENAMEAFTERDOWNLOAD", target.RenameAfterDownload);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual("LOGDIR", target.LogFolder);

			Assert.AreEqual("ftpex://priorbwihost@paftp.glshk.com:4021/in/*", target.MultipleLocations[0].Uri);
			Assert.AreEqual("ftpex://itnyulhost@xsfs.glshk.com:21/in/*", target.MultipleLocations[1].Uri);
			Assert.AreEqual("priorbwihost", target.MultipleLocations[0].UserName);
			Assert.AreEqual("itnyulhost", target.MultipleLocations[1].UserName);
			Assert.AreEqual("bbbbbbbb", target.MultipleLocations[0].Password);
			Assert.AreEqual("aaaaaaaa", target.MultipleLocations[1].Password);
			Assert.AreEqual("paftp.glshk.com", target.MultipleLocations[0].Server);
			Assert.AreEqual("xsfs.glshk.com", target.MultipleLocations[1].Server);
			Assert.AreEqual(4021, target.MultipleLocations[0].Port);
			Assert.AreEqual(21, target.MultipleLocations[1].Port);
			Assert.AreEqual("in", target.MultipleLocations[0].Folder);
			Assert.AreEqual("in", target.MultipleLocations[1].Folder);
			Assert.AreEqual("*", target.MultipleLocations[0].FileMask);
			Assert.AreEqual("*", target.MultipleLocations[1].FileMask);
			Assert.AreEqual(5, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsNotNull(target.Logger);
		}

		[TestMethod()]
		public void TransferrerProperties_ReceiveTest_GetMultipleLocationsFromConfig_Cancellation()
		{
			string uri = "URI://USR@SVR";
			var cancelTokenSource = MockRepository.GeneratePartialMock<CancellationTokenSource>();
			var target = MockRepository.GenerateMock<Receive>(uri);
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("FileMask", "FILEMASK"),
								new XElement("Logging", "True"),
								new XElement("LogDir", "LOGDIR"),
								new XElement("LogMaxSize", "5"),
								new XElement("LogMaxCount", "10"),
								new XElement("PollingInterval", "2"),
								new XElement("PollingUnit", "hours"),
								new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD"),
								new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD"),
								new XElement("EmptyFileOption", "EMPTYFILEOPTION"),
								new XElement("MultipleLocations", "ftpex://priorbwihost@paftp.glshk.com:4021/in/*\nftpex://itnyulhost@xsfs.glshk.com:21/in/*"),
								new XElement("MultipleLocationsCredentials", "priorbwihost:bbbbbbbb@paftp.glshk.com:4021\nitnyulhost:aaaaaaaa@xsfs.glshk.com:21"),
								new XElement("SortOrder", "Name")
								);
			var configDOM = new XmlDocument();
			configDOM.LoadXml(configXml.ToString());

			cancelTokenSource.Cancel();
			try
			{
				target.GetMultipleLocationsFromConfig("MultipleLocations", "multipleLocationCredentials", cancelTokenSource.Token);
				Assert.Fail("no exception thrown");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(OperationCanceledException));
				Assert.AreEqual(ex.Message, "The operation was canceled.");
			}
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential uri is null")]
		public void TransferrerProperties_ReceiveTest_readLocation_WithNullCredential_ShouldThrowAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			var reader = MockRepository.GenerateMock<IDataReader>();
			reader.Expect(_ => _["CX_Attr1"].ToString()).Return(null).Repeat.Once();

			target.ReadLocation(reader);
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential password is null")]
		public void TransferrerProperties_ReceiveTest_readLocation_WithNullPassword_ShouldThrowAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			var reader = MockRepository.GenerateMock<IDataReader>();
			reader.Expect(_ => _["CX_Attr1"].ToString()).Return("ftpex://priorbwihost@paftp.glshk.com:4021/in/*").Repeat.Once();
			reader.Expect(_ => _["CX_Password1"].ToString()).Return(null).Repeat.Once();
			target.ReadLocation(reader);
		}

		[TestMethod()]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithValidUri_ShouldReturnSuccessfully()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			var location = target.TryParseUri("ftpex://priorbwihost@paftp.glshk.com:4021/out/*");

			Assert.AreEqual("ftpex://priorbwihost@paftp.glshk.com:4021/out/*", location.Uri);
			Assert.AreEqual("priorbwihost", location.UserName);
			Assert.AreEqual("paftp.glshk.com", location.Server);
			Assert.AreEqual(4021, location.Port);
			Assert.AreEqual("*", location.FileMask);
			Assert.AreEqual("out", location.Folder);
		}

		[TestMethod()]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithMultipleDirectories_ShouldReturnSuccessfully()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			var location = target.TryParseUri("ftpex://priorbwihost@paftp.glshk.com:4021/firstout/secondout/out/*");

			Assert.AreEqual("ftpex://priorbwihost@paftp.glshk.com:4021/firstout/secondout/out/*", location.Uri);
			Assert.AreEqual("priorbwihost", location.UserName);
			Assert.AreEqual("paftp.glshk.com", location.Server);
			Assert.AreEqual(4021, location.Port);
			Assert.AreEqual("*", location.FileMask);
			Assert.AreEqual("firstout/secondout/out", location.Folder);
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential folder ftpex://priorbwihost@paftp.glshk.com:4021/firstout/secondout/out[]/* has invalid characters.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithInvalidMultipleDirectories_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://priorbwihost@paftp.glshk.com:4021/firstout/secondout/out[]/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential invaliduri is not a valid URI.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithInvalidUri_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("invaliduri");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential pex://priorbwihost@paftp.glshk.com:4021/in/* has a invalid scheme.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithInvalidScheme_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("pex://priorbwihost@paftp.glshk.com:4021/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential ://priorbwihost@paftp.glshk.com:4021/in/* does not contain a scheme.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithNoScheme_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("://priorbwihost@paftp.glshk.com:4021/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential ftpex://@paftp.glshk.com:4021/in/* does not contain a username.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithNoUsername_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://@paftp.glshk.com:4021/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential username ftpex://[[[[[[@paftp.glshk.com:4021/in/* has invalid characters.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithInvalidUsername_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://[[[[[[@paftp.glshk.com:4021/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential ftpex://priorbwihost@:4021/in/* does not contain a server name.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithNoServerName_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://priorbwihost@:4021/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential server ftpex://priorbwihost@[[[[:4021/in/* has invalid characters.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithInvalidServerName_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://priorbwihost@[[[[:4021/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential ftpex://priorbwihost@paftp.glshk.com:/in/* does not contain a port number.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithNoPortNumber_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://priorbwihost@paftp.glshk.com:/in/*");
		}

		[TestMethod()]
		[ExpectedException(typeof(AdapterException), "The credential ftpex://priorbwihost@paftp.glshk.com:4021/[[[[/* has invalid characters.")]
		public void TransferrerProperties_ReceiveTest_TryParseUri_WithInvalidFolder_ShouldReturnAdapterException()
		{
			var target = MockRepository.GeneratePartialMock<TransferrerProperties.Receive>("URI://USR@SVR");
			target.TryParseUri("ftpex://priorbwihost@paftp.glshk.com:4021/[[[[/*");
		}

		[TestMethod()]
		public void TransferrerProperties_TransmitTest_Defaults()
		{
			var configXml = new XElement("Config",
								new XElement("uri", "URI"),
								new XElement("Server", "SERVER"),
								new XElement("Port", "21"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD")
								);

			string propertyNamespace = "PROPERTYNAMESPACE";
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			var message = MockRepository.GenerateStub<IBaseMessage>();
			message.Context = context;

			var target = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.AreEqual("test://URI", target.Uri);
			Assert.AreEqual("test", target.Scheme);
			Assert.AreEqual("SERVER", target.Server);
			Assert.AreEqual(21, target.Port);
			Assert.AreEqual("USER", target.User);
			Assert.AreEqual("PASSWORD", target.Password);
			Assert.AreEqual(90000, target.Timeout);
			Assert.AreEqual(String.Empty, target.Folder);
			Assert.AreEqual("%MessageID%.xml", target.TargetFileName);
			Assert.AreEqual(String.Empty, target.TemporaryFolder);
			Assert.AreEqual(String.Empty, target.TemporaryFileName);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual(@"C:\Logs\BizTalk\Interfaces\{prefix,3}", target.LogFolder);
			Assert.AreEqual(2, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
		}

		[TestMethod()]
		public void TransferrerProperties_TransmitTest_ConfigurationFromLocation()
		{
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("Logging", "True"),
								new XElement("LogDir", "LOGDIR"),
								new XElement("LogMaxSize", "5"),
								new XElement("LogMaxCount", "10"),
								new XElement("TargetFileName", "TARGETFILENAME"),
								new XElement("TemporaryFolder", "TEMPORARYFOLDER"),
								new XElement("TemporaryFileName", "TEMPORARYFILENAME")
								);

			string propertyNamespace = "PROPERTYNAMESPACE";
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			var message = MockRepository.GenerateStub<IBaseMessage>();
			message.Context = context;

			var target = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.AreEqual("SERVER", target.Server);
			Assert.AreEqual(2121, target.Port);
			Assert.AreEqual("USER", target.User);
			Assert.AreEqual("PASSWORD", target.Password);
			Assert.AreEqual(30000, target.Timeout);
			Assert.AreEqual("FOLDER", target.Folder);
			Assert.AreEqual("TARGETFILENAME", target.TargetFileName);
			Assert.AreEqual("TEMPORARYFOLDER", target.TemporaryFolder);
			Assert.AreEqual("TEMPORARYFILENAME", target.TemporaryFileName);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual("LOGDIR", target.LogFolder);
			Assert.AreEqual(5, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
		}

		[TestMethod()]
		public void TransferrerProperties_TransmitTest_ConfigurationFromContext()
		{
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "2121"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("Timeout", "30000"),
								new XElement("Folder", "FOLDER"),
								new XElement("Logging", "True"),
								new XElement("LogDir", "LOGDIR"),
								new XElement("LogMaxSize", "5"),
								new XElement("LogMaxCount", "10"),
								new XElement("TargetFileName", "TARGETFILENAME"),
								new XElement("TemporaryFolder", "TEMPORARYFOLDER"),
								new XElement("TemporaryFileName", "TEMPORARYFILENAME"),
								new XElement("UseContextConfiguration", "True")
								);

			string propertyNamespace = "PROPERTYNAMESPACE";
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());

			//Context configuration
			context.Expect(x => x.Read("Server", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("SERVER1");
			context.Expect(x => x.Read("Port", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return(60000);
			context.Expect(x => x.Read("User", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("USER1");
			context.Expect(x => x.Read("Password", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("PASSWORD1");
			context.Expect(x => x.Read("Timeout", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return(99999);
			context.Expect(x => x.Read("Folder", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("FOLDER1");
			context.Expect(x => x.Read("TargetFileName", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("TARGETFILENAME1");
			context.Expect(x => x.Read("TemporaryFolder", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("TEMPORARYFOLDER1");
			context.Expect(x => x.Read("TemporaryFileName", TransferrerProperties.ContextConfigurationPropertyNamespace)).Return("TEMPORARYFILENAME1");

			var message = MockRepository.GenerateStub<IBaseMessage>();
			message.Context = context;

			var target = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.AreEqual("SERVER1", target.Server);
			Assert.AreEqual(60000, target.Port);
			Assert.AreEqual("USER1", target.User);
			Assert.AreEqual("PASSWORD1", target.Password);
			Assert.AreEqual(99999, target.Timeout);
			Assert.AreEqual("FOLDER1", target.Folder);
			Assert.AreEqual("TARGETFILENAME1", target.TargetFileName);
			Assert.AreEqual("TEMPORARYFOLDER1", target.TemporaryFolder);
			Assert.AreEqual("TEMPORARYFILENAME1", target.TemporaryFileName);

			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual("LOGDIR", target.LogFolder);
			Assert.AreEqual(5, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
		}

		[TestMethod()]
		public void TransferrerProperties_TransmitTest_KeepAlive()
		{
			var message = MockRepository.GenerateStub<IBaseMessage>();
			string propertyNamespace = "PROPERTYNAMESPACE";

			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(c => c.Read("KeepAlive", TransferrerProperties.ContextConfigurationPropertyNamespace))
				.Return(true);
			var configXml = new XElement("Config",
								new XElement("KeepAlive", "True"),
								new XElement("UseContextConfiguration", "True")
								);
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			message.Context = context;

			var transimit = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.IsTrue(transimit.UseContextConfiguration);
			Assert.IsFalse(transimit.KeepAlive);

			context = MockRepository.GenerateStub<IBaseMessageContext>();
			configXml = new XElement("Config",
								new XElement("KeepAlive", "False"),
								new XElement("UseContextConfiguration", "False")
								);
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			message.Context = context;

			transimit = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.IsFalse(transimit.UseContextConfiguration);
			Assert.IsFalse(transimit.KeepAlive);

			context = MockRepository.GenerateStub<IBaseMessageContext>();
			configXml = new XElement("Config",
								new XElement("KeepAlive", "True"),
								new XElement("UseContextConfiguration", "False")
								);
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			message.Context = context;

			transimit = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.IsFalse(transimit.UseContextConfiguration);
			Assert.IsTrue(transimit.KeepAlive);

			context = MockRepository.GenerateStub<IBaseMessageContext>();
			configXml = new XElement("Config",
								new XElement("UseContextConfiguration", "False")
								);
			context.Expect(x => x.Read("AdapterConfig", propertyNamespace)).Return(configXml.ToString());
			message.Context = context;

			transimit = new TransferrerProperties.Transmit(message, propertyNamespace, "test://URI");

			Assert.IsFalse(transimit.UseContextConfiguration);
			Assert.IsTrue(transimit.KeepAlive);
		}

		[TestMethod()]
		[DeploymentItem("CargoWise.eHub.BizTalkAdapters.FtpEx.dll")]
		public void TransferrerProperties_GetTimeIntervalTest()
		{
			int actual;
			actual = TransferrerProperties.Receive.GetTimeInterval(0, "");
			Assert.AreEqual(0, actual);
			actual = TransferrerProperties.Receive.GetTimeInterval(10, "seconds");
			Assert.AreEqual(10000, actual);
			actual = TransferrerProperties.Receive.GetTimeInterval(5, "minutes");
			Assert.AreEqual(300000, actual);
			actual = TransferrerProperties.Receive.GetTimeInterval(2, "hours");
			Assert.AreEqual(7200000, actual);
		}

		[TestMethod()]
		public async Task TransferrerProperties_DoWithRetries_InvalidOperationException()
		{
			string uri = "URI://USR@SVR";
			var target = MockRepository.GeneratePartialMock<Receive>(uri);

			var cancelTokenSource = MockRepository.GenerateMock<CancellationTokenSource>();

			var message = MockRepository.GenerateStub<IBaseMessage>();
			var logger = MockRepository.GenerateMock<ILog>();
			var exceptions = MockRepository.GenerateMock<ConcurrentDictionary<string,string>>();

			logger.Stub(x => x.IsDebugEnabled).Return(true);

			logger.Expect(x => x.Debug(Arg<string>.Matches(k => k.Contains("An error has occurred when getting client registrations. The function will retry after 1000 milliseconds.")))).Repeat.Once();
			logger.Expect(x => x.Debug(Arg<string>.Matches(k => k.Contains("An error has occurred when getting client registrations. The function will retry after 2000 milliseconds.")))).Repeat.Once();
			logger.Expect(x => x.Debug(Arg<string>.Matches(k => k.Contains("An error has occurred when getting client registrations. The function will retry after 3000 milliseconds.")))).Repeat.Once();

			var location1 = new Receive.Location()
			{
				Uri = "ftpex://priorbwihost@paftp.glshk.com:4021/in/*",
				UserName = "priorbwihost",
				Password = "rpbj23ZILk",
				Server = "paftp.glshk.com",
				Port = 4021,
				Folder = "in",
				FileMask = "*"
			};

			var location2 = new Receive.Location()
			{
				Uri = "ftpex://itnyulhost@xsfs.glshk.com:21/in/*",
				UserName = "itnyulhost",
				Password = "3^WAKmwdw6s)",
				Server = "xsfs.glshk.com",
				Port = 21,
				Folder = "in",
				FileMask = "*"
			};

			var locations = new List<Receive.Location> { location1, location2 };

			var exception1 = new InvalidOperationException("This is an invalid operation exception.");
			var exception2 = new InvalidOperationException("This is an invalid operation exception.");
			var exception3 = new InvalidOperationException("This is an invalid operation exception.");
			var mockFunction = MockRepository.GenerateStub<Func<CancellationToken, Task<List<Receive.Location>>>>();
			mockFunction.Stub(x => x.Invoke(cancelTokenSource.Token)).IgnoreArguments().Throw(exception1).Repeat.Once();
			mockFunction.Stub(x => x.Invoke(cancelTokenSource.Token)).IgnoreArguments().Throw(exception2).Repeat.Once();
			mockFunction.Stub(x => x.Invoke(cancelTokenSource.Token)).IgnoreArguments().Throw(exception3).Repeat.Once();
			mockFunction.Stub(x => x.Invoke(cancelTokenSource.Token)).IgnoreArguments().Return(Task.FromResult(locations)).Repeat.Once();


			var result = await target.DoWithRetriesAsync(mockFunction, logger, cancelTokenSource.Token);

			Assert.AreEqual("ftpex://priorbwihost@paftp.glshk.com:4021/in/*", result[0].Uri);
			Assert.AreEqual("ftpex://itnyulhost@xsfs.glshk.com:21/in/*", result[1].Uri);
			Assert.AreEqual("priorbwihost", result[0].UserName);
			Assert.AreEqual("itnyulhost", result[1].UserName);
			Assert.AreEqual("rpbj23ZILk", result[0].Password);
			Assert.AreEqual("3^WAKmwdw6s)", result[1].Password);
			Assert.AreEqual("paftp.glshk.com", result[0].Server);
			Assert.AreEqual("xsfs.glshk.com", result[1].Server);
			Assert.AreEqual(4021, result[0].Port);
			Assert.AreEqual(21, result[1].Port);
			Assert.AreEqual("in", result[0].Folder);
			Assert.AreEqual("in", result[1].Folder);
			Assert.AreEqual("*", result[0].FileMask);
			Assert.AreEqual("*", result[1].FileMask);

			Assert.AreEqual(0, target.retryInterval);
			Assert.AreEqual(0, target.exceptionStrings.Count);
			mockFunction.VerifyAllExpectations();
			logger.VerifyAllExpectations();

		}

		[TestMethod()]
		public async Task TransferrerProperties_DoWithRetries_TransientSqlException()
		{
			string uri = "URI://USR@SVR";
			var target = new Receive(uri);

			var logger = MockRepository.GenerateMock<ILog>();
			logger.Stub(x => x.IsDebugEnabled).Return(true);

			logger.Expect(x => x.Debug(Arg<string>.Matches(k => k.Contains("An error has occurred when getting client registrations. The function will retry immediately.")))).Repeat.Once();

			var multipleLocations = target.MultipleLocations;
			var cancelTokenSource = MockRepository.GenerateMock<CancellationTokenSource>();

			var location1 = new Receive.Location()
			{
				Uri = "ftpex://priorbwihost@paftp.glshk.com:4021/in/*",
				UserName = "priorbwihost",
				Password = "rpbj23ZILk",
				Server = "paftp.glshk.com",
				Port = 4021,
				Folder = "in",
				FileMask = "*"
			};

			var locations = new List<Receive.Location> { location1 };

			var mockFunction = MockRepository.GenerateMock<Func<CancellationToken, Task<List<Receive.Location>>>>();
			mockFunction.Expect(x => x.Invoke(cancelTokenSource.Token)).Throw(CreateSqlException(1064)).Repeat.Once();
			mockFunction.Expect(x => x.Invoke(cancelTokenSource.Token)).Return(Task.FromResult(locations)).Repeat.Once();

			var result = await target.DoWithRetriesAsync(mockFunction, logger, cancelTokenSource.Token);

			Assert.AreEqual(result, locations);
			Assert.AreEqual(0, target.retryInterval);
			Assert.AreEqual(0, target.exceptionStrings.Count);
			mockFunction.VerifyAllExpectations();
			logger.VerifyAllExpectations();
		}

		[TestMethod()]
		public async Task TransferrerProperties_DoWithRetries_NonTransientSqlException()
		{
			string uri = "URI://USR@SVR";
			var target = new Receive(uri);

			var logger = MockRepository.GenerateMock<ILog>();
			logger.Stub(x => x.IsDebugEnabled).Return(true);

			logger.Expect(x => x.Debug(Arg<string>.Matches(k => k.Contains("An error has occurred when getting client registrations. The function will retry after 1000 milliseconds.")))).Repeat.Once();

			var multipleLocations = target.MultipleLocations;
			var cancelTokenSource = MockRepository.GenerateMock<CancellationTokenSource>();

			var location1 = new Receive.Location()
			{
				Uri = "ftpex://priorbwihost@paftp.glshk.com:4021/in/*",
				UserName = "priorbwihost",
				Password = "rpbj23ZILk",
				Server = "paftp.glshk.com",
				Port = 4021,
				Folder = "in",
				FileMask = "*"
			};

			var locations = new List<Receive.Location> { location1 };

			var mockFunction = MockRepository.GenerateMock<Func<CancellationToken, Task<List<Receive.Location>>>>();
			mockFunction.Expect(x => x.Invoke(cancelTokenSource.Token)).Throw(CreateSqlException(50000)).Repeat.Once();
			mockFunction.Expect(x => x.Invoke(cancelTokenSource.Token)).Return(Task.FromResult(locations)).Repeat.Once();

			var result = await target.DoWithRetriesAsync(mockFunction, logger, cancelTokenSource.Token);

			Assert.AreEqual(result, locations);
			Assert.AreEqual(0, target.retryInterval);
			Assert.AreEqual(0, target.exceptionStrings.Count);
			mockFunction.VerifyAllExpectations();
			logger.VerifyAllExpectations();
		}

		[TestMethod()]
		public async Task TransferrerProperties_DoWithRetries_CancellationWhenReadingLocations()
		{
			string uri = "URI://USR@SVR";
			var target = MockRepository.GeneratePartialMock<Receive>(uri);

			var logger = MockRepository.GenerateMock<ILog>();
			var cancelTokenSource = new CancellationTokenSource();

			var func = MockRepository.GenerateStub<Func<CancellationToken, Task<Receive.Location>>>();
			func.Stub(x => x.Invoke(cancelTokenSource.Token)).Throw(CreateSqlException(2601)).Repeat.Once();
			func.Stub(x => x.Invoke(cancelTokenSource.Token)).Throw(new OperationCanceledException()).Repeat.Once();

			try
			{
				await target.DoWithRetriesAsync(func, logger, cancelTokenSource.Token);
				Assert.Fail("no exception thrown");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(OperationCanceledException));
				Assert.AreEqual(ex.Message, "The operation was canceled.");
			}
		}

		[TestMethod()]
		public async Task TransferrerProperties_DoWithRetries_Cancellation()
		{
			string uri = "URI://USR@SVR";
			var target = MockRepository.GeneratePartialMock<Receive>(uri);

			var logger = MockRepository.GenerateMock<ILog>();
			var cancellationTokenSource = new CancellationTokenSource();
			var func = MockRepository.GenerateStub<Func<CancellationToken, Task<Receive.Location>>>();

			cancellationTokenSource.Cancel();

			try
			{
				await target.DoWithRetriesAsync(func, logger, cancellationTokenSource.Token);
				Assert.Fail("no exception thrown");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(OperationCanceledException));
				Assert.AreEqual(ex.Message, "The operation was canceled.");
			}
		}

		static SqlException CreateSqlException(int errorNumber = 0, byte errorState = 0, byte errorClass = 0, string server = "", string errorMessage = "", string procedure = "", int lineNumber = 0)
		{
			var collection = typeof(SqlErrorCollection)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(new object[] { }) as SqlErrorCollection;

			var error = typeof(SqlError)
				.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
				new[]
				{
					typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int)
				},
				null).Invoke(new object[] { errorNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber }) as SqlError;

			typeof(SqlErrorCollection)
				.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(collection, new object[] { error });

			var ex = typeof(SqlException)
				.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(SqlErrorCollection), typeof(string) }, null)
				.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;

			return ex;
		}
	}
}
