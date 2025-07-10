using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(PermittedStorageDocsCollection))]
	sealed class PermittedStorageDocsCollectionTest : BusinessObjectCollectionViewTestCase<PermittedStorageDocsCollection>
	{
		public void TestDocTypeUnset()
		{
			var helper = new TestHelper(Factory);
			var collection = CollectionWithDocumentOfType(GetOrCreateDocType("ALL", "THA"));

			var view = new PermittedStorageDocsCollection(collection, helper.TestSiteUser);
			AssertContainsDocument("Should be enabled by default", view, "ALL", "THA");
		}

		public void TestDocTypeDenied()
		{
			var helper = new TestHelper(Factory);
			var docType = GetOrCreateDocType("ALL", "THA");
			var collection = CollectionWithDocumentOfType(docType);

			var docRights = new DocumentWebSecurityRights(Factory);

			SetGranted(helper.TestSiteUser, docRights.GetSecurityRight(docType), false);

			var view = new PermittedStorageDocsCollection(collection, helper.TestSiteUser);
			AssertDoesntContainDocument("Document should not be available when we dont have permission", view, "ALL", "THA");
		}

		public void TestDocTypeGranted()
		{
			var helper = new TestHelper(Factory);
			var docType = GetOrCreateDocType("ALL", "THA");
			var collection = CollectionWithDocumentOfType(docType);

			var docRights = new DocumentWebSecurityRights(Factory);

			SetGranted(helper.TestSiteUser, docRights.GetSecurityRight(docType), true);

			var view = new PermittedStorageDocsCollection(collection, helper.TestSiteUser);
			AssertContainsDocument("Document should be there when we have permission", view, "ALL", "THA");
		}

		#region Helpers

		void SetGranted(TrackingSiteUser site, WebSecurityRight right, bool isGranted)
		{
			var contact = site.LoggedInUser;
			var orgRights = new OrgSecurityCollection(contact.ParentOrg);

			var orgRight = orgRights.AddNew();
			orgRight.OX_Granted = isGranted;
			orgRight.OX_SecurityItemName = right.Code;
		}

		StorageDocsCollection CollectionWithDocumentOfType(RefDocType docType)
		{
			var collection = new StorageDocsCollection(MasterFactory);
			var doc = collection.AddNew();
			doc.FillWithValidTestData();

			doc.SC_DocType = docType.RT_DocType;

			return collection;
		}

		RefDocType GetOrCreateDocType(string refType, string docType)
		{
			var result = Factory.LoadTop1<RefDocType>(new ZQuery()
				.AddToFilter(RefDocTypeSchema.RT_DocType, docType)
				.AddToFilter(RefDocTypeSchema.RT_ReferenceType, refType));

			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefDocType>();
				result.RT_DocType = docType;
				result.RT_ReferenceType = refType;
			}

			return result;
		}

		void AssertContainsDocument(string message, PermittedStorageDocsCollection collection, string refType, string docType)
		{
			AssertDocumentWithType(message, collection, refType, docType, true);
		}

		void AssertDoesntContainDocument(string message, PermittedStorageDocsCollection collection, string refType, string docType)
		{
			AssertDocumentWithType(message, collection, refType, docType, false);
		}

		void AssertDocumentWithType(string message, PermittedStorageDocsCollection collection, string refType, string docType, bool shouldHaveItem)
		{
			var hasItem = collection.Cast<StorageDocsBase>()
				.Select(doc => doc.DocType)
				.Any(type => type.RT_ReferenceType == refType && type.RT_DocType == docType);

			Assert(message, hasItem == shouldHaveItem);
		}

		#endregion

		#region Implementation

		protected override PermittedStorageDocsCollection GetCollectionToTest()
		{
			var main = MasterFactory.New<StorageMain>();
			var helper = new TestHelper(Factory);
			return new PermittedStorageDocsCollection(main.Documents, helper.TestSiteUser);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var docs = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			var refDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			docs.AddDocType(refDocType);
			docs.SC_DocType = refDocType.RT_DocType;

			return docs;
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (masterFactory == null)
				{
					masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return masterFactory;
			}
		}
		DocumentFactory masterFactory;

		#endregion
	}
}
