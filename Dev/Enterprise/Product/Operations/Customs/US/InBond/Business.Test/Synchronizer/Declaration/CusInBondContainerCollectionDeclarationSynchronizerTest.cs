using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestReSynchronizationOfContainers()
		{
			synchronizer.SetEnabled(false, false);
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_Seal = "SL1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT1";
			container2.CO_Seal = "SL2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT1";
			container3.CO_Seal = "SL3";
			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CONT4";
			container4.CO_Seal = "SL4";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB1";
			bill.US_UI_NKBillIssuerSCAC = "ABCD";

			var packingGroup1 = bill.PackingGroups.AddNew(container1);
			var package1 = packingGroup1.Packages.AddNew();
			package1.CW_MarksAndNos = "MARKS 1";
			var packingGroup2 = bill.PackingGroups.AddNew(container2);
			var package2 = packingGroup2.Packages.AddNew();
			package2.CW_MarksAndNos = "MARKS 2";
			var packingGroup3 = bill.PackingGroups.AddNew(container3);
			var package3 = packingGroup3.Packages.AddNew();
			package3.CW_MarksAndNos = "MARKS 3";
			var packingGroup4 = bill.PackingGroups.AddNew(container4);
			var package4 = packingGroup4.Packages.AddNew();
			package4.CW_MarksAndNos = "MARKS 4";
			var packingGroup5 = bill.PackingGroups.AddNew();
			packingGroup5.CR_CO_Container = ZGuid.Empty;
			var package5 = packingGroup5.Packages.AddNew();
			package5.CW_MarksAndNos = "MARKS 5";

			var inBondBill = header.Bills.AddNew();
			inBondBill.B0_MasterBillNumber = "MB1";
			inBondBill.B0_IssuerCode = "ABCD";

			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew(inBondBill.PK);
			var inBondContainer1 = moveDetail.Containers.AddNew();
			inBondContainer1.BC_ContainerNum = "CONT1";
			AssertEquals("PreCondition: inBondContainer1.BC_Seal1", ZString.Empty, inBondContainer1.BC_Seal1);
			var inBondContainer2 = moveDetail.Containers.AddNew();
			inBondContainer2.BC_ContainerNum = "CONT1";
			AssertEquals("PreCondition: inBondContainer2.BC_Seal1", ZString.Empty, inBondContainer2.BC_Seal1);
			var inBondContainer3 = moveDetail.Containers.AddNew();
			inBondContainer3.BC_ContainerNum = "CONT1";
			AssertEquals("PreCondition: inBondContainer3.BC_Seal1", ZString.Empty, inBondContainer3.BC_Seal1);
			var inBondContainer4 = moveDetail.Containers.AddNew();
			inBondContainer4.BC_ContainerNum = "CONT4NOMATCH";
			AssertEquals("PreCondition: inBondContainer4.BC_Seal1", ZString.Empty, inBondContainer4.BC_Seal1);
			var inBondContainer5 = moveDetail.Containers.AddNew();
			inBondContainer5.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertEquals("PreCondition: inBondContainer5.BC_Seal1", ZString.Empty, inBondContainer5.BC_Seal1);

			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 5, moveDetail.Containers.Count);
			AssertEquals("inBondContainer1 matched", inBondContainer1, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL1"));
			AssertEquals("inBondContainer1.Commodities.Count", 1, inBondContainer1.Commodities.Count);
			var inBondContainer1Commodity = inBondContainer1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1");
			AssertNotNull("inBondContainer1Commodity", inBondContainer1Commodity);
			AssertEquals("inBondContainer2 matched", inBondContainer2, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL2"));
			AssertEquals("inBondContainer2.Commodities.Count", 1, inBondContainer2.Commodities.Count);
			var inBondContainer2Commodity = inBondContainer2.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2");
			AssertNotNull("inBondContainer2Commodity", inBondContainer2Commodity);
			AssertEquals("inBondContainer3 matched", inBondContainer3, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL3"));
			AssertEquals("inBondContainer3.Commodities.Count", 1, inBondContainer3.Commodities.Count);
			var inBondContainer3Commodity = inBondContainer3.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 3");
			AssertNotNull("inBondContainer3Commodity", inBondContainer3Commodity);
			AssertEquals("inBondContainer4.IsDeleted", false, inBondContainer4.IsDeleted);
			AssertEquals("inBondContainer4.BC_ParentID", ZGuid.Empty, inBondContainer4.BC_ParentID);
			AssertEquals("inBondContainer4.Commodities.Count", 0, inBondContainer4.Commodities.Count);
			AssertEquals("inBondContainer5 matched", inBondContainer5, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber && x.BC_Seal1 == ZString.Empty));
			AssertEquals("inBondContainer5.Commodities.Count", 1, inBondContainer5.Commodities.Count);
			var inBondContainer5Commodity = inBondContainer5.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 5");
			AssertNotNull("inBondContainer5Commodity", inBondContainer5Commodity);
			var newInBondContainer4 = moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL4");
			AssertNotNull("newInBondContainer4 ", newInBondContainer4);
			AssertEquals("newInBondContainer4.Commodities.Count", 1, newInBondContainer4.Commodities.Count);
			var newInBondContainer4Commodity = newInBondContainer4.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 4");
			AssertNotNull("newInBondContainer4Commodity", newInBondContainer4Commodity);

			container2.CO_ContainerNumber = "CONT4";
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 5, moveDetail.Containers.Count);
			AssertEquals("inBondContainer1 matched", inBondContainer1, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL1"));
			AssertEquals("inBondContainer1.Commodities.Count", 1, inBondContainer1.Commodities.Count);
			AssertEquals("inBondContainer1Commodity matched", inBondContainer1Commodity, inBondContainer1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertEquals("inBondContainer2 matched", inBondContainer2, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL2"));
			AssertEquals("inBondContainer2.Commodities.Count", 1, inBondContainer2.Commodities.Count);
			AssertEquals("inBondContainer2Commodity matched", inBondContainer2Commodity, inBondContainer2.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));
			AssertEquals("inBondContainer3 matched", inBondContainer3, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL3"));
			AssertEquals("inBondContainer3.Commodities.Count", 1, inBondContainer3.Commodities.Count);
			AssertEquals("inBondContainer3Commodity matched", inBondContainer3Commodity, inBondContainer3.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 3"));
			AssertEquals("newInBondContainer4 matched", newInBondContainer4, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL4"));
			AssertEquals("newInBondContainer4.Commodities.Count", 1, newInBondContainer4.Commodities.Count);
			AssertEquals("newInBondContainer4Commodity matched", newInBondContainer4Commodity, newInBondContainer4.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 4"));
			AssertEquals("inBondContainer5 matched", inBondContainer5, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber && x.BC_Seal1 == ZString.Empty));
			AssertEquals("inBondContainer5.Commodities.Count", 1, inBondContainer5.Commodities.Count);
			AssertEquals("inBondContainer5Commodity matched", inBondContainer5Commodity, inBondContainer5.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 5"));

			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
			AssertEquals("inBondContainer4.IsDeleted", true, inBondContainer4.IsDeleted);
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 5, moveDetail.Containers.Count);
			AssertEquals("inBondContainer1 matched", inBondContainer1, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL1"));
			AssertEquals("inBondContainer1.Commodities.Count", 1, inBondContainer1.Commodities.Count);
			AssertEquals("inBondContainer1Commodity matched", inBondContainer1Commodity, inBondContainer1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertEquals("inBondContainer2 matched", inBondContainer2, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL2"));
			AssertEquals("inBondContainer2.Commodities.Count", 1, inBondContainer2.Commodities.Count);
			AssertEquals("inBondContainer2Commodity matched", inBondContainer2Commodity, inBondContainer2.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));
			AssertEquals("inBondContainer3 matched", inBondContainer3, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL3"));
			AssertEquals("inBondContainer3.Commodities.Count", 1, inBondContainer3.Commodities.Count);
			AssertEquals("inBondContainer3Commodity matched", inBondContainer3Commodity, inBondContainer3.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 3"));
			AssertEquals("newInBondContainer4 matched", newInBondContainer4, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL4"));
			AssertEquals("newInBondContainer4.Commodities.Count", 1, newInBondContainer4.Commodities.Count);
			AssertEquals("newInBondContainer4Commodity matched", newInBondContainer4Commodity, newInBondContainer4.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 4"));
			AssertEquals("inBondContainer5 matched", inBondContainer5, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber && x.BC_Seal1 == ZString.Empty));
			AssertEquals("inBondContainer5.Commodities.Count", 1, inBondContainer5.Commodities.Count);
			AssertEquals("inBondContainer5Commodity matched", inBondContainer5Commodity, inBondContainer5.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 5"));

			synchronizer.SetEnabled(false, false);
			packingGroup2.Delete();
			container1.CO_ContainerNumber = "CONT4";
			container1.CO_Seal = "SL2";
			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 4, moveDetail.Containers.Count);
			AssertEquals("inBondContainer1 matched", inBondContainer1, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL2"));
			AssertEquals("inBondContainer1.Commodities.Count", 1, inBondContainer1.Commodities.Count);
			AssertEquals("inBondContainer1Commodity matched", inBondContainer1Commodity, inBondContainer1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertEquals("inBondContainer2.IsDeleted", false, inBondContainer2.IsDeleted);
			AssertEquals("inBondContainer2.BC_ParentID", ZGuid.Empty, inBondContainer2.BC_ParentID);
			AssertEquals("inBondContainer2.Commodities.Count", 0, inBondContainer2.Commodities.Count);
			AssertEquals("inBondContainer2Commodity.IsDeleted", true, inBondContainer2Commodity.IsDeleted);
			AssertEquals("inBondContainer3 matched", inBondContainer3, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL3"));
			AssertEquals("inBondContainer3.Commodities.Count", 1, inBondContainer3.Commodities.Count);
			AssertEquals("inBondContainer3Commodity matched", inBondContainer3Commodity, inBondContainer3.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 3"));
			AssertEquals("newInBondContainer4 matched", newInBondContainer4, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL4"));
			AssertEquals("newInBondContainer4.Commodities.Count", 1, newInBondContainer4.Commodities.Count);
			AssertEquals("newInBondContainer4Commodity matched", newInBondContainer4Commodity, newInBondContainer4.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 4"));
			AssertEquals("inBondContainer5 matched", inBondContainer5, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber && x.BC_Seal1 == ZString.Empty));
			AssertEquals("inBondContainer5.Commodities.Count", 1, inBondContainer5.Commodities.Count);
			AssertEquals("inBondContainer5Commodity matched", inBondContainer5Commodity, inBondContainer5.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 5"));

			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 4, moveDetail.Containers.Count);
			AssertEquals("inBondContainer1 matched", inBondContainer1, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL2"));
			AssertEquals("inBondContainer1.Commodities.Count", 1, inBondContainer1.Commodities.Count);
			AssertEquals("inBondContainer1Commodity matched", inBondContainer1Commodity, inBondContainer1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertEquals("inBondContainer2.IsDeleted", true, inBondContainer2.IsDeleted);
			AssertEquals("inBondContainer3 matched", inBondContainer3, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL3"));
			AssertEquals("inBondContainer3 matched", inBondContainer3, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT1" && x.BC_Seal1 == "SL3"));
			AssertEquals("inBondContainer3.Commodities.Count", 1, inBondContainer3.Commodities.Count);
			AssertEquals("inBondContainer3Commodity matched", inBondContainer3Commodity, inBondContainer3.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 3"));
			AssertEquals("newInBondContainer4 matched", newInBondContainer4, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT4" && x.BC_Seal1 == "SL4"));
			AssertEquals("newInBondContainer4.Commodities.Count", 1, newInBondContainer4.Commodities.Count);
			AssertEquals("newInBondContainer4Commodity matched", newInBondContainer4Commodity, newInBondContainer4.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 4"));
			AssertEquals("inBondContainer5 matched", inBondContainer5, moveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber && x.BC_Seal1 == ZString.Empty));
			AssertEquals("inBondContainer5.Commodities.Count", 1, inBondContainer5.Commodities.Count);
			AssertEquals("inBondContainer5Commodity matched", inBondContainer5Commodity, inBondContainer5.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 5"));
		}

		public void TestSynchronizeContainers()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT12321";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "KDHD32323";
			var moveHeader = header.MovementHeader;
			AssertEquals("moveHeader.MovementDetails.Count", 0, moveHeader.MovementDetails.Count);

			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "MB1";
			var masterBill1HouseBill1 = masterBill1.ChildBills.AddNew();
			masterBill1HouseBill1.CU_BillNum = "MB1HB1";
			var masterBill1HouseBill2 = masterBill1.ChildBills.AddNew();
			masterBill1HouseBill2.CU_BillNum = "MB1HB2";
			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MB2";
			declaration.Packages.RemoveAndDeleteAll();

			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			var masterBill1MoveDetail = moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1");
			AssertEquals("masterBill1MoveDetail.Containers.Count", 0, masterBill1MoveDetail.Containers.Count);
			var masterBill2MoveDetail = moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2");
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);

			var package1 = declaration.Packages.AddNew();
			package1.CW_MarksAndNos = "MARKS 1";
			package1.CW_HouseBill = masterBill1.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			var masterBill1NonContainerizedNumber = masterBill1MoveDetail.Containers[0];
			AssertEquals("masterBill1NonContainerizedNumber.BC_ContainerNum", CusInBondContainer.NonContainerizedNumber, masterBill1NonContainerizedNumber.BC_ContainerNum);
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 1, masterBill1NonContainerizedNumber.Commodities.Count);
			var commodity1 = masterBill1NonContainerizedNumber.Commodities[0];
			AssertEquals("commodity1.BY_MarksAndNumbers", "MARKS 1", commodity1.BY_MarksAndNumbers);

			package1.CW_ContainerNoOrEquipmentNo = "KDHD32323";
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			var masterBill1Container1 = masterBill1MoveDetail.Containers[0];
			AssertNotEquals(masterBill1NonContainerizedNumber, masterBill1Container1);
			AssertEquals("masterBill1NonContainerizedNumber.BC_ParentID", ZGuid.Empty, masterBill1NonContainerizedNumber.BC_ParentID);
			AssertEquals("masterBill1NonContainerizedNumber.IsDeleted", false, masterBill1NonContainerizedNumber.IsDeleted);
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 0, masterBill1NonContainerizedNumber.Commodities.Count);
			AssertEquals("masterBill1Container1.BC_ContainerNum", "KDHD32323", masterBill1Container1.BC_ContainerNum);
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1 matched", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			var package2 = declaration.Packages.AddNew();
			package2.CW_MarksAndNos = "MARKS 2";
			package2.CW_HouseBill = masterBill2.CU_BillUniqueCode;
			package2.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			AssertEquals("masterBill2MoveDetail.Containers.Count", 1, masterBill2MoveDetail.Containers.Count);
			var masterBill2NonContainerizedNumber = masterBill2MoveDetail.Containers[0];
			AssertEquals("masterBill2NonContainerizedNumber.BC_ContainerNum", CusInBondContainer.NonContainerizedNumber, masterBill2NonContainerizedNumber.BC_ContainerNum);
			AssertEquals("masterBill2CmasterBill2NonContainerizedNumberontainer1.Commodities.Count", 1, masterBill2NonContainerizedNumber.Commodities.Count);
			var commodity2 = masterBill2NonContainerizedNumber.Commodities[0];
			AssertEquals("commodity2.BY_MarksAndNumbers", "MARKS 2", commodity2.BY_MarksAndNumbers);

			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container1 matched", masterBill1Container1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "KDHD32323"));
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1 matched", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			package2.CW_HouseBill = masterBill1HouseBill2.CU_BillUniqueCode;
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill2NonContainerizedNumber.BC_ParentID", ZGuid.Empty, masterBill2NonContainerizedNumber.BC_ParentID);
			AssertEquals("masterBill2NonContainerizedNumber.IsDeleted", false, masterBill2NonContainerizedNumber.IsDeleted);
			AssertEquals("masterBill2NonContainerizedNumber.Commodities.Count", 0, masterBill2NonContainerizedNumber.Commodities.Count);

			AssertEquals("masterBill1MoveDetail.Containers.Count", 2, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1NonContainerizedNumber matched", masterBill1NonContainerizedNumber, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber));
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 1, masterBill1NonContainerizedNumber.Commodities.Count);
			AssertEquals("commodity2 matched", commodity2, masterBill1NonContainerizedNumber.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			AssertEquals("masterBill1MoveDetail.Containers.Count", masterBill1Container1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "KDHD32323"));
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1 matched", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			package2.CW_ContainerNoOrEquipmentNo = "CONT12321";
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill2NonContainerizedNumber.BC_ParentID", ZGuid.Empty, masterBill2NonContainerizedNumber.BC_ParentID);
			AssertEquals("masterBill2NonContainerizedNumber.IsDeleted", false, masterBill2NonContainerizedNumber.IsDeleted);
			AssertEquals("masterBill2NonContainerizedNumber.Commodities.Count", 0, masterBill2NonContainerizedNumber.Commodities.Count);

			AssertEquals("masterBill1MoveDetail.Containers.Count", 2, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1NonContainerizedNumber.BC_ParentID", ZGuid.Empty, masterBill1NonContainerizedNumber.BC_ParentID);
			AssertEquals("masterBill1NonContainerizedNumber.IsDeleted", false, masterBill1NonContainerizedNumber.IsDeleted);
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 0, masterBill1NonContainerizedNumber.Commodities.Count);

			var masterBill1Container2 = masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT12321");
			AssertEquals("masterBill1Container2.Commodities.Count", 1, masterBill1Container2.Commodities.Count);
			AssertEquals("commodity2 matched", commodity2, masterBill1Container2.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			AssertEquals("masterBill1MoveDetail.Containers.Count", masterBill1Container1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "KDHD32323"));
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1 matched", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
			AssertEquals("masterBill2NonContainerizedNumber.IsDeleted", true, masterBill2NonContainerizedNumber.IsDeleted);
			AssertEquals("masterBill1NonContainerizedNumber.IsDeleted", true, masterBill1NonContainerizedNumber.IsDeleted);

			container1.Delete();
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container2.IsDeleted", true, masterBill1Container2.IsDeleted);
			masterBill1NonContainerizedNumber = masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber);
			AssertNotNull("masterBill1NonContainerizedNumber", masterBill1NonContainerizedNumber);
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 1, masterBill1NonContainerizedNumber.Commodities.Count);
			AssertEquals("commodity2 matched", commodity2, masterBill1NonContainerizedNumber.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			AssertEquals("masterBill1MoveDetail.Containers.Count", 2, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container1 matched", masterBill1Container1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "KDHD32323"));
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1 matched", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());

			package1.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			package2.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container1.BC_ParentID", ZGuid.Empty, masterBill1Container1.BC_ParentID);
			AssertEquals("masterBill1Container1.IsDeleted", false, masterBill1Container1.IsDeleted);
			AssertEquals("masterBill1Container1.Commodities.Count", 0, masterBill1Container1.Commodities.Count);

			AssertEquals("masterBill1NonContainerizedNumber matched", masterBill1NonContainerizedNumber, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber));
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 2, masterBill1NonContainerizedNumber.Commodities.Count);
			AssertEquals("commodity1", commodity1, masterBill1NonContainerizedNumber.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertEquals("commodity2", commodity2, masterBill1NonContainerizedNumber.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			package1.Delete();
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container1.BC_ParentID", ZGuid.Empty, masterBill1Container1.BC_ParentID);
			AssertEquals("masterBill1Container1.IsDeleted", false, masterBill1Container1.IsDeleted);
			AssertEquals("masterBill1Container1.Commodities.Count", 0, masterBill1Container1.Commodities.Count);
			AssertEquals("masterBill1NonContainerizedNumber matched", masterBill1NonContainerizedNumber, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber));
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 1, masterBill1NonContainerizedNumber.Commodities.Count);
			AssertEquals("commodity1.IsDeleted", true, commodity1.IsDeleted);
			AssertEquals("commodity2", commodity2, masterBill1NonContainerizedNumber.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			package2.Delete();
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill1MoveDetail.Containers.Count", 0, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container1.BC_ParentID", ZGuid.Empty, masterBill1Container1.BC_ParentID);
			AssertEquals("masterBill1Container1.IsDeleted", false, masterBill1Container1.IsDeleted);
			AssertEquals("masterBill1Container1.Commodities.Count", 0, masterBill1Container1.Commodities.Count);
			AssertEquals("masterBill1NonContainerizedNumber.BC_ParentID", ZGuid.Empty, masterBill1NonContainerizedNumber.BC_ParentID);
			AssertEquals("masterBill1NonContainerizedNumber.IsDeleted", false, masterBill1NonContainerizedNumber.IsDeleted);
			AssertEquals("masterBill1NonContainerizedNumber.Commodities.Count", 0, masterBill1NonContainerizedNumber.Commodities.Count);
			AssertEquals("commodity2.IsDeleted", true, commodity2.IsDeleted);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
			AssertEquals("masterBill1Container1.IsDeleted", true, masterBill1Container1.IsDeleted);
			AssertEquals("masterBill1NonContainerizedNumber.IsDeleted", true, masterBill1NonContainerizedNumber.IsDeleted);
		}
	}
}
