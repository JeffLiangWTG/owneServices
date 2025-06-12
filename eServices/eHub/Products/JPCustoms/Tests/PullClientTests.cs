using System;
using System.IO;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{

	[TestClass]
	public class PullClientTests : TestBase
	{
		//This is integration test, pop3 server should be install first. Copy TestFiles/JPCustomsReplyMessage.eml file for user recipient1 after each successful test. 
		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsClient_Pull_Integration()
		{
			var expectedMessage = GetEmbeddedResourceAsString("TestFiles.Pop3ReplyMessage.txt");

			var client = new JPCustomsPullClient(Pop3MailClientConfiguration);
			client.Connect();
			Assert.AreEqual(1, client.MessageCount);
			var actualMessage = client.GetNextMessage();
			Assert.AreEqual(expectedMessage, actualMessage);
			client.Delete();
			client.Disconnect();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsClient_Pull()
		{
			var expectedText = GetEmbeddedResourceAsString("TestFiles.Pop3ReplyMessage.txt");
			var expectedText1 = GetEmbeddedResourceAsString("TestFiles.Pop3ReplyMessage1.txt");
			var pop3MailClientConfiguration = new TestPop3MailClientConfiguration(Logger) { HeaderLineNumberToSkip = 0 };

			using (var readerStream = GetEmbeddedResource("TestFiles.Pop3ServerSendMessage.txt"))
			{
				using (var writerStream = new MemoryStream())
				{
					var client = new TestJPCustomsPullClient(pop3MailClientConfiguration, new StreamReader(readerStream), new StreamWriter(writerStream) { AutoFlush = true });
					client.Connect();
					Assert.AreEqual(2, client.MessageCount);
					var actualText = client.GetNextMessage();
					Assert.AreEqual(expectedText, actualText);
					client.Delete();
					var actualText1 = client.GetNextMessage();
					Assert.AreEqual(expectedText1, actualText1);
					client.Delete();
					client.Disconnect();
				}
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsClient_Pull_SkipHeader()
		{
			var expectedText = GetEmbeddedResourceAsString("TestFiles.Pop3ReplyMessageWithHeader.txt");

			using (var readerStream = GetEmbeddedResource("TestFiles.Pop3ServerSendMessageWithHeader.txt"))
			{
				using (var writerStream = new MemoryStream())
				{
					var client = new TestJPCustomsPullClient(Pop3MailClientConfiguration, new StreamReader(readerStream), new StreamWriter(writerStream) { AutoFlush = true });
					client.Connect();
					Assert.AreEqual(1, client.MessageCount);
					var actualText = client.GetNextMessage();
					Assert.AreEqual(expectedText, actualText);
					client.Delete();
					client.Disconnect();
				}
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(MailboxLockedByAnotherClientException))]
		public void JPCustomsClient_Pull_MailBoxLockedByAnotherClient()
		{
			using (var readerStream = GetEmbeddedResource("TestFiles.Pop3ServerMailboxLockedByAnotherClient.txt"))
			{
				using (var writerStream = new MemoryStream())
				{
					var client = new TestJPCustomsPullClient(Pop3MailClientConfiguration, new StreamReader(readerStream),
						new StreamWriter(writerStream) {AutoFlush = true});
					client.Connect();
				}
			}
		}

		public class TestJPCustomsPullClient : JPCustomsPullClient
		{
			readonly IPop3MailClientConfiguration configuration;
			readonly StreamReader reader;
			readonly StreamWriter writer;

			public TestJPCustomsPullClient(IPop3MailClientConfiguration configuration, StreamReader reader, StreamWriter writer)
				: base(configuration)
			{
				this.configuration = configuration;
				this.reader = reader;
				this.writer = writer;
			}

			protected override Pop3MailClient GetNewPop3MailClient()
			{
				return new TestPop3MailClient(configuration, reader, writer);
			}
		}

		public class TestPop3MailClient : Pop3MailClient
		{
			readonly StreamReader reader;
			readonly StreamWriter writer;

			public TestPop3MailClient(IPop3MailClientConfiguration configuration, StreamReader reader, StreamWriter writer)
				: base(configuration)
			{
				if (reader == null) throw new ArgumentNullException("reader");
				if (writer == null) throw new ArgumentNullException("writer");
				this.reader = reader;
				this.writer = writer;

			}

			protected override StreamReader Reader
			{
				get { return reader; }
			}

			protected override StreamWriter Writer
			{
				get { return writer; }
			}
		}
	}
}
