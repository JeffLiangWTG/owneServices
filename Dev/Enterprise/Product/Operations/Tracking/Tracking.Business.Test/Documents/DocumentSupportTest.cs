using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class DocumentSupportTest : TestCaseWithFactory
	{
		public void TestStorageMain()
		{
			var blankShipment = Factory.New<TrackingShipment>();
			var documentSupport1 = new DocumentSupport(blankShipment);

			AssertNull("If an invalid business object is specified, the storage main should be null.", documentSupport1.StorageMain);
			AssertEquals("If an invalid business object is specified, zero documents should be returned.", 0, documentSupport1.PublishedEDocsAndFiles.Count);
			AssertEquals("If an invalid business object is specified, zero related document should be returned.", 0, documentSupport1.RelatedBODocuments.Count);

			AssertNotNull("The storage main should not be null with a valid parent.", Tester.StorageMain);
			AssertEquals("Two documents should be returned.", 2, Tester.StorageMain.Documents.Count);
		}

		public void TestDocumentsDoesNotBlowUpWhenDocumentDatabaseNotThere()
		{
			Parent.SM_DB = 99;

			MasterFactory.Save();

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			Assert("Documents should be a DocumentViewCollection", BizObj.DocumentHelper.Documents is DocumentViewCollection);
			AssertEquals("Document collection should be empty", 0, BizObj.DocumentHelper.Documents.Count);
		}

		public void TestDocumentsDoesNotReturnNull()
		{
			AssertNotNull("Documents should never return null", BizObj.DocumentHelper.Documents);
			AssertEquals("Documents should return empty collection", 0, BizObj.DocumentHelper.Documents.Count);
		}

		public void TestDocuments()
		{
			var document1 = Parent.Documents.AddNew();
			document1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document1.SC_IsPublished = true;

			var document2 = Parent.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_IsPublished = true;

			var document3 = Parent.Documents.AddNew();
			document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document3.SC_IsPublished = true;

			MasterFactory.Save();

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			Assert("Documents should be a DocumentViewCollection", BizObj.DocumentHelper.Documents is DocumentViewCollection);
			AssertEquals("Incorrect document count", 3, BizObj.DocumentHelper.Documents.Count);

			AssertEquals("First document not found", true, BizObj.DocumentHelper.Documents.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.Documents.Contains(document2.PK));
			AssertEquals("Third document not found", true, BizObj.DocumentHelper.Documents.Contains(document3.PK));
		}

		public void TestDocumentsDoesNotIncludeDeletedDocuments()
		{
			var document1 = Parent.Documents.AddNew();
			document1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document1.SC_IsPublished = true;

			var document2 = Parent.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_IsPublished = true;

			var document3 = Parent.Documents.AddNew();
			document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document3.SC_IsPublished = true;
			document3.DeleteQuietly();

			MasterFactory.Save();

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			Assert("Documents should be a DocumentViewCollection", BizObj.DocumentHelper.Documents is DocumentViewCollection);
			AssertEquals("Incorrect document count", 2, BizObj.DocumentHelper.Documents.Count);

			AssertEquals("First document not found", true, BizObj.DocumentHelper.Documents.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.Documents.Contains(document2.PK));
			AssertEquals("Third document not found", false, BizObj.DocumentHelper.Documents.Contains(document3.PK));
		}

		public void TestDocumentsDoesNotIncludeUnpublishedDocuments()
		{
			var document1 = Parent.Documents.AddNew();
			document1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document1.SC_IsPublished = true;

			var document2 = Parent.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_IsPublished = true;

			var document3 = Parent.Documents.AddNew();
			document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document3.SC_IsPublished = false;

			MasterFactory.Save();

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			Assert("Documents should be a DocumentViewCollection", BizObj.DocumentHelper.Documents is DocumentViewCollection);
			AssertEquals("Incorrect document count", 2, BizObj.DocumentHelper.Documents.Count);

			AssertEquals("First document not found", true, BizObj.DocumentHelper.Documents.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.Documents.Contains(document2.PK));
			AssertEquals("Third document should not be included", false, BizObj.DocumentHelper.Documents.Contains(document3.PK));
		}

		public void TestDocumentsChecksForPermissions()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var allowedDoc = Parent.Documents.AddNew();
			allowedDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			allowedDoc.SC_IsPublished = true;

			var bannedDoc = Parent.Documents.AddNew();
			bannedDoc.SC_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			bannedDoc.SC_IsPublished = true;

			var security = new DocumentWebSecurityRights(Factory).GetSecurityRight(bannedDoc.DocType);
			var right = helper.TestOrg.SecurityRights.AddNew();
			right.OX_SecurityItemName = security.Code;
			right.OX_Granted = false;

			MasterFactory.Save();

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			AssertEquals("Should contain the one permitted doc", 1, BizObj.DocumentHelper.Documents.Count);

			Assert("The allowedDoc should be permitted", BizObj.DocumentHelper.Documents.Contains(allowedDoc.PK));
			Assert("The docType we don't have permission for should not be present", !BizObj.DocumentHelper.Documents.Contains(bannedDoc.PK));
		}

		public void TestRequiredDocumentsWithoutWebSecurity()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var documentType = Factory.New<RefDocType>();
			documentType.RT_DocType = "TST";
			documentType.RT_Desc = "Test Document";
			documentType.RT_ReferenceType = "TST";

			var security = new DocumentWebSecurityRights(Factory).GetSecurityRight(documentType);
			var right = helper.TestOrg.SecurityRights.AddNew();
			right.OX_SecurityItemName = security.Code;
			right.OX_Granted = true;

			var contactRight = Factory.NewWithValidTestData<OrgSecurityContacts>();
			contactRight.OZ_OX = right.PK;
			contactRight.OZ_Granted = false;
			contactRight.OZ_OC = helper.TestContact.PK;

			var requiredDocumentsParent = ((IDocsAndCartageParent)Tester.Parent).RequiredDocumentsProvider;
			AssertNotNull(requiredDocumentsParent);

			var document = requiredDocumentsParent.RequiredDocuments.AddNew();
			document.EQ_DocType = "TST";
			document.EQ_DocCategory = "TST";

			AssertNotNull(Tester.Documents);
			AssertEquals("Should not include the required document without a web security", false, Tester.Documents.OfType<DocumentView>().Any(d => d.DocType == "TST"));
		}

		public void TestRequiredDocumentsNotPublished()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var documentType = Factory.New<RefDocType>();
			documentType.RT_DocType = "TST";
			documentType.RT_Desc = "Test Document";
			documentType.RT_ReferenceType = "TST";
			documentType.RT_IsPublished = false;

			var requiredDocumentsParent = ((IDocsAndCartageParent)Tester.Parent).RequiredDocumentsProvider;
			AssertNotNull(requiredDocumentsParent);

			var document = requiredDocumentsParent.RequiredDocuments.AddNew();
			document.EQ_DocType = "TST";
			document.EQ_DocCategory = "TST";

			AssertNotNull(Tester.Documents);
			AssertEquals("Should not include the required document since it is unpublished", false, Tester.Documents.OfType<DocumentView>().Any(d => d.DocType == "TST"));
		}

		public void TestDocumentsChecksForPermissions_RelatedBO()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var parentFk = ZGuid.NewZGuid();
			var related = MasterFactory.NewWithValidTestData<StorageMain>();
			related.SM_Type = "DEC";
			related.SM_ParentFK = parentFk;

			BizObj.AddToDocRelatedPKs(parentFk);

			var allowedDoc = related.Documents.AddNew();
			allowedDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			allowedDoc.SC_IsPublished = true;

			var bannedDoc = related.Documents.AddNew();
			bannedDoc.SC_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			bannedDoc.SC_IsPublished = true;

			var security = new DocumentWebSecurityRights(Factory).GetSecurityRight(bannedDoc.DocType);
			var right = helper.TestOrg.SecurityRights.AddNew();
			right.OX_SecurityItemName = security.Code;
			right.OX_Granted = true;

			var contactRight = Factory.NewWithValidTestData<OrgSecurityContacts>();
			contactRight.OZ_OX = right.PK;
			contactRight.OZ_Granted = false;
			contactRight.OZ_OC = helper.TestContact.PK;

			MasterFactory.Save();

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			AssertEquals("Should contain the one permitted doc", 1, BizObj.DocumentHelper.Documents.Count);

			Assert("The allowedDoc should be permitted", BizObj.DocumentHelper.Documents.Contains(allowedDoc.PK));
			Assert("The docType we don't have permission for should not be present", !BizObj.DocumentHelper.Documents.Contains(bannedDoc.PK));
		}

		public void TestDocumentsDoesNotIncludeRequiredDocWithUnpublishedDocuments()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var document1 = Parent.Documents.AddNew();
			document1.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document1.SC_IsPublished = true;

			var document2 = Parent.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document2.SC_IsPublished = true;

			var document3 = Parent.Documents.AddNew();
			document3.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document3.SC_Desc = "Unpublished Miscellaneous Document";
			document3.SC_IsPublished = false;

			var document4 = Parent.Documents.AddNew();
			document4.SC_DocType = Core.Constants.RefDocTypes.Letter;
			document3.SC_IsPublished = false;

			var requiredDoc = BizObj.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Label);
			requiredDoc.DocType.RT_IsPublished = true;

			var requiredDoc3 = BizObj.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.MiscellaneousDocument);
			requiredDoc3.EQ_DocDescription = "Unpublished Miscellaneous Document";

			BizObj.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Letter);

			MasterFactory.Save();

			AssertEquals("Should be three Required Document", 3, BizObj.RequiredDocuments.Count);

			AssertNotNull("Documents should not be null", BizObj.DocumentHelper.Documents);
			Assert("Documents should be a DocumentViewCollection", BizObj.DocumentHelper.Documents is DocumentViewCollection);
			AssertEquals("Incorrect document count", 3, BizObj.DocumentHelper.Documents.Count);

			AssertEquals("First document not found", true, BizObj.DocumentHelper.Documents.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.Documents.Contains(document2.PK));
			AssertEquals("Required document not found", true, BizObj.DocumentHelper.Documents.Contains(requiredDoc.PK));
			AssertEquals("Third document should not be included", false, BizObj.DocumentHelper.Documents.Contains(document3.PK));
			AssertEquals("Fourth document should not be included", false, BizObj.DocumentHelper.Documents.Contains(document4.PK));
		}

		public void TestDocumentViewDescriptionShouldBeFileName()
		{
			var fileNames = new[] { "Test document.txt", "Test file.doc" };

			CombineAssertions(() =>
			{
				Tester.Documents.OfType<DocumentView>()
					.ForEach(d => Assert(fileNames.Contains(Path.GetFileNameWithoutExtension(d.Description))));
			});
		}

		public void TestLoadingDocumentsDontTriggerImageData()
		{
			var relatedPK = ZGuid.NewZGuid();
			var relatedStorage = MasterFactory.NewWithValidTestData<StorageMain>();
			relatedStorage.SM_Type = "DEC";
			relatedStorage.SM_ParentFK = relatedPK;

			var smallTiffBytes = DocumentUtilities.GetFileAsBytes(SmallTifPath);

			using (var originalDoc = Parent.Documents.AddNew())
			using (var originalrelatedDoc = relatedStorage.Documents.AddNew())
			{
				originalDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				originalDoc.SC_IsPublished = true;
				originalDoc.SC_ImageData = smallTiffBytes;
				originalDoc.SaveToTempFile();
				File.Copy(SmallTifPath, originalDoc.TempFileName, true);
				File.SetAttributes(originalDoc.TempFileName, FileAttributes.Normal);
				originalDoc.SetImageData();

				originalrelatedDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				originalrelatedDoc.SC_IsPublished = true;
				originalrelatedDoc.SC_ImageData = smallTiffBytes;
				originalrelatedDoc.SaveToTempFile();
				File.Copy(SmallTifPath, originalrelatedDoc.TempFileName, true);
				File.SetAttributes(originalrelatedDoc.TempFileName, FileAttributes.Normal);
				originalrelatedDoc.SetImageData();

				MasterFactory.Save();
				Factory.Save();

				var reloadedFactory = new BusinessObjectFactory();
				var reloadedBizObj = reloadedFactory.Load<IWebDocumentsSupportInternalTest.DummyBusinessObjectWithDocumentSupport>(BizObj.PK);
				AssertNotNull(reloadedBizObj);

				reloadedBizObj.AddToDocRelatedPKs(relatedPK);
				AssertEquals(2, reloadedBizObj.DocumentHelper.Documents.Count);

				var reloadedDoc1 = reloadedBizObj.DocumentHelper.Documents[0];
				AssertEquals(true, reloadedDoc1.StorageDoc.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));

				var reloadedDoc2 = reloadedBizObj.DocumentHelper.Documents[1];
				AssertEquals(true, reloadedDoc2.StorageDoc.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
			}
		}

		public void TestLoadingAllDocumentsDontTriggerImageData()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			using (var originalDoc = Parent.Documents.AddNew())
			{
				originalDoc.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				originalDoc.SC_IsPublished = true;
				originalDoc.SC_ImageData = DocumentUtilities.GetFileAsBytes(SmallTifPath);
				originalDoc.SaveToTempFile();
				File.Copy(SmallTifPath, originalDoc.TempFileName, true);
				File.SetAttributes(originalDoc.TempFileName, FileAttributes.Normal);
				originalDoc.SetImageData();

				MasterFactory.Save();
				Factory.Save();

				var reloadedFactory = new BusinessObjectFactory();
				var reloadedBizObj = reloadedFactory.Load<IWebDocumentsSupportInternalTest.DummyBusinessObjectWithDocumentSupport>(BizObj.PK);
				AssertNotNull(reloadedBizObj);

				AssertEquals(1, reloadedBizObj.DocumentHelper.AllDocuments.Count);

				var reloadedDoc = reloadedBizObj.DocumentHelper.AllDocuments[0];
				AssertEquals(true, reloadedDoc.StorageDoc.BlobFieldsNeedLoadingExposedForTest(StorageDocsSchema.SC_ImageData));
			}
		}

		public void TestRelatedBODocumentsDoesNotReturnNullWithoutRelatedBO()
		{
			AssertNotNull("RelatedBizOPKs should not be null", BizObj.DocRelatedPKs);
			AssertEquals("RelatedBizOPKs is empty ZGuid Array", 0, BizObj.DocRelatedPKs.Count);
			AssertNotNull("RelatedBODocuments should never return null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("RelatedBODocuments should return empty collection", 0, BizObj.DocumentHelper.RelatedBODocuments.Count);
		}

		public void TestRelatedBODocumentsDoesNotReturnNull()
		{
			var relatedPK = ZGuid.NewZGuid();

			BizObj.AddToDocRelatedPKs(relatedPK);
			var related = MasterFactory.New<StorageMain>();
			related.SM_Type = "DEC";
			related.SM_ParentFK = relatedPK;
			related.SM_DB = 1;

			MasterFactory.Save();

			AssertNotNull("RelatedBODocuments should never return null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("RelatedBODocuments should return empty collection", 0, BizObj.DocumentHelper.RelatedBODocuments.Count);
		}

		public void TestRelatedBODocuments()
		{
			var relatedPK1 = ZGuid.NewZGuid();
			var relatedPK2 = ZGuid.NewZGuid();
			BizObj.AddToDocRelatedPKs(relatedPK1);
			BizObj.AddToDocRelatedPKs(relatedPK2);
			AssertEquals("RelatedPKs were not added", 2, BizObj.DocRelatedPKs.Count);
			AssertEquals("First RelatedPK is something different from what we added", relatedPK1, BizObj.DocRelatedPKs[0]);
			AssertEquals("Second RelatedPK is something different from what we added", relatedPK2, BizObj.DocRelatedPKs[1]);

			var related1 = MasterFactory.New<StorageMain>();
			related1.SM_Type = "DEC";
			related1.SM_ParentFK = relatedPK1;
			related1.SM_DB = 1;

			var document1 = related1.Documents.AddNew();
			document1.SC_IsPublished = true;
			var document2 = related1.Documents.AddNew();
			document2.SC_IsPublished = true;
			var document3 = related1.Documents.AddNew();
			document3.SC_IsPublished = true;

			var related2 = MasterFactory.New<StorageMain>();
			related2.SM_Type = "DEC";
			related2.SM_ParentFK = relatedPK2;
			related2.SM_DB = 1;

			var document4 = related2.Documents.AddNew();
			document4.SC_IsPublished = true;
			var document5 = related2.Documents.AddNew();
			document5.SC_IsPublished = true;

			AssertEquals("Documents were not added to the second related BO", 2, related2.Documents.Count);

			MasterFactory.Save();

			AssertNotNull("RelatedBODocuments should not be null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("Incorrect document count", 5, BizObj.DocumentHelper.RelatedBODocuments.Count);
			AssertEquals("First document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document2.PK));
			AssertEquals("Third document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document3.PK));
			AssertEquals("Fourth document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document4.PK));
			AssertEquals("Fifth document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document5.PK));

			AssertEquals("First document not found", true, BizObj.DocumentHelper.Documents.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.Documents.Contains(document2.PK));
			AssertEquals("Third document not found", true, BizObj.DocumentHelper.Documents.Contains(document3.PK));
			AssertEquals("Fourth document not found", true, BizObj.DocumentHelper.Documents.Contains(document4.PK));
			AssertEquals("Fifth document not found", true, BizObj.DocumentHelper.Documents.Contains(document5.PK));
		}

		public void TestRelatedBODocumentsDoesNotIncludedDeletedDocuments()
		{
			var relatedPK1 = ZGuid.NewZGuid();
			var relatedPK2 = ZGuid.NewZGuid();
			BizObj.AddToDocRelatedPKs(relatedPK1);
			BizObj.AddToDocRelatedPKs(relatedPK2);

			AssertEquals("RelatedPKs were not added", 2, BizObj.DocRelatedPKs.Count);
			AssertEquals("First RelatedPK is something different from what we added", relatedPK1, BizObj.DocRelatedPKs[0]);
			AssertEquals("Second RelatedPK is something different from what we added", relatedPK2, BizObj.DocRelatedPKs[1]);

			var related1 = MasterFactory.New<StorageMain>();
			related1.SM_Type = "DEC";
			related1.SM_ParentFK = relatedPK1;
			related1.SM_DB = 1;

			var document1 = related1.Documents.AddNew();
			document1.SC_IsPublished = true;
			var document2 = related1.Documents.AddNew();
			document2.SC_IsPublished = true;
			var document3 = related1.Documents.AddNew();
			document3.SC_IsPublished = true;
			document3.DeleteQuietly();

			var related2 = MasterFactory.New<StorageMain>();
			related2.SM_Type = "DEC";
			related2.SM_ParentFK = relatedPK2;
			related2.SM_DB = 1;

			var document4 = related2.Documents.AddNew();
			document4.SC_IsPublished = true;
			var document5 = related2.Documents.AddNew();
			document5.SC_IsPublished = true;

			AssertEquals("Documents were not added to the second related BO", 2, related2.Documents.Count);

			MasterFactory.Save();

			AssertNotNull("RelatedBODocuments should not be null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("Incorrect document count", 4, BizObj.DocumentHelper.RelatedBODocuments.Count);
			AssertEquals("First document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document2.PK));
			AssertEquals("Deleted document found", false, BizObj.DocumentHelper.RelatedBODocuments.Contains(document3.PK));
			AssertEquals("Fourth document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document4.PK));
			AssertEquals("Fifth document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document5.PK));
		}

		public void TestRelatedBODocumentsDoesNotIncludedUnpublishedDocuments()
		{
			var relatedPK1 = ZGuid.NewZGuid();
			var relatedPK2 = ZGuid.NewZGuid();
			BizObj.AddToDocRelatedPKs(relatedPK1);
			BizObj.AddToDocRelatedPKs(relatedPK2);

			AssertEquals("RelatedPKs were not added", 2, BizObj.DocRelatedPKs.Count);
			AssertEquals("First RelatedPK is something different from what we added", relatedPK1, BizObj.DocRelatedPKs[0]);
			AssertEquals("Second RelatedPK is something different from what we added", relatedPK2, BizObj.DocRelatedPKs[1]);

			var related1 = MasterFactory.New<StorageMain>();
			related1.SM_Type = "DEC";
			related1.SM_ParentFK = relatedPK1;
			related1.SM_DB = 1;

			var document1 = related1.Documents.AddNew();
			document1.SC_IsPublished = true;
			var document2 = related1.Documents.AddNew();
			document2.SC_IsPublished = true;
			var document3 = related1.Documents.AddNew();
			document3.SC_IsPublished = false;

			var related2 = MasterFactory.New<StorageMain>();
			related2.SM_Type = "DEC";
			related2.SM_ParentFK = relatedPK2;
			related2.SM_DB = 1;

			var document4 = related2.Documents.AddNew();
			document4.SC_IsPublished = true;
			var document5 = related2.Documents.AddNew();
			document5.SC_IsPublished = true;

			AssertEquals("Documents were not added to the second related BO", 2, related2.Documents.Count);

			MasterFactory.Save();

			AssertNotNull("RelatedBODocuments should not be null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("Incorrect document count", 4, BizObj.DocumentHelper.RelatedBODocuments.Count);
			AssertEquals("First document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document2.PK));
			AssertEquals("Unpublished document not found", false, BizObj.DocumentHelper.RelatedBODocuments.Contains(document3.PK));
			AssertEquals("Fourth document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document4.PK));
			AssertEquals("Fifth document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document5.PK));
		}

		public void TestRelatedBODocumentsIncludeAllDataTypes()
		{
			var relatedPK1 = ZGuid.NewZGuid();
			BizObj.AddToDocRelatedPKs(relatedPK1);

			AssertEquals("RelatedPKs were not added", 1, BizObj.DocRelatedPKs.Count);
			AssertEquals("First RelatedPK is something different from what we added", relatedPK1, BizObj.DocRelatedPKs[0]);

			var related1 = MasterFactory.New<StorageMain>();
			related1.SM_Type = "DEC";
			related1.SM_ParentFK = relatedPK1;
			related1.SM_DB = 1;

			var document1 = related1.Documents.AddNew();
			document1.SC_IsPublished = true;
			document1.SC_DataType = Core.Constants.FileFormats.TIF;
			var document2 = related1.Documents.AddNew();
			document2.SC_IsPublished = true;
			document2.SC_DataType = "DOC";
			var document3 = related1.Documents.AddNew();
			document3.SC_IsPublished = true;
			document3.SC_DataType = "TXT";

			MasterFactory.Save();

			AssertNotNull("RelatedBODocuments should not be null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("Incorrect document count", 3, BizObj.DocumentHelper.RelatedBODocuments.Count);
			AssertEquals("First document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document2.PK));
			AssertEquals("Third document not found", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document3.PK));
		}

		public void TestRelatedAndRequiredDocuments()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var relatedPK1 = ZGuid.NewZGuid();
			var relatedPK2 = ZGuid.NewZGuid();
			BizObj.AddToDocRelatedPKs(relatedPK1);
			BizObj.AddToDocRelatedPKs(relatedPK2);

			AssertEquals("RelatedPKs were not added", 2, BizObj.DocRelatedPKs.Count);
			AssertEquals("First RelatedPK is something different from what we added", relatedPK1, BizObj.DocRelatedPKs[0]);
			AssertEquals("Second RelatedPK is something different from what we added", relatedPK2, BizObj.DocRelatedPKs[1]);

			var mainDocument = Parent.Documents.AddNew();
			mainDocument.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			mainDocument.SC_IsPublished = true;

			var related1 = MasterFactory.New<StorageMain>();
			related1.SM_Type = "DEC";
			related1.SM_ParentFK = relatedPK1;
			related1.SM_DB = 1;

			var document1 = related1.Documents.AddNew();
			document1.SC_DocType = Core.Constants.RefDocTypes.Label;
			document1.SC_IsPublished = true;
			var document2 = related1.Documents.AddNew();
			document2.SC_DocType = Core.Constants.RefDocTypes.Label;
			document2.SC_IsPublished = true;
			var document3 = related1.Documents.AddNew();
			document3.SC_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			document3.SC_IsPublished = false;

			var related2 = MasterFactory.New<StorageMain>();
			related2.SM_Type = "DEC";
			related2.SM_ParentFK = relatedPK2;
			related2.SM_DB = 1;

			var document4 = related2.Documents.AddNew();
			document4.SC_DocType = Core.Constants.RefDocTypes.Label;
			document4.SC_IsPublished = true;

			var document5 = related2.Documents.AddNew();
			document5.SC_DocType = Core.Constants.RefDocTypes.Label;
			document5.SC_IsPublished = true;

			BizObj.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Label);
			BizObj.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.AgentsInvoice);
			var requiredDoc3 = BizObj.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.ArrivalNotice);

			AssertEquals("Documents were not added to the second related BO", 2, related2.Documents.Count);

			MasterFactory.Save();

			AssertNotNull("RelatedBODocuments should not be null", BizObj.DocumentHelper.RelatedBODocuments);
			Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
			AssertEquals("Main document not found", true, BizObj.DocumentHelper.Documents.Contains(mainDocument.PK));
			AssertEquals("First document not found", true, BizObj.DocumentHelper.Documents.Contains(document1.PK));
			AssertEquals("Second document not found", true, BizObj.DocumentHelper.Documents.Contains(document2.PK));
			AssertEquals("Unpublished document not found", false, BizObj.DocumentHelper.Documents.Contains(document3.PK));
			AssertEquals("Fourth document not found", true, BizObj.DocumentHelper.Documents.Contains(document4.PK));
			AssertEquals("Fifth document not found", true, BizObj.DocumentHelper.Documents.Contains(document5.PK));
			AssertEquals("Required document not found", true, BizObj.DocumentHelper.Documents.Contains(requiredDoc3.PK));
			AssertEquals("Incorrect document count", 6, BizObj.DocumentHelper.Documents.Count);
		}

		public void TestRelatedBODocumentsWhenRelatedBOIsTransportBooking_OnlyIncludesPOD()
		{
			var booking = MasterFactory.NewWithValidTestData<DtbBooking>();
			BizObj.AddToDocRelatedPKs(booking.PK);

			AssertEquals("RelatedPKs were not added", 1, BizObj.DocRelatedPKs.Count);
			AssertEquals("Existing RelatedPK is something different from what we added", booking.PK, BizObj.DocRelatedPKs[0]);

			var related = MasterFactory.New<StorageMain>();
			related.SM_Type = Core.Constants.DocManagerCodes.DomesticTransportBooking;
			related.SM_ParentFK = booking.PK;
			related.SM_DB = 1;

			var document1 = related.Documents.AddNew();
			document1.SC_DocType = "MSC";
			document1.SC_IsPublished = true;

			var document2 = related.Documents.AddNew();
			document2.SC_DocType = "POD";
			document2.SC_IsPublished = true;

			var document3 = related.Documents.AddNew();
			document3.SC_DocType = "POD";
			document3.SC_IsPublished = false;

			MasterFactory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull("RelatedBODocuments should not be null", BizObj.DocumentHelper.RelatedBODocuments);
				Assert("RelatedBODocuments should be a DocumentViewCollection", BizObj.DocumentHelper.RelatedBODocuments is DocumentViewCollection);
				AssertEquals("Document count", 1, BizObj.DocumentHelper.RelatedBODocuments.Count);
				AssertEquals("MSC document should not be found on related TB", false, BizObj.DocumentHelper.RelatedBODocuments.Contains(document1.PK));
				AssertEquals("POD document should be found on related TB", true, BizObj.DocumentHelper.RelatedBODocuments.Contains(document2.PK));
				AssertEquals("Unpublished document should not be found", false, BizObj.DocumentHelper.RelatedBODocuments.Contains(document3.PK));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string smallTifPath;
		string SmallTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(smallTifPath))
				{
					smallTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.small.tif", "small.tif");
				}
				return smallTifPath;
			}
		}

		DocumentSupport Tester
		{
			get
			{
				if (tester == null)
				{
					var shipment = Factory.NewWithValidTestData<TrackingShipment>();

					var masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
					var storageMain = masterFactory.RetrieveExistingOrCreateStorageMainForPK(shipment.PK, ((IDocManagerSupport)shipment).DocManagerInfo.DocManagerCode);

					var storageDocs = storageMain.Documents.AddNew();
					storageDocs.SC_Date = ZDateTime.Now;
					storageDocs.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
					storageDocs.SC_FileName = "Test document.txt";
					storageDocs.SC_IsPublished = true;
					storageDocs.SC_ImageData = new byte[] { 1, 1, 1, 1, 1 };
					storageMain.Documents.Add(storageDocs);

					var storageFile = storageMain.Files.AddNew();
					storageFile.SC_Date = ZDateTime.Now;
					storageFile.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
					storageFile.SC_FileName = "Test file.doc";
					storageFile.SC_IsPublished = true;
					storageFile.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
					storageMain.Files.Add(storageFile);

					Factory.Save();
					masterFactory.Save();

					tester = new DocumentSupport(shipment);
				}

				return tester;
			}
		}
		DocumentSupport tester;

		DocumentFactory masterFactory;
		DocumentFactory MasterFactory => masterFactory ?? (masterFactory = new DocumentFactoryProvider().GetFactory(Factory));

		StorageMain parent;
		StorageMain Parent
		{
			get
			{
				if (parent == null)
				{
					parent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
					parent.SM_Type = Core.Constants.DocManagerCodes.Shipment;
					parent.SM_DB = 1;
					parent.SM_ParentFK = BizObj.DocParentPK;
				}
				return parent;
			}
		}

		IWebDocumentsSupportInternalTest.DummyBusinessObjectWithDocumentSupport BizObj
		{
			get
			{
				if (bizObj == null)
				{
					bizObj = GetNewBusinessObject();
				}
				return bizObj;
			}
		}
		IWebDocumentsSupportInternalTest.DummyBusinessObjectWithDocumentSupport bizObj;

		IWebDocumentsSupportInternalTest.DummyBusinessObjectWithDocumentSupport GetNewBusinessObject()
		{
			return Factory.New<IWebDocumentsSupportInternalTest.DummyBusinessObjectWithDocumentSupport>();
		}
	}
}
