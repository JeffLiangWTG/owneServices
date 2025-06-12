using System;
using System.IO;
using eServices.LoggingEnhancement.Log4netImpl.Log4Net;
using log4net.Core;
using log4net.Util;
using NUnit.Framework;

namespace eServices.LoggingEnhancement.Tests.Log4Net
{
	public class IndentationExceptionPatternConverterTests
	{
		private PatternConverter _converter;
		private StringWriter _writer;

		[SetUp]
		public void Setup()
		{
			_converter = new IndentationExceptionPatternConverter();
			_writer = new StringWriter();
		}

		[TestCase("message")]
		[TestCase("source")]
		[TestCase("stacktrace")]
		[TestCase("targetsite")]
		[TestCase("helplink")]
		[TestCase("")]
		public void TestFormat(string option)
		{
			Exception exception;
			try
			{
				throw new Exception(@"
This is a test exception
Second line
   Third line");
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			_converter = new IndentationExceptionPatternConverter { Option = option };

			var loggingEvent = new LoggingEvent(
				typeof(IndentationExceptionPatternConverterTests), 
				null, 
				nameof(IndentationExceptionPatternConverterTests), 
				Level.Debug, 
				"Logging message", 
				exception);

			_converter.Format(_writer, loggingEvent);
			var result = _writer.ToString();
			var lines = result.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				Assert.IsTrue(line.StartsWith("   "), "Exception line doesn't start with three spaces: {0}", line);
			}
		}
	}
}
