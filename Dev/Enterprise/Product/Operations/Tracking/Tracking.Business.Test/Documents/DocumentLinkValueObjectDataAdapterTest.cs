using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(DocumentLinkValueObjectDataAdapter))]
	[HttpContextEnabledTest]
	sealed class DocumentLinkValueObjectDataAdapterTest : ValueObjectDataAdapterTest<StorageDocsBase, Xsd.DocumentLink>
	{
		public void TestBusinessObjectType()
		{
			var dataAdapter = new DocumentLinkValueObjectDataAdapter();
			AssertEquals("Business Object Type should be StorageDocs", typeof(StorageDocsBase), dataAdapter.BusinessObjectType);
		}

		public void TestValueObjectType()
		{
			var dataAdapter = new DocumentLinkValueObjectDataAdapter();
			AssertEquals("Value Object Type should be Xsd.DocumentLink", typeof(Xsd.DocumentLink), dataAdapter.ValueObjectType);
		}

		public void TestSchema()
		{
			var dataAdapter = new DocumentLinkValueObjectDataAdapter();
			AssertEquals("Schema should be DocumentLinkSchema", XmlSchemaDefinitions.Instance.SingleDocumentLinkSchema, dataAdapter.Schema);
		}

		public void TestCollectionSchema()
		{
			var dataAdapter = new DocumentLinkValueObjectDataAdapter();
			AssertEquals("CollectionSchema should be DocumentLinkssSchema", XmlSchemaDefinitions.Instance.DocumentLinksSchema, dataAdapter.CollectionSchema);
		}

		public void TestExportCollection()
		{
			StorageMain.Factory.Save();
			AssertEquals("Should be no files", 0, StorageMain.eDocs.Count);

			var doc1 = PopulatedBusinessObject();

			var doc2 = StorageDocs.New_DEBUG(DocumentFactory.GetFactory(1));
			doc2.SC_DataType = "TIF";
			doc2.SC_DocType = "COR";
			doc2.SC_Desc = "Correspondence";
			doc2.SC_FileName = "Small";
			doc2.SC_Date = new ZDateTime(2005, 10, 21);
			doc2.SC_IsPublished = true;
			doc2.SC_SM = StorageMain.PK;

			StorageMain.RequireReload();
			AssertEquals("Should be two files", 2, StorageMain.eDocs.Count);
			AssertEquals("Should be two published files", 2, StorageMain.PublishedEDocsAndFiles.Count);

			var adapter = new DocumentLinkValueObjectDataAdapter();
			var links = adapter.ExportToXmlValueObjectCollection(StorageMain.PublishedEDocsAndFiles, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Two Links", 2, links.Count);

			var docViews = new DocumentViewCollection(DocumentFactory);
			var docView1 = docViews.AddNew();
			docView1.StorageDoc = doc1;

			var docView2 = docViews.AddNew();
			docView2.StorageDoc = doc2;

			docViews.AddNew();
			AssertEquals("Should be three DoViews", 3, docViews.Count);

			var viewLinks = adapter.ExportToXmlValueObjectCollection(docViews, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Two Links", 2, viewLinks.Count);
		}

		protected override ValueObjectDataAdapter<StorageDocsBase, Xsd.DocumentLink> GetNewBizObjXmlDataAdapter() => new DocumentLinkValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "DocumentLinks";

		protected override string ExpectedRootElementName => "DocumentLink";

		protected override StorageDocsBase NewBusinessObject() => StorageDocs.New_DEBUG(DocumentFactory);

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyDocumentLinkPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyDocumentLink.xml", "EmptyDocumentLink.xml");
			return new BusinessObjectAndExpectedOutputFileName(EmptyBusinessObject(), emptyDocumentLinkPath, ValidationKind.None, "Empty DocumentLink");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var fullDocumentLinkPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullDocumentLink.xml", "FullDocumentLink.xml");

			return new BusinessObjectAndExpectedOutputFileName(PopulatedBusinessObject(), fullDocumentLinkPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated DocumentLink");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample() => GetEmptyBizObjSample();

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("This test is not required because import from value is not supported, implement ImportFromValueObjectCore first", true);
		}

		protected override bool IsImportFromValueObjectSupported => false;

		protected override void SetUp()
		{
			base.SetUp();
			initialUserContext = EnvProxy.Instance.CurrentUserContext;
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			EnvProxy.Instance.SetUserContext(initialUserContext);
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		StorageDocsBase EmptyBusinessObject()
		{
			var storageDoc = StorageDocs.New_DEBUG(DocumentFactory);

			storageDoc.SC_DataType = "";
			storageDoc.SC_Date = new ZDateTime(2005, 10, 21);

			return storageDoc;
		}

		StorageDocsBase PopulatedBusinessObject()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/Tracking/");
			LoginWebUser();

			var storageDoc = StorageDocs.New_DEBUG(DocumentFactory.GetFactory(1));
			storageDoc.SC_DataType = "TIF";
			storageDoc.SC_DocType = "MBL";
			storageDoc.SC_Desc = "Masterbill";
			storageDoc.SC_FileName = "Small";
			storageDoc.SC_Date = new ZDateTime(2005, 10, 21);
			storageDoc.SC_ImageData = new byte[] { 1, 2, 3 };
			storageDoc.SC_IsSystemGenerated = true;
			storageDoc.SC_SaveVersions = true;
			storageDoc.SC_IsPublished = true;
			storageDoc.SC_Language = "EN-US";
			storageDoc.SC_SM = StorageMain.PK;

			return storageDoc;
		}

		IUserContext initialUserContext;
		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		DocumentFactory DocumentFactory
		{
			get
			{
				if (documentFactory == null)
				{
					documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return documentFactory;
			}
		}
		DocumentFactory documentFactory;

		StorageMain StorageMain
		{
			get
			{
				if (storageMain == null)
				{
					storageMain = DocumentFactory.New<StorageMain>();
					storageMain.SM_ParentFK = new ZGuid("01f6f60f-244c-4b4d-9b6c-f95bc7ae6d98");
					storageMain.SM_DB = 1;
				}
				return storageMain;
			}
		}
		StorageMain storageMain;

		void LoginWebUser()
		{
			var contact = Factory.Load<OrgContact>(new ZGuid("f960e868-fef4-4cab-a1f5-3ace433c04e9"));
			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);
			contact.OC_WebAccessEnabled = true;
			var password = "test";
			contact.SetHashedPassword(password);

			Factory.Save();

			var user = WebEnv.AppInstance.GetNewSiteUser();
			user.Login(contact.ParentOrg.OH_Code, contact.OC_Email, password);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertEquals("Logged in Contact Name", "TONY MORAN - SALES", WebEnv.CurrentUser.Name);
		}
	}
}
