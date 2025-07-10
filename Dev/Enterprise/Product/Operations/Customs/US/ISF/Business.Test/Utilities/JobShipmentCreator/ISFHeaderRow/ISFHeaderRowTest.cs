using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFHeaderRow))]
	sealed class ISFHeaderRowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(header, headerRow.Header);
		}

		public void TestConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertNull(headerRow.Consol);
			headerRow.ConsolPK = ZGuid.Invalid;
			AssertNull(headerRow.Consol);
			headerRow.ConsolPK = consol.PK;
			AssertEquals(consol, headerRow.Consol);
		}

		public void TestDefault()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill houseBill1 = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			CusISFBill houseBill2 = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(ZGuid.Empty, headerRow.MasterBillPK);
			AssertEquals(2, headerRow.Bills.Count);
			ISFBillRow billRow1 = headerRow.Bills[0];
			ISFBillRow billRow2 = headerRow.Bills[1];
			if (billRow1.BillPK == houseBill2.PK)
			{
				billRow1 = headerRow.Bills[1];
				billRow2 = headerRow.Bills[2];
			}

			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			CusISFBill masterBill1 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			masterBill1.BB_BillNum = "MB1";
			headerRow = new ISFHeaderRow(header);
			AssertEquals(masterBill1.PK, headerRow.MasterBillPK);
			AssertEquals(2, headerRow.Bills.Count);
			billRow1 = headerRow.Bills[0];
			billRow2 = headerRow.Bills[1];
			if (billRow1.BillPK == houseBill2.PK)
			{
				billRow1 = headerRow.Bills[1];
				billRow2 = headerRow.Bills[2];
			}

			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			CusISFBill masterBill2 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			headerRow = new ISFHeaderRow(header);
			AssertEquals(ZGuid.Empty, headerRow.MasterBillPK);
			AssertEquals(2, headerRow.Bills.Count);
			billRow1 = headerRow.Bills[0];
			billRow2 = headerRow.Bills[1];
			if (billRow1.BillPK == houseBill2.PK)
			{
				billRow1 = headerRow.Bills[1];
				billRow2 = headerRow.Bills[2];
			}

			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			masterBill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			headerRow = new ISFHeaderRow(header);
			AssertEquals(ZGuid.Empty, headerRow.MasterBillPK);
			AssertEquals(2, headerRow.Bills.Count);
			billRow1 = headerRow.Bills[0];
			billRow2 = headerRow.Bills[1];
			if (billRow1.BillPK == houseBill2.PK)
			{
				billRow1 = headerRow.Bills[1];
				billRow2 = headerRow.Bills[2];
			}

			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			masterBill1.Delete();
			headerRow = new ISFHeaderRow(header);
			AssertEquals(masterBill2.PK, headerRow.MasterBillPK);
			AssertEquals(1, headerRow.Bills.Count);
			AssertEquals(masterBill2.PK, headerRow.Bills[0].BillPK);
		}

		public void TestDefaultConsolPK()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = "MB1";
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_MasterBillNum = "MB2";
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_MasterBillNum = "MB2";
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			masterBill1.BB_BillNum = "MB1";
			CusISFBill masterBill2 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			masterBill2.BB_BillNum = "MB2";
			CusISFBill masterBill3 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			masterBill3.BB_BillNum = "MB3";
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
			headerRow.MasterBillPK = masterBill1.PK;
			AssertEquals(consol1.PK, headerRow.ConsolPK);
			headerRow.MasterBillPK = masterBill3.PK;
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
			headerRow.MasterBillPK = masterBill2.PK;
			AssertEquals(ZGuid.Empty, headerRow.ConsolPK);
		}

		public void TestMasterBill()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			CusISFBill masterBill2 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			CusISFBill houseBill1 = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			CusISFBill houseBill2 = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			CusISFBill oceanBill1 = AddBill(header, BillTypeList.Codes.OceanBillOfLading);
			CusISFBill oceanBill2 = AddBill(header, BillTypeList.Codes.OceanBillOfLading);
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			foreach (ZGuid masterBillPK in new ZGuid[] { ZGuid.Empty, houseBill1.PK, houseBill2.PK, ZGuid.Invalid })
			{
				headerRow.MasterBillPK = masterBillPK;
				AssertNull(headerRow.MasterBill);
			}

			headerRow.MasterBillPK = masterBill1.PK;
			AssertEquals(masterBill1, headerRow.MasterBill);
			AssertEquals(2, headerRow.Bills.Count);
			ISFBillRow billRow1 = headerRow.Bills[0];
			ISFBillRow billRow2 = headerRow.Bills[1];
			if (billRow1.BillPK == houseBill2.PK)
			{
				billRow1 = headerRow.Bills[1];
				billRow2 = headerRow.Bills[2];
			}

			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			headerRow.MasterBillPK = oceanBill1.PK;
			AssertEquals(oceanBill1, headerRow.MasterBill);
			AssertEquals(1, headerRow.Bills.Count);
			AssertEquals(billRow1, headerRow.Bills[0]);
			AssertEquals(oceanBill1.PK, billRow1.BillPK);
			AssertEquals(true, billRow2.IsDeleted);
			headerRow.MasterBillPK = houseBill1.PK;
			AssertNull(headerRow.MasterBill);
			AssertEquals(2, headerRow.Bills.Count);
			AssertEquals(billRow1, headerRow.Bills[0]);
			billRow2 = headerRow.Bills[1];
			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			headerRow.MasterBillPK = masterBill2.PK;
			AssertEquals(masterBill2, headerRow.MasterBill);
			AssertEquals(2, headerRow.Bills.Count);
			AssertEquals(billRow1, headerRow.Bills[0]);
			AssertEquals(billRow2, headerRow.Bills[1]);
			AssertEquals(houseBill1.PK, billRow1.BillPK);
			AssertEquals(houseBill2.PK, billRow2.BillPK);
			billRow2.BillPK = oceanBill2.PK;
			headerRow.MasterBillPK = oceanBill2.PK;
			AssertEquals(oceanBill2, headerRow.MasterBill);
			AssertEquals(1, headerRow.Bills.Count);
			AssertEquals(billRow2, headerRow.Bills[0]);
			AssertEquals(oceanBill2.PK, billRow2.BillPK);
			AssertEquals(true, billRow1.IsDeleted);
			headerRow.Bills.RemoveAndDeleteAll();
			headerRow.MasterBillPK = oceanBill1.PK;
			AssertEquals(oceanBill1, headerRow.MasterBill);
			AssertEquals(1, headerRow.Bills.Count);
			AssertEquals(oceanBill1.PK, headerRow.Bills[0].BillPK);
		}

		public void TestMasterBills()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill1 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			CusISFBill masterBill2 = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			CusISFBill houseBill1 = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			CusISFBill houseBill2 = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			CusISFBill oceanBill1 = AddBill(header, BillTypeList.Codes.OceanBillOfLading);
			CusISFBill oceanBill2 = AddBill(header, BillTypeList.Codes.OceanBillOfLading);
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			var masterBills = headerRow.MasterBills;
			AssertEquals(4, masterBills.Count);
			AssertCollectionContains(masterBill1, masterBills);
			AssertCollectionContains(masterBill2, masterBills);
			AssertCollectionContains(oceanBill1, masterBills);
			AssertCollectionContains(oceanBill2, masterBills);
		}

		public void TestIsOceanBillData()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill masterBill = AddBill(header, BillTypeList.Codes.MasterBillOfLading);
			CusISFBill houseBill = AddBill(header, BillTypeList.Codes.HouseBillOfLading);
			CusISFBill oceanBill = AddBill(header, BillTypeList.Codes.OceanBillOfLading);
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(false, headerRow.IsOceanBillData);
			headerRow.MasterBillPK = masterBill.PK;
			AssertEquals(false, headerRow.IsOceanBillData);
			headerRow.MasterBillPK = houseBill.PK;
			AssertEquals(false, headerRow.IsOceanBillData);
			headerRow.MasterBillPK = oceanBill.PK;
			AssertEquals(true, headerRow.IsOceanBillData);
		}

		public void TestContainers()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "CNT1";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "CNT2";
			CusISFEquip container3 = header.Equipments.AddNew();
			container3.BE_ContainerNum = "CNT3";
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(3, headerRow.Containers.Count);
			var list = headerRow.Containers.ToArray<ISFContainerRow>();
			AssertNotNull(list.First(x => x.ContainerNumber == "CNT1"));
			AssertNotNull(list.First(x => x.ContainerNumber == "CNT2"));
			AssertNotNull(list.First(x => x.ContainerNumber == "CNT3"));
		}

		protected override BusinessObject GetNewBusinessObject() => new ISFHeaderRow(Factory.New<CusISFHeader>());

		CusISFBill AddBill(CusISFHeader header, ZString billType)
		{
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = billType;
			return bill;
		}
	}
}
