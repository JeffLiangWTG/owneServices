using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(MessageSendingActionCollection))]
	sealed class MessageSendingActionCollectionTest : MessageSendingActionCollectionBaseTest<MessageSendingActionCollection, MessageSendingAction, DISDocument>
	{
		public override void TestSendMessages()
		{
			var declaration = (MasterFiles.Business.DIS.IUSDISHost)JobDeclaration;
			var requiredDocument1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var disDocument1 = HostWrapper.DISDocuments.AddNew();
			disDocument1.RequiredDocumentPK = requiredDocument1.PK;
			var disDocument2 = HostWrapper.DISDocuments.AddNew();
			disDocument2.Status = StatusList.Codes.COS;
			disDocument2.RequiredDocumentPK = requiredDocument2.PK;
			var disDocument3 = HostWrapper.DISDocuments.AddNew();
			var coll = new MessageSendingActionCollection(HostWrapper as DISHostWrapper);
			AssertEquals("PreCondition", 3, coll.Count);
			Assert("Status is empty therefore 'Send' is defaulted", coll[0].Send);
			Assert("Status is NOT empty therefore 'Send' is not defaulted", !coll[1].Send);
			coll[1].SendWithdrawal = true;
			coll[2].Send = false;
			coll.SendMessages();
			AssertEquals(1, disDocument1.Messages.Count);
			AssertEquals(1, disDocument2.Messages.Count);
			AssertEquals(0, disDocument3.Messages.Count);
		}

		protected override MessageSendingActionCollection GetCollectionToTest() => new MessageSendingActionCollection(HostWrapper as DISHostWrapper);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new MessageSendingAction(HostWrapper.DISDocuments.AddNew());

		DISHostWrapper hostWrapper;
		protected override DISHostWrapperBase<DISDocument> HostWrapper => hostWrapper ?? (hostWrapper = new DISHostWrapper((MasterFiles.Business.DIS.IUSDISHost)JobDeclaration));

		BusinessObject jobDeclaration;
		protected override BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
