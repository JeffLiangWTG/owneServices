using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.eHub.AlertService.Helpers;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Moq;
using WTG.ErrorReporting;

namespace CargoWise.eHub.AlertService.Tests
{


	[TestClass()]
	public class OrchestrationHelperTest : BaseComponentTest
	{

		/*
		 * This test should not be run automatically as it will raise an IssueManager issue.
		 * Check IssueManager to confirm a new issue got raised.
		 */
		[TestMethod]
		public void TestIssueManagerReporting()
		{
			var log = new Mock<ILog>();
			var testURI = "https://errorstest.cargowise.com/";
			var errorReportingClient = new Mock<IErrorReportingClient>();
			
			var keyToCheck = string.Empty;
			var exceptionMessageToCheck = string.Empty;
			string[] textToCheck = null;
			var keyCheckPasses = false;
			var textCheckPasses = false;
			var containsEHubStackTrace = false;
			var containsExeCreationTime = false;
			var exceptionCheckPasses = false;
			var exceptionMessagePasses = false;
			string allText = string.Empty;
			errorReportingClient.Setup(_ => _.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Callback((IOpaqueErrorReport errBuilder, CancellationToken cancellationToken) =>
				{
					var reportBuilder = errBuilder as EnterpriseErrorReportBuilder;
				
					using (var memStream = new MemoryStream())
					{
						var task = reportBuilder.WriteToAsync(memStream);
						task.ConfigureAwait(false);
						task.Wait();
						memStream.Seek(0, SeekOrigin.Begin);
						using (var reader = new StreamReader(memStream))
						{
							var t2 = reader.ReadToEndAsync();
							t2.ConfigureAwait(false);
							t2.Wait();
							allText = t2.Result;
						}

						exceptionCheckPasses = allText.Contains("<ExceptionType>CargoWise.eHub.AlertService.Helpers.IssueAlertException</ExceptionType>");
						keyCheckPasses = allText.Contains("<Key>" + keyToCheck + "</Key>");
						exceptionMessagePasses = allText.Contains("<Message>" + exceptionMessageToCheck + "</Message>");
						textCheckPasses = keyCheckPasses && textToCheck.All(t => allText.Contains(t));
						containsEHubStackTrace = allText.Contains("at CargoWise.eHub.AlertService.Helpers.OrchestrationHelper.ReportAlertToIssueManager");
						containsExeCreationTime = allText.Contains("<ExeCreationTime>");
					}
				});

			var factory = new Mock<ErrorReporterFactory>();
			factory.Setup(_ => _.BuildErrorReporter(It.IsAny<Uri>())).Returns(errorReportingClient.Object).Callback((Uri uri) =>
			{
				Assert.AreEqual(uri.AbsoluteUri, testURI);
			});

			var errorMessage1 =
				"An output message of the component \"Unknown \" in receive pipeline \"CargoWise.eHub.Core.Pipelines.Rcv_ResolveInboundEDIMessage, CargoWise.eHub.Core.Pipelines, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350\" is suspended due to the following error: \r\n Error: 1 (Miscellaneous error)47: Envelope functionality not supported\r\n\r\n\tError: 2 (Field level error)\r\n\tSegmentID: UNB\r\n\tPosition in TS: 1\r\n\tData Element ID: UNB5\r\n\tPosition in Segment: 5\r\n\tData Value: 5093235999\r\nUNH\r\n\t39: Data element too long";

			var key = errorMessage1.Substring(0, 300);
			 
			keyToCheck = $"eHub.Alert SampleSender SampleRecipient {key}";
			exceptionMessageToCheck = $"eHub.Alert SampleSender SampleRecipient {errorMessage1}";
			var propertiesInfo = string.Join("\r\n", "----------BIZTALK PROPERTIES----------", "http://SampleNamespace#SampeProperty1 = Value1", "http://SampleNamespace#SampeProperty2 = Value2");
			textToCheck = new string[] {"trackingPK0", DateTime.UtcNow.AddDays(-2.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", $"{errorMessage1}\r\n{propertiesInfo}" };
			OrchestrationHelper.ReportAlertToIssueManager(log.Object, factory.Object, testURI, "examplePK0", "trackingPK0", DateTime.UtcNow.AddDays(-2.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", $"{errorMessage1}\r\n{propertiesInfo}");
			Assert.IsTrue(exceptionCheckPasses, allText);
			Assert.IsTrue(exceptionMessagePasses, allText);
			Assert.IsTrue(keyCheckPasses, allText);
			Assert.IsTrue(textCheckPasses, allText);
			Assert.IsTrue(containsEHubStackTrace, allText);
			Assert.IsTrue(containsExeCreationTime, allText);

			var errorMessage2WithDifferentDataValue =
				"An output message of the component \"Unknown \" in receive pipeline \"CargoWise.eHub.Core.Pipelines.Rcv_ResolveInboundEDIMessage, CargoWise.eHub.Core.Pipelines, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350\" is suspended due to the following error: \r\n Error: 1 (Miscellaneous error)47: Envelope functionality not supported\r\n\r\n\tError: 2 (Field level error)\r\n\tSegmentID: UNB\r\n\tPosition in TS: 1\r\n\tData Element ID: UNB5\r\n\tPosition in Segment: 5\r\n\tData Value: 5093235999\r\nUNH\r\n\t39: Data element too long";

			key = errorMessage2WithDifferentDataValue.Substring(0, 300);

			keyToCheck =$"eHub.Alert SampleSender SampleRecipient {key}";
			exceptionMessageToCheck = $"eHub.Alert SampleSender SampleRecipient {errorMessage2WithDifferentDataValue}";

			textToCheck = new string[] {"trackingPK1", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", $"{errorMessage2WithDifferentDataValue}\r\n{propertiesInfo}" };
			OrchestrationHelper.ReportAlertToIssueManager(log.Object, factory.Object, testURI, "examplePK1", "trackingPK1", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", $"{errorMessage2WithDifferentDataValue}\r\n{propertiesInfo}");
			Assert.IsTrue(exceptionCheckPasses, allText);
			Assert.IsTrue(exceptionMessagePasses, allText);
			Assert.IsTrue(keyCheckPasses, allText);
			Assert.IsTrue(textCheckPasses, allText);
			Assert.IsTrue(containsEHubStackTrace, allText);
			Assert.IsTrue(containsExeCreationTime, allText);

			keyToCheck = exceptionMessageToCheck = "eHub.Alert SampleSender SampleRecipient ErrorMessage2";
			textToCheck = new string[] {"trackingPK2", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", "ErrorMessage2" };
			OrchestrationHelper.ReportAlertToIssueManager(log.Object, factory.Object, testURI, "examplePK2", "trackingPK2", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", $"ErrorMessage2\r\n{propertiesInfo}");
			Assert.IsTrue(exceptionCheckPasses, allText);
			Assert.IsTrue(exceptionMessagePasses, allText);
			Assert.IsTrue(keyCheckPasses, allText);
			Assert.IsTrue(textCheckPasses, allText);
			Assert.IsTrue(containsEHubStackTrace, allText);
			Assert.IsTrue(containsExeCreationTime, allText);

			keyToCheck = "eHub.Alert SampleSender SampleRecipient ErrorMessage2";
			textToCheck = new string[] {"trackingPK3", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", "ErrorMessage2" };
			OrchestrationHelper.ReportAlertToIssueManager(log.Object, factory.Object, testURI, "examplePK3", "trackingPK3", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", "ErrorMessage2");
			Assert.IsTrue(exceptionCheckPasses, allText);
			Assert.IsTrue(exceptionMessagePasses, allText);
			Assert.IsTrue(keyCheckPasses, allText);
			Assert.IsTrue(textCheckPasses, allText);
			Assert.IsTrue(containsEHubStackTrace, allText);
			Assert.IsTrue(containsExeCreationTime, allText);

			keyToCheck = exceptionMessageToCheck = "Test Alert key";
			textToCheck = new string[] { "trackingPK4", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", "ErrorMessage3" };

			OrchestrationHelper.ReportAlertToIssueManager(log.Object, factory.Object, testURI, "examplePK4", "trackingPK4", DateTime.UtcNow.AddDays(-5.0).ToString(), DateTime.UtcNow.ToString(), "SampleSender", "SampleRecipient", "SampleSource", "ErrorMessage3", keyToCheck);
			Assert.IsTrue(exceptionCheckPasses, allText);
			Assert.IsTrue(exceptionMessagePasses, allText);
			Assert.IsTrue(keyCheckPasses, allText);
			Assert.IsTrue(textCheckPasses, allText);
			Assert.IsTrue(containsEHubStackTrace, allText);
			Assert.IsTrue(containsExeCreationTime, allText);

			errorReportingClient.Verify(_ => _.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleError_InvalidCharInXml()
		{
			var mockMessage = new Mock<XLANGMessage>();
			var properties = new OrderedDictionary();
			properties.Add(new XmlQName("PortName", "http://schemas.microsoft.com/BizTalk/2003/messagetracking-properties"), "TestReceivePort");
			properties.Add(new XmlQName("ErrorType", "http://schemas.microsoft.com/BizTalk/2005/error-report"), "FailedMessage");
			OrchestrationHelper.GetContextProperties = x => properties;

			var inboxPK = "inboxPK";
			var outboxPK = "outboxPK";
			var errorPK = "errorPK";
			var senderID = "senderID";
			var recipientID = "recipientID";
			var inboxMessageTrackingID = "inboxMessageTrackingID";
			var messageTrackingID = "messageTrackingID";
			var errorType = "Something";
			var errorDescription = "Reason: '', hexadecimal value 0x04, is an invalid character.";
			var currentUTCTime = "currentUTCTime";
			var skip = "skip";
			var wcf = OrchestrationHelper.GenerateInsertErrorWcfSqlString(outboxPK, inboxPK, errorPK, senderID, recipientID, inboxMessageTrackingID, messageTrackingID, errorType, errorDescription, currentUTCTime, skip, mockMessage.Object);
			AssertXmlStream(GetEmbeddedResource("TestFiles.InsertErrorWcfSql.xml", Assembly.GetExecutingAssembly()), new MemoryStream(Encoding.UTF8.GetBytes(wcf)));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleError_InsertOutbox()
		{
			var inboxPK = "inboxPK";
			var outboxPK = "outboxPK";
			var senderID = "senderID";
			var recipientID = "recipientID";
			var messageTrackingID = "messageTrackingID";
			var overrideEmailSubject = "Something";
			var overrideFilename = "Reason: '', hexadecimal value 0x04, is an invalid character.";
			var currentUTCTime = "currentUTCTime";
			var envelopeTrackingID = "envelopeTrackingID";
			var inboxContent = "inboxContent";
			var wcf = OrchestrationHelper.GenerateInsertOutboxWcfSqlString(outboxPK, inboxPK, envelopeTrackingID, messageTrackingID, senderID, recipientID, overrideEmailSubject, overrideFilename, currentUTCTime, inboxContent);
			AssertXmlStream(GetEmbeddedResource("TestFiles.InsertOutboxWcfSql.xml", Assembly.GetExecutingAssembly()), new MemoryStream(Encoding.UTF8.GetBytes(wcf)));
		}

		[TestMethod]
		public void TestGetContextPropertiesAsString()
		{
			Hashtable hashTable = new Hashtable();
			hashTable.Add(new XmlQName("PortName", "http://schemas.microsoft.com/BizTalk/2003/messagetracking-properties"), "TestReceivePort");
			hashTable.Add(new XmlQName("ErrorType", "http://schemas.microsoft.com/BizTalk/2005/error-report"), "FailedMessage");

			OrchestrationHelper.GetContextProperties = x => hashTable;
			var results = OrchestrationHelper.GetContextPropertiesAsString(null).Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries); // process doesn't follow any order so we will split and check the list.
			Assert.IsTrue(results.Any(x => x == "http://schemas.microsoft.com/BizTalk/2003/messagetracking-properties#PortName = TestReceivePort"));
			Assert.IsTrue(results.Any(x => x == "http://schemas.microsoft.com/BizTalk/2005/error-report#ErrorType = FailedMessage"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestIsXml()
		{
			var mockMessage = new Mock<XLANGMessage>();
			var mockPart = new Mock<XLANGPart>();
			string messageText = "<message>text</message>";
			var bodyStream = new MemoryStream(Encoding.Default.GetBytes(messageText));
			mockMessage.Setup(x => x[0]).Returns(mockPart.Object);
			mockPart.Setup(x => x.RetrieveAs(typeof(Stream))).Returns(bodyStream);

			Assert.IsTrue(OrchestrationHelper.IsXml(mockMessage.Object));
			bodyStream.Position = 0;
			Assert.AreEqual(messageText, new StreamReader((Stream)mockMessage.Object[0].RetrieveAs(typeof(Stream))).ReadToEnd());

			messageText = "text";
			bodyStream = new MemoryStream(Encoding.Default.GetBytes(messageText));
			mockPart.Setup(x => x.RetrieveAs(typeof(Stream))).Returns(bodyStream);

			Assert.IsFalse(OrchestrationHelper.IsXml(mockMessage.Object));
			bodyStream.Position = 0;
			Assert.AreEqual(messageText, new StreamReader((Stream)mockMessage.Object[0].RetrieveAs(typeof(Stream))).ReadToEnd());
		}
	}
}
