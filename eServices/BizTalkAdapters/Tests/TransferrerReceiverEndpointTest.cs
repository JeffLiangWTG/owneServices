using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using CargoWise.eHub.BizTalkAdapters.FtpEx;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class TransferrerReceiverEndpointTest : TestBase
	{
		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmitTest()
		{
			DownloadAndSubmitTest();
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmitTransferrerThrowsOnClose()
		{
			DownloadAndSubmitTest(closeException: new SocketException(10054));
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmitMultipleTransferrersThrowOnClose()
		{
			DownloadAndSubmitTest(closeException: new SocketException(10054), maximumConcurrentDownloads: 2);
		}

		void DownloadAndSubmitTest(Exception closeException = null, int maximumConcurrentDownloads = 1)
		{
			string uri = "URI://USR@SVR";
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "0"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("PollingInterval", "30"),
								new XElement("PollingUnit", "Seconds"),
								new XElement("EmptyFileOption", "Ignore"),
								new XElement("SortOrder", "None"),
								new XElement("MaximumConcurrentDownloads", maximumConcurrentDownloads)
								);
			var fakeConfig = new FakePropertyBag() { Config = configXml.ToString() };
			var bizTalkConfig = MockRepository.GenerateStub<IPropertyBag>();
			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			string transportType = "FtpEx";
			string propertyNamespace = "http://cargowise.com/ehub/biztalkadapters/FtpEx-properties";
			var control = new ControlledTermination();
			var log = new ConcurrentQueue<Tuple<string, string>>();
			var logger = CreateStubLogger(log);

			var repeats = maximumConcurrentDownloads > 1 ? maximumConcurrentDownloads + 1 : 1;
			var mockFtpTransferrer = MockRepository.GenerateStrictMock<ITransferrer, IDisposable>();
			mockFtpTransferrer.Stub(x => x.Server).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Port).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.UserName).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Password).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Timeout).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.CancelToken).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Logger).Return(logger);
			mockFtpTransferrer.Expect(x => x.Logger = Arg<ILog>.Is.Anything);
			mockFtpTransferrer.Expect(x => x.ReadLocationConfiguration(Arg<XmlDocument>.Is.Anything));
			mockFtpTransferrer.Expect(x => x.Open()).Repeat.Times(repeats);
			mockFtpTransferrer.Expect(x => x.ListFiles(null, null, null)).IgnoreArguments().Return(
				new List<TransferrerFileInfo> {
						new TransferrerFileInfo { Name = "FILE2.002", Timestamp = DateTime.Parse("1999-12-31 00:00"), Size = 500 },
						new TransferrerFileInfo { Name = "FILE3.003", Timestamp = DateTime.Parse("2002-01-01 00:00"), Size = 1000 },
						new TransferrerFileInfo { Name = "FILE4.004", Timestamp = DateTime.Parse("2001-01-01 00:00"), Size = 100 },
					}
			);
			var fs2 = CreatePaddedStream(500);
			var fs3 = CreatePaddedStream(1000);
			var fs4 = CreatePaddedStream(100);

			var allTransferrersCreatedWaitSpan = TimeSpan.FromSeconds(30);
			using (var allTransferrersCreated = new ManualResetEvent(false))
			{
				mockFtpTransferrer.Expect(x => x.GetFile("FILE2.002")).Do(new Func<string, MemoryStream>((f) =>
				{
					allTransferrersCreated.WaitOne(allTransferrersCreatedWaitSpan);
					return fs2;
				}));
				mockFtpTransferrer.Expect(x => x.GetFile("FILE3.003")).Do(new Func<string, MemoryStream>((f) =>
				{
					allTransferrersCreated.WaitOne(allTransferrersCreatedWaitSpan);
					return fs3;
				}));
				mockFtpTransferrer.Expect(x => x.GetFile("FILE4.004")).Do(new Func<string, MemoryStream>((f) =>
				{
					allTransferrersCreated.WaitOne(allTransferrersCreatedWaitSpan);
					return fs4;
				}));
				mockFtpTransferrer.Expect(x => x.DeleteFile("FILE2.002"));
				mockFtpTransferrer.Expect(x => x.DeleteFile("FILE3.003"));
				mockFtpTransferrer.Expect(x => x.DeleteFile("FILE4.004"));

				if (closeException != null)
				{
					mockFtpTransferrer.Expect(x => x.Dispose()).Repeat.Times(repeats).Throw(closeException);
				}
				else
				{
					mockFtpTransferrer.Expect(x => x.Dispose()).Repeat.Times(repeats);
				}

				mockFtpTransferrer.Expect(x => x.Close()).Repeat.Never();

				var transferrersCreated = 0;
				var mockTransferrerFactory = MockRepository.GenerateMock<FtpTransferrerFactory>();
				mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Do(new Func<ITransferrer>(() =>
				{
					if (Interlocked.Increment(ref transferrersCreated) == repeats)
					{
						allTransferrersCreated.Set();
					}
					return mockFtpTransferrer;
				}));
				var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
				stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(new MemoryStream());
				var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
				stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
				var stubBatch = MockRepository.GenerateStub<ISyncReceiveSubmitBatch>();
				stubBatch.Expect(x => x.Wait()).Return(true);
				var stubBatchFactory = MockRepository.GenerateStub<ISyncReceiveSubmitBatchFactory>();
				stubBatchFactory.Expect(x => x.CreateBatch(Arg<IBTTransportProxy>.Is.Anything, Arg<ControlledTermination>.Is.Anything, Arg<int>.Is.Anything)).Return(stubBatch);
				var mockMessageFactory = MockRepository.GenerateMock<ITransferrerMessageFactory>();

				var mockRecieverEndpoints = MockRepository.GenerateMock<IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>>();
				mockRecieverEndpoints.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new ConcurrentDictionary<string, TransferrerReceiverEndpoint>());
				var mockRecieverLoggers = MockRepository.GenerateMock<IDictionary<Type, ILog>>();
				mockRecieverLoggers.Stub(x => x[typeof(object)]).IgnoreArguments().Return(logger);
				var mockHostCancelTokenSource = MockRepository.GenerateMock<IDictionary<Type, CancellationTokenSource>>();
				mockHostCancelTokenSource.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new CancellationTokenSource());

				TransferrerReceiver.ReceiverEndpoints = mockRecieverEndpoints;
				TransferrerReceiver.ReceiverLoggers = mockRecieverLoggers;
				TransferrerReceiver.HostCancelTokenSource = mockHostCancelTokenSource;

				var target = MockRepository.GeneratePartialMock<FtpExReceiverEndpoint>(mockTransferrerFactory, stubBatchFactory, mockMessageFactory, new ReceivePropertiesFactoryForTest(logger));
				mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE2.002"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs2))).Return(stubMessage);
				mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE3.003"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs3))).Return(stubMessage);
				mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE4.004"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs4))).Do(
					new Func<IBaseMessageFactory, string, string, string, string, Stream, IBaseMessage>((f, n, u, tl, t, s) =>
				{
					Task.Factory.StartNew(() => target.Dispose());
					Thread.Sleep(200);
					return stubMessage;
				}));
				target.Stub(x => x.EndpointTask());
				string portName;
				target.Stub(x => x.TryGetPortName(out portName)).Return(false);

				target.Open(uri, fakeConfig, bizTalkConfig, handlerPropertyBag, transportProxy, transportType, propertyNamespace, control);

				target.DownloadFilesAndSubmit();

				target.VerifyAllExpectations();
				mockFtpTransferrer.VerifyAllExpectations();

				if (closeException != null)
				{
					Assert.IsNotNull(log.FirstOrDefault(x => x.Item1 == "Error" && x.Item2.Contains("System.Net.Sockets.SocketException: An existing connection was forcibly closed by the remote host")));
				}
			}
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmitRenamesTest()
		{
			string uri = "URI://USR@SVR";
			var configXml = new XElement("Config",
								new XElement("Server", "SERVER"),
								new XElement("Port", "0"),
								new XElement("User", "USER"),
								new XElement("Password", "PASSWORD"),
								new XElement("PollingInterval", "30"),
								new XElement("PollingUnit", "Seconds"),
								new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD-{f}-{n}-{x}"),
								new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD-{f}-{n}-{x}"),
								new XElement("EmptyFileOption", "Discard"),
								new XElement("SortOrder", "None"),
								new XElement("UseNLST", "True")
								);
			var fakeConfig = new FakePropertyBag() { Config = configXml.ToString() };
			var bizTalkConfig = MockRepository.GenerateStub<IPropertyBag>();
			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			string transportType = "FtpEx";
			string propertyNamespace = "http://cargowise.com/ehub/biztalkadapters/FtpEx-properties";
			var control = new ControlledTermination();
			var logger = LogManager.GetLogger(GetType().FullName);

			var mockFtpTransferrer = MockRepository.GenerateStrictMock<ITransferrer, IDisposable>();
			mockFtpTransferrer.Stub(x => x.Server).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Port).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.UserName).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Password).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Timeout).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.CancelToken).PropertyBehavior();
			mockFtpTransferrer.Stub(x => x.Logger).Return(logger);
			mockFtpTransferrer.Expect(x => x.Logger = Arg<ILog>.Is.Anything);
			mockFtpTransferrer.Expect(x => x.ReadLocationConfiguration(Arg<XmlDocument>.Is.Anything));
			mockFtpTransferrer.Expect(x => x.Open());
			mockFtpTransferrer.Expect(x => x.ListFiles(null, null, null)).IgnoreArguments().Return(
				new List<TransferrerFileInfo> {
						new TransferrerFileInfo { Name = "FILE2.002", Timestamp = DateTime.Parse("1999-12-31 00:00"), Size = 500 },
						new TransferrerFileInfo { Name = "FILE3.003", Timestamp = DateTime.Parse("2002-01-01 00:00"), Size = 1000 },
						new TransferrerFileInfo { Name = "FILE4.004", Timestamp = DateTime.Parse("2001-01-01 00:00"), Size = 100 },
					}
			);
			var fs2 = CreatePaddedStream(500);
			var fs3 = CreatePaddedStream(500);
			var fs4 = CreatePaddedStream(100);
			mockFtpTransferrer.Expect(x => x.RenameFile("FILE2.002", "RENAMEBEFOREDOWNLOAD-FILE2.002-FILE2-.002"));
			mockFtpTransferrer.Expect(x => x.RenameFile("FILE3.003", "RENAMEBEFOREDOWNLOAD-FILE3.003-FILE3-.003"));
			mockFtpTransferrer.Expect(x => x.RenameFile("FILE4.004", "RENAMEBEFOREDOWNLOAD-FILE4.004-FILE4-.004"));
			mockFtpTransferrer.Expect(x => x.GetFile("RENAMEBEFOREDOWNLOAD-FILE2.002-FILE2-.002")).Return(fs2);
			mockFtpTransferrer.Expect(x => x.GetFile("RENAMEBEFOREDOWNLOAD-FILE3.003-FILE3-.003")).Return(fs3);
			mockFtpTransferrer.Expect(x => x.GetFile("RENAMEBEFOREDOWNLOAD-FILE4.004-FILE4-.004")).Return(fs4);
			mockFtpTransferrer.Expect(x => x.RenameFile("RENAMEBEFOREDOWNLOAD-FILE2.002-FILE2-.002", "RENAMEAFTERDOWNLOAD-FILE2.002-FILE2-.002"));
			mockFtpTransferrer.Expect(x => x.RenameFile("RENAMEBEFOREDOWNLOAD-FILE3.003-FILE3-.003", "RENAMEAFTERDOWNLOAD-FILE3.003-FILE3-.003"));
			mockFtpTransferrer.Expect(x => x.RenameFile("RENAMEBEFOREDOWNLOAD-FILE4.004-FILE4-.004", "RENAMEAFTERDOWNLOAD-FILE4.004-FILE4-.004"));
			mockFtpTransferrer.Expect(x => x.Close()).Repeat.Never();
			mockFtpTransferrer.Expect(x => x.Dispose()).Repeat.AtLeastOnce();

			var mockTransferrerFactory = MockRepository.GenerateMock<FtpTransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(mockFtpTransferrer);
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(new MemoryStream());
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			var stubBatch = MockRepository.GenerateStub<ISyncReceiveSubmitBatch>();
			stubBatch.Expect(x => x.Wait()).Return(true);
			var stubBatchFactory = MockRepository.GenerateStub<ISyncReceiveSubmitBatchFactory>();
			stubBatchFactory.Expect(x => x.CreateBatch(Arg<IBTTransportProxy>.Is.Anything, Arg<ControlledTermination>.Is.Anything, Arg<int>.Is.Anything)).Return(stubBatch);
			var mockMessageFactory = MockRepository.GenerateMock<ITransferrerMessageFactory>();

			var mockRecieverEndpoints = MockRepository.GenerateMock<IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>>();
			mockRecieverEndpoints.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new ConcurrentDictionary<string, TransferrerReceiverEndpoint>());
			var mockRecieverLoggers = MockRepository.GenerateMock<IDictionary<Type, ILog>>();
			mockRecieverLoggers.Stub(x => x[typeof(object)]).IgnoreArguments().Return(LogManager.GetLogger(typeof(TransferrerReceiver).FullName));
			var mockHostCancelTokenSource = MockRepository.GenerateMock<IDictionary<Type, CancellationTokenSource>>();
			mockHostCancelTokenSource.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new CancellationTokenSource());

			TransferrerReceiver.ReceiverEndpoints = mockRecieverEndpoints;
			TransferrerReceiver.ReceiverLoggers = mockRecieverLoggers;
			TransferrerReceiver.HostCancelTokenSource = mockHostCancelTokenSource;

			var target = MockRepository.GeneratePartialMock<FtpExReceiverEndpoint>(mockTransferrerFactory, stubBatchFactory, mockMessageFactory, new TransferrerProperties.ReceiveFactory());
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE2.002"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs2))).Return(stubMessage);
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE3.003"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs3))).Return(stubMessage);
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE4.004"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs4))).Return(stubMessage);
			target.Stub(x => x.EndpointTask());
			string portName;
			target.Stub(x => x.TryGetPortName(out portName)).Return(false);

			target.Open(uri, fakeConfig, bizTalkConfig, handlerPropertyBag, transportProxy, transportType, propertyNamespace, control);

			target.DownloadFilesAndSubmit();

			target.VerifyAllExpectations();
			mockFtpTransferrer.VerifyAllExpectations();
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_MultipleLocationsAndFlagFiles()
		{
			string uri = "URI://USR@SVR";

			var token = new CancellationTokenSource();
			var configXml = new XElement("Config",
								new XElement("Server"),
								new XElement("Port", "0"),
								new XElement("User"),
								new XElement("Password"),
								new XElement("MultipleLocations", "ftpex://USER@SERVER:21/FOLDER1/*.xml" + Environment.NewLine + "ftpex://USER%2FCARGOWISE@SERVER:21/FOLDER2/*.xml"),
								new XElement("MultipleLocationsCredentials", "USER:Pa%23%23word@SERVER:21" + Environment.NewLine + "USER%2FCARGOWISE:Pa%24%24word@SERVER:21"),
								new XElement("PollingInterval", "30"),
								new XElement("PollingUnit", "Seconds"),
								new XElement("FlagFile", "{f}.ok"),
								new XElement("RenameBeforeDownload", "RENAMEBEFOREDOWNLOAD-{f}-{n}-{x}"),
								new XElement("RenameAfterDownload", "RENAMEAFTERDOWNLOAD-{f}-{n}-{x}"),
								new XElement("EmptyFileOption", "Ignore"),
								new XElement("SortOrder", "Timestamp")
								);
			var fakeConfig = new FakePropertyBag() { Config = configXml.ToString() };
			var bizTalkConfig = MockRepository.GenerateStub<IPropertyBag>();

			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			string transportType = "FtpEx";
			string propertyNamespace = "http://cargowise.com/ehub/biztalkadapters/FtpEx-properties";
			var control = new ControlledTermination();
			var logger = LogManager.GetLogger(GetType().FullName);

			var mockFtpTransferrer = MockRepository.GenerateMock<ITransferrer, IDisposable>();
			mockFtpTransferrer.Stub(x => x.Logger).Return(logger);
			var mockTransferrerFactory = MockRepository.GenerateMock<FtpTransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(mockFtpTransferrer);
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(new MemoryStream());
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			var stubBatch = MockRepository.GenerateStub<ISyncReceiveSubmitBatch>();
			stubBatch.Expect(x => x.Wait()).Return(true);
			var stubBatchFactory = MockRepository.GenerateStub<ISyncReceiveSubmitBatchFactory>();
			stubBatchFactory.Expect(x => x.CreateBatch(Arg<IBTTransportProxy>.Is.Anything, Arg<ControlledTermination>.Is.Anything, Arg<int>.Is.Anything)).Return(stubBatch);
			var mockMessageFactory = MockRepository.GenerateMock<ITransferrerMessageFactory>();

			var mockRecieverEndpoints = MockRepository.GenerateMock<IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>>();
			mockRecieverEndpoints.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new ConcurrentDictionary<string, TransferrerReceiverEndpoint>());
			var mockRecieverLoggers = MockRepository.GenerateMock<IDictionary<Type, ILog>>();
			mockRecieverLoggers.Stub(x => x[typeof(object)]).IgnoreArguments().Return(LogManager.GetLogger(typeof(TransferrerReceiver).FullName));
			var mockHostCancelTokenSource = MockRepository.GenerateMock<IDictionary<Type, CancellationTokenSource>>();
			mockHostCancelTokenSource.Stub(x => x[typeof(object)]).IgnoreArguments().Return(token);

			TransferrerReceiver.ReceiverEndpoints = mockRecieverEndpoints;
			TransferrerReceiver.ReceiverLoggers = mockRecieverLoggers;
			TransferrerReceiver.HostCancelTokenSource = mockHostCancelTokenSource;

			var target = MockRepository.GeneratePartialMock<FtpExReceiverEndpoint>(mockTransferrerFactory, stubBatchFactory, mockMessageFactory, new TransferrerProperties.ReceiveFactory());
			target.Stub(x => x.EndpointTask());
			target.Stub(x => x.TryGetPortName(out var portName)).Return(false);

			target.Open(uri, fakeConfig, bizTalkConfig, handlerPropertyBag, transportProxy, transportType, propertyNamespace, control);
			Assert.AreEqual("Pa##word", target.properties.MultipleLocations[0].Password);
			Assert.AreEqual("USER/CARGOWISE", target.properties.MultipleLocations[1].UserName);
			Assert.AreEqual("Pa$$word", target.properties.MultipleLocations[1].Password);

			mockFtpTransferrer.Expect(x => x.ListFiles(Arg<TransferrerProperties.Receive.Location>.Is.Equal(target.properties.MultipleLocations[0]), Arg<TransferrerProperties.Receive>.Is.Equal(target.properties), Arg<CancellationTokenSource>.Is.Anything)).Return(
				new List<TransferrerFileInfo> {
						new TransferrerFileInfo { Name = "FILE1.001.xml", Timestamp = DateTime.Parse("1999-12-31 00:00"), Size = 500 },
						new TransferrerFileInfo { Name = "FILE2.002.xml", Timestamp = DateTime.Parse("2002-01-01 00:00"), Size = 1000 }
					}
			);

			var fs1 = CreatePaddedStream(500);
			mockFtpTransferrer.Expect(x => x.RenameFile("FOLDER1/FILE1.001.xml", "FOLDER1/RENAMEBEFOREDOWNLOAD-FILE1.001.xml-FILE1.001-.xml"));
			mockFtpTransferrer.Expect(x => x.GetFile("FOLDER1/RENAMEBEFOREDOWNLOAD-FILE1.001.xml-FILE1.001-.xml")).Return(fs1);
			mockFtpTransferrer.Expect(x => x.RenameFile("FOLDER1/RENAMEBEFOREDOWNLOAD-FILE1.001.xml-FILE1.001-.xml", "FOLDER1/RENAMEAFTERDOWNLOAD-FILE1.001.xml-FILE1.001-.xml"));
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE1.001.xml"), Arg<string>.Is.Equal("ftpex://USER@SERVER:21/FOLDER1/*.xml"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs1))).Return(stubMessage);
			mockFtpTransferrer.Expect(x => x.DeleteFile("FOLDER1/FILE1.001.xml.ok"));

			var fs2 = CreatePaddedStream(1000);
			mockFtpTransferrer.Expect(x => x.RenameFile("FOLDER1/FILE2.002.xml", "FOLDER1/RENAMEBEFOREDOWNLOAD-FILE2.002.xml-FILE2.002-.xml"));
			mockFtpTransferrer.Expect(x => x.GetFile("FOLDER1/RENAMEBEFOREDOWNLOAD-FILE2.002.xml-FILE2.002-.xml")).Return(fs2);
			mockFtpTransferrer.Expect(x => x.RenameFile("FOLDER1/RENAMEBEFOREDOWNLOAD-FILE2.002.xml-FILE2.002-.xml", "FOLDER1/RENAMEAFTERDOWNLOAD-FILE2.002.xml-FILE2.002-.xml"));
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE2.002.xml"), Arg<string>.Is.Equal("ftpex://USER@SERVER:21/FOLDER1/*.xml"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs2))).Return(stubMessage);
			mockFtpTransferrer.Expect(x => x.DeleteFile("FOLDER1/FILE2.002.xml.ok"));


			mockFtpTransferrer.Expect(x => x.ListFiles(Arg<TransferrerProperties.Receive.Location>.Is.Equal(target.properties.MultipleLocations[1]), Arg<TransferrerProperties.Receive>.Is.Equal(target.properties), Arg<CancellationTokenSource>.Is.Anything)).Return(
				new List<TransferrerFileInfo> {
					new TransferrerFileInfo { Name = "FILE3.003.xml", Timestamp = DateTime.Parse("1999-12-31 00:00"), Size = 500 }
				}
			);

			var fs3 = CreatePaddedStream(500);
			mockFtpTransferrer.Expect(x => x.RenameFile("FOLDER2/FILE3.003.xml", "FOLDER2/RENAMEBEFOREDOWNLOAD-FILE3.003.xml-FILE3.003-.xml"));
			mockFtpTransferrer.Expect(x => x.GetFile("FOLDER2/RENAMEBEFOREDOWNLOAD-FILE3.003.xml-FILE3.003-.xml")).Return(fs3);
			mockFtpTransferrer.Expect(x => x.RenameFile("FOLDER2/RENAMEBEFOREDOWNLOAD-FILE3.003.xml-FILE3.003-.xml", "FOLDER2/RENAMEAFTERDOWNLOAD-FILE3.003.xml-FILE3.003-.xml"));
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE3.003.xml"), Arg<string>.Is.Equal("ftpex://USER%2FCARGOWISE@SERVER:21/FOLDER2/*.xml"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs3))).Return(stubMessage);
			mockFtpTransferrer.Expect(x => x.DeleteFile("FOLDER2/FILE3.003.xml.ok"));


			target.DownloadFilesAndSubmit();
			target.VerifyAllExpectations();

			mockFtpTransferrer.VerifyAllExpectations();
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DoesNotThrowExceptions()
		{
			string uri = "URI://USR@SVR";
			var receive = new TransferrerProperties.Receive(uri);
			var location = new TransferrerProperties.Receive.Location
			{
				Folder = "FOLDER1",
				FileMask = "*.xml"
			};

			var token = new CancellationTokenSource();
			var configXml = new XElement("Config",
							new XElement("Server", "SERVER"),
							new XElement("Port", "0"),
							new XElement("User", "USER"),
							new XElement("Password", "PASSWORD"),
							new XElement("PollingInterval", "30"),
							new XElement("PollingUnit", "Seconds"),
							new XElement("EmptyFileOption", "Ignore"),
							new XElement("SortOrder", "None")
			);
			var fakeConfig = new FakePropertyBag() { Config = configXml.ToString() };
			var bizTalkConfig = MockRepository.GenerateStub<IPropertyBag>();

			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			string transportType = "FtpEx";
			string propertyNamespace = "http://cargowise.com/ehub/biztalkadapters/FtpEx-properties";
			var control = new ControlledTermination();
			var logger = LogManager.GetLogger(GetType().FullName);

			var mockFtpTransferrer = MockRepository.GenerateMock<ITransferrer, IDisposable>();
			mockFtpTransferrer.Stub(x => x.Logger).Return(logger);

			var stubBatch = MockRepository.GenerateStub<ISyncReceiveSubmitBatch>();
			stubBatch.Expect(x => x.Wait()).Return(true);
			var stubBatchFactory = MockRepository.GenerateStub<ISyncReceiveSubmitBatchFactory>();
			stubBatchFactory.Expect(x => x.CreateBatch(Arg<IBTTransportProxy>.Is.Anything, Arg<ControlledTermination>.Is.Anything, Arg<int>.Is.Anything)).Return(stubBatch);
			var mockMessageFactory = MockRepository.GenerateMock<ITransferrerMessageFactory>();

			var mockTransferrerFactory = MockRepository.GenerateMock<FtpTransferrerFactory>();
			mockTransferrerFactory.Expect(x => x.CreateTransferrer()).Return(mockFtpTransferrer);

			var mockRecieverEndpoints = MockRepository.GenerateMock<IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>>();
			mockRecieverEndpoints.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new ConcurrentDictionary<string, TransferrerReceiverEndpoint>());
			var mockRecieverLoggers = MockRepository.GenerateMock<IDictionary<Type, ILog>>();
			mockRecieverLoggers.Stub(x => x[typeof(object)]).IgnoreArguments().Return(LogManager.GetLogger(typeof(TransferrerReceiver).FullName));
			var mockHostCancelTokenSource = MockRepository.GenerateMock<IDictionary<Type, CancellationTokenSource>>();
			mockHostCancelTokenSource.Stub(x => x[typeof(object)]).IgnoreArguments().Return(token);

			TransferrerReceiver.ReceiverEndpoints = mockRecieverEndpoints;
			TransferrerReceiver.ReceiverLoggers = mockRecieverLoggers;
			TransferrerReceiver.HostCancelTokenSource = mockHostCancelTokenSource;

			var target = MockRepository.GeneratePartialMock<FtpExReceiverEndpoint>(mockTransferrerFactory, stubBatchFactory, mockMessageFactory, new TransferrerProperties.ReceiveFactory());
			target.Stub(x => x.EndpointTask());
			target.Stub(x => x.TryGetPortName(out var portName)).Return(false);
			target.Open(uri, fakeConfig, bizTalkConfig, handlerPropertyBag, transportProxy, transportType, propertyNamespace, control);


			mockFtpTransferrer.Expect(x => x.ListFiles(null, null, null)).IgnoreArguments().Return(new List<TransferrerFileInfo> {
						new TransferrerFileInfo { Name = "FILE1.001.xml", Timestamp = DateTime.Parse("1999-12-31 00:00"), Size = 500 }
					}
			);

			var fs1 = CreatePaddedStream(500);
			mockFtpTransferrer.Expect(x => x.GetFile("")).IgnoreArguments().Return(fs1);
			mockMessageFactory.Expect(x => x.CreateMessage(Arg<IBaseMessageFactory>.Is.Anything, Arg<string>.Is.Equal("FILE1.001.xml"), Arg<string>.Is.Equal("URI://USR@SVR"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<Stream>.Is.Equal(fs1)))
				.Throw(new Exception("Test exception to catch"));

			target.DownloadFilesAndSubmit();
			target.VerifyAllExpectations();

			mockFtpTransferrer.VerifyAllExpectations();
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ConcurrentDownload_DownloadInOriginalOrder_RandomDownloadTimes()
		{
			var testCase = new ParallelTestCase(500);
			testCase.MaxConcurrentDownloads = 20;

			string uri = "URI://USR@SVR";
			var configXml = new XElement("Config",
				new XElement("Server", "SERVER"),
				new XElement("Port", "0"),
				new XElement("User", "USER"),
				new XElement("Password", "PASSWORD"),
				new XElement("PollingInterval", "300"),
				new XElement("PollingUnit", "Seconds"),
				new XElement("EmptyFileOption", "Ignore"),
				new XElement("SortOrder", "None"),
				new XElement("FlagFile", "{f}-FLAG"),
				new XElement("RenameBeforeDownload", "{f}-RENAME BEFORE"),
				new XElement("RenameAfterDownload", "{f}-RENAME AFTER"),
				new XElement("MaximumConcurrentDownloads", testCase.MaxConcurrentDownloads)
			);
			var fakeConfig = new FakePropertyBag() { Config = configXml.ToString() };
			var bizTalkConfig = MockRepository.GenerateStub<IPropertyBag>();
			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			string transportType = "FtpEx";
			string propertyNamespace = "http://cargowise.com/ehub/biztalkadapters/FtpEx-properties";
			var control = new ControlledTermination();

			var mockTransferrerFactory = MockRepository.GenerateStub<FtpTransferrerFactory>();

			mockTransferrerFactory.Stub(x => x.CreateTransferrer())
						   .Do((Func<ITransferrer>)(() =>
			{
				return new FtpTransferrerForTest(testCase);
			}));

			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(new MemoryStream());
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			var stubBatch = MockRepository.GenerateStub<ISyncReceiveSubmitBatch>();
			stubBatch.Expect(x => x.Wait()).Return(true);
			var stubBatchFactory = MockRepository.GenerateStub<ISyncReceiveSubmitBatchFactory>();
			stubBatchFactory.Expect(x => x.CreateBatch(Arg<IBTTransportProxy>.Is.Anything, Arg<ControlledTermination>.Is.Anything, Arg<int>.Is.Anything)).Return(stubBatch);

			var mockRecieverEndpoints = MockRepository.GenerateMock<IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>>();
			mockRecieverEndpoints.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new ConcurrentDictionary<string, TransferrerReceiverEndpoint>());
			var mockRecieverLoggers = MockRepository.GenerateMock<IDictionary<Type, ILog>>();
			mockRecieverLoggers.Stub(x => x[typeof(object)]).IgnoreArguments().Return(LogManager.GetLogger(typeof(TransferrerReceiver).FullName));
			var mockHostCancelTokenSource = MockRepository.GenerateMock<IDictionary<Type, CancellationTokenSource>>();
			mockHostCancelTokenSource.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new CancellationTokenSource());

			TransferrerReceiver.ReceiverEndpoints = mockRecieverEndpoints;
			TransferrerReceiver.ReceiverLoggers = mockRecieverLoggers;
			TransferrerReceiver.HostCancelTokenSource = mockHostCancelTokenSource;

			var messageFactory = new FtpExMessageFactoryForTest(testCase, () => { return stubMessage; });
			var target = new FtpExReceiverEndpointForTest(messageFactory, mockTransferrerFactory, stubBatchFactory, new TransferrerProperties.ReceiveFactory());

			Stopwatch actualTime = Stopwatch.StartNew();
			target.Open(uri, fakeConfig, bizTalkConfig, handlerPropertyBag, transportProxy, transportType, propertyNamespace, control);
			target.DownloadFilesAndSubmitCompleted.Wait();
			actualTime.Stop();

			Assert.IsTrue(testCase.NumberofOpenedConnections <= testCase.MaxConcurrentDownloads + 1, $"Number of open connections of {testCase.NumberofOpenedConnections} exceeds maximum of {testCase.MaxConcurrentDownloads + 1}");
			Assert.AreEqual(testCase.files.Length, testCase.MessageSubmits.Count);
			var previousSubmit = -1;
			foreach (var submit in testCase.MessageSubmits.ToArray())
			{
				Assert.IsTrue(submit > previousSubmit, "The order of submitted messages is different than the order of server listed files.");
				previousSubmit = submit;
			}

			var totalDownloadTime = testCase.files.Sum(x => x.DownloadElapsedTime.TotalMilliseconds);
			Assert.IsTrue(actualTime.ElapsedMilliseconds < totalDownloadTime / 2, "The actual time ({0} ms) was more than half the total wait time {1}, are we doing the downloads in parallel?", actualTime.ElapsedMilliseconds, totalDownloadTime);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ConcurrentDownload_DownloadInOriginalOrder_ReverseOrderDownloadTimes()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 10, FirstDownloadTime: 1000, LastDownloadTime: 0, BatchWaitTime: 20);
			testCase.MaxConcurrentDownloads = 10;

			ExecuteTestCase(testCase);

			var previousSubmit = -1;
			foreach (var submit in testCase.MessageSubmits.ToArray())
			{
				Assert.IsTrue(submit == previousSubmit + 1, "The order of submitted messages is different than the order of server listed files.");
				previousSubmit = submit;
			}

		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ConcurrentDownload_Stress()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 10000);
			testCase.MaxConcurrentDownloads = 200;

			ExecuteTestCase(testCase);

			var previousSubmit = -1;
			foreach (var submit in testCase.MessageSubmits.ToArray())
			{
				Assert.IsTrue(submit == previousSubmit + 1, "The order of submitted messages is different than the order of server listed files.");
				previousSubmit = submit;
			}

			var log = testCase.Log.ToArray();

			Assert.IsNotNull(log.FirstOrDefault(x => x.Item1 == "Info" && x.Item2.Contains("Downloading files using 200 maximum number of workers.")));
			Assert.IsTrue(testCase.NumberofOpenedConnections <= testCase.MaxConcurrentDownloads + 1, $"Number of open connections of {testCase.NumberofOpenedConnections} exceeds maximum of {testCase.MaxConcurrentDownloads + 1}");
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ConcurrentDownload_DontDownloadTooMuchAhead()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 1000, FirstDownloadTime: 100, LastDownloadTime: 100, BatchWaitTime: 10);
			testCase.MaxConcurrentDownloads = 10;

			ExecuteTestCase(testCase);

			var previousSubmit = -1;
			foreach (var submit in testCase.MessageSubmits.ToArray())
			{
				Assert.IsTrue(submit == previousSubmit + 1, "The order of submitted messages is different than the order of server listed files.");
				previousSubmit = submit;
			}

			var downloadLogs = from x in testCase.files
							   select new { fileNumber = x.fileNumber, time = x.DownloadBeginTime, action = "download" };
			var submitLogs = from x in testCase.files
							 select new { fileNumber = x.fileNumber, time = x.SubmitBeginTime, action = "submit" };

			var biggestGap = submitLogs.Select(s => downloadLogs.Count(d => d.time < s.time) - s.fileNumber).Max();
			var biggestAcceptableGap = testCase.MaxConcurrentDownloads;
			Assert.IsTrue(biggestGap <= testCase.MaxConcurrentDownloads, $"Submits were {biggestGap} behind downloads, expected no more than {biggestAcceptableGap}");
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ConcurrentDownload_DontEnumerateTooMuchAhead()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 1000, FirstDownloadTime: 100, LastDownloadTime: 100, BatchWaitTime: 10);
			testCase.MaxConcurrentDownloads = 10;

			ExecuteTestCase(testCase);

			var previousSubmit = -1;
			foreach (var submit in testCase.MessageSubmits.ToArray())
			{
				Assert.IsTrue(submit == previousSubmit + 1, "The order of submitted messages is different than the order of server listed files.");
				previousSubmit = submit;
			}

			var EnumeratorYield = from x in testCase.files
								  select new { fileNumber = x.fileNumber, time = x.EnumeratorYieldTime, action = "yield" };
			var submitLogs = from x in testCase.files
							 select new { fileNumber = x.fileNumber, time = x.SubmitBeginTime, action = "submit" };

			var biggestGap = submitLogs.Select(s => EnumeratorYield.Count(d => d.time < s.time) - s.fileNumber).Max();
			var biggestAcceptableGap = testCase.MaxConcurrentDownloads + 1;
			Assert.IsTrue(biggestGap <= biggestAcceptableGap, $"Submits were {biggestGap} behind file enumerations, expected no more than {biggestAcceptableGap}");
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ConcurrentDownload_NumberOfWorkers()
		{
			var filesBeingDownloaded = 0;
			var numberOfFiles = 5;
			var testCase = new ParallelTestCase(NumberOfFiles: numberOfFiles);
			foreach (var testCaseFile in testCase.files)
			{
				testCaseFile.DuringDownloadAction = () =>
				{
					Interlocked.Increment(ref filesBeingDownloaded);
					for (int i = 0; i < 300 && filesBeingDownloaded < numberOfFiles; ++i)
					{
						Thread.Sleep(100);
					}
				};
			}
			testCase.MaxConcurrentDownloads = 10;

			ExecuteTestCase(testCase);

			var log = testCase.Log.ToArray();

			Assert.IsNotNull(log.FirstOrDefault(x => x.Item1 == "Info" && x.Item2.Contains("Downloading files using 10 maximum number of workers.")));
			Assert.AreEqual(numberOfFiles + 1, testCase.NumberofOpenedConnections);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ParallelDownload_ExceptionDuringDownload()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 2;
			testCase.files[2].DuringDownloadAction = () => throw new Exception("Download File Exception");

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ParallelDownload_ExceptionDuringDownload_FirstFile()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 100, FirstDownloadTime: 200, LastDownloadTime: 200, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 100;
			testCase.files[0].DuringDownloadAction = () => throw new Exception("Download File Exception");

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			for (int i = 1; i < testCase.files.Length; i++)
			{
				var file = testCase.files[i];
				Assert.AreEqual(file.isFileRenamedBeforeDownload, file.isFileNameRestored);
				Assert.IsFalse(file.isFileSubmitted);
			}
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ParallelDownload_ExceptionDuringSubmit()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 2;
			testCase.files[2].DuringSubmitAction = () =>
				throw new Exception("Submit File Exception");

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: true, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ParallelDownload_CancellationDuringDownload()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 2;
			testCase.files[2].DuringDownloadAction = () => testCase.CancelHost();

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ParallelDownload_CancellationDuringDownload_FirstFile()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 100, FirstDownloadTime: 200, LastDownloadTime: 200, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 100;
			testCase.files[0].DuringDownloadAction = () => testCase.CancelHost();

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			for (int i = 1; i < testCase.files.Length; i++)
			{
				var file = testCase.files[i];
				Assert.AreEqual(file.isFileRenamedBeforeDownload, file.isFileNameRestored);
				Assert.IsFalse(file.isFileSubmitted);
			}
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_ParallelDownload_CancellationDuringSubmit()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 2;
			testCase.files[2].DuringSubmitAction = () => testCase.CancelHost();

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_SequentialDownload_ExceptionDuringDownload()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 1;
			testCase.files[2].DuringDownloadAction = () => throw new Exception("Download File Exception");

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_SequentialDownload_ExceptionDuringSubmit()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 1;
			testCase.files[2].DuringSubmitAction = () =>
				throw new Exception("Submit File Exception");

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: true, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_SequentialDownload_CancellationDuringDownload()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 1;
			testCase.files[2].DuringDownloadAction = () => testCase.CancelHost();

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_SequentialDownload_CancellationDuringSubmit()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 5, FirstDownloadTime: 500, LastDownloadTime: 100, BatchWaitTime: 50);
			testCase.MaxConcurrentDownloads = 1;
			testCase.files[2].DuringSubmitAction = () => testCase.CancelHost();

			ExecuteTestCase(testCase);

			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
			testCase.files[4].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_SequentialDownlaod_TestNoFilesToDownload()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 0);
			testCase.MaxConcurrentDownloads = 1;

			ExecuteTestCase(testCase);

			Assert.AreEqual(1, testCase.NumberofOpenedConnections);
			Assert.AreEqual(1, testCase.NumberofClosedConnections);

			var log = testCase.Log.ToArray();

			Assert.IsNotNull(log.FirstOrDefault(x => x.Item1 == "Info" && x.Item2.Contains("Starting receive for location 'URI://USR@SVR'.")));
			Assert.IsNotNull(log.FirstOrDefault(x => x.Item1 == "Info" && x.Item2.Contains("The number of downloaded file(s) is 0.")));
			Assert.IsNotNull(log.FirstOrDefault(x => x.Item1 == "Debug" && x.Item2.Contains("Finished receive for location.")));
		}

		[TestMethod()]
		public void TransferrerReceiverEndpoint_DownloadAndSubmit_RetryDownload()
		{
			var testCase = new ParallelTestCase(NumberOfFiles: 10);
			testCase.DownloadRetryDelay = 500;
			testCase.DownloadRetries = 1;
			testCase.MaxConcurrentDownloads = 1;

			int downloadAttempts1 = 0;
			testCase.files[1].DuringDownloadAction = () =>
			{
				downloadAttempts1++;
				if (downloadAttempts1 == 1)
				{
					throw new Exception("Test Exception To be retried");
				}
			};

			int downloadAttempts2 = 0;
			testCase.files[3].DuringDownloadAction = () =>
			{
				downloadAttempts2++;
				throw new Exception("Test Exception 2");
			};

			ExecuteTestCase(testCase);

			Assert.AreEqual(2, downloadAttempts1);
			testCase.files[0].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[1].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[2].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: true, isFileSubmitted: true, isFileRenamedAfterDownload: true, isFileNameRestored: false, isFlagFileDeleted: true);
			testCase.files[3].VerifyExpectations(isFileRenamedBeforeDownload: true, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: true, isFlagFileDeleted: false);
			testCase.files[5].VerifyExpectations(isFileRenamedBeforeDownload: false, isFileDownloaded: false, isFileSubmitted: false, isFileRenamedAfterDownload: false, isFileNameRestored: false, isFlagFileDeleted: false);

		}


		private void ExecuteTestCase(ParallelTestCase testCase, FtpTransferrerFactory transferrerFactory = null)
		{
			string uri = "URI://USR@SVR";
			var configXml = new XElement("Config",
				new XElement("Server", "SERVER"),
				new XElement("Port", "0"),
				new XElement("User", "USER"),
				new XElement("Password", "PASSWORD"),
				new XElement("PollingInterval", "300"),
				new XElement("PollingUnit", "Seconds"),
				new XElement("EmptyFileOption", "Ignore"),
				new XElement("SortOrder", "None"),
				new XElement("FlagFile", "{f}-FLAG"),
				new XElement("RenameBeforeDownload", "{f}-RENAME BEFORE"),
				new XElement("RenameAfterDownload", "{f}-RENAME AFTER"),
				new XElement("MaximumConcurrentDownloads", testCase.MaxConcurrentDownloads),
				new XElement("ConnectionRetries", testCase.ConnectionRetries),
				new XElement("ConnectionRetryDelay", testCase.ConnectionRetryDelay),
				new XElement("DownloadRetries", testCase.DownloadRetries),
				new XElement("DownloadRetryDelay", testCase.DownloadRetryDelay),
				new XElement("Logging", true)
			);
			var fakeConfig = new FakePropertyBag() { Config = configXml.ToString() };
			var bizTalkConfig = MockRepository.GenerateStub<IPropertyBag>();
			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			string transportType = "FtpEx";
			string propertyNamespace = "http://cargowise.com/ehub/biztalkadapters/FtpEx-properties";
			var control = new ControlledTermination();

			if (transferrerFactory == null)
			{
				transferrerFactory = MockRepository.GenerateStub<FtpTransferrerFactory>();
				transferrerFactory.Stub(x => x.CreateTransferrer())
					.Do((Func<ITransferrer>)(() => { return new FtpTransferrerForTest(testCase); }));
			}

			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(new MemoryStream());
			var stubMessage = MockRepository.GenerateStub<IBaseMessage>();
			stubMessage.Expect(x => x.BodyPart).Return(stubMessagePart);
			var stubBatch = MockRepository.GenerateStub<ISyncReceiveSubmitBatch>();
			stubBatch.Expect(x => x.Wait()).Do(new Func<bool>(() =>
			{
				if (testCase.BatchWaitTime > 0)
				{
					Thread.Sleep(testCase.BatchWaitTime);
				}
				return true;
			}));
			var stubBatchFactory = MockRepository.GenerateStub<ISyncReceiveSubmitBatchFactory>();
			stubBatchFactory.Expect(x => x.CreateBatch(Arg<IBTTransportProxy>.Is.Anything, Arg<ControlledTermination>.Is.Anything, Arg<int>.Is.Anything)).Return(stubBatch);

			testCase.SetupTransferrerReceiver();

			var messageFactory = new FtpExMessageFactoryForTest(testCase, () => { return stubMessage; });
			using (var target = new FtpExReceiverEndpointForTest(messageFactory, transferrerFactory, stubBatchFactory, new TransferrerProperties.ReceiveFactory()))
			{
				target.Open(uri, fakeConfig, bizTalkConfig, handlerPropertyBag, transportProxy, transportType, propertyNamespace, control);
				target.properties.Logger = TransferrerReceiver.ReceiverLoggers[typeof(FtpExReceiverEndpointForTest)];
				target.DownloadFilesAndSubmitCompleted.Wait();
				if (testCase.HostIsCancelled)
				{
					Assert.IsTrue(target.Task.Wait(TimeSpan.FromSeconds(1)), "The endpoint should have stopped as it was cancelled, see log below:\r\n" + LogToString(testCase.Log));
				}
				else
				{
					Assert.IsTrue(target.WaitForNextScheduledExecutionStarted.Wait(TimeSpan.FromSeconds(1)), "The endpoint should still be running, waiting for it's next schedule download, see log below:\r\n" + LogToString(testCase.Log));
				}
			}
		}

		static string LogToString(ConcurrentQueue<Tuple<string, string>> log)
		{
			return string.Join("\r\n", log.Select(t => string.Format("{0} {1}", t.Item1, t.Item2)));
		}

		static ILog CreateStubLogger(ConcurrentQueue<Tuple<string, string>> log)
		{
			var mockLog = MockRepository.GenerateStub<ILog>();
			mockLog.Stub(x => x.IsDebugEnabled).Repeat.Any().Return(true);
			mockLog.Stub(x => x.Debug("")).IgnoreArguments().Do(new Action<object>((message) => { log.Enqueue(new Tuple<string, string>("Debug", message.ToString())); })).Repeat.Any();
			mockLog.Stub(x => x.IsErrorEnabled).Return(true).Repeat.Any();
			mockLog.Stub(x => x.Error("")).IgnoreArguments().Do(new Action<object>((message) => { log.Enqueue(new Tuple<string, string>("Error", message.ToString())); })).Repeat.Any();
			mockLog.Stub(x => x.IsInfoEnabled).Return(true).Repeat.Any();
			mockLog.Stub(x => x.Info("")).IgnoreArguments().Do(new Action<object>((message) => { log.Enqueue(new Tuple<string, string>("Info", message.ToString())); })).Repeat.Any();
			mockLog.Stub(x => x.IsTraceEnabled).Return(true).Repeat.Any();
			mockLog.Stub(x => x.Trace("")).IgnoreArguments().Do(new Action<object>((message) => { log.Enqueue(new Tuple<string, string>("Trace", message.ToString())); })).Repeat.Any();
			mockLog.Stub(x => x.IsWarnEnabled).Return(true).Repeat.Any();
			mockLog.Stub(x => x.Warn("")).IgnoreArguments().Do(new Action<object>((message) => { log.Enqueue(new Tuple<string, string>("Warn", message.ToString())); })).Repeat.Any();
			return mockLog;
		}

		private class ParallelTestCase
		{
			internal VirtualFile[] files;
			public int MaxConcurrentDownloads { get; set; } = 1;
			public int ConnectionRetries { get; set; } = 0;
			public int ConnectionRetryDelay { get; set; } = 1000;
			public int DownloadRetries { get; set; } = 0;
			public int DownloadRetryDelay { get; set; } = 1000;


			public int BatchWaitTime = 0;
			public bool HostIsCancelled { get; private set; }
			CancellationTokenSource HostCancellationTokenSource;

			// Assessments :

			public int NumberofOpenedConnections = 0;
			public int NumberofClosedConnections = 0;
			public readonly ConcurrentQueue<int> GetFileRequests = new ConcurrentQueue<int>();
			public readonly ConcurrentQueue<int> MessageSubmits = new ConcurrentQueue<int>();
			public readonly ConcurrentQueue<Tuple<string, string>> Log = new ConcurrentQueue<Tuple<string, string>>();

			internal ParallelTestCase(int NumberOfFiles)
			{
				HostCancellationTokenSource = new CancellationTokenSource();
				Random rnd = new Random();

				files = (from i in Enumerable.Range(0, NumberOfFiles)
						 select new VirtualFile()
						 {
							 fileNumber = i,
							 fileName = $"File{i}",
							 downloadTime = rnd.Next(10) + (rnd.Next(10) == 1 ? 100 : 0),
							 FileStream = CreateRandomStream(rnd.Next(50))
						 }).ToArray();
			}
			internal ParallelTestCase(int NumberOfFiles, int FirstDownloadTime, int LastDownloadTime, int BatchWaitTime) : this(NumberOfFiles)
			{
				if (NumberOfFiles == 1)
				{
					files[0].downloadTime = FirstDownloadTime;
				}
				else if (NumberOfFiles > 1)
				{
					for (int i = 0; i < files.Length; i++)
					{
						files[i].downloadTime = ((LastDownloadTime - FirstDownloadTime) / (files.Length - 1)) * i + FirstDownloadTime;
					}
				}
				this.BatchWaitTime = BatchWaitTime;
			}

			internal void CancelHost()
			{
				HostCancellationTokenSource.Cancel();
				HostIsCancelled = true;
			}

			internal void SetupTransferrerReceiver()
			{
				var mockRecieverEndpoints = MockRepository.GenerateMock<IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>>();
				mockRecieverEndpoints.Stub(x => x[typeof(object)]).IgnoreArguments().Return(new ConcurrentDictionary<string, TransferrerReceiverEndpoint>());
				var mockRecieverLoggers = MockRepository.GenerateMock<IDictionary<Type, ILog>>();
				var mockLog = CreateStubLogger(Log);
				mockRecieverLoggers.Stub(x => x[typeof(object)]).IgnoreArguments().Return(mockLog);
				var mockHostCancelTokenSource = MockRepository.GenerateMock<IDictionary<Type, CancellationTokenSource>>();
				mockHostCancelTokenSource.Stub(x => x[typeof(object)]).IgnoreArguments().Return(HostCancellationTokenSource);

				TransferrerReceiver.ReceiverEndpoints = mockRecieverEndpoints;
				TransferrerReceiver.ReceiverLoggers = mockRecieverLoggers;
				TransferrerReceiver.HostCancelTokenSource = mockHostCancelTokenSource;
			}

			static Stream CreateRandomStream(int length)
			{
				Random rnd = new Random();
				var buffer = new byte[length];
				for (int i = 0; i < buffer.Length; i++)
				{
					buffer[i] = (byte)rnd.Next(32, 127);
				}
				return new MemoryStream(buffer);
			}

			internal class VirtualFile
			{
				public int fileNumber;
				public string fileName;
				public int downloadTime;
				public Stream FileStream;
				Stopwatch downloadStopwatch = new Stopwatch();

				public DateTime DownloadBeginTime;
				public DateTime SubmitBeginTime;
				public DateTime EnumeratorYieldTime;

				public Action DuringDownloadAction;
				public Action DuringSubmitAction;

				// Assessments:

				public bool isFileDownloaded { get; set; }
				public bool isFileRenamedBeforeDownload { get; set; }
				public bool isFileRenamedAfterDownload { get; set; }
				public bool isFileNameRestored { get; set; }
				public bool isFlagFileDeleted { get; set; }
				public bool isFileSubmitted { get; set; }

				public TimeSpan DownloadElapsedTime { get => downloadStopwatch.Elapsed; }

				public void Download()
				{
					downloadStopwatch.Start();
					DownloadBeginTime = DateTime.Now;
					if (downloadTime > 0)
					{
						Thread.Sleep(downloadTime);
					}
					DuringDownloadAction?.Invoke();
					isFileDownloaded = true;
					downloadStopwatch.Stop();
				}

				public void VerifyExpectations(bool isFileRenamedBeforeDownload, bool isFileDownloaded, bool isFileSubmitted, bool isFileRenamedAfterDownload, bool isFileNameRestored, bool isFlagFileDeleted)
				{
					Assert.AreEqual(isFileRenamedBeforeDownload, this.isFileRenamedBeforeDownload, $"Verification failed for file number {fileNumber}. {nameof(isFileRenamedBeforeDownload)}=={isFileRenamedBeforeDownload} is invalid");
					Assert.AreEqual(isFileDownloaded, this.isFileDownloaded, $"Verification failed for file number {fileNumber}. {nameof(isFileDownloaded)}=={isFileDownloaded} is invalid");
					Assert.AreEqual(isFileSubmitted, this.isFileSubmitted, $"Verification failed for file number {fileNumber}. {nameof(isFileSubmitted)}=={isFileSubmitted} is invalid");
					Assert.AreEqual(isFileRenamedAfterDownload, this.isFileRenamedAfterDownload, $"Verification failed for file number {fileNumber}. {nameof(isFileRenamedAfterDownload)}=={isFileRenamedAfterDownload} is invalid");
					Assert.AreEqual(isFileNameRestored, this.isFileNameRestored, $"Verification failed for file number {fileNumber}. {nameof(isFileNameRestored)}=={isFileNameRestored} is invalid");
					Assert.AreEqual(isFlagFileDeleted, this.isFlagFileDeleted, $"Verification failed for file number {fileNumber}. {nameof(isFlagFileDeleted)}=={isFlagFileDeleted} is invalid");
				}

			}
		}

		private class FtpTransferrerForTest : FtpTransferrer
		{
			private ParallelTestCase testCase;

			internal FtpTransferrerForTest(ParallelTestCase testCase)
			{
				this.testCase = testCase;
			}

			public override void Open()
			{
				Interlocked.Increment(ref testCase.NumberofOpenedConnections);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				Interlocked.Increment(ref testCase.NumberofClosedConnections);
			}

			public override Stream GetFile(string path)
			{
				var fileNumber = Convert.ToInt32(path.Split('-')[0].Substring(4));
				var file = testCase.files[fileNumber];
				testCase.GetFileRequests.Enqueue(fileNumber);
				file.Download();
				return file.FileStream;
			}

			public override IEnumerable<TransferrerFileInfo> ListFiles(TransferrerProperties.Receive.Location location, TransferrerProperties.Receive properties, CancellationTokenSource tokenSource)
			{
				foreach (var file in testCase.files)
				{
					file.EnumeratorYieldTime = DateTime.Now;
					yield return new TransferrerFileInfo()
					{
						Name = file.fileName,
						Timestamp = DateTime.Parse("1999-12-31 00:00"),
						Size = file.FileStream.Length
					}; ;
				}
			}

			public IEnumerable<TransferrerFileInfo> ListFilesPrev(TransferrerProperties.Receive.Location location, TransferrerProperties.Receive properties, CancellationTokenSource tokenSource)
			{
				var fileQuery = from file in testCase.files
								select new TransferrerFileInfo()
								{
									Name = file.fileName,
									Timestamp = DateTime.Parse("1999-12-31 00:00"),
									Size = file.FileStream.Length
								};
				return fileQuery.ToList();
			}

			public override void ReadLocationConfiguration(XmlDocument configDOM)
			{

			}

			public override void DeleteFile(string path)
			{
				var fileNumber = Convert.ToInt32(path.Split('-')[0].Substring(4));
				var action = path.Split('-')[1].Trim();
				if (action == "FLAG") testCase.files[fileNumber].isFlagFileDeleted = true;
			}

			public override void RenameFile(string path, string dest)
			{
				var fileNumber = Convert.ToInt32(path.Split('-')[0].Substring(4));
				var destSplit = dest.Split('-');
				var action = destSplit.Length == 1 ? "UNDO RENAME" : destSplit[1].Trim();
				if (action == "RENAME BEFORE") testCase.files[fileNumber].isFileRenamedBeforeDownload = true;
				else if (action == "RENAME AFTER") testCase.files[fileNumber].isFileRenamedAfterDownload = true;
				else if (action == "UNDO RENAME") testCase.files[fileNumber].isFileNameRestored = true;
			}
		}

		private class FtpExMessageFactoryForTest : ITransferrerMessageFactory
		{
			readonly ParallelTestCase testCase;
			readonly Func<IBaseMessage> createStubMessage;

			public FtpExMessageFactoryForTest(ParallelTestCase testCase, Func<IBaseMessage> createStubMessage)
			{
				this.testCase = testCase;
				this.createStubMessage = createStubMessage;
			}

			public IBaseMessage CreateMessage(IBaseMessageFactory baseFactory, string fileName, string location, string transportLocation, string transportType, Stream fs)
			{
				var fileNumber = System.Convert.ToInt32(fileName.Split('-')[0].Substring(4));
				var file = testCase.files[fileNumber];
				file.SubmitBeginTime = DateTime.Now;
				file.DuringSubmitAction?.Invoke();
				file.isFileSubmitted = true;
				testCase.MessageSubmits.Enqueue(file.fileNumber);
				return createStubMessage();
			}
		}

		private class FtpExReceiverEndpointForTest : FtpExReceiverEndpoint
		{
			internal ManualResetEventSlim WaitForNextScheduledExecutionStarted = new ManualResetEventSlim(false);
			internal ManualResetEventSlim DownloadFilesAndSubmitCompleted = new ManualResetEventSlim(false);

			internal FtpExReceiverEndpointForTest(FtpExMessageFactoryForTest messageFactory, FtpTransferrerFactory transferrerFactory,
				ISyncReceiveSubmitBatchFactory batchFactory, TransferrerProperties.IReceiveFactory receivePropertiesFactory) : base(transferrerFactory, batchFactory, messageFactory, receivePropertiesFactory)
			{
			}

			internal override bool TryGetPortName(out string portName)
			{
				portName = "";
				return false;
			}

			internal override void DownloadFilesAndSubmit()
			{
				base.DownloadFilesAndSubmit();
				DownloadFilesAndSubmitCompleted.Set();
			}

			internal override void WaitForNextScheduledExecution(Stopwatch executionStopWatch)
			{
				WaitForNextScheduledExecutionStarted.Set();
				base.WaitForNextScheduledExecution(executionStopWatch);
			}
		}

		private class ReceivePropertiesFactoryForTest : TransferrerProperties.IReceiveFactory
		{
			readonly ILog logger;

			public ReceivePropertiesFactoryForTest(ILog logger)
			{
				this.logger = logger;
			}

			public TransferrerProperties.Receive Create(string uri) => new ReceivePropertiesForTest(uri, logger);
		}

		private class ReceivePropertiesForTest : TransferrerProperties.Receive
		{
			readonly ILog logger;

			public ReceivePropertiesForTest(string uri, ILog logger) : base(uri)
			{
				this.logger = logger;
			}

			public override ILog Logger { get => logger; }
		}
	}
}
