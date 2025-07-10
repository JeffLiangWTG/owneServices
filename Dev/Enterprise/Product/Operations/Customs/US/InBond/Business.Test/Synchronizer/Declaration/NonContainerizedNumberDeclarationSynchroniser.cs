using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchronizeNonContainerizedData()
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT12321";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB233";
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			package.CW_MarksAndNos = "HELLO WORLD";
			var moveHeader = header.MovementHeader;
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			var moveDetail = moveHeader.MovementDetails[0];
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			var inBondContainer = moveDetail.Containers[0];
			AssertEquals("inBondContainer.BC_ContainerNum", CusInBondContainer.NonContainerizedNumber, inBondContainer.BC_ContainerNum);
			AssertEquals("inBondContainer.BC_ContainerNumInfo.ReadOnly", true, inBondContainer.BC_ContainerNumInfo.ReadOnly);
			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			var commodity = inBondContainer.Commodities[0];
			AssertEquals("commodity.BY_MarksAndNumbers", "HELLO WORLD", commodity.BY_MarksAndNumbers);

			package.CW_ContainerNoOrEquipmentNo = "CONT12321";
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			inBondContainer = moveDetail.Containers[0];
			AssertEquals("inBondContainer.BC_ContainerNum", "CONT12321", inBondContainer.BC_ContainerNum);
			AssertEquals("inBondContainer.BC_ContainerNumInfo.ReadOnly", true, inBondContainer.BC_ContainerNumInfo.ReadOnly);
			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			AssertEquals("inBondContainer.Commodities[0]", commodity, inBondContainer.Commodities[0]);
			AssertEquals("commodity.BY_MarksAndNumbers", "HELLO WORLD", commodity.BY_MarksAndNumbers);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}
	}
}
