using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests.Handlers
{
	[TestFixture]
	public class JSONBodyExtractionHandlerTests
	{
		private static readonly object[] _testCases =
		{
			new object[] { "FileUploadFailed.json", new List<string> { "message.messageBody", "message.response" }, "{\r\n  \"message\": \"Uploaded file not accepted.\",\r\n  \"code\": \"BAD_REQUEST\"\r\n}" },  
			new object[] { "FileUploadFailed.json", new List<string> { "message.messageBody" }, string.Empty },
			new object[] { "InputBodyArrival.json", new List<string> { "message.body", "message.response" }, "<CC008A>blahblah</CC008A>" },
			new object[] { "InputMessageUriArrival.json", new List<string> { "message.messageBody", "message.response", "message.body" }, string.Empty },
		};

		[Test]
		[TestCaseSource(nameof(_testCases))]
		public void TestJSONBodyExtractionHandler(string inputMessageFile, List<string> properties, string expectedOutput)
		{
			var logger = new MockLogger();
			var inputMessage = GetMessage(inputMessageFile);
			var jsonExtractor = new JSONBodyExtractionHandler();

			string actualOutput;
			if (properties.Count > 1)
			{
				actualOutput = jsonExtractor.Extract(logger, inputMessage, properties);
			}
			else
			{
				actualOutput = jsonExtractor.Extract(logger, inputMessage, properties[0]);
			}

			Assert.AreEqual(expectedOutput, actualOutput);
		}

		private string GetMessage(string source)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests.TestFiles.{source}").ReadToEnd();
		}
	}
}
