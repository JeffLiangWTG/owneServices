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

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	public class SupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SupportingDocumentCollection>
	{
		[TestDate(2007, 1, 1, 12, 12, 12)]
		public void TestStorageDocsCollection()
		{
			eDocs docs = new eDocs();
			eDoc doc = new eDoc();
			doc.FileName = "test.abc";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "docdoc.doc";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "pdfdoc.pdf";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "xlsdoc.xls";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "emfdoc.emf";
			docs.Add(doc);
			SupportingDocumentCollection supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			supportingDocumentCollection.SetStorageDocs(docs);
			AssertEquals(4, supportingDocumentCollection.StorageDocs.Count);
			AssertEquals("ABC - docdoc.doc", supportingDocumentCollection.StorageDocs[0].Code);
			AssertEquals("Date Added: 01-Jan-07 12:12:12", supportingDocumentCollection.StorageDocs[0].Description);
			AssertEquals("ABC - pdfdoc.pdf", supportingDocumentCollection.StorageDocs[1].Code);
			AssertEquals("ABC - xlsdoc.xls", supportingDocumentCollection.StorageDocs[2].Code);
			AssertEquals("ABC - emfdoc.emf", supportingDocumentCollection.StorageDocs[3].Code);
		}

		public void TestStorageDocsCollection_IllegalCharacterInFileName()
		{
			eDocs docs = new eDocs();
			eDoc doc = new eDoc();
			doc.FileName = "test|.abc";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "docdoc/.doc";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "pdfdoc>.pdf";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "xl<>/|sdoc<.xls";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "emfdoc.emf";
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "test?File*Name.doc";
			doc.DocType = SupportingDocumentTypeCodeList.Codes.DocType001;
			docs.Add(doc);
			doc = new eDoc();
			doc.FileName = "N°093498 - HAWB.PDF";
			doc.DocType = SupportingDocumentTypeCodeList.Codes.DocType005;
			docs.Add(doc);
			var supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			AssertNoExceptionThrown("No exception should be thrown", () => supportingDocumentCollection.SetStorageDocs(docs));
			AssertEquals(6, supportingDocumentCollection.StorageDocs.Count);
			AssertEquals("Illegal character (/) should be stripped from file name as well as file extension", "ABC - docdoc.doc", supportingDocumentCollection.StorageDocs[0].Code);
			AssertEquals("Illegal character (>) should be stripped from file name", "ABC - pdfdoc.pdf", supportingDocumentCollection.StorageDocs[1].Code);
			AssertEquals("Illegal characters (<>/|) should be stripped from file name", "ABC - xlsdoc.xls", supportingDocumentCollection.StorageDocs[2].Code);
			AssertEquals("ABC - emfdoc.emf", supportingDocumentCollection.StorageDocs[3].Code);
			AssertEquals("Illegal characters (?*) should be stripped from file name", "001 - testFileName.doc", supportingDocumentCollection.StorageDocs[4].Code);
			AssertEquals("Illegal character (°) should be stripped from file name", "005 - N093498_-_HAWB.PDF", supportingDocumentCollection.StorageDocs[5].Code);
		}

		public void TestAttachmentHasValidFileName()
		{
			eDocs docs = new eDocs();
			eDoc invalidDoc = new eDoc();
			invalidDoc.FileName = "CIV - CIÂ PLÂ -Â POÂ 1693715Â -Â SGN+MYÂ -Â FINAL.PDF";
			invalidDoc.DocType = "doc";
			docs.Add(invalidDoc);

			var supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			AssertNoExceptionThrown("No exception should be thrown", () => supportingDocumentCollection.SetStorageDocs(docs));
			AssertEquals("Illegal characters should be stripped from file name and white space replaced with underscores", "doc - CIV_-_CI_PL_-_PO_1693715_-_SGNMY_-_FINAL.PDF", supportingDocumentCollection.StorageDocs[0].Code);
		}

		public void TestAttachmentFileNameWhenAllCharactersStrippedOut()
		{
			eDocs docs = new eDocs();
			eDoc invalidDoc = new eDoc();
			invalidDoc.FileName = "ÂÂÂ.PDF";
			invalidDoc.DocType = "doc";
			docs.Add(invalidDoc);

			var supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			AssertNoExceptionThrown("No exception should be thrown", () => supportingDocumentCollection.SetStorageDocs(docs));
			AssertEquals("File name is all illegal characters", "doc - AttachedDocument.PDF", supportingDocumentCollection.StorageDocs[0].Code);
		}

		public void TestMultipleAttachmentFileNamesWhenAllCharactersStrippedOut()
		{
			var docs = new eDocs();
			var invalidDoc1 = new eDoc();
			invalidDoc1.FileName = "ÂÂÂ.PDF";
			invalidDoc1.DocType = "doc";
			docs.Add(invalidDoc1);

			var validDocName = new eDoc();
			validDocName.FileName = "testDoc.doc";
			validDocName.DocType = "doc";
			docs.Add(validDocName);

			var invalidDoc2 = new eDoc();
			invalidDoc2.FileName = "ÂÂÂ.PDF";
			invalidDoc2.DocType = "doc";
			docs.Add(invalidDoc1);

			var supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			AssertNoExceptionThrown("No exception should be thrown", () => supportingDocumentCollection.SetStorageDocs(docs));
			AssertEquals(3, supportingDocumentCollection.StorageDocs.Count);
			AssertEquals("File name is all illegal characters", "doc - AttachedDocument.PDF", supportingDocumentCollection.StorageDocs[0].Code);
			AssertEquals("doc - testDoc.doc", supportingDocumentCollection.StorageDocs[1].Code);
			AssertEquals("Subsequent illegal File names generated names are incremented", "doc - AttachedDocument2.PDF", supportingDocumentCollection.StorageDocs[2].Code);
		}

		public void TestMultipleDuplicatedFileNames()
		{
			var docs = new eDocs();
			var invalidDoc1 = new eDoc();
			invalidDoc1.FileName = "ÂÂÂ.PDF";
			invalidDoc1.DocType = "doc";
			docs.Add(invalidDoc1);

			var validDocName = new eDoc();
			validDocName.FileName = "testDoc.doc";
			validDocName.DocType = "doc";
			docs.Add(validDocName);

			var invalidDoc2 = new eDoc();
			invalidDoc2.FileName = "ÂÂÂ.PDF";
			invalidDoc2.DocType = "doc";
			docs.Add(invalidDoc1);

			var validDocName2 = new eDoc();
			validDocName2.FileName = "testDoc.doc";
			validDocName2.DocType = "doc";
			docs.Add(validDocName2);

			var invalidDoc3 = new eDoc();
			invalidDoc3.FileName = "货物清单ÂÂ.xlsx";
			invalidDoc3.DocType = "doc";
			docs.Add(invalidDoc3);

			var validDocName3 = new eDoc();
			validDocName3.FileName = "testWordDoc.doc";
			validDocName3.DocType = "doc";
			docs.Add(validDocName3);

			var validDocName4 = new eDoc();
			validDocName4.FileName = "testWordDoc.doc";
			validDocName4.DocType = "doc";
			docs.Add(validDocName4);

			var supportingDocumentCollection = new SupportingDocumentCollection(Factory);
			AssertNoExceptionThrown("No exception should be thrown", () => supportingDocumentCollection.SetStorageDocs(docs));
			AssertEquals(7, supportingDocumentCollection.StorageDocs.Count);
			AssertEquals("File name is all illegal characters", "doc - AttachedDocument.PDF", supportingDocumentCollection.StorageDocs[0].Code);
			AssertEquals("doc - testDoc.doc", supportingDocumentCollection.StorageDocs[1].Code);
			AssertEquals("Subsequent illegal File names generated names are incremented", "doc - AttachedDocument2.PDF", supportingDocumentCollection.StorageDocs[2].Code);
			AssertEquals("Subsequent duplicate File names names are also incremented", "doc - testDoc3.doc", supportingDocumentCollection.StorageDocs[3].Code);
			AssertEquals("Subsequent illegal File names generated names are incremented", "doc - AttachedDocument4.xlsx", supportingDocumentCollection.StorageDocs[4].Code);
			AssertEquals("doc - testWordDoc.doc", supportingDocumentCollection.StorageDocs[5].Code);
			AssertEquals("Subsequent duplicate File names names are also incremented", "doc - testWordDoc5.doc", supportingDocumentCollection.StorageDocs[6].Code);
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
				get
				{
					return docs.Count;
				}
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

			public IeDoc this[int index]
			{
				get
				{
					return (IeDoc)docs[index];
				}
			}

			public bool ContainsDocType(ZString docType)
			{
				return false;
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
				get
				{
					return ZDateTime.Now;
				}

				set
				{
				}
			}

			public ZString Description
			{
				get
				{
					return "Description";
				}

				set
				{
				}
			}

			public ZString DocType
			{
				get
				{
					return docType;
				}

				set
				{
					docType = value;
				}
			}

			ZString docType = "ABC";
			public ZString DocSourceDescription
			{
				get
				{
					return "Description";
				}

				set
				{
				}
			}

			public ZString DocSource
			{
				get;
				set;
			}

			public CodeDescriptionPairList DocType_List
			{
				get
				{
					return new CodeDescriptionPairList();
				}
			}

			public ZString FileName
			{
				get
				{
					return fileName;
				}

				set
				{
					fileName = value;
				}
			}

			ZString fileName;
			public ZBlob ImageData
			{
				get
				{
					return null;
				}

				set
				{
				}
			}

			public ZBool IsDeleted
			{
				get
				{
					return false;
				}

				set
				{
				}
			}

			public ZBool IsPublished
			{
				get
				{
					return false;
				}

				set
				{
				}
			}

			public ZBool IsSystemGenerated
			{
				get
				{
					return false;
				}
			}

			public ZString VisibleCompanyCode
			{
				get
				{
					return string.Empty;
				}
			}

			public ZString VisibleBranchCode
			{
				get
				{
					return string.Empty;
				}
			}

			public ZString VisibleDepartmentCode
			{
				get
				{
					return string.Empty;
				}
			}

			public ZDateTime LastEdited
			{
				get
				{
					return ZDateTime.Now.AddDays(-1);
				}
			}

			public ZString LastEditedUser
			{
				get
				{
					return GlbStaff.CurrentUser.GS_Code;
				}
			}

			public ZBool IsCustomisableDocTypes
			{
				get
				{
					return false;
				}
			}

			public void NotifyReadByUser()
			{
			}

			public ZGuid UniqueKey
			{
				get
				{
					return uniqueKey;
				}
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
				get
				{
					return null;
				}
			}

			public ZDecimal FileSizeInMB
			{
				get;
				set;
			}

			public IDisposable OpenForEdit()
			{
				throw new NotImplementedException();
			}

			public Stream GetImageDataReader() => new CargoWise.IO.Shim.SubStreamableStream();
			public void SetImageDataStream(Stream stream)
			{
			}

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
			return new SupportingDocumentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SupportingDocument(Factory);
		}
		#endregion
	}
}
