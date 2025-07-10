using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFBillRow))]
	sealed class ISFBillRowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeaderRow()
		{
			ISFHeaderRow headerRow = new ISFHeaderRow(Factory.New<CusISFHeader>());
			ISFBillRow billRow = new ISFBillRow(headerRow);
			AssertEquals(headerRow, billRow.HeaderRow);
		}

		public void TestBill()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill = header.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			billRow.BillPK = bill.PK;
			AssertEquals(bill, billRow.Bill);
			billRow.BillPK = ZGuid.Empty;
			AssertNull(billRow.Bill);
			billRow.BillPK = ZGuid.Invalid;
			AssertNull(billRow.Bill);
		}

		public void TestBills()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = header.ReferenceDatas.AddNew();
			masterBill1.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill masterBill2 = header.ReferenceDatas.AddNew();
			masterBill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill houseBill1 = header.ReferenceDatas.AddNew();
			houseBill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill houseBill2 = header.ReferenceDatas.AddNew();
			houseBill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill oceanBill1 = header.ReferenceDatas.AddNew();
			oceanBill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			CusISFBill oceanBill2 = header.ReferenceDatas.AddNew();
			oceanBill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			headerRow.MasterBillPK = oceanBill1.PK;
			var bills = billRow.Bills;
			AssertEquals(1, bills.Count);
			AssertEquals(oceanBill1, bills[0]);
			foreach (ZGuid masterBillPK in new ZGuid[] { ZGuid.Empty, masterBill1.PK, masterBill2.PK, houseBill1.PK, houseBill2.PK, ZGuid.Invalid })
			{
				headerRow.MasterBillPK = masterBillPK;
				bills = billRow.Bills;
				AssertEquals(2, bills.Count);
				AssertCollectionContains(houseBill1, bills);
				AssertCollectionContains(houseBill2, bills);
			}

			headerRow.MasterBillPK = oceanBill2.PK;
			bills = billRow.Bills;
			AssertEquals(1, bills.Count);
			AssertEquals(oceanBill2, bills[0]);
		}

		public void TestHouseBill()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill = header.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			billRow.BillPK = bill.PK;
			AssertEquals(bill.PK, billRow.HouseBillPK);
			AssertNull(billRow.HouseBill);
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			AssertEquals(bill, billRow.HouseBill);
		}

		public void TestHouseBills()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = header.ReferenceDatas.AddNew();
			masterBill1.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill masterBill2 = header.ReferenceDatas.AddNew();
			masterBill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill houseBill1 = header.ReferenceDatas.AddNew();
			houseBill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill houseBill2 = header.ReferenceDatas.AddNew();
			houseBill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill oceanBill1 = header.ReferenceDatas.AddNew();
			oceanBill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			CusISFBill oceanBill2 = header.ReferenceDatas.AddNew();
			oceanBill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			foreach (ZGuid masterBillPK in new ZGuid[] { ZGuid.Empty, masterBill1.PK, masterBill2.PK, houseBill1.PK, houseBill2.PK, ZGuid.Invalid })
			{
				headerRow.MasterBillPK = masterBillPK;
				var houseBills = billRow.HouseBills;
				AssertEquals(2, houseBills.Count);
				AssertCollectionContains(houseBill1, houseBills);
				AssertCollectionContains(houseBill2, houseBills);
			}

			foreach (ZGuid masterBillPK in new ZGuid[] { oceanBill1.PK, oceanBill2.PK })
			{
				headerRow.MasterBillPK = masterBillPK;
				var houseBills = billRow.HouseBills;
				AssertEquals(0, houseBills.Count);
			}
		}

		public void TestReadOnly()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill = header.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			AssertEquals(true, billRow.OceanBillPKInfo.ReadOnly);
			AssertEquals(false, billRow.HouseBillPKInfo.ReadOnly);
			AssertEquals(false, billRow.BillPKInfo.ReadOnly);
		}

		public void TestOceanBills()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = header.ReferenceDatas.AddNew();
			masterBill1.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill masterBill2 = header.ReferenceDatas.AddNew();
			masterBill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			CusISFBill houseBill1 = header.ReferenceDatas.AddNew();
			houseBill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill houseBill2 = header.ReferenceDatas.AddNew();
			houseBill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			CusISFBill oceanBill1 = header.ReferenceDatas.AddNew();
			oceanBill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			CusISFBill oceanBill2 = header.ReferenceDatas.AddNew();
			oceanBill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			headerRow.MasterBillPK = oceanBill2.PK;
			var oceanBills = billRow.Bills;
			AssertEquals(1, oceanBills.Count);
			AssertEquals(oceanBill2, oceanBills[0]);
			foreach (ZGuid masterBillPK in new ZGuid[] { ZGuid.Empty, masterBill1.PK, masterBill2.PK, houseBill1.PK, houseBill2.PK, ZGuid.Invalid })
			{
				headerRow.MasterBillPK = masterBillPK;
				oceanBills = billRow.OceanBills;
				AssertEquals(0, oceanBills.Count);
			}

			headerRow.MasterBillPK = oceanBill1.PK;
			oceanBills = billRow.Bills;
			AssertEquals(1, oceanBills.Count);
			AssertEquals(oceanBill1, oceanBills[0]);
		}

		public void TestOceanBill()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill = header.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			billRow.BillPK = bill.PK;
			AssertEquals(bill.PK, billRow.OceanBillPK);
			AssertNull(billRow.OceanBill);
			bill.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			AssertEquals(bill, billRow.OceanBill);
		}

		public void TestBuyingParties()
		{
			CusISFHeader header1 = Factory.New<CusISFHeader>();
			ISFDocAddress buyingParty1 = header1.BuyingParty;
			ISFDocAddress buyingParty2 = header1.DocAddresses.AddNew(DocAddressType.BuyingParty);
			ISFDocAddress sellingParty1 = header1.SellingParty;
			ISFDocAddress sellingParty2 = header1.DocAddresses.AddNew(DocAddressType.SellingParty);
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			ISFDocAddress buyingParty3 = header2.BuyingParty;
			ISFDocAddress sellingParty3 = header2.SellingParty;
			CusISFBill bill = header1.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header1);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			var parties = billRow.BuyingParties;
			AssertEquals(2, parties.Count);
			AssertCollectionContains(buyingParty1, parties);
			AssertCollectionContains(buyingParty2, parties);
		}

		public void TestSellingParties()
		{
			CusISFHeader header1 = Factory.New<CusISFHeader>();
			ISFDocAddress buyingParty1 = header1.BuyingParty;
			ISFDocAddress buyingParty2 = header1.DocAddresses.AddNew(DocAddressType.BuyingParty);
			ISFDocAddress sellingParty1 = header1.SellingParty;
			ISFDocAddress sellingParty2 = header1.DocAddresses.AddNew(DocAddressType.SellingParty);
			CusISFHeader header2 = Factory.New<CusISFHeader>();
			ISFDocAddress buyingParty3 = header2.BuyingParty;
			ISFDocAddress sellingParty3 = header2.SellingParty;
			CusISFBill bill = header1.ReferenceDatas.AddNew();
			ISFHeaderRow headerRow = new ISFHeaderRow(header1);
			ISFBillRow billRow = new ISFBillRow(headerRow);
			var parties = billRow.SellingParties;
			AssertEquals(2, parties.Count);
			AssertCollectionContains(sellingParty1, parties);
			AssertCollectionContains(sellingParty2, parties);
		}

		protected override BusinessObject GetNewBusinessObject() => new ISFBillRow(new ISFHeaderRow(Factory.New<CusISFHeader>()));
	}
}
