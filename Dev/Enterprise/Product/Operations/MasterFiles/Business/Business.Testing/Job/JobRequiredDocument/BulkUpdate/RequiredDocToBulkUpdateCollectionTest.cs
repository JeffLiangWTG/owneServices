using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RequiredDocToBulkUpdateCollection))]
	sealed class RequiredDocToBulkUpdateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RequiredDocToBulkUpdateCollection>
	{
		public void TestAddDocToBulkUpdate()
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)),
				TestBusinessObjectKind.MinimumRequiredToSave);
			JobRequiredDocument doc = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.HeXiaoDan);
			doc.EQ_DocNumber = "1235";
			Factory.Save();

			RequiredDocToBulkUpdate bulkUpdateDoc = BulkUpdateCollection.AddDocToBulkUpdate(doc.PK, typeof(OrgHeader));
			AssertEquals(doc.EQ_DocNumber, bulkUpdateDoc.DocumentNumber);
			AssertEquals(doc.EQ_DocType, bulkUpdateDoc.DocumentType);
			AssertEquals(doc.EQ_Calc_ParentUniqueConsignRef, bulkUpdateDoc.DocumentParentID);
			AssertEquals(typeof(OrgHeader), bulkUpdateDoc.Document.ParentType);
		}

		#region Implementation

		DocumentTrackingBulkUpdateBusinessObject Parent;
		RequiredDocToBulkUpdateCollection BulkUpdateCollection;

		protected override void SetUp()
		{
			base.SetUp();
			Parent = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			BulkUpdateCollection = Parent.SelectedDocuments;
		}

		protected override RequiredDocToBulkUpdateCollection GetCollectionToTest()
		{
			return new RequiredDocToBulkUpdateCollection(Factory, Parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RequiredDocToBulkUpdate(Factory, Parent);
		}

		#endregion
	}
}
