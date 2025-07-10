using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business.Test
{
	sealed class CusStorageDocPivotHAWBValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSD_StorageDocReference()
		{
			const string message = "Cannot add this document. The same document is found in the Header or in another House Bill.";
			var airCargo = Factory.New<CusMAWB>();
			var houseBill1 = airCargo.ChildBills.AddNew();
			var houseBill2 = airCargo.ChildBills.AddNew();
			var eDoc1 = airCargo.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "CIV");
			var eDoc2 = airCargo.DocManagerInfo.AddFileOrDocument(new byte[1], "Test2.pdf", "CIV");
			var eDoc3 = airCargo.DocManagerInfo.AddFileOrDocument(new byte[1], "Test3.pdf", "CIV");
			var eDoc4 = airCargo.DocManagerInfo.AddFileOrDocument(new byte[1], "Test4.pdf", "CIV");

			var doc1 = airCargo.EDocPivotCollection.AddNew();
			doc1.CSD_StorageDocReference = eDoc1.UniqueKey;
			var doc2 = houseBill1.EDocPivotCollection.AddNew();
			doc2.CSD_StorageDocReference = eDoc2.UniqueKey;
			var doc3 = houseBill2.EDocPivotCollection.AddNew();
			doc3.CSD_StorageDocReference = eDoc3.UniqueKey;

			var doc4 = houseBill1.EDocPivotCollection.AddNew();
			CombineAssertions(() =>
			{
				doc4.CSD_StorageDocReference = eDoc4.UniqueKey;
				AssertNoError("No duplicate", doc4.CSD_StorageDocReferenceInfo, message);

				doc4.CSD_StorageDocReference = eDoc1.UniqueKey;
				AssertHasError("The same document already added to header", doc4.CSD_StorageDocReferenceInfo, message);

				doc4.CSD_StorageDocReference = eDoc2.UniqueKey;
				AssertHasError("The same document already added to house bill #1", doc4.CSD_StorageDocReferenceInfo, message);

				doc4.CSD_StorageDocReference = eDoc3.UniqueKey;
				AssertHasError("The same document already added to house bill #2", doc4.CSD_StorageDocReferenceInfo, message);
			});
		}
	}
}
