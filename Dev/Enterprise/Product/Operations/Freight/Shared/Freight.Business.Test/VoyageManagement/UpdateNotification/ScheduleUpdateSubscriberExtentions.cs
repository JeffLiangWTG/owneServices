using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ScheduleUpdateSubscriberExtentions : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestATDChanged()
		{
			var origin = Factory.New<VoyageOrigin>();
			var mock = new MockRepository(MockBehavior.Strict);
			var services = mock.Create<IScheduleUpdateServices>(MockBehavior.Strict);
			var subscriber1 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber3 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			IScheduleUpdateSubscriber[] list = { subscriber1.Object, subscriber2.Object, subscriber3.Object };

			var date = new ZDateTime(1981, 2, 5);

			subscriber1.Setup(m => m.ATDChanged(services.Object, origin, date));
			subscriber2.Setup(m => m.ATDChanged(services.Object, origin, date));
			subscriber3.Setup(m => m.ATDChanged(services.Object, origin, date));
			list.ATDChanged(services.Object, origin, date);
		}

		public void TestATDChanged_ExceptionHandling()
		{
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			var mock = new MockRepository(MockBehavior.Strict);
			var services = mock.Create<IScheduleUpdateServices>(MockBehavior.Strict);
			var subscriber1 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber3 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			IScheduleUpdateSubscriber[] list = { subscriber1.Object, subscriber2.Object, subscriber3.Object };

			var date = new ZDateTime(1981, 2, 5);

			subscriber1.Setup(m => m.ATDChanged(services.Object, origin, date));
			subscriber2.Setup(m => m.ATDChanged(services.Object, origin, date)).Throws(new InvalidOperationException("KA-BOOM!!!"));
			subscriber3.Setup(m => m.ATDChanged(services.Object, origin, date));

			list.ATDChanged(services.Object, origin, date);

			AssertEquals("Should have reported the exception", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestETDChanged()
		{
			var origin = Factory.New<VoyageOrigin>();
			var mock = new MockRepository(MockBehavior.Strict);
			var services = mock.Create<IScheduleUpdateServices>(MockBehavior.Strict);
			var subscriber1 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber3 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			IScheduleUpdateSubscriber[] list = { subscriber1.Object, subscriber2.Object, subscriber3.Object };

			var date = new ZDateTime(1981, 2, 5);

			subscriber1.Setup(m => m.ETDChanged(services.Object, origin, date));
			subscriber2.Setup(m => m.ETDChanged(services.Object, origin, date));
			subscriber3.Setup(m => m.ETDChanged(services.Object, origin, date));
			list.ETDChanged(services.Object, origin, date);
		}

		public void TestETDChanged_ExceptionHandling()
		{
			var origin = Factory.New<VoyageOrigin>();
			var mock = new MockRepository(MockBehavior.Strict);
			var services = mock.Create<IScheduleUpdateServices>();
			var subscriber1 = mock.Create<IScheduleUpdateSubscriber>();
			var subscriber2 = mock.Create<IScheduleUpdateSubscriber>();
			var subscriber3 = mock.Create<IScheduleUpdateSubscriber>();

			IScheduleUpdateSubscriber[] list = { subscriber1.Object, subscriber2.Object, subscriber3.Object };

			var date = new ZDateTime(1981, 2, 5);

			subscriber1.Setup(m => m.ETDChanged(services.Object, origin, date));
			subscriber2.Setup(m => m.ETDChanged(services.Object, origin, date)).Throws(new InvalidOperationException("KA-BOOM!!!"));
			subscriber3.Setup(m => m.ETDChanged(services.Object, origin, date));

			list.ETDChanged(services.Object, origin, date);

			AssertEquals("Should have reported the exception", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestETAChanged()
		{
			var destination = Factory.New<VoyageDestination>();
			var mock = new MockRepository(MockBehavior.Strict);
			var services = mock.Create<IScheduleUpdateServices>(MockBehavior.Strict);
			var subscriber1 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber3 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			IScheduleUpdateSubscriber[] list = { subscriber1.Object, subscriber2.Object, subscriber3.Object };

			var date = new ZDateTime(1981, 2, 5);

			subscriber1.Setup(m => m.ETAChanged(services.Object, destination, date));
			subscriber2.Setup(m => m.ETAChanged(services.Object, destination, date));
			subscriber3.Setup(m => m.ETAChanged(services.Object, destination, date));
			list.ETAChanged(services.Object, destination, date);
		}

		public void TestETAChanged_ExceptionHandling()
		{
			var destination = Factory.New<VoyageDestination>();
			var mock = new MockRepository(MockBehavior.Strict);
			var services = mock.Create<IScheduleUpdateServices>(MockBehavior.Strict);
			var subscriber1 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber2 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);
			var subscriber3 = mock.Create<IScheduleUpdateSubscriber>(MockBehavior.Strict);

			IScheduleUpdateSubscriber[] list = { subscriber1.Object, subscriber2.Object, subscriber3.Object };

			var date = new ZDateTime(1981, 2, 5);

			subscriber1.Setup(m => m.ETAChanged(services.Object, destination, date));
			subscriber2.Setup(m => m.ETAChanged(services.Object, destination, date)).Throws(new InvalidOperationException("KA-BOOM!!!"));
			subscriber3.Setup(m => m.ETAChanged(services.Object, destination, date));
			list.ETAChanged(services.Object, destination, date);

			AssertEquals("Should have reported the exception", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}
	}
}
