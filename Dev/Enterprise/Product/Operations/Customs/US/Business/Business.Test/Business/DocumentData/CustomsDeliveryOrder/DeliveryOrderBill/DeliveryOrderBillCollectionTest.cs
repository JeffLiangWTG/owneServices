using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using BillTypeList = Enterprise.Customs.Business.BillTypeList;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderBillCollection))]
	sealed class DeliveryOrderBillCollectionTest : ActiveBusinessObjectCollectionTestCase<DeliveryOrderBillCollection>
	{
		public void TestShowDeliveryOrderBillsInReport()
		{
			Header.DeliveryOrderBills.DeleteAll();
			AssertEquals(0, Header.DeliveryOrderBills.Count);
			AssertEquals("when count = 0 show = false", false, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
			var bill1 = Header.DeliveryOrderBills.AddNew();
			bill1.CY_Code = BillTypeList.Codes.MasterBill;
			bill1.CY_Data = "MB001";
			AssertEquals(1, Header.DeliveryOrderBills.Count);
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals("when count = 1 master bill = 1 then show = false", false, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
			Header.DeliveryOrderBills.DeleteAll();
			AssertEquals(0, Header.DeliveryOrderBills.Count);
			var bill2 = Header.DeliveryOrderBills.AddNew();
			bill2.CY_Code = BillTypeList.Codes.HouseBill;
			bill2.CY_Data = "HB001";
			AssertEquals(1, Header.DeliveryOrderBills.Count);
			AssertEquals(1, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when count = 1 house bill = 1 then show = false", false, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
			var bill3 = Header.DeliveryOrderBills.AddNew();
			bill3.CY_Code = BillTypeList.Codes.MasterBill;
			bill3.CY_Data = "MB001";
			AssertEquals(2, Header.DeliveryOrderBills.Count);
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(1, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when count = 2 master bill = 1 and house bill = 1 then show = false", false, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
			bill2.CY_Code = BillTypeList.Codes.MasterBill;
			bill2.CY_Data = "MB002";
			AssertEquals(2, Header.DeliveryOrderBills.Count);
			AssertEquals(2, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(0, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when count = 2 master bill = 2 then show = true", true, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
			bill2.CY_Code = BillTypeList.Codes.HouseBill;
			bill2.CY_Data = "HB001";
			bill3.CY_Code = BillTypeList.Codes.HouseBill;
			bill3.CY_Data = "HB002";
			AssertEquals(2, Header.DeliveryOrderBills.Count);
			AssertEquals(0, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(2, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when count = 2 house bill = 2 then show = true", true, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
			var bill4 = Header.DeliveryOrderBills.AddNew();
			bill4.CY_Code = BillTypeList.Codes.MasterBill;
			bill4.CY_Data = "MB003";
			AssertEquals(3, Header.DeliveryOrderBills.Count);
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(2, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when count = 3 then show = true", true, Header.DeliveryOrderBills.ShowDeliveryOrderBillsInReport);
		}

		public void TestBills()
		{
			AssertEquals(0, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(0, Header.DeliveryOrderBills.HouseBills.Count);
			DeliveryOrderBill bill1 = Header.DeliveryOrderBills.AddNew();
			bill1.CY_Code = BillTypeList.Codes.MasterBill;
			bill1.CY_Data = "MB123";
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(bill1, Header.DeliveryOrderBills.MasterBills[0]);
			AssertEquals(0, Header.DeliveryOrderBills.HouseBills.Count);
			DeliveryOrderBill bill2 = Header.DeliveryOrderBills.AddNew();
			bill2.CY_Code = BillTypeList.Codes.HouseBill;
			bill2.CY_Data = "HB123";
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(bill1, Header.DeliveryOrderBills.MasterBills[0]);
			AssertEquals(1, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals(bill2, Header.DeliveryOrderBills.HouseBills[0]);
			DeliveryOrderBill bill3 = Header.DeliveryOrderBills.AddNew();
			bill3.CY_Code = BillTypeList.Codes.SubHouseBill;
			bill3.CY_Data = "SH123";
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals(bill1, Header.DeliveryOrderBills.MasterBills[0]);
			AssertEquals(1, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals(bill2, Header.DeliveryOrderBills.HouseBills[0]);
			DeliveryOrderBill bill4 = Header.DeliveryOrderBills.AddNew();
			bill4.CY_Code = BillTypeList.Codes.MasterBill;
			bill4.CY_Data = "MB234";
			AssertEquals(2, Header.DeliveryOrderBills.MasterBills.Count);
			AssertContainsExactElementsInAnyOrder(new DeliveryOrderBill[] { bill1, bill4 }, Header.DeliveryOrderBills.MasterBills);
			AssertEquals(1, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals(bill2, Header.DeliveryOrderBills.HouseBills[0]);
			DeliveryOrderBill bill5 = Header.DeliveryOrderBills.AddNew();
			bill5.CY_Code = BillTypeList.Codes.HouseBill;
			bill5.CY_Data = "HB234";
			AssertEquals(2, Header.DeliveryOrderBills.MasterBills.Count);
			AssertContainsExactElementsInAnyOrder(new DeliveryOrderBill[] { bill1, bill4 }, Header.DeliveryOrderBills.MasterBills);
			AssertEquals(2, Header.DeliveryOrderBills.HouseBills.Count);
			AssertContainsExactElementsInAnyOrder(new DeliveryOrderBill[] { bill2, bill5 }, Header.DeliveryOrderBills.HouseBills);
		}

		protected override DeliveryOrderBillCollection GetCollectionToTest() => new DeliveryOrderBillCollection(Header);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DeliveryOrderBill result = Factory.New<DeliveryOrderBill>();
			result.CY_ParentTableCode = Header.TablePrefix;
			return result;
		}

		DeliveryOrderHeader header;
		DeliveryOrderHeader Header
		{
			get
			{
				if (header == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					header = declaration.DeliveryOrderHeaders.AddNew();
				}

				return header;
			}
		}
	}
}
