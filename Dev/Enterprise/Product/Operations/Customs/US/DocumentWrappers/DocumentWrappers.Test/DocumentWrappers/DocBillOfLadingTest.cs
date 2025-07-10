using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocBillOfLading))]
	sealed class DocBillOfLadingTest : DocBaseBillOfLadingTest
	{
		public void TestBillOfLading()
		{
			var declaration = Factory.New<JobDeclaration>();
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB1111111";
			masterBill.CU_NoOfPacks = 100m;
			masterBill.CU_PackType = "PK";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";
			var masterBillOfLading = new DocBillOfLading(masterBill);
			AssertEquals("Master:APLUMB1111111", masterBillOfLading.BillNumber);
			AssertEquals(100m, masterBillOfLading.ManifestQty);
			AssertEquals("PK", masterBillOfLading.UOM);

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HB11111111";
			houseBill.CU_NoOfPacks = 200m;
			houseBill.CU_PackType = "BG";
			houseBill.US_UI_NKBillIssuerSCAC = "SHCR";
			houseBill.CU_CU_ParentBill = ZGuid.Empty;
			var houseBillOfLading = new DocBillOfLading(houseBill);
			AssertEquals("House:SHCRHB11111111", houseBillOfLading.BillNumber);
			AssertEquals(200m, houseBillOfLading.ManifestQty);
			AssertEquals("BG", houseBillOfLading.UOM);

			houseBill.CU_CU_ParentBill = masterBill.PK;
			houseBillOfLading = new DocBillOfLading(houseBill);
			AssertEquals("Master:APLUMB1111111     House:SHCRHB11111111", houseBillOfLading.BillNumber);
			AssertEquals(200m, houseBillOfLading.ManifestQty);
			AssertEquals("BG", houseBillOfLading.UOM);

			houseBill.ITNumber = "171494737";
			AssertEquals("Master:APLUMB1111111     House:SHCRHB11111111     IT:171494737", houseBillOfLading.BillNumber);
		}

		protected override DocBaseBillOfLading CreateBillOfLading()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.CreatePrimaryBill(Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			return new DocBillOfLading(bill);
		}
	}
}
