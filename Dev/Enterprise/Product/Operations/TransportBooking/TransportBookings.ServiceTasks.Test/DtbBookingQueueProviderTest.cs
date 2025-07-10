using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	public class DtbBookingQueueProviderTest : TestCaseWithFactory
	{
		public void TestProviderHasCorrectHostedServiceQueueProviderAttribute()
		{
			var hostedServiceQueueProviderAttribute = Assembly.GetAssembly(typeof(DtbBookingQueueProvider))
				.GetCustomAttributes(typeof(HostedServiceQueueProviderAttribute), false)
				.OfType<HostedServiceQueueProviderAttribute>()
				.FirstOrDefault((HostedServiceQueueProviderAttribute a) => a.QueueName == "Transport Job Generator");

			AssertNotNull("DtbBookingQueueProvider assembly (Enterprise.TransportBookings.ServiceTasks) should have HostedServiceQueueProvider attribute", hostedServiceQueueProviderAttribute);

			CombineAssertions(() =>
			{
				AssertEquals("KMQ", hostedServiceQueueProviderAttribute.ServiceTaskCode);
				AssertEquals(typeof(DtbBookingQueueProvider), hostedServiceQueueProviderAttribute.Type);
			});
		}

		[TestDate(2023, 10, 1)]
		public void TestQueueSizeEqualsNumberOfRecordsInQueueTable_AndQueueAgeEqualsAgeOfOldestQueueRecord()
		{
			var queueRecord1 = CreateTestDtbBookingQueueRecord(ZGuid.NewZGuid(), JobShipmentSchema.Constants.Prefix, "PIC");
			var queueRecord2 = CreateTestDtbBookingQueueRecord(ZGuid.NewZGuid(), JobShipmentSchema.Constants.Prefix, "PIC");
			var queueRecord3 = CreateTestDtbBookingQueueRecord(ZGuid.NewZGuid(), JobShipmentSchema.Constants.Prefix, "PIC");
			var queueRecord4 = CreateTestDtbBookingQueueRecord(ZGuid.NewZGuid(), JobShipmentSchema.Constants.Prefix, "PIC");

			Factory.Save();

			var queueProvider = new DtbBookingQueueProvider();

			AssertEquals("Queue Provider QueueSize should match the number of records in the table", 4, queueProvider.QueueResult.QueueSize);
			var expectedAge = DateTime.UtcNow - TestDateAttribute.Date;
			AssertCloseEnough("MaximumItemAge should indicate the time since the oldest last edit date of queue records", (int)expectedAge.TotalSeconds, (int)queueProvider.QueueResult.MaximumItemAge.TotalSeconds, 60);
		}

		DtbBookingQueue CreateTestDtbBookingQueueRecord(ZGuid parentID, ZString parentTableCode, ZString direction, short iteration = 0)
		{
			var queueRecord = Factory.New<DtbBookingQueue>();

			queueRecord.KMQ_ParentID = parentID;
			queueRecord.KMQ_ParentTableCode = parentTableCode;
			queueRecord.KMQ_Direction = direction;
			queueRecord.Iteration = iteration;
			queueRecord.KMQ_SystemCreateTimeUtc = queueRecord.KMQ_SystemLastEditTimeUtc = DateTime.UtcNow;
			queueRecord.KMQ_SystemCreateUser = queueRecord.KMQ_SystemLastEditUser = "XXX";

			return queueRecord;
		}
	}
}
