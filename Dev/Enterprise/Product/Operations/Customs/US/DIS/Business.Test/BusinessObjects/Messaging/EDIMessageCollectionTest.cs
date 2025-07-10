using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(EDIMessageCollection))]
	sealed class EDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var jobDeclaration = (MasterFiles.Business.DIS.IUSDISHost)new TestHelper(Factory).GetJobDeclaration();
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocumentAddInfo = requiredDocument.AddInfos.AddNew();
			var message = Factory.New<EDIMessage>();
			message.EM_LinkedObject = requiredDocumentAddInfo;
			var requiredDocument2 = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocumentAddInfo2 = requiredDocument2.AddInfos.AddNew();
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = requiredDocumentAddInfo2;
			var hostWrapper = new DISHostWrapper(jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			disDocument.RequiredDocumentAddInfo = requiredDocumentAddInfo;
			var coll = new EDIMessageCollection(disDocument);
			coll.Load();
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(message));
			coll = new EDIMessageCollection(disDocument);
			coll.Load();
			disDocument.RequiredDocumentAddInfo = null;
			AssertEquals(0, disDocument.Messages.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper(declaration);
			var document = new DISDocument(hostWrapper);
			return new EDIMessageCollection(document);
		}
	}
}
