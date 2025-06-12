using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Products.JPCustoms.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class SendClientTests : TestBase
	{
		//This is integration test. Smtp server should be installed
		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsClient_Send_Integration()
		{
			using (var message = GetEmbeddedResource("TestFiles.JPCustomsSendMessage.txt"))
			{
				var client = new JPCustomsSendClient(SmtpMailClientConfiguration);
				client.Send(message);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsClient_Send()
		{
			const string readerText = @"220 SMTP server is ready.  Please state your business.
250 Hello to you also
250 Sender of sender@test.com has been accepted
250 Recipient of recipient@test.com has been accepted
354 Enter text for message and end with single . on a line by itself
250 Message queued for delivery
221 Goodbye
";
			var expectedText = GetEmbeddedResourceAsString("TestFiles.SmtpEmailText.txt");

			using (var message = GetEmbeddedResource("TestFiles.JPCustomsSendMessage.txt"))
			{
				using (var readerStream = new MemoryStream(Encoding.UTF8.GetBytes(readerText)))
				{
					using (var writerStream = new MemoryStream())
					{
						var client = new TestJPCustomsSendClient(SmtpMailClientConfiguration, new StreamReader(readerStream), new StreamWriter(writerStream) { AutoFlush = true });
						client.Send(message);

						writerStream.Position = 0;
						var actualText = new StreamReader(writerStream).ReadToEnd();
						Assert.AreEqual(expectedText, actualText);
					}
				}
			}
		}

		public class TestJPCustomsSendClient : JPCustomsSendClient
		{
			readonly ISmtpMailClientConfiguration configuration;
			readonly StreamReader reader;
			readonly StreamWriter writer;

			public TestJPCustomsSendClient(ISmtpMailClientConfiguration configuration, StreamReader reader, StreamWriter writer)
				: base(configuration)
			{
				if (configuration == null) throw new ArgumentNullException("configuration");
				if (reader == null) throw new ArgumentNullException("reader");
				if (writer == null) throw new ArgumentNullException("writer");
				this.configuration = configuration;
				this.reader = reader;
				this.writer = writer;
			}

			protected override SmtpMailClient GetSmtpMailClient()
			{
				return new TestSmtpMailClient(configuration, reader, writer);
			}
		}

		public class TestSmtpMailClient : SmtpMailClient
		{
			readonly StreamReader reader;
			readonly StreamWriter writer;

			public TestSmtpMailClient(ISmtpMailClientConfiguration configuration, StreamReader reader, StreamWriter writer)
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
