using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Common
{
	public class WinScpTransmitterEndpointTests
	{
		const string PropertyNamespace = "http://cargowise.com/ehub/biztalkadapters/winscp-properties";
		private Func<string> generateBtsDateTimeHolder;
		private bool generateBtsDateTimeCalled;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			generateBtsDateTimeHolder = TransmitterEndpoint.GenerateBtsDateTime;
			TransmitterEndpoint.GenerateBtsDateTime = () =>
			{
				generateBtsDateTimeCalled = true;
				return generateBtsDateTimeHolder();
			};
		}

		[SetUp]
		public void Setup()
		{
			generateBtsDateTimeCalled = false;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			TransmitterEndpoint.GenerateBtsDateTime = generateBtsDateTimeHolder;
		}

		public static IEnumerable<(IBaseMessage, string, string, string)> TransmitMessageAsync_TempFolder_TempFile_FlagFile_TestCases => new[]
			{
				(CreateMessage(config: new[] { ("TargetFileName", "filename.xml") }), "filename.xml", (string)null, (string)null),
				(CreateMessage(config: new[] { ("TargetFileName", "filename.xml"), ("TemporaryFolder", "temp") }), "filename.xml", "temp/filename.xml", null),
				(CreateMessage(config: new[] { ("TargetFileName", "filename.xml"), ("TemporaryFileName", ".filename.xml") }), "filename.xml", ".filename.xml", null),
				(CreateMessage(config: new[] { ("TargetFileName", "filename.xml"), ("FlagFile", "filename.flg") }), "filename.xml", null, "filename.flg")
			};

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCaseSource(nameof(TransmitMessageAsync_TempFolder_TempFile_FlagFile_TestCases))]
		public void TransmitMessageAsync_Cases((IBaseMessage message, string serverPath, string tempPath, string flagPath) testCase)
		{
			var client = new Mock<IWinScpClient>();
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory.Setup(x => x.CreateClient(It.IsAny<WinScpLocation>(), It.IsAny<IWinScpConfiguration>(), It.IsAny<ILog>(), It.IsAny<string>()))
				.Returns(client.Object);

			var target = CreateTransmitterEndpointTarget(clientFactory.Object);
			var result = target.ProcessMessage(testCase.message);

			Assert.That(result, Is.Null);
			if (testCase.tempPath == null)
			{
				client.Verify(x => x.PutFile(
					It.IsAny<Stream>(),
					It.Is<WinScpLocation>(l => l.GetPath() == testCase.serverPath)));
			}
			else
			{
				client.Verify(x => x.PutFile(
					It.IsAny<Stream>(),
					It.Is<WinScpLocation>(l => l.GetPath() == testCase.tempPath)));
				client.Verify(x => x.MoveFile(
					It.Is<WinScpLocation>(l => l.GetPath() == testCase.tempPath),
					It.Is<WinScpLocation>(l => l.GetPath() == testCase.serverPath)));
			}
			if (testCase.flagPath != null)
			{
				client.Verify(x => x.PutFile(
					It.IsAny<Stream>(),
					It.Is<WinScpLocation>(l => l.GetPath() == testCase.flagPath)));
			}
		}

		private static IEnumerable<TestCaseData> TransmitterEndpoint_ReplaceFileNameMacros_TestCaseSource = new[]
		{
			new TestCaseData( new[] { "%OverrideFilename%.msg", "%OverrideFilename%.tmp", "%OverrideFilename%.flg" },
				new Dictionary<string, string>
				{
					["http://cargowise.com/ehub/processing/2010/06#OverrideFilename"] = "NAME.MSG"
				},
				new[] { "NAME.MSG.msg", "NAME.MSG.tmp", "NAME.MSG.flg" }
			).SetName("ReplaceFileNameMacros_OverrideFilename"),

			new TestCaseData(
				new[] { "@%DestinationPartyQualifier%", null, null },
				new Dictionary<string, string>
				{
					["http://schemas.microsoft.com/BizTalk/2003/system-properties#DestinationPartyQualifier"] = "NAME.MSG"
				},
				new[] { "@NAME.MSG", null, null }
			).SetName("ReplaceFileNameMacros_DestinationPartyQualifier")
		};

		[TestCaseSource(nameof(TransmitterEndpoint_ReplaceFileNameMacros_TestCaseSource))]
		public void TransmitterEndpoint_ReplaceFileNameMacros_FromContext(string[] fileNames, Dictionary<string, string> context, string[] expectedResults)
		{
			var testMessageFactory = new MessageFactory();
			var message = testMessageFactory.CreateMessage();
			message.Context = testMessageFactory.CreateMessageContext();
			foreach (var kvp in context ?? new Dictionary<string, string>())
			{
				var uri = new Uri(kvp.Key);
				message.Context.Write(uri.Fragment.Substring(1), uri.GetLeftPart(UriPartial.Path), kvp.Value);
			}
			var results = TransmitterEndpoint.ReplaceFileNameMacros(fileNames, message);

			Assert.That(results, Is.EqualTo(expectedResults));
		}

		[Test]
		public void TransmitterEndpoint_ReplaceFileNameMacros_ShouldNotLockIfUnnecessary()
		{
			var testMessageFactory = new MessageFactory();
			var message = testMessageFactory.CreateMessage();
			message.Context = testMessageFactory.CreateMessageContext();
			var fileNamewithDateTime = new[] { "%MessageID%", "%MessageID%", "%MessageID%" };
			TransmitterEndpoint.ReplaceFileNameMacros(fileNamewithDateTime, message);

			Assert.That(generateBtsDateTimeCalled, Is.False, "Should not call GenerateBtsDateTime if not contains %datetime_bts2000%");
		}

		[Test]
		public void TransmitterEndpoint_ReplaceFileNameMacros_DateTime()
		{
			var testMessageFactory = new MessageFactory();
			var message = testMessageFactory.CreateMessage();
			message.Context = testMessageFactory.CreateMessageContext();
			var fileNamewithDateTime = new[] { "%datetime_bts2000%", "%datetime_bts2000%", "%datetime_bts2000%" };
			var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(() => TransmitterEndpoint.ReplaceFileNameMacros(fileNamewithDateTime, message))).ToArray();
			Task.WaitAll(tasks);

			var results = tasks.Select(t => t.Result).ToList();
			Assert.Multiple(() =>
			{
				Assert.That(results.Select(r => r[0]), Is.Unique, "Each result set should be unique");

				foreach (var result in results)
				{
					Assert.That(result, Is.All.EqualTo(result[0]), "All file names within a single task's result set should be the same.");
				}
			});
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransmitMessageAsync_UseContextConfiguration()
		{
			var message = CreateMessage(
				config: new[]
				{
					("TargetFileName", "filename.xml"),
					("UserName", "User"),
					("Password", "Password"),
					("Server", "Server"),
					("Port", "21"),
					("Folder", "Folder"),
					("UseContextConfiguration", "true")
				},
				context: new[]
				{
					("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", (object)"ContextUserName"),
					("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", (object)"ContextPassword"),
					("Server", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", (object)"ContextServer"),
					("Port", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", (object)"2221"),
					("Folder", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", (object)"ContextFolder"),
				});

			WinScpLocation callbackLocation = null;
			var client = new Mock<IWinScpClient>();
			var clientFactory = new Mock<IWinScpClientFactory>();
			clientFactory
				.Setup(x => x.CreateClient(
					It.IsAny<WinScpLocation>(),
					It.IsAny<IWinScpConfiguration>(),
					It.IsAny<ILog>(),
					It.IsAny<string>()))
				.Callback<WinScpLocation, IWinScpConfiguration, ILog, string>((location, a, b, c) =>
				{
					callbackLocation = location;
				})
				.Returns(client.Object);
			var target = CreateTransmitterEndpointTarget(clientFactory.Object);

			_ = target.ProcessMessage(message);

			Assert.That(callbackLocation, Is.Not.Null);
			Assert.That(callbackLocation.UserName, Is.EqualTo("ContextUserName"));
			Assert.That(callbackLocation.Password, Is.EqualTo("ContextPassword"));
			Assert.That(callbackLocation.Server, Is.EqualTo("ContextServer"));
			Assert.That(callbackLocation.Port, Is.EqualTo(2221));
			Assert.That(callbackLocation.Folder, Is.EqualTo("ContextFolder"));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public async Task TransmitMessageAsync_ConnectionLimiter()
		{
			var client = new Mock<IWinScpClient>();
			var clientFactory = new Mock<IWinScpClientFactory>();
			var target = CreateTransmitterEndpointTarget(clientFactory.Object);

			var clientFactoryDelay = new Queue<int>(new[] { 500, 2000, Timeout.Infinite });
			clientFactory.Setup(x => x.CreateClient(It.IsAny<WinScpLocation>(), It.IsAny<IWinScpConfiguration>(), It.IsAny<ILog>(), It.IsAny<string>()))
				.Callback<WinScpLocation, IWinScpConfiguration, ILog, string>((location, config, logger, activityId) =>
				{
					var waitTime = clientFactoryDelay.Dequeue();
					Thread.Sleep(waitTime);
					if (clientFactoryDelay.Peek() is Timeout.Infinite)
						target.Transmitter.Terminate();
				})
				.Returns(client.Object);

			var tasks = Enumerable.Range(1, 3).Select(i =>
			{
				var message = CreateMessage(config: new[] { ("TargetFileName", $"file{i}.xml"), ("Timeout", "1000"), ("ConnectionLimit", "1") });
				return Task.Run(() => target.ProcessMessage(message));
			}).ToList();

			try
			{
				await Task.WhenAll(tasks);
			}
			catch { }

			Assert.Multiple(() =>
			{
				Assert.That(tasks.Select(t => t.Status),
					Is.EquivalentTo(new[] { TaskStatus.RanToCompletion, TaskStatus.Faulted, TaskStatus.Faulted }));
				clientFactory.Verify(x => x.CreateClient(It.IsAny<WinScpLocation>(), It.IsAny<IWinScpConfiguration>(), It.IsAny<ILog>(), It.IsAny<string>()),
					Times.Exactly(2));
				client.Verify(x => x.Open(), Times.Once);
			});
		}

		private static WinScpTransmitterEndpoint CreateTransmitterEndpointTarget(IWinScpClientFactory clientFactory)
		{
			var loggerFactory = new Mock<ILoggerFactory>();
			loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>(), It.IsAny<XmlDocument>()))
				.Returns(new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpTransmitterEndpointTests)));
			loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<XmlDocument>()))
				.Returns(new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(WinScpTransmitterEndpointTests)));
			var transportProxy = new Mock<IBTTransportProxy>() { CallBase = true };
			object handlerConfigValue = new XElement("Config", new XElement("InterfacesLogDir", "X:\\LogDir"), new XElement("TerminateWaitLimit", "60000")).ToString();
			var handlerProperties = new Mock<IPropertyBag>();
			handlerProperties.Setup(x => x.Read("AdapterConfig", out handlerConfigValue, It.IsAny<int>()));
			var transmitHandler = new Mock<Transmitter>("WinSCP Transmitter", "1.0", "WinSCP Adapter", "WinSCP", Guid.Empty, PropertyNamespace, null, 1, loggerFactory.Object) { CallBase = true };
			transmitHandler.Object.Initialize(transportProxy.Object);
			transmitHandler.Object.Load(handlerProperties.Object, 0);
			var endpoint = new Mock<WinScpTransmitterEndpoint>(transmitHandler.Object, loggerFactory.Object, clientFactory, typeof(WinScpTransmitConfiguration)) { CallBase = true };
			var transmitterEndpointParameters = new TransmitterEndpointParameters("winscp://port-uri", "PORT_NAME");
			endpoint.Object.Open(transmitterEndpointParameters, null, PropertyNamespace);
			return endpoint.Object;
		}

		private static IBaseMessage CreateMessage(
			Stream data = null,
			IEnumerable<(string Key, string Value)> config = null,
			IEnumerable<(string, string, object)> context = null)
		{
			var testMessageFactory = new MessageFactory();
			var transmitMessage = testMessageFactory.CreateMessage();
			transmitMessage.Context = testMessageFactory.CreateMessageContext();
			transmitMessage.AddPart("Body", testMessageFactory.CreateMessagePart(), true);
			transmitMessage.BodyPart.Data = data ?? new MemoryStream();
			var configXml = new XElement("Config", config?.Select(c => new XElement(c.Key, c.Value)));
			transmitMessage.Context.Write("AdapterConfig", PropertyNamespace, configXml.ToString());
			transmitMessage.Context.Write("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "PORT_NAME");
			context?.ToList().ForEach(c => transmitMessage.Context.Write(c.Item1, c.Item2, c.Item3));
			return transmitMessage;
		}
	}
}
