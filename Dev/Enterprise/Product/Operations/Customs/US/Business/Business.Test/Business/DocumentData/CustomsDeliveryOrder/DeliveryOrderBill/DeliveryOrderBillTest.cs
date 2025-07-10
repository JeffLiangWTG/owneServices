using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderBill))]
	sealed class DeliveryOrderBillTest : Customs.Business.Testing.CusCodeDataTest<DeliveryOrderBill>
	{
		public void TestShowDeliveryOrderBillInReport()
		{
			Header.DeliveryOrderBills.DeleteAll();
			AssertEquals(0, Header.DeliveryOrderBills.Count);
			var bill1 = Header.DeliveryOrderBills.AddNew();
			bill1.CY_Code = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CY_Data = "MB001";
			AssertEquals(1, Header.DeliveryOrderBills.Count);
			AssertEquals(1, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals("when master bill = 1 then hide", false, Header.DeliveryOrderBills.MasterBills[0].ShowDeliveryOrderBillInReport);
			var bill2 = Header.DeliveryOrderBills.AddNew();
			bill2.CY_Code = Customs.Business.BillTypeList.Codes.MasterBill;
			bill2.CY_Data = "MB002";
			AssertEquals(2, Header.DeliveryOrderBills.MasterBills.Count);
			AssertEquals("when house bills = 2 then first.show = true, second.show = true", true, Header.DeliveryOrderBills.MasterBills[0].ShowDeliveryOrderBillInReport);
			AssertEquals("when house bills = 2 then first.show = true, second.show = true", true, Header.DeliveryOrderBills.MasterBills[1].ShowDeliveryOrderBillInReport);
			Header.DeliveryOrderBills.DeleteAll();
			AssertEquals(0, Header.DeliveryOrderBills.Count);
			var bill3 = Header.DeliveryOrderBills.AddNew();
			bill3.CY_Code = Customs.Business.BillTypeList.Codes.HouseBill;
			bill3.CY_Data = "MB001";
			AssertEquals(1, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when master bill = 1 then hide", false, Header.DeliveryOrderBills.HouseBills[0].ShowDeliveryOrderBillInReport);
			var bill4 = Header.DeliveryOrderBills.AddNew();
			bill4.CY_Code = Customs.Business.BillTypeList.Codes.HouseBill;
			bill4.CY_Data = "MB002";
			AssertEquals(2, Header.DeliveryOrderBills.HouseBills.Count);
			AssertEquals("when house bills = 2 then first.show = true, second.show = true", true, Header.DeliveryOrderBills.HouseBills[0].ShowDeliveryOrderBillInReport);
			AssertEquals("when house bills = 2 then first.show = true, second.show = true", true, Header.DeliveryOrderBills.HouseBills[1].ShowDeliveryOrderBillInReport);
		}

		public void TestUpdateBillDetails()
		{
			Bill bill1 = Declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill1.CU_BillNum = "HB323432";
			bill1.US_UI_NKBillIssuerSCAC = "ABDD";
			Bill bill2 = Declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "HB685445";
			bill2.US_UI_NKBillIssuerSCAC = "ABDD";
			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TURE3232342";
			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "TURE9856485";
			CusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "TURE3689564";
			PackingGroup packingGroup1 = Declaration.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = bill1.PK;
			packingGroup1.CR_CO_Container = container1.PK;
			PackingGroup packingGroup2 = Declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill1.PK;
			packingGroup2.CR_CO_Container = container2.PK;
			PackingGroup packingGroup3 = Declaration.PackingGroups.AddNew();
			packingGroup3.CR_CU_HouseBill = bill2.PK;
			packingGroup3.CR_CO_Container = container3.PK;
			Header.DeliveryOrderContainers.RemoveAndDeleteAll();
			DeliveryOrderBill orderBill = Header.DeliveryOrderBills.AddNew();
			orderBill.CY_Data = "ABDD";
			AssertEquals(ZString.Empty, orderBill.CY_Code);
			AssertEquals(0, Header.DeliveryOrderContainers.Count);
			orderBill.CY_Data = "HB323432";
			AssertEquals(2, Header.DeliveryOrderContainers.Count);
			DeliveryOrderContainer orderContainer1 = Header.DeliveryOrderContainers[0];
			DeliveryOrderContainer orderContainer2 = Header.DeliveryOrderContainers[1];
			if (orderContainer1.US_ContainerNumber != "TURE3232342")
			{
				orderContainer1 = Header.DeliveryOrderContainers[1];
				orderContainer2 = Header.DeliveryOrderContainers[0];
			}

			AssertEquals("TURE3232342", orderContainer1.US_ContainerNumber);
			AssertEquals("TURE9856485", orderContainer2.US_ContainerNumber);
			Header.DeliveryOrderContainers.RemoveAndDeleteAll();
			orderBill.CY_Data = "ABDDHB323432";
			AssertEquals(Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, orderBill.CY_Code);
			AssertEquals(2, Header.DeliveryOrderContainers.Count);
			orderContainer1 = Header.DeliveryOrderContainers[0];
			orderContainer2 = Header.DeliveryOrderContainers[1];
			if (orderContainer1.US_ContainerNumber != "TURE3232342")
			{
				orderContainer1 = Header.DeliveryOrderContainers[1];
				orderContainer2 = Header.DeliveryOrderContainers[0];
			}

			AssertEquals("TURE3232342", orderContainer1.US_ContainerNumber);
			AssertEquals("TURE9856485", orderContainer2.US_ContainerNumber);
			Header.DeliveryOrderContainers.RemoveAndDeleteAll();
			orderBill.CY_Data = "ABDDHB685445";
			AssertEquals(1, Header.DeliveryOrderContainers.Count);
			AssertEquals("TURE3689564", Header.DeliveryOrderContainers[0].US_ContainerNumber);
			bill2.US_UI_NKBillIssuerSCAC = "AB";
			Header.DeliveryOrderContainers.RemoveAndDeleteAll();
			orderBill.CY_Data = "ABHB685445";
			AssertEquals(1, Header.DeliveryOrderContainers.Count);
			AssertEquals("TURE3689564", Header.DeliveryOrderContainers[0].US_ContainerNumber);
		}

		public void TestSetDefaultValues()
		{
			DeliveryOrderBill deliveryOrderBill = (DeliveryOrderBill)GetNewBusinessObject();
			AssertEquals("default value", deliveryOrderBill.CY_Type, DeliveryOrderHeader.Constants.CusCodeDataBill);
		}

		public void TestBillPackagesIsNotCopied()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, declaration.ContainersRequired);
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = ShippingOrPackingingUnitList.Codes.Bolt;
			declaration.JE_GoodsDescription = "GOODS DESC";
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "BH3232342";
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "BH9856485";
			PackingGroup packingGroup1 = declaration.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = bill1.PK;
			Package package1 = packingGroup1.Packages.AddNew();
			package1.CW_PackQty = 10;
			package1.CW_PackType = ShippingOrPackingingUnitList.Codes.Bag;
			package1.CW_MarksAndNos = "Marks N Numbers 1";
			Package package2 = packingGroup1.Packages.AddNew();
			package2.CW_PackQty = 20;
			package2.CW_PackType = ShippingOrPackingingUnitList.Codes.Plate;
			package2.CW_MarksAndNos = "Marks N Numbers 2";
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TURE3232342";
			PackingGroup packingGroup2 = declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill2.PK;
			packingGroup2.CR_CO_Container = container1.PK;
			Package package3 = packingGroup2.Packages.AddNew();
			package3.CW_PackQty = 16;
			package3.CW_PackType = ShippingOrPackingingUnitList.Codes.Matchbox;
			package3.CW_MarksAndNos = "Marks N Numbers 3";
			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			AssertLine(header.DeliveryOrderLines[0], 10, ShippingOrPackingingUnitList.Codes.Bolt, "GOODS DESC");
			header.DeliveryOrderLines.RemoveAndDeleteAll();
			DeliveryOrderBill orderBill = header.DeliveryOrderBills.AddNew();
			orderBill.CY_Data = "BH3232342";
			AssertEquals(0, header.DeliveryOrderContainers.Count);
			AssertEquals(0, header.DeliveryOrderLines.Count);
			orderBill.CY_Data = "BH9856485";
			AssertEquals(0, header.DeliveryOrderContainers.Count);
			AssertEquals(0, header.DeliveryOrderLines.Count);
			bill2.US_UI_NKBillIssuerSCAC = "AD";
			orderBill.CY_Data = "ADBH9856485";
			AssertEquals(0, header.DeliveryOrderContainers.Count);
			AssertEquals(0, header.DeliveryOrderLines.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => Header.DeliveryOrderBills.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var bill = header.DeliveryOrderBills.AddNew();
			bill.CY_Data = "12322";
			return bill;
		}

		void AssertLine(DeliveryOrderLine deliveryOrderLine, ZInt packQty, ZString packType, ZString marksAndNumbers)
		{
			AssertEquals(packQty, deliveryOrderLine.US_NoOfPackages);
			AssertEquals(packType, deliveryOrderLine.US_PackageType);
			AssertEquals(marksAndNumbers, deliveryOrderLine.US_GoodsDescription);
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		DeliveryOrderHeader header;
		DeliveryOrderHeader Header => header ?? (header = Declaration.DeliveryOrderHeaders.AddNew());
	}
}
