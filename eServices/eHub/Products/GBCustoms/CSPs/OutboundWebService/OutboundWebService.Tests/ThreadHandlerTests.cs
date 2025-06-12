using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService.Tests
{
	public class ThreadHandlerTests
	{
		public MessageSender testMessageSender;
		public ILog testLogger;

		[SetUp]
		public void Setup()
		{
			testMessageSender = new MessageSender();
			testLogger = MockRepository.GenerateMock<ILog>();
		}

		[Test]
		public void SendMessageThreadSafeTest()
		{
			using (var messageSender1 = new DummyMessageSender())
			using (var messageSender2 = new DummyMessageSender())
			using (var messageSender3 = new DummyMessageSender())
			{
				var testRequest1 = new HttpRequestMessage();
				testRequest1.Method = HttpMethod.Post;
				testRequest1.Content = new StringContent("1");

				var testRequest2 = new HttpRequestMessage();
				testRequest2.Method = HttpMethod.Post;
				testRequest2.Content = new StringContent("2");

				var testRequest3 = new HttpRequestMessage();
				testRequest3.Method = HttpMethod.Post;
				testRequest3.Content = new StringContent("3");

				Task<ResponseAndCompletionTime> task1 = new Task<ResponseAndCompletionTime>(() => Send(messageSender1, testRequest1, "Account1"));
				Task<ResponseAndCompletionTime> task2 = new Task<ResponseAndCompletionTime>(() => Send(messageSender2, testRequest2, "Account1"));
				Task<ResponseAndCompletionTime> task3 = new Task<ResponseAndCompletionTime>(() => Send(messageSender3, testRequest3, "Account2"));

				task1.Start();
				Assert.IsTrue(messageSender1.WaitForBeginSend(), "Should have begun the send for the first message");
				task2.Start();
				Assert.IsFalse(messageSender2.WaitForBeginSend(), "Should not have begun the send for the second message, first message is still being processed");
				task3.Start();
				Assert.IsTrue(messageSender3.WaitForBeginSend(), "Should have begun the send for the third message as it is on a different account");

				messageSender1.CompleteSend();
				messageSender3.CompleteSend();

				Assert.IsTrue(messageSender2.WaitForBeginSend(), "Should have begun the send for the second message now the first message is complete");

				messageSender2.CompleteSend();
				Task.WhenAll(task1, task2, task3);
			}
		}

		private ResponseAndCompletionTime Send(IMessageSender messageSender, HttpRequestMessage httpRequestMessage, string account)
		{
			var res = ThreadHandler.SendMessageThreadSafe(messageSender, testLogger, "", httpRequestMessage, account, "https://mockendpoints-api-test.wtg.zone/SampleRest/SampleOK");
			return new ResponseAndCompletionTime()
			{
				Res = res.Result,
				CompleteTime = DateTime.Now
			};
		}
	}

	public class ResponseAndCompletionTime
	{
		public HttpResponseMessage Res { get; set; }
		public DateTime CompleteTime { get; set; }
	}

	public class DummyMessageSender : IMessageSender, IDisposable
	{
		readonly AutoResetEvent beginSendEvent = new AutoResetEvent(false);
		readonly AutoResetEvent completeSendEvent = new AutoResetEvent(false);

		public void Dispose()
		{
			completeSendEvent.Set();
			beginSendEvent.Dispose();
			completeSendEvent.Dispose();
		}

		public bool WaitForBeginSend() => beginSendEvent.WaitOne(TimeSpan.FromSeconds(5));
		public void CompleteSend() => completeSendEvent.Set();

		public Task<HttpResponseMessage> SendMessage(ILog logger, string logPrefix, HttpRequestMessage httpRequestMessage, string uri)
		{
			beginSendEvent.Set();
			completeSendEvent.WaitOne();
			return null;
		}
	}
}
