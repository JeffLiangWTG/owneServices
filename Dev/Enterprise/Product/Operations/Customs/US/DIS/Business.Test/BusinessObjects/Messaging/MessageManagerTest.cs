using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class MessageManagerTest : Customs.Business.Testing.DISMessageManagerBaseTest<DISDocument>
	{
		public override void TestSendSubmission()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			new MessageManager(disDocument).SendSubmission();
			AssertEquals(1, disDocument.Messages.Count);
			AssertEquals(StatusList.Codes.AOS, disDocument.Status);
		}

		public override void TestSendWithdrawal()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			new MessageManager(disDocument).SendWithdrawl();
			AssertEquals(1, disDocument.Messages.Count);
			AssertEquals(StatusList.Codes.AWS, disDocument.Status);
		}

		[TestDate(2015, 5, 27, 11, 24, 0)]
		public void TestPopulateSubmitDate()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.RequiredDocumentPK = requiredDocument.PK;
			new MessageManager(disDocument).SendSubmission();
			AssertEquals("Populate SubmitDateUTC when it is empty & when sending the first message", new ZDateTime(2015, 5, 27, 11, 24, 0), disDocument.SubmitDateUTC);
			disDocument.SubmitDateUTC = new ZDateTime(2015, 5, 20);
			new MessageManager(disDocument).SendWithdrawl();
			AssertEquals("Populate SubmitDateUTC when it is empty & when sending the first message", new ZDateTime(2015, 5, 20), disDocument.SubmitDateUTC);
		}

		protected override DISMessageManagerBase GetMessageManager(IDISDocumentBase disDocument) => new MessageManager(disDocument as DISDocument);

		DISHostWrapper hostWrapper;
		protected override DISHostWrapperBase<DISDocument> HostWrapper => hostWrapper ?? (hostWrapper = new DISHostWrapper((MasterFiles.Business.DIS.IUSDISHost)JobDeclaration));

		BusinessObject jobDeclaration;
		protected override BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
