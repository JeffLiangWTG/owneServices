using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISDocumentCollection))]
	sealed class DISDocumentCollectionTest : DISDocumentCollectionBaseTest<DISDocumentCollection, DISDocument>
	{
		public void TestLoadingMessages()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_AddInfo = xml1;
			var message = Factory.New<EDIMessage>();
			message.EM_LinkedObject = addInfo;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<Integration.Customs.US.IJobDeclaration>(jobDeclaration.PK);
			var hostWrapper = new DISHostWrapper((IUSDISHost)declarationLoaded);
			AssertEquals(1, hostWrapper.DISDocuments.Count);
			AssertEquals("messages loaded correctly", 1, hostWrapper.DISDocuments[0].Messages.Count);
		}

		public void TestDeleteUnmappedRequiredAddInfo()
		{
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			var requiredDocument2 = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var addInfo2 = requiredDocument2.AddInfos.AddNew();
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			var collection = GetCollectionToTest();
			var document1 = collection.AddNew();
			var document2 = collection.AddNew();
			document1.RequiredDocumentPK = requiredDocument2.PK;
			collection.Remove(document2);
			collection.Serialize();
			AssertEquals("no AddInfos;existing ones are deleted", 0, requiredDocument.AddInfos.Count);
			AssertEquals("only one AddInfos", 1, requiredDocument2.AddInfos.Count);
		}

		protected override IDISHost GetDISHost() => JobDeclaration as IDISHost;

		protected override string xml1 => @"<DISDocument xmlns=""http://www.cargowise.com/Schemas/DISDocument""><DocumentDescription>ABC</DocumentDescription></DISDocument>";

		protected override string xml2 => @"<DISDocument xmlns=""http://www.cargowise.com/Schemas/DISDocument""><DocumentDescription>DEF</DocumentDescription></DISDocument>";

		protected override string ApplicationCode => Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DISDocument(HostWrapper);

		protected override DISDocumentCollection GetCollectionToTest() => new DISDocumentCollection(HostWrapper);

		DISHostWrapper hostWrapper;
		DISHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DISHostWrapper((IUSDISHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
