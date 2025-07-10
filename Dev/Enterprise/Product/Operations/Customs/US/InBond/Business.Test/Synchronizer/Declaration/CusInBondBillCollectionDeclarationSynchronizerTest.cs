using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchroniseHouseBillsInDifferentTransportMode()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_CU_ParentBill = masterBill1.PK;
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_CU_ParentBill = masterBill1.PK;
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;

			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals(2, header.Bills.Count);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals(3, header.Bills.Count);
		}

		public void TestReSynchronizationOfBills()
		{
			synchronizer.SetEnabled(false, false);

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "MB1";
			bill1.US_UI_NKBillIssuerSCAC = "ABCD";
			bill1.CU_PackType = "T1";
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "MB2";
			bill2.US_UI_NKBillIssuerSCAC = "ABCD";
			bill2.CU_PackType = "T2";
			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_BillNum = "MB2";
			bill3.US_UI_NKBillIssuerSCAC = "ABCD";
			bill3.CU_PackType = "T3";
			var bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.MasterBill;
			bill4.CU_BillNum = "MB1";
			bill4.US_UI_NKBillIssuerSCAC = "DGGF";
			bill4.CU_PackType = "T4";

			var inBondBill1 = header.Bills.AddNew("DGGF", "MB4");
			var inBondBill2 = header.Bills.AddNew("ABCD", "MB2");
			var inBondBill3 = header.Bills.AddNew("ABCD", "MB2");
			var inBondBill4 = header.Bills.AddNew("ABCD", "MB1");

			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals("header.Bills.Count", 4, header.Bills.Count);
			AssertEquals("inBondBill1.IsDeleted", true, inBondBill1.IsDeleted);
			inBondBill1 = header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "DGGF" && x.B0_MasterBillNumber == "MB1" && x.B0_ManifestUQ == "T4");
			AssertNotNull("inBondBill1", inBondBill1);
			AssertEquals("header.Bills.Contains(inBondBill2)", true, header.Bills.Contains(inBondBill2));
			AssertEquals("header.Bills.Contains(inBondBill3)", true, header.Bills.Contains(inBondBill3));
			if (inBondBill3.B0_ManifestUQ == "T2")
			{
				var tempBill = inBondBill3;
				inBondBill3 = inBondBill2;
				inBondBill2 = tempBill;
			}
			AssertEquals("inBondBill2.B0_ManifestUQ", "T2", inBondBill2.B0_ManifestUQ);
			AssertEquals("inBondBill3.B0_ManifestUQ", "T3", inBondBill3.B0_ManifestUQ);
			AssertEquals("inBondBill4 matched", inBondBill4, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1" && x.B0_ManifestUQ == "T1"));

			bill3.CU_PackType = "TB";
			AssertEquals("inBondBill3.B0_ManifestUQ", "TB", inBondBill3.B0_ManifestUQ);
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());

			synchronizer.SetEnabled(false, false);
			bill4.Delete();
			inBondBill3.Delete();
			bill1.CU_PackType = "TA";
			synchronizer.SetEnabled(true, false);
			synchronizer.Synchronise(true);
			AssertEquals("header.Bills.Count", 3, header.Bills.Count);
			AssertEquals("inBondBill1.IsDeleted", true, inBondBill1.IsDeleted);
			AssertEquals("inBondBill2.IsDeleted", false, inBondBill2.IsDeleted);
			AssertEquals("inBondBill2 matched", inBondBill2, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB2" && x.B0_ManifestUQ == "T2"));
			AssertEquals("inBondBill3.IsDeleted", true, inBondBill3.IsDeleted);
			inBondBill3 = header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB2" && x.B0_ManifestUQ == "TB");
			AssertNotNull("inBondBill3", inBondBill3);
			AssertEquals("inBondBill4.IsDeleted", false, inBondBill4.IsDeleted);
			AssertEquals("inBondBill4 matched", inBondBill4, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1" && x.B0_ManifestUQ == "TA"));
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}

		public void TestSynchronizeBills()
		{
			AssertEquals("header.Bills.Count", 0, header.Bills.Count);

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "MB1";
			bill1.US_UI_NKBillIssuerSCAC = "ABCD";
			AssertEquals("header.Bills.Count", 1, header.Bills.Count);
			var inBondBill1 = header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1");
			AssertNotNull("inBondBill1", inBondBill1);
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "MB2";
			bill2.US_UI_NKBillIssuerSCAC = "ABCD";
			AssertEquals("header.Bills.Count", 2, header.Bills.Count);
			AssertEquals("inBondBill1 matched", inBondBill1, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1"));
			var inBondBill2 = header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB2");
			AssertNotNull("inBondBill2", inBondBill2);

			bill2.US_UI_NKBillIssuerSCAC = "GHKD";
			AssertEquals("header.Bills.Count", 2, header.Bills.Count);
			AssertEquals("inBondBill1 matched", inBondBill1, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1"));
			AssertEquals("inBondBill2 matched", inBondBill2, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "GHKD" && x.B0_MasterBillNumber == "MB2"));

			bill2.CU_BillNum = "MB2A";
			AssertEquals("header.Bills.Count", 2, header.Bills.Count);
			AssertEquals("inBondBill1 matched", inBondBill1, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1"));
			AssertEquals("inBondBill2 matched", inBondBill2, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "GHKD" && x.B0_MasterBillNumber == "MB2A"));

			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("header.Bills.Count", 1, header.Bills.Count);
			AssertEquals("inBondBill1.IsDeleted", true, inBondBill1.IsDeleted);
			AssertEquals("inBondBill2 matched", inBondBill2, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "GHKD" && x.B0_MasterBillNumber == "MB2A"));

			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("header.Bills.Count", 2, header.Bills.Count);
			inBondBill1 = header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "ABCD" && x.B0_MasterBillNumber == "MB1");
			AssertNotNull("inBondBill1", inBondBill1);
			AssertEquals("inBondBill2 matched", inBondBill2, header.Bills.FirstOrDefault(x => x.B0_IssuerCode == "GHKD" && x.B0_MasterBillNumber == "MB2A"));
			AssertNoExceptionThrown("Schronisation data able to save without any issue", () => Factory.Save());
		}
	}
}
