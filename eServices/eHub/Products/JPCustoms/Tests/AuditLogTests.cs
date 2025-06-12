using System;
using System.IO;
using System.Linq;
using CargoWise.eHub.Products.JPCustoms.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class AuditLoggerTests : TestBase
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AuditLogger_Constructor()
		{
			Assert.IsFalse(AuditFolder.Exists, "Prerequisite: Log folder already exists.");

			// ReSharper disable once ObjectCreationAsStatement
			new AuditLogger(AuditLoggerConfiguration);

			AuditFolder.Refresh();
			Assert.IsTrue(AuditFolder.Exists, "Log folder should be created.");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(DirectoryNotFoundException), "")]
		public void AuditLogger_Constructor_FailedToCreateFolderException()
		{
			var logFolder = new DirectoryInfo(@"M:\Dummy\");
			Assert.IsFalse(logFolder.Exists, "Prerequisite: Log folder not exists.");
			var configuration = new TestAuditLoggerConfiguration(DateTimeProvider, logFolder, FileNamePattern);

			// ReSharper disable once ObjectCreationAsStatement
			new AuditLogger(configuration);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AuditLogger_MessageSubmitted_Simple()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			logger.AddLogWithFallback("Reporter1", "Code1", "213.209.119.114;193.168.0.1");

			string text = ReadFileAsString(Path.Combine(AuditFolder.FullName, "JPCustoms_AuditLog_20130926.txt"));
			Assert.AreEqual("Reporter1	Code1	20130926224511	193.168.0.1\r\n", text);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AuditLogger_MessageSubmitted_AccessDenied_WriteToAnotherFile()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);

			using (new StreamWriter(Path.Combine(AuditFolder.FullName, "JPCustoms_AuditLog_20130926.txt")))
			{
				logger.AddLogWithFallback("Reporter1", "Code1", "213.209.119.114;193.168.0.1");
			}

			var files = AuditFolder.GetFiles("JPCustoms_AuditLog_20130926_*.txt");
			Assert.AreEqual(1, files.Count());

			var text = ReadFileAsString(files[0].FullName);
			Assert.AreEqual("Reporter1	Code1	20130926224511	193.168.0.1\r\n", text);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ArgumentException), "Invalid JP Customs message format: message has only 1 line")]
		public void AuditLogger_MessageSubmitted_Message_HasOneLineException()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			logger.MessageSubmitted("One line only", "213.209.119.114;193.168.0.1");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ArgumentException), "Invalid JP Customs message format: message has only 1 line")]
		public void AuditLogger_MessageSubmitted_Message_FirstLineHasLessThan45SymbolsException()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			logger.MessageSubmitted("One line 12345678901234567890\r\nSecons line", "213.209.119.114;193.168.0.1");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AuditLogger_MessageSubmitted()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			var message = GetEmbeddedResourceAsString("TestFiles.JPCustomsSendMessage.txt");
			logger.MessageSubmitted(message, "213.209.119.114;193.168.0.1");

			string text = ReadFileAsString(Path.Combine(AuditFolder.FullName, "JPCustoms_AuditLog_20130926.txt"));
			Assert.AreEqual("1AABC123	AHR	20130926224511	193.168.0.1\r\n", text);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ArgumentException), "Invalid JP Customs message format: ReporterId is empty")]
		public void AuditLogger_MessageSubmitted_ReporterIDEmpty()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			logger.MessageSubmitted("SS AHR                                       1A234B                                                           \r\n Bla \r\n", "213.209.119.114;193.168.0.1");

			string text = ReadFileAsString(Path.Combine(AuditFolder.FullName, "JPCustoms_AuditLog_20130926.txt"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ArgumentException), "Invalid JP Customs message format: PurposeCode is empty")]
		public void AuditLogger_MessageSubmitted_PurposeCodeEmpty()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			logger.MessageSubmitted("SS                           1AABC123                                                                         \r\n Bla \r\n", "213.209.119.114;193.168.0.1");

			string text = ReadFileAsString(Path.Combine(AuditFolder.FullName, "JPCustoms_AuditLog_20130926.txt"));
			Assert.AreEqual("2013-09-26T22:45:11	1AABC	AHR	193.168.0.1\r\n", text);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FormatException), "Invalid format of clientAndGatewayIPAddress('213.209.119.114'). Correct format should be: clientIPAddress;GatewayIPAddress. An example is 213.209.119.114;193.168.0.1")]
		public void AuditLogger_MessageSubmitted_InvalidFormatOfClientAndGatewayIPAddress()
		{
			var logger = new AuditLogger(AuditLoggerConfiguration);
			var message = GetEmbeddedResourceAsString("TestFiles.JPCustomsSendMessage.txt");
			logger.MessageSubmitted(message, "213.209.119.114");

			string text = ReadFileAsString(Path.Combine(AuditFolder.FullName, "JPCustoms_AuditLog_20130926.txt"));
		}
	}
}
