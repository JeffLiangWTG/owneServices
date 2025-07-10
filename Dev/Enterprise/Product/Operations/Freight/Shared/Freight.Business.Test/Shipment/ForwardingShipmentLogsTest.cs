using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonShipmentLogs))]
	sealed class ForwardingShipmentLogsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			var shipment = Factory.New<CommonShipment>();
			var logs = new CommonShipmentLogs(shipment);
			AssertEquals(shipment, logs.Parent);
		}

		public void TestForwardingShipmentLogs_DoNotLoadQuotedBookingLogs()
		{
			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);
			var logParent = (IStmALogParent)quotedBooking;
			var quotedBookingLogEntry = logParent.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(quotedBooking.ForwardingShipment.PK, factory);
			var logs = new CommonShipmentLogs((CommonShipment)quotedBooking.ForwardingShipment);
			Assert("should not contain quotedBooking logs", logs.GetAllLogs().Cast<StmALog>().All(log => log.PK != quotedBookingLogEntry.PK));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommonShipmentLogs(Factory.New<CommonShipment>());
		}
	}
}
