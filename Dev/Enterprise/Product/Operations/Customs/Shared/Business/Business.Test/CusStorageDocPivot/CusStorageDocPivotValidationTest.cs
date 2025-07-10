using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class CusStorageDocPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSD_DocType_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(GetNewPivot().CSD_DocTypeInfo);
		}

		public void TestCheckCSD_DocType_CheckDuplicates()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "CIV");
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();

			Factory.Save();

			var pivot1 = GetNewPivot();
			pivot1.CSD_ParentID = entry1.PK;
			var pivot2 = GetNewPivot();
			pivot2.CSD_ParentID = entry2.PK;

			CombineAssertions(() =>
			{
				pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
				pivot1.CSD_DocType = "TY1";
				pivot2.CSD_StorageDocReference = eDoc1.UniqueKey;
				pivot2.CSD_DocType = "TY1";
				AssertNoError("If the reference and the type are the same but the entry is not the same", pivot2.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

				var pivot3 = GetNewPivot();
				pivot3.CSD_ParentID = entry1.PK;
				pivot3.CSD_StorageDocReference = eDoc1.UniqueKey;
				pivot3.CSD_DocType = "TY1";
				AssertHasError("If the same eDoc (reference and type) is added twice to one entry", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

				pivot3.CSD_DocType = "TY2";
				AssertNoError("If the type is not the same", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

				pivot3.CSD_StorageDocReference = eDoc2.UniqueKey;
				pivot3.CSD_DocType = "TY1";
				AssertNoError("If the reference is not the same", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);
			});
		}

		public void TestCheckCSD_DocType_CheckDuplicates_WithParentTableCode()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "CIV");
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();

			Factory.Save();

			var pivot1 = GetNewPivot();
			pivot1.CSD_ParentID = entry1.PK;
			pivot1.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot1.CSD_DocType = "TY1";
			pivot1.CSD_ParentTableCode = "JI";

			var pivot2 = GetNewPivot();
			pivot2.CSD_ParentID = entry1.PK;
			pivot2.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot2.CSD_ParentTableCode = "JI";
			pivot2.CSD_DocType = "TY1";
			AssertHasError("ParentID, DocType, CSD_StorageDocReference and ParentTableCode are all the same, then there should be error", pivot2.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

			var pivot3 = GetNewPivot();
			pivot3.CSD_ParentID = entry1.PK;
			pivot3.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot3.CSD_ParentTableCode = "JZ";
			pivot3.CSD_DocType = "TY1";
			AssertNoError("ParentID, DocType, CSD_StorageDocReference are same but ParentTableCode is not, then there should be no error", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

			var pivot4 = GetNewPivot();
			pivot4.CSD_ParentID = entry1.PK;
			pivot4.CSD_StorageDocReference = eDoc2.UniqueKey;
			pivot4.CSD_ParentTableCode = "JI";
			pivot4.CSD_DocType = "TY1";
			AssertNoError("ParentTableCode, DocType, ParentID are same but CSD_StorageDocReference is not, then there should be no error", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

			var pivot5 = GetNewPivot();
			pivot5.CSD_ParentID = entry1.PK;
			pivot5.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot5.CSD_ParentTableCode = "JI";
			pivot5.CSD_DocType = "TY2";
			AssertNoError("ParentTableCode, ParentID, CSD_StorageDocReference are same but DocType is not, then there should be no error", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);

			var pivot6 = GetNewPivot();
			pivot6.CSD_ParentID = entry2.PK;
			pivot6.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot6.CSD_ParentTableCode = "JI";
			pivot6.CSD_DocType = "TY1";
			AssertNoError("CSD_StorageDocReference, DocType, ParentTableCode are same but ParentID is not, then there should be no error", pivot3.CSD_DocTypeInfo, DocTypeStorageDuplicatingMessage);
		}

		protected virtual BaseCusStorageDocPivot GetNewPivot() => Factory.New<BaseCusStorageDocPivot>();

		protected virtual string DocTypeStorageDuplicatingMessage => "The combination of eDoc, Document Type should be unique.";
	}
}
