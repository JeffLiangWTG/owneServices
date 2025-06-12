using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.Test.BizTalk.PipelineObjects;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer
{
	public class TransmitterTests
	{
		private static IEnumerable<TestCaseData> TransmitMessageAsync_InstancePerPort_TestCases = new TestCaseData[]
		{
			new (new[] { ("PORT_1", "winscp://uri-1") }),
			new (new[] { ("PORT_1", "winscp://uri-1"), ("PORT_1", "winscp://uri-1") }),
			new (new[] { ("PORT_1", "winscp://uri-1"), ("PORT_1", "winscp://uri-1"), ("PORT_1", "winscp://uri-1") }),
			new (new[] { ("PORT_1", "winscp://uri-1"), ("PORT_2", "winscp://uri-2") }),
			new (new[] { ("PORT_1", "winscp://uri-1"), ("PORT_2", "winscp://uri-2"), ("PORT_2", "winscp://uri-2") }),
			new (new[] { ("PORT_1", "winscp://uri-1"), ("PORT_2", "winscp://uri-2"), ("PORT_3", "winscp://uri-3") }),
			new (new[] { ("PORT_1", "winscp://uri-1a"), ("PORT_1", "winscp://uri-1b"), ("PORT_1", "winscp://uri-1c") }),
			new (new[] { ("PORT_1A", "winscp://uri-1"), ("PORT_1B", "winscp://uri-1"), ("PORT_1C", "winscp://uri-1") })
		};

		[TestCaseSource(nameof(TransmitMessageAsync_InstancePerPort_TestCases))]
		[Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Transmitter_InstancePerPort((string PortName, string Uri)[] messages)
		{
			var transmitter = new Mock<Transmitter>("Test Transmitter",
				"1.0",
				"Test adapter",
				"TEST",
				Guid.NewGuid(),
				"http://cargowise.com/ehub/biztalkadapters/winscp-properties",
				typeof(TestTransmitterEndpoint),
				1)
			{ CallBase = true };
			transmitter.Object.Initialize(new Mock<IBTTransportProxy>().Object);
			var messageFactory = new MessageFactory();

			var results = messages.Select(m =>
			{
				var message = messageFactory.CreateMessage();
				message.Context = messageFactory.CreateMessageContext();
				message.Context.Write("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties", m.PortName);
				message.Context.Write("OutboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties", m.Uri);

				return transmitter.Object.GetEndpoint(message);
			}).ToList();

			Assert.Multiple(() =>
			{
				Assert.That(results, Is.All.InstanceOf<TestTransmitterEndpoint>());
				Assert.That(results.Distinct().Count, Is.EqualTo(messages.Distinct().Count()));
			});
			TestContext.WriteLine($"Instances created: {results.Distinct().Count()}");
		}

		public class TestTransmitterEndpoint : TransmitterEndpoint
		{
			public TestTransmitterEndpoint(AsyncTransmitter transmitter) : base(transmitter, TestLoggerFactory.Instance)
			{
			}
		}

		public class TestLoggerFactory : ILoggerFactory
		{
			public static ILoggerFactory Instance => LoggerFactory.Instance;
			private ILog ConsoleLogger() => new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(TestLoggerFactory));
			public ILog CreateLogger(string name, XmlDocument configXml) => ConsoleLogger();

			public ILog CreateLogger(string name, string type, XmlDocument configXml) => ConsoleLogger();

			public ILog CreateLogger(string name, string type, ILoggerConfiguration config) => ConsoleLogger();
		}
	}
}
