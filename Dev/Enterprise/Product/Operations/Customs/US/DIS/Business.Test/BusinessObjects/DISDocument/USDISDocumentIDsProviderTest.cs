using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class USDISDocumentIDsProviderTest : TestCaseWithFactory
	{
		public void TestGetDocumentIDList()
		{
			var disWrapper = new DISHostWrapper((IUSDISHost)JobDeclaration);
			var declaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			var eDoc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			var document1 = disWrapper.DISDocuments.AddNew();
			document1.IDSuffix = 1;
			document1.DocumentDescription = "BOB THE BUILDER";
			document1.EDocsDocumentPK = eDoc.UniqueKey;
			document1.DocumentLabel = "AMS01";
			document1.Status = StatusList.Codes.COS;
			var document2 = disWrapper.DISDocuments.AddNew();
			document2.IDSuffix = 2;
			document2.DocumentDescription = "WENDY THE DESTROYER";
			document2.EDocsDocumentPK = eDoc.UniqueKey;
			Factory.Save();
			var host = (IUSDISHost)new BusinessObjectFactory().Load<Integration.Customs.US.IJobDeclaration>(JobDeclaration.PK);
			var provider = ObjectFactory.Get<IUSDISDocumentIDsProvider>();
			var list = provider.GetDocumentIDList(host);
			AssertEquals(2, list.Count);
			AssertEquals("BOB THE BUILDER", list.GetDescriptionFromCode(document1.DocumentID));
			AssertEquals("WENDY THE DESTROYER", list.GetDescriptionFromCode(document2.DocumentID));
			Assert("Has been accept", provider.HasBeenAccepted(host, "AMS01"));
		}

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
