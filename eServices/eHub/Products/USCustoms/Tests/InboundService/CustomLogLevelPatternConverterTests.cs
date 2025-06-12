using System;
using System.Collections.Generic;
using System.IO;
using log4net.Core;
using CargoWise.eServices.USCustoms.InboundService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class CustomLogLevelPatternConverterTests
	{
		private CustomLogLevelPatternConverter _converter;
		private List<string> _logEntries;
		private Mock<TextWriter> _mockTextWriter;

		[TestInitialize]
		public void Setup()
		{
			_converter = new CustomLogLevelPatternConverter();
			_logEntries = new List<string>();
			_mockTextWriter = new Mock<TextWriter>();

			_mockTextWriter.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(msg => _logEntries.Add(msg));
		}

		[TestMethod]
		public void TestCustomLogLevelPatternConverter_ConvertsLevelsCorrectly()
		{
			var infoLoggingEvent = CreateLoggingEvent("INFO");
			var debugLoggingEvent = CreateLoggingEvent("DEBUG");
			var warnLoggingEvent = CreateLoggingEvent("WARN");
			var errorLoggingEvent = CreateLoggingEvent("ERROR");
			var fatalLoggingEvent = CreateLoggingEvent("FATAL");
			var traceLoggingEvent = CreateLoggingEvent("TRACE");

			_converter.FormatLogLevel(_mockTextWriter.Object, infoLoggingEvent);
			_converter.FormatLogLevel(_mockTextWriter.Object, debugLoggingEvent);
			_converter.FormatLogLevel(_mockTextWriter.Object, warnLoggingEvent);
			_converter.FormatLogLevel(_mockTextWriter.Object, errorLoggingEvent);
			_converter.FormatLogLevel(_mockTextWriter.Object, fatalLoggingEvent);
			_converter.FormatLogLevel(_mockTextWriter.Object, traceLoggingEvent);

			Assert.AreEqual(6, _logEntries.Count);
			CollectionAssert.AreEqual(new[] { "INF", "DBG", "WRN", "ERR", "FAL", "TRC" }, _logEntries);
		}

		[TestMethod]
		public void TestCustomLogLevelPatternConverter_HandlesUnknownLogLevelCorrectly()
		{
			var unknownLoggingEvent = CreateLoggingEvent("UNKNOWN");

			_converter.FormatLogLevel(_mockTextWriter.Object, unknownLoggingEvent);

			Assert.AreEqual(1, _logEntries.Count);
			Assert.AreEqual("UNK", _logEntries[0]);
		}

		private LoggingEvent CreateLoggingEvent(string logLevel)
		{
			var level = new Level(100000, logLevel);
			return new LoggingEvent(new LoggingEventData
			{
				Level = level
			});
		}
	}
}
