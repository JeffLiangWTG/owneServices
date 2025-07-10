using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class DISDocumentCollectionBaseTest<T1, T2> : NonPersistentBusinessObjectCollectionTestCase<T1>
			where T1 : DISDocumentCollectionBase<T2>
			where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		public void TestPopulateCollection()
		{
			var jobDeclaration = GetDISHost();
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = ApplicationCode;
			addInfo.EX_AddInfo = xml1;
			addInfo.EX_Status = "AOS";

			var requiredDocument2 = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo2 = requiredDocument2.AddInfos.AddNew();
			addInfo2.EX_ApplicationCode = ApplicationCode;
			addInfo2.EX_AddInfo = xml2;
			addInfo2.EX_Status = "ARS";

			var addInfo3 = requiredDocument2.AddInfos.AddNew();

			var collection = GetCollectionToTest() as DISDocumentCollectionBase<T2>;
			AssertEquals(2, collection.Count);

			AssertEquals("ABC", collection[0].DocumentDescription);
			AssertEquals(requiredDocument.PK, collection[0].RequiredDocumentPK);
			AssertEquals(addInfo, collection[0].RequiredDocumentAddInfo);
			AssertEquals("AOS", collection[0].Status);

			AssertEquals("DEF", collection[1].DocumentDescription);
			AssertEquals(requiredDocument2.PK, collection[1].RequiredDocumentPK);
			AssertEquals(addInfo2, collection[1].RequiredDocumentAddInfo);
			AssertEquals("ARS", collection[1].Status);

			AssertEquals("HasChanges", false, collection[0].HasChanges);
			AssertEquals("HasChanges", false, collection[1].HasChanges);
		}

		public void TestSerialise()
		{
			var jobDeclaration = GetDISHost();
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = ApplicationCode;
			addInfo.EX_AddInfo = xml1;

			var requiredDocument2 = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo2 = requiredDocument2.AddInfos.AddNew();
			addInfo2.EX_ApplicationCode = ApplicationCode;
			addInfo2.EX_AddInfo = xml2;

			var collection = GetCollectionToTest() as DISDocumentCollectionBase<T2>;
			AssertEquals(2, collection.Count);

			collection[0].Status = "COS";
			collection[1].RequiredDocumentPK = requiredDocument.PK;
			AssertNull("Precondition", collection[1].RequiredDocumentAddInfo);

			collection.Serialize();

			AssertEquals("COS", addInfo.EX_Status);
			AssertEquals("addInfo2 should have been deleted", true, addInfo2.IsDeleted);
			AssertEquals("RequiredDocument has two addinfos now", 2, requiredDocument.AddInfos.Count);
			AssertNotNull("during serialisation, system should have set RequiredDocumentAddInfo to a valid object", collection[1].RequiredDocumentAddInfo);

			var addInfo3 = collection[1].RequiredDocumentAddInfo;
			collection[1].RequiredDocumentPK = requiredDocument2.PK;

			collection.Serialize();
			AssertEquals("addInfo2 should have been deleted", true, addInfo3.IsDeleted);
			AssertEquals(1, requiredDocument.AddInfos.Count);
			AssertEquals(1, requiredDocument2.AddInfos.Count);
		}

		protected abstract IDISHost GetDISHost();
		protected abstract string xml1 { get; }
		protected abstract string xml2 { get; }
		protected abstract string ApplicationCode { get; }
	}
}
