using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(NonPersistentBusinessObject))]
	public class SupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentType()
		{
			SupportingDocument.DocumentType = "XXX";
			AssertEquals(true, SupportingDocument.DocumentTypeInfo.HasErrors());
			SupportingDocument.DocumentType = SupportingDocumentTypeCodeList.Codes.DocType004;
			AssertEquals(false, SupportingDocument.DocumentTypeInfo.HasErrors());
			SupportingDocument.DocumentType = "";
			AssertEquals(true, SupportingDocument.DocumentTypeInfo.HasErrors());
		}

		public void TestDocumentTypes()
		{
			AssertEquals(43, SupportingDocument.DocumentTypes.Count);
		}

		public void TesteDoc()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE", "DESCRIPTION");
			SupportingDocument.eDoc = ZGuid.Invalid;
			AssertEquals(true, SupportingDocument.eDocInfo.HasErrors());
			SupportingDocument.eDoc = ZGuid.Empty;
			AssertEquals(true, SupportingDocument.eDocInfo.HasErrors());
			SupportingDocument.eDoc = pK1;
			AssertEquals(false, SupportingDocument.eDocInfo.HasErrors());
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE", "DESCRIPTION");
			SupportingDocument duplicateDocument = SupportingDocuments.AddNew();
			duplicateDocument.eDoc = pK1;
			AssertEquals(true, duplicateDocument.eDocInfo.HasErrors());
			ZGuid pK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK2, "CODE", "DESCRIPTION");
			duplicateDocument.eDoc = pK2;
			AssertEquals(false, duplicateDocument.eDocInfo.HasErrors());
		}

		public void TesteDocFileNameLength()
		{
			var docPK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(docPK1, "File name", "DESCRIPTION");
			SupportingDocument.eDoc = docPK1;
			AssertEquals(false, SupportingDocument.eDocInfo.HasErrors());
			var docPK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(docPK2, "BOE - ORIGINAL 1 - (FOR ISSUING CARRIER) - HouseBill NO_ SSIN3584206.PDF", "Date Added: 2020/10/28");
			SupportingDocument.eDoc = docPK2;
			AssertEquals("File name is too long for TradeNet", true, SupportingDocument.eDocInfo.HasErrors());
			SupportingDocument.eDocInfo.ClearAllNotifications();
			var validDoc = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(validDoc, "CAD - TradeNet acceptable length file name of 64 characters allowd.pdf", "Date Added: 2020/10/28");
			SupportingDocument.eDoc = validDoc;
			AssertEquals("File name is just right for TradeNet", false, SupportingDocument.eDocInfo.HasErrors());
		}

		public void TestValidFileName()
		{
			var docPK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(docPK1, "CI PL – PO 1693715 – SGN+MY – FINAL.PDF", "DESCRIPTION");
			SupportingDocument.eDoc = docPK1;
			AssertEquals(false, SupportingDocument.eDocInfo.HasErrors());
		}

		public void TestStorageDocs()
		{
			AssertNotNull(SupportingDocuments.StorageDocs);
		}

		public void TestPreSaveValidation()
		{
			SupportingDocument.RunPreSaveValidation();
			AssertEquals(true, SupportingDocument.DocumentTypeInfo.HasErrors());
			AssertEquals(true, SupportingDocument.eDocInfo.HasErrors());
		}

		#region ICusAttachment
		public void TestUniqueIdentifier_()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE", "DESCRIPTION");
			SupportingDocument.eDoc = pK1;
			AssertEquals(pK1, CusAttatchment.UniqueIdentifier);
		}

		public void TestFileName_()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE.tif", "DESCRIPTION");
			SupportingDocument.eDoc = pK1;
			AssertEquals("CODE.pdf", CusAttatchment.FileName);
			ZGuid pK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK2, "CODE.doc", "DESCRIPTION");
			SupportingDocument.eDoc = pK2;
			AssertEquals("CODE.doc", CusAttatchment.FileName);
		}

		public void TestDocType_()
		{
			SupportingDocument.DocumentType = SupportingDocumentTypeCodeList.Codes.DocType004;
			AssertEquals(SupportingDocumentTypeCodeList.Codes.DocType004, CusAttatchment.DocType);
		}

		#endregion
		#region Implementation
		SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				return supportingDocuments ?? (supportingDocuments = new SupportingDocumentCollection(Factory));
			}
		}

		SupportingDocumentCollection supportingDocuments;
		ICusAttachment CusAttatchment
		{
			get
			{
				return SupportingDocument;
			}
		}

		SupportingDocument SupportingDocument
		{
			get
			{
				return supportingDocument ?? (supportingDocument = SupportingDocuments.AddNew());
			}
		}

		SupportingDocument supportingDocument;
		#endregion
		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return SupportingDocument;
		}
		#endregion
	}
}
