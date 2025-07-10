using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchronizeCusInBondMoveDetail()
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT12321";

			var moveHeader = header.MovementHeader;
			AssertEquals("moveHeader.MovementDetails.Count", 0, moveHeader.MovementDetails.Count);
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB233";

			AssertEquals("header.Bills.Count", 1, header.Bills.Count);
			var inBondBill = header.Bills[0];
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			var moveDetail = moveHeader.MovementDetails[0];
			AssertEquals("moveDetail.B9_B0", inBondBill.PK, moveDetail.B9_B0);
			AssertEquals("moveDetail.B9_B0Info.ReadOnly", true, moveDetail.B9_B0Info.ReadOnly);

			AssertEquals("moveDetail.Containers.Count", 0, moveDetail.Containers.Count);
			var package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "HELLO WORLD";
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			var inBondContainer = moveDetail.Containers[0];
			AssertEquals("inBondContainer.BC_ContainerNum", CusInBondContainer.NonContainerizedNumber, inBondContainer.BC_ContainerNum);
			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			var commodity = inBondContainer.Commodities[0];
			AssertEquals("commodity.BY_MarksAndNumbers", "HELLO WORLD", commodity.BY_MarksAndNumbers);

			package.CW_ContainerNoOrEquipmentNo = "CONT12321";
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			inBondContainer = moveDetail.Containers[0];
			AssertEquals("inBondContainer.BC_ContainerNum", "CONT12321", inBondContainer.BC_ContainerNum);
			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			AssertEquals("inBondContainer.Commodities[0]", commodity, inBondContainer.Commodities[0]);
			AssertEquals("commodity.BY_MarksAndNumbers", "HELLO WORLD", commodity.BY_MarksAndNumbers);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}
	}
}
