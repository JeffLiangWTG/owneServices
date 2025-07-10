using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocBillOfLadingCollection))]
	sealed class DocBillOfLadingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocBillOfLadingCollection>
	{
		public void TestBillOfLadingForDocumentCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "MLB1111111";
			declaration.JE_HouseBillIssuerSCAC = "SCHR";
			declaration.JE_HouseBill = "HSB11111111";
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalNoOfPacksPackType = "PK";

			var masterBill1 = declaration.Bills.CreatePrimaryBill(Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			masterBill1.CU_BillNum = "MLB222222222";
			masterBill1.US_UI_NKBillIssuerSCAC = "APLU";
			masterBill1.CU_NoOfPacks = 120m;
			masterBill1.CU_PackType = "BG";

			var houseBill1 = declaration.Bills.CreatePrimaryBill(Enterprise.Customs.Business.BillTypeList.Codes.HouseBill);
			houseBill1.CU_BillNum = "HSB22222222";
			houseBill1.US_UI_NKBillIssuerSCAC = "SHCR";
			houseBill1.CU_NoOfPacks = 200m;
			houseBill1.CU_PackType = "AE";

			var proofOfReleaseCollection = new DocBillOfLadingCollection(declaration);
			proofOfReleaseCollection.LoadBillOfLadings();
			AssertEquals(3, proofOfReleaseCollection.Count);
			var billOfLading0 = proofOfReleaseCollection[0];
			AssertEquals("Master:APLUMLB1111111     House:SCHRHSB11111111", billOfLading0.BillNumber);
			AssertEquals(100m, billOfLading0.ManifestQty);
			AssertEquals("PK", billOfLading0.UOM);
			var billOfLading1 = proofOfReleaseCollection[1];
			AssertEquals("Master:APLUMLB222222222", billOfLading1.BillNumber);
			AssertEquals(120m, billOfLading1.ManifestQty);
			AssertEquals("BG", billOfLading1.UOM);
			var billOfLading2 = proofOfReleaseCollection[2];
			AssertEquals("House:SHCRHSB22222222", billOfLading2.BillNumber);
			AssertEquals(200m, billOfLading2.ManifestQty);
			AssertEquals("AE", billOfLading2.UOM);
		}

		protected override DocBillOfLadingCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new DocBillOfLadingCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.CreatePrimaryBill(Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			return new DocBillOfLading(bill);
		}
	}
}
