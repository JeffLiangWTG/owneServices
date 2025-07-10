using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.Assertion;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public static class CFSReceiveTestHelper
	{
		public static PackingLine CreatePackingLine(string packingLineID, decimal innerQty, long packQty, decimal weight, decimal volume, ZDateTime? receiptDate = null)
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.InnerPackType = new PackageType() { Code = "CNT" };
			packingLine.InnerQty = innerQty;
			packingLine.PackQty = packQty;
			packingLine.Weight = weight;
			packingLine.Volume = volume;
			packingLine.PackingLineID = packingLineID;
			packingLine.LastCFSReceiptDate = receiptDate ?? new ZDateTime(2023, 10, 10);
			return packingLine;
		}

		public static void AssertPackingLine(JobSupplierBookingLine line, decimal innerQty, long packQty, decimal weight, decimal volume, ZDateTime? firstReceiptDate = null, ZDateTime? lastReceiptDate = null)
		{
			AssertEquals(innerQty, line.JSL_ReceivedQuantity);
			AssertEquals(packQty, line.JSL_ReceivedPackages);
			AssertEquals(weight, line.JSL_ReceivedWeight);
			AssertEquals(volume, line.JSL_ReceivedVolume);
			AssertEquals(firstReceiptDate ?? new ZDateTime(2023, 10, 9, 13, 0, 0), line.JSL_FirstReceiptDateUtc);
			AssertEquals(lastReceiptDate ?? new ZDateTime(2023, 10, 9, 13, 0, 0), line.JSL_LastReceiptDateUtc);
		}

		public static void SetBookingLineValue(
			JobSupplierBookingLine line,
			string lineID,
			decimal bookedQuantity,
			decimal receivedQuantity = 0,
			int receivedPackages = 0,
			decimal weight = 0,
			decimal volume = 0,
			ZDateTime? firstReceiptDate = null,
			ZDateTime? lastReceiptDate = null)
		{
			line.JSL_BookingLineId = lineID;
			line.JSL_BookedQuantity = bookedQuantity;
			line.JSL_ReceivedQuantity = receivedQuantity;
			line.JSL_ReceivedPackages = receivedPackages;
			line.JSL_ReceivedWeight = weight;
			line.JSL_ReceivedVolume = volume;
			line.JSL_FirstReceiptDateUtc = firstReceiptDate ?? ZDateTime.Empty;
			line.JSL_LastReceiptDateUtc = lastReceiptDate ?? ZDateTime.Empty;
		}

		public static void SetBookingCfsAddress(UniversalObjectFactory factory, JobSupplierBooking booking)
		{
			var cfsOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			cfsOrgHeader.OH_Code = "AAAAA";
			cfsOrgHeader.Addresses.AddNew().FillWithValidTestData();
			cfsOrgHeader.Addresses[0].OA_RL_NKRelatedPortCode = "AUSYD";
			booking.JSB_OA_CFSAddress = factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, cfsOrgHeader.PK)).PK;
		}
	}
}
