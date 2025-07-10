using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RequiredDocToBulkUpdate))]
	sealed class RequiredDocToBulkUpdateTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocument()
		{
			AssertNull("Document should be null initially", BulkUpdateDoc.Document);

			BulkUpdateDoc.SetDocument(DocToUpdate);
			AssertNotNull("Document should not be null", BulkUpdateDoc.Document);
			AssertEquals("Properties should have been copied over", DocToUpdate.PK, BulkUpdateDoc.DocumentPK);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_DocType, BulkUpdateDoc.DocumentType);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_DocNumber, BulkUpdateDoc.DocumentNumber);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_Calc_ParentUniqueConsignRef, BulkUpdateDoc.DocumentParentID);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_DateReceived, BulkUpdateDoc.DateReceived);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_SntToCustomsBroker, BulkUpdateDoc.DateSentToBroker);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_RcvFromCustomsBroker, BulkUpdateDoc.DateReceivedFromBroker);
			AssertEquals("Properties should have been copied over", DocToUpdate.EQ_ReturnToShipper, BulkUpdateDoc.DateReturnedToShipper);
			AssertEquals("Properties should have been copied over", DocToUpdate.DocumentOwner.OH_Code, BulkUpdateDoc.DocumentOwner);
		}

		public void TestUpdateFrom()
		{
			BulkUpdateDoc.SetDocument(DocToUpdate);

			foreach (string propertyName in RequiredDocToBulkUpdate.PropertiesToUpdate)
			{
				AssertEquals("All properties should be empty initially for the user", true, ((IZType)DummyParent[propertyName]).IsEmpty);
			}

			foreach (string propertyName in RequiredDocToBulkUpdate.PropertiesToUpdate)
			{
				object emptyValue = null;
				object docOriginalValue = null;
				object newBulkUpdateValue = null;
				if (DummyParent[propertyName] is ZDateTime)
				{
					emptyValue = ZDateTime.Invalid;
					docOriginalValue = ZDateTime.Now;
					newBulkUpdateValue = ZDateTime.Now.AddDays(1);
				}
				else if (DummyParent[propertyName] is ZDateTimeOffset)
				{
					emptyValue = ZDateTimeOffset.Invalid;
					docOriginalValue = ZDateTimeOffset.Now;
					newBulkUpdateValue = ZDateTimeOffset.Now.AddDays(1);
				}
				else if (DummyParent[propertyName] is ZString)
				{
					emptyValue = ZString.Empty;
					docOriginalValue = "blah";
					newBulkUpdateValue = "blah2";
				}
				else
				{
					Fail("Don't know how to handle this type " + DummyParent[propertyName].GetType().Name);
				}

				BulkUpdateDoc.Document[propertyName] = docOriginalValue;
				DummyParent[propertyName] = emptyValue;
				BulkUpdateDoc.UpdateFrom(DummyParent);
				AssertEquals("Should not change the value because it is empty", docOriginalValue, BulkUpdateDoc.Document[propertyName]);

				DummyParent[propertyName] = newBulkUpdateValue;
				BulkUpdateDoc.UpdateFrom(DummyParent);
				AssertEquals("Property " + propertyName + " should update as a value is specified", newBulkUpdateValue, BulkUpdateDoc.Document[propertyName]);
			}
		}

		public void TestDocumentParentIDMaxLength()
		{
			AssertEquals(255, BulkUpdateDoc.DocumentParentIDInfo.MaxLength);
		}

		#region Implementation

		DocumentTrackingBulkUpdateBusinessObject DummyParent;
		RequiredDocToBulkUpdate BulkUpdateDoc;
		JobRequiredDocument DocToUpdate;
		OrgHeader DocOwner;

		protected override void SetUp()
		{
			base.SetUp();

			DummyParent = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			BulkUpdateDoc = DummyParent.SelectedDocuments.AddNew();

			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)),
				TestBusinessObjectKind.MinimumRequiredToSave);

			DocToUpdate = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.HeXiaoDan);
			DocToUpdate.EQ_DocNumber = "1234";
			DocToUpdate.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-5);
			DocToUpdate.EQ_SntToCustomsBroker = ZDateTime.Today.AddDays(-4);
			DocToUpdate.EQ_RcvFromCustomsBroker = ZDateTime.Today.AddDays(-3);
			DocToUpdate.EQ_ReturnToShipper = ZDateTime.Today.AddDays(-2);

			DocOwner = Factory.New<OrgHeader>();
			DocOwner.OH_Code = "MILLY";
			DocToUpdate.EQ_OH_DocumentOwner = DocOwner.PK;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RequiredDocToBulkUpdate(Factory, DummyParent);
		}

		#endregion
	}
}
