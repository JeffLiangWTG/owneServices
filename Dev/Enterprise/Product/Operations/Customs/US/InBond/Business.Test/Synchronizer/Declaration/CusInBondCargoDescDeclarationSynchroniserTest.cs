using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchronizeCusInBondCargoDesc()
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT12321";
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB1";
			var moveHeader = header.MovementHeader;
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			var moveDetail = moveHeader.MovementDetails[0];
			AssertEquals("moveDetail.Containers.Count", 0, moveDetail.Containers.Count);
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = "CONT12321";
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			var inBondContainer = moveDetail.Containers[0];
			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			var commodity = inBondContainer.Commodities[0];
			AssertEquals("commodity.BY_MarksAndNumbers", ZString.Empty, commodity.BY_MarksAndNumbers);
			AssertEquals("commodity.BY_MarksAndNumbersInfo.ReadOnly", true, commodity.BY_MarksAndNumbersInfo.ReadOnly);
			var maxMarksAndNumbers = "HELLO WORLD".PadRight(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength, '@');
			declaration.JE_MarksAndNumbers = maxMarksAndNumbers + "D";
			AssertEquals("commodity.BY_MarksAndNumbers", maxMarksAndNumbers, commodity.BY_MarksAndNumbers);
			AssertEquals("commodity.BY_MarksAndNumbersInfo.ReadOnly", true, commodity.BY_MarksAndNumbersInfo.ReadOnly);
			package.CW_MarksAndNos = "HELLO WORLD";
			AssertEquals("commodity.BY_MarksAndNumbers", "HELLO WORLD", commodity.BY_MarksAndNumbers);
			AssertEquals("commodity.BY_MarksAndNumbersInfo.ReadOnly", true, commodity.BY_MarksAndNumbersInfo.ReadOnly);

			AssertEquals("commodity.BY_PieceCount", ZInt.Zero, commodity.BY_PieceCount);
			AssertEquals("commodity.BY_PieceCountInfo.ReadOnly", true, commodity.BY_PieceCountInfo.ReadOnly);
			package.CW_PackQty = 2560;
			AssertEquals("commodity.BY_PieceCount", 2560, commodity.BY_PieceCount);
			AssertEquals("commodity.BY_PieceCountInfo.ReadOnly", true, commodity.BY_PieceCountInfo.ReadOnly);

			AssertEquals("commodity.BY_ManifestUnitCode", ZString.Empty, commodity.BY_ManifestUnitCode);
			AssertEquals("commodity.BY_ManifestUnitCodeInfo.ReadOnly", true, commodity.BY_ManifestUnitCodeInfo.ReadOnly);
			package.CW_PackType = "SD";
			AssertEquals("commodity.BY_ManifestUnitCode", "SD", commodity.BY_ManifestUnitCode);
			AssertEquals("commodity.BY_ManifestUnitCodeInfo.ReadOnly", true, commodity.BY_ManifestUnitCodeInfo.ReadOnly);
			AssertNoExceptionThrown("Synchronisation data able to save without any issue", () => Factory.Save());
		}
	}
}
