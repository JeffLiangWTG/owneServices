using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UCKHostedServiceQueueTest : TestCaseWithFactory
	{
		[TestDate(2024, 7, 31, 10, 15, 32)]
		public void TestQueueResult()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = "_T2";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(-45);
			Factory.Save();
			IHostedServiceQueue uckHostedServiceQueue = new UCKHostedServiceQueue("_T1");
			AssertEquals("No EDIMessage", QueueResult.Zero, uckHostedServiceQueue.QueueResult);
			message.EM_ApplicationCode = "_T1";
			Factory.Save();
			AssertEquals("uckHostedServiceQueue.QueueResult.QueueSize", 1, uckHostedServiceQueue.QueueResult.QueueSize);
			AssertCloseEnough("uckHostedServiceQueue.QueueResult.MaximumItemAge", 45, (int)uckHostedServiceQueue.QueueResult.MaximumItemAge.TotalSeconds, allowedVariation: 1);
		}
	}
}
