using System;
using log4net.Util;
using NUnit.Framework;
using System.IO;
using eServices.LoggingEnhancement.Log4netImpl.Log4Net;
using log4net.Core;

namespace eServices.LoggingEnhancement.Tests.Log4Net
{
	public class IndentationMessagePatternConverterTests
	{
		private PatternConverter _converter;
		private StringWriter _writer;

		[SetUp]
		public void Setup()
		{
			_converter = new IndentationMessagePatternConverter();
			_writer = new StringWriter();
		}

		[Test]
		public void TestFormat()
		{
			var loggingEvent = new LoggingEvent(new LoggingEventData { 
				Level = new Level(int.MaxValue, "DEBUG"), 
				Message = @"
First line
 Second line
  Third line
   Fourth line
"});
			_converter.Format(_writer, loggingEvent);

			foreach (var line in _writer.ToString().Split(new [] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
			{
				Assert.IsTrue(line.StartsWith("   "), "Message line doesn't start with three spaces: {0}", line);
			}
		}
	}
}
