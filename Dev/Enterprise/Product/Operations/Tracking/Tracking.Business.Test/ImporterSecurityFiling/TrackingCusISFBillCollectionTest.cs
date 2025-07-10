using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling.Testing
{
	[TestedType(typeof(TrackingCusISFBillCollection))]
	sealed class TrackingCusISFBillCollectionTest : ActiveBusinessObjectCollectionTestCase<TrackingCusISFBillCollection>
	{
		public void TestCollectionContainsOnlyBills()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "123";
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.ISFBondNumber;
			bill2.BB_BillNum = "321";
			AssertEquals("both bills are in reference data collection", header.ReferenceDatas.Count, 2);
			AssertEquals("there is only one element in collection", header.BillNumbersReferences.Count, 1);
			Assert("bill with type HouseBillOfLading should be in collection", header.BillNumbersReferences.Contains(bill1));

			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill1.BB_BillNum = "123";

			bill = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill1.BB_BillNum = "123";

			bill = header.ReferenceDatas.AddNew();

			AssertEquals("all bills are in reference data collection", header.ReferenceDatas.Count, 5);
			AssertEquals("there are four elements in BillNumbersReferences collection", header.BillNumbersReferences.Count, 4);
		}

		public void TestCollectionContainsCorrectAddresses()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			AssertNotNull(header.BookingParty);
			AssertNotNull(header.MainShipToParty);
			AssertNotNull(header.SellingParty);
			AssertNotNull(header.BuyingParty);
			AssertNotNull(header.StuffingLocation);
			AssertNotNull(header.Consolidator);
			AssertEquals(header.ManufacturerAddresses.Count, 0);
			AssertEquals(header.DocAddresses.Count, 6);
			AssertEquals(header.DocAddressesExcludeManufacturer.Count, 0);

			header.ManufacturerAddresses.AddNew();
			AssertEquals(header.ManufacturerAddresses.Count, 1);
			AssertEquals(header.DocAddresses.Count, 7);
			AssertEquals(header.DocAddressesExcludeManufacturer.Count, 0);

			header.DocAddressesExcludeManufacturer.AddNew();
			AssertEquals(header.ManufacturerAddresses.Count, 1);
			AssertEquals(header.DocAddresses.Count, 8);
			AssertEquals(header.DocAddressesExcludeManufacturer.Count, 1);
		}

		public void TestNewElementInBillNumbersAppearesInReferenceCollection()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			header.BillNumbersReferences.AddNew();
			AssertEquals("new element added", header.BillNumbersReferences.Count, 1);
			AssertEquals("element appeared in referenece datas", header.ReferenceDatas.Count, header.BillNumbersReferences.Count);
		}

		protected override TrackingCusISFBillCollection GetCollectionToTest()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			return new TrackingCusISFBillCollection(header);
		}
	}
}
