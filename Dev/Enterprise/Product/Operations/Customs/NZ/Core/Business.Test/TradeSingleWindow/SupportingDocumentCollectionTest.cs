using System;
using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	sealed class SupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SupportingDocumentCollection>
	{
		[TestDate(2007, 1, 1, 12, 12, 12)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStorageDocsCollection()
		{
			var docs = new eDocs();

			var doc = new eDoc();
			doc.FileName = "test.abc";
			docs.Add(doc);

			doc = new eDoc();
			doc.FileName = "docdoc.docx";
			doc.DocType = Core.Constants.RefDocTypes.MasterAirWaybill;
			doc.Description = "MasterBill for B000123345";
			docs.Add(doc);

			doc = new eDoc();
			doc.FileName = "pdfdoc.pdf";
			doc.DocType = Core.Constants.RefDocTypes.Invoice;
			doc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF"));
			docs.Add(doc);

			doc = new eDoc();
			doc.FileName = "xlsdoc.xlsx";
			doc.Description = "Client Data";
			docs.Add(doc);

			doc = new eDoc();
			doc.FileName = "pngdoc.png";
			doc.DocType = Core.Constants.RefDocTypes.QuarantineRemotePrint;
			doc.Description = "Quarantine Report";
			docs.Add(doc);

			doc = new eDoc();
			doc.FileName = "jpegdoc.jpg";
			doc.DocType = Core.Constants.RefDocTypes.PackingList;
			docs.Add(doc);

			doc = new eDoc();
			doc.FileName = "gifdoc.gif";
			docs.Add(doc);

			var supportingDocumentCollection = new SupportingDocumentCollection(Factory, "IM1");
			supportingDocumentCollection.SetStorageDocs(docs);

			AssertEquals(6, supportingDocumentCollection.StorageDocs.Count);
			AssertEquals("MAW-docdoc.docx", supportingDocumentCollection.StorageDocs[0].Code);
			AssertEquals("Added: 01-Jan-07 12:12:12 - MasterBill for B000123345", supportingDocumentCollection.StorageDocs[0].Description);
			AssertEquals("INV-pdfdoc.pdf", supportingDocumentCollection.StorageDocs[1].Code);
			AssertEquals("Added: 01-Jan-07 12:12:12 - Description", supportingDocumentCollection.StorageDocs[1].Description);
			AssertEquals("ABC-xlsdoc.xlsx", supportingDocumentCollection.StorageDocs[2].Code);
			AssertEquals("Added: 01-Jan-07 12:12:12 - Client Data", supportingDocumentCollection.StorageDocs[2].Description);
			AssertEquals("QRP-pngdoc.png", supportingDocumentCollection.StorageDocs[3].Code);
			AssertEquals("Added: 01-Jan-07 12:12:12 - Quarantine Report", supportingDocumentCollection.StorageDocs[3].Description);
			AssertEquals("PKL-jpegdoc.jpg", supportingDocumentCollection.StorageDocs[4].Code);
			AssertEquals("Added: 01-Jan-07 12:12:12 - Description", supportingDocumentCollection.StorageDocs[4].Description);
			AssertEquals("ABC-gifdoc.gif", supportingDocumentCollection.StorageDocs[5].Code);
			AssertEquals("Added: 01-Jan-07 12:12:12 - Description", supportingDocumentCollection.StorageDocs[5].Description);
		}

		[TestDate(2020, 12, 10, 11, 36, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAttachmentsAreValidTypes()
		{
			var docs = new eDocs();
			var supportingDocumentCollection = new SupportingDocumentCollection(Factory, "IM1");

			var invalidDoc = new eDoc();
			docs.Add(invalidDoc);

			var validDoc = new eDoc();
			validDoc.FileName = "Document.docx";
			validDoc.DocType = Core.Constants.RefDocTypes.MasterAirWaybill;
			validDoc.Description = "Valid word Document";
			validDoc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF"));
			docs.Add(validDoc);

			var validExcelDoc = new eDoc();
			validExcelDoc.FileName = "Spreadsheet.xlsx";
			validExcelDoc.DocType = Core.Constants.RefDocTypes.PackingList;
			validExcelDoc.Description = "Valid excel Document";
			validExcelDoc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"));
			docs.Add(validExcelDoc);

			var validPdfDoc = new eDoc();
			validPdfDoc.FileName = "ProtectedDataFile.pdf";
			validPdfDoc.DocType = Core.Constants.RefDocTypes.ContainerList;
			validPdfDoc.Description = "Valid pdf Document";
			validPdfDoc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF"));
			docs.Add(validPdfDoc);

			var validCsvDoc = new eDoc();
			validCsvDoc.FileName = "CommaSeperatedFile.csv";
			validCsvDoc.Description = "Valid csv Document";
			docs.Add(validCsvDoc);

			var unknownDoc = new eDoc();
			unknownDoc.FileName = "UNKNOWN.pif";
			unknownDoc.Description = "Invalid Document";
			docs.Add(unknownDoc);

			var validTifDoc = new eDoc();
			validTifDoc.FileName = "TIF.tif";
			validTifDoc.Description = "Valid tif Document";
			validTifDoc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Small.tif"));
			docs.Add(validTifDoc);

			var validGifDoc = new eDoc();
			validGifDoc.FileName = "GIF.gif";
			validGifDoc.Description = "Valid gif Document";
			validGifDoc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Small.gif"));
			docs.Add(validGifDoc);

			var validJpegDoc = new eDoc();
			validJpegDoc.FileName = "JPEG.jpg";
			validJpegDoc.Description = "Valid jpeg Document";
			validJpegDoc.ImageData = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Small.jpg"));
			docs.Add(validJpegDoc);

			var validEmfDoc = new eDoc();
			validEmfDoc.FileName = "PNG.png";
			validEmfDoc.Description = "Valid png Document";
			docs.Add(validEmfDoc);

			supportingDocumentCollection.SetStorageDocs(docs);
			AssertEquals("All eDocs", 10, docs.Count);
			AssertEquals("Only valid Documents for sending are shown to user for selection on sending", 8, supportingDocumentCollection.StorageDocs.Count);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid word Document", supportingDocumentCollection.StorageDocs[0].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid excel Document", supportingDocumentCollection.StorageDocs[1].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid pdf Document", supportingDocumentCollection.StorageDocs[2].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid csv Document", supportingDocumentCollection.StorageDocs[3].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid tif Document", supportingDocumentCollection.StorageDocs[4].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid gif Document", supportingDocumentCollection.StorageDocs[5].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid jpeg Document", supportingDocumentCollection.StorageDocs[6].Description);
			AssertEquals("Added: 10-Dec-20 11:36:15 - Valid png Document", supportingDocumentCollection.StorageDocs[7].Description);
		}

		#region TestClasses

		public class eDocs : IStorageDocsBaseCollection
		{
			readonly ArrayList docs = new ArrayList();

			#region IStorageDocsBaseCollection Members

			public void Add(IeDoc elementToAdd)
			{
				docs.Add(elementToAdd);
			}

			public bool Contains(IeDoc element)
			{
				return docs.Contains(element);
			}

			public int Count
			{
				get { return docs.Count; }
			}

			public IeDoc GetFromUniqueKey(Guid uniqueKey)
			{
				foreach (IeDoc doc in docs)
				{
					if (doc.UniqueKey == uniqueKey)
					{
						return doc;
					}
				}

				return null;
			}

			public IeDoc GetMostRecentEDoc(string docType)
			{
				return null;
			}

			public void Remove(IeDoc elementToRemove)
			{
			}

			public bool ContainsDocType(ZString docType)
			{
				return false;
			}

			public IeDoc this[int index]
			{
				get { return (IeDoc)docs[index]; }
			}

			#endregion

			#region IEnumerable Members

			public IEnumerator GetEnumerator()
			{
				return docs.GetEnumerator();
			}

			#endregion
		}

		sealed public class eDoc : IeDoc
		{
			#region IeDoc Members

			public ZDateTime DateAdded
			{
				get { return ZDateTime.Now; }
				set { /* Why oh why is this code duplicated here and everywhere else???*/ }
			}

			public ZString Description
			{
				get { return description; }
				set { description = value; }
			}
			ZString description = "Description";

			public ZString DocType
			{
				get { return docType; }
				set { docType = value; }
			}
			ZString docType = "ABC";

			public ZString DocSourceDescription
			{
				get { return "Description"; }
				set { }
			}

			public ZString DocSource
			{
				get;
				set;
			}

			public CodeDescriptionPairList DocType_List
			{
				get { return new CodeDescriptionPairList(); }
			}

			public ZString FileName
			{
				get { return fileName; }
				set { fileName = value; }
			}
			ZString fileName;

			public ZBlob ImageData
			{
				get { return null; }
				set { }
			}

			public ZBool IsDeleted
			{
				get { return false; }
				set { }
			}

			public ZBool IsPublished
			{
				get { return false; }
				set { }
			}

			public ZBool IsSystemGenerated
			{
				get { return false; }
			}

			public ZDateTime LastEdited
			{
				get { return ZDateTime.Now.AddDays(-1); }
			}

			public ZString LastEditedUser
			{
				get { return GlbStaff.CurrentUser.GS_Code; }
			}

			public ZString VisibleCompanyCode
			{
				get { return string.Empty; }
			}

			public ZString VisibleBranchCode
			{
				get { return string.Empty; }
			}

			public ZString VisibleDepartmentCode
			{
				get { return string.Empty; }
			}

			public void NotifyReadByUser()
			{
			}

			public ZBool IsCustomisableDocTypes
			{
				get { return false; }
			}

			public ZGuid UniqueKey
			{
				get { return uniqueKey; }
			}
			readonly ZGuid uniqueKey = ZGuid.NewZGuid();

			public BusinessObject ParentMain
			{
				get
				{
					return null;
				}
			}

			public void SetValuesForTest(ZDateTime dateTime, ZString dataType)
			{
			}

			public ZString DataType
			{
				get
				{
					return null;
				}
			}

			public ZString FileNameOnly
			{
				get { return null; }
			}

			public ZDecimal FileSizeInMB
			{
				get
				{
					return ZDecimal.Zero;
				}
			}

			public IDisposable OpenForEdit()
			{
				throw new NotImplementedException();
			}

			public Stream GetImageDataReader() => new CargoWise.IO.Shim.SubStreamableStream();
			public void SetImageDataStream(Stream stream) { }

			public string CreateReference()
			{
				throw new NotImplementedException();
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		#endregion

		#region Overrides

		protected override SupportingDocumentCollection GetCollectionToTest()
		{
			return new SupportingDocumentCollection(Factory, "IM1");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupportingDocument(Factory, "IM1");
		}

		#endregion
	}
}
