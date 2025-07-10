using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchronizeContainerizedData()
		{
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "#1A";
			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "#2A";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT12321";
			container.CO_Seal = "SL323";
			container.CO_SecondSeal = "SL4394";
			container.CO_RC = refContainer1.PK;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB233";
			var package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = "CONT12321";
			package.CW_MarksAndNos = "HELLO WORLD";
			var moveHeader = header.MovementHeader;
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			var moveDetail = moveHeader.MovementDetails[0];
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			var inBondContainer = moveDetail.Containers[0];

			AssertEquals("inBondContainer.BC_ContainerNum", "CONT12321", inBondContainer.BC_ContainerNum);
			AssertEquals("inBondContainer.BC_ContainerNumInfo.ReadOnly", true, inBondContainer.BC_ContainerNumInfo.ReadOnly);
			container.CO_ContainerNumber = "CONT8964";
			AssertEquals("inBondContainer.BC_ContainerNum", "CONT8964", inBondContainer.BC_ContainerNum);
			AssertEquals("inBondContainer.BC_ContainerNumInfo.ReadOnly", true, inBondContainer.BC_ContainerNumInfo.ReadOnly);

			AssertEquals("inBondContainer.BC_Seal1", "SL323", inBondContainer.BC_Seal1);
			AssertEquals("inBondContainer.BC_Seal1Info.ReadOnly", true, inBondContainer.BC_Seal1Info.ReadOnly);
			container.CO_Seal = "GF968";
			AssertEquals("inBondContainer.BC_Seal1", "GF968", inBondContainer.BC_Seal1);
			AssertEquals("inBondContainer.BC_Seal1Info.ReadOnly", true, inBondContainer.BC_Seal1Info.ReadOnly);
			container.CO_Seal = ZString.Empty;
			AssertEquals("inBondContainer.BC_Seal1", ZString.Empty, inBondContainer.BC_Seal1);
			AssertEquals("inBondContainer.BC_Seal1Info.ReadOnly", true, inBondContainer.BC_Seal1Info.ReadOnly);

			AssertEquals("inBondContainer.BC_Seal2", "SL4394", inBondContainer.BC_Seal2);
			AssertEquals("inBondContainer.BC_Seal2Info.ReadOnly", true, inBondContainer.BC_Seal2Info.ReadOnly);
			container.CO_SecondSeal = "KJ66";
			AssertEquals("inBondContainer.BC_Seal2", "KJ66", inBondContainer.BC_Seal2);
			AssertEquals("inBondContainer.BC_Seal2Info.ReadOnly", true, inBondContainer.BC_Seal2Info.ReadOnly);
			container.CO_SecondSeal = ZString.Empty;
			AssertEquals("inBondContainer.BC_Seal2", ZString.Empty, inBondContainer.BC_Seal2);
			AssertEquals("inBondContainer.BC_Seal2Info.ReadOnly", true, inBondContainer.BC_Seal2Info.ReadOnly);

			AssertEquals("inBondContainer.BC_RC", refContainer1.PK, inBondContainer.BC_RC);
			AssertEquals("inBondContainer.BC_RCInfo.ReadOnly", true, inBondContainer.BC_RCInfo.ReadOnly);
			container.CO_RC = ZGuid.Empty;
			AssertEquals("inBondContainer.BC_RC", ZGuid.Empty, inBondContainer.BC_RC);
			AssertEquals("inBondContainer.BC_RCInfo.ReadOnly", true, inBondContainer.BC_RCInfo.ReadOnly);
			container.CO_RC = refContainer2.PK;
			AssertEquals("inBondContainer.BC_RC", refContainer2.PK, inBondContainer.BC_RC);
			AssertEquals("inBondContainer.BC_RCInfo.ReadOnly", true, inBondContainer.BC_RCInfo.ReadOnly);

			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			var commodity = inBondContainer.Commodities[0];
			AssertEquals("commodity.BY_MarksAndNumbers", "HELLO WORLD", commodity.BY_MarksAndNumbers);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}
	}
}
