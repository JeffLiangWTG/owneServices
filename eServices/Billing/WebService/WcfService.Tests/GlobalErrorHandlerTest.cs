using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Log4Net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class GlobalErrorHandlerTest
	{
		[Test]
		public void TestProvideFault()
		{
			var handler = new GlobalErrorHandler();
			var exception = new Exception("Some Error.");
			
			Message message1 = null;
			handler.ProvideFault(exception, MessageVersion.Default, ref message1);
			Assert.That(message1.IsFault);
			var fault = MessageFault.CreateFault(message1, int.MaxValue);
			var faultException = FaultException.CreateFault(fault);
			Assert.That(faultException.Message, Is.EqualTo(exception.Message));

			Message message2 = null;
			handler.ProvideFault(faultException, MessageVersion.Default, ref message2);
			Assert.That(message2, Is.Null);
		}

		[Test]
		public void TestHandleError()
		{
			var handler = new GlobalErrorHandler();
			var exception = new Exception("Some Error.");

			Assert.That(handler.HandleError(exception));
			var loggedErrors = memoryAppender.GetEvents().Where(e => e.Level == Level.Error);
			Assert.That(loggedErrors.Count(e => (string) e.MessageObject == "Unknown error" && e.ExceptionObject == exception), Is.EqualTo(1));

			memoryAppender.Clear();
			var faultException = new FaultException("Some Other Error.");
			Assert.That(handler.HandleError(faultException));
			loggedErrors = memoryAppender.GetEvents().Where(e => e.Level == Level.Error);
			Assert.That(!loggedErrors.Any());
		}

		[SetUp]
		public void SetUp()
		{
			memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
			LogManager.Adapter = new Log4NetLoggerFactoryAdapter(new NameValueCollection { { "configType", "EXTERNAL" } });
		}

		MemoryAppender memoryAppender;
	}
}