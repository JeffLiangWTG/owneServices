using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestReSynchronizationOfCommodities()
		{
			synchronizer.SetEnabled(false, false);

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB1";
			bill.US_UI_NKBillIssuerSCAC = "ABCD";

			var package1 = declaration.Packages.AddNew();
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			package1.CW_MarksAndNos = "MARKS 1";
			package1.CW_PackType = "T1";
			var package2 = declaration.Packages.AddNew();
			package2.CW_HouseBill = bill.CU_BillUniqueCode;
			package2.CW_MarksAndNos = "MARKS 2";
			package2.CW_PackType = "T2";
			var package3 = declaration.Packages.AddNew();
			package3.CW_HouseBill = bill.CU_BillUniqueCode;
			package3.CW_MarksAndNos = "MARKS 3";
			package3.CW_PackType = "T2";
			var package4 = declaration.Packages.AddNew();
			package4.CW_HouseBill = bill.CU_BillUniqueCode;
			package4.CW_MarksAndNos = "MARKS 4";
			package4.CW_PackType = "T4";

			var inBondBill = header.Bills.AddNew();
			inBondBill.B0_MasterBillNumber = "MB1";
			inBondBill.B0_IssuerCode = "ABCD";

			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew(inBondBill.PK);
			var inBondContainer = moveDetail.Containers.AddNew();
			inBondContainer.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;

			var commodity1 = inBondContainer.Commodities.AddNew();
			commodity1.BY_ManifestUnitCode = "T5";
			var commodity2 = inBondContainer.Commodities.AddNew();
			commodity2.BY_ManifestUnitCode = "T2";
			var commodity3 = inBondContainer.Commodities.AddNew();
			commodity3.BY_ManifestUnitCode = "T2";
			var commodity4 = inBondContainer.Commodities.AddNew();
			commodity4.BY_ManifestUnitCode = "T1";

			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			AssertEquals("moveDetail.Containers[0]", inBondContainer, moveDetail.Containers[0]);
			AssertEquals("inBondContainer.Commodities.Count", 4, inBondContainer.Commodities.Count);
			AssertEquals("commodity1.IsDeleted", true, commodity1.IsDeleted);
			commodity1 = inBondContainer.Commodities.FirstOrDefault(x => x.BY_ManifestUnitCode == "T4" && x.BY_MarksAndNumbers == "MARKS 4");
			AssertNotNull("commodity1", commodity1);
			AssertEquals("commodity2.IsDeleted", false, commodity2.IsDeleted);
			AssertEquals("inBondContainer.Commodities.Contains(commodity2)", true, inBondContainer.Commodities.Contains(commodity2));
			AssertEquals("commodity3.IsDeleted", false, commodity3.IsDeleted);
			AssertEquals("inBondContainer.Commodities.Contains(commodity3)", true, inBondContainer.Commodities.Contains(commodity3));
			if (commodity3.BY_MarksAndNumbers == "MARKS 2")
			{
				var tempCommodity = commodity3;
				commodity3 = commodity2;
				commodity2 = tempCommodity;
			}
			AssertEquals("commodity2.BY_MarksAndNumbers", "MARKS 2", commodity2.BY_MarksAndNumbers);
			AssertEquals("commodity3.BY_MarksAndNumbers", "MARKS 3", commodity3.BY_MarksAndNumbers);
			AssertEquals("commodity4.IsDeleted", false, commodity4.IsDeleted);
			AssertEquals("commodity4 matched", commodity4, inBondContainer.Commodities.FirstOrDefault(x => x.BY_ManifestUnitCode == "T1" && x.BY_MarksAndNumbers == "MARKS 1"));

			package3.CW_MarksAndNos = "MARKS 3 A";
			AssertEquals("commodity3.BY_MarksAndNumbers", "MARKS 3 A", commodity3.BY_MarksAndNumbers);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());

			synchronizer.SetEnabled(false, false);
			package4.Delete();
			commodity3.Delete();
			package1.CW_PackType = "TA";
			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals("moveHeader.MovementDetails.Count", 1, moveHeader.MovementDetails.Count);
			AssertEquals("moveHeader.MovementDetails[0]", moveDetail, moveHeader.MovementDetails[0]);
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			AssertEquals("moveDetail.Containers[0]", inBondContainer, moveDetail.Containers[0]);
			AssertEquals("inBondContainer.Commodities.Count", 3, inBondContainer.Commodities.Count);
			AssertEquals("commodity1.IsDeleted", true, commodity1.IsDeleted);
			AssertEquals("commodity2.IsDeleted", false, commodity2.IsDeleted);
			AssertEquals("commodity2 matched", commodity2, inBondContainer.Commodities.FirstOrDefault(x => x.BY_ManifestUnitCode == "T2" && x.BY_MarksAndNumbers == "MARKS 2"));
			AssertEquals("commodity3.IsDeleted", true, commodity3.IsDeleted);
			commodity3 = inBondContainer.Commodities.FirstOrDefault(x => x.BY_ManifestUnitCode == "T2" && x.BY_MarksAndNumbers == "MARKS 3 A");
			AssertNotNull("commodity3", commodity3);
			AssertEquals("commodity4.IsDeleted", false, commodity4.IsDeleted);
			AssertEquals("commodity4 matched", commodity4, inBondContainer.Commodities.FirstOrDefault(x => x.BY_ManifestUnitCode == "TA" && x.BY_MarksAndNumbers == "MARKS 1"));
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}

		public void TestSynchronizeCommodities()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT12321";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "KDHD32323";
			var moveHeader = header.MovementHeader;
			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "MB1";
			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MB2";

			var package1 = declaration.Packages.AddNew();
			package1.CW_MarksAndNos = "MARKS 1";
			package1.CW_HouseBill = masterBill1.CU_BillUniqueCode;
			package1.CW_ContainerNoOrEquipmentNo = ZString.Empty;
			var package2 = declaration.Packages.AddNew();
			package2.CW_MarksAndNos = "MARKS 2";
			package2.CW_HouseBill = masterBill2.CU_BillUniqueCode;
			package2.CW_ContainerNoOrEquipmentNo = ZString.Empty;

			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			var masterBill1MoveDetail = moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1");
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			var moveDetail1NonContainerized1 = masterBill1MoveDetail.Containers[0];
			AssertEquals("moveDetail1NonContainerized1.BC_ContainerNum", CusInBondContainer.NonContainerizedNumber, moveDetail1NonContainerized1.BC_ContainerNum);
			AssertEquals("moveDetail1NonContainerized1.Commodities.Count", 1, moveDetail1NonContainerized1.Commodities.Count);
			var commodity1 = moveDetail1NonContainerized1.Commodities[0];
			AssertEquals("commodity1.BY_MarksAndNumbers", "MARKS 1", commodity1.BY_MarksAndNumbers);

			var masterBill2MoveDetail = moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2");
			AssertEquals("masterBill2MoveDetail.Containers.Count", 1, masterBill2MoveDetail.Containers.Count);
			var moveDetail2NonContainerized = masterBill2MoveDetail.Containers[0];
			AssertEquals("moveDetail2NonContainerized.BC_ContainerNum", CusInBondContainer.NonContainerizedNumber, moveDetail2NonContainerized.BC_ContainerNum);
			AssertEquals("moveDetail2NonContainerized.Commodities.Count", 1, moveDetail2NonContainerized.Commodities.Count);
			var commodity2 = moveDetail2NonContainerized.Commodities[0];
			AssertEquals("commodity2.BY_MarksAndNumbers", "MARKS 2", commodity2.BY_MarksAndNumbers);

			package2.CW_HouseBill = masterBill1.CU_BillUniqueCode;
			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			AssertEquals("masterBill1MoveDetail", masterBill1MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1"));
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			AssertEquals("moveDetail1NonContainerized1 matched", moveDetail1NonContainerized1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber));
			AssertEquals("moveDetail1NonContainerized1.Commodities.Count", 2, moveDetail1NonContainerized1.Commodities.Count);
			AssertEquals("commodity1", commodity1, moveDetail1NonContainerized1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));
			AssertEquals("commodity2", commodity2, moveDetail1NonContainerized1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			AssertEquals("masterBill2MoveDetail", masterBill2MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2"));
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);
			AssertEquals("moveDetail2NonContainerized.IsDeleted", false, moveDetail2NonContainerized.IsDeleted);
			AssertEquals("moveDetail2NonContainerized.BC_ParentID", ZGuid.Empty, moveDetail2NonContainerized.BC_ParentID);
			AssertEquals("moveDetail2NonContainerized.Commodities.Count", 0, moveDetail2NonContainerized.Commodities.Count);

			package1.CW_ContainerNoOrEquipmentNo = "CONT12321";
			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			AssertEquals("masterBill1MoveDetail", masterBill1MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1"));
			AssertEquals("masterBill1MoveDetail.Containers.Count", 2, masterBill1MoveDetail.Containers.Count);
			AssertEquals("moveDetail1NonContainerized1", moveDetail1NonContainerized1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == CusInBondContainer.NonContainerizedNumber));
			AssertEquals("moveDetail1NonContainerized1.Commodities.Count", 1, moveDetail1NonContainerized1.Commodities.Count);
			AssertEquals("commodity2", commodity2, moveDetail1NonContainerized1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));
			var masterBill1Container1 = masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT12321");
			AssertNotNull("masterBill1Container1", masterBill1Container1);
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			AssertEquals("masterBill2MoveDetail", masterBill2MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2"));
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);

			package2.CW_ContainerNoOrEquipmentNo = "KDHD32323";
			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			AssertEquals("masterBill1MoveDetail", masterBill1MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1"));
			AssertEquals("masterBill1MoveDetail.Containers.Count", 2, masterBill1MoveDetail.Containers.Count);
			AssertEquals("moveDetail1NonContainerized1.IsDeleted", false, moveDetail1NonContainerized1.IsDeleted);
			AssertEquals("moveDetail1NonContainerized1.BC_ParentID", ZGuid.Empty, moveDetail1NonContainerized1.BC_ParentID);
			AssertEquals("moveDetail1NonContainerized1.Commodities.Count", 0, moveDetail1NonContainerized1.Commodities.Count);
			var masterBill1Container2 = masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "KDHD32323");
			AssertNotNull("masterBill1Container2", masterBill1Container2);
			AssertEquals("masterBill1Container2.Commodities.Count", 1, masterBill1Container2.Commodities.Count);
			AssertEquals("commodity2", commodity2, masterBill1Container2.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			AssertEquals("masterBill1Container1 matched", masterBill1Container1, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT12321"));
			AssertEquals("masterBill1Container1.Commodities.Count", 1, masterBill1Container1.Commodities.Count);
			AssertEquals("commodity1", commodity1, masterBill1Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			AssertEquals("masterBill2MoveDetail", masterBill2MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2"));
			AssertEquals("masterBill2MoveDetail.Containers.Count", 0, masterBill2MoveDetail.Containers.Count);

			package1.CW_HouseBill = masterBill2.CU_BillUniqueCode;
			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			AssertEquals("masterBill1MoveDetail", masterBill1MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1"));
			AssertEquals("masterBill1MoveDetail.Containers.Count", 1, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container2 matched", masterBill1Container2, masterBill1MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "KDHD32323"));
			AssertEquals("masterBill1Container2.Commodities.Count", 1, masterBill1Container2.Commodities.Count);
			AssertEquals("commodity2 matched", commodity2, masterBill1Container2.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 2"));

			AssertEquals("masterBill1Container1.IsDeleted", false, masterBill1Container1.IsDeleted);
			AssertEquals("masterBill1Container1.BC_ParentID", ZGuid.Empty, masterBill1Container1.BC_ParentID);
			AssertEquals("masterBill1Container1.Commodities.Count", 0, masterBill1Container1.Commodities.Count);

			AssertEquals("masterBill2MoveDetail matched", masterBill2MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2"));
			AssertEquals("masterBill2MoveDetail.Containers.Count", 1, masterBill2MoveDetail.Containers.Count);
			var masterBill2Container1 = masterBill2MoveDetail.Containers[0];
			AssertEquals("masterBill2Container1.BC_ContainerNum", "CONT12321", masterBill2Container1.BC_ContainerNum);
			AssertEquals("commodity1", commodity1, masterBill2Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			AssertEquals("moveDetailNonContainerized1.IsDeleted", false, moveDetail1NonContainerized1.IsDeleted);
			AssertEquals("moveDetailNonContainerized2.IsDeleted", false, moveDetail2NonContainerized.IsDeleted);

			package2.Delete();
			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			AssertEquals("masterBill1MoveDetail", masterBill1MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1"));
			AssertEquals("masterBill1MoveDetail.Containers.Count", 0, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container2.IsDeleted", false, masterBill1Container2.IsDeleted);
			AssertEquals("masterBill1Container2.BC_ParentID", ZGuid.Empty, masterBill1Container2.BC_ParentID);
			AssertEquals("masterBill1Container2.Commodities.Count", 0, masterBill1Container2.Commodities.Count);
			AssertEquals("commodity2.IsDeleted", true, commodity2.IsDeleted);

			AssertEquals("masterBill1Container1.IsDeleted", false, masterBill1Container1.IsDeleted);
			AssertEquals("masterBill1Container1.BC_ParentID", ZGuid.Empty, masterBill1Container1.BC_ParentID);
			AssertEquals("masterBill1Container1.Commodities.Count", 0, masterBill1Container1.Commodities.Count);

			AssertEquals("masterBill2MoveDetail matched", masterBill2MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2"));
			AssertEquals("masterBill2MoveDetail.Containers.Count", 1, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill2Container1 matched", masterBill2Container1, masterBill2MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT12321"));
			AssertEquals("commodity1", commodity1, masterBill2Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			AssertEquals("moveDetailNonContainerized1.IsDeleted", false, moveDetail1NonContainerized1.IsDeleted);
			AssertEquals("moveDetailNonContainerized2.IsDeleted", false, moveDetail2NonContainerized.IsDeleted);

			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
			AssertEquals("moveHeader.MovementDetails.Count", 2, moveHeader.MovementDetails.Count);
			AssertEquals("masterBill1MoveDetail", masterBill1MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB1"));
			AssertEquals("masterBill1MoveDetail.Containers.Count", 0, masterBill1MoveDetail.Containers.Count);
			AssertEquals("masterBill1Container2.IsDeleted", true, masterBill1Container2.IsDeleted);
			AssertEquals("masterBill1Container1.IsDeleted", true, masterBill1Container1.IsDeleted);

			AssertEquals("masterBill2MoveDetail matched", masterBill2MoveDetail, moveHeader.MovementDetails.FirstOrDefault(x => x.MasterBillNumber == "MB2"));
			AssertEquals("masterBill2MoveDetail.Containers.Count", 1, masterBill2MoveDetail.Containers.Count);
			AssertEquals("masterBill2Container1 matched", masterBill2Container1, masterBill2MoveDetail.Containers.FirstOrDefault(x => x.BC_ContainerNum == "CONT12321"));
			AssertEquals("commodity1", commodity1, masterBill2Container1.Commodities.FirstOrDefault(x => x.BY_MarksAndNumbers == "MARKS 1"));

			AssertEquals("moveDetailNonContainerized1.IsDeleted", true, moveDetail1NonContainerized1.IsDeleted);
			AssertEquals("moveDetailNonContainerized2.IsDeleted", true, moveDetail2NonContainerized.IsDeleted);
		}
	}
}
