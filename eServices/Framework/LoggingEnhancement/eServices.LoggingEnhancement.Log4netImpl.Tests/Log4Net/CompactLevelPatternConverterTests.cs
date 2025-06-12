using System.IO;
using eServices.LoggingEnhancement.Log4netImpl.Log4Net;
using log4net.Core;
using log4net.Util;
using NUnit.Framework;

namespace eServices.LoggingEnhancement.Log4netImpl.Tests
{
	public class CompactLevelPatternConverterTests
	{
		private PatternConverter _converter;

		[SetUp]
		public void Setup()
		{
			_converter = new CompactLevelPatternConverter();
		}

		[TestCase("TRACE", "TRC")]
		[TestCase("DEBUG", "DBG")]
		[TestCase("INFO", "INF")]
		[TestCase("WARN", "WRN")]
		[TestCase("ERROR", "ERR")]
		[TestCase("FATAL", "FAL")]
		public void TestFormat(string levelName, string expectedValue)
		{
			var loggingEvent = new LoggingEvent(new LoggingEventData() { Level = new Level(int.MaxValue, levelName )});
			var writer = new StringWriter();
			_converter.Format(writer, loggingEvent);
			Assert.AreEqual(expectedValue, writer.ToString());
		}
	}
}