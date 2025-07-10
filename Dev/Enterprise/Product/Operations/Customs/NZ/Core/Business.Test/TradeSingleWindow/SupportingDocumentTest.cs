using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	[TestedType(typeof(NonPersistentBusinessObject))]
	public class SupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentType()
		{
			SupportingDocument.AttachmentType = "XXX";
			AssertEquals(true, SupportingDocument.AttachmentTypeInfo.HasErrors());

			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.BOL;
			AssertEquals(false, SupportingDocument.AttachmentTypeInfo.HasErrors());

			SupportingDocument.AttachmentType = "";
			AssertEquals(true, SupportingDocument.AttachmentTypeInfo.HasErrors());
		}

		public void TestInvalidCharactersPastedIntoDocType()
		{
			SupportingDocument.AttachmentType = "XXX";
			AssertEquals("Invalid code", true, SupportingDocument.AttachmentTypeInfo.HasErrors());

			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.PP;
			AssertEquals("Valid code", false, SupportingDocument.AttachmentTypeInfo.HasErrors());

			SupportingDocument.AttachmentType = "\tPP";
			AssertEquals("Unprintable characters before the code", true, SupportingDocument.AttachmentTypeInfo.HasErrors());

			SupportingDocument.AttachmentType = " PP";
			AssertEquals("Unprintable characters before the code", true, SupportingDocument.AttachmentTypeInfo.HasErrors());

			SupportingDocument.AttachmentType = "PP\t";
			AssertEquals("Unprintable characters after the code are removed", "PP", SupportingDocument.AttachmentType);
			AssertEquals(false, SupportingDocument.AttachmentTypeInfo.HasErrors());
		}

		public void TestDocumentAttachmentTypes()
		{
			AssertEquals("Full Attachment Types code list elements provided by NZ Customs.", 21, SupportingDocument.AttachmentTypes.Count);

			var ex1SuppDocs = new SupportingDocumentCollection(Factory, MessageTypeList.Codes.E40);
			var ex1Doc = ex1SuppDocs.AddNew();
			AssertEquals("EX1 Attachment Types code list elements is a subset of full list.", 6, ex1Doc.AttachmentTypes.Count);
			AssertEquals("EX1 attachment list should have this code", true, ex1Doc.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.CER));
			AssertEquals("EX1 attachment list should not have this code", false, ex1Doc.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.SST));

			var im1SuppDocs = new SupportingDocumentCollection(Factory, MessageTypeList.Codes.I11);
			var im1Doc = im1SuppDocs.AddNew();
			AssertEquals("IM1 Attachment Types code list elements is a subset of full list.", 9, im1Doc.AttachmentTypes.Count);
			AssertEquals("IM1 attachment list should have this code", true, im1Doc.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.CQD));
			AssertEquals("IM1 attachment list should not have this code", false, im1Doc.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.PAX));

			var creSuppDocs = new SupportingDocumentCollection(Factory, MessageTypeList.Codes.CRE);
			var creDoc = creSuppDocs.AddNew();
			AssertEquals("CRE Attachment Types code list should only have 1 element.", 1, creDoc.AttachmentTypes.Count);
			AssertEquals("CRE attachment list should only have this code", true, creDoc.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.OTH));

			var ocrSuppDocs = new SupportingDocumentCollection(Factory, MessageTypeList.Codes.OCR);
			var ocrDoc = ocrSuppDocs.AddNew();
			AssertEquals("OCR Attachment Types code list should only have 1 element.", 1, ocrDoc.AttachmentTypes.Count);
			AssertEquals("OCR attachment list should only have this code", true, ocrDoc.AttachmentTypes.ContainsCode(AttachmentTypeList.Codes.OTH));
		}

		public void TesteDoc()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE", "DESCRIPTION");

			SupportingDocument.eDoc = ZGuid.Invalid;
			AssertEquals(true, SupportingDocument.eDocInfo.HasErrors());//invalid

			SupportingDocument.eDoc = ZGuid.Empty;
			AssertEquals(true, SupportingDocument.eDocInfo.HasErrors());//empty

			SupportingDocument.eDoc = pK1;
			AssertEquals(false, SupportingDocument.eDocInfo.HasErrors());

			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE", "DESCRIPTION");//duplicate
			SupportingDocument duplicateDocument = SupportingDocuments.AddNew();
			duplicateDocument.eDoc = pK1;
			AssertEquals(true, duplicateDocument.eDocInfo.HasErrors());

			ZGuid pK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK2, "CODE.doc", "DESCRIPTION");
			duplicateDocument.eDoc = pK2;
			AssertEquals(false, duplicateDocument.eDocInfo.HasErrors());

			var pK3 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK3, "你好.docx", "DESCRIPTION1");
			var pK4 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK4, "中国.docx", "DESCRIPTION2");
			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.OTH;
			SupportingDocument.eDoc = pK3;
			var anotherDocument = SupportingDocuments.AddNew();
			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.OTH;
			anotherDocument.eDoc = pK4;
			AssertHasErrorContaining(anotherDocument.eDocInfo, "Filename contains Invalid character(s).");
		}

		public void TestStorageDocs()
		{
			AssertNotNull(SupportingDocuments.StorageDocs);
		}

		public void TestPreSaveValidation()
		{
			SupportingDocument.RunPreSaveValidation();
			AssertEquals(true, SupportingDocument.AttachmentTypeInfo.HasErrors());
			AssertEquals(true, SupportingDocument.eDocInfo.HasErrors());
		}

		#region ITSWAttachment

		public void TestUniqueIdentifier_()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE", "DESCRIPTION");

			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.BOL;
			SupportingDocument.eDoc = pK1;
			AssertEquals(pK1, CusAttachment.UniqueIdentifier);
		}

		public void TestFileName_()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "CODE.tif", "DESCRIPTION");

			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.BOL;
			SupportingDocument.eDoc = pK1;
			AssertEquals("CODE.tif", CusAttachment.FileName);

			ZGuid pK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK2, "CODE.doc", "DESCRIPTION");

			SupportingDocument.eDoc = pK2;
			AssertEquals("CODE.doc", CusAttachment.FileName);
		}

		public void TestDocType_()
		{
			SupportingDocument.AttachmentType = AttachmentTypeList.Codes.BOL;
			AssertEquals(AttachmentTypeList.Codes.BOL, CusAttachment.DocType);
		}

		public void TestFileTooBig()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1, "Doc1", "DESCRIPTION");
			SupportingDocument.eDoc = pK1;
			AssertEquals(false, CusAttachment.FileTooBig);

			ZGuid pK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK2, "Doc2", "DESCRIPTION" + StorageDocList.FileTooBigIndicator);
			SupportingDocument.eDoc = pK2;
			AssertEquals(true, CusAttachment.FileTooBig);
		}

		#endregion

		#region Implementation

		SupportingDocumentCollection SupportingDocuments
		{
			get { return supportingDocuments ?? (supportingDocuments = new SupportingDocumentCollection(Factory, MessageTypeList.Codes.ANA)); }
		}
		SupportingDocumentCollection supportingDocuments;

		ITSWAttachment CusAttachment
		{
			get { return SupportingDocument; }
		}

		SupportingDocument SupportingDocument
		{
			get { return supportingDocument ?? (supportingDocument = SupportingDocuments.AddNew()); }
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
