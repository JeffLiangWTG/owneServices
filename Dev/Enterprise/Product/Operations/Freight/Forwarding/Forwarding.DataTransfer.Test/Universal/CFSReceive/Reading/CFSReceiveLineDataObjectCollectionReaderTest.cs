using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(CFSReceiveLineDataObjectCollectionReader))]
	public class CFSReceiveLineDataObjectCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var packingLines = new DataObjectList<PackingLine>();
			var receiptDate1 = new ZDateTime(2023, 10, 10);
			var receiptDate2 = new ZDateTime(2023, 10, 20);
			var receiptDate3 = new ZDateTime(2023, 10, 30);
			var expectDate = new ZDateTime(2023, 10, 9, 13, 0, 0);

			var packingLine1 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 1, 1, 1, 2, receiptDate1);
			var packingLine2 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", 2, 2, 2, 2, receiptDate1);
			packingLines.AddRange(new[] { packingLine1, packingLine2 });

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = "SB00000001";
			var bookingLine1 = booking.SupplierBookingLines.AddNew();
			var bookingLine2 = booking.SupplierBookingLines.AddNew();
			CFSReceiveTestHelper.SetBookingCfsAddress(Factory, booking);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5 ,0, 0, 0, 0, receiptDate2, receiptDate3);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5);

			var logger = new TestErrorLogger();
			var reader = new CFSReceiveLineDataObjectCollectionReader(packingLines, booking, logger, Factory);
			reader.ReadIntoCollection();
			CFSReceiveTestHelper.AssertPackingLine(bookingLine1, 1, 1, 1, 2, expectDate, receiptDate3);
			CFSReceiveTestHelper.AssertPackingLine(bookingLine2, 2, 2, 2, 2, expectDate, expectDate);
		}
	}
}
