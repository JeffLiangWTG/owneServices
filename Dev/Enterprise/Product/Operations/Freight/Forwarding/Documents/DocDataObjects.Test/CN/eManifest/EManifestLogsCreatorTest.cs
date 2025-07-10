using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class EManifestLogsCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		public void TestCreateMessageSentLog()
		{
			var logParent = Factory.New<DummyEnterpriseBusinessObject>();
			var eManifest = new EManifest(nameof(ForwardingShipment),
				"ZZZ",
				"eManifest");
			eManifest.Bookings = new[]
			{
				new Booking("AAA")
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB")
				{
					BookingNumber = "BBB",
					Send = false
				},
				new Booking("CCC")
				{
					BookingNumber = "CCC",
					Send = true
				}
			};
			var data = eManifest.MakeDynamic();

			var logCreator = new EManifestLogsCreator();
			logCreator.CreateMessageSentLog(logParent, data, "eManifest", "Bob");

			var msnEvents = logParent
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("MSN logs parameters for sent bookings",
				new[]
				{
					"|DEP=Bob|MST=eManifest|RFN=AAA",
					"|DEP=Bob|MST=eManifest|RFN=CCC"
				},
				msnEvents.Select(log => log.SL_Reference));
		}

		#endregion

		#region TestCreateWithdrawalSentLog

		public void TestCreateWithdrawalSentLog()
		{
			var logParent = Factory.New<DummyEnterpriseBusinessObject>();
			var eManifest = new EManifest(nameof(ForwardingShipment),
				"ZZZ",
				"eManifest");

			eManifest.Bookings = new[]
			{
				new Booking("AAA")
				{
					BookingNumber = "AAA",
					Send = true
				},
				new Booking("BBB")
				{
					BookingNumber = "BBB",
					Send = false
				},
				new Booking("CCC")
				{
					BookingNumber = "CCC",
					Send = true
				}
			};
			var data = eManifest.MakeDynamic();

			var logCreator = new EManifestLogsCreator();
			logCreator.CreateWithdrawalSentLog(logParent, data, "eManifest", "Todd", "compliance");

			var mwrEvents = logParent
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageWithdrawCancelRequestCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("MWR logs parameters for sent bookings",
				new[]
				{
					"|DEP=Todd|MST=eManifest|RES=compliance|RFN=AAA",
					"|DEP=Todd|MST=eManifest|RES=compliance|RFN=CCC"
				},
				mwrEvents.Select(log => log.SL_Reference));
		}

		#endregion
	}
}
