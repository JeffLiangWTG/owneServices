using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLVPreScreeningProcessorTest : TestCaseWithFactory
	{
		public void TestGetFromObjectFactory_GivenShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var processor = ObjectFactory.Get<IProcessor>("HVLVPreScreeningProcessor", shipment);
			AssertType<HVLVPreScreeningProcessor>(processor);
		}

		public void TestGetFromObjectFactory_GivenBookingHeader()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var processor = ObjectFactory.Get<IProcessor>("HVLVPreScreeningProcessor", bookingHeader);
			AssertType<HVLVPreScreeningProcessor>(processor);
		}

		public void TestProcess_GivenShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GetOrCreateHVLVConsignmentHeader();
			var logs = new NotificationCollection();
			var processor = new HVLVPreScreeningProcessor(shipment);

			AssertNoExceptionThrown("Expected Pre-Screening to have run", () =>
			{
				processor.Process(logs);
			});

			AssertEquals("There should be one error logs", 1, logs.Count);
			AssertEquals("Request failed: Consignment parent " + shipment.PK + " with table code JS has no consignment", logs.GetFirstMessage());
		}

		public void TestProcess_GivenBookingHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var logs = new NotificationCollection();
			var processor = new HVLVPreScreeningProcessor(bookingHeader);

			AssertNoExceptionThrown("Expected Pre-Screening to have run", () =>
			{
				processor.Process(logs);
			});

			AssertEquals("There should be one error logs", 1, logs.Count);
			AssertEquals("Request failed: Consignment parent " + bookingHeader.PK + " with table code HVH has no consignment", logs.GetFirstMessage());
		}

		protected override void SetUp()
		{
			HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true });
		}
	}
}
